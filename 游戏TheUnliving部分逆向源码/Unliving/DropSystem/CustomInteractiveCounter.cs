using System;
using Unliving.Pickables;

namespace Unliving.DropSystem
{
	// Token: 0x02000283 RID: 643
	public class CustomInteractiveCounter : CustomInteractiveObject
	{
		// Token: 0x0600163B RID: 5691 RVA: 0x00047513 File Offset: 0x00045713
		public void IncrementCounter()
		{
			this.pickableObjectData = null;
			this.counter++;
		}

		// Token: 0x170004C8 RID: 1224
		// (get) Token: 0x0600163C RID: 5692 RVA: 0x0004752C File Offset: 0x0004572C
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (this.pickableObjectData == null)
				{
					this.pickableObjectData = new PickableObjectData
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, new string[]
						{
							this.counter.ToString()
						})
					};
				}
				return this.pickableObjectData;
			}
		}

		// Token: 0x04000CDA RID: 3290
		public int counter;
	}
}
