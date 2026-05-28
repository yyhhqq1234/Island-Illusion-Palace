using System;
using Game.Core;

namespace Unliving.Player
{
	// Token: 0x0200015D RID: 349
	public abstract class PlayerMobsActivationControllerBase<TData> : PlayerMobsActivationControllerBase where TData : struct
	{
		// Token: 0x060009B2 RID: 2482 RVA: 0x0002104D File Offset: 0x0001F24D
		public PlayerMobsActivationControllerBase(PlayerBehaviour targetMob) : base(targetMob)
		{
			this.currentGame.BindDataDirectly(ref this.data);
		}

		// Token: 0x040005AF RID: 1455
		protected TData data;
	}
}
