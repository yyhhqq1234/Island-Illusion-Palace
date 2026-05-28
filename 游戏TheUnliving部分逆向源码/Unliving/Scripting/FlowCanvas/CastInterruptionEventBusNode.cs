using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000091 RID: 145
	[Name("Cast Interruption Events", 0)]
	[Category("Unliving/Events")]
	public sealed class CastInterruptionEventBusNode : EventNode
	{
		// Token: 0x060003E4 RID: 996 RVA: 0x0000D7FC File Offset: 0x0000B9FC
		private void Initialize(Flow flow)
		{
			BaseGameMob value = this.targetMob.value;
			GameAbilitiesController gameAbilitiesController = (value != null) ? value.AbilitiesController : null;
			if (gameAbilitiesController != null)
			{
				IReadOnlyList<BaseAbility> abilities = gameAbilitiesController.Abilities;
				for (int i = 0; i < abilities.Count; i++)
				{
					AbilityPreparationResistanceController abilityPreparationResistanceController;
					if (abilities[i].TryGetExtension(out abilityPreparationResistanceController))
					{
						abilityPreparationResistanceController.OtherAbilityCastInterrupted += this.OnOtherAbilityCastInterrupted;
						abilityPreparationResistanceController.RemovedFromAbility += this.OnPrepResistanceControllerRemovedFromAbility;
					}
				}
			}
		}

		// Token: 0x060003E5 RID: 997 RVA: 0x0000D870 File Offset: 0x0000BA70
		private void OnOtherAbilityCastInterrupted(AbilityPreparationResistanceController prepResistanceController, BaseAbility ability)
		{
			this.castInterrupter = (prepResistanceController.CurrentAbility.Owner as BaseGameMob);
			this.interruptedCastAbility = ability;
			this.otherAbilityCastInterrupted.Call(default(Flow));
			this.castInterrupter = null;
			this.interruptedCastAbility = null;
		}

		// Token: 0x060003E6 RID: 998 RVA: 0x0000D8BC File Offset: 0x0000BABC
		private void OnPrepResistanceControllerRemovedFromAbility(IAbility ability, IAbilityExtension extension)
		{
			AbilityPreparationResistanceController abilityPreparationResistanceController = (AbilityPreparationResistanceController)extension;
			abilityPreparationResistanceController.OtherAbilityCastInterrupted -= this.OnOtherAbilityCastInterrupted;
			abilityPreparationResistanceController.RemovedFromAbility -= this.OnPrepResistanceControllerRemovedFromAbility;
		}

		// Token: 0x060003E7 RID: 999 RVA: 0x0000D8E8 File Offset: 0x0000BAE8
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Initialize), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.otherAbilityCastInterrupted = base.AddFlowOutput("otherAbilityCastInterrupted", "");
			base.AddValueOutput<BaseGameMob>("castInterrupter", () => this.castInterrupter, "");
			base.AddValueOutput<BaseAbility>("interruptedCastAbility", () => this.interruptedCastAbility, "");
		}

		// Token: 0x04000266 RID: 614
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000267 RID: 615
		private FlowOutput otherAbilityCastInterrupted;

		// Token: 0x04000268 RID: 616
		private BaseGameMob castInterrupter;

		// Token: 0x04000269 RID: 617
		private BaseAbility interruptedCastAbility;
	}
}
