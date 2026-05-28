using System;
using Game.Damage;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001E3 RID: 483
	public sealed class MobDecayController : IHitPointsSource, IDisposable
	{
		// Token: 0x17000321 RID: 801
		// (get) Token: 0x06000FBB RID: 4027 RVA: 0x000316F0 File Offset: 0x0002F8F0
		public float InitialHitPoints
		{
			get
			{
				return this.initialHitPoints;
			}
		}

		// Token: 0x17000322 RID: 802
		// (get) Token: 0x06000FBC RID: 4028 RVA: 0x000316F8 File Offset: 0x0002F8F8
		public float CurrentHitPoints
		{
			get
			{
				return this.currentHitPoints;
			}
		}

		// Token: 0x17000323 RID: 803
		// (get) Token: 0x06000FBD RID: 4029 RVA: 0x00031700 File Offset: 0x0002F900
		public float CurrentDecayProgress
		{
			get
			{
				return 1f - Mathf.Clamp01(this.currentHitPoints / this.initialHitPoints);
			}
		}

		// Token: 0x17000324 RID: 804
		// (get) Token: 0x06000FBE RID: 4030 RVA: 0x0003171A File Offset: 0x0002F91A
		// (set) Token: 0x06000FBF RID: 4031 RVA: 0x00031722 File Offset: 0x0002F922
		public float DecaySpeed
		{
			get
			{
				return this.decaySpeed;
			}
			set
			{
				this.decaySpeed = value;
			}
		}

		// Token: 0x17000325 RID: 805
		// (get) Token: 0x06000FC0 RID: 4032 RVA: 0x0003172B File Offset: 0x0002F92B
		// (set) Token: 0x06000FC1 RID: 4033 RVA: 0x00031733 File Offset: 0x0002F933
		IHitPointsSource IHitPointsSource.ParentHitPointsSource
		{
			get
			{
				return this.parentHitPointsSource;
			}
			set
			{
				this.SetParentHitPointsSource(value);
			}
		}

		// Token: 0x140000AD RID: 173
		// (add) Token: 0x06000FC2 RID: 4034 RVA: 0x0003173C File Offset: 0x0002F93C
		// (remove) Token: 0x06000FC3 RID: 4035 RVA: 0x00031774 File Offset: 0x0002F974
		public event Action<IHitPointsSource, object, IHitPointsChangingArgs> BeforeHitPointsChanged;

		// Token: 0x140000AE RID: 174
		// (add) Token: 0x06000FC4 RID: 4036 RVA: 0x000317AC File Offset: 0x0002F9AC
		// (remove) Token: 0x06000FC5 RID: 4037 RVA: 0x000317E4 File Offset: 0x0002F9E4
		public event Action<IHitPointsSource, object, IHitPointsChangingArgs> HitPointsChanged;

		// Token: 0x06000FC6 RID: 4038 RVA: 0x0003181C File Offset: 0x0002FA1C
		private void ResetInitialHitPoints()
		{
			if (this.parentHitPointsSource == null)
			{
				return;
			}
			float num = this.parentHitPointsSource.InitialHitPoints;
			float relativeDecaySpeed = this.decayParams.RelativeDecaySpeed;
			this.initialHitPoints = num * this.decayParams.decayPointsRatio;
			this.decaySpeed = ((relativeDecaySpeed > 0f) ? (num * relativeDecaySpeed) : this.decayParams.decaySpeed);
		}

		// Token: 0x06000FC7 RID: 4039 RVA: 0x0003187B File Offset: 0x0002FA7B
		private void SetParentHitPointsSource(IHitPointsSource hitPointsSource)
		{
			if (this.parentHitPointsSource == hitPointsSource)
			{
				return;
			}
			this.parentHitPointsSource = hitPointsSource;
			this.parentDamageReceiver = (hitPointsSource as IDamageable);
			this.ResetInitialHitPoints();
			this.currentHitPoints = this.initialHitPoints;
			this.ResetLastDecayTime();
		}

		// Token: 0x06000FC8 RID: 4040 RVA: 0x000318B4 File Offset: 0x0002FAB4
		private void SyncParentHitPoints()
		{
			if (this.parentDamageReceiver == null)
			{
				return;
			}
			float num = this.parentDamageReceiver.CurrentHitPoints - this.currentHitPoints;
			if (num > 0f)
			{
				MobDecayController.DamageArgs.amount = num;
				this.parentDamageReceiver.ModifyHitPoints(this.CurrentMob, MobDecayController.DamageArgs);
			}
		}

		// Token: 0x06000FC9 RID: 4041 RVA: 0x00031907 File Offset: 0x0002FB07
		private bool IsDecayBlockedByGameSession()
		{
			return this.sessionManager != null && this.sessionManager.CurrentSessionState > SessionState.InProgress;
		}

		// Token: 0x06000FCA RID: 4042 RVA: 0x00031921 File Offset: 0x0002FB21
		private void ResetLastDecayTime()
		{
			this.lastDecayTime = Time.time;
		}

		// Token: 0x06000FCB RID: 4043 RVA: 0x00031930 File Offset: 0x0002FB30
		public MobDecayController(BaseGameMob currentMob, MobHealthController.DecayControllerParams decayParams, Func<BaseGameMob, bool> isActiveDelegate, float updateStep = 0.1f)
		{
			currentMob.CurrentGame.Services.TryGet<IGameSessionManager>(out this.sessionManager);
			this.CurrentMob = currentMob;
			this.decayParams = decayParams;
			this.updateStep = updateStep;
			this.SetParentHitPointsSource(currentMob.HitPointsController);
			this.activityCondition = isActiveDelegate;
		}

		// Token: 0x06000FCC RID: 4044 RVA: 0x00031984 File Offset: 0x0002FB84
		internal void UpdateInitialHitPoints()
		{
			float num = this.currentHitPoints / this.initialHitPoints;
			this.ResetInitialHitPoints();
			this.currentHitPoints = this.initialHitPoints * num;
		}

		// Token: 0x06000FCD RID: 4045 RVA: 0x000319B4 File Offset: 0x0002FBB4
		public void SetDecayProgress(float decayProgress)
		{
			float num = (1f - Mathf.Clamp01(decayProgress)) * this.initialHitPoints;
			if (this.currentHitPoints == num)
			{
				return;
			}
			this.currentHitPoints = num;
			this.SyncParentHitPoints();
			HitPointsController.HPChangingArgs arg = new HitPointsController.HPChangingArgs(false)
			{
				amount = num - this.currentHitPoints,
				isSilentChanging = true,
				disableTargetReaction = true,
				isForcedChanging = true
			};
			Action<IHitPointsSource, object, IHitPointsChangingArgs> hitPointsChanged = this.HitPointsChanged;
			if (hitPointsChanged == null)
			{
				return;
			}
			hitPointsChanged(this, this.CurrentMob, arg);
		}

		// Token: 0x06000FCE RID: 4046 RVA: 0x00031A2E File Offset: 0x0002FC2E
		public void ResetDecay()
		{
			this.SetDecayProgress(0f);
		}

		// Token: 0x06000FCF RID: 4047 RVA: 0x00031A3B File Offset: 0x0002FC3B
		public float ModifyHitPoints(object sender, IHitPointsChangingArgs args)
		{
			return 0f;
		}

		// Token: 0x06000FD0 RID: 4048 RVA: 0x00031A44 File Offset: 0x0002FC44
		public void Update()
		{
			if (this.currentHitPoints == 0f || this.decaySpeed <= 0f || this.CurrentMob.IsKinematic || this.IsDecayBlockedByGameSession() || (this.activityCondition != null && !this.activityCondition(this.CurrentMob)))
			{
				this.ResetLastDecayTime();
				return;
			}
			float num = Time.time - this.lastDecayTime;
			if (num < this.updateStep)
			{
				return;
			}
			float num2 = this.decaySpeed * num;
			Action<IHitPointsSource, object, IHitPointsChangingArgs> beforeHitPointsChanged = this.BeforeHitPointsChanged;
			if (beforeHitPointsChanged != null)
			{
				beforeHitPointsChanged(this, this.CurrentMob, null);
			}
			this.currentHitPoints -= num2;
			if (this.currentHitPoints <= 0f)
			{
				this.currentHitPoints = 0f;
			}
			this.SyncParentHitPoints();
			MobDecayController.DamageArgs.amount = num2;
			Action<IHitPointsSource, object, IHitPointsChangingArgs> hitPointsChanged = this.HitPointsChanged;
			if (hitPointsChanged != null)
			{
				hitPointsChanged(this, this.CurrentMob, MobDecayController.DamageArgs);
			}
			this.ResetLastDecayTime();
		}

		// Token: 0x06000FD1 RID: 4049 RVA: 0x00031B35 File Offset: 0x0002FD35
		public void Dispose()
		{
			this.parentHitPointsSource = null;
			this.parentDamageReceiver = null;
			this.activityCondition = null;
		}

		// Token: 0x0400091E RID: 2334
		private static readonly HitPointsController.HPChangingArgs DamageArgs = new HitPointsController.HPChangingArgs(true)
		{
			isSilentChanging = true,
			disableTargetReaction = true
		};

		// Token: 0x04000921 RID: 2337
		public readonly BaseGameMob CurrentMob;

		// Token: 0x04000922 RID: 2338
		private readonly MobHealthController.DecayControllerParams decayParams;

		// Token: 0x04000923 RID: 2339
		private readonly IGameSessionManager sessionManager;

		// Token: 0x04000924 RID: 2340
		private readonly float updateStep;

		// Token: 0x04000925 RID: 2341
		private Func<BaseGameMob, bool> activityCondition;

		// Token: 0x04000926 RID: 2342
		private IHitPointsSource parentHitPointsSource;

		// Token: 0x04000927 RID: 2343
		private IDamageable parentDamageReceiver;

		// Token: 0x04000928 RID: 2344
		private float initialHitPoints;

		// Token: 0x04000929 RID: 2345
		private float currentHitPoints;

		// Token: 0x0400092A RID: 2346
		private float decaySpeed;

		// Token: 0x0400092B RID: 2347
		private float lastDecayTime;
	}
}
