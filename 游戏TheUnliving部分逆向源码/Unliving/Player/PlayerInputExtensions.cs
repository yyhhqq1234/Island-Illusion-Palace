using System;

namespace Unliving.Player
{
	// Token: 0x02000154 RID: 340
	public static class PlayerInputExtensions
	{
		// Token: 0x06000973 RID: 2419 RVA: 0x000203D0 File Offset: 0x0001E5D0
		public static bool Has(this PlayerInputController.InputBehaviour inputBehavioursMask, PlayerInputController.InputBehaviour inputBehaviours)
		{
			return (inputBehavioursMask & inputBehaviours) > PlayerInputController.InputBehaviour.NONE;
		}
	}
}
