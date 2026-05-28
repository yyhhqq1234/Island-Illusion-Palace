using System;
using Common.CollectionsExtensions;
using Game.Buffs;
using UnityEngine.Events;
using Unliving.LevelGeneration;
using Unliving.Player.TemporaryItemsStorage;

namespace Unliving.Challenges
{
	// Token: 0x0200034F RID: 847
	[Serializable]
	public class ChallengeRewardInfo
	{
		// Token: 0x06001B77 RID: 7031 RVA: 0x00056640 File Offset: 0x00054840
		public void CreateReward(TemporaryItemsStorageController storageController, LocationChunk.TypeID[] validChunckTypes)
		{
			ChallengeReward challengeReward;
			if (!this.rewards.GetRandomWeightedItem(out challengeReward, 0, 2147483647, null))
			{
				return;
			}
			UnityEvent customEvents = challengeReward.customEvents;
			if (customEvents != null)
			{
				customEvents.Invoke();
			}
			if (challengeReward.dropSpawner != null)
			{
				challengeReward.dropSpawner.SpawnPickable(null);
			}
			if (challengeReward.essenceSpawner != null)
			{
				challengeReward.essenceSpawner.Spawn();
			}
			if (challengeReward.rewardBuffs == null || challengeReward.rewardBuffs.Length == 0)
			{
				return;
			}
			BuffsGeneratorBuilderAsset.ReferenceBase[] rewardBuffs = challengeReward.rewardBuffs;
			IBuffsGenerator[] array;
			rewardBuffs.Instantiate(out array);
			IBuffsGenerator[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				IBuff content = array2[i].GenerateBuffForTempStorage(storageController);
				if (challengeReward.chunksCountDuration > 0)
				{
					storageController.AddItem(new EnemyChunksCountingTempItem(content, validChunckTypes, challengeReward.chunksCountDuration));
				}
				else if (challengeReward.activatedMobsCountDuration > 0)
				{
					storageController.AddItem(new ActivatedMobsCountingTemporaryItem(content, challengeReward.activatedMobsCountDuration));
				}
			}
		}

		// Token: 0x04000F6E RID: 3950
		public ChallengeReward[] rewards;
	}
}
