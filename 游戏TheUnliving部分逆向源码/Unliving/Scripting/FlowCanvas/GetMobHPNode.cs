using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009D RID: 157
	[Name("Get Mob HP", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetMobHPNode : FlowControlNode
	{
		// Token: 0x06000424 RID: 1060 RVA: 0x0000E8B0 File Offset: 0x0000CAB0
		private void Update(Flow flow)
		{
			this.maxHP = 0f;
			this.currentHP = 0f;
			this.normalizedHP = 0f;
			this.lackOfHP = 0f;
			this.hpContainersCount = 0;
			if (this.targetMob.value != null)
			{
				IDamageable hitPointsController = this.targetMob.value.HitPointsController;
				if (hitPointsController != null)
				{
					this.maxHP = hitPointsController.MaxHitPoints;
					this.currentHP = hitPointsController.CurrentHitPoints;
					this.normalizedHP = ((this.maxHP > 0f) ? (this.currentHP / this.maxHP) : 0f);
					this.lackOfHP = hitPointsController.HitPointsLack;
					IContainerBasedHPController containerBasedHPController = hitPointsController as IContainerBasedHPController;
					if (containerBasedHPController != null)
					{
						this.hpContainersCount = containerBasedHPController.CurrentSlotsCount;
					}
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000425 RID: 1061 RVA: 0x0000E984 File Offset: 0x0000CB84
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<float>("maxHP", () => this.maxHP, "");
			base.AddValueOutput<float>("currentHP", () => this.currentHP, "");
			base.AddValueOutput<float>("normalizedHP", () => this.normalizedHP, "");
			base.AddValueOutput<float>("lackOfHP", () => this.lackOfHP, "");
			base.AddValueOutput<int>("hpContainersCount", () => this.hpContainersCount, "");
		}

		// Token: 0x0400029B RID: 667
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x0400029C RID: 668
		private FlowOutput flowOut;

		// Token: 0x0400029D RID: 669
		private float maxHP;

		// Token: 0x0400029E RID: 670
		private float currentHP;

		// Token: 0x0400029F RID: 671
		private float normalizedHP;

		// Token: 0x040002A0 RID: 672
		private float lackOfHP;

		// Token: 0x040002A1 RID: 673
		private int hpContainersCount;
	}
}
