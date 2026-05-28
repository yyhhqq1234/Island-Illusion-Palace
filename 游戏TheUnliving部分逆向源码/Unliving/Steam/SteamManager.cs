using System;
using System.Collections.Generic;
using Common.ServiceRegistry;
using Game.Core;
using Game.Localization;
using Steamworks;
using UnityEngine;
using Unliving.Plot.Milestones;
using Unliving.Statistics;

namespace Unliving.Steam
{
	// Token: 0x02000052 RID: 82
	[Service(typeof(SteamManager), new Type[]
	{
		typeof(IStoreStatisticsProvider),
		typeof(IDefaultLanguageProvider)
	})]
	public class SteamManager : GlobalSceneManagerBase, IStoreStatisticsProvider, IDefaultLanguageProvider
	{
		// Token: 0x06000294 RID: 660 RVA: 0x0000A1A4 File Offset: 0x000083A4
		public override void Initialize(IGame currentGame)
		{
			if (currentGame.Services.Get<SteamManager>() != null)
			{
				UnityEngine.Object.DestroyImmediate(base.gameObject);
				return;
			}
			currentGame.BindDataDirectly(ref this.languageData);
			this.userAchievementStored = Callback<UserAchievementStored_t>.Create(new Callback<UserAchievementStored_t>.DispatchDelegate(this.OnAchievementStored));
			base.Initialize(currentGame);
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (!Packsize.Test())
			{
				Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
			}
			if (!DllCheck.Test())
			{
				Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
			}
			if (this.enableSteamGuard)
			{
				try
				{
					if (SteamAPI.RestartAppIfNecessary(new AppId_t(986040U)))
					{
						Application.Quit();
						return;
					}
				}
				catch (DllNotFoundException ex)
				{
					string str = "[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n";
					DllNotFoundException ex2 = ex;
					Debug.LogError(str + ((ex2 != null) ? ex2.ToString() : null), this);
					Application.Quit();
					return;
				}
			}
			this.isInitialized = SteamAPI.Init();
			if (!this.isInitialized)
			{
				Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);
				return;
			}
			this.gameID = new CGameID(SteamUtils.GetAppID());
			if (this.clearDataOnStart)
			{
				this.ClearStatisticsData();
				this.ClearAchievementsData();
			}
			else
			{
				this.needToRequestStats = true;
			}
			if (base.CurrentGame.Services.TryGet<IPlotMilestoneManager>(out this.plotMilestoneManager))
			{
				this.plotMilestoneManager.MilestoneReached += this.OnMilestoneReached;
			}
		}

		// Token: 0x06000295 RID: 661 RVA: 0x0000A300 File Offset: 0x00008500
		private void OnMilestoneReached(PlotMilestoneNode node)
		{
			if (node.steamAchievement)
			{
				this.SetAchievementReached(node.milestoneID);
			}
		}

		// Token: 0x06000296 RID: 662 RVA: 0x0000A318 File Offset: 0x00008518
		public bool TryGetDefaultLanguage(out SystemLanguage language)
		{
			language = SystemLanguage.Unknown;
			if (!this.isInitialized)
			{
				return false;
			}
			string currentGameLanguage = SteamApps.GetCurrentGameLanguage();
			SteamManager.SteamLanguageData steamLanguageData;
			if (this.languageData.TryGetLanguageData(currentGameLanguage, out steamLanguageData))
			{
				language = steamLanguageData.systemLanguage;
				return true;
			}
			return false;
		}

		// Token: 0x06000297 RID: 663 RVA: 0x0000A354 File Offset: 0x00008554
		public bool SetStatValue(string statID, int value)
		{
			if (!this.isInitialized)
			{
				return false;
			}
			int num;
			if (this.storeStatsBuffer.TryGetValue(statID, out num))
			{
				if (num < value)
				{
					this.storeStatsBuffer[statID] = value;
				}
			}
			else
			{
				this.storeStatsBuffer.Add(statID, value);
			}
			this.needToStoreStats = true;
			return true;
		}

		// Token: 0x06000298 RID: 664 RVA: 0x0000A3A4 File Offset: 0x000085A4
		public void ClearAchievementsData()
		{
			uint numAchievements = SteamUserStats.GetNumAchievements();
			for (uint num = 0U; num < numAchievements; num += 1U)
			{
				SteamUserStats.ClearAchievement(SteamUserStats.GetAchievementName(num));
			}
		}

		// Token: 0x06000299 RID: 665 RVA: 0x0000A3CF File Offset: 0x000085CF
		public void ClearStatisticsData()
		{
			SteamUserStats.ResetAllStats(true);
			SteamUserStats.RequestCurrentStats();
		}

