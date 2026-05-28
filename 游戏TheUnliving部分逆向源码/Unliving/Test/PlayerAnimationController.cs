using System;
using Common.Animation;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs.Animation;
using Unliving.Mobs.Motion;
using Unliving.Player;

namespace Unliving.Test
{
	// Token: 0x02000040 RID: 64
	public sealed class PlayerAnimationController : GameMobAnimationController, IAnimatableActionHandler
	{
		// Token: 0x06000225 RID: 549 RVA: 0x00008C01 File Offset: 0x00006E01
		private void SetCursorActionDirection()
		{
			if (!this.updateLookDirection)
			{
				this.actionDirection = default(Vector2);
				return;
			}
			this.actionDirection = this.targetMob.CurrentLookDirection;
		}

		// Token: 0x06000226 RID: 550 RVA: 0x00008C29 File Offset: 0x00006E29
		protected override Vector2 GetCurrentVelocity()
		{
			return this.playerMovementController.DesiredVelocity;
		}

		// Token: 0x06000227 RID: 551 RVA: 0x00008C38 File Offset: 0x00006E38
		protected override Vector2 UpdateMovementVisuals(Vector2 smoothedVelocity, Vector2 actualVelocity, float lookDirection, float velocityDamping = 15f)
		{
			bool flag = false;
			float value = 1f;
			GameMobKinematicMotionBase currentKinematicMotion = this.playerMovementController.CurrentKinematicMotion;
			if (currentKinematicMotion != null && (this.playerMovementController.CurrentGroundDistance > 0f || !currentKinematicMotion.IsMotionStarter(this.player)))
			{
				this.actionDirection.x = Mathf.Sign(actualVelocity.x);
			}
			else
			{
				flag = (actualVelocity.SqrMagnitude() > 0.01f);
			}
			if (flag)
			{
				Vector2 normalized = actualVelocity.normalized;
				if (this.lookAtCursorInMovingState)
				{
					this.SetCursorActionDirection();
					value = Mathf.Sign(Vector2.Dot(normalized, this.actionDirection));
				}
				else
				{
					this.actionDirection.x = normalized.x;
					this.actionDirection.y = normalized.y;
				}
			}
			else if (this.lookAtCursorInIdleState && !this.player.IsKinematic)
			{
				this.SetCursorActionDirection();
			}
			if (this.scaleAlongLookDirection && Mathf.Abs(this.actionDirection.x) > 0.7f)
			{
				lookDirection = -Mathf.Sign(this.actionDirection.x);
				if (base.GetCurrentLookDirectionRaw() != lookDirection)
				{
					Vector3 localScale = this.mobTransform.localScale;
					this.mobTransform.localScale = new Vector3(Mathf.Abs(localScale.x) * lookDirection, localScale.y, localScale.z);
				}
			}
			this._targetAnimator.SetBool(PlayerAnimationController.MovementFlagParamID, flag);
			this._targetAnimator.SetFloat(PlayerAnimationController.MovementDirParamID, value);
			base.SetMovementXParam(this.actionDirection.x);
			this._targetAnimator.SetFloat(PlayerAnimationController.MovementYParamID, this.actionDirection.y);
			return Vector2.Lerp(smoothedVelocity, actualVelocity, velocityDamping * Time.deltaTime);
		}

		// Token: 0x06000228 RID: 552 RVA: 0x00008DE0 File Offset: 0x00006FE0
		protected override void Awake()
		{
			base.Awake();
			this.player = (PlayerBehaviour)this.targetMob;
		}

		// Token: 0x06000229 RID: 553 RVA: 0x00008DF9 File Offset: 0x00006FF9
		protected override void Start()
		{
			base.Start();
			this.playerMovementController = this.player.MotionController;
		}

		// Token: 0x0600022A RID: 554 RVA: 0x00008E14 File Offset: 0x00007014
		void IAnimatableActionHandler.OnAnimatableActionActivated(object sender, string actionID, object actionData)
		{
			DashAbility dashAbility = sender as DashAbility;
			if (dashAbility != null)
			{
				Vector2 dashDirection = dashAbility.DashDirection;
				this._targetAnimator.SetFloat(PlayerAnimationController.DashXParamID, dashDirection.x);
				this._targetAnimator.SetFloat(PlayerAnimationController.DashYParamID, dashDirection.y);
			}
		}

		// Token: 0x0400013F RID: 319
		private static readonly int MovementFlagParamID = Animator.StringToHash("IsMoving");

		// Token: 0x04000140 RID: 320
		private static readonly int MovementDirParamID = Animator.StringToHash("MoveDirection");

		// Token: 0x04000141 RID: 321
		private static readonly int MovementYParamID = Animator.StringToHash("MoveY");

		// Token: 0x04000142 RID: 322
		private static readonly int DashXParamID = Animator.StringToHash("DashX");

		// Token: 0x04000143 RID: 323
		private static readonly int DashYParamID = Animator.StringToHash("DashY");

		// Token: 0x04000144 RID: 324
		public bool lookAtCursorInIdleState;

		// Token: 0x04000145 RID: 325
		public bool lookAtCursorInMovingState;

		// Token: 0x04000146 RID: 326
		public bool scaleAlongLookDirection;

		// Token: 0x04000147 RID: 327
		private PlayerBehaviour player;

		// Token: 0x04000148 RID: 328
		private GameMobMotionControllerBase playerMovementController;

		// Token: 0x04000149 RID: 329
		private Vector2 actionDirection;
	}
}
