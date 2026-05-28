using System;
using Game.Abilities;
using Unliving.Currencies;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.Abilities
{
	// Token: 0x0200036D RID: 877
	public sealed class AbilityCurrencyConsumptionController : AbilityExtensionAssetBase
	{
		// Token: 0x170005FC RID: 1532
		// (get) Token: 0x06001CDE RID: 7390 RVA: 0x0005B35C File Offset: 0x0005955C
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001CDF RID: 7391 RVA: 0x0005B360 File Offset: 0x00059560
		protected override void OnAbilityOwnerChanged(BaseAbility ability, object lastOwner, object newOwner)
		{
			if (this.playerProfile == null)
			{
				PlayerBehaviour playerBehaviour = newOwner as PlayerBehaviour;
				if (playerBehaviour != null)
				{
					PlayerProfileManager playerProfileManager = playerBehaviour.CurrentGame.Services.Get<PlayerProfileManager>();
					this.playerProfile = ((playerProfileManager != null) ? playerProfileManager.CurrentPlayerProfile : null);
				}
			}
		}

		// Token: 0x06001CE0 RID: 7392 RVA: 0x0005B3A1 File Offset: 0x000595A1
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.Activating += this.OnActivatingAbility;
			ability.Activated += this.OnAbilityActivated;
		}

		// Token: 0x06001CE1 RID: 7393 RVA: 0x0005B3CE File Offset: 0x000595CE
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activating -= this.OnActivatingAbility;
			ability.Activated -= this.OnAbilityActivated;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001CE2 RID: 7394 RVA: 0x0005B3FC File Offset: 0x000595FC
		private void OnActivatingAbility(IAbility ability, object args)
		{
			if (this.playerProfile == null)
			{
				return;
			}
			BaseAbility baseAbility = (BaseAbility)ability;
			BaseAbility.UsingArgs usingArgs = (BaseAbility.UsingArgs)args;
			for (int i = 0; i < this.abilityActivationCost.Length; i++)
			{
				ref AbilityCurrencyConsumptionController.Cost ptr = ref this.abilityActivationCost[i];
				int currencyAmount = this.playerProfile.GetCurrencyAmount(ptr.currencyID);
				if (ptr.amount > currencyAmount)
				{
					AbilityCurrencyConsumptionController.AbilityInterruptionError.Reset();
					AbilityCurrencyConsumptionController.AbilityInterruptionError.type = BaseAbility.ActivationErrorType.External;
					AbilityCurrencyConsumptionController.AbilityInterruptionError.usingArgs = usingArgs;
					AbilityCurrencyConsumptionController.AbilityInterruptionError.interruptionSource = this;
					baseAbility.TryInterruptActivation(AbilityCurrencyConsumptionController.AbilityInterruptionError);
					return;
				}
			}
		}

		// Token: 0x06001CE3 RID: 7395 RVA: 0x0005B494 File Offset: 0x00059694
		private void OnAbilityActivated(IAbility ability, object args)
		{
			if (this.playerProfile == null)
			{
				return;
			}
			for (int i = 0; i < this.abilityActivationCost.Length; i++)
			{
				this.playerProfile.TryExecuteCurrencyOperation(this.abilityActivationCost[i].ToCurrencyArgs(this));
			}
		}

		// Token: 0x04001058 RID: 4184
		private static readonly BaseAbility.ActivationError AbilityInterruptionError = new BaseAbility.ActivationError();

		// Token: 0x04001059 RID: 4185
		public AbilityCurrencyConsumptionController.Cost[] abilityActivationCost;

		// Token: 0x0400105A RID: 4186
		[NonSerialized]
		private PlayerProfile playerProfile;

		// Token: 0x02000564 RID: 1380
		[Serializable]
		public struct Cost
		{
			// Token: 0x06002709 RID: 9993 RVA: 0x0007990A File Offset: 0x00077B0A
			public CurrencyOperationArgs ToCurrencyArgs(object sender)
			{
				AbilityCurrencyConsumptionController.Cost.currencyArgs.currencyID = this.currencyID;
				AbilityCurrencyConsumptionController.Cost.currencyArgs.amount = (float)this.amount;
				AbilityCurrencyConsumptionController.Cost.currencyArgs.spending = true;
				AbilityCurrencyConsumptionController.Cost.currencyArgs.sender = sender;
				return AbilityCurrencyConsumptionController.Cost.currencyArgs;
			}

			// Token: 0x04001C13 RID: 7187
			private static CurrencyOperationArgs currencyArgs;

			// Token: 0x04001C14 RID: 7188
			public CurrencyID currencyID;

			// Token: 0x04001C15 RID: 7189
			public int amount;
		}
	}
}
