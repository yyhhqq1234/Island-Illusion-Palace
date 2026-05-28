using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009B RID: 155
	[Name("Get HP State", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetHPStateNode : FlowControlNode
	{
		// Token: 0x06000419 RID: 1049 RVA: 0x0000E568 File Offset: 0x0000C768
		private void Update(Flow flow)
		{
			this.hasEnoughHP = false;
			this.containerWillBeDestroyed = false;
			this.currentHP = 0f;
			this.lackOfHP = 0f;
			if (this.targetMob.value != null)
			{
				IDamageable hitPointsController = this.targetMob.value.HitPointsController;
				if (hitPointsController != null)
				{
					this.currentHP = hitPointsController.CurrentHitPoints;
					this.lackOfHP = Mathf.Max(Mathf.Abs(this.requiredAmount.value) - this.currentHP, 0f);
					this.hasEnoughHP = (this.lackOfHP == 0f);
					IContainerBasedHPController containerBasedHPController = hitPointsController as IContainerBasedHPController;
					if (containerBasedHPController != null && containerBasedHPController.MaxSlotsCount - containerBasedHPController.EmptySlotsCount > 1)
					{
						IEnergyContainer currentHealthContainer = containerBasedHPController.CurrentHealthContainer;
						this.containerWillBeDestroyed = (currentHealthContainer.CurrentEnergyAmount <= this.requiredAmount.value);
					}
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x0600041A RID: 1050 RVA: 0x0000E658 File Offset: 0x0000C858
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.requiredAmount = base.AddValueInput<float>("requiredAmount", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<float>("currentHP", () => this.currentHP, "");
			base.AddValueOutput<float>("lackOfHP", () => this.lackOfHP, "");
			base.AddValueOutput<bool>("hasEnoughHP", () => this.hasEnoughHP, "");
			base.AddValueOutput<bool>("containerWillBeDestroyed", () => this.containerWillBeDestroyed, "");
		}

		// Token: 0x0400028F RID: 655
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000290 RID: 656
		private ValueInput<float> requiredAmount;

		// Token: 0x04000291 RID: 657
		private FlowOutput flowOut;

		// Token: 0x04000292 RID: 658
		private float currentHP;

		// Token: 0x04000293 RID: 659
		private float lackOfHP;

		// Token: 0x04000294 RID: 660
		private bool hasEnoughHP;

		// Token: 0x04000295 RID: 661
		private bool containerWillBeDestroyed;
	}
}
