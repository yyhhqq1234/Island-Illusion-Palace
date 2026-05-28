using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Gameplay;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000392 RID: 914
	[Serializable]
	public sealed class PushingImpulseAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E28 RID: 7720 RVA: 0x0005F88D File Offset: 0x0005DA8D
		public PushingImpulseAbilityEffect()
		{
		}

		// Token: 0x06001E29 RID: 7721 RVA: 0x0005F8A0 File Offset: 0x0005DAA0
		public PushingImpulseAbilityEffect(PushingImpulseAbilityEffect effectPrototype)
		{
			this.pushImpulse = effectPrototype.pushImpulse;
			this.pushTowardsEffectOwner = effectPrototype.pushTowardsEffectOwner;
			this.verticalImpulse = effectPrototype.verticalImpulse;
			this.parabolicTrajectoryObstacles = effectPrototype.parabolicTrajectoryObstacles;
			this.useDistanceFalloff = effectPrototype.useDistanceFalloff;
			this.maxTargetsCountOverride = effectPrototype.maxTargetsCountOverride;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x0005F910 File Offset: 0x0005DB10
		protected override bool Use(Component effectTarget, float dt)
		{
			float num = (this.pushImpulse > 0f) ? (this.pushImpulse * base.GetAmountModifier(effectTarget)) : 0f;
			if (num <= 0f || (this.maxTargetsCountOverride > 0 && this.affectedTargetsCount == this.maxTargetsCountOverride))
			{
				return false;
			}
			IMovableObject movableObject = effectTarget.CastOrGetComponent<IMovableObject>();
			if (movableObject == null)
			{
				return false;
			}
			Vector2 vector = movableObject.Position;
			Vector2 vector2 = this.invertImpulseDirection ? (this.impulseTargetPosition - vector) : (vector - this.impulseTargetPosition);
			float num2 = this.useDistanceFalloff ? (1f - Mathf.Clamp01(vector2.SqrMagnitude() / this.abilityRangeSq)) : 1f;
			if (num2 < 0.0001f)
			{
				return false;
			}
			vector2.Normalize();
			vector2.x *= num * num2;
			vector2.y *= num * num2;
			this.affectedTargetsCount++;
			if (this.verticalImpulse > 0f)
			{
				BaseGameMob baseGameMob = movableObject as BaseGameMob;
				if (baseGameMob != null && baseGameMob.MotionController != null)
				{
					Vector2 impulsePushDisplacement = baseGameMob.MotionController.GetImpulsePushDisplacement(vector2, false);
					float num3 = impulsePushDisplacement.magnitude;
					Vector2 vector3 = impulsePushDisplacement / num3;
					RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, vector3, num3, this.parabolicTrajectoryObstacles);
					if (raycastHit2D.collider != null && raycastHit2D.distance < num3)
					{
						num3 = raycastHit2D.distance;
					}
					if (num3 > 0.05f)
					{
						Vector2 vector4 = new Vector2
						{
							y = this.verticalImpulse * num2
						};
						baseGameMob.MotionController.ImpulseDamping.Apply(ref vector4);
						if (baseGameMob.MotionController.JumpToPoint(vector + vector3 * num3, vector4.y, 0f, false, this.currentAbility) != null)
						{
							return true;
						}
					}
				}
			}
			movableObject.AddMovementImpulse(vector2, false);
			return true;
		}

		// Token: 0x06001E2B RID: 7723 RVA: 0x0005FB08 File Offset: 0x0005DD08
		protected override float GetEffectAmount()
		{
			return this.pushImpulse;
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x0005FB10 File Offset: 0x0005DD10
		protected override void SetEffectAmount(float newAmount)
		{
			this.pushImpulse = newAmount;
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x0005FB19 File Offset: 0x0005DD19
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new PushingImpulseAbilityEffect((PushingImpulseAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E2E RID: 7726 RVA: 0x0005FB28 File Offset: 0x0005DD28
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			this.invertImpulseDirection = false;
			if (this.pushTowardsEffectOwner)
			{
				IGameMob gameMob = base.GetEffectOwner() as IGameMob;
				if (gameMob != null)
				{
					this.impulseTargetPosition = gameMob.Position;
					this.invertImpulseDirection = true;
					goto IL_44;
				}
			}
			this.impulseTargetPosition = abilityUsingArgs.targetPosition;
			IL_44:
			this.affectedTargetsCount = 0;
			if (this.useDistanceFalloff)
			{
				this.abilityRangeSq = this.currentAbility.Range;
				this.abilityRangeSq *= this.abilityRangeSq;
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x040010FE RID: 4350
		public float pushImpulse = 10f;

		// Token: 0x040010FF RID: 4351
		public bool pushTowardsEffectOwner;

		// Token: 0x04001100 RID: 4352
		public float verticalImpulse;

		// Token: 0x04001101 RID: 4353
		public LayerMask parabolicTrajectoryObstacles;

		// Token: 0x04001102 RID: 4354
		public bool useDistanceFalloff;

		// Token: 0x04001103 RID: 4355
		public int maxTargetsCountOverride;

		// Token: 0x04001104 RID: 4356
		private Vector2 impulseTargetPosition;

		// Token: 0x04001105 RID: 4357
		private bool invertImpulseDirection;

		// Token: 0x04001106 RID: 4358
		private float abilityRangeSq;

		// Token: 0x04001107 RID: 4359
		private int affectedTargetsCount;
	}
}
