using System;
using Common.Editor;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.PlayerProfileManagement
{
	// Token: 0x02000121 RID: 289
	[Serializable]
	public struct PlayerProfileGameDifficultyRewardState
	{
		// Token: 0x1700012A RID: 298
		// (get) Token: 0x06000726 RID: 1830 RVA: 0x0001701C File Offset: 0x0001521C
		public GameLocation.TypeID CompletedLocationID
		{
			get
			{
				return this.completedLocationID;
			}
		}

		// Token: 0x06000727 RID: 1831 RVA: 0x00017024 File Offset: 0x00015224
		public PlayerProfileGameDifficultyRewardState(int difficultyLevel, GameLocation.TypeID completedLocationID)
		{
			this = default(PlayerProfileGameDifficultyRewardState);
			this.DifficultyLevel = difficultyLevel;
			this.completedLocationID = completedLocationID;
		}

		// Token: 0x0400044A RID: 1098
		public readonly int DifficultyLevel;

		// Token: 0x0400044B RID: 1099
		[SerializeField]
		[EnumPopup]
		private GameLocation.TypeID completedLocationID;

		// Token: 0x0400044C RID: 1100
		public bool isRewardTaken;
	}
}
