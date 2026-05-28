using System;
using Rewired.Interfaces;

namespace Unliving.InputManager
{
	// Token: 0x020002A0 RID: 672
	public interface IResetableControllerMapStore : IControllerMapStore
	{
		// Token: 0x06001749 RID: 5961
		void RestoreDefaults();
	}
}
