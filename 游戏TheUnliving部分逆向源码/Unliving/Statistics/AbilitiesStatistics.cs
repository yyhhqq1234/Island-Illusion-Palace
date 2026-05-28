using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Statistics
{
	// Token: 0x02000128 RID: 296
	public sealed class AbilitiesStatistics : StatisticsBase<AbilitiesStatistics.AbilitiesStatData>
	{
		// Token: 0x0600076F RID: 1903 RVA: 0x00017B54 File Offset: 0x00015D54
		private void OnAbilityUsed(AbilityID abilityID)
		{
			for (int i = 0; i < this.data.abilitiesStats.Count; i++)
			{
				AbilitiesStatistics.AbilityStat abilityStat = this.data.abilitiesStats[i];
				if (abilityStat.abilityID == abilityID)
				{
					abilityStat.usageCount++;
					this.data.abilitiesStats[i] = abilityStat;
					return;
				}
			}
			this.data.abilitiesStats.Add(new AbilitiesStatistics.AbilityStat
			{
				abilityID = abilityID,
				usageCount = 1
			});
		}

		// Token: 0x06000770 RID: 1904 RVA: 0x00017BE0 File Offset: 0x00015DE0
		private int GetAbilityKillsCount(AbilityID abilityID)
		{
			for (int i = 0; i < this.data.abilitiesStats.Count; i++)
			{
				AbilitiesStatistics.AbilityStat abilityStat = this.data.abilitiesStats[i];
				if (abilityStat.abilityID == abilityID)
				{
					return abilityStat.mobsKilledCount;
				}
			}
			return 0;
		}

		// Token: 0x06000771 RID: 1905 RVA: 0x00017C2C File Offset: 0x00015E2C
		private int GetAbilityUsageCount(AbilityID abilityID)
		{
			for (int i = 0; i < this.data.abilitiesStats.Count; i++)
			{
				AbilitiesStatistics.AbilityStat abilityStat = this.data.abilitiesStats[i];
				if (abilityStat.abilityID == abilityID)
				{
					return abilityStat.usageCount;
				}
			}
			return 0;
		}

		// Token: 0x06000772 RID: 1906 RVA: 0x00017C78 File Offset: 0x00015E78
		public override void SetGameData(IGame game, PlayerStatisticsManager.Data data)
		{
			base.SetGameData(game, data);
			if (game.Services.TryGet<PurchaseManager>(out this.purchaseManager))
			{
				this.purchaseManager.ItemPurchased += this.OnItemPurchased;
			}
			data.GetAbilityUsageCount = new Func<AbilityID, int>(this.GetAbilityUsageCount);
			data.GetAbilityKillsCount = new Func<AbilityID, int>(this.GetAbilityKillsCount);
		}

		// Token: 0x06000773 RID: 1907 RVA: 0x00017CDC File Offset: 0x00015EDC
		public override void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			base.OnSceneLoaded(storeStatisticsProvider);
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
		}

		// Token: 0x06000774 RID: 1908 RVA: 0x00017D44 File Offset: 0x00015F44
		private void OnItemPurchased(IPurchasable purchasable)
		{
			PurchaseItemAbility purchaseItemAbility = purchasable as PurchaseItemAbility;
			if (purchaseItemAbility != null)
			{
				AbilityID purchaseItem = purchaseItemAbility.PurchaseItem;
				for (int i = 0; i < this.data.abilitiesStats.Count; i++)
				{
					AbilitiesStatistics.AbilityStat abilityStat = this.data.abilitiesStats[i];
					if (abilityStat.abilityID == purchaseItem)
					{
						abilityStat.purchased = true;
						this.data.abilitiesStats[i] = abilityStat;
						return;
					}
				}
				this.data.abilitiesStats.Add(new AbilitiesStatistics.AbilityStat
				{
					abilityID = purchaseItem,
					purchased = true
				});
			}
		}

		// Token: 0x06000775 RID: 1909 RVA: 0x00017DE0 File Offset: 0x00015FE0
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.DamageApplied -= this.OnDamageApplied;
				this.currentPlayer.AbilitiesController.AbilityUsed -= this.OnAbilityUsed;
			}
			this.currentPlayer = player;
			this.currentPlayer.DamageApplied += this.OnDamageApplied;
			this.currentPlayer.AbilitiesController.AbilityUsed += this.OnAbilityUsed;
		}

		// Token: 0x06000776 RID: 1910 RVA: 0x00017E68 File Offset: 0x00016068
		private void OnDamageApplied(IDamageable damagedObject, float damageAmount)
		{
			if (damagedObject.IsAlive)
			{
				return;
			}
			BaseGameMob baseGameMob = damagedObject as BaseGameMob;
			if (baseGameMob != null && baseGameMob.isEnvironmentMob)
			{
				return;
			}
			IAbility ability = damagedObject.LastDamageSource as IAbility;
			if (ability != null)
			{
				AbilityID id = (AbilityID)ability.ID;
				int i = 0;
				while (i < this.data.abilitiesStats.Count)
				{
					AbilitiesStatistics.AbilityStat abilityStat = this.data.abilitiesStats[i];
					if (abilityStat.abilityID == id)
					{
						abilityStat.mobsKilledCount++;
						this.data.abilitiesStats[i] = abilityStat;
						IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
						if (storeStatisticsProvider == null)
						{
							return;
						}
						storeStatisticsProvider.SetStatValue(abilityStat.AbilityKillsCountStatID, abilityStat.mobsKilledCount);
						return;
					}
					else
					{
						i++;
					}
				}
				AbilitiesStatistics.AbilityStat abilityStat2 = new AbilitiesStatistics.AbilityStat
				{
					abilityID = id,
					mobsKilledCount = 1
				};
				this.data.abilitiesStats.Add(abilityStat2);
				IStoreStatisticsProvider storeStatisticsProvider2 = this.storeStatisticsProvider;
				if (storeStatisticsProvider2 == null)
				{
					return;
				}
				storeStatisticsProvider2.SetStatValue(abilityStat2.AbilityKillsCountStatID, abilityStat2.mobsKilledCount);
			}
		}

		// Token: 0x06000777 RID: 1911 RVA: 0x00017F71 File Offset: 0x00016171
		private void OnAbilityUsed(IAbility ability, object abilityUsingArgs)
		{
			this.OnAbilityUsed((AbilityID)ability.ID);
		}

		// Token: 0x06000778 RID: 1912 RVA: 0x00017F80 File Offset: 0x00016180
		public override void Destroy()
		{
			if (!this.playerProvider.IsNull())
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			if (!this.purchaseManager.IsNull())
			{
				this.purchaseManager.ItemPurchased -= this.OnItemPurchased;
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.DamageApplied -= this.OnDamageApplied;
				this.currentPlayer.AbilitiesController.AbilityUsed -= this.OnAbilityUsed;
			}
		}

		// Token: 0x04000459 RID: 1113
		private IPlayerProvider playerProvider;

		// Token: 0x0400045A RID: 1114
		private PlayerBehaviour currentPlayer;

		// Token: 0x0400045B RID: 1115
		private PurchaseManager purchaseManager;

		// Token: 0x02000439 RID: 1081
		[Serializable]
		public sealed class AbilitiesStatData : StatisticsSerializationDataBase
		{
			// Token: 0x17000717 RID: 1815
			// (get) Token: 0x06002308 RID: 8968 RVA: 0x0006C957 File Offset: 0x0006AB57
			public override Type StatisticsType
			{
				get
				{
					return typeof(AbilitiesStatistics);
				}
			}

			// Token: 0x06002309 RID: 8969 RVA: 0x0006C963 File Offset: 0x0006AB63
			public override IStatistics CreateInstance()
			{
				return new AbilitiesStatistics();
			}

			// Token: 0x0600230A RID: 8970 RVA: 0x0006C96A File Offset: 0x0006AB6A
			public override void Initialize()
			{
			}

			// Token: 0x04001665 RID: 5733
			internal const string AbilityKillsCountStatIDPrefix = "mobs_killed_using_ability_";

			// Token: 0x04001666 RID: 5734
			public readonly List<AbilitiesStatistics.AbilityStat> abilitiesStats = new List<AbilitiesStatistics.AbilityStat>();
		}

		// Token: 0x0200043A RID: 1082
		[Serializable]
		public struct AbilityStat
		{
			// Token: 0x17000718 RID: 1816
			// (get) Token: 0x0600230C RID: 8972 RVA: 0x0006C97F File Offset: 0x0006AB7F
			public readonly string AbilityKillsCountStatID
			{
				get
				{
					return "mobs_killed_using_ability_" + this.abilityID.ToString();
				}
			}

			// Token: 0x04001667 RID: 5735
			public AbilityID abilityID;

			// Token: 0x04001668 RID: 5736
			public int usageCount;

			// Token: 0x04001669 RID: 5737
			public int mobsKilledCount;

			// Token: 0x0400166A RID: 5738
			public bool purchased;
		}
	}
}
