using System;
using Common;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x0200019A RID: 410
	public interface IPickingSettings : ICloneable<IPickingSettings>
	{
		// Token: 0x17000210 RID: 528
		// (get) Token: 0x06000BC8 RID: 3016
		float CurrentProgress { get; }

		// Token: 0x06000BC9 RID: 3017
		float UpdateCurrentPickingProgress(PlayerInputController.ActionArgs inputArgs);

		// Token: 0x06000BCA RID: 3018
		void ResetCurrentPickingProgress();
	}
}
