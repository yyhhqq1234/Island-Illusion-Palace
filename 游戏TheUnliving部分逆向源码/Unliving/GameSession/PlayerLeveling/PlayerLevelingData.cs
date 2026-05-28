using System;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BD RID: 701
	[Serializable]
	public sealed class PlayerLevelingData
	{
		// Token: 0x06001859 RID: 6233 RVA: 0x0004C671 File Offset: 0x0004A871
		public int GetMaxLevel()
		{
			if (this.levelsInfo == null || this.levelsInfo.Length == 0)
			{
				return 0;
			}
			return this.levelsInfo.Length;
		}

		// Token: 0x0600185A RID: 6234 RVA: 0x0004C68E File Offset: 0x0004A88E
		public bool TryGetNextLevelInfo(int currentLevel, out PlayerLevelInfo nextLevelInfo)
		{
			if (currentLevel > 0 && currentLevel < this.GetMaxLevel() - 1)
			{
				nextLevelInfo = this.levelsInfo[currentLevel];
				return true;
			}
			nextLevelInfo = default(PlayerLevelInfo);
			return false;
		}

		// Token: 0x04000DB6 RID: 3510
		public PlayerLevelInfo[] levelsInfo;
	}
}
