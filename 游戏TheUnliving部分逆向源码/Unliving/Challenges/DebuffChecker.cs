using System;
using Game.Buffs;
using Game.Core;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Challenges
{
	// Token: 0x02000352 RID: 850
	public class DebuffChecker : GameBehaviourBase
	{
		// Token: 0x14000107 RID: 263
		// (add) Token: 0x06001B7E RID: 7038 RVA: 0x000567BC File Offset: 0x000549BC
		// (remove) Token: 0x06001B7F RID: 7039 RVA: 0x000567F4 File Offset: 0x000549F4
		public event Action RewardSpawned;

		// Token: 0x06001B80 RID: 7040 RVA: 0x0005682C File Offset: 0x00054A2C
		private void Start()
		{
			IPlayerProvider playerProvider = base.CurrentGame.Services.Get<IPlayerProvider>();
			this.currentPlayer = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
			this.mob = base.GetComponent<StaticGameMob>();
			this.buffsController = this.mob.BuffsController;
			this.buffsController.BuffActivated += this.OnBuffsControllerBuffActivated;
		}

		// Token: 0x06001B81 RID: 7041 RVA: 0x00056890 File Offset: 0x00054A90
		private void OnBuffsControllerBuffActivated(IBuff buff)
		{
			if (this.isRewardSpawned)
			{
				return;
			}
			if (!this.buffsGeneratorAsset.IsRelatedBuff(buff))
			{
				return;
			}
			this.isRewardSpawned = true;
			this.reward.CreateReward(this.currentPlayer.TempItemsStorageController, this.validChunckTypes);
			Action rewardSpawned = this.RewardSpawned;
			if (rewardSpawned == null)
			{
				return;
			}
			rewardSpawned();
		}

		// Token: 0x06001B82 RID: 7042 RVA: 0x000568E8 File Offset: 0x00054AE8
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.buffsController.BuffActivated -= this.OnBuffsControllerBuffActivated;
		}

		// Token: 0x04000F79 RID: 3961
		public ChallengeRewardInfo reward;

		// Token: 0x04000F7A RID: 3962
		public BuffsGeneratorBuilderAsset buffsGeneratorAsset;

		// Token: 0x04000F7B RID: 3963
		private StaticGameMob mob;

		// Token: 0x04000F7C RID: 3964
		private IBuffsController buffsController;

		// Token: 0x04000F7D RID: 3965
		private bool isRewardSpawned;

		// Token: 0x04000F7E RID: 3966
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000F7F RID: 3967
		private readonly LocationChunk.TypeID[] validChunckTypes = new LocationChunk.TypeID[]
		{
			LocationChunk.TypeID.BattleChunk,
			LocationChunk.TypeID.BossChunk
		};
	}
}
