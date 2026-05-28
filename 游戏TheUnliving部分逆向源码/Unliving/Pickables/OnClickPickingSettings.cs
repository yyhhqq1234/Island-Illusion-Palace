using System;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x0200019B RID: 411
	public class OnClickPickingSettings : PickingSettingsBase
	{
		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06000BCB RID: 3019 RVA: 0x0002580C File Offset: 0x00023A0C
		public override float CurrentProgress
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x06000BCC RID: 3020 RVA: 0x00025813 File Offset: 0x00023A13
		public override float UpdateCurrentPickingProgress(PlayerInputController.ActionArgs inputArgs)
		{
			return (float)(base.HasPickingAction(inputArgs) ? 1 : 0);
		}

		// Token: 0x06000BCD RID: 3021 RVA: 0x00025823 File Offset: 0x00023A23
		public override void ResetCurrentPickingProgress()
		{
		}
	}
}
