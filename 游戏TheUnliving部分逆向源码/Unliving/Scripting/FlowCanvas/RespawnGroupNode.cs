using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C1 RID: 193
	[Name("Respawn Group", 0)]
	[Category("Unliving/Mobs")]
	public sealed class RespawnGroupNode : FlowControlNode
	{
		// Token: 0x060004DA RID: 1242 RVA: 0x000118B0 File Offset: 0x0000FAB0
		private void RespawnGroup(Flow flow)
		{
			GameObject value = this.spawnerObject.value;
			if (value == null)
			{
				return;
			}
			if (this.cachedSpawner != null || value.TryGetComponent<MobBehaviourSpawner>(out this.cachedSpawner))
			{
				this.cachedSpawner.TryStartGroupRespawn(new Action<MobBehaviourSpawner>(this.OnRespawnStarted), new Action<MobBehaviourSpawner>(this.OnRespawnCompleted));
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004DB RID: 1243 RVA: 0x00011920 File Offset: 0x0000FB20
		private void OnRespawnStarted(MobBehaviourSpawner spawner)
		{
			this.respawnStarted.Call(default(Flow));
		}

		// Token: 0x060004DC RID: 1244 RVA: 0x00011944 File Offset: 0x0000FB44
		private void OnRespawnCompleted(MobBehaviourSpawner spawner)
		{
			this.respawnCompleted.Call(default(Flow));
		}

		// Token: 0x060004DD RID: 1245 RVA: 0x00011968 File Offset: 0x0000FB68
		protected override void RegisterPorts()
		{
			this.spawnerObject = base.AddValueInput<GameObject>("spawnerObject", "");
			base.AddFlowInput("", new FlowHandler(this.RespawnGroup), "");
			this.flowOut = base.AddFlowOutput("", "");
			this.respawnStarted = base.AddFlowOutput("respawnStarted", "");
			this.respawnCompleted = base.AddFlowOutput("respawnCompleted", "");
		}

		// Token: 0x04000342 RID: 834
		private ValueInput<GameObject> spawnerObject;

		// Token: 0x04000343 RID: 835
		private FlowOutput flowOut;

		// Token: 0x04000344 RID: 836
		private FlowOutput respawnStarted;

		// Token: 0x04000345 RID: 837
		private FlowOutput respawnCompleted;

		// Token: 0x04000346 RID: 838
		private MobBehaviourSpawner cachedSpawner;
	}
}
