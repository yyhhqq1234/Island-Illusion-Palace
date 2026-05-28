using System;
using System.Collections.Generic;
using System.Linq;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AE RID: 174
	[Name("Get Activatable HP Containers", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetActivatableHPContainersNode : FlowControlNode
	{
		// Token: 0x0600046F RID: 1135 RVA: 0x0000F934 File Offset: 0x0000DB34
		private static AbilityID[] GetContainerAbilities(IAbilityActivatedContainer container)
		{
			IReadOnlyList<BaseAbility> abilities = container.GetAbilities();
			AbilityID[] array = new AbilityID[abilities.Count];
			for (int i = 0; i < abilities.Count; i++)
			{
				array[i] = (AbilityID)abilities[i].ID;
			}
			return array;
		}

		// Token: 0x06000470 RID: 1136 RVA: 0x0000F978 File Offset: 0x0000DB78
		private void Update(Flow flow)
		{
			IGameMob value = this.targetMob.value;
			IAbilityActivatedContainersController abilityActivatedContainersController = ((value != null) ? value.HitPointsController : null) as IAbilityActivatedContainersController;
			if (this.containersController != abilityActivatedContainersController)
			{
				if (abilityActivatedContainersController != null)
				{
					abilityActivatedContainersController.AbilityActivatedContainerDestroyed += this.OnContainerDestroyed;
				}
				if (this.containersController != null)
				{
					this.containersController.AbilityActivatedContainerDestroyed -= this.OnContainerDestroyed;
				}
				this.containersController = abilityActivatedContainersController;
				IAbilityActivatedContainersController abilityActivatedContainersController2 = this.containersController;
				this.currentContainers = (IList<IAbilityActivatedContainer>)((abilityActivatedContainersController2 != null) ? abilityActivatedContainersController2.GetContainers() : null);
				this.lastContainerAbilities = Array.Empty<AbilityID>();
				this.lastActivatedContainerAbilities = Array.Empty<AbilityID>();
				if (this.currentContainers != null && this.currentContainers.Count != 0)
				{
					this.lastContainerAbilities = GetActivatableHPContainersNode.GetContainerAbilities(this.currentContainers.Last<IAbilityActivatedContainer>());
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000471 RID: 1137 RVA: 0x0000FA54 File Offset: 0x0000DC54
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.targetMob = base.AddValueInput<IGameMob>("targetMob", "");
			base.AddValueOutput<IList<IAbilityActivatedContainer>>("currentContainers", () => this.currentContainers, "");
			base.AddValueOutput<AbilityID[]>("lastContainerAbilities", () => this.lastContainerAbilities, "");
			base.AddValueOutput<AbilityID[]>("lastActivatedContainerAbilities", () => this.lastActivatedContainerAbilities, "");
			base.AddValueOutput<bool>("wasActivated", () => this.lastActivatedContainerAbilities != null && this.lastActivatedContainerAbilities.Length != 0, "");
			this.flowOut = base.AddFlowOutput("", "");
			this.containerDestroyed = base.AddFlowOutput("containerDestroyed", "");
		}

		// Token: 0x06000472 RID: 1138 RVA: 0x0000FB34 File Offset: 0x0000DD34
		private void OnContainerDestroyed(IAbilityActivatedContainer container)
		{
			this.lastActivatedContainerAbilities = GetActivatableHPContainersNode.GetContainerAbilities(container);
			this.containerDestroyed.Call(default(Flow));
		}

		// Token: 0x040002D5 RID: 725
		private ValueInput<IGameMob> targetMob;

		// Token: 0x040002D6 RID: 726
		private FlowOutput flowOut;

		// Token: 0x040002D7 RID: 727
		private FlowOutput containerDestroyed;

		// Token: 0x040002D8 RID: 728
		private IAbilityActivatedContainersController containersController;

		// Token: 0x040002D9 RID: 729
		private IList<IAbilityActivatedContainer> currentContainers;

		// Token: 0x040002DA RID: 730
		private AbilityID[] lastContainerAbilities;

		// Token: 0x040002DB RID: 731
		private AbilityID[] lastActivatedContainerAbilities;
	}
}
