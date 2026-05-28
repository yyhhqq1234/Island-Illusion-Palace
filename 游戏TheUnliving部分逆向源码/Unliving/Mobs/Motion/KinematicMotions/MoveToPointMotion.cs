using System;
using UnityEngine;

namespace Unliving.Mobs.Motion.KinematicMotions
{
	// Token: 0x0200021A RID: 538
	public sealed class MoveToPointMotion : MoveToPointMotionBase
	{
		// Token: 0x0600125C RID: 4700 RVA: 0x00039EFC File Offset: 0x000380FC
		public MoveToPointMotion(GameMobMotionControllerBase motionController, object motionContext, Vector2 targetPoint, float speed) : base(motionController, motionContext, targetPoint, speed)
		{
		}

		// Token: 0x0600125D RID: 4701 RVA: 0x00039F09 File Offset: 0x00038109
		public override void Update(float t, out Vector3 velocity)
		{
			velocity = this.MotionDirection * base.Speed;
		}

		// Token: 0x0600125E RID: 4702 RVA: 0x00039F27 File Offset: 0x00038127
		public override bool BlockHitPointsModification(float t)
		{
			return false;
		}
	}
}
