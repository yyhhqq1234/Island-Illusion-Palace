using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities.EffectsTriggers
{
	// Token: 0x020003D1 RID: 977
	[CreateAssetMenu(fileName = "AbilityEffectDeadTargetTrigger", menuName = "Abilities/Effects Triggers/Dead Target Trigger")]
	public sealed class AbilityEffectDeadTargetTrigger : AbilityEffectTriggerBase
	{
		// Token: 0x170006B2 RID: 1714
		// (get) Token: 0x0600211F RID: 8479 RVA: 0x00067FE3 File Offset: 0x000661E3
		public override bool IsInverted
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06002120 RID: 8480 RVA: 0x00067FE8 File Offset: 0x000661E8
		public override bool IsFired(AbilityEffectBase effect, Component effectTarget)
		{
			if (this.triggerOnce && effect.WasUsed())
			{
				return false;
			}
			BaseGameMob baseGameMob;
			if (this.target == AbilityEffectDeadTargetTrigger.Target.EffectSender)
			{
				if (effect.WasUsed())
				{
					return true;
				}
				BaseAbility currentAbility = effect.CurrentAbility;
				baseGameMob = ((currentAbility != null) ? currentAbility.AbilityEffectSender.CastOrGetComponent<BaseGameMob>() : null);
			}
			else
			{
				baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			}
			if (baseGameMob == null)
			{
				return false;
			}
			if (this.targetState != AbilityEffectDeadTargetTrigger.TargetState.Sacrificed)
			{
				return baseGameMob.IsKilled;
			}
			return baseGameMob.IsSacrificed;
		}

		// Token: 0x040014B5 RID: 5301
		public AbilityEffectDeadTargetTrigger.Target target;

		// Token: 0x040014B6 RID: 5302
		public AbilityEffectDeadTargetTrigger.TargetState targetState;

		// Token: 0x040014B7 RID: 5303
		public bool triggerOnce = true;

		// Token: 0x02000590 RID: 1424
		public enum Target
		{
			// Token: 0x04001CD8 RID: 7384
			EffectSender,
			// Token: 0x04001CD9 RID: 7385
			EffectTarget
		}

		// Token: 0x02000591 RID: 1425
		public enum TargetState
		{
			// Token: 0x04001CDB RID: 7387
			Killed,
			// Token: 0x04001CDC RID: 7388
			Sacrificed
		}
	}
}
