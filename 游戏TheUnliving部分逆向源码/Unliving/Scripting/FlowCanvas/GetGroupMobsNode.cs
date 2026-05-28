using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009A RID: 154
	[Name("Get Group Mobs", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetGroupMobsNode : FlowControlNode
	{
		// Token: 0x06000412 RID: 1042 RVA: 0x0000E3F0 File Offset: 0x0000C5F0
		private void GetGroup(Flow flow)
		{
			GameObject value = this.groupObject.value;
			this.hasMobs = false;
			if (value != null)
			{
				if (this.group == null)
				{
					IGameMobGroupControllerProvider gameMobGroupControllerProvider;
					if (!value.TryGetComponent<IGameMobGroupControllerProvider>(out gameMobGroupControllerProvider))
					{
						return;
					}
					this.group = gameMobGroupControllerProvider.GroupController;
				}
				this.mobs = this.group.Mobs;
				this.hasMobs = this.group.HasMobs;
				this.flowOut.Call(flow);
			}
		}

		// Token: 0x06000413 RID: 1043 RVA: 0x0000E468 File Offset: 0x0000C668
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.GetGroup), "");
			this.groupObject = base.AddValueInput<GameObject>("groupObject", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<GameMobsGroupControllerBase>("group", () => this.group, "");
			base.AddValueOutput<IReadOnlyList<BaseGameMob>>("mobs", () => this.mobs, "");
			base.AddValueOutput<int>("mobsCount", delegate()
			{
				IReadOnlyList<BaseGameMob> readOnlyList = this.mobs;
				if (readOnlyList == null)
				{
					return 0;
				}
				return readOnlyList.Count;
			}, "");
			base.AddValueOutput<bool>("hasMobs", () => this.hasMobs, "");
		}

		// Token: 0x0400028A RID: 650
		private ValueInput<GameObject> groupObject;

		// Token: 0x0400028B RID: 651
		private FlowOutput flowOut;

		// Token: 0x0400028C RID: 652
		private GameMobsGroupControllerBase group;

		// Token: 0x0400028D RID: 653
		private IReadOnlyList<BaseGameMob> mobs;

		// Token: 0x0400028E RID: 654
		private bool hasMobs;
	}
}
