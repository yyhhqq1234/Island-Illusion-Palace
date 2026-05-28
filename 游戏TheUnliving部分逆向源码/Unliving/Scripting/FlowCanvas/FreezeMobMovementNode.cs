using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AD RID: 173
	[Name("Freeze Mob Movement", 0)]
	[Category("Unliving/Mobs/Motion")]
	public sealed class FreezeMobMovementNode : FlowControlNode, IUpdatable, IGraphElement
	{
		// Token: 0x0600046B RID: 1131 RVA: 0x0000F790 File Offset: 0x0000D990
		private void StartMovementFreeze(Flow flow)
		{
			this.freezeCompletionTime = null;
			BaseGameMob value = this.targetMob.value;
			if (((value != null) ? value.MotionController : null) != null)
			{
				this.freezeCompletionTime = new float?(Time.time + this.freezeDuration.value);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x0600046C RID: 1132 RVA: 0x0000F7EC File Offset: 0x0000D9EC
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.StartMovementFreeze), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.freezeDuration = base.AddValueInput<float>("freezeDuration", "");
			this.freezeDuration.SetDefaultAndSerializedValue(5f);
			this.makeMobFullyStatic = base.AddValueInput<bool>("makeMobFullyStatic", "");
			this.flowOut = base.AddFlowOutput("", "");
			this.freezeCompleted = base.AddFlowOutput("freezeCompleted", "");
		}

		// Token: 0x0600046D RID: 1133 RVA: 0x0000F898 File Offset: 0x0000DA98
		void IUpdatable.Update()
		{
			if (this.freezeCompletionTime != null)
			{
				BaseGameMob value = this.targetMob.value;
				if (value != null && !value.IsKilled)
				{
					float time = Time.time;
					float? num = this.freezeCompletionTime;
					if (time < num.GetValueOrDefault() & num != null)
					{
						value.MotionController.FreezeMovement(0f, this.makeMobFullyStatic.value);
						return;
					}
					this.freezeCompleted.Call(default(Flow));
					this.freezeCompletionTime = null;
				}
			}
		}

		// Token: 0x040002CF RID: 719
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002D0 RID: 720
		private ValueInput<float> freezeDuration;

		// Token: 0x040002D1 RID: 721
		private ValueInput<bool> makeMobFullyStatic;

		// Token: 0x040002D2 RID: 722
		private FlowOutput flowOut;

		// Token: 0x040002D3 RID: 723
		private FlowOutput freezeCompleted;

		// Token: 0x040002D4 RID: 724
		private float? freezeCompletionTime;
	}
}
