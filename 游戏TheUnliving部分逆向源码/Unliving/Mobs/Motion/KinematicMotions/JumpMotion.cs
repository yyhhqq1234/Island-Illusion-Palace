using System;
using Common.Math.Gameplay;
using UnityEngine;

namespace Unliving.Mobs.Motion.KinematicMotions
{
	// Token: 0x02000219 RID: 537
	public sealed class JumpMotion : MoveToPointMotionBase, IGameMobJumpMotion
	{
		// Token: 0x170003E8 RID: 1000
		// (get) Token: 0x06001257 RID: 4695 RVA: 0x00039E2D File Offset: 0x0003802D
		public float MaxHeight
		{
			get
			{
				return this.height;
			}
		}

		// Token: 0x170003E9 RID: 1001
		// (get) Token: 0x06001258 RID: 4696 RVA: 0x00039E35 File Offset: 0x00038035
		float IGameMobJumpMotion.MaxDistance
		{
			get
			{
				return base.Speed * this.duration;
			}
		}

		// Token: 0x06001259 RID: 4697 RVA: 0x00039E44 File Offset: 0x00038044
		public JumpMotion(GameMobMotionControllerBase motionController, object motionContext, Vector2 targetPoint, float launchVelocityY, float gravity = 10f) : base(motionController, motionContext, targetPoint, 0f)
		{
			this.gravity = gravity;
			this.launchVelocityY = launchVelocityY;
			base.Speed = FallMotion.GetLaunchVelocityX(this.MotionPath, launchVelocityY, gravity);
			this.duration = FallMotion.GetTimeOfFlight(launchVelocityY, gravity);
			this.height = FallMotion.GetMaxHeight(launchVelocityY, gravity);
		}

		// Token: 0x0600125A RID: 4698 RVA: 0x00039EA4 File Offset: 0x000380A4
		public override void Update(float t, out Vector3 velocity)
		{
			velocity = this.MotionDirection * base.Speed;
			float verticalVelocity = FallMotion.GetVerticalVelocity(this.launchVelocityY, t, this.gravity);
			velocity.y += verticalVelocity;
			velocity.z = verticalVelocity;
		}

		// Token: 0x0600125B RID: 4699 RVA: 0x00039EF2 File Offset: 0x000380F2
		public override bool BlockHitPointsModification(float t)
		{
			return t > 0.0001f;
		}

		// Token: 0x04000A79 RID: 2681
		private readonly float height;

		// Token: 0x04000A7A RID: 2682
		private readonly float gravity;

		// Token: 0x04000A7B RID: 2683
		private readonly float launchVelocityY;
	}
}
