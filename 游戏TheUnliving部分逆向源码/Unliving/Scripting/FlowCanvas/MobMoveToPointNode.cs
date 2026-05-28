using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000AC RID: 172
	[Name("Move To Point", 0)]
	[Category("Unliving/Mobs")]
	public class MobMoveToPointNode : FlowControlNode
	{
		// Token: 0x06000465 RID: 1125 RVA: 0x0000F584 File Offset: 0x0000D784
		private Vector2 GetDestinationPoint()
		{
			Transform value = this.targetPointPivot.value;
			if (!(value != null))
			{
				return this.targetPoint.value;
			}
			return value.position;
		}

		// Token: 0x06000466 RID: 1126 RVA: 0x0000F5C0 File Offset: 0x0000D7C0
		private void SetMobTargetPoint(Flow flow)
		{
			BaseGameMob value = this.targetMob.value;
			if (value == null)
			{
				return;
			}
			GameMobMotionController gameMobMotionController = value.MotionController as GameMobMotionController;
			if (gameMobMotionController != null)
			{
				GameMobGroupController gameMobGroupController = value.Group as GameMobGroupController;
				if (gameMobGroupController != null)
				{
					gameMobGroupController.SetForcedGroupDestination(this.GetDestinationPoint(), null);
					gameMobMotionController.GroupDestinationReached += this.OnGroupDestinationReached;
					value.Killed += this.OnTargetMobKilled;
					this.flowOut.Call(flow);
				}
			}
		}

		// Token: 0x06000467 RID: 1127 RVA: 0x0000F640 File Offset: 0x0000D840
		private void OnGroupDestinationReached(GameMobMotionController motionController)
		{
			motionController.GroupDestinationReached -= this.OnGroupDestinationReached;
			Vector2? currentDestination = motionController.CurrentDestination;
			Vector2 destinationPoint = this.GetDestinationPoint();
			if (currentDestination == null || (currentDestination != null && currentDestination.GetValueOrDefault() != destinationPoint))
			{
				return;
			}
			this.pointReached.Call(default(Flow));
			motionController.LeaveReachedGroupDestination();
		}

		// Token: 0x06000468 RID: 1128 RVA: 0x0000F6B0 File Offset: 0x0000D8B0
		private void OnTargetMobKilled(IGameMob mob)
		{
			mob.Killed -= this.OnTargetMobKilled;
			GameMobMotionController gameMobMotionController = mob.MotionController as GameMobMotionController;
			if (gameMobMotionController != null)
			{
				gameMobMotionController.GroupDestinationReached -= this.OnGroupDestinationReached;
			}
		}

		// Token: 0x06000469 RID: 1129 RVA: 0x0000F6F0 File Offset: 0x0000D8F0
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SetMobTargetPoint), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.targetPointPivot = base.AddValueInput<Transform>("targetPointPivot", "");
			this.targetPoint = base.AddValueInput<Vector2>("targetPoint", "");
			this.flowOut = base.AddFlowOutput("", "");
			this.pointReached = base.AddFlowOutput("pointReached", "");
		}

		// Token: 0x040002CA RID: 714
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002CB RID: 715
		private ValueInput<Transform> targetPointPivot;

		// Token: 0x040002CC RID: 716
		private ValueInput<Vector2> targetPoint;

		// Token: 0x040002CD RID: 717
		private FlowOutput flowOut;

		// Token: 0x040002CE RID: 718
		private FlowOutput pointReached;
	}
}
