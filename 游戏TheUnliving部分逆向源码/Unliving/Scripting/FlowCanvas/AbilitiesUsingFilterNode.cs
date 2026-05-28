using System;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000084 RID: 132
	[Name("Abilities Using Filter", 0)]
	[Category("Unliving/Mobs")]
	public sealed class AbilitiesUsingFilterNode : GameContextDependentNodeBase
	{
		// Token: 0x060003AE RID: 942 RVA: 0x0000C874 File Offset: 0x0000AA74
		private void SetActive(bool isActive, Flow flow)
		{
			if (this.isActive == isActive)
			{
				return;
			}
			BaseGameMob value = this.targetMob.value;
			if (value == null)
			{
				return;
			}
			GameMobAIController aicontroller = value.AIController;
			if (aicontroller == null)
			{
				return;
			}
			GameAbilitiesController abilitiesController = value.AbilitiesController;
			if (isActive)
			{
				AbilityDescription value2 = this.abilityDescription.value;
				if (!value2.IsBlank())
				{
					if (abilitiesController != null && this.addAbilityIfNeeded.value)
					{
						BaseAbility abilityPrototypeReference = value2.abilityPrototypeReference;
						AbilityID abilityID = value2.abilityID;
						if (abilityPrototypeReference != null)
						{
							if (!abilitiesController.HasAbilityWithPrototype(abilityPrototypeReference))
							{
								IGameAbilitiesFactory gameAbilitiesFactory = base.CurrentGame.Services.Get<IGameAbilitiesFactory>();
								this.addedAbility = (BaseAbility)gameAbilitiesFactory.Create(new AbilityFactoryArgs
								{
									abilityPrototype = abilityPrototypeReference
								});
								if (!abilitiesController.AddAbility(this.addedAbility))
								{
									this.addedAbility = null;
								}
							}
						}
						else if (abilityID != AbilityID.None && !abilitiesController.HasAbilityWithID(abilityID))
						{
							(this.addedAbility = abilitiesController.AddAbility(new AbilityInfo(abilityID))) == null;
						}
						value2.abilityReference = this.addedAbility;
					}
					aicontroller.SetExclusivelyUsingAbilitiesDescription(value2);
				}
			}
			else
			{
				aicontroller.ResetExclusivelyUsingAbilitiesDescription();
				if (abilitiesController != null && this.addedAbility != null)
				{
					abilitiesController.RemoveAbility(this.addedAbility);
					this.addedAbility = null;
				}
			}
			this.isActive = isActive;
			this.flowOut.Call(flow);
		}

		// Token: 0x060003AF RID: 943 RVA: 0x0000C9D4 File Offset: 0x0000ABD4
		private void Activate(Flow flow)
		{
			this.SetActive(true, flow);
		}

		// Token: 0x060003B0 RID: 944 RVA: 0x0000C9DE File Offset: 0x0000ABDE
		private void Deactivate(Flow flow)
		{
			this.SetActive(false, flow);
		}

		// Token: 0x060003B1 RID: 945 RVA: 0x0000C9E8 File Offset: 0x0000ABE8
		protected override void RegisterPorts()
		{
			base.AddFlowInput("activate", new FlowHandler(this.Activate), "");
			base.AddFlowInput("deactivate", new FlowHandler(this.Deactivate), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityDescription = base.AddValueInput<AbilityDescription>("abilityDescription", "");
			this.abilityDescription.SetDefaultAndSerializedValue(AbilityDescription.BlankDescription);
			this.addAbilityIfNeeded = base.AddValueInput<bool>("addAbilityIfNeeded", "");
			this.addAbilityIfNeeded.SetDefaultAndSerializedValue(true);
			base.AddValueOutput<bool>("isActive", () => this.isActive, "");
			base.AddValueOutput<BaseAbility>("abilityPrototype", () => this.abilityDescription.value.abilityPrototypeReference, "");
			base.AddValueOutput<AbilityID>("abilityID", () => this.abilityDescription.value.abilityID, "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000232 RID: 562
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000233 RID: 563
		private ValueInput<AbilityDescription> abilityDescription;

		// Token: 0x04000234 RID: 564
		private ValueInput<bool> addAbilityIfNeeded;

		// Token: 0x04000235 RID: 565
		private FlowOutput flowOut;

		// Token: 0x04000236 RID: 566
		private BaseAbility addedAbility;

		// Token: 0x04000237 RID: 567
		private bool isActive;
	}
}
