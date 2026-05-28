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
	// Token: 0x02000089 RID: 137
	[Name("Block Abilities", 0)]
	[Category("Unliving/Mobs")]
	public sealed class BlockAbilitiesNode : FlowControlNode
	{
		// Token: 0x060003C5 RID: 965 RVA: 0x0000D085 File Offset: 0x0000B285
		private void ResetAbilitiesUsingFilter(BaseGameMob targetMob)
		{
			if (!this.resetAbilitiesUsingFilter.value)
			{
				return;
			}
			GameMobAIController aicontroller = targetMob.AIController;
			if (aicontroller == null)
			{
				return;
			}
			aicontroller.ResetExclusivelyUsingAbilitiesDescription();
		}

		// Token: 0x060003C6 RID: 966 RVA: 0x0000D0A8 File Offset: 0x0000B2A8
		private void SetBlockActive(Flow flow, bool isActive)
		{
			BaseGameMob value = this.targetMob.value;
			if (value == null)
			{
				return;
			}
			GameAbilitiesController abilitiesController = value.AbilitiesController;
			IReadOnlyList<BaseAbility> readOnlyList = (abilitiesController != null) ? abilitiesController.Abilities : null;
			if (readOnlyList == null)
			{
				return;
			}
			this.abilitiesFilter.abilityPrototypeReference = this.abilityPrototype.value;
			this.abilitiesFilter.abilityID = this.abilityID.value;
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				BaseAbility baseAbility = readOnlyList[i];
				if (this.abilitiesFilter.IsMatch(baseAbility))
				{
					if (isActive)
					{
						baseAbility.Complete();
					}
					baseAbility.SetBlocked(isActive);
				}
			}
			this.ResetAbilitiesUsingFilter(value);
			this.flowOut.Call(flow);
		}

		// Token: 0x060003C7 RID: 967 RVA: 0x0000D158 File Offset: 0x0000B358
		private void ActivateBlock(Flow flow)
		{
			this.SetBlockActive(flow, true);
		}

		// Token: 0x060003C8 RID: 968 RVA: 0x0000D162 File Offset: 0x0000B362
		private void DeactivateBlock(Flow flow)
		{
			this.SetBlockActive(flow, false);
		}

		// Token: 0x060003C9 RID: 969 RVA: 0x0000D16C File Offset: 0x0000B36C
		protected override void RegisterPorts()
		{
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.abilityPrototype = base.AddValueInput<BaseAbility>("abilityPrototype", "");
			this.abilityID = base.AddValueInput<AbilityID>("abilityID", "");
			this.resetAbilitiesUsingFilter = base.AddValueInput<bool>("resetAbilitiesUsingFilter", "");
			this.resetAbilitiesUsingFilter.SetDefaultAndSerializedValue(true);
			base.AddFlowInput("activateBlock", new FlowHandler(this.ActivateBlock), "");
			base.AddFlowInput("deactivateBlock", new FlowHandler(this.DeactivateBlock), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400024A RID: 586
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x0400024B RID: 587
		private ValueInput<BaseAbility> abilityPrototype;

		// Token: 0x0400024C RID: 588
		private ValueInput<AbilityID> abilityID;

		// Token: 0x0400024D RID: 589
		private ValueInput<bool> resetAbilitiesUsingFilter;

		// Token: 0x0400024E RID: 590
		private FlowOutput flowOut;

		// Token: 0x0400024F RID: 591
		private AbilityDescription abilitiesFilter = AbilityDescription.BlankDescription;
	}
}
