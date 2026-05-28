using System;
using System.Runtime.CompilerServices;
using Game.Abilities;
using Game.Stats;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x0200037A RID: 890
	[CreateAssetMenu(fileName = "AbilityUsingMovementSpeedController", menuName = "Abilities/Controllers/Movement Speed Controller")]
	public sealed class AbilityUsingMovementSpeedController : AbilityExtensionAssetBase
	{
		// Token: 0x1700060E RID: 1550
		// (get) Token: 0x06001D4B RID: 7499 RVA: 0x0005CD79 File Offset: 0x0005AF79
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D4C RID: 7500 RVA: 0x0005CD7C File Offset: 0x0005AF7C
		private void StartMovementModificationTask(AbilityUsingMovementSpeedController.MovementModifier movementModifier)
		{
			AbilityUsingMovementSpeedController.<StartMovementModificationTask>d__5 <StartMovementModificationTask>d__;
			<StartMovementModificationTask>d__.<>4__this = this;
			<StartMovementModificationTask>d__.movementModifier = movementModifier;
			<StartMovementModificationTask>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<StartMovementModificationTask>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <StartMovementModificationTask>d__.<>t__builder;
			<>t__builder.Start<AbilityUsingMovementSpeedController.<StartMovementModificationTask>d__5>(ref <StartMovementModificationTask>d__);
		}

		// Token: 0x06001D4D RID: 7501 RVA: 0x0005CDBD File Offset: 0x0005AFBD
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.Activating += this.OnAbilityActivation;
		}

		// Token: 0x06001D4E RID: 7502 RVA: 0x0005CDD8 File Offset: 0x0005AFD8
		private void OnAbilityActivation(IAbility ability, object args)
		{
			if (this.abilityActivationSlowdown <= 0f && this.abilityUsingSlowdown <= 0f)
			{
				return;
			}
			if (ability.PrepProgress == 0f)
			{
				this.StartMovementModificationTask(new AbilityUsingMovementSpeedController.MovementModifier(ability));
			}
		}

		// Token: 0x06001D4F RID: 7503 RVA: 0x0005CE0E File Offset: 0x0005B00E
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activating -= this.OnAbilityActivation;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x04001094 RID: 4244
		[Range(0f, 1f)]
		public float abilityActivationSlowdown;

		// Token: 0x04001095 RID: 4245
		[Range(0f, 1f)]
		public float abilityUsingSlowdown;

		// Token: 0x02000569 RID: 1385
		private struct MovementModifier
		{
			// Token: 0x06002713 RID: 10003 RVA: 0x000799F0 File Offset: 0x00077BF0
			public MovementModifier(IAbility ability)
			{
				this.Ability = ability;
				this.abilityOwner = (ability.Owner as IGameMob);
				IStatsOwner<MobStatModifier> statsOwner = this.abilityOwner as IStatsOwner<MobStatModifier>;
				this.statsController = ((statsOwner != null) ? statsOwner.StatsController : null);
				IGameMob gameMob = this.abilityOwner;
				this.motionController = ((gameMob != null) ? gameMob.MotionController : null);
				this.activeSpeedModifier = MobStatModifier.Neutral;
				this.isMovementFreezed = false;
			}

			// Token: 0x06002714 RID: 10004 RVA: 0x00079A5C File Offset: 0x00077C5C
			public void ModifyAbilityOwnerSpeed(float slowdownAmount)
			{
				if (slowdownAmount <= 0f)
				{
					this.ResetAbilityOwnerSpeed();
					return;
				}
				if (slowdownAmount >= 1f)
				{
					if (this.motionController != null && !this.motionController.ControllerOwner.IsKinematic)
					{
						this.abilityOwner.SetNavMeshAgentActive(false);
						this.motionController.FreezeMovement(0f, true);
						this.isMovementFreezed = true;
					}
					return;
				}
				if (this.statsController == null)
				{
					return;
				}
				float num = 1f - slowdownAmount;
				if (this.activeSpeedModifier.ExtraModifier != num)
				{
					this.statsController.RemoveModifier(AbilityUsingMovementSpeedController.MovementModifier.SpeedStatID, this.activeSpeedModifier);
					this.activeSpeedModifier = new MobStatModifier(0f, 0f, num);
					this.statsController.AddModifier(AbilityUsingMovementSpeedController.MovementModifier.SpeedStatID, this.activeSpeedModifier);
				}
			}

			// Token: 0x06002715 RID: 10005 RVA: 0x00079B20 File Offset: 0x00077D20
			public void ResetAbilityOwnerSpeed()
			{
				if (this.isMovementFreezed)
				{
					if (!this.motionController.ControllerOwner.IsKinematic)
					{
						this.abilityOwner.SetNavMeshAgentActive(true);
					}
					this.isMovementFreezed = false;
					return;
				}
				if (this.statsController != null && !this.activeSpeedModifier.IsNeutral())
				{
					this.statsController.RemoveModifier(AbilityUsingMovementSpeedController.MovementModifier.SpeedStatID, this.activeSpeedModifier);
					this.activeSpeedModifier = MobStatModifier.Neutral;
				}
			}

			// Token: 0x04001C20 RID: 7200
			private static readonly int SpeedStatID = 1;

			// Token: 0x04001C21 RID: 7201
			public readonly IAbility Ability;

			// Token: 0x04001C22 RID: 7202
			private readonly IStatsController<MobStatModifier> statsController;

			// Token: 0x04001C23 RID: 7203
			private readonly GameMobMotionControllerBase motionController;

			// Token: 0x04001C24 RID: 7204
			private readonly IGameMob abilityOwner;

			// Token: 0x04001C25 RID: 7205
			private MobStatModifier activeSpeedModifier;

			// Token: 0x04001C26 RID: 7206
			private bool isMovementFreezed;
		}
	}
}
