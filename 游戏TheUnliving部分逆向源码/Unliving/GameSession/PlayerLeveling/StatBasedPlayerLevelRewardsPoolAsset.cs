using System;
using System.Collections.Generic;
using UnityEngine;
using Unliving.DataParsing;
using Unliving.MobsStats;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BF RID: 703
	[CreateAssetMenu(fileName = "StatBasedPlayerLevelRewardsPool", menuName = "Player Level Rewards/Stat Based RewardsPool")]
	public sealed class StatBasedPlayerLevelRewardsPoolAsset : PlayerLevelRewardsPoolAssetBase
	{
		// Token: 0x17000533 RID: 1331
		// (get) Token: 0x06001861 RID: 6241 RVA: 0x0004C761 File Offset: 0x0004A961
		public override PlayerLevelRewardsPoolBase RewardPool
		{
			get
			{
				return this.rewardPool;
			}
		}

		// Token: 0x06001862 RID: 6242 RVA: 0x0004C76C File Offset: 0x0004A96C
		protected override IPlayerLevelReward[] ParseTable(List<List<string>> table)
		{
			StatBasedPlayerLevelRewardsPoolAsset.rewardBuffer.Clear();
			for (int i = 1; i < table.Count; i++)
			{
				List<string> list = table[i];
				int num = 0;
				TargetedMobStatModifier targetedMobStatModifier;
				ParsingUtility.TryParseMobStatModifier(list[num++], list[num++], out targetedMobStatModifier, MobStatModifierType.BaseModifier);
				float num2;
				ParsingUtility.TryParseFloat(list[num++], out num2);
				float num3;
				ParsingUtility.TryParseFloat(list[num++], out num3);
				if (num3 > 0f && targetedMobStatModifier.targetStat != MobStatID.Undefined)
				{
					StatBasedPlayerLevelRewardsPoolAsset.rewardBuffer.Add(new StatBasedPlayerLevelReward((int)num2, num3, new TargetedMobStatModifier[]
					{
						targetedMobStatModifier
					}));
				}
			}
			return StatBasedPlayerLevelRewardsPoolAsset.rewardBuffer.ToArray();
		}

		// Token: 0x06001863 RID: 6243 RVA: 0x0004C82B File Offset: 0x0004AA2B
		protected override void OnTableParsed(IPlayerLevelReward[] data)
		{
			this.rewardPool.Rewards = (StatBasedPlayerLevelReward[])data;
		}

		// Token: 0x06001864 RID: 6244 RVA: 0x0004C83E File Offset: 0x0004AA3E
		[ContextMenu("Force Parse Table")]
		private void ForceParse()
		{
			base.ForceParseTable();
		}

		// Token: 0x04000DB8 RID: 3512
		private static readonly List<StatBasedPlayerLevelReward> rewardBuffer = new List<StatBasedPlayerLevelReward>(32);

		// Token: 0x04000DB9 RID: 3513
		[SerializeField]
		private PlayerLevelRewardsPool<StatBasedPlayerLevelReward> rewardPool;
	}
}
