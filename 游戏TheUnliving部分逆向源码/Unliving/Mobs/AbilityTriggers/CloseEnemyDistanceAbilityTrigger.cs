using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000229 RID: 553
	[CreateAssetMenu(fileName = "CloseEnemyDistanceAbilityTrigger", menuName = "Abilities/Triggers/Close Enemy Distance Trigger")]
	public sealed class CloseEnemyDistanceAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x170003FF RID: 1023
		// (get) Token: 0x060012EE RID: 4846 RVA: 0x0003C0B9 File Offset: 0x0003A2B9
		public override bool RequiresTarget
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000400 RID: 1024
		// (get) Token: 0x060012EF RID: 4847 RVA: 0x0003C0BC File Offset: 0x0003A2BC
		public override float ActivationRange
		{
			get
			{
				return this._distanceThreshold;
			}
		}

		// Token: 0x17000401 RID: 1025
		// (get) Token: 0x060012F0 RID: 4848 RVA: 0x0003C0C4 File Offset: 0x0003A2C4
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000402 RID: 1026
		// (get) Token: 0x060012F1 RID: 4849 RVA: 0x0003C0C7 File Offset: 0x0003A2C7
		// (set) Token: 0x060012F2 RID: 4850 RVA: 0x0003C0CF File Offset: 0x0003A2CF
		public float DistanceThreshold
		{
			get
			{
				return this._distanceThreshold;
			}
			set
			{
				this._distanceThreshold = value;
			}
		}

		// Token: 0x060012F3 RID: 4851 RVA: 0x0003C0D8 File Offset: 0x0003A2D8
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			MobAbilityTriggerArgs mobAbilityTriggerArgs;
			return abilityUsingArgs.TryGetAdditionalContext(out mobAbilityTriggerArgs) && mobAbilityTriggerArgs.targetDistance < this._distanceThreshold;
		}

		// Token: 0x04000B19 RID: 2841
		[SerializeField]
		[Tooltip("Триггер будет активирован, если целью для атаки будет достигнуто расстояние меньше этого значения.")]
		private float _distanceThreshold = 0.3f;
	}
}
