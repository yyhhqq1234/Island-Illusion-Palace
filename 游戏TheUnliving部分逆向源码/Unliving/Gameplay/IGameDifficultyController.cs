using System;
using Unliving.LevelGeneration;
using Unliving.PlayerProfileManagement;

namespace Unliving.Gameplay
{
	// Token: 0x020002AB RID: 683
	public interface IGameDifficultyController
	{
		// Token: 0x1700051C RID: 1308
		// (get) Token: 0x060017F5 RID: 6133
		// (set) Token: 0x060017F6 RID: 6134
		int CurrentDifficultyLevel { get; set; }

		// Token: 0x060017F7 RID: 6135
		void TryTakeDifficultyLevelCompletionReward(PlayerProfile playerProfile, GameLocation.TypeID locationID);
	}
}
