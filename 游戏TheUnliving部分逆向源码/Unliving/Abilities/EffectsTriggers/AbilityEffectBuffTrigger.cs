using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Abilities.EffectsTriggers
{
	// Token: 0x020003D0 RID: 976
	[CreateAssetMenu(fileName = "AbilityEffectBuffTrigger", menuName = "Abilities/Effects Triggers/Buff Trigger")]
	public sealed class AbilityEffectBuffTrigger : AbilityEffectTriggerBase
	{
		// Token: 0x170006B1 RID: 1713
		// (get) Token: 0x0600211C RID: 8476 RVA: 0x00067F76 File Offset: 0x00066176
		public override bool IsInverted
		{
			get
			{
				return this.triggerIfBuffIsNotPresent;
			}
		}

		// Token: 0x0600211D RID: 8477 RVA: 0x00067F80 File Offset: 0x00066180
		public override bool IsFired(AbilityEffectBase effect, Component effectTarget)
		{
			if (this.expectedBuffsGenerator != null)
			{
				IBuffableObject buffableObject = ((this.buffLocation == AbilityEffectBuffTrigger.BuffLocation.EffectOwner) ? effect.GetEffectOwner() : effectTarget).CastOrGetComponent<IBuffableObject>();
				IBuffsController buffsController = (buffableObject != null) ? buffableObject.BuffsController : null;
				if (buffsController != null)
				{
					return buffsController.HasBuff(this.expectedBuffsGenerator.GetBuffsID());
				}
			}
			return false;
		}

		// Token: 0x040014B2 RID: 5298
		public AbilityEffectBuffTrigger.BuffLocation buffLocation = AbilityEffectBuffTrigger.BuffLocation.EffectTarget;

		// Token: 0x040014B3 RID: 5299
		public BuffsGeneratorBuilderAsset expectedBuffsGenerator;

		// Token: 0x040014B4 RID: 5300
		public bool triggerIfBuffIsNotPresent;

		// Token: 0x0200058F RID: 1423
		public enum BuffLocation
		{
			// Token: 0x04001CD5 RID: 7381
			EffectOwner,
			// Token: 0x04001CD6 RID: 7382
			EffectTarget
		}
	}
}
