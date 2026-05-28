using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B1 RID: 177
	public abstract class KinematicMotionNodeBase : FlowControlNode
	{
		// Token: 0x06000482 RID: 1154 RVA: 0x0000FDD0 File Offset: 0x0000DFD0
		private void StartMotion(Flow flow)
		{
			BaseGameMob value = this.targetMob.value;
			GameMobMotionControllerBase gameMobMotionControllerBase = (value != null) ? value.MotionController : null;
			if (gameMobMotionControllerBase != null)
			{
				Transform value2 = this.finalPositionPivot.value;
				Vector2 vector = (value2 != null) ? value2.position : this.finalPosition.value;
				GameMobKinematicMotionBase gameMobKinematicMotionBase = this.CreateMotion(gameMobMotionControllerBase, vector);
				if (gameMobKinematicMotionBase != null && gameMobMotionControllerBase.StartKinematicMotion(gameMobKinematicMotionBase, true))
				{
					this.currentMotion = gameMobKinematicMotionBase;
					this.motionStarted.Call(flow);
					gameMobMotionControllerBase.KinematicMotionCompleted += this.OnKinematicMotionCompleted;
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000483 RID: 1155 RVA: 0x0000FE6C File Offset: 0x0000E06C
		private void OnKinematicMotionCompleted(GameMobKinematicMotionBase motion)
		{
			if (this.currentMotion == motion)
			{
				GameMobMotionControllerBase motionController = motion.MotionController;
				if (!motionController.ControllerOwner.IsKilled)
				{
					this.motionCompleted.Call(default(Flow));
				}
				motionController.KinematicMotionCompleted -= this.OnKinematicMotionCompleted;
				this.currentMotion = null;
			}
		}

		// Token: 0x06000484 RID: 1156
		protected abstract GameMobKinematicMotionBase CreateMotion(GameMobMotionControllerBase motionController, Vector2 finalPosition);

		// Token: 0x06000485 RID: 1157 RVA: 0x0000FEC4 File Offset: 0x0000E0C4
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.StartMotion), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.finalPosition = base.AddValueInput<Vector2>("finalPosition", "");
			this.finalPositionPivot = base.AddValueInput<Transform>("finalPositionPivot", "");
			this.flowOut = base.AddFlowOutput("", "");
			this.motionStarted = base.AddFlowOutput("motionStarted", "");
			this.motionCompleted = base.AddFlowOutput("motionCompleted", "");
		}

		// Token: 0x040002E4 RID: 740
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x040002E5 RID: 741
		private ValueInput<Vector2> finalPosition;

		// Token: 0x040002E6 RID: 742
		private ValueInput<Transform> finalPositionPivot;

		// Token: 0x040002E7 RID: 743
		private FlowOutput flowOut;

		// Token: 0x040002E8 RID: 744
		private FlowOutput motionStarted;

		// Token: 0x040002E9 RID: 745
		private FlowOutput motionCompleted;

		// Token: 0x040002EA RID: 746
		private GameMobKinematicMotionBase currentMotion;
	}
}
