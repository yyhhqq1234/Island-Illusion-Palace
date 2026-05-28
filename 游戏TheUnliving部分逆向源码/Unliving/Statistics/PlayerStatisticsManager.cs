using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Editor;
using Common.RestorableState;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Abilities;
using Unliving.Currencies;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.ActivationModifiers;
using Unliving.PlayerProfileManagement;

namespace Unliving.Statistics
{
	// Token: 0x02000127 RID: 295
	[CreateAssetMenu(fileName = "PlayerStatisticsManager", menuName = "Game/Player/Statistics Manager")]
	public sealed class PlayerStatisticsManager : GlobalManagerBase
	{
		// Token: 0x1700012E RID: 302
		// (get) Token: 0x06000765 RID: 1893 RVA: 0x00017892 File Offset: 0x00015A92
		public PlayerStatisticsManager.Data PlayerStatsData
		{
			get
			{
				return this.playerStatsData;
			}
		}

		// Token: 0x1700012F RID: 303
		// (get) Token: 0x06000766 RID: 1894 RVA: 0x0001789A File Offset: 0x00015A9A
		public PlayerStatisticsManager.Data GameSessionStatsData
		{
			get
			{
				return this.gameSessionStatsData;
			}
		}

		// Token: 0x06000767 RID: 1895 RVA: 0x000178A4 File Offset: 0x00015AA4
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.defaultStatisticsData = this.profileManager.DefaultPlayerProfile.statisticsState.statisticsData;
				this.playerStatsData = new PlayerStatisticsManager.Data(this.defaultStatisticsData);
				this.playerStatsData.name = "PlayerStatsData";
				this.InitStatsData(this.playerStatsData);
				this.gameSessionStatsData = new PlayerStatisticsManager.Data(this.defaultStatisticsData);
				this.gameSessionStatsData.name = "GameSessionStatsData";
				this.InitStatsData(this.gameSessionStatsData);
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
			if (currentGame.Services.TryGet<GameManager>(out this.gameManager))
			{
				this.gameManager.GameStarted += this.OnGameStarted;
			}
		}

