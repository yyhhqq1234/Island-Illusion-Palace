using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.Stores
{
	// Token: 0x0200004E RID: 78
	public abstract class StoreInteractionSpotBase<T> : PickableObjectBase, IStoreInteractionSpot, IPickableObject where T : class, IStoreManager
	{
		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600026D RID: 621 RVA: 0x00009CC6 File Offset: 0x00007EC6
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (this.pickableObjectData == null)
				{
					this.pickableObjectData = new PickableObjectData
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, Array.Empty<string>())
					};
				}
				return this.pickableObjectData;
			}
		}

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x0600026E RID: 622 RVA: 0x00009CFD File Offset: 0x00007EFD
		// (set) Token: 0x0600026F RID: 623 RVA: 0x00009D00 File Offset: 0x00007F00
		public override MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext
		{
			get
			{
				return MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject;
			}
			set
			{
			}
		}

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000270 RID: 624 RVA: 0x00009D02 File Offset: 0x00007F02
		protected override IPickingSettings PickupSettings
		{
			get
			{
				return new OnClickPickingSettings();
			}
		}

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000271 RID: 625 RVA: 0x00009D09 File Offset: 0x00007F09
		protected override string LocalizationID
		{
			get
			{
				return this.metadataKey;
			}
		}

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000272 RID: 626 RVA: 0x00009D11 File Offset: 0x00007F11
		public Transform PivotTransform
		{
			get
			{
				if (this.cameraPivot.IsNull())
				{
					return base.transform;
				}
				return this.cameraPivot;
			}
		}

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000273 RID: 627 RVA: 0x00009D2D File Offset: 0x00007F2D
		public IStoreManager StoreManager
		{
			get
			{
				return this.storeManager;
			}
		}

		// Token: 0x14000027 RID: 39
		// (add) Token: 0x06000274 RID: 628 RVA: 0x00009D3C File Offset: 0x00007F3C
		// (remove) Token: 0x06000275 RID: 629 RVA: 0x00009D74 File Offset: 0x00007F74
		public event Action PlayerEnteredTrigger;

		// Token: 0x14000028 RID: 40
		// (add) Token: 0x06000276 RID: 630 RVA: 0x00009DAC File Offset: 0x00007FAC
		// (remove) Token: 0x06000277 RID: 631 RVA: 0x00009DE4 File Offset: 0x00007FE4
		public event Action PlayerExitedTrigger;

		// Token: 0x06000278 RID: 632 RVA: 0x00009E19 File Offset: 0x00008019
		public void OpenStore()
		{
			if (this.storeManager != null && !this.storeManager.IsStoreOpened)
			{
				this.storeManager.OpenStore(this);
			}
		}

		// Token: 0x06000279 RID: 633 RVA: 0x00009E4B File Offset: 0x0000804B
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<T>(out this.storeManager))
			{
				this.storeManager.StoreStateChanged += this.OnStoreStateChanged;
			}
		}

		// Token: 0x0600027A RID: 634 RVA: 0x00009E83 File Offset: 0x00008083
		private void OnStoreStateChanged(IStoreManager storeManager, bool isVisible)
		{
			this.collider2D.enabled = !isVisible;
		}

		// Token: 0x0600027B RID: 635 RVA: 0x00009E94 File Offset: 0x00008094
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return this.storeManager != null;
		}

		// Token: 0x0600027C RID: 636 RVA: 0x00009EA4 File Offset: 0x000080A4
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			this.OpenStore();
		}

		// Token: 0x0600027D RID: 637 RVA: 0x00009EAC File Offset: 0x000080AC
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameItem)
		{
			hasSameItem = false;
			slotBusy = false;
			return 0;
		}

		// Token: 0x0600027E RID: 638 RVA: 0x00009EB5 File Offset: 0x000080B5
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.storeManager.IsNull())
			{
				this.storeManager.StoreStateChanged -= this.OnStoreStateChanged;
			}
		}

		// Token: 0x04000178 RID: 376
		[SerializeField]
		private Transform cameraPivot;

		// Token: 0x04000179 RID: 377
		[SerializeField]
		private string metadataKey;

		// Token: 0x0400017A RID: 378
		private T storeManager;
	}
}
