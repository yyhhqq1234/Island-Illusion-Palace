using System;
using Game.Core;
using Unliving.PlayerProfileManagement;

namespace Unliving.Purchasing
{
	// Token: 0x02000112 RID: 274
	public class Context
	{
		// Token: 0x0400040D RID: 1037
		public IGame game;

		// Token: 0x0400040E RID: 1038
		public PurchaseManager purchaseManager;

		// Token: 0x0400040F RID: 1039
		public PlayerProfileManager playerProfileManager;

		// Token: 0x04000410 RID: 1040
		public string requester;
	}
}
