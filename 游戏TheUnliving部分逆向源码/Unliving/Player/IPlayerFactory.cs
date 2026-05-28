using System;
using Common.Factories;

namespace Unliving.Player
{
	// Token: 0x0200013E RID: 318
	public interface IPlayerFactory : IObjectFactory<PlayerBehaviour>, IFactory<IBaseObjectDescription, PlayerBehaviour>, IFactory
	{
		// Token: 0x06000800 RID: 2048
		PlayerBehaviour Create(PlayerFactoryArgs args);
	}
}
