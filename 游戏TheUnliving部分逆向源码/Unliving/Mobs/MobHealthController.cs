using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Damage;
using Game.LevelGeneration;
using Game.Stats;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Gameplay;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001E4 RID: 484
	public class MobHealthController : HitPointsController
	{
		// Token: 0x17000326 RID: 806
		// (get) Token: 0x06000FD3 RID: 4051 RVA: 0x00031B67 File Offset: 0x0002FD67
		public static int RevivableLayer
		{
			get
			{
				if (MobHealthController.revivableLayer >= 0)
				{
					return MobHealthController.revivableLayer;
				}
				return MobHealthController.revivableLayer = LayerMask.NameToLayer("RevivableTarget");
			}
		}

		// Token: 0x06000FD4 RID: 4052 RVA: 0x00031B88 File Offset: 0x0002FD88
		private static bool IsRevivableMob(BaseGameMob mob)
		{
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			return mobBehaviour != null && mobBehaviour.IsRevivable;
		}

		// Token: 0x06000FD5 RID: 4053 RVA: 0x00031BA7 File Offset: 0x0002FDA7
		private static int GetFinalCollidersLayer(BaseGameMob killedMob)
		{
			if (!MobHealthController.IsRevivableMob(killedMob))
			{
				return 2;
			}
			return MobHealthController.RevivableLayer;
		}

		// Token: 0x17000327 RID: 807
		// (get) Token: 0x06000FD6 RID: 4054 RVA: 0x00031BB8 File Offset: 0x0002FDB8
		// (set) Token: 0x06000FD7 RID: 4055 RVA: 0x00031BC0 File Offset: 0x0002FDC0
		public override float InitialHitPoints
		{
			get
			{
				return base.InitialHitPoints;
			}
			set
			{
				if (base.InitialHitPoints == value)
				{
					return;
				}
				base.InitialHitPoints = value;
				MobDecayController mobDecayController = this.decayController;
				if (mobDecayController == null)
				{
					return;
				}
				mobDecayController.UpdateInitialHitPoints();
			}
		}

		// Token: 0x17000328 RID: 808
		// (get) Token: 0x06000FD8 RID: 4056 RVA: 0x00031BE3 File Offset: 0x0002FDE3
		public MobDecayController DecayController
		{
			get
			{
				return this.decayController;
			}
		}

		// Token: 0x06000FD9 RID: 4057 RVA: 0x00031BEB File Offset: 0x0002FDEB
		protected override void OnSetDestroyed()
		{
			this.RemoveDecayController();
			base.StartCoroutine(this.DeathRoutine());
		}

		// Token: 0x06000FDA RID: 4058 RVA: 0x00031C00 File Offset: 0x0002FE00
		private void RemoveDecayController()
		{
			if (this.decayController == null)
			{
				return;
			}
			base.RemoveAdditionalHitPointsSource(this.decayController);
			this.decayController.Dispose();
			this.decayController = null;
		}

		// Token: 0x06000FDB RID: 4059 RVA: 0x00031C2A File Offset: 0x0002FE2A
		private IEnumerator DeathRoutine()
		{
			NavMeshAgent navMeshAgent = this.currentMob.NavMeshAgent;
			if (this.currentMob.IsActiveNavMeshAgent())
			{
				navMeshAgent.isStopped = true;
			}
			if (this.currentMob.MotionController != null)
			{
				GameMobMotionControllerBase motionController = this.currentMob.MotionController;
				WaitForEndOfFrame frameDelay = new WaitForEndOfFrame();
				while (motionController.CurrentImpulse.sqrMagnitude > 0.001f)
				{
					yield return frameDelay;
					if (this.currentMob.IsActiveNavMeshAgent())
					{
						navMeshAgent.SetDestination(this.currentMob.transform.position);
					}
				}
				motionController = null;
				frameDelay = null;
			}
			base.RaiseTotallyDestroyedEvent();
			MobHealthController.MobCollidersBuffer.Clear();
			this.currentMob.GetComponentsInChildren<Collider2D>(MobHealthController.MobCollidersBuffer);
			int deadMobsLayer = MobHealthController.GetFinalCollidersLayer(this.currentMob);
			this.currentMob.SetLayer(deadMobsLayer);
			Rigidbody2D mobRigidbody = this.currentMob.Rigidbody;
			if (mobRigidbody != null)
			{
				mobRigidbody.simulated = false;
			}
			yield return new WaitForFixedUpdate();
			for (int i = 0; i < MobHealthController.MobCollidersBuffer.Count; i++)
			{
				MobHealthController.MobCollidersBuffer[i].gameObject.layer = deadMobsLayer;
			}
			if (mobRigidbody != null)
			{
				mobRigidbody.simulated = true;
			}
			if (!MobHealthController.IsRevivableMob(this.currentMob))
			{
				yield return new WaitForSeconds((this.corpseLifetimeOverride > 0f) ? this.corpseLifetimeOverride : MobHealthController.CorpseLifetime);
				this.currentMob.DestroyMob();
			}
			yield break;
		}

		// Token: 0x06000FDC RID: 4060 RVA: 0x00031C39 File Offset: 0x0002FE39
		private void ApplyDamageImpulse(Vector2? impulse, float multiplier)
		{
			if (impulse != null && multiplier > 0f && this.currentMob != null)
			{
				this.currentMob.AddMovementImpulse(impulse.Value * multiplier, false);
			}
		}

		// Token: 0x06000FDD RID: 4061 RVA: 0x00031C78 File Offset: 0x0002FE78
		protected override void OnHitPointsChanged(object sender, IHitPointsChangingArgs args)
		{
			if (base.gameObject.activeInHierarchy)
			{
				DamageGenerator.DamageSendingArgs damageSendingArgs = args as DamageGenerator.DamageSendingArgs;
				if (damageSendingArgs != null)
				{
					if (this.IsAlive)
					{
						this.ApplyDamageImpulse(damageSendingArgs.damageImpulse, this.damageImpulseMultiplier);
					}
					else
					{
						this.ApplyDamageImpulse(damageSendingArgs.damageImpulse, this.finalDamageImpulseMultiplier);
					}
				}
			}
			base.OnHitPointsChanged(sender, args);
		}

		// Token: 0x06000FDE RID: 4062 RVA: 0x00031CD4 File Offset: 0x0002FED4
		protected override void Start()
		{
			base.Start();
			this.currentMob = (this._behaviour as BaseGameMob);
			if (!(this.currentMob == null) && this.decayControllerParams != null && this.decayControllerParams.IsValid())
			{
				this.decayController = new MobDecayController(this.currentMob, this.decayControllerParams, new Func<BaseGameMob, bool>(MobHealthController.<Start>g__IsDecayActive|25_0), 0.5f);
				base.AddAdditionalHitPointsSource(this.decayController);
				StatsControllerBase<MobStatModifier> statsController = this.currentMob.StatsController;
				MobRottingSpeedStat mobRottingSpeedStat = ((statsController != null) ? statsController.GetStat(43) : null) as MobRottingSpeedStat;
				if (mobRottingSpeedStat != null)
				{
					mobRottingSpeedStat.RottingController = this.decayController;
				}
			}
		}

		// Token: 0x06000FDF RID: 4063 RVA: 0x00031D7E File Offset: 0x0002FF7E
		private void LateUpdate()
		{
			MobDecayController mobDecayController = this.decayController;
			if (mobDecayController == null)
			{
				return;
			}
			mobDecayController.Update();
		}

		// Token: 0x06000FE0 RID: 4064 RVA: 0x00031D90 File Offset: 0x0002FF90
		protected override void OnDestroy()
		{
			this.RemoveDecayController();
			base.OnDestroy();
		}

		// Token: 0x06000FE3 RID: 4067 RVA: 0x00031DDC File Offset: 0x0002FFDC
		[CompilerGenerated]
		internal static bool <Start>g__IsDecayActive|25_0(BaseGameMob mob)
		{
			GameMobsGroupControllerBase group = mob.Group;
			ILocationObject locationObject = ((group != null) ? group.Leader : null) as ILocationObject;
			if (locationObject != null)
			{
				LocationChunk locationChunk = locationObject.CurrentLocationChunk as LocationChunk;
				if (locationChunk != null)
				{
					EnemyLocationChunkClearingTrigger enemyLocationChunkClearingTrigger;
					return locationChunk.IsCoreChunk && locationChunk.TryGetComponent<EnemyLocationChunkClearingTrigger>(out enemyLocationChunkClearingTrigger) && !enemyLocationChunkClearingTrigger.IsChunkCleared;
				}
			}
			return false;
		}

		// Token: 0x0400092C RID: 2348
		public const int IgnorableDeadMobsLayer = 2;

		// Token: 0x0400092D RID: 2349
		private static readonly List<Collider2D> MobCollidersBuffer = new List<Collider2D>(8);

		// Token: 0x0400092E RID: 2350
		public static float CorpseLifetime = 10f;

		// Token: 0x0400092F RID: 2351
		private static int revivableLayer = -1;

		// Token: 0x04000930 RID: 2352
		public float damageImpulseMultiplier = 1f;

		// Token: 0x04000931 RID: 2353
		public float finalDamageImpulseMultiplier = 1f;

		// Token: 0x04000932 RID: 2354
		public float corpseLifetimeOverride;

		// Token: 0x04000933 RID: 2355
		[Space(5f)]
		public MobHealthController.DecayControllerParams decayControllerParams;

		// Token: 0x04000934 RID: 2356
		private BaseGameMob currentMob;

		// Token: 0x04000935 RID: 2357
		private MobDecayController decayController;

		// Token: 0x0200049D RID: 1181
		[Serializable]
		public sealed class DecayControllerParams
		{
			// Token: 0x17000762 RID: 1890
			// (get) Token: 0x06002466 RID: 9318 RVA: 0x00070BB1 File Offset: 0x0006EDB1
			// (set) Token: 0x06002467 RID: 9319 RVA: 0x00070BB9 File Offset: 0x0006EDB9
			public float RelativeDecaySpeed
			{
				get
				{
					return this._relativeDecaySpeed;
				}
				set
				{
					this._relativeDecaySpeed = Mathf.Clamp01(value);
				}
			}

			// Token: 0x06002468 RID: 9320 RVA: 0x00070BC7 File Offset: 0x0006EDC7
			public bool IsValid()
			{
				return this.decayPointsRatio > 0f && (this.decaySpeed > 0f || this._relativeDecaySpeed > 0f);
			}

			// Token: 0x040018F2 RID: 6386
			public float decayPointsRatio = 1.5f;

			// Token: 0x040018F3 RID: 6387
			public float decaySpeed;

			// Token: 0x040018F4 RID: 6388
			[SerializeField]
			[Range(0f, 1f)]
			private float _relativeDecaySpeed;
		}
	}
}
