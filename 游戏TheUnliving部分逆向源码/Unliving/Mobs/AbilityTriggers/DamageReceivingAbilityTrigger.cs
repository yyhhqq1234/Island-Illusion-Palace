using System;
using Game.Abilities;
using Game.Damage;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022B RID: 555
	[CreateAssetMenu(fileName = "DamageReceivingAbilityTrigger", menuName = "Abilities/Triggers/Damage Receiving Trigger")]
	public sealed class DamageReceivingAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000406 RID: 1030
		// (get) Token: 0x06001302 RID: 4866 RVA: 0x0003C29B File Offset: 0x0003A49B
		// (set) Token: 0x06001303 RID: 4867 RVA: 0x0003C2A3 File Offset: 0x0003A4A3
		public float NormalizedHitPointsThreshold
		{
			get
			{
				return this._normalizedHitPointsThreshold;
			}
			set
			{
				this._normalizedHitPointsThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x17000407 RID: 1031
		// (get) Token: 0x06001304 RID: 4868 RVA: 0x0003C2B1 File Offset: 0x0003A4B1
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000408 RID: 1032
		// (get) Token: 0x06001305 RID: 4869 RVA: 0x0003C2B4 File Offset: 0x0003A4B4
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000409 RID: 1033
		// (get) Token: 0x06001306 RID: 4870 RVA: 0x0003C2BB File Offset: 0x0003A4BB
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001307 RID: 4871 RVA: 0x0003C2C0 File Offset: 0x0003A4C0
		private void HandleDamageEvents(IDamageable hitPointsController, bool subscribe)
		{
			if (this.triggerEvent != DamageReceivingAbilityTrigger.Event.AfterDamageApplied)
			{
				if (this.triggerEvent == DamageReceivingAbilityTrigger.Event.BeforeDamageApplied)
				{
					if (subscribe)
					{
						hitPointsController.BeforeHitPointsChanged += this.OnMobHitPointsChanged;
						return;
					}
					hitPointsController.BeforeHitPointsChanged -= this.OnMobHitPointsChanged;
				}
				return;
			}
			if (subscribe)
			{
				hitPointsController.HitPointsChanged += this.OnMobHitPointsChanged;
				return;
			}
			hitPointsController.HitPointsChanged -= this.OnMobHitPointsChanged;
		}

		// Token: 0x06001308 RID: 4872 RVA: 0x0003C32F File Offset: 0x0003A52F
		private void SetHitPointsController(IDamageable newHitPointsController)
		{
			if (this.hitPointsController != null)
			{
				this.HandleDamageEvents(this.hitPointsController, false);
			}
			if (newHitPointsController != null)
			{
				this.isConditionReached = false;
				this.HandleDamageEvents(newHitPointsController, true);
			}
			this.hitPointsController = newHitPointsController;
		}

		// Token: 0x06001309 RID: 4873 RVA: 0x0003C35F File Offset: 0x0003A55F
		private void UpdateDamageSendersFilterState()
		{
			this.useDamageSendersFilter = !this.allowedDamageSendersDescription.IsBlank();
		}

		// Token: 0x0600130A RID: 4874 RVA: 0x0003C375 File Offset: 0x0003A575
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return this.isConditionReached;
		}

		// Token: 0x0600130B RID: 4875 RVA: 0x0003C37D File Offset: 0x0003A57D
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.UpdateDamageSendersFilterState();
		}

		// Token: 0x0600130C RID: 4876 RVA: 0x0003C38C File Offset: 0x0003A58C
		protected override void ResetTrigger(BaseAbility ability)
		{
			base.ResetTrigger(ability);
			this.UpdateDamageSendersFilterState();
			this.isConditionReached = false;
		}

		// Token: 0x0600130D RID: 4877 RVA: 0x0003C3A2 File Offset: 0x0003A5A2
		protected override void OnAbilityOwnerChanged(BaseAbility ability, object lastOwner, object newOwner)
		{
			base.OnAbilityOwnerChanged(ability, lastOwner, newOwner);
			BaseGameMob abilityOwningMob = base.GetAbilityOwningMob(ability);
			this.SetHitPointsController((abilityOwningMob != null) ? abilityOwningMob.HitPointsController : null);
		}

		// Token: 0x0600130E RID: 4878 RVA: 0x0003C3C8 File Offset: 0x0003A5C8
		private void OnMobHitPointsChanged(IHitPointsSource hitPointsSource, object attacker, IHitPointsChangingArgs args)
		{
			if (!this.isConditionReached && args.IsDamage)
			{
				if (this.ignoreSilentDamage)
				{
					HitPointsController.HPChangingArgs hpchangingArgs = args as HitPointsController.HPChangingArgs;
					if (hpchangingArgs != null && hpchangingArgs.isSilentChanging)
					{
						return;
					}
				}
				if (this.useDamageSendersFilter)
				{
					BaseGameMob baseGameMob = attacker as BaseGameMob;
					if (baseGameMob != null && !this.allowedDamageSendersDescription.IsMatch(baseGameMob))
					{
						return;
					}
				}
				if (this._normalizedHitPointsThreshold <= 0f)
				{
					this.isConditionReached = true;
				}
				else
				{
					if (this.isHPContainerTrigger)
					{
						IContainerBasedHPController containerBasedHPController = this.hitPointsController as IContainerBasedHPController;
						if (containerBasedHPController != null && containerBasedHPController.HealthContainers.Count != 0)
						{
							this.isConditionReached = (containerBasedHPController.CurrentHealthContainer.FillAmount < this._normalizedHitPointsThreshold);
							goto IL_BA;
						}
					}
					this.isConditionReached = (this.hitPointsController.GetNormalizedHitPoints() < this._normalizedHitPointsThreshold);
				}
				IL_BA:
				if (this.isConditionReached)
				{
					base.TryTriggerAutoUseAbility(this.currentAbility, (this.triggerEvent == DamageReceivingAbilityTrigger.Event.BeforeDamageApplied) ? args : null, false);
				}
				return;
			}
		}

		// Token: 0x04000B1C RID: 2844
		public DamageReceivingAbilityTrigger.Event triggerEvent = DamageReceivingAbilityTrigger.Event.AfterDamageApplied;

		// Token: 0x04000B1D RID: 2845
		[SerializeField]
		[Range(0f, 1f)]
		private float _normalizedHitPointsThreshold;

		// Token: 0x04000B1E RID: 2846
		[Tooltip("Если опция активна, то срабатывание будет проверяться по количеству хп в текущем контейнере.")]
		public bool isHPContainerTrigger;

		// Token: 0x04000B1F RID: 2847
		public bool ignoreSilentDamage = true;

		// Token: 0x04000B20 RID: 2848
		public GameMobDescription allowedDamageSendersDescription = GameMobDescription.BlankDescription;

		// Token: 0x04000B21 RID: 2849
		private IDamageable hitPointsController;

		// Token: 0x04000B22 RID: 2850
		private bool useDamageSendersFilter;

		// Token: 0x04000B23 RID: 2851
		private bool isConditionReached;

		// Token: 0x020004C7 RID: 1223
		public enum Event
		{
			// Token: 0x040019B7 RID: 6583
			BeforeDamageApplied,
			// Token: 0x040019B8 RID: 6584
			AfterDamageApplied
		}
	}
}
