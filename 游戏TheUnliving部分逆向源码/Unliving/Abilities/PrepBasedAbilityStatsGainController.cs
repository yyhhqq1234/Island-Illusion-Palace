using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Damage.Projectiles;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x0200037C RID: 892
	[CreateAssetMenu(fileName = "PrepBasedAbilityStatsGainController", menuName = "Abilities/Controllers/Prep Based Ability Stats Gain Controller")]
	public sealed class PrepBasedAbilityStatsGainController : AbilityExtensionAssetBase, IMobStatsModifiersGenerator
	{
		// Token: 0x1700060F RID: 1551
		// (get) Token: 0x06001D57 RID: 7511 RVA: 0x0005CF27 File Offset: 0x0005B127
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D58 RID: 7512 RVA: 0x0005CF2C File Offset: 0x0005B12C
		private void PushModifiers(IAbility targetAbility, float gainProgress)
		{
			ITempMobStatsModifiersReceiver tempMobStatsModifiersReceiver = (ITempMobStatsModifiersReceiver)targetAbility;
			for (int i = 0; i < this.statsModifiers.Length; i++)
			{
				tempMobStatsModifiersReceiver.AddTempStatModifier(this.statsModifiers[i].ToTargetedStatModifier(gainProgress));
			}
		}

		// Token: 0x06001D59 RID: 7513 RVA: 0x0005CF6C File Offset: 0x0005B16C
		int IMobStatsModifiersGenerator.GetCurrentStatsModifiers(object modifiersTarget, out TargetedMobStatModifier[] statsModifiers)
		{
			statsModifiers = PrepBasedAbilityStatsGainController.StatModifiersBuffer;
			IAbility ability = modifiersTarget as IAbility;
			if (ability != null && (ability.IsActivated || ability.IsPrepInProgress()))
			{
				float prepProgress = ability.PrepProgress;
				for (int i = 0; i < this.statsModifiers.Length; i++)
				{
					statsModifiers[i] = this.statsModifiers[i].ToTargetedStatModifier(prepProgress);
				}
				return this.statsModifiers.Length;
			}
			return 0;
		}

		// Token: 0x06001D5A RID: 7514 RVA: 0x0005CFD8 File Offset: 0x0005B1D8
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			if (!(ability is ITempMobStatsModifiersReceiver))
			{
				return;
			}
			ability.Activated += this.OnAbilityPrepared;
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched += this.OnAbilityProjectileLaunched;
				projectileAbilityBase.ProjectileUsingPrepared += this.OnBeforeAbilityProjectileHit;
				projectileAbilityBase.ProjectileDestroyed += this.OnAbilityProjectileDestroyed;
			}
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x0005D048 File Offset: 0x0005B248
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activated -= this.OnAbilityPrepared;
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
				projectileAbilityBase.ProjectileUsingPrepared -= this.OnBeforeAbilityProjectileHit;
				projectileAbilityBase.ProjectileDestroyed -= this.OnAbilityProjectileDestroyed;
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x0005D0AE File Offset: 0x0005B2AE
		private void OnAbilityPrepared(IAbility ability, object usingArgs)
		{
			this.PushModifiers(ability, ability.PrepProgress);
		}

		// Token: 0x06001D5D RID: 7517 RVA: 0x0005D0C0 File Offset: 0x0005B2C0
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs launchArgs)
		{
			int instanceID = launchArgs.launchedProjectile.InstanceID;
			this.projectilesGains.Add(instanceID, launchArgs.ability.PrepProgress);
		}

		// Token: 0x06001D5E RID: 7518 RVA: 0x0005D0F0 File Offset: 0x0005B2F0
		private void OnBeforeAbilityProjectileHit(ProjectileAbilityBase.UsingEventArgs usingArgs)
		{
			int instanceID = usingArgs.projectileHitArgs.projectile.InstanceID;
			float gainProgress;
			if (this.projectilesGains.TryGetValue(instanceID, out gainProgress))
			{
				this.PushModifiers(usingArgs.ability, gainProgress);
			}
		}

		// Token: 0x06001D5F RID: 7519 RVA: 0x0005D12B File Offset: 0x0005B32B
		private void OnAbilityProjectileDestroyed(ProjectileAbilityBase ability, IProjectile projectile)
		{
			this.projectilesGains.Remove(projectile.InstanceID);
		}

		// Token: 0x04001097 RID: 4247
		private static readonly TargetedMobStatModifier[] StatModifiersBuffer = new TargetedMobStatModifier[32];

		// Token: 0x04001098 RID: 4248
		public CurveBasedMobStatModifier[] statsModifiers;

		// Token: 0x04001099 RID: 4249
		private readonly Dictionary<int, float> projectilesGains = new Dictionary<int, float>(16);
	}
}
