using System;
using Common.Editor;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000226 RID: 550
	[CreateAssetMenu(fileName = "AbilityEffectReactionAbilityTrigger", menuName = "Abilities/Triggers/Ability Effect Reaction Trigger")]
	public sealed class AbilityEffectReactionAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x170003F5 RID: 1013
		// (get) Token: 0x060012DA RID: 4826 RVA: 0x0003BEB5 File Offset: 0x0003A0B5
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003F6 RID: 1014
		// (get) Token: 0x060012DB RID: 4827 RVA: 0x0003BEB8 File Offset: 0x0003A0B8
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170003F7 RID: 1015
		// (get) Token: 0x060012DC RID: 4828 RVA: 0x0003BEBF File Offset: 0x0003A0BF
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060012DD RID: 4829 RVA: 0x0003BEC2 File Offset: 0x0003A0C2
		protected override MobAbilityTriggerBase.TriggerState CreateTriggerState(BaseAbility ability)
		{
			return new MobAbilityTriggerBase.TriggerState();
		}

		// Token: 0x060012DE RID: 4830 RVA: 0x0003BEC9 File Offset: 0x0003A0C9
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return base.GetStoredTriggerState(ability).isConditionReached;
		}

		// Token: 0x060012DF RID: 4831 RVA: 0x0003BED8 File Offset: 0x0003A0D8
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			INotifyAbilityEffectReceived notifyAbilityEffectReceived = lastOwner as INotifyAbilityEffectReceived;
			if (notifyAbilityEffectReceived != null)
			{
				notifyAbilityEffectReceived.AbilityEffectReceived -= this.OnAbilityEffectReceived;
			}
			INotifyAbilityEffectReceived notifyAbilityEffectReceived2 = newOwner as INotifyAbilityEffectReceived;
			if (notifyAbilityEffectReceived2 != null)
			{
				notifyAbilityEffectReceived2.AbilityEffectReceived += this.OnAbilityEffectReceived;
			}
		}

		// Token: 0x060012E0 RID: 4832 RVA: 0x0003BF1D File Offset: 0x0003A11D
		private void OnAbilityEffectReceived(BaseAbility ability, AbilityEffectBase abilityEffect, float effectAmount)
		{
			if (abilityEffect is TriggerAbilityEffect && (this.expectedAbility == AbilityID.None || this.expectedAbility == (AbilityID)ability.ID))
			{
				base.GetStoredTriggerState(ability).isConditionReached = true;
			}
		}

		// Token: 0x04000B13 RID: 2835
		[EnumPopup]
		[Tooltip("Конкретная ожидаемая абилити для активации триггера. Если не задана, то учитываться не будет.")]
		public AbilityID expectedAbility;
	}
}
