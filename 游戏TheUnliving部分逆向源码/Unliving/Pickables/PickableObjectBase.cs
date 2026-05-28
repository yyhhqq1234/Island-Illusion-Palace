using System;
using Game.Core;
using Game.Localization;
using Unliving.Factories;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x02000187 RID: 391
	public abstract class PickableObjectBase : PickableBase, IPickingContextProvider, IPurchasableObject, IPickableObject
	{
		// Token: 0x14000066 RID: 102
		// (add) Token: 0x06000ADC RID: 2780 RVA: 0x00023A68 File Offset: 0x00021C68
		// (remove) Token: 0x06000ADD RID: 2781 RVA: 0x00023AA0 File Offset: 0x00021CA0
		public event Action<PickableObjectBase, IPickableObjectCollector> Purchased;

		// Token: 0x14000067 RID: 103
		// (add) Token: 0x06000ADE RID: 2782 RVA: 0x00023AD8 File Offset: 0x00021CD8
		// (remove) Token: 0x06000ADF RID: 2783 RVA: 0x00023B10 File Offset: 0x00021D10
		public event Action<IPurchasableObject, IPickableObjectCollector> PurchaseFailed;

		// Token: 0x14000068 RID: 104
		// (add) Token: 0x06000AE0 RID: 2784 RVA: 0x00023B48 File Offset: 0x00021D48
		// (remove) Token: 0x06000AE1 RID: 2785 RVA: 0x00023B80 File Offset: 0x00021D80
		public event Action<IPickableObjectCollector> ObjectCollectionStarted;

		// Token: 0x14000069 RID: 105
		// (add) Token: 0x06000AE2 RID: 2786 RVA: 0x00023BB8 File Offset: 0x00021DB8
		// (remove) Token: 0x06000AE3 RID: 2787 RVA: 0x00023BF0 File Offset: 0x00021DF0
		public event Action<IPickableObjectCollector> ObjectCollected;

		// Token: 0x1400006A RID: 106
		// (add) Token: 0x06000AE4 RID: 2788 RVA: 0x00023C28 File Offset: 0x00021E28
		// (remove) Token: 0x06000AE5 RID: 2789 RVA: 0x00023C60 File Offset: 0x00021E60
		public event Action<string> AnimationEventFired;

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06000AE6 RID: 2790 RVA: 0x00023C95 File Offset: 0x00021E95
		// (set) Token: 0x06000AE7 RID: 2791 RVA: 0x00023C9D File Offset: 0x00021E9D
		public virtual MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext
		{
			get
			{
				return this.currentPickingContext;
			}
			set
			{
				this.currentPickingContext = value;
				this.pickupSettings = null;
			}
		}

		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06000AE8 RID: 2792 RVA: 0x00023CAD File Offset: 0x00021EAD
		public virtual bool IsPurchasable
		{
			get
			{
				return this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject || this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject;
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06000AE9 RID: 2793 RVA: 0x00023CC3 File Offset: 0x00021EC3
		// (set) Token: 0x06000AEA RID: 2794 RVA: 0x00023CCB File Offset: 0x00021ECB
		public IPurchasable PurchasableData { get; set; }

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06000AEB RID: 2795 RVA: 0x00023CD4 File Offset: 0x00021ED4
		// (set) Token: 0x06000AEC RID: 2796 RVA: 0x00023CDC File Offset: 0x00021EDC
		public bool CanBePickedInHomespace { get; protected set; }

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06000AED RID: 2797 RVA: 0x00023CE5 File Offset: 0x00021EE5
		protected override IPickingSettings PickupSettings
		{
			get
			{
				if (this.pickupSettings == null)
				{
					PlayerInputController playerInput = base.PlayerInput;
					this.pickupSettings = ((playerInput != null) ? playerInput.GetPickingSettings(this.CurrentPickingContext) : null);
				}
				return this.pickupSettings;
			}
		}

		// Token: 0x06000AEE RID: 2798 RVA: 0x00023D13 File Offset: 0x00021F13
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<LocalizationManager>(out this.localizationManager);
		}

		// Token: 0x06000AEF RID: 2799
		public abstract int CalculateItemLevel(out bool slotBusy, out bool hasSameItem);

		// Token: 0x06000AF0 RID: 2800 RVA: 0x00023D2E File Offset: 0x00021F2E
		public virtual void OnAnimationEventFired(string eventArg)
		{
			Action<string> animationEventFired = this.AnimationEventFired;
			if (animationEventFired == null)
			{
				return;
			}
			animationEventFired(eventArg);
		}

		// Token: 0x06000AF1 RID: 2801 RVA: 0x00023D41 File Offset: 0x00021F41
		protected virtual void OnObjectCollected()
		{
			Action<IPickableObjectCollector> objectCollected = this.ObjectCollected;
			if (objectCollected != null)
			{
				objectCollected(this.currentCollector);
			}
			this.currentCollector.OnPickableObjectCollected(this);
		}

		// Token: 0x06000AF2 RID: 2802 RVA: 0x00023D66 File Offset: 0x00021F66
		protected virtual void OnPurchased(IPickableObjectCollector collector)
		{
			Action<PickableObjectBase, IPickableObjectCollector> purchased = this.Purchased;
			if (purchased == null)
			{
				return;
			}
			purchased(this, collector);
		}

		// Token: 0x06000AF3 RID: 2803 RVA: 0x00023D7A File Offset: 0x00021F7A
		protected virtual void OnPurchaseFailed(IPickableObjectCollector collector)
		{
			Action<IPurchasableObject, IPickableObjectCollector> purchaseFailed = this.PurchaseFailed;
			if (purchaseFailed == null)
			{
				return;
			}
			purchaseFailed(this, collector);
		}

		// Token: 0x06000AF4 RID: 2804 RVA: 0x00023D8E File Offset: 0x00021F8E
		protected virtual void OnObjectCollectionStarted()
		{
			Action<IPickableObjectCollector> objectCollectionStarted = this.ObjectCollectionStarted;
			if (objectCollectionStarted == null)
			{
				return;
			}
			objectCollectionStarted(this.currentCollector);
		}

		// Token: 0x04000643 RID: 1603
		protected IPickableObjectCollector currentCollector;

		// Token: 0x04000644 RID: 1604
		protected LocalizationManager localizationManager;

		// Token: 0x04000645 RID: 1605
		private MultiRepresentationObjectInstantiator.ObjectType currentPickingContext = MultiRepresentationObjectInstantiator.ObjectType.PickableObject;
	}
}
