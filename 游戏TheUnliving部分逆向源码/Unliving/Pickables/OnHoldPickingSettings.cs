using System;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x0200019C RID: 412
	public class OnHoldPickingSettings : PickingSettingsBase
	{
		// Token: 0x17000212 RID: 530
		// (get) Token: 0x06000BCF RID: 3023 RVA: 0x0002582D File Offset: 0x00023A2D
		public override float CurrentProgress
		{
			get
			{
				return this.currentProgress;
			}
		}

		// Token: 0x06000BD0 RID: 3024 RVA: 0x00025838 File Offset: 0x00023A38
		public override float UpdateCurrentPickingProgress(PlayerInputController.ActionArgs inputArgs)
		{
			if (base.HasPickingInterruptAction(inputArgs))
			{
				this.currentProgress = 0f;
			}
			else if (base.HasPickingAction(inputArgs))
			{
				this.currentProgress = Mathf.Clamp01(this.currentProgress + Time.deltaTime);
			}
			else
			{
				this.currentProgress = 0f;
			}
			return this.currentProgress;
		}

		// Token: 0x06000BD1 RID: 3025 RVA: 0x0002588E File Offset: 0x00023A8E
		public override void ResetCurrentPickingProgress()
		{
			this.currentProgress = 0f;
		}

		// Token: 0x04000696 RID: 1686
		public float holdTime = 2f;

		// Token: 0x04000697 RID: 1687
		private float currentProgress;
	}
}
