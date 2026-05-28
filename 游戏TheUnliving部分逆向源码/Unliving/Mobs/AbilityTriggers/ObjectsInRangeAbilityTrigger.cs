using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000236 RID: 566
	[CreateAssetMenu(fileName = "ObjectsInRangeAbilityTrigger", menuName = "Abilities/Triggers/Objects In Range Trigger")]
	public sealed class ObjectsInRangeAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x0600135E RID: 4958 RVA: 0x0003CBAD File Offset: 0x0003ADAD
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x0600135F RID: 4959 RVA: 0x0003CBB0 File Offset: 0x0003ADB0
		public override float ActivationRange
		{
			get
			{
				return this.activationRange;
			}
		}

		// Token: 0x1700042A RID: 1066
		// (get) Token: 0x06001360 RID: 4960 RVA: 0x0003CBB8 File Offset: 0x0003ADB8
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001361 RID: 4961 RVA: 0x0003CBBC File Offset: 0x0003ADBC
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			bool flag = Physics2D.OverlapCircle(this.checkConditionAtUsingPoint ? abilityUsingArgs.targetPosition : ability.OwnerPosition, this.activationRange, this.activationLayers);
			if (this.triggerIfThereAreNoObjects)
			{
				flag = !flag;
			}
			return flag;
		}

		// Token: 0x04000B3B RID: 2875
		public LayerMask activationLayers;

		// Token: 0x04000B3C RID: 2876
		public float activationRange = 1f;

		// Token: 0x04000B3D RID: 2877
		public bool triggerIfThereAreNoObjects;

		// Token: 0x04000B3E RID: 2878
		public bool checkConditionAtUsingPoint;
	}
}
