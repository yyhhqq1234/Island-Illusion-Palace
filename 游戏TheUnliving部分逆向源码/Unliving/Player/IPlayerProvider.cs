using System;

namespace Unliving.Player
{
	// Token: 0x02000147 RID: 327
	public interface IPlayerProvider
	{
		// Token: 0x17000168 RID: 360
		// (get) Token: 0x06000883 RID: 2179
		PlayerBehaviour CurrentPlayer { get; }

		// Token: 0x14000050 RID: 80
		// (add) Token: 0x06000884 RID: 2180
		// (remove) Token: 0x06000885 RID: 2181
		event Action<PlayerBehaviour> PlayerRegistered;

		// Token: 0x06000886 RID: 2182
		void RegisterPlayer(PlayerBehaviour player);
	}
}
