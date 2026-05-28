using System;
using UnityEngine;

namespace Unliving.Mobs.Motion.KinematicMotions
{
	// Token: 0x0200021B RID: 539
	public abstract class MoveToPointMotionBase : GameMobKinematicMotionBase
	{
		// Token: 0x170003EA RID: 1002
		// (get) Token: 0x0600125F RID: 4703 RVA: 0x00039F2A File Offset: 0x0003812A
		// (set) Token: 0x06001260 RID: 4704 RVA: 0x00039F32 File Offset: 0x00038132
		public float Speed { get; protected set; }

		// Token: 0x06001261 RID: 4705 RVA: 0x00039F3B File Offset: 0x0003813B
		protected MoveToPointMotionBase(GameMobMotionControllerBase motionController, object motionContext) : base(motionController, motionContext)
		{
		}

		// Token: 0x06001262 RID: 4706 RVA: 0x00039F45 File Offset: 0x00038145
		public MoveToPointMotionBase(GameMobMotionControllerBase motionController, object motionContext, Vector2 targetPoint, float speed) : base(motionController, motionContext)
		{
			this.Speed = speed;
			if (this.GetMotionDirection(targetPoint, out this.MotionDirection, out this.MotionPath) && speed > 0f)
			{
				this.duration = this.MotionPath / speed;
			}
		}

		// Token: 0x06001263 RID: 4707 RVA: 0x00039F84 File Offset: 0x00038184
		protected bool GetMotionDirection(Vector2 targetPoint, out Vector2 direction, out float distance)
		{
			direction = targetPoint - this.MotionController.ControllerOwner.Position;
			distance = direction.magnitude;
			if (distance < 0.001f)
			{
				return false;
			}
			direction /= distance;
			return true;
		}

		// Token: 0x06001264 RID: 4708 RVA: 0x00039FD4 File Offset: 0x000381D4
		protected override bool Start()
		{
			return this.MotionDirection.x != 0f || this.MotionDirection.y != 0f;
		}

		// Token: 0x04000A7D RID: 2685
		protected readonly Vector2 MotionDirection;

		// Token: 0x04000A7E RID: 2686
		protected readonly float MotionPath;
	}
}
