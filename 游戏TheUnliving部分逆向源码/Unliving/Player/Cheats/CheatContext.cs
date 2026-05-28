using System;
using Unliving.LevelGeneration;
using Unliving.PlayerProfileManagement;

namespace Unliving.Player.Cheats
{
	// Token: 0x02000179 RID: 377
	public sealed class CheatContext
	{
		// Token: 0x06000A72 RID: 2674 RVA: 0x0002279B File Offset: 0x0002099B
		public void Reset()
		{
			this.playerProfile = null;
			this.currentPlayer = null;
			this.isHomespace = false;
			this.isTutorial = false;
			this.isNewGame = false;
			this.locationID = GameLocation.TypeID.Undefined;
		}

		// Token: 0x04000613 RID: 1555
		public PlayerProfile playerProfile;

		// Token: 0x04000614 RID: 1556
		public PlayerBehaviour currentPlayer;

		// Token: 0x04000615 RID: 1557
		public bool isHomespace;

		// Token: 0x04000616 RID: 1558
		public bool isTutorial;

		// Token: 0x04000617 RID: 1559
		public bool isNewGame;

		// Token: 0x04000618 RID: 1560
		public GameLocation.TypeID locationID;
	}
}
