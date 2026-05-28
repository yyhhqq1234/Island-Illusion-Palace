using System;
using Game.Abilities;
using Game.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Abilities
{
	// Token: 0x0200037F RID: 895
	[CreateAssetMenu(fileName = "StaminaConsumptionAbilityController", menuName = "Abilities/Controllers/Stamina Consumption Controller")]
	public sealed class StaminaConsumptionAbilityController : AbilityExtensionAssetBase, IAbilityUpdateNotifiable
	{
		// Token: 0x17000611 RID: 1553
		// (get) Token: 0x06001D7B RID: 7547 RVA: 0x0005D9B7 File Offset: 0x0005BBB7
		// (set) Token: 0x06001D7C RID: 7548 RVA: 0x0005D9BF File Offset: 0x0005BBBF
		public float AbilityUnlockingThreshold
		{
			get
			{
				return this._abilityUnlockingThreshold;
			}
			set
			{
				this._abilityUnlockingThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x06001D7D RID: 7549 RVA: 0x0005D9CD File Offset: 0x0005BBCD
		private float GetStaminaCost()
		{
			return this.staminaCost * this.currentAbility.GetAmountModifier();
		}

		// Token: 0x06001D7E RID: 7550 RVA: 0x0005D9E4 File Offset: 0x0005BBE4
		private void ResetAbility()
		{
			if (this.initialAbilityReloadingTime != 0f)
			{
				this.currentAbility.ReloadingTime = this.initialAbilityReloadingTime;
			}
			if (this.staminaStatOwner != null)
			{
				this.staminaStatOwner.StaminaRestoringSpeed = this.initialStaminaRestoringSpeed;
				this.staminaStatOwner.StaminaRestoringDelay = this.initialStaminaRestoringDelay;
			}
		}

		// Token: 0x06001D7F RID: 7551 RVA: 0x0005DA39 File Offset: 0x0005BC39
		public bool IsWaitingForStamina()
		{
			return this.isAbilityBlocked || this.staminaStatOwner.CurrentStamina <= 0f;
		}

		// Token: 0x06001D80 RID: 7552 RVA: 0x0005DA5A File Offset: 0x0005BC5A
		public override void OnAddedToAbility(BaseAbility ability)
		{
			ability.Activating += this.OnAbilityPreparing;
			ability.Used += this.OnAbilityUsed;
			ability.AddUpdateListener(this);
			base.OnAddedToAbility(ability);
		}

		// Token: 0x06001D81 RID: 7553 RVA: 0x0005DA8E File Offset: 0x0005BC8E
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			this.ResetAbility();
			ability.Activating -= this.OnAbilityPreparing;
			ability.Used -= this.OnAbilityUsed;
			ability.RemoveUpdateListener(this);
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D82 RID: 7554 RVA: 0x0005DAC8 File Offset: 0x0005BCC8
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			this.ResetAbility();
			this.staminaStatOwner = (newOwner as IStaminaStatOwner);
			this.initialAbilityReloadingTime = currentAbility.ReloadingTime;
			if (this.staminaStatOwner != null)
			{
				this.initialStaminaRestoringSpeed = this.staminaStatOwner.StaminaRestoringSpeed;
				this.initialStaminaRestoringDelay = this.staminaStatOwner.StaminaRestoringDelay;
				if (this.staminaRestoringSpeedOverride > 0f)
				{
					this.staminaStatOwner.StaminaRestoringSpeed = this.staminaRestoringSpeedOverride;
				}
				if (this.staminaRestoringDelayOverride > 0f)
				{
					this.staminaStatOwner.StaminaRestoringDelay = this.staminaRestoringDelayOverride;
				}
			}
		}

		// Token: 0x06001D83 RID: 7555 RVA: 0x0005DB5C File Offset: 0x0005BD5C
		private void OnAbilityPreparing(IAbility ability, object usingArgs)
		{
			if (ability.PrepProgress == 0f && this.staminaStatOwner != null && this.IsWaitingForStamina())
			{
				StaminaConsumptionAbilityController.InterruptionError.Reset();
				StaminaConsumptionAbilityController.InterruptionError.type = BaseAbility.ActivationErrorType.External;
				StaminaConsumptionAbilityController.InterruptionError.usingArgs = (BaseAbility.UsingArgs)usingArgs;
				StaminaConsumptionAbilityController.InterruptionError.interruptionSource = this;
				((BaseAbility)ability).TryInterruptActivation(StaminaConsumptionAbilityController.InterruptionError);
			}
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x0005DBC8 File Offset: 0x0005BDC8
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			if (this.staminaStatOwner != null)
			{
				this.staminaStatOwner.CurrentStamina -= this.GetStaminaCost();
				if (this.staminaStatOwner.CurrentStamina <= 0f)
				{
					((BaseAbility)ability).Complete();
					this.isAbilityBlocked = true;
				}
			}
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x0005DC1C File Offset: 0x0005BE1C
		void IAbilityUpdateNotifiable.OnAbilityUpdated(BaseAbility ability, BaseAbility.UsingArgs currentUsingArgs)
		{
			if (this.useReloadingTimeSlowdown && this.staminaStatOwner.MaxStamina > 0f && this.staminaStatOwner.CurrentStamina != this.staminaStatOwner.MaxStamina)
			{
				float time = Mathf.Clamp01(this.staminaStatOwner.CurrentStamina / this.staminaStatOwner.MaxStamina);
				float num = this.reloadingTimeSlowdownCurve.Evaluate(time);
				this.currentAbility.ReloadingTime = this.initialAbilityReloadingTime * ((num > 0f) ? (1f / num) : 0f);
			}
			if (this.isAbilityBlocked && this.staminaStatOwner.CurrentStamina >= this.staminaStatOwner.MaxStamina * this._abilityUnlockingThreshold)
			{
				this.isAbilityBlocked = false;
			}
		}

		// Token: 0x040010AA RID: 4266
		private static readonly BaseAbility.ActivationError InterruptionError = new BaseAbility.ActivationError();

		// Token: 0x040010AB RID: 4267
		[FormerlySerializedAs("staminaAmount")]
		public float staminaCost = 5f;

		// Token: 0x040010AC RID: 4268
		public float staminaRestoringSpeedOverride;

		// Token: 0x040010AD RID: 4269
		public float staminaRestoringDelayOverride;

		// Token: 0x040010AE RID: 4270
		[SerializeField]
		[Range(0f, 1f)]
		private float _abilityUnlockingThreshold;

		// Token: 0x040010AF RID: 4271
		[Space(5f)]
		public AnimationCurve reloadingTimeSlowdownCurve;

		// Token: 0x040010B0 RID: 4272
		public bool useReloadingTimeSlowdown;

		// Token: 0x040010B1 RID: 4273
		private IStaminaStatOwner staminaStatOwner;

		// Token: 0x040010B2 RID: 4274
		private float initialStaminaRestoringSpeed;

		// Token: 0x040010B3 RID: 4275
		private float initialStaminaRestoringDelay;

		// Token: 0x040010B4 RID: 4276
		private bool isAbilityBlocked;

		// Token: 0x040010B5 RID: 4277
		private float initialAbilityReloadingTime;
	}
}
