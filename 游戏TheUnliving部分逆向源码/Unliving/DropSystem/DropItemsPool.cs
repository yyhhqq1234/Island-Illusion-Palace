using System;
using System.Collections.Generic;
using System.Linq;
using Common.CollectionsExtensions;
using Common.UnityExtensions;
using Game.Core;
using Unliving.Purchasing;

namespace Unliving.DropSystem
{
	// Token: 0x02000288 RID: 648
	[Serializable]
	public class DropItemsPool
	{
		// Token: 0x170004CF RID: 1231
		// (get) Token: 0x0600165C RID: 5724 RVA: 0x00047BAF File Offset: 0x00045DAF
		public IDropable[] PoolItems
		{
			get
			{
				if (this.overridePoolItems != null)
				{
					return this.overridePoolItems;
				}
				if (this.additionalPoolItems != null)
				{
					return this.poolItems.Concat(this.additionalPoolItems).ToArray<IDropable>();
				}
				return this.poolItems;
			}
		}

		// Token: 0x170004D0 RID: 1232
		// (get) Token: 0x0600165D RID: 5725 RVA: 0x00047BE5 File Offset: 0x00045DE5
		private PurchaseManager PurchaseManager
		{
			get
			{
				if (this.purchaseManager.IsNull())
				{
					this.purchaseManager = this.currentGame.Services.Get<PurchaseManager>();
				}
				return this.purchaseManager;
			}
		}

		// Token: 0x0600165E RID: 5726 RVA: 0x00047C10 File Offset: 0x00045E10
		public void Initialize(IGame currentGame)
		{
			this.currentGame = currentGame;
			this.poolItems = this.abilities.OfType<IDropable>().Concat(this.passiveAbilities).Concat(this.anotherDropables).Concat(this.activationModifiers).Concat(this.abilityActivatedContainers).ToArray<IDropable>();
			this.defaultPoolItems = this.poolItems.ToArray<IDropable>();
			this.dropablesBuffer = new List<IDropable>(this.PoolItems.Length);
		}

		// Token: 0x0600165F RID: 5727 RVA: 0x00047C8A File Offset: 0x00045E8A
		public void AddPoolItems(IDropable[] dropables)
		{
			this.additionalPoolItems = dropables;
		}

		// Token: 0x06001660 RID: 5728 RVA: 0x00047C93 File Offset: 0x00045E93
		public void ReplacePoolItems(IDropable[] dropables)
		{
			this.overridePoolItems = dropables;
		}

		// Token: 0x06001661 RID: 5729 RVA: 0x00047C9C File Offset: 0x00045E9C
		public void ResetPoolItems()
		{
			IDropable[] array = this.defaultPoolItems;
			this.poolItems = ((array != null) ? array.ToArray<IDropable>() : null);
		}

		// Token: 0x06001662 RID: 5730 RVA: 0x00047CB6 File Offset: 0x00045EB6
		public void RemovePoolItem(IDropable dropable)
		{
			if (dropable == null)
			{
				return;
			}
			this.poolItems = this.poolItems.Except(new IDropable[]
			{
				dropable
			}).ToArray<IDropable>();
		}

		// Token: 0x06001663 RID: 5731 RVA: 0x00047CDC File Offset: 0x00045EDC
		public bool TryGetItem(object objectID, out IDropable dropable)
		{
			for (int i = 0; i < this.PoolItems.Length; i++)
			{
				IDropable dropable2 = this.PoolItems[i];
				IPurchasable purchasable;
				if (object.Equals(dropable2.ObjectID, objectID) && (this.PurchaseManager.DebugUnlockAll || (this.PurchaseManager.TryGetPurchasable(dropable2.ObjectID, out purchasable) && purchasable.Purchased)))
				{
					dropable = dropable2;
					return true;
				}
			}
			dropable = null;
			return false;
		}

		// Token: 0x06001664 RID: 5732 RVA: 0x00047D48 File Offset: 0x00045F48
		public IDropable GetRandomItem(IList<object> exceptItems)
		{
			if (this.PoolItems == null)
			{
				return null;
			}
			if (this.PoolItems.Length == 1)
			{
				IDropable dropable = this.PoolItems[0];
				if (this.PurchaseManager.DebugUnlockAll)
				{
					return dropable;
				}
				IPurchasable purchasable;
				if (this.PurchaseManager.TryGetPurchasable(dropable.ObjectID, out purchasable))
				{
					if (!purchasable.Purchased)
					{
						return null;
					}
					return dropable;
				}
			}
			this.dropablesBuffer.Clear();
			for (int i = 0; i < this.PoolItems.Length; i++)
			{
				IDropable item = this.PoolItems[i];
				IPurchasable purchasable2;
				if ((exceptItems == null || !exceptItems.Any((object exceptItem) => object.Equals(exceptItem, item.ObjectID))) && (this.PurchaseManager.DebugUnlockAll || item is AnotherItemDropable || (this.PurchaseManager.TryGetPurchasable(item.ObjectID, out purchasable2) && purchasable2.Purchased)))
				{
					this.dropablesBuffer.Add(item);
				}
			}
			if (this.dropablesBuffer.Count == 0)
			{
				return null;
			}
			this.totalWeight = new float?(this.dropablesBuffer.GetTotalWeight(0, int.MaxValue));
			IDropable result;
			this.dropablesBuffer.GetRandomWeightedItem(out result, 0, int.MaxValue, this.totalWeight);
			return result;
		}

		// Token: 0x04000CF4 RID: 3316
		public AbilityActivatedContainerDropable[] abilityActivatedContainers;

		// Token: 0x04000CF5 RID: 3317
		public AbilityDropable[] abilities;

		// Token: 0x04000CF6 RID: 3318
		public PassiveAbilityDropable[] passiveAbilities;

		// Token: 0x04000CF7 RID: 3319
		public ActivationModifierDropable[] activationModifiers;

		// Token: 0x04000CF8 RID: 3320
		public AnotherItemDropable[] anotherDropables;

		// Token: 0x04000CF9 RID: 3321
		private float? totalWeight;

		// Token: 0x04000CFA RID: 3322
		private List<IDropable> dropablesBuffer;

		// Token: 0x04000CFB RID: 3323
		private IGame currentGame;

		// Token: 0x04000CFC RID: 3324
		private PurchaseManager purchaseManager;

		// Token: 0x04000CFD RID: 3325
		private IDropable[] poolItems;

		// Token: 0x04000CFE RID: 3326
		private IDropable[] defaultPoolItems;

		// Token: 0x04000CFF RID: 3327
		private IDropable[] additionalPoolItems;

		// Token: 0x04000D00 RID: 3328
		private IDropable[] overridePoolItems;
	}
}
