using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000092 RID: 146
	[Name("On Fixed Interval Update", 0)]
	[Category("Unliving/Events")]
	public sealed class OnFixedIntervalUpdate : EventNode, IUpdatable, IGraphElement
	{
		// Token: 0x060003EB RID: 1003 RVA: 0x0000D990 File Offset: 0x0000BB90
		protected override void RegisterPorts()
		{
			this.updateInterval = base.AddValueInput<float>("updateInterval", "");
			this.updateInterval.SetDefaultAndSerializedValue(1f);
			this.update = base.AddFlowOutput("", "");
		}

		// Token: 0x060003EC RID: 1004 RVA: 0x0000D9D0 File Offset: 0x0000BBD0
		void IUpdatable.Update()
		{
			if (!base.graph.isRunning)
			{
				return;
			}
			float time = Time.time;
			if (time > this.nextUpdateTime)
			{
				this.nextUpdateTime = time + this.updateInterval.value;
				this.update.Call(default(Flow));
			}
		}

		// Token: 0x0400026A RID: 618
		private ValueInput<float> updateInterval;

		// Token: 0x0400026B RID: 619
		private FlowOutput update;

		// Token: 0x0400026C RID: 620
		private float nextUpdateTime;
	}
}
