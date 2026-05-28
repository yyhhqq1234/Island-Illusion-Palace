using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022C RID: 556
	[CreateAssetMenu(fileName = "FarEnemyDistanceAbilityTrigger", menuName = "Abilities/Triggers/Far Enemy Distance Trigger")]
	public sealed class FarEnemyDistanceAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x1700040A RID: 1034
		// (get) Token: 0x06001310 RID: 4880 RVA: 0x0003C4D2 File Offset: 0x0003A6D2
		public override bool RequiresTarget
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700040B RID: 1035
		// (get) Token: 0x06001311 RID: 4881 RVA: 0x0003C4D5 File Offset: 0x0003A6D5
		public override float ActivationRange
		{
			get
			{
				return this._minDistance;
			}
		}

		// Token: 0x1700040C RID: 1036
		// (get) Token: 0x06001312 RID: 4882 RVA: 0x0003C4DD File Offset: 0x0003A6DD
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700040D RID: 1037
		// (get) Token: 0x06001313 RID: 4883 RVA: 0x0003C4E0 File Offset: 0x0003A6E0
		// (set) Token: 0x06001314 RID: 4884 RVA: 0x0003C4E8 File Offset: 0x0003A6E8
		public float MinDistance
		{
			get
			{
				return this._minDistance;
			}
			set
			{
				this._minDistance = value;
			}
		}

		// Token: 0x06001315 RID: 4885 RVA: 0x0003C4F4 File Offset: 0x0003A6F4
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			MobAbilityTriggerArgs mobAbilityTriggerArgs;
			return abilityUsingArgs.TryGetAdditionalContext(out mobAbilityTriggerArgs) && ((this.canBeUsedWithoutTarget && mobAbilityTriggerArgs.target == null) || mobAbilityTriggerArgs.targetDistance > this._minDistance);
		}

		// Token: 0x04000B24 RID: 2852
		[SerializeField]
		private float _minDistance = 1f;

		// Token: 0x04000B25 RID: 2853
		public bool canBeUsedWithoutTarget;
	}
}
