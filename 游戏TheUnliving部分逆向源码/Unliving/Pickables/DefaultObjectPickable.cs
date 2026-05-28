using System;
using Game.Localization;

namespace Unliving.Pickables
{
	// Token: 0x0200018C RID: 396
	public class DefaultObjectPickable : PickableBase
	{
		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06000B29 RID: 2857 RVA: 0x00024751 File Offset: 0x00022951
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

		// Token: 0x170001EF RID: 495
		// (get) Token: 0x06000B2A RID: 2858 RVA: 0x00024788 File Offset: 0x00022988
		protected override string LocalizationID
		{
			get
			{
				return this.localizationID;
			}
		}

		// Token: 0x06000B2B RID: 2859 RVA: 0x00024790 File Offset: 0x00022990
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000B2C RID: 2860 RVA: 0x00024793 File Offset: 0x00022993
		protected virtual void Start()
		{
			this.localizationManager = base.CurrentGame.Services.Get<LocalizationManager>();
		}

		// Token: 0x06000B2D RID: 2861 RVA: 0x000247AB File Offset: 0x000229AB
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
		}

		// Token: 0x04000659 RID: 1625
		public string localizationID;

		// Token: 0x0400065A RID: 1626
		private new PickableObjectData pickableObjectData;

		// Token: 0x0400065B RID: 1627
		private LocalizationManager localizationManager;
	}
}
