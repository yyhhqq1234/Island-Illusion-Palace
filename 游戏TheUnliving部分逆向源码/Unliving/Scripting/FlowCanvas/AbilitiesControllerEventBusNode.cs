using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Common.UnityExtensions;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000083 RID: 131
	[Name("Abilities Controller Events", 0)]
	[Category("Unliving/Events")]
	public sealed class AbilitiesControllerEventBusNode : ObjectEventBusNodeBase
	{
		// Token: 0x0600039A RID: 922 RVA: 0x0000C3B0 File Offset: 0x0000A5B0
		private void UpdateSubscriptions(BaseGameMob mob, bool subscribe)
		{
			if (mob == null)
			{
				return;
			}
			GameAbilitiesController abilitiesController = mob.AbilitiesController;
			IPlayerAbilitiesController playerAbilitiesController = abilitiesController as IPlayerAbilitiesController;
			if (abilitiesController != null)
			{
				if (subscribe)
				{
					IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
					for (int i = 0; i < abilities.Count; i++)
					{
						this.OnAbilityAdded(abilities[i]);
					}
					abilitiesController.AbilityAdded += this.OnAbilityAdded;
					abilitiesController.AbilityRemoved += this.OnAbilityRemoved;
					abilitiesController.AbilityUsed += this.OnAbilityUsed;
					if (playerAbilitiesController != null)
					{
						playerAbilitiesController.ActiveAbilitySlotSelected += this.OnPlayerAbilitySlotSelected;
						return;
					}
				}
				else
				{
					abilitiesController.AbilityAdded -= this.OnAbilityAdded;
					abilitiesController.AbilityRemoved -= this.OnAbilityRemoved;
					abilitiesController.AbilityUsed -= this.OnAbilityUsed;
					if (playerAbilitiesController != null)
					{
						playerAbilitiesController.ActiveAbilitySlotSelected -= this.OnPlayerAbilitySlotSelected;
					}
				}
			}
		}

		// Token: 0x0600039B RID: 923 RVA: 0x0000C49C File Offset: 0x0000A69C
		private void UpdateSubscriptions(bool subscribe)
		{
			IReadOnlyList<BaseGameMob> value = this.targetMobs.value;
			if (value != null)
			{
				for (int i = 0; i < value.Count; i++)
				{
					this.UpdateSubscriptions(value[i], subscribe);
				}
				return;
			}
			this.UpdateSubscriptions(this.targetMob.value, subscribe);
		}

		// Token: 0x0600039C RID: 924 RVA: 0x0000C4EA File Offset: 0x0000A6EA
		private void UpdateLastAbility(IAbility ability)
		{
			this.lastEventAbility = (BaseAbility)ability;
		}

		// Token: 0x0600039D RID: 925 RVA: 0x0000C4F8 File Offset: 0x0000A6F8
		private void UpdateLastAbilityTarget(BaseAbility.UsingArgs usingArgs)
		{
			this.lastEventAbilityTarget = usingArgs.targetObject.CastOrGetComponent<BaseGameMob>();
		}

		// Token: 0x0600039E RID: 926 RVA: 0x0000C50C File Offset: 0x0000A70C
		private void UpdateVisibleSlotAbilityState(IAbility ability)
		{
			this.isVisibleSlotAbility = false;
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			IPlayerAbilitiesController playerAbilitiesController = ((baseGameMob != null) ? baseGameMob.AbilitiesController : null) as IPlayerAbilitiesController;
			if (playerAbilitiesController != null)
			{
				int abilitySlot = playerAbilitiesController.AbilitySlots.IndexOf(ability);
				this.isVisibleSlotAbility = !playerAbilitiesController.IsNativeAbilitySlot(abilitySlot);
			}
		}

		// Token: 0x0600039F RID: 927 RVA: 0x0000C560 File Offset: 0x0000A760
		private void OnAbilityAdded(IAbility ability)
		{
			this.UpdateLastAbility(ability);
			this.UpdateVisibleSlotAbilityState(ability);
			this.abilityAdded.Call(default(Flow));
		}

		// Token: 0x060003A0 RID: 928 RVA: 0x0000C590 File Offset: 0x0000A790
		private void OnAbilityRemoved(IAbility ability)
		{
			this.UpdateLastAbility(ability);
			this.UpdateVisibleSlotAbilityState(ability);
			this.abilityRemoved.Call(default(Flow));
		}

		// Token: 0x060003A1 RID: 929 RVA: 0x0000C5C0 File Offset: 0x0000A7C0
		private void OnPlayerAbilitySlotSelected(IPlayerAbilitiesController abilitiesController, int slot, BaseAbility.UsingArgs usingArgs, BaseAbility.ActivationErrorType error)
		{
			IAbility ability = abilitiesController.AbilitySlots[slot];
			this.activationError = error;
			this.UpdateLastAbility(ability);
			this.UpdateLastAbilityTarget(usingArgs);
			this.UpdateVisibleSlotAbilityState(ability);
			this.playerAbilitySlotSelected.Call(default(Flow));
		}

		// Token: 0x060003A2 RID: 930 RVA: 0x0000C60C File Offset: 0x0000A80C
		private void OnAbilityUsed(IAbility ability, object args)
		{
			BaseAbility.UsingArgs usingArgs = (BaseAbility.UsingArgs)args;
			this.abilityUsingFlow = default(Flow);
			this.UpdateLastAbility(ability);
			this.UpdateLastAbilityTarget(usingArgs);
			this.UpdateVisibleSlotAbilityState(ability);
			this.abilityUsed.Call(this.abilityUsingFlow);
			usingArgs.ProcessTargets(new Action<Component>(this.OnAbilityUsedOnTarget));
		}

		// Token: 0x060003A3 RID: 931 RVA: 0x0000C664 File Offset: 0x0000A864
		private void OnAbilityUsedOnTarget(Component abilityTarget)
		{
			BaseGameMob x = abilityTarget.CastOrGetComponent<BaseGameMob>();
			if (x == null)
			{
				return;
			}
			this.lastAbilityAffectedMob = x;
			this.abilityUsedOnTarget.Call(this.abilityUsingFlow);
			if (this.lastEventAbility.IsPostMortemAbility)
			{
				this.postMortemAbilityUsedOnTarget.Call(this.abilityUsingFlow);
			}
		}

		// Token: 0x060003A4 RID: 932 RVA: 0x0000C6B8 File Offset: 0x0000A8B8
		protected override GameObject GetEventsSourceObject()
		{
			if (this.targetMobs != null)
			{
				return null;
			}
			BaseGameMob value = this.targetMob.value;
			if (value == null)
			{
				return null;
			}
			return value.gameObject;
		}

		// Token: 0x060003A5 RID: 933 RVA: 0x0000C6DC File Offset: 0x0000A8DC
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.targetMobs = base.AddValueInput<IReadOnlyList<BaseGameMob>>("targetMobs", "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityAdded = base.AddFlowOutput("abilityAdded", "");
			this.abilityRemoved = base.AddFlowOutput("abilityRemoved", "");
			this.playerAbilitySlotSelected = base.AddFlowOutput("playerAbilitySlotSelected", "");
			this.abilityUsed = base.AddFlowOutput("abilityUsed", "");
			this.abilityUsedOnTarget = base.AddFlowOutput("abilityUsedOnTarget", "");
			this.postMortemAbilityUsedOnTarget = base.AddFlowOutput("postMortemAbilityUsedOnTarget", "");
			base.AddValueOutput<BaseAbility.ActivationErrorType>("activationError", () => this.activationError, "");
			base.AddValueOutput<BaseAbility>("lastEventAbility", () => this.lastEventAbility, "");
			base.AddValueOutput<BaseGameMob>("lastEventAbilityTarget", () => this.lastEventAbilityTarget, "");
			base.AddValueOutput<bool>("isVisibleSlotAbility", () => this.isVisibleSlotAbility, "");
			base.AddValueOutput<BaseGameMob>("lastAbilityAffectedMob", () => this.lastAbilityAffectedMob, "");
		}

		// Token: 0x060003A6 RID: 934 RVA: 0x0000C830 File Offset: 0x0000AA30
		protected override void OnInitialize(Flow flow)
		{
			this.UpdateSubscriptions(true);
		}

		// Token: 0x060003A7 RID: 935 RVA: 0x0000C839 File Offset: 0x0000AA39
		protected override void OnFinalize()
		{
			this.UpdateSubscriptions(false);
		}

		// Token: 0x04000224 RID: 548
		private ValueInput<IReadOnlyList<BaseGameMob>> targetMobs;

		// Token: 0x04000225 RID: 549
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000226 RID: 550
		private FlowOutput abilityAdded;

		// Token: 0x04000227 RID: 551
		private FlowOutput abilityRemoved;

		// Token: 0x04000228 RID: 552
		private FlowOutput playerAbilitySlotSelected;

		// Token: 0x04000229 RID: 553
		private FlowOutput abilityUsed;

		// Token: 0x0400022A RID: 554
		private FlowOutput abilityUsedOnTarget;

		// Token: 0x0400022B RID: 555
		private FlowOutput postMortemAbilityUsedOnTarget;

		// Token: 0x0400022C RID: 556
		private Flow abilityUsingFlow;

		// Token: 0x0400022D RID: 557
		private BaseAbility.ActivationErrorType activationError;

		// Token: 0x0400022E RID: 558
		private BaseAbility lastEventAbility;

		// Token: 0x0400022F RID: 559
		private BaseGameMob lastEventAbilityTarget;

		// Token: 0x04000230 RID: 560
		private bool isVisibleSlotAbility;

		// Token: 0x04000231 RID: 561
		private BaseGameMob lastAbilityAffectedMob;
	}
}
