using System;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C0 RID: 192
	[Name("Remove Ability", 0)]
	[Category("Unliving/Abilities")]
	public sealed class RemoveAbilityNode : CallableActionNode<BaseGameMob, BaseAbility, BaseAbility, bool, AbilityID, int>
	{
		// Token: 0x060004D7 RID: 1239 RVA: 0x00011815 File Offset: 0x0000FA15
		private void ResetAbilitiesUsingFilter(BaseGameMob targetMob)
		{
			GameMobAIController aicontroller = targetMob.AIController;
			if (aicontroller == null)
			{
				return;
			}
			aicontroller.ResetExclusivelyUsingAbilitiesDescription();
		}

		// Token: 0x060004D8 RID: 1240 RVA: 0x00011828 File Offset: 0x0000FA28
		public override void Invoke(BaseGameMob targetMob, BaseAbility abilityInstance, BaseAbility abilityPrototype, bool resetAbilitiesUsingFilter = true, AbilityID abilityID = AbilityID.None, int abilityLevel = -1)
		{
			if (targetMob == null)
			{
				return;
			}
			GameAbilitiesController gameAbilitiesController;
			if (!ScriptingUtility.TryGetAbilitiesController(targetMob, out gameAbilitiesController))
			{
				return;
			}
			if (abilityInstance != null && gameAbilitiesController.RemoveAbility(abilityInstance) && resetAbilitiesUsingFilter)
			{
				this.ResetAbilitiesUsingFilter(targetMob);
			}
			if (abilityPrototype != null)
			{
				if (gameAbilitiesController.RemoveAbilityWithPrototype(abilityPrototype) && resetAbilitiesUsingFilter)
				{
					this.ResetAbilitiesUsingFilter(targetMob);
					return;
				}
			}
			else if (abilityID != AbilityID.None)
			{
				AbilityInfo abilityDescription = new AbilityInfo(abilityID, abilityLevel);
				if (gameAbilitiesController.RemoveAbility(abilityDescription) && resetAbilitiesUsingFilter)
				{
					this.ResetAbilitiesUsingFilter(targetMob);
				}
			}
		}
	}
}
