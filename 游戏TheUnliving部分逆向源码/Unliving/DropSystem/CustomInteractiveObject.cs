using System;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Pickables;

namespace Unliving.DropSystem
{
	// Token: 0x02000284 RID: 644
	public class CustomInteractiveObject : NonFactoryPickableBase
	{
		// Token: 0x170004C9 RID: 1225
		// (get) Token: 0x0600163E RID: 5694 RVA: 0x00047587 File Offset: 0x00045787
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (this.pickableObjectData == null)
				{
					this.pickableObjectData = new PickableObjectData
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, this.pickableObjectDataParams)
					};
				}
				return this.pickableObjectData;
			}
		}

		// Token: 0x170004CA RID: 1226
		// (get) Token: 0x0600163F RID: 5695 RVA: 0x000475BF File Offset: 0x000457BF
		public override NonFactoryPickableType ID
		{
			get
			{
				return this.pickableType;
			}
		}

		// Token: 0x170004CB RID: 1227
		// (get) Token: 0x06001640 RID: 5696 RVA: 0x000475C7 File Offset: 0x000457C7
		protected override string LocalizationID
		{
			get
			{
				return this.metadataKey;
			}
		}

		// Token: 0x06001641 RID: 5697 RVA: 0x000475CF File Offset: 0x000457CF
		public void SetLocalizationData(string id, params string[] args)
		{
			this.metadataKey = id;
			this.pickableObjectDataParams = args;
			this.pickableObjectData = null;
		}

		// Token: 0x06001642 RID: 5698 RVA: 0x000475E6 File Offset: 0x000457E6
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return targetCollector == this.PointerEventsSender;
		}

		// Token: 0x06001643 RID: 5699 RVA: 0x000475F1 File Offset: 0x000457F1
		protected override void TryCollectObject(IPickableObjectCollector targetCollector)
		{
			this.currentCollector = targetCollector;
			this.interactionEvents.Invoke();
		}

		// Token: 0x06001644 RID: 5700 RVA: 0x00047605 File Offset: 0x00045805
		private void OnInteractionAnimationEnds()
		{
			this.animationEndsEvents.Invoke();
		}

		// Token: 0x06001645 RID: 5701 RVA: 0x00047612 File Offset: 0x00045812
		public override void OnAnimationEventFired(string eventArg)
		{
			base.OnAnimationEventFired(eventArg);
			if (eventArg == "collected")
			{
				this.OnObjectCollected();
				return;
			}
			if (eventArg == "destroy")
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x04000CDB RID: 3291
		private const string DestroyAnimationArg = "destroy";

		// Token: 0x04000CDC RID: 3292
		private const string CollectionPerformedAnimationArg = "collected";

		// Token: 0x04000CDD RID: 3293
		public NonFactoryPickableType pickableType;

		// Token: 0x04000CDE RID: 3294
		public string metadataKey = "drop_chest";

		// Token: 0x04000CDF RID: 3295
		public string[] pickableObjectDataParams;

		// Token: 0x04000CE0 RID: 3296
		public UnityEvent interactionEvents;

		// Token: 0x04000CE1 RID: 3297
		public UnityEvent animationEndsEvents;
	}
}
