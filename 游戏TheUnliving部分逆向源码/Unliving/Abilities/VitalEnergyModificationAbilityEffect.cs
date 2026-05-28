using System;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000398 RID: 920
	public sealed class VitalEnergyModificationAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E58 RID: 7768 RVA: 0x000601C1 File Offset: 0x0005E3C1
		protected override float GetEffectAmount()
		{
			return this.amount;
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x000601C9 File Offset: 0x0005E3C9
		protected override void SetEffectAmount(float newAmount)
		{
			this.amount = newAmount;
		}

		// Token: 0x06001E5A RID: 7770 RVA: 0x000601D4 File Offset: 0x0005E3D4
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.amount != 0f)
			{
				IGameMob gameMob = effectTarget as IGameMob;
				VitalEnergyHitPointsController vitalEnergyHitPointsController = ((gameMob != null) ? gameMob.HitPointsController : null) as VitalEnergyHitPointsController;
				if (vitalEnergyHitPointsController != null)
				{
					HitPointsController.HPChangingArgs hpchangingArgs = (this.amount > 0f) ? VitalEnergyModificationAbilityEffect.RestoringArgs : VitalEnergyModificationAbilityEffect.ConsumingArgs;
					hpchangingArgs.amount = this.amount * dt;
					return vitalEnergyHitPointsController.ModifyHitPoints(base.GetEffectOwner(), hpchangingArgs) != 0f;
				}
			}
			return false;
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x00060250 File Offset: 0x0005E450
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new VitalEnergyModificationAbilityEffect(this);
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x00060258 File Offset: 0x0005E458
		public VitalEnergyModificationAbilityEffect()
		{
			this.amount = 10f;
		}

		// Token: 0x06001E5D RID: 7773 RVA: 0x0006026B File Offset: 0x0005E46B
		public VitalEnergyModificationAbilityEffect(VitalEnergyModificationAbilityEffect effectPrototype)
		{
			this.amount = effectPrototype.amount;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x04001118 RID: 4376
		private static readonly VitalEnergyHitPointsController.RestoreVitalEnergyArgs RestoringArgs = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs();

		// Token: 0x04001119 RID: 4377
		private static readonly VitalEnergyHitPointsController.ConsumeVitalEnergyArgs ConsumingArgs = new VitalEnergyHitPointsController.ConsumeVitalEnergyArgs();

		// Token: 0x0400111A RID: 4378
		public float amount;
	}
}
