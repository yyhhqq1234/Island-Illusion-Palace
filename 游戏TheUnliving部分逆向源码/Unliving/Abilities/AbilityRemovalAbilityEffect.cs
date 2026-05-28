using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x02000383 RID: 899
	[Serializable]
	public sealed class AbilityRemovalAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001DAE RID: 7598 RVA: 0x0005E3AB File Offset: 0x0005C5AB
		public AbilityRemovalAbilityEffect()
		{
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x0005E3B3 File Offset: 0x0005C5B3
		public AbilityRemovalAbilityEffect(AbilityRemovalAbilityEffect effectPrototype)
		{
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x0005E3C2 File Offset: 0x0005C5C2
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DB1 RID: 7601 RVA: 0x0005E3C9 File Offset: 0x0005C5C9
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DB2 RID: 7602 RVA: 0x0005E3CB File Offset: 0x0005C5CB
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.isActivated)
			{
				return false;
			}
			if (base.CurrentAbility != null)
			{
				base.CurrentAbility.Completed += this.OnAbilityCompleted;
			}
			this.isActivated = true;
			return true;
		}

		// Token: 0x06001DB3 RID: 7603 RVA: 0x0005E404 File Offset: 0x0005C604
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new AbilityRemovalAbilityEffect(this);
		}

		// Token: 0x06001DB4 RID: 7604 RVA: 0x0005E40C File Offset: 0x0005C60C
		private void OnAbilityCompleted(IAbility ability, object args)
		{
			ability.Completed -= this.OnAbilityCompleted;
			BaseAbility baseAbility = ability as BaseAbility;
			if (baseAbility != null && baseAbility.CurrentController != null)
			{
				baseAbility.CurrentController.RemoveAbility(ability);
				return;
			}
			ability.Destroy();
		}

		// Token: 0x040010C9 RID: 4297
		private bool isActivated;
	}
}
