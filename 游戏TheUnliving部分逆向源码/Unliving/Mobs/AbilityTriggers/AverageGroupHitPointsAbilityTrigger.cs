using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000227 RID: 551
	[CreateAssetMenu(fileName = "AverageGroupHitPointsAbilityTrigger", menuName = "Abilities/Triggers/Group Hit Points Trigger")]
	public sealed class AverageGroupHitPointsAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x170003F8 RID: 1016
		// (get) Token: 0x060012E2 RID: 4834 RVA: 0x0003BF52 File Offset: 0x0003A152
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003F9 RID: 1017
		// (get) Token: 0x060012E3 RID: 4835 RVA: 0x0003BF55 File Offset: 0x0003A155
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170003FA RID: 1018
		// (get) Token: 0x060012E4 RID: 4836 RVA: 0x0003BF5C File Offset: 0x0003A15C
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003FB RID: 1019
		// (get) Token: 0x060012E5 RID: 4837 RVA: 0x0003BF5F File Offset: 0x0003A15F
		// (set) Token: 0x060012E6 RID: 4838 RVA: 0x0003BF67 File Offset: 0x0003A167
		public float NormalizedHitPointsThreshold
		{
			get
			{
				return this._normalizedHitPointsThreshold;
			}
			set
			{
				this._normalizedHitPointsThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x060012E7 RID: 4839 RVA: 0x0003BF78 File Offset: 0x0003A178
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			if (baseGameMob != null)
			{
				GameMobGroupController gameMobGroupController = baseGameMob.Group as GameMobGroupController;
				if (gameMobGroupController != null && gameMobGroupController.HasMobs)
				{
					float currentGroupCharactersHitPointsSum = gameMobGroupController.CurrentGroupCharactersHitPointsSum;
					float num = 0f;
					AverageGroupHitPointsAbilityTrigger.Type type = this.type;
					if (type != AverageGroupHitPointsAbilityTrigger.Type.AverageHitPointsBySpawnedCount)
					{
						if (type != AverageGroupHitPointsAbilityTrigger.Type.CurrentAverageHitPoints)
						{
							return false;
						}
						num = gameMobGroupController.MaxGroupCharactersHitPointsSum;
					}
					else
					{
						MobBehaviourSpawner mobBehaviourSpawner = gameMobGroupController.GroupMobsSpawner as MobBehaviourSpawner;
						if (mobBehaviourSpawner != null)
						{
							num = mobBehaviourSpawner.MaxGroupHitPointsSum;
						}
					}
					if (!this.forceCountAbilityOwner && !ability.CanBeUsedOnOwner)
					{
						GameMobGroupController.ExcludeFromHitPointsSums(baseGameMob, ref currentGroupCharactersHitPointsSum, ref num);
					}
					float num2 = (num > 0f) ? (currentGroupCharactersHitPointsSum / num) : 0f;
					if (!this.triggerIfHitPointsIsGreater)
					{
						return num2 < this._normalizedHitPointsThreshold;
					}
					return num2 > this._normalizedHitPointsThreshold;
				}
			}
			return false;
		}

		// Token: 0x04000B14 RID: 2836
		public AverageGroupHitPointsAbilityTrigger.Type type;

		// Token: 0x04000B15 RID: 2837
		[SerializeField]
		[Range(0f, 1f)]
		private float _normalizedHitPointsThreshold = 0.5f;

		// Token: 0x04000B16 RID: 2838
		public bool triggerIfHitPointsIsGreater;

		// Token: 0x04000B17 RID: 2839
		public bool forceCountAbilityOwner;

		// Token: 0x020004C6 RID: 1222
		public enum Type
		{
			// Token: 0x040019B4 RID: 6580
			AverageHitPointsBySpawnedCount,
			// Token: 0x040019B5 RID: 6581
			CurrentAverageHitPoints
		}
	}
}