		// Token: 0x0600029A RID: 666 RVA: 0x0000A3E0 File Offset: 0x000085E0
		public void SetAchievementReached(string achievementName)
		{
			SteamUserStats.SetAchievement(achievementName);
			Debug.Log(achievementName + " " + SteamUserStats.StoreStats().ToString());
		}

		// Token: 0x0600029B RID: 667 RVA: 0x0000A414 File Offset: 0x00008614
		public int GetStatValue(string statID)
		{
			int result;
			if (this.isInitialized && SteamUserStats.GetStat(statID, out result))
			{
				return result;
			}
			return 0;
		}

		// Token: 0x0600029C RID: 668 RVA: 0x0000A438 File Offset: 0x00008638
		private void OnAchievementStored(UserAchievementStored_t result)
		{
			if ((ulong)this.gameID != result.m_nGameID)
			{
				return;
			}
			if (result.m_nMaxProgress == 0U)
			{
				Debug.Log("Achievement '" + result.m_rgchAchievementName + "' unlocked!");
				return;
			}
			Debug.Log(string.Concat(new string[]
			{
				"Achievement '",
				result.m_rgchAchievementName,
				"' progress callback, (",
				result.m_nCurProgress.ToString(),
				",",
				result.m_nMaxProgress.ToString(),
				")"
			}));
		}

		// Token: 0x0600029D RID: 669 RVA: 0x0000A4D8 File Offset: 0x000086D8
		private void Update()
		{
			if (!this.isInitialized)
			{
				return;
			}
			if (this.needToStoreStats)
			{
				foreach (KeyValuePair<string, int> keyValuePair in this.storeStatsBuffer)
				{
					SteamUserStats.SetStat(keyValuePair.Key, keyValuePair.Value);
				}
				this.needToStoreStats = !SteamUserStats.StoreStats();
			}
			if (this.needToRequestStats)
			{
				this.needToRequestStats = !SteamUserStats.RequestCurrentStats();
			}
			SteamAPI.RunCallbacks();
		}

		// Token: 0x0600029E RID: 670 RVA: 0x0000A574 File Offset: 0x00008774
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.isInitialized)
			{
				SteamAPI.Shutdown();
			}
			if (this.plotMilestoneManager != null)
			{
				this.plotMilestoneManager.MilestoneReached -= this.OnMilestoneReached;
			}
		}

		// Token: 0x04000182 RID: 386
		private const uint STEAM_GAME_ID = 986040U;

		// Token: 0x04000183 RID: 387
		public bool enableSteamGuard;

		// Token: 0x04000184 RID: 388
		[SerializeField]
		private bool clearDataOnStart;

		// Token: 0x04000185 RID: 389
		private bool needToRequestStats;

		// Token: 0x04000186 RID: 390
		private bool needToStoreStats;

		// Token: 0x04000187 RID: 391
		private bool isInitialized;

		// Token: 0x04000188 RID: 392
		private CGameID gameID;

		// Token: 0x04000189 RID: 393
		private Callback<UserAchievementStored_t> userAchievementStored;

		// Token: 0x0400018A RID: 394
		private readonly Dictionary<string, int> storeStatsBuffer = new Dictionary<string, int>();

		// Token: 0x0400018B RID: 395
		private SteamManager.LanguagesData languageData;

		// Token: 0x0400018C RID: 396
		private IPlotMilestoneManager plotMilestoneManager;

		// Token: 0x0200041C RID: 1052
		[Serializable]
		public struct SteamLanguageData
		{
			// Token: 0x040015DB RID: 5595
			public string steamLanguage;

			// Token: 0x040015DC RID: 5596
			public SystemLanguage systemLanguage;
		}

		// Token: 0x0200041D RID: 1053
		[Serializable]
		public class LanguagesData
		{
			// Token: 0x06002280 RID: 8832 RVA: 0x0006B1CC File Offset: 0x000693CC
			public bool TryGetLanguageData(string steamLanguage, out SteamManager.SteamLanguageData languageData)
			{
				languageData = new SteamManager.SteamLanguageData
				{
					steamLanguage = null,
					systemLanguage = SystemLanguage.Unknown
				};
				for (int i = 0; i < this.languagesData.Length; i++)
				{
					SteamManager.SteamLanguageData steamLanguageData = this.languagesData[i];
					if (string.Equals(steamLanguage, steamLanguageData.steamLanguage))
					{
						languageData = steamLanguageData;
						return true;
					}
				}
				return false;
			}

			// Token: 0x040015DD RID: 5597
			public SteamManager.SteamLanguageData[] languagesData;
		}
	}
}
