using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.PassiveAbilities;
using Game.Stats;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Abilities;
using Unliving.Mobs.Motion;
using Unliving.Mobs.VFX;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001FF RID: 511
	public sealed class PushableGameMob : BaseGameMob
	{
		// Token: 0x17000385 RID: 901
		// (get) Token: 0x06001102 RID: 4354 RVA: 0x0003545C File Offset: 0x0003365C
		// (set) Token: 0x06001103 RID: 4355 RVA: 0x00035464 File Offset: 0x00033664
		public bool IsActive { get; private set; }

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x06001104 RID: 4356 RVA: 0x0003546D File Offset: 0x0003366D
		public override bool IsCharacter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x06001105 RID: 4357 RVA: 0x00035470 File Offset: 0x00033670
		public override GameMobMotionControllerBase MotionController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x06001106 RID: 4358 RVA: 0x00035473 File Offset: 0x00033673
		public override GameMobAIController AIController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x06001107 RID: 4359 RVA: 0x00035476 File Offset: 0x00033676
		public override GameAbilitiesController AbilitiesController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x06001108 RID: 4360 RVA: 0x00035479 File Offset: 0x00033679
		public override BasePassiveAbilitiesController PassiveAbilitiesController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x06001109 RID: 4361 RVA: 0x0003547C File Offset: 0x0003367C
		public override StatsControllerBase<MobStatModifier> StatsController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x0600110A RID: 4362 RVA: 0x0003547F File Offset: 0x0003367F
		protected override bool CanGenerateResources
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600110B RID: 4363 RVA: 0x00035484 File Offset: 0x00033684
		protected override void OnMobInitialization()
		{
			base.OnMobInitialization();
			if (this.AttackTargetsProvider == null)
			{
				this.AttackTargetsProvider = base.GetComponent<GameMobAreaObserver>();
			}
			if (!this.AttackTargetsProvider.IsNull())
			{
				this.AttackTargetsProvider.ObjectEnteredArea += this.OnMobEnteredRange;
				this.AttackTargetsProvider.ObjectExitedArea += this.OnMobExitedRange;
			}
			this.SetActive(this.ActiveOnAwake, true, this.FirstStateChangeDelay);
		}

		// Token: 0x0600110C RID: 4364 RVA: 0x000354FF File Offset: 0x000336FF
		public void SwitchState(float delay)
		{
			this.SetActive(!this.IsActive, false, delay);
		}

		// Token: 0x0600110D RID: 4365 RVA: 0x00035514 File Offset: 0x00033714
		public void SetActive(bool state, bool force = false, float delayOverride = -1f)
		{
			PushableGameMob.<>c__DisplayClass33_0 CS$<>8__locals1 = new PushableGameMob.<>c__DisplayClass33_0();
			CS$<>8__locals1.state = state;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.delayOverride = delayOverride;
			if (!force)
			{
				if (!CS$<>8__locals1.state && this.PushOnlyOnce)
				{
					return;
				}
				if (this.IsActive & CS$<>8__locals1.state)
				{
					return;
				}
			}
			CancellationToken cancellationToken = this.currentCancelToken;
			if (this.currentCancelToken.CanBeCanceled)
			{
				this.currentCancelTokenSource.Cancel();
			}
			this.currentCancelTokenSource = new CancellationTokenSource();
			this.currentCancelToken = this.currentCancelTokenSource.Token;
			new Task(delegate()
			{
				PushableGameMob.<>c__DisplayClass33_0.<<SetActive>b__0>d <<SetActive>b__0>d;
				<<SetActive>b__0>d.<>4__this = CS$<>8__locals1;
				<<SetActive>b__0>d.<>t__builder = AsyncVoidMethodBuilder.Create();
				<<SetActive>b__0>d.<>1__state = -1;
				AsyncVoidMethodBuilder <>t__builder = <<SetActive>b__0>d.<>t__builder;
				<>t__builder.Start<PushableGameMob.<>c__DisplayClass33_0.<<SetActive>b__0>d>(ref <<SetActive>b__0>d);
			}, this.currentCancelToken).Start();
		}

		// Token: 0x0600110E RID: 4366 RVA: 0x000355B8 File Offset: 0x000337B8
		private static void ActivateEffects(GameMobVFXController.VisualEffect[] effects, BaseGameMob currentMob)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Activate(currentMob);
			}
		}

		// Token: 0x0600110F RID: 4367 RVA: 0x000355E4 File Offset: 0x000337E4
		private static void DestroyEffects(GameMobVFXController.VisualEffect[] effects, BaseGameMob currentMob)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Destroy(currentMob);
			}
		}

		// Token: 0x06001110 RID: 4368 RVA: 0x0003560C File Offset: 0x0003380C
		private void OnMobEnteredRange(BaseGameMob mob)
		{
			this.currentTargetsCount++;
			if (this.currentTargetsCount > 0)
			{
				this.SetActive(true, false, -1f);
			}
		}

		// Token: 0x06001111 RID: 4369 RVA: 0x00035632 File Offset: 0x00033832
		private void OnMobExitedRange(BaseGameMob mob)
		{
			this.currentTargetsCount--;
			if (this.currentTargetsCount <= 0)
			{
				this.SetActive(false, false, -1f);
			}
		}

		// Token: 0x06001112 RID: 4370 RVA: 0x00035658 File Offset: 0x00033858
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.AttackTargetsProvider.IsNull())
			{
				this.AttackTargetsProvider.ObjectEnteredArea -= this.OnMobEnteredRange;
				this.AttackTargetsProvider.ObjectExitedArea -= this.OnMobExitedRange;
			}
			CancellationToken cancellationToken = this.currentCancelToken;
			if (this.currentCancelToken.CanBeCanceled)
			{
				this.currentCancelTokenSource.Cancel();
			}
		}

		// Token: 0x0400099E RID: 2462
		[Tooltip("Задержка смены состояния на старте")]
		public float FirstStateChangeDelay;

		// Token: 0x0400099F RID: 2463
		[Tooltip("Активна ли кнопка на старте?")]
		public bool ActiveOnAwake;

		// Token: 0x040009A0 RID: 2464
		[Tooltip("Защёлкивающиеся или нет?")]
		public bool PushOnlyOnce;

		// Token: 0x040009A1 RID: 2465
		public GameMobAreaObserver AttackTargetsProvider;

		// Token: 0x040009A2 RID: 2466
		[Tooltip("Задержка на срабатывание триггера")]
		public float ActivationDelay = -1f;

		// Token: 0x040009A3 RID: 2467
		[Tooltip("Эффекты на активацию кнопки")]
		public GameMobVFXController.VisualEffect[] ActivationEffects;

		// Token: 0x040009A4 RID: 2468
		[Tooltip("События на активацию кнопки")]
		public UnityEvent ActivationEvent;

		// Token: 0x040009A5 RID: 2469
		[Tooltip("Задержка на срабатывание триггера")]
		public float DeactivationDelay = -1f;

		// Token: 0x040009A6 RID: 2470
		[Tooltip("Эффекты на деактивацию кнопки")]
		public GameMobVFXController.VisualEffect[] DeactivationEffects;

		// Token: 0x040009A7 RID: 2471
		[Tooltip("События на деактивацию кнопки")]
		public UnityEvent DeactivationEvent;

		// Token: 0x040009A8 RID: 2472
		private int currentTargetsCount;

		// Token: 0x040009A9 RID: 2473
		private CancellationTokenSource currentCancelTokenSource = new CancellationTokenSource();

		// Token: 0x040009AA RID: 2474
		private CancellationToken currentCancelToken;
	}
}