		// Token: 0x06000768 RID: 1896 RVA: 0x00017995 File Offset: 0x00015B95
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			this.profileManager.LoadPlayerStatistics(this);
		}

		// Token: 0x06000769 RID: 1897 RVA: 0x000179A4 File Offset: 0x00015BA4
		private void InitStatsData(PlayerStatisticsManager.Data data)
		{
			for (int i = 0; i < data.statistics.Length; i++)
			{
				data.statistics[i].SetGameData(base.CurrentGame, data);
			}
		}

		// Token: 0x0600076A RID: 1898 RVA: 0x000179D8 File Offset: 0x00015BD8
		private void ResetStatsData(PlayerStatisticsManager.Data data)
		{
			if (data == null)
			{
				return;
			}
			for (int i = 0; i < data.statistics.Length; i++)
			{
				data.statistics[i].Destroy();
			}
		}

		// Token: 0x0600076B RID: 1899 RVA: 0x00017A0C File Offset: 0x00015C0C
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			base.CurrentGame.Services.TryGet<IStoreStatisticsProvider>(out this.storeStatisticsProvider);
			for (int i = 0; i < this.playerStatsData.statistics.Length; i++)
			{
				this.playerStatsData.statistics[i].OnSceneLoaded(this.storeStatisticsProvider);
			}
			for (int j = 0; j < this.gameSessionStatsData.statistics.Length; j++)
			{
				this.gameSessionStatsData.statistics[j].OnSceneLoaded(null);
			}
		}

		// Token: 0x0600076C RID: 1900 RVA: 0x00017A8C File Offset: 0x00015C8C
		private void OnGameStarted(IGameManager gameManager)
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				playerProfileManager.UpdatePlayerStatistics(this);
			}
			this.ResetStatsData(this.gameSessionStatsData);
			this.gameSessionStatsData = new PlayerStatisticsManager.Data(this.defaultStatisticsData);
			this.InitStatsData(this.gameSessionStatsData);
			this.gameSessionStatsData.name = "GameSessionStatsData";
		}

		// Token: 0x0600076D RID: 1901 RVA: 0x00017AF0 File Offset: 0x00015CF0
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.gameManager.IsNull())
			{
				this.gameManager.GameStarted -= this.OnGameStarted;
			}
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
		}

		// Token: 0x04000453 RID: 1107
		private PlayerStatisticsManager.Data playerStatsData;

		// Token: 0x04000454 RID: 1108
		private PlayerStatisticsManager.Data gameSessionStatsData;

		// Token: 0x04000455 RID: 1109
		private GameManager gameManager;

		// Token: 0x04000456 RID: 1110
		private PlayerProfileManager profileManager;

		// Token: 0x04000457 RID: 1111
		private IStatisticsSerializationData[] defaultStatisticsData;

		// Token: 0x04000458 RID: 1112
		private IStoreStatisticsProvider storeStatisticsProvider;

		// Token: 0x02000437 RID: 1079
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PlayerStatisticsManager>, ICloneable<PlayerStatisticsManager.RestorableState>
		{
			// Token: 0x060022CB RID: 8907 RVA: 0x0006C5E2 File Offset: 0x0006A7E2
			public RestorableState() : base(null)
			{
			}

			// Token: 0x060022CC RID: 8908 RVA: 0x0006C5F7 File Offset: 0x0006A7F7
			public RestorableState(PlayerStatisticsManager statisticsManager) : base(statisticsManager)
			{
			}

			// Token: 0x060022CD RID: 8909 RVA: 0x0006C60C File Offset: 0x0006A80C
			public PlayerStatisticsManager.RestorableState Clone()
			{
				PlayerStatisticsManager.RestorableState restorableState = new PlayerStatisticsManager.RestorableState();
				restorableState.statisticsData = (from s in this.statisticsData
				select s.Clone()).ToArray<IStatisticsSerializationData>();
				return restorableState;
			}

			// Token: 0x060022CE RID: 8910 RVA: 0x0006C648 File Offset: 0x0006A848
			public override void Store(PlayerStatisticsManager statisticsManager)
			{
				this.statisticsData = statisticsManager.PlayerStatsData.GetSaveData();
			}

			// Token: 0x060022CF RID: 8911 RVA: 0x0006C65B File Offset: 0x0006A85B
			public override void Restore(PlayerStatisticsManager statisticsManager, object args = null)
			{
				statisticsManager.ResetStatsData(statisticsManager.PlayerStatsData);
				statisticsManager.PlayerStatsData.SetSaveData(this.statisticsData);
				statisticsManager.InitStatsData(statisticsManager.PlayerStatsData);
			}

			// Token: 0x04001648 RID: 5704
			[SerializeReference]
			[ManagedObjectField(typeof(StatisticsSerializationDataBase))]
			public IStatisticsSerializationData[] statisticsData = new IStatisticsSerializationData[0];
		}

		// Token: 0x02000438 RID: 1080
		[Serializable]
		public sealed class Data : ICloneable<PlayerStatisticsManager.Data>
		{
			// Token: 0x170006FD RID: 1789
			// (get) Token: 0x060022D0 RID: 8912 RVA: 0x0006C686 File Offset: 0x0006A886
			// (set) Token: 0x060022D1 RID: 8913 RVA: 0x0006C68E File Offset: 0x0006A88E
			public Func<AbilityID, int> GetAbilityUsageCount { get; internal set; }

			// Token: 0x170006FE RID: 1790
			// (get) Token: 0x060022D2 RID: 8914 RVA: 0x0006C697 File Offset: 0x0006A897
			// (set) Token: 0x060022D3 RID: 8915 RVA: 0x0006C69F File Offset: 0x0006A89F
			public Func<AbilityID, int> GetAbilityKillsCount { get; internal set; }

			// Token: 0x170006FF RID: 1791
			// (get) Token: 0x060022D4 RID: 8916 RVA: 0x0006C6A8 File Offset: 0x0006A8A8
			// (set) Token: 0x060022D5 RID: 8917 RVA: 0x0006C6B0 File Offset: 0x0006A8B0
			public Func<float> GetPlayingTime { get; internal set; }

			// Token: 0x17000700 RID: 1792
			// (get) Token: 0x060022D6 RID: 8918 RVA: 0x0006C6B9 File Offset: 0x0006A8B9
			// (set) Token: 0x060022D7 RID: 8919 RVA: 0x0006C6C1 File Offset: 0x0006A8C1
			public Func<IList<LocationChunk.TypeID>, int> GetChunksVisitedCount { get; internal set; }

			// Token: 0x17000701 RID: 1793
			// (get) Token: 0x060022D8 RID: 8920 RVA: 0x0006C6CA File Offset: 0x0006A8CA
			// (set) Token: 0x060022D9 RID: 8921 RVA: 0x0006C6D2 File Offset: 0x0006A8D2
			public Func<int> GetChunksGeneratedCount { get; internal set; }

			// Token: 0x17000702 RID: 1794
			// (get) Token: 0x060022DA RID: 8922 RVA: 0x0006C6DB File Offset: 0x0006A8DB
			// (set) Token: 0x060022DB RID: 8923 RVA: 0x0006C6E3 File Offset: 0x0006A8E3
			public Func<IList<LocationChunk.TypeID>, int> GetZeroSacrificedMobsChunksCount { get; internal set; }

			// Token: 0x17000703 RID: 1795
			// (get) Token: 0x060022DC RID: 8924 RVA: 0x0006C6EC File Offset: 0x0006A8EC
			// (set) Token: 0x060022DD RID: 8925 RVA: 0x0006C6F4 File Offset: 0x0006A8F4
			public Func<int> GetGamerunsCount { get; internal set; }

			// Token: 0x17000704 RID: 1796
			// (get) Token: 0x060022DE RID: 8926 RVA: 0x0006C6FD File Offset: 0x0006A8FD
			// (set) Token: 0x060022DF RID: 8927 RVA: 0x0006C705 File Offset: 0x0006A905
			public Func<GameLocation.TypeID, int, int> GetLocationRunsCount { get; internal set; }

			// Token: 0x17000705 RID: 1797
			// (get) Token: 0x060022E0 RID: 8928 RVA: 0x0006C70E File Offset: 0x0006A90E
			// (set) Token: 0x060022E1 RID: 8929 RVA: 0x0006C716 File Offset: 0x0006A916
			public Func<CurrencyID, float> GetCurrencyGainedAmount { get; internal set; }

			// Token: 0x17000706 RID: 1798
			// (get) Token: 0x060022E2 RID: 8930 RVA: 0x0006C71F File Offset: 0x0006A91F
			// (set) Token: 0x060022E3 RID: 8931 RVA: 0x0006C727 File Offset: 0x0006A927
			public Func<CurrencyID, float> GetCurrencySpentAmount { get; internal set; }

			// Token: 0x17000707 RID: 1799
			// (get) Token: 0x060022E4 RID: 8932 RVA: 0x0006C730 File Offset: 0x0006A930
			// (set) Token: 0x060022E5 RID: 8933 RVA: 0x0006C738 File Offset: 0x0006A938
			public Func<int> GetVitalEnergySpentAmount { get; internal set; }

			// Token: 0x17000708 RID: 1800
			// (get) Token: 0x060022E6 RID: 8934 RVA: 0x0006C741 File Offset: 0x0006A941
			// (set) Token: 0x060022E7 RID: 8935 RVA: 0x0006C749 File Offset: 0x0006A949
			public Func<int> GetHPContainersLostCount { get; internal set; }

			// Token: 0x17000709 RID: 1801
			// (get) Token: 0x060022E8 RID: 8936 RVA: 0x0006C752 File Offset: 0x0006A952
			// (set) Token: 0x060022E9 RID: 8937 RVA: 0x0006C75A File Offset: 0x0006A95A
			public Func<int> GetPlayerKilledCount { get; internal set; }

			// Token: 0x1700070A RID: 1802
			// (get) Token: 0x060022EA RID: 8938 RVA: 0x0006C763 File Offset: 0x0006A963
			// (set) Token: 0x060022EB RID: 8939 RVA: 0x0006C76B File Offset: 0x0006A96B
			public Func<int> GetAllRevivedMobsCount { get; internal set; }

			// Token: 0x1700070B RID: 1803
			// (get) Token: 0x060022EC RID: 8940 RVA: 0x0006C774 File Offset: 0x0006A974
			// (set) Token: 0x060022ED RID: 8941 RVA: 0x0006C77C File Offset: 0x0006A97C
			public Func<GameLocation.TypeID, int, int> GetLocationWinsCount { get; internal set; }

			// Token: 0x1700070C RID: 1804
			// (get) Token: 0x060022EE RID: 8942 RVA: 0x0006C785 File Offset: 0x0006A985
			// (set) Token: 0x060022EF RID: 8943 RVA: 0x0006C78D File Offset: 0x0006A98D
			public Func<GameLocation.TypeID, bool> IsLocationExlored { get; internal set; }

			// Token: 0x1700070D RID: 1805
			// (get) Token: 0x060022F0 RID: 8944 RVA: 0x0006C796 File Offset: 0x0006A996
			// (set) Token: 0x060022F1 RID: 8945 RVA: 0x0006C79E File Offset: 0x0006A99E
			public Func<MobBehaviour.ID, int> GetMobsKilledCount { get; internal set; }

			// Token: 0x1700070E RID: 1806
			// (get) Token: 0x060022F2 RID: 8946 RVA: 0x0006C7A7 File Offset: 0x0006A9A7
			// (set) Token: 0x060022F3 RID: 8947 RVA: 0x0006C7AF File Offset: 0x0006A9AF
			public Func<int> GetAllMobsKilledCount { get; internal set; }

			// Token: 0x1700070F RID: 1807
			// (get) Token: 0x060022F4 RID: 8948 RVA: 0x0006C7B8 File Offset: 0x0006A9B8
			// (set) Token: 0x060022F5 RID: 8949 RVA: 0x0006C7C0 File Offset: 0x0006A9C0
			public Func<int> GetAllMobsSpawnedCount { get; internal set; }

			// Token: 0x17000710 RID: 1808
			// (get) Token: 0x060022F6 RID: 8950 RVA: 0x0006C7C9 File Offset: 0x0006A9C9
			// (set) Token: 0x060022F7 RID: 8951 RVA: 0x0006C7D1 File Offset: 0x0006A9D1
			public Func<MobBehaviour.ID, long> GetMobDamageAmountToPlayer { get; internal set; }

			// Token: 0x17000711 RID: 1809
			// (get) Token: 0x060022F8 RID: 8952 RVA: 0x0006C7DA File Offset: 0x0006A9DA
			// (set) Token: 0x060022F9 RID: 8953 RVA: 0x0006C7E2 File Offset: 0x0006A9E2
			public Func<MobBehaviour.ID, long> GetPlayerDamageAmountToMob { get; internal set; }

			// Token: 0x17000712 RID: 1810
			// (get) Token: 0x060022FA RID: 8954 RVA: 0x0006C7EB File Offset: 0x0006A9EB
			// (set) Token: 0x060022FB RID: 8955 RVA: 0x0006C7F3 File Offset: 0x0006A9F3
			public Func<Dictionary<MobBehaviour.ID, int>> GetAllKilledMobs { get; internal set; }

			// Token: 0x17000713 RID: 1811
			// (get) Token: 0x060022FC RID: 8956 RVA: 0x0006C7FC File Offset: 0x0006A9FC
			// (set) Token: 0x060022FD RID: 8957 RVA: 0x0006C804 File Offset: 0x0006AA04
			public Func<Dictionary<MobBehaviour.ID, int>> GetAllRevivedMobs { get; internal set; }

			// Token: 0x17000714 RID: 1812
			// (get) Token: 0x060022FE RID: 8958 RVA: 0x0006C80D File Offset: 0x0006AA0D
			// (set) Token: 0x060022FF RID: 8959 RVA: 0x0006C815 File Offset: 0x0006AA15
			public Func<MobActivationModifierID, int> GetModifierPickedUpCount { get; internal set; }

			// Token: 0x17000715 RID: 1813
			// (get) Token: 0x06002300 RID: 8960 RVA: 0x0006C81E File Offset: 0x0006AA1E
			// (set) Token: 0x06002301 RID: 8961 RVA: 0x0006C826 File Offset: 0x0006AA26
			public Func<int> GetAllModifiersPickedUpCount { get; internal set; }

			// Token: 0x17000716 RID: 1814
			// (get) Token: 0x06002302 RID: 8962 RVA: 0x0006C82F File Offset: 0x0006AA2F
			// (set) Token: 0x06002303 RID: 8963 RVA: 0x0006C837 File Offset: 0x0006AA37
			public Func<int> GetPlayerLevelingEXP { get; internal set; }

			// Token: 0x06002304 RID: 8964 RVA: 0x0006C840 File Offset: 0x0006AA40
			public Data(IStatisticsSerializationData[] statisticsData)
			{
				this.SetSaveData(statisticsData);
			}

			// Token: 0x06002305 RID: 8965 RVA: 0x0006C850 File Offset: 0x0006AA50
			public void SetSaveData(IStatisticsSerializationData[] statisticsData)
			{
				if (this.statistics == null || this.statistics.Length == 0)
				{
					this.statistics = new IStatistics[statisticsData.Length];
					for (int i = 0; i < statisticsData.Length; i++)
					{
						IStatisticsSerializationData statisticsSerializationData = statisticsData[i].Clone();
						if (this.statistics[i] == null)
						{
							this.statistics[i] = statisticsSerializationData.CreateInstance();
						}
						this.statistics[i].SetSerializationData(statisticsSerializationData);
					}
					return;
				}
				for (int j = 0; j < statisticsData.Length; j++)
				{
					IStatisticsSerializationData statisticsSerializationData2 = statisticsData[j].Clone();
					for (int k = 0; k < this.statistics.Length; k++)
					{
						IStatistics statistics = this.statistics[k];
						if (object.Equals(statisticsSerializationData2.StatisticsType, statistics.GetType()))
						{
							statistics.SetSerializationData(statisticsSerializationData2);
						}
					}
				}
			}

			// Token: 0x06002306 RID: 8966 RVA: 0x0006C910 File Offset: 0x0006AB10
			public IStatisticsSerializationData[] GetSaveData()
			{
				IStatisticsSerializationData[] array = new IStatisticsSerializationData[this.statistics.Length];
				for (int i = 0; i < this.statistics.Length; i++)
				{
					array[i] = this.statistics[i].GetSerializationData();
				}
				return array;
			}

			// Token: 0x06002307 RID: 8967 RVA: 0x0006C94F File Offset: 0x0006AB4F
			public PlayerStatisticsManager.Data Clone()
			{
				return this.Copy<PlayerStatisticsManager.Data>();
			}

			// Token: 0x04001663 RID: 5731
			public string name;

			// Token: 0x04001664 RID: 5732
			[SerializeReference]
			[ManagedObjectField(typeof(StatisticsBase))]
			public IStatistics[] statistics;
		}
	}
}
