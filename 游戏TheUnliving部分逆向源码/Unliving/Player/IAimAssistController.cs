using System;

namespace Unliving.Player
{
	// Token: 0x0200013B RID: 315
	public interface IAimAssistController
	{
		// Token: 0x1700013F RID: 319
		// (get) Token: 0x060007F3 RID: 2035
		IAimAssistMode CurrentMode { get; }

		// Token: 0x14000049 RID: 73
		// (add) Token: 0x060007F4 RID: 2036
		// (remove) Token: 0x060007F5 RID: 2037
		event Action AimAssistModeChanged;

		// Token: 0x060007F6 RID: 2038
		void OnLateUpdate();
	}
}
