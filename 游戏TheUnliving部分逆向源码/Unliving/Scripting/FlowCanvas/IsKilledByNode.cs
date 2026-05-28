using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A4 RID: 164
	[Name("Is Killed By", 0)]
	[Category("Unliving/Mobs")]
	public sealed class IsKilledByNode : FlowControlNode
	{
		// Token: 0x06000450 RID: 1104 RVA: 0x0000F284 File Offset: 0x0000D484
		private void Update(Flow flow)
		{
			this.isKilledBy = false;
			if (this.killer.value != null && this.targetMob.value != null)
			{
				HitPointsController hitPointsController = this.targetMob.value.HitPointsController as HitPointsController;
				if (hitPointsController != null)
				{
					this.isKilledBy = (!hitPointsController.IsAlive && hitPointsController.LastDamageSender == this.killer.value);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000451 RID: 1105 RVA: 0x0000F308 File Offset: 0x0000D508
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.Update), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.killer = base.AddValueInput<object>("killer", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<bool>("isKilledBy", () => this.isKilledBy, "");
		}

		// Token: 0x040002BF RID: 703
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002C0 RID: 704
		private ValueInput<object> killer;

		// Token: 0x040002C1 RID: 705
		private FlowOutput flowOut;

		// Token: 0x040002C2 RID: 706
		private bool isKilledBy;
	}
}
