using System;
using Game.Abilities;
using Game.Damage.Projectiles;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x02000391 RID: 913
	[Serializable]
	public sealed class ProjectileSplittingAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E1D RID: 7709 RVA: 0x0005F5A8 File Offset: 0x0005D7A8
		private static Vector2 GetRandomDirection()
		{
			float f = UnityEngine.Random.value * 3.1415927f * 2f;
			return new Vector2
			{
				x = Mathf.Cos(f),
				y = Mathf.Sin(f)
			};
		}

		// Token: 0x06001E1E RID: 7710 RVA: 0x0005F5EA File Offset: 0x0005D7EA
		public ProjectileSplittingAbilityEffect()
		{
			this.minProjectilesCount = 3;
			this.minReboundDistance = 5f;
		}

		// Token: 0x06001E1F RID: 7711 RVA: 0x0005F604 File Offset: 0x0005D804
		public ProjectileSplittingAbilityEffect(ProjectileSplittingAbilityEffect effectPrototype)
		{
			this.minProjectilesCount = effectPrototype.minProjectilesCount;
			this.maxProjectilesCount = effectPrototype.maxProjectilesCount;
			this.minReboundDistance = effectPrototype.minReboundDistance;
			this.maxReboundDistance = effectPrototype.maxReboundDistance;
			this.maxSplittingDepth = effectPrototype.maxSplittingDepth;
		}

		// Token: 0x06001E20 RID: 7712 RVA: 0x0005F653 File Offset: 0x0005D853
		protected override float GetEffectAmount()
		{
			return (float)this.minProjectilesCount;
		}

		// Token: 0x06001E21 RID: 7713 RVA: 0x0005F65C File Offset: 0x0005D85C
		protected override void SetEffectAmount(float newAmount)
		{
			this.minProjectilesCount = (int)newAmount;
		}

		// Token: 0x06001E22 RID: 7714 RVA: 0x0005F668 File Offset: 0x0005D868
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.parentProjectile == null)
			{
				return;
			}
			if (this.parentProjectile.Generation >= Mathf.Max(this.maxSplittingDepth, 1))
			{
				return;
			}
			IProjectile projectilePrototype = this.currentProjectileAbility.ProjectilePrototype;
			int num = (this.maxProjectilesCount > 0) ? UnityEngine.Random.Range(this.minProjectilesCount, this.maxProjectilesCount) : this.minProjectilesCount;
			for (int i = 0; i < num; i++)
			{
				float num2 = (this.maxReboundDistance > 0f) ? UnityEngine.Random.Range(this.minReboundDistance, this.maxReboundDistance) : this.minReboundDistance;
				if (num2 > 0f)
				{
					Vector3 direction = ProjectileSplittingAbilityEffect.GetRandomDirection();
					ProjectileSplittingAbilityEffect.ProjectileLaunchArgs.launcher = this.currentProjectileAbility.Owner;
					ProjectileSplittingAbilityEffect.ProjectileLaunchArgs.position = this.hitPosition;
					ProjectileSplittingAbilityEffect.ProjectileLaunchArgs.direction = direction;
					ProjectileSplittingAbilityEffect.ProjectileLaunchArgs.lifetimeOverride = projectilePrototype.GetTimeToReachTarget(ProjectileSplittingAbilityEffect.ProjectileLaunchArgs, num2);
					IProjectile projectile = this.currentProjectileAbility.LaunchProjectile(ProjectileSplittingAbilityEffect.ProjectileLaunchArgs, abilityUsingArgs, null);
					if (projectile != null)
					{
						projectile.Generation = this.parentProjectile.Generation + 1;
						if (this.hitReceiver != null)
						{
							BaseProjectile baseProjectile = projectile as BaseProjectile;
							if (baseProjectile != null)
							{
								baseProjectile.AddIgnorableHitReceiver(this.hitReceiver);
							}
						}
					}
				}
			}
			this.parentProjectile = null;
			this.hitReceiver = null;
		}

		// Token: 0x06001E23 RID: 7715 RVA: 0x0005F7C5 File Offset: 0x0005D9C5
		protected override bool Use(Component effectTarget, float dt)
		{
			return false;
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x0005F7C8 File Offset: 0x0005D9C8
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new ProjectileSplittingAbilityEffect((ProjectileSplittingAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E25 RID: 7717 RVA: 0x0005F7D8 File Offset: 0x0005D9D8
		protected override void OnAbilityChanged(BaseAbility newAbility)
		{
			if (this.currentProjectileAbility != null)
			{
				this.currentProjectileAbility.ProjectileUsingPrepared -= this.OnAbilityProjectileUsingPrepared;
			}
			if ((this.currentProjectileAbility = (newAbility as ProjectileAbilityBase)) != null)
			{
				this.currentProjectileAbility.ProjectileUsingPrepared += this.OnAbilityProjectileUsingPrepared;
			}
		}

		// Token: 0x06001E26 RID: 7718 RVA: 0x0005F838 File Offset: 0x0005DA38
		private void OnAbilityProjectileUsingPrepared(ProjectileAbilityBase.UsingEventArgs args)
		{
			ProjectileHitInfo projectileHitArgs = args.projectileHitArgs;
			this.parentProjectile = projectileHitArgs.projectile;
			this.hitPosition = projectileHitArgs.point;
			Component component = projectileHitArgs.hitReceiver;
			this.hitReceiver = ((component != null) ? component.gameObject : null);
		}

		// Token: 0x040010F4 RID: 4340
		private static readonly ProjectileLaunchArgs ProjectileLaunchArgs = new ProjectileLaunchArgs();

		// Token: 0x040010F5 RID: 4341
		public int minProjectilesCount;

		// Token: 0x040010F6 RID: 4342
		public int maxProjectilesCount;

		// Token: 0x040010F7 RID: 4343
		public float minReboundDistance;

		// Token: 0x040010F8 RID: 4344
		public float maxReboundDistance;

		// Token: 0x040010F9 RID: 4345
		public int maxSplittingDepth;

		// Token: 0x040010FA RID: 4346
		private ProjectileAbilityBase currentProjectileAbility;

		// Token: 0x040010FB RID: 4347
		private IProjectile parentProjectile;

		// Token: 0x040010FC RID: 4348
		private Vector2 hitPosition;

		// Token: 0x040010FD RID: 4349
		private GameObject hitReceiver;
	}
}
