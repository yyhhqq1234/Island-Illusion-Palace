using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Steam
{
	// Token: 0x02000050 RID: 80
	public class SteamInput : GameBehaviourBase
	{
		// Token: 0x06000290 RID: 656 RVA: 0x0000A148 File Offset: 0x00008348
		public override async void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			await new WaitWhile(delegate()
			{
				this.steamManager = currentGame.Services.Get<SteamManager>();
				return this.steamManager == null;
			});
		}

		// Token: 0x04000181 RID: 385
		private SteamManager steamManager;
	}
}
