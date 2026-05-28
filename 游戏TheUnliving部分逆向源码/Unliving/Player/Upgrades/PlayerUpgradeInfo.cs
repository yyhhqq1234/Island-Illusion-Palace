using System;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000168 RID: 360
	[Serializable]
	public struct PlayerUpgradeInfo
	{
		// Token: 0x040005E4 RID: 1508
		public static readonly PlayerUpgradeInfo None = new PlayerUpgradeInfo
		{
			upgradeID = PlayerUpgradeID.None,
			upgradeLevel = 0
		};

		// Token: 0x040005E5 RID: 1509
		public PlayerUpgradeID upgradeID;

		// Token: 0x040005E6 RID: 1510
		public int upgradeLevel;
	}
}
