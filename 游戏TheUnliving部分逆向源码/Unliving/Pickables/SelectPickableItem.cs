using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Currencies;
using Unliving.DropSystem;
using Unliving.Factories;
using Unliving.LeveledItems;

namespace Unliving.Pickables
{
	// Token: 0x0200018A RID: 394
	public abstract class SelectPickableItem : NonFactoryPickableBase
	{
		// Token: 0x1400006B RID: 107
		// (add) Token: 0x06000B0A RID: 2826 RVA: 0x00024208 File Offset: 0x00022408
		// (remove) Token: 0x06000B0B RID: 2827 RVA: 0x00024240 File Offset: 0x00022440
		public event Action PickablesCreated;

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06000B0C RID: 2828 RVA: 0x00024275 File Offset: 0x00022475
		protected override string LocalizationID
		{
			get
			{
				return this.localizationID;
			}
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06000B0D RID: 2829 RVA: 0x0002427D File Offset: 0x0002247D
		public IReadOnlyList<IPickableObject> Pickables
		{
			get
			{
				return this.pickables;
			}
		}

		// Token: 0x06000B0E RID: 2830
		public abstract CurrencyOperationArgs GetCloseRewardArgs();

		// Token: 0x06000B0F RID: 2831 RVA: 0x00024285 File Offset: 0x00022485
		protected override void Start()
		{
			base.Start();
			base.CurrentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager);
		}

		// Token: 0x06000B10 RID: 2832 RVA: 0x000242A4 File Offset: 0x000224A4
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameItem)
		{
			slotBusy = false;
			hasSameItem = false;
			return 0;
		}

		// Token: 0x06000B11 RID: 2833 RVA: 0x000242AD File Offset: 0x000224AD
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000B12 RID: 2834 RVA: 0x000242B0 File Offset: 0x000224B0
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject && !base.PurchasableData.TryPurchase(this.CurrentPickingContext))
			{
				return;
			}
			if (this.dropSpawners == null || this.dropSpawners.Length == 0)
			{
				if (this.gameSessionManager.CurrentSessionState == SessionState.Freezed)
				{
					this.Disable();
				}
				return;
			}
			if (this.gameSessionManager.CurrentSessionState == SessionState.InProgress)
			{
				this.gameSessionManager.SetSessionState(SessionState.Freezed);
			}
			List<object> list = new List<object>();
			this.pickables.Clear();
			for (int i = 0; i < this.dropSpawners.Length; i++)
			{
				IPickableObject pickableObject;
				IDropable dropable;
				this.dropSpawners[i].SpawnPickable(list, out pickableObject, out dropable, null);
				if (!pickableObject.IsNull())
				{
					ILeveledItem leveledItem = pickableObject as ILeveledItem;
					if (leveledItem != null)
					{
						IPurchasableObject purchasableObject = pickableObject as IPurchasableObject;
						if (purchasableObject != null && purchasableObject.PurchasableData.ItemLevel > 0)
						{
							leveledItem.ItemLevel = purchasableObject.PurchasableData.ItemLevel;
						}
					}
					pickableObject.Component.gameObject.SetActive(false);
					list.Add(dropable.ObjectID);
					this.pickables.Add(pickableObject);
				}
			}
			if (list.Count == 0)
			{
				this.Disable();
				return;
			}
			Action pickablesCreated = this.PickablesCreated;
			if (pickablesCreated == null)
			{
				return;
			}
			pickablesCreated();
		}

		// Token: 0x06000B13 RID: 2835 RVA: 0x000243E1 File Offset: 0x000225E1
		public void OnItemSelected(IPickableObject selectedItem)
		{
			this.Disable();
		}

		// Token: 0x06000B14 RID: 2836 RVA: 0x000243EC File Offset: 0x000225EC
		private void Disable()
		{
			for (int i = 0; i < this.pickables.Count; i++)
			{
				UnityEngine.Object.Destroy(this.pickables[i].Component.gameObject);
			}
			UnityEngine.Object.Destroy(base.gameObject);
			this.gameSessionManager.SetSessionState(SessionState.InProgress);
		}

		// Token: 0x0400064C RID: 1612
		public string localizationID;

		// Token: 0x0400064D RID: 1613
		public DropSpawner[] dropSpawners;

		// Token: 0x0400064E RID: 1614
		private GameSessionManager gameSessionManager;

		// Token: 0x0400064F RID: 1615
		private readonly List<IPickableObject> pickables = new List<IPickableObject>();
	}
}
