using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CB RID: 971
	[CreateAssetMenu(fileName = "FinishingDamageModifiersActivator", menuName = "Abilities/Modifiers Activators/Finishing Damage Activator")]
	public sealed class FinishingDamageModifiersActivator : AbilityModifiersActivatorBase
	{
		// Token: 0x060020F3 RID: 8435 RVA: 0x000678C0 File Offset: 0x00065AC0
		private bool IsKilledBy(Component abilityTarget, object abilityOwner)
		{
			BaseGameMob baseGameMob = abilityTarget.CastOrGetComponent<BaseGameMob>();
			return baseGameMob != null && baseGameMob.HitPointsController.IsFinishedOffBy(abilityOwner);
		}

		// Token: 0x060020F4 RID: 8436 RVA: 0x000678EB File Offset: 0x00065AEB
		protected override bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			return true;
		}

		// Token: 0x060020F5 RID: 8437 RVA: 0x000678F1 File Offset: 0x00065AF1
		public override bool IsAllowedActivationStage(AbilityUsingStage activationStage)
		{
			return activationStage == AbilityUsingStage.PostUsed;
		}

		// Token: 0x060020F6 RID: 8438 RVA: 0x000678F7 File Offset: 0x00065AF7
		public override bool CanBeActivated(AbilityModifiersActivatorArgs args)
		{
			this.activateOnHitOnly = true;
			return base.CanBeActivated(args);
		}

		// Token: 0x060020F7 RID: 8439 RVA: 0x00067908 File Offset: 0x00065B08
		public override bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs)
		{
			modifiersUsingArgs = base.InitializeModifiersUsingArgs();
			if (this.IsAllowedActivationStage(args.abilityUsingStage))
			{
				object owner = args.ability.Owner;
				BaseAbility.UsingArgs abilityUsingArgs = args.abilityUsingArgs;
				int targetsCount = abilityUsingArgs.TargetsCount;
				int num = 0;
				if (targetsCount != 0)
				{
					IList<Component> targetsList = abilityUsingArgs.targetsList;
					for (int i = 0; i < targetsCount; i++)
					{
						if (this.IsKilledBy(targetsList[i], owner))
						{
							num++;
							if (!this.countKilledMobsAsModifiersUses)
							{
								break;
							}
						}
					}
				}
				else if (abilityUsingArgs.HasTargetObject && this.IsKilledBy(abilityUsingArgs.targetObject, owner))
				{
					num = 1;
				}
				if (num != 0)
				{
					if (this.countKilledMobsAsModifiersUses)
					{
						base.PrepareModifiersUsingArgs(args, modifiersUsingArgs, num, true);
					}
					else
					{
						base.PrepareModifiersUsingArgs(args, modifiersUsingArgs, this.defaultModifiersUsingCount, false);
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x060020F8 RID: 8440 RVA: 0x000679C9 File Offset: 0x00065BC9
		public override void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false)
		{
			this.activateOnHitOnly = true;
		}

		// Token: 0x060020F9 RID: 8441 RVA: 0x000679D2 File Offset: 0x00065BD2
		private void Reset()
		{
			this.activateOnHitOnly = true;
		}

		// Token: 0x040014A2 RID: 5282
		public int defaultModifiersUsingCount = 1;

		// Token: 0x040014A3 RID: 5283
		[Tooltip("Если активно, то добитые мобы будут учитываться как количество использований модификаторов.")]
		public bool countKilledMobsAsModifiersUses;
	}
}
