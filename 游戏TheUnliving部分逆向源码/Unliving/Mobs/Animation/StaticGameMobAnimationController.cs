using System;
using Common.Animation;
using UnityEngine;

namespace Unliving.Mobs.Animation
{
	// Token: 0x0200021F RID: 543
	public sealed class StaticGameMobAnimationController : CommonAnimationController
	{
		// Token: 0x0600129B RID: 4763 RVA: 0x0003B159 File Offset: 0x00039359
		private void OnMobKilled(IGameMob killedMob)
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.deathStateAnimatorDisabler.DisableAnimator(this, new int?(StaticGameMobAnimationController.DeathStateFlagParamID), null);
		}

		// Token: 0x0600129C RID: 4764 RVA: 0x0003B180 File Offset: 0x00039380
		private void Start()
		{
			if (base.TargetAnimator == null || !base.TargetAnimator.TryGetComponent<StaticGameMob>(out this.targetMob))
			{
				base.enabled = false;
				return;
			}
			this.updateLookDirection = this.targetMob.CanAim;
			this.deathStateAnimatorDisabler = new AnimatorDisabler(this._targetAnimator, StaticGameMobAnimationController.DeathStateTagID);
			this.targetMob.Killed += this.OnMobKilled;
		}

		// Token: 0x0600129D RID: 4765 RVA: 0x0003B1F4 File Offset: 0x000393F4
		private void Update()
		{
			if (this.updateLookDirection)
			{
				Vector2 currentLookDirection = this.targetMob.CurrentLookDirection;
				this._targetAnimator.SetFloat(StaticGameMobAnimationController.LookDirectionXParamID, currentLookDirection.x);
				this._targetAnimator.SetFloat(StaticGameMobAnimationController.LookDirectionYParamID, currentLookDirection.y);
			}
		}

		// Token: 0x0600129E RID: 4766 RVA: 0x0003B241 File Offset: 0x00039441
		private void OnDisable()
		{
			if (this.deathStateAnimatorDisabler.InProgress && !base.gameObject.activeInHierarchy)
			{
				this._targetAnimator.enabled = false;
			}
		}

		// Token: 0x0600129F RID: 4767 RVA: 0x0003B269 File Offset: 0x00039469
		private void OnDestroy()
		{
			if (this.targetMob != null)
			{
				this.targetMob.Killed -= this.OnMobKilled;
			}
		}

		// Token: 0x04000ABF RID: 2751
		private static readonly int LookDirectionXParamID = Animator.StringToHash("LookDirectionX");

		// Token: 0x04000AC0 RID: 2752
		private static readonly int LookDirectionYParamID = Animator.StringToHash("LookDirectionY");

		// Token: 0x04000AC1 RID: 2753
		private static readonly int DeathStateFlagParamID = Animator.StringToHash("Death");

		// Token: 0x04000AC2 RID: 2754
		private static readonly int DeathStateTagID = Animator.StringToHash("DeathState");

		// Token: 0x04000AC3 RID: 2755
		private StaticGameMob targetMob;

		// Token: 0x04000AC4 RID: 2756
		private bool updateLookDirection;

		// Token: 0x04000AC5 RID: 2757
		private AnimatorDisabler deathStateAnimatorDisabler;
	}
}
