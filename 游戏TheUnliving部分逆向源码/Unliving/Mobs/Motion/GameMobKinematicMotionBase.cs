using System;
using Common;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000210 RID: 528
	public abstract class GameMobKinematicMotionBase : CustomYieldInstruction, IGameMobKinematicMotion, IProgressBasedAction
	{
		// Token: 0x170003C1 RID: 961
		// (get) Token: 0x060011C7 RID: 4551 RVA: 0x00037BBB File Offset: 0x00035DBB
		public bool IsPaused
		{
			get
			{
				return this.isPaused;
			}
		}

		// Token: 0x170003C2 RID: 962
		// (get) Token: 0x060011C8 RID: 4552 RVA: 0x00037BC3 File Offset: 0x00035DC3
		public bool IsInterrupted
		{
			get
			{
				return this.isInterrupted;
			}
		}

		// Token: 0x170003C3 RID: 963
		// (get) Token: 0x060011C9 RID: 4553 RVA: 0x00037BCB File Offset: 0x00035DCB
		public bool IsCompleted
		{
			get
			{
				return this.isCompleted;
			}
		}

		// Token: 0x170003C4 RID: 964
		// (get) Token: 0x060011CA RID: 4554 RVA: 0x00037BD4 File Offset: 0x00035DD4
		public float CurrentProgress
		{
			get
			{
				float result;
				if (this.MotionController.CurrentKinematicMotion == this && this.MotionController.TryGetKinematicMotionProgress(out result))
				{
					return result;
				}
				return 0f;
			}
		}

		// Token: 0x170003C5 RID: 965
		// (get) Token: 0x060011CB RID: 4555 RVA: 0x00037C05 File Offset: 0x00035E05
		public sealed override bool keepWaiting
		{
			get
			{
				return !this.isCompleted;
			}
		}

		// Token: 0x060011CC RID: 4556 RVA: 0x00037C10 File Offset: 0x00035E10
		public GameMobKinematicMotionBase(GameMobMotionControllerBase motionController, object motionContext)
		{
			this.MotionController = motionController;
			this.MotionContext = motionContext;
		}

		// Token: 0x060011CD RID: 4557
		protected abstract bool Start();

		// Token: 0x060011CE RID: 4558 RVA: 0x00037C26 File Offset: 0x00035E26
		protected virtual void OnMotionCompleted()
		{
		}

		// Token: 0x060011CF RID: 4559 RVA: 0x00037C28 File Offset: 0x00035E28
		public bool IsMotionStarter(BaseGameMob mob)
		{
			if (this.MotionContext != mob)
			{
				IAbility ability = this.MotionContext as IAbility;
				return ((ability != null) ? ability.Owner : null) == mob;
			}
			return true;
		}

		// Token: 0x060011D0 RID: 4560
		public abstract void Update(float t, out Vector3 velocity);

		// Token: 0x060011D1 RID: 4561 RVA: 0x00037C4F File Offset: 0x00035E4F
		public virtual bool BlockHitPointsModification(float t)
		{
			return t < 1f;
		}

		// Token: 0x060011D2 RID: 4562 RVA: 0x00037C59 File Offset: 0x00035E59
		public void PauseMotion()
		{
			this.isPaused = true;
		}

		// Token: 0x060011D3 RID: 4563 RVA: 0x00037C62 File Offset: 0x00035E62
		public void Interrupt()
		{
			if (this.isCompleted)
			{
				return;
			}
			this.isInterrupted = true;
			this.isCompleted = true;
		}

		// Token: 0x060011D4 RID: 4564 RVA: 0x00037C7B File Offset: 0x00035E7B
		bool IGameMobKinematicMotion.Start(GameMobMotionControllerBase motionController)
		{
			this.isCompleted = false;
			return this.Start();
		}

		// Token: 0x060011D5 RID: 4565 RVA: 0x00037C8A File Offset: 0x00035E8A
		void IGameMobKinematicMotion.TryResetPauseState()
		{
			this.isPaused = false;
		}

		// Token: 0x060011D6 RID: 4566 RVA: 0x00037C93 File Offset: 0x00035E93
		void IGameMobKinematicMotion.OnCompleted(GameMobMotionControllerBase motionController)
		{
			this.isCompleted = true;
			this.OnMotionCompleted();
		}

		// Token: 0x04000A2C RID: 2604
		public readonly GameMobMotionControllerBase MotionController;

		// Token: 0x04000A2D RID: 2605
		public readonly object MotionContext;

		// Token: 0x04000A2E RID: 2606
		public float duration;

		// Token: 0x04000A2F RID: 2607
		private bool isPaused;

		// Token: 0x04000A30 RID: 2608
		private bool isInterrupted;

		// Token: 0x04000A31 RID: 2609
		private bool isCompleted;
	}
}
