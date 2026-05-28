using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CollectionsExtensions;
using Common.RestorableState;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.PlayerProfileManagement;
using Unliving.Statistics;

namespace Unliving.Achievements
{
	// Token: 0x02000134 RID: 308
	[CreateAssetMenu(fileName = "PlayerAchievementsManager", menuName = "Game/Player/Achievements Manager")]
	public sealed class PlayerAchievementsManager : GlobalManagerBase
	{
		// Token: 0x14000046 RID: 70
		// (add) Token: 0x060007CB RID: 1995 RVA: 0x00019A64 File Offset: 0x00017C64
		// (remove) Token: 0x060007CC RID: 1996 RVA: 0x00019A9C File Offset: 0x00017C9C
		public event Action<IAchievement> AchievementUnlocked;

		// Token: 0x060007CD RID: 1997 RVA: 0x00019AD4 File Offset: 0x00017CD4
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
		}

		// Token: 0x060007CE RID: 1998 RVA: 0x00019B28 File Offset: 0x00017D28
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			IAchievement[] abilityUseCountAchievements = this.AbilityUseCountAchievements;
			this.InitializeAchievements(abilityUseCountAchievements);
			this.profileManager.LoadPlayerAchievements(this);
		}

		// Token: 0x060007CF RID: 1999 RVA: 0x00019B4F File Offset: 0x00017D4F
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			this.statisticsManager = base.CurrentGame.Services.Get<PlayerStatisticsManager>();
			PlayerProfileManager playerProfileManager = base.CurrentGame.Services.Get<PlayerProfileManager>();
			if (playerProfileManager == null)
			{
				return;
			}
			playerProfileManager.UpdatePlayerAchievements(this);
		}

		// Token: 0x060007D0 RID: 2000 RVA: 0x00019B84 File Offset: 0x00017D84
		private void InitializeAchievements(IAchievement[] achievements)
		{
			foreach (IAchievement achievement in achievements)
			{
				if (this.achievementsDict.ContainsKey(achievement.AchievementName))
				{
					Debug.LogWarning("Duplicate achievement with name: <color=red>" + achievement.AchievementName + "</color>");
				}
				else
				{
					this.achievementsDict.Add(achievement.AchievementName, achievement);
				}
			}
		}

		// Token: 0x060007D1 RID: 2001 RVA: 0x00019BE6 File Offset: 0x00017DE6
		public void UnlockAchievement(string name)
		{
			if (this.achievementsDict == null || !this.achievementsDict.ContainsKey(name))
			{
				return;
			}
			this.achievementsDict[name].UnlockAchievment();
		}

		// Token: 0x060007D2 RID: 2002 RVA: 0x00019C10 File Offset: 0x00017E10
		public string[] GetUnlockedAchievementsArray()
		{
			return (from a in this.achievementsDict.Values
			where a.IsAchieved
			select a.AchievementName).ToArray<string>();
		}

		// Token: 0x060007D3 RID: 2003 RVA: 0x00019C75 File Offset: 0x00017E75
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
		}

		// Token: 0x0400047A RID: 1146
		public AbilityUseCountAchievement[] AbilityUseCountAchievements;

		// Token: 0x0400047B RID: 1147
		private Dictionary<string, IAchievement> achievementsDict = new Dictionary<string, IAchievement>();

		// Token: 0x0400047C RID: 1148
		private PlayerStatisticsManager statisticsManager;

		// Token: 0x0400047D RID: 1149
		private PlayerProfileManager profileManager;

		// Token: 0x02000441 RID: 1089
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PlayerAchievementsManager>, ICloneable<PlayerAchievementsManager.RestorableState>
		{
			// Token: 0x06002338 RID: 9016 RVA: 0x0006CEEB File Offset: 0x0006B0EB
			public RestorableState() : base(null)
			{
			}

			// Token: 0x06002339 RID: 9017 RVA: 0x0006CEF4 File Offset: 0x0006B0F4
			public RestorableState(PlayerAchievementsManager achievementsManager) : base(achievementsManager)
			{
			}

			// Token: 0x0600233A RID: 9018 RVA: 0x0006CEFD File Offset: 0x0006B0FD
			public PlayerAchievementsManager.RestorableState Clone()
			{
				return new PlayerAchievementsManager.RestorableState
				{
					AchievementsData = this.AchievementsData.CloneArray<string>()
				};
			}

			// Token: 0x0600233B RID: 9019 RVA: 0x0006CF15 File Offset: 0x0006B115
			public override void Store(PlayerAchievementsManager achievementsManager)
			{
				this.AchievementsData = achievementsManager.GetUnlockedAchievementsArray();
			}

			// Token: 0x0600233C RID: 9020 RVA: 0x0006CF24 File Offset: 0x0006B124
			public override void Restore(PlayerAchievementsManager achievementsManager, object args = null)
			{
				foreach (string name in this.AchievementsData)
				{
					achievementsManager.UnlockAchievement(name);
				}
			}

			// Token: 0x04001694 RID: 5780
			public string[] AchievementsData;
		}
	}
}
