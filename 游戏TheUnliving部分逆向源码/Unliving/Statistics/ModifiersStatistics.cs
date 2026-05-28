using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Statistics
{
	// Token: 0x0200012B RID: 299
	public sealed class ModifiersStatistics : StatisticsBase<ModifiersStatistics.ModifiersStatData>
	{
		// Token: 0x0600078E RID: 1934 RVA: 0x00018C28 File Offset: 0x00016E28
		public override void SetGameData(IGame game, PlayerStatisticsManager.Data data)
		{
			base.SetGameData(game, data);
			if (game.Services.TryGet<PurchaseManager>(out this.purchaseManager))
			{
				this.purchaseManager.ItemPurchased += this.OnItemPurchased;
			}
			data.GetModifierPickedUpCount = new Func<MobActivationModifierID, int>(this.GetModifierPickedUpCount);
			data.GetAllModifiersPickedUpCount = new Func<int>(this.GetAllModifiersPickedUpCount);
		}

		// Token: 0x0600078F RID: 1935 RVA: 0x00018C8C File Offset: 0x00016E8C
		private int GetModifierPickedUpCount(MobActivationModifierID modifierID)
		{
			for (int i = 0; i < this.data.modifiersStats.Count; i++)
			{
				ModifiersStatistics.ModifierStat modifierStat = this.data.modifiersStats[i];
				if (modifierStat.modifierID == modifierID)
				{
					return modifierStat.pickedUpCount;
				}
			}
			return 0;
		}

		// Token: 0x06000790 RID: 1936 RVA: 0x00018CD8 File Offset: 0x00016ED8
		private int GetAllModifiersPickedUpCount()
		{
			int num = 0;
			for (int i = 0; i < this.data.modifiersStats.Count; i++)
			{
				num += this.data.modifiersStats[i].pickedUpCount;
			}
			return num;
		}

		// Token: 0x06000791 RID: 1937 RVA: 0x00018D1C File Offset: 0x00016F1C
		public override void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			base.OnSceneLoaded(storeStatisticsProvider);
			if (this.playerProvider != null)
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			this.OnPlayerRegistered(null);
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
		}

		// Token: 0x06000792 RID: 1938 RVA: 0x00018DA8 File Offset: 0x00016FA8
		private void OnItemPurchased(IPurchasable purchasable)
		{
			PurchasableActivationModifier purchasableActivationModifier = purchasable as PurchasableActivationModifier;
			if (purchasableActivationModifier != null)
			{
				MobActivationModifierID purchaseItem = purchasableActivationModifier.PurchaseItem;
				for (int i = 0; i < this.data.modifiersStats.Count; i++)
				{
					ModifiersStatistics.ModifierStat modifierStat = this.data.modifiersStats[i];
					if (modifierStat.modifierID == purchaseItem)
					{
						modifierStat.purchased = true;
						this.data.modifiersStats[i] = modifierStat;
						return;
					}
				}
				this.data.modifiersStats.Add(new ModifiersStatistics.ModifierStat
				{
					modifierID = purchaseItem,
					purchased = true
				});
			}
		}

		// Token: 0x06000793 RID: 1939 RVA: 0x00018E44 File Offset: 0x00017044
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.playerModifiersController.IsNull())
			{
				this.playerModifiersController.ModifierAdded -= this.OnModifierAdded;
			}
			if (player.IsNull())
			{
				return;
			}
			this.playerModifiersController = player.ActivationModifiersController;
			this.playerModifiersController.ModifierAdded += this.OnModifierAdded;
		}

		// Token: 0x06000794 RID: 1940 RVA: 0x00018EA4 File Offset: 0x000170A4
		private void OnModifierAdded(MobsActivationModifiersController controller, int slotIndex)
		{
			MobsActivationModifiersController.Slot slot = controller.Slots[slotIndex];
			if (slot.IsFree())
			{
				return;
			}
			MobActivationModifierID currentModifierID = slot.CurrentModifierID;
			if (currentModifierID == MobActivationModifierID.None)
			{
				return;
			}
			for (int i = 0; i < this.data.modifiersStats.Count; i++)
			{
				ModifiersStatistics.ModifierStat modifierStat = this.data.modifiersStats[i];
				if (modifierStat.modifierID == currentModifierID)
				{
					modifierStat.pickedUpCount++;
					this.data.modifiersStats[i] = modifierStat;
					return;
				}
			}
			this.data.modifiersStats.Add(new ModifiersStatistics.ModifierStat
			{
				modifierID = currentModifierID,
				pickedUpCount = 1
			});
		}

		// Token: 0x06000795 RID: 1941 RVA: 0x00018F54 File Offset: 0x00017154
		public override void Destroy()
		{
			if (!this.purchaseManager.IsNull())
			{
				this.purchaseManager.ItemPurchased -= this.OnItemPurchased;
			}
			if (!this.playerModifiersController.IsNull())
			{
				this.playerModifiersController.ModifierAdded -= this.OnModifierAdded;
			}
		}

		// Token: 0x04000466 RID: 1126
		private IPlayerProvider playerProvider;

		// Token: 0x04000467 RID: 1127
		private PurchaseManager purchaseManager;

		// Token: 0x04000468 RID: 1128
		private MobsActivationModifiersController playerModifiersController;

		// Token: 0x0200043E RID: 1086
		[Serializable]
		public sealed class ModifiersStatData : StatisticsSerializationDataBase
		{
			// Token: 0x1700071B RID: 1819
			// (get) Token: 0x06002329 RID: 9001 RVA: 0x0006CD91 File Offset: 0x0006AF91
			public override Type StatisticsType
			{
				get
				{
					return typeof(ModifiersStatistics);
				}
			}

			// Token: 0x0600232A RID: 9002 RVA: 0x0006CD9D File Offset: 0x0006AF9D
			public override IStatistics CreateInstance()
			{
				return new ModifiersStatistics();
			}

			// Token: 0x0600232B RID: 9003 RVA: 0x0006CDA4 File Offset: 0x0006AFA4
			public override void Initialize()
			{
			}

			// Token: 0x0400167E RID: 5758
			public readonly List<ModifiersStatistics.ModifierStat> modifiersStats = new List<ModifiersStatistics.ModifierStat>();
		}

		// Token: 0x0200043F RID: 1087
		[Serializable]
		public struct ModifierStat
		{
			// Token: 0x0400167F RID: 5759
			public MobActivationModifierID modifierID;

			// Token: 0x04001680 RID: 5760
			public bool purchased;

			// Token: 0x04001681 RID: 5761
			public int pickedUpCount;
		}
	}
}
