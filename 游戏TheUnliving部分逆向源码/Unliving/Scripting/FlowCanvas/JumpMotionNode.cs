using System;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.Mobs.Motion.KinematicMotions;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B0 RID: 176
	[Name("Jump Motion", 0)]
	[Category("Unliving/Mobs/Motion")]
	public sealed class JumpMotionNode : KinematicMotionNodeBase
	{
		// Token: 0x0600047F RID: 1151 RVA: 0x0000FD20 File Offset: 0x0000DF20
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.jumpSpeed = base.AddValueInput<float>("jumpSpeed", "");
			this.jumpSpeed.SetDefaultAndSerializedValue(10f);
			this.gravityOverride = base.AddValueInput<float>("gravityOverride", "");
		}

		// Token: 0x06000480 RID: 1152 RVA: 0x0000FD70 File Offset: 0x0000DF70
		protected override GameMobKinematicMotionBase CreateMotion(GameMobMotionControllerBase motionController, Vector2 finalPosition)
		{
			float value = this.jumpSpeed.value;
			if (value <= 0f)
			{
				return null;
			}
			BaseGameMob controllerOwner = motionController.ControllerOwner;
			float value2 = this.gravityOverride.value;
			if (value2 > 0f)
			{
				return new JumpMotion(motionController, controllerOwner, finalPosition, value, value2);
			}
			return new JumpMotion(motionController, controllerOwner, finalPosition, value, 10f);
		}

		// Token: 0x040002E2 RID: 738
		private ValueInput<float> jumpSpeed;

		// Token: 0x040002E3 RID: 739
		private ValueInput<float> gravityOverride;
	}
}
