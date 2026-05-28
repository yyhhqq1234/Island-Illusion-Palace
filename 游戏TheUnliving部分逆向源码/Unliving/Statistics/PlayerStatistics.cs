using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Unliving.AbilityResources;
using Unliving.Currencies;
using Unliving.GameSession.PlayerLeveling;
using Unliving.Mobs;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.Statistics
{
	// Token: 0x0200012C RID: 300
	[Serializable]
	public sealed class PlayerStatistics : StatisticsBase<PlayerStatistics.PlayerStatisticsData>
	{
		// Token: 0x06000797 RID: 1943 RVA: 0x00018FB4 File Offset: 0x000171B4
		public override void SetGameData(IGame game, PlayerStatisticsManager.Data playerStatisticsData)
		{
			base.SetGameData(game, playerStatisticsData);
			if (game.Services.TryGet<PlayerProfileManager>(out this.playerProfileManager))
			{
				this.playerProfileManager.ProfileLoaded += this.OnProfileLoaded;
				this.OnProfileLoaded(this.playerProfileManager.CurrentPlayerProfile);
			}
			playerStatisticsData.GetCurrencyGainedAmount = new Func<CurrencyID, float>(this.data.GetCurrencyGainedAmount);
			playerStatisticsData.GetCurrencySpentAmount = new Func<CurrencyID, float>(this.data.GetCurrencySpentAmount);
			playerStatisticsData.GetPlayerDamageAmountToMob = new Func<MobBehaviour.ID, long>(this.data.GetPlayerDamageAmountToMob);
			playerStatisticsData.GetHPContainersLostCount = new Func<int>(this.data.GetHPContainersLostCount);
			playerStatisticsData.GetPlayerKilledCount = new Func<int>(this.data.GetPlayerKilledCount);
			playerStatisticsData.GetVitalEnergySpentAmount = new Func<int>(this.data.GetVitalEnergySpentAmount);
			playerStatisticsData.GetPlayerLevelingEXP = new Func<int>(this.data.GetPlayerLevelingEXP);
		}

		// Token: 0x06000798 RID: 1944 RVA: 0x000190A8 File Offset: 0x000172A8
		public override void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			base.OnSceneLoaded(storeStatisticsProvider);
			if (this.currentGame.Services.TryGet<IPlayerLevelingManager>(out this.playerLevelingManager))
			{
				this.playerLevelingManager.PlayerEXPChanged += this.OnPlayerExpChanged;
			}
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
		}

		// Token: 0x06000799 RID: 1945 RVA: 0x0001913D File Offset: 0x0001733D
		private void OnPlayerExpChanged(IPlayerLevelingManager manager, int lastEXP, int currentEXP)
		{
			this.data.playerLevelingEXP += currentEXP - lastEXP;
		}

		// Token: 0x0600079A RID: 1946 RVA: 0x00019154 File Offset: 0x00017354
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.ConsumingAbilityResources -= this.OnConsumingAbilityResources;
			}
			if (this.playerMobsGroupController != null)
			{
				this.playerMobsGroupController.MobAdded -= this.OnPlayerGroupMobAdded;
			}
			if (!this.hitPointsController.IsNull())
			{
				this.hitPointsController.HitPointsChanged -= this.OnPlayerHealthChanged;
				this.hitPointsController.ContainerDestroyed -= this.OnContainerDestroyed;
			}
			this.currentPlayer = player;
			if (!this.currentPlayer.IsNull())
			{
				if (this.currentPlayer.Group != null)
				{
					this.playerMobsGroupController = this.currentPlayer.Group;
					this.playerMobsGroupController.MobAdded += this.OnPlayerGroupMobAdded;
					IReadOnlyList<BaseGameMob> mobs = this.playerMobsGroupController.Mobs;
					if (mobs != null)
					{
						for (int i = 0; i < mobs.Count; i++)
						{
							this.OnPlayerGroupMobAdded(this.playerMobsGroupController, mobs[i]);
						}
					}
				}
				this.currentPlayer.ConsumingAbilityResources += this.OnConsumingAbilityResources;
				VitalEnergyHitPointsController vitalEnergyHitPointsController = this.currentPlayer.HitPointsController as VitalEnergyHitPointsController;
				if (vitalEnergyHitPointsController != null)
				{
					this.hitPointsController = vitalEnergyHitPointsController;
					this.hitPointsController.HitPointsChanged += this.OnPlayerHealthChanged;
					this.hitPointsController.ContainerDestroyed += this.OnContainerDestroyed;
				}
			}
		}

		// Token: 0x0600079B RID: 1947 RVA: 0x000192BE File Offset: 0x000174BE
		private void OnPlayerGroupMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (mob.IsNull())
			{
				return;
			}
			mob.ConsumingAbilityResources += this.OnConsumingAbilityResources;
			mob.Destroyed += this.OnMobDestroyed;
		}

		// Token: 0x0600079C RID: 1948 RVA: 0x000192F0 File Offset: 0x000174F0
		private void OnMobDestroyed(object obj)
		{
			BaseGameMob baseGameMob = obj as BaseGameMob;
			if (baseGameMob != null)
			{
				baseGameMob.ConsumingAbilityResources -= this.OnConsumingAbilityResources;
				baseGameMob.Destroyed -= this.OnMobDestroyed;
			}
		}

		// Token: 0x0600079D RID: 1949 RVA: 0x0001932C File Offset: 0x0001752C
		private void OnConsumingAbilityResources(BaseAbility ability, IReadOnlyList<CollectableAbilityResource> resources)
		{
			this.consumingResourcesBuffer.Clear();
			for (int i = 0; i < resources.Count; i++)
			{
				AbilityResourceType type = resources[i].type;
				if (this.consumingResourcesBuffer.ContainsKey(type))
				{
					Dictionary<AbilityResourceType, int> dictionary = this.consumingResourcesBuffer;
					AbilityResourceType key = type;
					int num = dictionary[key];
					dictionary[key] = num + 1;
				}
				else
				{
					this.consumingResourcesBuffer.Add(type, 1);
				}
			}
			foreach (KeyValuePair<AbilityResourceType, int> keyValuePair in this.consumingResourcesBuffer)
			{
				int value = keyValuePair.Value;
				AbilityResourceType key2 = keyValuePair.Key;
				if (key2 == AbilityResourceType.Blood)
				{
					this.data.collectedBloodAmount += value;
					IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
					if (storeStatisticsProvider != null)
					{
						storeStatisticsProvider.SetStatValue("resources_collected_blood", this.data.collectedBloodAmount);
					}
				}
				else if (key2 == AbilityResourceType.Bone)
				{
					this.data.collectedBoneAmount += value;
					IStoreStatisticsProvider storeStatisticsProvider2 = this.storeStatisticsProvider;
					if (storeStatisticsProvider2 != null)
					{
						storeStatisticsProvider2.SetStatValue("resources_collected_bone", this.data.collectedBoneAmount);
					}
				}
				else if (key2 == AbilityResourceType.Echo)
				{
					this.data.collectedEchoAmount += value;
					IStoreStatisticsProvider storeStatisticsProvider3 = this.storeStatisticsProvider;
					if (storeStatisticsProvider3 != null)
					{
						storeStatisticsProvider3.SetStatValue("resources_collected_echo", this.data.collectedEchoAmount);
					}
				}
			}
		}

		// Token: 0x0600079E RID: 1950 RVA: 0x000194A8 File Offset: 0x000176A8
		private void OnContainerDestroyed(IEnergyContainer obj)
		{
			if (this.currentPlayer.IsAlive())
			{
				this.data.hitPointContainersLost++;
				IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
				if (storeStatisticsProvider == null)
				{
					return;
				}
				storeStatisticsProvider.SetStatValue("hp_containers_lost", this.data.hitPointContainersLost);
			}
		}

		// Token: 0x0600079F RID: 1951 RVA: 0x000194F8 File Offset: 0x000176F8
		private void OnPlayerHealthChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (args is VitalEnergyHitPointsController.ConsumeVitalEnergyArgs)
			{
				this.data.consumeEnergyAmount += (int)args.Amount;
				IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
				if (storeStatisticsProvider != null)
				{
					storeStatisticsProvider.SetStatValue("consume_energy_amount", this.data.consumeEnergyAmount);
				}
			}
			if (!args.IsDamage || args.Amount <= 0f)
			{
				return;
			}
			HitPointsController.HPChangingArgs hpchangingArgs = args as HitPointsController.HPChangingArgs;
			if (hpchangingArgs != null && hpchangingArgs.isVitalEnergyExchange)
			{
				this.data.vitalEnergyExchangeAmount += (int)hpchangingArgs.amount;
				IStoreStatisticsProvider storeStatisticsProvider2 = this.storeStatisticsProvider;
				if (storeStatisticsProvider2 != null)
				{
					storeStatisticsProvider2.SetStatValue("consume_hp_for_ability_amount", this.data.vitalEnergyExchangeAmount);
				}
			}
			MobBehaviour mobBehaviour = sender as MobBehaviour;
			if (mobBehaviour == null || mobBehaviour.IsPlayerMob)
			{
				return;
			}
			MobBehaviour.ID objectID = mobBehaviour.ObjectID;
			bool flag = !this.currentPlayer.IsAlive();
			if (flag)
			{
				this.data.playerKilledCount++;
				IStoreStatisticsProvider storeStatisticsProvider3 = this.storeStatisticsProvider;
				if (storeStatisticsProvider3 != null)
				{
					storeStatisticsProvider3.SetStatValue("player_killed_count", this.data.playerKilledCount);
				}
			}
			for (int i = 0; i < this.data.mobsDamageData.Count; i++)
			{
				PlayerStatistics.PlayerStatisticsData.MobDamageData mobDamageData = this.data.mobsDamageData[i];
				if (mobDamageData.mobID == objectID)
				{
					mobDamageData.damage += (long)args.Amount;
					if (flag)
					{
						mobDamageData.killPlayerCount++;
					}
					this.data.mobsDamageData[i] = mobDamageData;
					return;
				}
			}
			PlayerStatistics.PlayerStatisticsData.MobDamageData item = new PlayerStatistics.PlayerStatisticsData.MobDamageData
			{
				mobID = objectID,
				damage = (long)args.Amount
			};
			if (flag)
			{
				item.killPlayerCount = 1;
			}
			this.data.mobsDamageData.Add(item);
		}

		// Token: 0x060007A0 RID: 1952 RVA: 0x000196C4 File Offset: 0x000178C4
		private void OnProfileLoaded(PlayerProfile profile)
		{
			if (this.currentProfile != null)
			{
				this.currentProfile.CurrencyOperationSucceed -= this.OnCurrencyOperationSucceed;
			}
			if (profile == null)
			{
				return;
			}
			this.currentProfile = profile;
			this.currentProfile.CurrencyOperationSucceed += this.OnCurrencyOperationSucceed;
		}

		// Token: 0x060007A1 RID: 1953 RVA: 0x00019712 File Offset: 0x00017912
		private void OnCurrencyOperationSucceed(ICurrencyOperationArgs args)
		{
			if (args.Spending)
			{
				this.UpdateCurrencyList(args, ref this.data.currenciesSpent);
				return;
			}
			this.UpdateCurrencyList(args, ref this.data.currenciesGained);
		}

		// Token: 0x060007A2 RID: 1954 RVA: 0x00019744 File Offset: 0x00017944
		private void UpdateCurrencyList(ICurrencyOperationArgs operationArgs, ref List<ICurrencyOperationArgs> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].CurrencyID == operationArgs.CurrencyID)
				{
					list[i].Amount += operationArgs.Amount;
					return;
				}
			}
			list.Add(operationArgs);
		}

		// Token: 0x060007A3 RID: 1955 RVA: 0x0001979B File Offset: 0x0001799B
		public override IStatisticsSerializationData GetSerializationData()
		{
			return this.data;
		}

		// Token: 0x060007A4 RID: 1956 RVA: 0x000197A4 File Offset: 0x000179A4
		public override void Destroy()
		{
			if (!this.playerProfileManager.IsNull())
			{
				this.playerProfileManager.ProfileLoaded -= this.OnProfileLoaded;
			}
			if (this.currentProfile != null)
			{
				this.currentProfile.CurrencyOperationSucceed -= this.OnCurrencyOperationSucceed;
			}
			if (this.playerProvider != null)
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			if (this.playerLevelingManager != null)
			{
				this.playerLevelingManager.PlayerEXPChanged -= this.OnPlayerExpChanged;
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.ConsumingAbilityResources -= this.OnConsumingAbilityResources;
			}
			if (!this.hitPointsController.IsNull())
			{
				this.hitPointsController.HitPointsChanged -= this.OnPlayerHealthChanged;
				this.hitPointsController.ContainerDestroyed -= this.OnContainerDestroyed;
			}
			if (this.playerMobsGroupController != null)
			{
				IReadOnlyList<BaseGameMob> mobs = this.playerMobsGroupController.Mobs;
				if (mobs != null)
				{
					for (int i = 0; i < mobs.Count; i++)
					{
						BaseGameMob baseGameMob = mobs[i];
						baseGameMob.ConsumingAbilityResources -= this.OnConsumingAbilityResources;
						baseGameMob.Destroyed -= this.OnMobDestroyed;
					}
				}
				this.playerMobsGroupController.MobAdded -= this.OnPlayerGroupMobAdded;
			}
		}

		// Token: 0x04000469 RID: 1129
		private IPlayerLevelingManager playerLevelingManager;

		// Token: 0x0400046A RID: 1130
		private PlayerProfileManager playerProfileManager;

		// Token: 0x0400046B RID: 1131
		private PlayerProfile currentProfile;

		// Token: 0x0400046C RID: 1132
		private IPlayerProvider playerProvider;

		// Token: 0x0400046D RID: 1133
		private PlayerBehaviour currentPlayer;

		// Token: 0x0400046E RID: 1134
		private VitalEnergyHitPointsController hitPointsController;

		// Token: 0x0400046F RID: 1135
		private GameMobsGroupControllerBase playerMobsGroupController;

		// Token: 0x04000470 RID: 1136
		private readonly Dictionary<AbilityResourceType, int> consumingResourcesBuffer = new Dictionary<AbilityResourceType, int>(Enum.GetValues(typeof(AbilityResourceType)).Length);

		// Token: 0x02000440 RID: 1088
		[Serializable]
		public sealed class PlayerStatisticsData : StatisticsSerializationDataBase
		{
			// Token: 0x1700071C RID: 1820
			// (get) Token: 0x0600232D RID: 9005 RVA: 0x0006CDB9 File Offset: 0x0006AFB9
			public override Type StatisticsType
			{
				get
				{
					return typeof(PlayerStatistics);
				}
			}

			// Token: 0x0600232E RID: 9006 RVA: 0x0006CDC5 File Offset: 0x0006AFC5
			public override IStatistics CreateInstance()
			{
				return new PlayerStatistics();
			}

			// Token: 0x0600232F RID: 9007 RVA: 0x0006CDCC File Offset: 0x0006AFCC
			public override void Initialize()
			{
			}

			// Token: 0x06002330 RID: 9008 RVA: 0x0006CDD0 File Offset: 0x0006AFD0
			internal float GetCurrencyGainedAmount(CurrencyID currencyID)
			{
				for (int i = 0; i < this.currenciesGained.Count; i++)
				{
					ICurrencyOperationArgs currencyOperationArgs = this.currenciesGained[i];
					if (currencyOperationArgs.CurrencyID == currencyID)
					{
						return currencyOperationArgs.Amount;
					}
				}
				return 0f;
			}

			// Token: 0x06002331 RID: 9009 RVA: 0x0006CE18 File Offset: 0x0006B018
			internal float GetCurrencySpentAmount(CurrencyID currencyID)
			{
				for (int i = 0; i < this.currenciesSpent.Count; i++)
				{
					ICurrencyOperationArgs currencyOperationArgs = this.currenciesSpent[i];
					if (currencyOperationArgs.CurrencyID == currencyID)
					{
						return currencyOperationArgs.Amount;
					}
				}
				return 0f;
			}

			// Token: 0x06002332 RID: 9010 RVA: 0x0006CE60 File Offset: 0x0006B060
			internal long GetPlayerDamageAmountToMob(MobBehaviour.ID mobID)
			{
				for (int i = 0; i < this.mobsDamageData.Count; i++)
				{
					PlayerStatistics.PlayerStatisticsData.MobDamageData mobDamageData = this.mobsDamageData[i];
					if (mobDamageData.mobID == mobID)
					{
						return mobDamageData.damage;
					}
				}
				return 0L;
			}

			// Token: 0x06002333 RID: 9011 RVA: 0x0006CEA2 File Offset: 0x0006B0A2
			internal int GetHPContainersLostCount()
			{
				return this.hitPointContainersLost;
			}

			// Token: 0x06002334 RID: 9012 RVA: 0x0006CEAA File Offset: 0x0006B0AA
			internal int GetVitalEnergySpentAmount()
			{
				return this.consumeEnergyAmount;
			}

			// Token: 0x06002335 RID: 9013 RVA: 0x0006CEB2 File Offset: 0x0006B0B2
			internal int GetPlayerLevelingEXP()
			{
				return this.playerLevelingEXP;
			}

			// Token: 0x06002336 RID: 9014 RVA: 0x0006CEBA File Offset: 0x0006B0BA
			internal int GetPlayerKilledCount()
			{
				return this.playerKilledCount;
			}

			// Token: 0x04001682 RID: 5762
			internal const string HPContainersLostStatID = "hp_containers_lost";

			// Token: 0x04001683 RID: 5763
			internal const string PlayerKilledCountStatID = "player_killed_count";

			// Token: 0x04001684 RID: 5764
			internal const string ConsumeEnergyAmountStatID = "consume_energy_amount";

			// Token: 0x04001685 RID: 5765
			internal const string ConsumeHPUsingAbilityAmountStatID = "consume_hp_for_ability_amount";

			// Token: 0x04001686 RID: 5766
			internal const string CollectedBloodAmountStatID = "resources_collected_blood";

			// Token: 0x04001687 RID: 5767
			internal const string CollectedBoneAmountStatID = "resources_collected_bone";

			// Token: 0x04001688 RID: 5768
			internal const string CollectedEchoAmountStatID = "resources_collected_echo";

			// Token: 0x04001689 RID: 5769
			public int hitPointContainersLost;

			// Token: 0x0400168A RID: 5770
			[OptionalField]
			public int playerKilledCount;

			// Token: 0x0400168B RID: 5771
			public int consumeEnergyAmount;

			// Token: 0x0400168C RID: 5772
			[OptionalField]
			public int vitalEnergyExchangeAmount;

			// Token: 0x0400168D RID: 5773
			public int collectedBloodAmount;

			// Token: 0x0400168E RID: 5774
			public int collectedBoneAmount;

			// Token: 0x0400168F RID: 5775
			public int collectedEchoAmount;

			// Token: 0x04001690 RID: 5776
			public int playerLevelingEXP;

			// Token: 0x04001691 RID: 5777
			public readonly List<PlayerStatistics.PlayerStatisticsData.MobDamageData> mobsDamageData = new List<PlayerStatistics.PlayerStatisticsData.MobDamageData>();

			// Token: 0x04001692 RID: 5778
			public List<ICurrencyOperationArgs> currenciesGained = new List<ICurrencyOperationArgs>();

			// Token: 0x04001693 RID: 5779
			public List<ICurrencyOperationArgs> currenciesSpent = new List<ICurrencyOperationArgs>();

			// Token: 0x020005AD RID: 1453
			public struct MobDamageData
			{
				// Token: 0x04001D1F RID: 7455
				public MobBehaviour.ID mobID;

				// Token: 0x04001D20 RID: 7456
				public long damage;

				// Token: 0x04001D21 RID: 7457
				public int killPlayerCount;
			}
		}
	}
}
