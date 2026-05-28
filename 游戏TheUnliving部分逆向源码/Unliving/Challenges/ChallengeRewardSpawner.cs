using System;
using Game.Core;
using Unliving.LevelGeneration;
using Unliving.Player;
using Unliving.Player.TemporaryItemsStorage;

namespace Unliving.Challenges
{
	// Token: 0x02000351 RID: 849
	public class ChallengeRewardSpawner : GameBehaviourBase
	{
		// Token: 0x06001B7C RID: 7036 RVA: 0x00056754 File Offset: 0x00054954
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				TemporaryItemsStorageController temporaryItemsStorageController = (currentPlayer != null) ? currentPlayer.TempItemsStorageController : null;
				if (temporaryItemsStorageController != null)
				{
					this.reward.CreateReward(temporaryItemsStorageController, this.buffValidChunckTypes);
				}
			}
		}

		// Token: 0x04000F76 RID: 3958
		public ChallengeRewardInfo reward;

		// Token: 0x04000F77 RID: 3959
		public LocationChunk.TypeID[] buffValidChunckTypes = new LocationChunk.TypeID[]
		{
			LocationChunk.TypeID.BattleChunk,
			LocationChunk.TypeID.BossChunk
		};
	}
}
