using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Statistics
{
	// Token: 0x0200012A RID: 298
	[Serializable]
	public sealed class MobsStatistics : StatisticsBase<MobsStatistics.MobsStatisticsData>
	{
		// Token: 0x06000785 RID: 1925 RVA: 0x00018624 File Offset: 0x00016824
		public override void SetGameData(IGame game, PlayerStatisticsManager.Data playerStatisticsData)
		{
			base.SetGameData(game, playerStatisticsData);
			playerStatisticsData.GetMobsKilledCount = new Func<MobBehaviour.ID, int>(this.data.GetMobsKilledCount);
			playerStatisticsData.GetMobDamageAmountToPlayer = new Func<MobBehaviour.ID, long>(this.data.GetMobDamageAmountToPlayer);
			playerStatisticsData.GetAllKilledMobs = new Func<Dictionary<MobBehaviour.ID, int>>(this.data.GetAllKilledMobs);
			playerStatisticsData.GetAllRevivedMobs = new Func<Dictionary<MobBehaviour.ID, int>>(this.data.GetAllRevivedMobs);
			playerStatisticsData.GetAllMobsKilledCount = new Func<int>(this.data.GetAllMobsKilledCount);
			playerStatisticsData.GetAllMobsSpawnedCount = new Func<int>(this.data.GetAllSpawnedMobsCount);
			playerStatisticsData.GetAllRevivedMobsCount = new Func<int>(this.data.GetAllRevivedMobsCount);
		}

		// Token: 0x06000786 RID: 1926 RVA: 0x000186DC File Offset: 0x000168DC
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.RevivedMob -= this.OnMobRevived;
				this.currentPlayer.DamageApplied -= this.OnDamageApplied;
				this.currentPlayer.AbilityUsedOnTarget -= this.OnPlayerAbilityUsed;
			}
			this.currentPlayer = player;
			this.currentPlayer.RevivedMob += this.OnMobRevived;
			this.currentPlayer.DamageApplied += this.OnDamageApplied;
			this.currentPlayer.AbilityUsedOnTarget += this.OnPlayerAbilityUsed;
			this.playerAbilitiesController = (this.currentPlayer.AbilitiesController as PlayerAbilitiesController);
		}

		// Token: 0x06000787 RID: 1927 RVA: 0x000187A0 File Offset: 0x000169A0
		private void OnPlayerAbilityUsed(IAbility ability, object abilityTarget, object args)
		{
			if (this.playerAbilitiesController.IsCurrentlyEquipedMainBattleAbility(ability.ID))
			{
				BaseGameMob baseGameMob = abilityTarget as BaseGameMob;
				if (baseGameMob != null && !baseGameMob.IsAlive())
				{
					this.data.mainBattleAbilityKillsCount++;
					IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
					if (storeStatisticsProvider == null)
					{
						return;
					}
					storeStatisticsProvider.SetStatValue("main_attack_kills_count", this.data.mainBattleAbilityKillsCount);
				}
			}
		}

		// Token: 0x06000788 RID: 1928 RVA: 0x00018808 File Offset: 0x00016A08
		private void OnMobRevived(BaseGameMob gameMob, IRevivableGameMob revivableGameMob)
		{
			MobBehaviour mobBehaviour = gameMob as MobBehaviour;
			if (mobBehaviour != null)
			{
				int i = 0;
				while (i < this.data.mobsStats.Count)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.data.mobsStats[i];
					if (mobStat.mobID == mobBehaviour.ObjectID)
					{
						mobStat.revivedCount++;
						this.data.mobsStats[i] = mobStat;
						this.data.IncrementAllRevivedMobsCount();
						IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
						if (storeStatisticsProvider != null)
						{
							storeStatisticsProvider.SetStatValue(mobStat.RevivedMobsCountStatID, mobStat.revivedCount);
						}
						IStoreStatisticsProvider storeStatisticsProvider2 = this.storeStatisticsProvider;
						if (storeStatisticsProvider2 == null)
						{
							return;
						}
						storeStatisticsProvider2.SetStatValue("mobs_revived", this.data.GetAllRevivedMobsCount());
						return;
					}
					else
					{
						i++;
					}
				}
				MobsStatistics.MobsStatisticsData.MobStat item = new MobsStatistics.MobsStatisticsData.MobStat
				{
					mobID = mobBehaviour.ObjectID,
					revivedCount = 1
				};
				this.data.mobsStats.Add(item);
				this.data.IncrementAllRevivedMobsCount();
				IStoreStatisticsProvider storeStatisticsProvider3 = this.storeStatisticsProvider;
				if (storeStatisticsProvider3 != null)
				{
					storeStatisticsProvider3.SetStatValue(item.RevivedMobsCountStatID, 1);
				}
				IStoreStatisticsProvider storeStatisticsProvider4 = this.storeStatisticsProvider;
				if (storeStatisticsProvider4 == null)
				{
					return;
				}
				storeStatisticsProvider4.SetStatValue("mobs_revived", this.data.GetAllRevivedMobsCount());
			}
		}

		// Token: 0x06000789 RID: 1929 RVA: 0x00018948 File Offset: 0x00016B48
		private void OnMobRegistered(BaseGameMob mob)
		{
			if (!mob.IsSummoned && !mob.isEnvironmentMob)
			{
				MobBehaviour mobBehaviour = mob as MobBehaviour;
				if (mobBehaviour != null)
				{
					bool isPlayerMob = mob.IsPlayerMob;
					for (int i = 0; i < this.data.mobsStats.Count; i++)
					{
						MobsStatistics.MobsStatisticsData.MobStat mobStat = this.data.mobsStats[i];
						if (mobStat.mobID == mobBehaviour.ObjectID)
						{
							mobStat.spawnedCount++;
							mobStat.isPlayerMob = isPlayerMob;
							this.data.mobsStats[i] = mobStat;
							return;
						}
					}
					this.data.mobsStats.Add(new MobsStatistics.MobsStatisticsData.MobStat
					{
						mobID = mobBehaviour.ObjectID,
						spawnedCount = 1,
						isPlayerMob = isPlayerMob
					});
				}
			}
		}

		// Token: 0x0600078A RID: 1930 RVA: 0x00018A18 File Offset: 0x00016C18
		private void OnDamageApplied(IDamageable damagedObject, float damageAmount)
		{
			MobBehaviour mobBehaviour = damagedObject.Behaviour as MobBehaviour;
			if (mobBehaviour != null)
			{
				MobBehaviour.ID objectID = mobBehaviour.ObjectID;
				bool flag = !damagedObject.IsAlive;
				for (int i = 0; i < this.data.mobsStats.Count; i++)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.data.mobsStats[i];
					if (mobStat.mobID == objectID)
					{
						mobStat.damage += (long)damageAmount;
						if (flag)
						{
							mobStat.killsCount++;
						}
						this.data.mobsStats[i] = mobStat;
						return;
					}
				}
				MobsStatistics.MobsStatisticsData.MobStat item = new MobsStatistics.MobsStatisticsData.MobStat
				{
					mobID = objectID,
					damage = (long)damageAmount
				};
				if (flag)
				{
					item.killsCount = 1;
				}
				this.data.mobsStats.Add(item);
			}
		}

		// Token: 0x0600078B RID: 1931 RVA: 0x00018AF0 File Offset: 0x00016CF0
		public override void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			base.OnSceneLoaded(storeStatisticsProvider);
			this.data.Initialize();
			if (this.currentGame == null)
			{
				return;
			}
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
			if (this.currentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager))
			{
				this.gameSessionManager.MobRegistered += this.OnMobRegistered;
			}
		}

		// Token: 0x0600078C RID: 1932 RVA: 0x00018B9C File Offset: 0x00016D9C
		public override void Destroy()
		{
			if (!this.playerProvider.IsNull())
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.RevivedMob -= this.OnMobRevived;
				this.currentPlayer.DamageApplied -= this.OnDamageApplied;
				this.currentPlayer.AbilityUsedOnTarget -= this.OnPlayerAbilityUsed;
			}
		}

		// Token: 0x04000462 RID: 1122
		private IPlayerProvider playerProvider;

		// Token: 0x04000463 RID: 1123
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000464 RID: 1124
		private GameSessionManager gameSessionManager;

		// Token: 0x04000465 RID: 1125
		private PlayerAbilitiesController playerAbilitiesController;

		// Token: 0x0200043D RID: 1085
		[Serializable]
		public sealed class MobsStatisticsData : StatisticsSerializationDataBase
		{
			// Token: 0x1700071A RID: 1818
			// (get) Token: 0x0600231B RID: 8987 RVA: 0x0006CB4E File Offset: 0x0006AD4E
			public override Type StatisticsType
			{
				get
				{
					return typeof(MobsStatistics);
				}
			}

			// Token: 0x0600231C RID: 8988 RVA: 0x0006CB5A File Offset: 0x0006AD5A
			public override IStatistics CreateInstance()
			{
				return new MobsStatistics();
			}

			// Token: 0x0600231D RID: 8989 RVA: 0x0006CB64 File Offset: 0x0006AD64
			public override void Initialize()
			{
				this.allRevivedMobsCount = 0;
				this.allKilledMobsCount = 0;
				this.allSpawnedMobsCount = 0;
				for (int i = 0; i < this.mobsStats.Count; i++)
				{
					this.allRevivedMobsCount += this.mobsStats[i].revivedCount;
					this.allKilledMobsCount += this.mobsStats[i].killsCount;
					this.allSpawnedMobsCount += this.mobsStats[i].spawnedCount;
				}
			}

			// Token: 0x0600231E RID: 8990 RVA: 0x0006CBF6 File Offset: 0x0006ADF6
			internal int GetAllRevivedMobsCount()
			{
				return this.allRevivedMobsCount;
			}

			// Token: 0x0600231F RID: 8991 RVA: 0x0006CBFE File Offset: 0x0006ADFE
			internal int IncrementAllRevivedMobsCount()
			{
				this.allRevivedMobsCount++;
				return this.allRevivedMobsCount;
			}

			// Token: 0x06002320 RID: 8992 RVA: 0x0006CC14 File Offset: 0x0006AE14
			internal int GetAllMobsKilledCount()
			{
				return this.allKilledMobsCount;
			}

			// Token: 0x06002321 RID: 8993 RVA: 0x0006CC1C File Offset: 0x0006AE1C
			internal int IncrementAllKilledMobsCount()
			{
				this.allKilledMobsCount++;
				return this.allKilledMobsCount;
			}

			// Token: 0x06002322 RID: 8994 RVA: 0x0006CC32 File Offset: 0x0006AE32
			internal int GetAllSpawnedMobsCount()
			{
				return this.allSpawnedMobsCount;
			}

			// Token: 0x06002323 RID: 8995 RVA: 0x0006CC3A File Offset: 0x0006AE3A
			internal int IncrementAllSpawnedMobsCount()
			{
				this.allSpawnedMobsCount++;
				return this.allSpawnedMobsCount;
			}

			// Token: 0x06002324 RID: 8996 RVA: 0x0006CC50 File Offset: 0x0006AE50
			internal int GetMobsKilledCount(MobBehaviour.ID mobID)
			{
				for (int i = 0; i < this.mobsStats.Count; i++)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.mobsStats[i];
					if (mobStat.mobID == mobID)
					{
						return mobStat.killsCount;
					}
				}
				return 0;
			}

			// Token: 0x06002325 RID: 8997 RVA: 0x0006CC94 File Offset: 0x0006AE94
			internal long GetMobDamageAmountToPlayer(MobBehaviour.ID mobID)
			{
				for (int i = 0; i < this.mobsStats.Count; i++)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.mobsStats[i];
					if (mobStat.mobID == mobID)
					{
						return mobStat.damage;
					}
				}
				return 0L;
			}

			// Token: 0x06002326 RID: 8998 RVA: 0x0006CCD8 File Offset: 0x0006AED8
			internal Dictionary<MobBehaviour.ID, int> GetAllKilledMobs()
			{
				Dictionary<MobBehaviour.ID, int> dictionary = new Dictionary<MobBehaviour.ID, int>();
				for (int i = 0; i < this.mobsStats.Count; i++)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.mobsStats[i];
					if (mobStat.killsCount > 0)
					{
						dictionary.Add(mobStat.mobID, mobStat.killsCount);
					}
				}
				return dictionary;
			}

			// Token: 0x06002327 RID: 8999 RVA: 0x0006CD2C File Offset: 0x0006AF2C
			internal Dictionary<MobBehaviour.ID, int> GetAllRevivedMobs()
			{
				Dictionary<MobBehaviour.ID, int> dictionary = new Dictionary<MobBehaviour.ID, int>();
				for (int i = 0; i < this.mobsStats.Count; i++)
				{
					MobsStatistics.MobsStatisticsData.MobStat mobStat = this.mobsStats[i];
					if (mobStat.revivedCount > 0)
					{
						dictionary.Add(mobStat.mobID, mobStat.revivedCount);
					}
				}
				return dictionary;
			}

			// Token: 0x04001676 RID: 5750
			internal const string MainBattleAbilityKillsCountStatID = "main_attack_kills_count";

			// Token: 0x04001677 RID: 5751
			internal const string AllRevivedMobsCountStatID = "mobs_revived";

			// Token: 0x04001678 RID: 5752
			internal const string RevivedMobsCountStatIDPrefix = "revived_";

			// Token: 0x04001679 RID: 5753
			public readonly List<MobsStatistics.MobsStatisticsData.MobStat> mobsStats = new List<MobsStatistics.MobsStatisticsData.MobStat>();

			// Token: 0x0400167A RID: 5754
			private int allRevivedMobsCount;

			// Token: 0x0400167B RID: 5755
			private int allKilledMobsCount;

			// Token: 0x0400167C RID: 5756
			private int allSpawnedMobsCount;

			// Token: 0x0400167D RID: 5757
			internal int mainBattleAbilityKillsCount;

			// Token: 0x020005AB RID: 1451
			public struct MobStatField<T>
			{
				// Token: 0x1700081D RID: 2077
				// (get) Token: 0x060027D0 RID: 10192 RVA: 0x0007C6B0 File Offset: 0x0007A8B0
				public string GetStatID
				{
					get
					{
						string str = this.prefix;
						T t = this.value;
						return str + ((t != null) ? t.ToString() : null);
					}
				}

				// Token: 0x04001D17 RID: 7447
				public string prefix;

				// Token: 0x04001D18 RID: 7448
				public T value;
			}

			// Token: 0x020005AC RID: 1452
			[Serializable]
			public struct MobStat
			{
				// Token: 0x1700081E RID: 2078
				// (get) Token: 0x060027D1 RID: 10193 RVA: 0x0007C6E7 File Offset: 0x0007A8E7
				public string RevivedMobsCountStatID
				{
					get
					{
						return "revived_" + this.mobID.ToString();
					}
				}

				// Token: 0x04001D19 RID: 7449
				public MobBehaviour.ID mobID;

				// Token: 0x04001D1A RID: 7450
				public bool isPlayerMob;

				// Token: 0x04001D1B RID: 7451
				public int revivedCount;

				// Token: 0x04001D1C RID: 7452
				public int spawnedCount;

				// Token: 0x04001D1D RID: 7453
				public long damage;

				// Token: 0x04001D1E RID: 7454
				public int killsCount;
			}
		}
	}
}
