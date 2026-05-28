using System;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.Mobs.Motion.KinematicMotions;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B3 RID: 179
	[Name("Move Motion", 0)]
	[Category("Unliving/Mobs/Motion")]
	public sealed class MoveMotionNode : KinematicMotionNodeBase
	{
		// Token: 0x0600048A RID: 1162 RVA: 0x0001006A File Offset: 0x0000E26A
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.speedOverride = base.AddValueInput<float>("speedOverride", "");
		}

		// Token: 0x0600048B RID: 1163 RVA: 0x00010088 File Offset: 0x0000E288
		protected override GameMobKinematicMotionBase CreateMotion(GameMobMotionControllerBase motionController, Vector2 finalPosition)
		{
			BaseGameMob controllerOwner = motionController.ControllerOwner;
			float value = this.speedOverride.value;
			float speed = controllerOwner.Speed;
			return new MoveToPointMotion(motionController, controllerOwner, finalPosition, (value > 0f) ? value : speed);
		}

		// Token: 0x040002EF RID: 751
		private ValueInput<float> speedOverride;
	}
}
