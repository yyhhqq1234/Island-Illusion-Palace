using System;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000085 RID: 133
	[Name("Add Ability", 0)]
	[Category("Unliving/Abilities")]
	public sealed class AddAbilityNode : GameContextDependentNodeBase
	{
		// Token: 0x060003B6 RID: 950 RVA: 0x0000CB30 File Offset: 0x0000AD30
		private void UpdateOutValue(Flow flow, BaseAbility addedAbility)
		{
			this.addedAbility = addedAbility;
			this.flowOut.Call(flow);
		}

		// Token: 0x060003B7 RID: 951 RVA: 0x0000CB48 File Offset: 0x0000AD48
		private void AddAbility(Flow flow)
		{
			BaseGameMob value = this.targetMob.value;
			GameAbilitiesController gameAbilitiesController;
			if (value != null && ScriptingUtility.TryGetAbilitiesController(value, out gameAbilitiesController))
			{
				BaseAbility value2 = this.abilityPrototype.value;
				AbilityID value3 = this.abilityID.value;
				int value4 = this.abilityLevel.value;
				AbilitySpecialBehaviourDescription value5 = this.specialBehaviourDescription.value;
				if (value2 != null)
				{
					AddAbilityNode.AbilitiesFactoryArgs.abilityPrototype = value2;
					AddAbilityNode.AbilitiesFactoryArgs.abilityLevel = value4;
					AddAbilityNode.AbilitiesFactoryArgs.specialBehaviourDescription = value5;
					IGameAbilitiesFactory gameAbilitiesFactory = base.CurrentGame.Services.Get<IGameAbilitiesFactory>();
					BaseAbility baseAbility = (BaseAbility)((gameAbilitiesFactory != null) ? gameAbilitiesFactory.Create(AddAbilityNode.AbilitiesFactoryArgs) : null);
					if (baseAbility != null && gameAbilitiesController.AddAbility(baseAbility))
					{
						this.UpdateOutValue(flow, baseAbility);
						return;
					}
				}
				else if (value3 != AbilityID.None)
				{
					AbilityInfo abilityDescription = new AbilityInfo(value3, value4)
					{
						specialBehaviourDescription = value5
					};
					BaseAbility x = gameAbilitiesController.AddAbility(abilityDescription);
					if (x != null)
					{
						this.UpdateOutValue(flow, x);
					}
				}
			}
		}

		// Token: 0x060003B8 RID: 952 RVA: 0x0000CC58 File Offset: 0x0000AE58
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.AddAbility), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityPrototype = base.AddValueInput<BaseAbility>("abilityPrototype", "");
			this.abilityID = base.AddValueInput<AbilityID>("abilityID", "");
			this.abilityID.SetDefaultAndSerializedValue(AbilityID.None);
			this.abilityLevel = base.AddValueInput<int>("abilityLevel", "");
			this.abilityLevel.SetDefaultAndSerializedValue(1);
			this.specialBehaviourDescription = base.AddValueInput<AbilitySpecialBehaviourDescription>("specialBehaviourDescription", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<BaseAbility>("Added Ability", () => this.addedAbility, "");
		}

		// Token: 0x04000238 RID: 568
		private static readonly AbilityFactoryArgs AbilitiesFactoryArgs = new AbilityFactoryArgs
		{
			canGenerateBuffs = true
		};

		// Token: 0x04000239 RID: 569
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x0400023A RID: 570
		private ValueInput<BaseAbility> abilityPrototype;

		// Token: 0x0400023B RID: 571
		private ValueInput<AbilityID> abilityID;

		// Token: 0x0400023C RID: 572
		private ValueInput<int> abilityLevel;

		// Token: 0x0400023D RID: 573
		private ValueInput<AbilitySpecialBehaviourDescription> specialBehaviourDescription;

		// Token: 0x0400023E RID: 574
		private FlowOutput flowOut;

		// Token: 0x0400023F RID: 575
		private BaseAbility addedAbility;
	}
}
