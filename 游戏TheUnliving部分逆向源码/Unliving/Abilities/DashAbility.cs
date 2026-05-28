using System;
using Common.Animation;
using Common.Math.Gameplay;
using Game.Abilities;
using UnityEngine;
using Unliving.GameSettings;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000380 RID: 896
	[CreateAssetMenu(fileName = "DashAbility", menuName = "Abilities/Dash Ability")]
	public sealed class DashAbility : BaseAbility
	{
		// Token: 0x17000612 RID: 1554
		// (get) Token: 0x06001D88 RID: 7560 RVA: 0x0005DCFA File Offset: 0x0005BEFA
		// (set) Token: 0x06001D89 RID: 7561 RVA: 0x0005DD02 File Offset: 0x0005BF02
		public override int ID { get; set; }

		// Token: 0x17000613 RID: 1555
		// (get) Token: 0x06001D8A RID: 7562 RVA: 0x0005DD0B File Offset: 0x0005BF0B
		// (set) Token: 0x06001D8B RID: 7563 RVA: 0x0005DD13 File Offset: 0x0005BF13
		public override int Type
		{
			get
			{
				return (int)this.abilityType;
			}
			set
			{
				this.abilityType = (AbilityTypes)value;
			}
		}

		// Token: 0x17000614 RID: 1556
		// (get) Token: 0x06001D8C RID: 7564 RVA: 0x0005DD1C File Offset: 0x0005BF1C
		public override bool IsTargetedAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000615 RID: 1557
		// (get) Token: 0x06001D8D RID: 7565 RVA: 0x0005DD1F File Offset: 0x0005BF1F
		public override bool IsObjectTargetRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000616 RID: 1558
		// (get) Token: 0x06001D8E RID: 7566 RVA: 0x0005DD22 File Offset: 0x0005BF22
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000617 RID: 1559
		// (get) Token: 0x06001D8F RID: 7567 RVA: 0x0005DD25 File Offset: 0x0005BF25
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000618 RID: 1560
		// (get) Token: 0x06001D90 RID: 7568 RVA: 0x0005DD28 File Offset: 0x0005BF28
		public override bool CanBeUsed
		{
			get
			{
				return this.dashDistance > 0f && this.movableOwner != null;
			}
		}

		// Token: 0x17000619 RID: 1561
		// (get) Token: 0x06001D91 RID: 7569 RVA: 0x0005DD45 File Offset: 0x0005BF45
		public override bool IsContinuous
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700061A RID: 1562
		// (get) Token: 0x06001D92 RID: 7570 RVA: 0x0005DD48 File Offset: 0x0005BF48
		public Vector2 DashDirection
		{
			get
			{
				return this.dashDirection;
			}
		}

		// Token: 0x1700061B RID: 1563
		// (get) Token: 0x06001D93 RID: 7571 RVA: 0x0005DD50 File Offset: 0x0005BF50
		public Vector2 DashImpulse
		{
			get
			{
				return this.dashImpulse;
			}
		}

		// Token: 0x06001D94 RID: 7572 RVA: 0x0005DD58 File Offset: 0x0005BF58
		private float GetCursorDashDirectionAngle()
		{
			if (this.inputSettings != null && !this.inputSettings.smartDash)
			{
				return 0f;
			}
			return this.cursorDashDirectionAngle;
		}

		// Token: 0x06001D95 RID: 7573 RVA: 0x0005DD7B File Offset: 0x0005BF7B
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorSource)
		{
			if (this.movableOwner != null && this.movableOwner.MotionController.IsFullyStatic)
			{
				return BaseAbility.ActivationErrorType.Internal;
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorSource);
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x0005DDA8 File Offset: 0x0005BFA8
		protected override void OnPreparing(BaseAbility.UsingArgs usingArgs)
		{
			this.dashDirection = usingArgs.targetPosition - base.OwnerPosition;
			Vector3 inputDirection = usingArgs.inputDirection;
			float num = Vector2.Angle(this.dashDirection, inputDirection);
			if (inputDirection.sqrMagnitude > 0.01f && num > this.GetCursorDashDirectionAngle())
			{
				this.dashDirection = inputDirection;
			}
			this.dashDirection.Normalize();
			float impulseDrag = this.movableOwner.GetImpulseDrag();
			this.dashImpulse = MotionUtils.GetTargetImpulse(this.dashDirection, this.dashDistance, impulseDrag);
			float num2 = Vector2.Dot(this.dashImpulse, this.dashDirection);
			this.UsingDuration = ((num2 > 0f) ? MotionUtils.GetImpulseDampingTime(num2, impulseDrag) : 0f);
			IAnimatableActionHandler animatableActionHandler = this.movableOwner.AnimationController as IAnimatableActionHandler;
			if (animatableActionHandler != null)
			{
				animatableActionHandler.OnAnimatableActionActivated(this, "dash", null);
			}
			base.OnPreparing(usingArgs);
		}

		// Token: 0x06001D97 RID: 7575 RVA: 0x0005DE9F File Offset: 0x0005C09F
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			if (base.WasUsed)
			{
				return;
			}
			this.movableOwner.AddMovementImpulse(this.dashImpulse, true);
		}

		// Token: 0x06001D98 RID: 7576 RVA: 0x0005DEC1 File Offset: 0x0005C0C1
		protected override void Reset()
		{
			base.Reset();
			this.UsingDuration = 0f;
			this.dashImpulse = default(Vector2);
		}

		// Token: 0x06001D99 RID: 7577 RVA: 0x0005DEE0 File Offset: 0x0005C0E0
		protected override void OnInitialize(object context)
		{
			base.OnInitialize(context);
			base.UsingLoopStep = 0f;
		}

		// Token: 0x06001D9A RID: 7578 RVA: 0x0005DEF4 File Offset: 0x0005C0F4
		protected override void OnOwnerChanged(object lastOwner, object newOwner)
		{
			base.OnOwnerChanged(lastOwner, newOwner);
			this.movableOwner = (newOwner as BaseGameMob);
			GameSettingsManager gameSettingsManager;
			if (this.movableOwner != null && this.movableOwner.CurrentGame.Services.TryGet<GameSettingsManager>(out gameSettingsManager))
			{
				this.inputSettings = gameSettingsManager.CurrentState.inputData;
			}
		}

		// Token: 0x040010B7 RID: 4279
		public AbilityTypes abilityType;

		// Token: 0x040010B8 RID: 4280
		public float dashDistance;

		// Token: 0x040010B9 RID: 4281
		public float cursorDashDirectionAngle = 90f;

		// Token: 0x040010BA RID: 4282
		private BaseGameMob movableOwner;

		// Token: 0x040010BB RID: 4283
		private Vector2 dashDirection;

		// Token: 0x040010BC RID: 4284
		private Vector2 dashImpulse;

		// Token: 0x040010BD RID: 4285
		private GameSettingsState.InputData inputSettings;
	}
}
