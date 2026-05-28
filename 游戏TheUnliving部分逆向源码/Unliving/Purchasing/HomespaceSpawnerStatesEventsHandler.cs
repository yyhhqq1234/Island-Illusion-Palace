using System;
using Common.UnityExtensions;
using Game.Core;
using UltEvents;
using UnityEngine.Serialization;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x020000FD RID: 253
	public class HomespaceSpawnerStatesEventsHandler : GameBehaviourBase
	{
		// Token: 0x0600061E RID: 1566 RVA: 0x00014E24 File Offset: 0x00013024
		private void Start()
		{
			if (this.spawner != null)
			{
				if (this.spawner.SpawnedPickable != null)
				{
					this.OnPickableSpawned();
				}
				else
				{
					this.spawner.PickableObjectSpawned += this.OnPickableSpawned;
				}
				if (base.CurrentGame.Services.TryGet<PurchaseManager>(out this.purchaseManager))
				{
					this.purchaseManager.ItemPurchased += this.OnItemPurchased;
				}
			}
		}

		// Token: 0x0600061F RID: 1567 RVA: 0x00014EA0 File Offset: 0x000130A0
		private void OnPickableSpawned()
		{
			this.spawner.PickableObjectSpawned -= this.OnPickableSpawned;
			this.pickable = this.spawner.SpawnedPickable;
			this.UpdateState();
			if (!this.pickable.PurchasableData.Purchased)
			{
				this.pickable.Purchased += this.OnPickablePurchased;
			}
		}

		// Token: 0x06000620 RID: 1568 RVA: 0x00014F04 File Offset: 0x00013104
		private void OnItemPurchased(IPurchasable obj)
		{
			this.UpdateState();
		}

		// Token: 0x06000621 RID: 1569 RVA: 0x00014F0C File Offset: 0x0001310C
		private void OnPickablePurchased(PickableObjectBase arg1, IPickableObjectCollector arg2)
		{
			this.UpdateState();
		}

		// Token: 0x06000622 RID: 1570 RVA: 0x00014F14 File Offset: 0x00013114
		private void UpdateState()
		{
			PickableObjectBase pickableObjectBase = this.pickable;
			if (((pickableObjectBase != null) ? pickableObjectBase.PurchasableData : null) == null)
			{
				return;
			}
			if (this.pickable.PurchasableData.Purchased)
			{
				UltEvent ultEvent = this.purchasedStateEvents;
				if (ultEvent == null)
				{
					return;
				}
				ultEvent.Invoke();
				return;
			}
			else if (!this.pickable.PurchasableData.Locked)
			{
				UltEvent ultEvent2 = this.unlockedStateEvents;
				if (ultEvent2 == null)
				{
					return;
				}
				ultEvent2.Invoke();
				return;
			}
			else
			{
				UltEvent ultEvent3 = this.lockedStateEvents;
				if (ultEvent3 == null)
				{
					return;
				}
				ultEvent3.Invoke();
				return;
			}
		}

		// Token: 0x06000623 RID: 1571 RVA: 0x00014F8C File Offset: 0x0001318C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.spawner.IsNull())
			{
				this.spawner.PickableObjectSpawned -= this.OnPickableSpawned;
			}
			if (!this.pickable.IsNull())
			{
				this.pickable.Purchased -= this.OnPickablePurchased;
			}
			if (!this.purchaseManager.IsNull())
			{
				this.purchaseManager.ItemPurchased -= this.OnItemPurchased;
			}
		}

		// Token: 0x040003F5 RID: 1013
		public HomespaceShopObjectSpawnerBase spawner;

		// Token: 0x040003F6 RID: 1014
		[FormerlySerializedAs("activeStateEvents")]
		public UltEvent purchasedStateEvents;

		// Token: 0x040003F7 RID: 1015
		[FormerlySerializedAs("inactiveStateEvents")]
		public UltEvent unlockedStateEvents;

		// Token: 0x040003F8 RID: 1016
		public UltEvent lockedStateEvents;

		// Token: 0x040003F9 RID: 1017
		private PickableObjectBase pickable;

		// Token: 0x040003FA RID: 1018
		private PurchaseManager purchaseManager;
	}
}
