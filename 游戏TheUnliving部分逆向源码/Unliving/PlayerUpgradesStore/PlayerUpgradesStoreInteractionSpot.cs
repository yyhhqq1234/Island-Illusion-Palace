using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;
using Unliving.Stores;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x0200011B RID: 283
	public sealed class PlayerUpgradesStoreInteractionSpot : PickableObjectBase, IStoreInteractionSpot, IPickableObject
	{
		// Token: 0x17000113 RID: 275
		// (get) Token: 0x060006C6 RID: 1734 RVA: 0x00015F62 File Offset: 0x00014162
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

		// Token: 0x17000114 RID: 276
		// (get) Token: 0x060006C7 RID: 1735 RVA: 0x00015F99 File Offset: 0x00014199
		protected override IPickingSettings PickupSettings
		{
			get
			{
				return new OnClickPickingSettings();
			}
		}

		// Token: 0x17000115 RID: 277
		// (get) Token: 0x060006C8 RID: 1736 RVA: 0x00015FA0 File Offset: 0x000141A0
		// (set) Token: 0x060006C9 RID: 1737 RVA: 0x00015FA3 File Offset: 0x000141A3
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

		// Token: 0x17000116 RID: 278
		// (get) Token: 0x060006CA RID: 1738 RVA: 0x00015FA5 File Offset: 0x000141A5
		protected override string LocalizationID
		{
			get
			{
				return this.metadataKey;
			}
		}

		// Token: 0x17000117 RID: 279
		// (get) Token: 0x060006CB RID: 1739 RVA: 0x00015FAD File Offset: 0x000141AD
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

		// Token: 0x17000118 RID: 280
		// (get) Token: 0x060006CC RID: 1740 RVA: 0x00015FC9 File Offset: 0x000141C9
		public IStoreManager StoreManager
		{
			get
			{
				return this.playerUpgradesStore;
			}
		}

		// Token: 0x060006CD RID: 1741 RVA: 0x00015FD1 File Offset: 0x000141D1
		public void OpenStore()
		{
			if (this.playerUpgradesStore == null || this.playerUpgradesStore.IsStoreOpened)
			{
				return;
			}
			this.playerUpgradesStore.OpenStore(this);
		}

		// Token: 0x060006CE RID: 1742 RVA: 0x00015FF5 File Offset: 0x000141F5
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			this.OpenStore();
		}

		// Token: 0x060006CF RID: 1743 RVA: 0x00015FFD File Offset: 0x000141FD
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return this.playerUpgradesStore != null;
		}

		// Token: 0x060006D0 RID: 1744 RVA: 0x00016008 File Offset: 0x00014208
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<IPlayerUpgradesStoreManager>(out this.playerUpgradesStore))
			{
				this.playerUpgradesStore.StoreStateChanged += this.OnStoreStateChanged;
			}
		}

		// Token: 0x060006D1 RID: 1745 RVA: 0x0001603B File Offset: 0x0001423B
		private void OnStoreStateChanged(IStoreManager storeManager, bool isVisible)
		{
			this.collider2D.enabled = !isVisible;
		}

		// Token: 0x060006D2 RID: 1746 RVA: 0x0001604C File Offset: 0x0001424C
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameItem)
		{
			hasSameItem = false;
			slotBusy = false;
			return 0;
		}

		// Token: 0x060006D3 RID: 1747 RVA: 0x00016055 File Offset: 0x00014255
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.playerUpgradesStore.IsNull())
			{
				this.playerUpgradesStore.StoreStateChanged -= this.OnStoreStateChanged;
			}
		}

		// Token: 0x04000418 RID: 1048
		[SerializeField]
		private Transform cameraPivot;

		// Token: 0x04000419 RID: 1049
		[SerializeField]
		private string metadataKey;

		// Token: 0x0400041A RID: 1050
		private IPlayerUpgradesStoreManager playerUpgradesStore;
	}
}
