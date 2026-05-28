using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000BC RID: 188
	[Name("Modify HP", 0)]
	[Category("Unliving/Mobs")]
	public sealed class ModifyHPNode : FlowControlNode
	{
		// Token: 0x060004C1 RID: 1217 RVA: 0x000111F4 File Offset: 0x0000F3F4
		private void ModifyHP(Flow flow)
		{
			float num = this.amount.value;
			this.realAmount = 0f;
			if (num != 0f && this.targetMob.value != null)
			{
				IDamageable hitPointsController = this.targetMob.value.HitPointsController;
				if (hitPointsController != null)
				{
					if (this.isContinuous.value)
					{
						num *= Time.deltaTime;
					}
					HitPointsController.HPChangingArgs hpchangingArgs = (num > 0f) ? ModifyHPNode.HealArgs : ModifyHPNode.DamageArgs;
					hpchangingArgs.amount = Mathf.Abs(num);
					hpchangingArgs.isVitalEnergyExchange = this.isVitalEnergyExchange.value;
					this.realAmount = hitPointsController.ModifyHitPoints(this.hitPointsChanger.value ?? base.graphAgent, hpchangingArgs);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004C2 RID: 1218 RVA: 0x000112C0 File Offset: 0x0000F4C0
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ModifyHP), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.hitPointsChanger = base.AddValueInput<object>("hitPointsChanger", "");
			this.amount = base.AddValueInput<float>("amount", "");
			this.isContinuous = base.AddValueInput<bool>("isContinuous", "");
			this.isVitalEnergyExchange = base.AddValueInput<bool>("isVitalEnergyExchange", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<float>("realAmount", () => this.realAmount, "");
		}

		// Token: 0x04000328 RID: 808
		private static readonly HitPointsController.HPChangingArgs DamageArgs = new HitPointsController.HPChangingArgs(true);

		// Token: 0x04000329 RID: 809
		private static readonly HitPointsController.HPChangingArgs HealArgs = new HitPointsController.HPChangingArgs(false);

		// Token: 0x0400032A RID: 810
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x0400032B RID: 811
		private ValueInput<object> hitPointsChanger;

		// Token: 0x0400032C RID: 812
		private ValueInput<float> amount;

		// Token: 0x0400032D RID: 813
		private ValueInput<bool> isContinuous;

		// Token: 0x0400032E RID: 814
		private ValueInput<bool> isVitalEnergyExchange;

		// Token: 0x0400032F RID: 815
		private FlowOutput flowOut;

		// Token: 0x04000330 RID: 816
		private float realAmount;
	}
}
