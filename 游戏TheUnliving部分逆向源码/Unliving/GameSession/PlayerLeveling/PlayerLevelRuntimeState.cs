using System;
using System.Collections.Generic;
using Common;
using Game.Stats;
using UnityEngine;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.GameSession.PlayerLeveling
{
	// Token: 0x020002BC RID: 700
	[Serializable]
	public sealed class PlayerLevelRuntimeState : ICloneable<PlayerLevelRuntimeState>
	{
		// Token: 0x17000531 RID: 1329
		// (get) Token: 0x06001851 RID: 6225 RVA: 0x0004C453 File Offset: 0x0004A653
		public IReadOnlyList<PlayerLevelRuntimeState.StoredStatModifiers> CurrentStoredStatModifiers
		{
			get
			{
				return this.storedStatModificationRewards;
			}
		}

		// Token: 0x06001852 RID: 6226 RVA: 0x0004C45B File Offset: 0x0004A65B
		public PlayerLevelRuntimeState(PlayerLevelRuntimeState otherState)
		{
			this.currentLevel = otherState.currentLevel;
			this.currentEXP = otherState.currentEXP;
			if (otherState.storedStatModificationRewards != null)
			{
				this.storedStatModificationRewards = new List<PlayerLevelRuntimeState.StoredStatModifiers>(otherState.storedStatModificationRewards);
			}
		}

		// Token: 0x06001853 RID: 6227 RVA: 0x0004C494 File Offset: 0x0004A694
		public PlayerLevelRuntimeState()
		{
			this.Reset();
		}

		// Token: 0x06001854 RID: 6228 RVA: 0x0004C4A4 File Offset: 0x0004A6A4
		public bool TryGetStatModifiers(MobStatID statID, out MobStatModifier modifiersSum)
		{
			for (int i = 0; i < this.storedStatModificationRewards.Count; i++)
			{
				if (this.storedStatModificationRewards[i].statID == (int)statID)
				{
					modifiersSum = this.storedStatModificationRewards[i].modifiersSum;
					return true;
				}
			}
			modifiersSum = default(MobStatModifier);
			return false;
		}

		// Token: 0x06001855 RID: 6229 RVA: 0x0004C500 File Offset: 0x0004A700
		public void StoreReward(IPlayerLevelReward reward)
		{
			IStatBasedPlayerLevelReward statBasedPlayerLevelReward = reward as IStatBasedPlayerLevelReward;
			if (statBasedPlayerLevelReward != null)
			{
				if (this.storedStatModificationRewards == null)
				{
					this.storedStatModificationRewards = new List<PlayerLevelRuntimeState.StoredStatModifiers>(16);
				}
				IReadOnlyList<TargetedMobStatModifier> statModifiers = statBasedPlayerLevelReward.StatModifiers;
				for (int i = 0; i < statModifiers.Count; i++)
				{
					TargetedMobStatModifier targetedMobStatModifier = statModifiers[i];
					MobStatModifier mobStatModifier = targetedMobStatModifier.ToStatModifier();
					if (!(mobStatModifier == default(MobStatModifier)))
					{
						int num = -1;
						for (int j = 0; j < this.storedStatModificationRewards.Count; j++)
						{
							if (this.storedStatModificationRewards[i].statID == targetedMobStatModifier.TargetStatID)
							{
								num = j;
								break;
							}
						}
						if (num != -1)
						{
							PlayerLevelRuntimeState.StoredStatModifiers value = this.storedStatModificationRewards[num];
							value.AddModifier(mobStatModifier);
							this.storedStatModificationRewards[num] = value;
						}
						else
						{
							this.storedStatModificationRewards.Add(new PlayerLevelRuntimeState.StoredStatModifiers(targetedMobStatModifier.TargetStatID, mobStatModifier));
						}
					}
				}
			}
		}

		// Token: 0x06001856 RID: 6230 RVA: 0x0004C5F8 File Offset: 0x0004A7F8
		public void RestoreRewards(PlayerBehaviour player)
		{
			if (this.storedStatModificationRewards != null)
			{
				StatsControllerBase<MobStatModifier> statsController = player.StatsController;
				for (int i = 0; i < this.storedStatModificationRewards.Count; i++)
				{
					PlayerLevelRuntimeState.StoredStatModifiers storedStatModifiers = this.storedStatModificationRewards[i];
					statsController.AddModifier(storedStatModifiers.statID, storedStatModifiers.modifiersSum);
				}
			}
		}

		// Token: 0x06001857 RID: 6231 RVA: 0x0004C649 File Offset: 0x0004A849
		public void Reset()
		{
			this.currentLevel = 1;
			this.currentEXP = 0;
			List<PlayerLevelRuntimeState.StoredStatModifiers> list = this.storedStatModificationRewards;
			if (list == null)
			{
				return;
			}
			list.Clear();
		}

		// Token: 0x06001858 RID: 6232 RVA: 0x0004C669 File Offset: 0x0004A869
		public PlayerLevelRuntimeState Clone()
		{
			return new PlayerLevelRuntimeState(this);
		}

		// Token: 0x04000DB3 RID: 3507
		public int currentLevel;

		// Token: 0x04000DB4 RID: 3508
		public int currentEXP;

		// Token: 0x04000DB5 RID: 3509
		[SerializeField]
		[HideInInspector]
		private List<PlayerLevelRuntimeState.StoredStatModifiers> storedStatModificationRewards;

		// Token: 0x02000525 RID: 1317
		[Serializable]
		public struct StoredStatModifiers
		{
			// Token: 0x06002646 RID: 9798 RVA: 0x00077E5E File Offset: 0x0007605E
			public StoredStatModifiers(int statID, MobStatModifier modifiersSum)
			{
				this.statID = statID;
				this.modifiersSum = modifiersSum;
			}

			// Token: 0x06002647 RID: 9799 RVA: 0x00077E6E File Offset: 0x0007606E
			public void AddModifier(MobStatModifier modifier)
			{
				this.modifiersSum.Combine(modifier);
			}

			// Token: 0x04001B40 RID: 6976
			public int statID;

			// Token: 0x04001B41 RID: 6977
			public MobStatModifier modifiersSum;
		}
	}
}
