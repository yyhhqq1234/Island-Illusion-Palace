using System;
using System.Collections.Generic;
using Common;
using Common.Math.Gameplay;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x0200020E RID: 526
	public sealed class GameMobHangingPlatform : GameBehaviourBase, IGameMobsHangingPlatform, IDestroyable
	{
		// Token: 0x170003C0 RID: 960
		// (get) Token: 0x060011BB RID: 4539 RVA: 0x000377A0 File Offset: 0x000359A0
		public float Height
		{
			get
			{
				return this._height;
			}
		}

		// Token: 0x060011BC RID: 4540 RVA: 0x000377A8 File Offset: 0x000359A8
		private float GetGravity()
		{
			if (this.maxGravityVariation <= 0f)
			{
				return this.gravity;
			}
			return this.gravity * (1f + UnityEngine.Random.Range(-1f, 1f) * this.maxGravityVariation);
		}

		// Token: 0x060011BD RID: 4541 RVA: 0x000377E4 File Offset: 0x000359E4
		public Vector2 GetPosition()
		{
			Vector2 result = base.transform.position;
			result.x += this.positionOffset.x;
			result.y += this.positionOffset.y;
			return result;
		}

		// Token: 0x060011BE RID: 4542 RVA: 0x0003782F File Offset: 0x00035A2F
		public GameMobKinematicMotionBase GetFallMotion(GameMobMotionControllerBase motionController)
		{
			return new GameMobHangingPlatform.FallingMotion(motionController, this);
		}

		// Token: 0x060011BF RID: 4543 RVA: 0x00037838 File Offset: 0x00035A38
		public void OnMobAdded(BaseGameMob mob)
		{
			if (this.disableMobsColliders && mob.HitCollider != null)
			{
				mob.HitCollider.enabled = false;
			}
			if (this.blockMobsMovement && mob.MotionController != null)
			{
				mob.MotionController.IsActive = false;
			}
			if (this.modifyMobsDrawingOrder && mob.Renderer != null)
			{
				mob.Renderer.sortingOrder = (int)this._height;
			}
			HitPointsController hitPointsController = mob.HitPointsController as HitPointsController;
			if (hitPointsController != null)
			{
				hitPointsController.ModifyDamageResistance(this.mobsDamageResistance);
			}
			AssetBasedBuffsBlockersCollection assetBasedBuffsBlockersCollection = this.ignorableBuffs;
			if (assetBasedBuffsBlockersCollection != null)
			{
				assetBasedBuffsBlockersCollection.SetActive(true, mob.BuffsController);
			}
			GameMobHangingPlatform.MobState value = new GameMobHangingPlatform.MobState(mob);
			value.TryOverrideMobNavmeshAgentPriority(this.navmeshAgentsPriorityOverride);
			value.SetBuffsActive(this, true);
			this.mobStates.Add(mob.GetInstanceID(), value);
		}

		// Token: 0x060011C0 RID: 4544 RVA: 0x00037910 File Offset: 0x00035B10
		public void OnMobRemoved(BaseGameMob mob)
		{
			if (GameApplication.IsGameStateChanging)
			{
				return;
			}
			if (this.disableMobsColliders && mob.HitCollider != null)
			{
				mob.HitCollider.enabled = true;
			}
			if (this.blockMobsMovement && mob.MotionController != null)
			{
				mob.MotionController.IsActive = true;
			}
			if (this.modifyMobsDrawingOrder)
			{
				mob.ResetLocalDrawingOrder();
			}
			HitPointsController hitPointsController = mob.HitPointsController as HitPointsController;
			if (hitPointsController != null)
			{
				hitPointsController.ModifyDamageResistance(-this.mobsDamageResistance);
			}
			int instanceID = mob.GetInstanceID();
			GameMobHangingPlatform.MobState mobState;
			this.mobStates.TryGetValue(mob.GetInstanceID(), out mobState);
			this.mobStates.Remove(instanceID);
			mobState.SetBuffsActive(this, false);
			mobState.ResetMobNavmeshAgentPriority();
			AssetBasedBuffsBlockersCollection assetBasedBuffsBlockersCollection = this.ignorableBuffs;
			if (assetBasedBuffsBlockersCollection == null)
			{
				return;
			}
			assetBasedBuffsBlockersCollection.SetActive(false, mob.BuffsController);
		}

		// Token: 0x060011C1 RID: 4545 RVA: 0x000379DC File Offset: 0x00035BDC
		private void Start()
		{
			BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffsGenerators;
			generatorsBuilders.Instantiate(out this.instantiatedBuffsGenerators);
		}

		// Token: 0x060011C2 RID: 4546 RVA: 0x000379FC File Offset: 0x00035BFC
		private void OnDrawGizmosSelected()
		{
			Vector3 vector = this.GetPosition();
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(vector, 0.1f);
			if (this._height <= 0f)
			{
				return;
			}
			Vector3 vector2 = vector - new Vector3
			{
				y = this._height
			};
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(vector2, 1f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(vector, vector2);
		}

		// Token: 0x04000A1B RID: 2587
		[SerializeField]
		private float _height = 5f;

		// Token: 0x04000A1C RID: 2588
		public Vector2 positionOffset;

		// Token: 0x04000A1D RID: 2589
		public float gravity = 9.8f;

		// Token: 0x04000A1E RID: 2590
		public float maxGravityVariation = 0.1f;

		// Token: 0x04000A1F RID: 2591
		public float fallDamage = 10f;

		// Token: 0x04000A20 RID: 2592
		[Space(5f)]
		public bool modifyMobsDrawingOrder = true;

		// Token: 0x04000A21 RID: 2593
		public bool disableMobsColliders = true;

		// Token: 0x04000A22 RID: 2594
		public bool blockMobsMovement = true;

		// Token: 0x04000A23 RID: 2595
		public int navmeshAgentsPriorityOverride = 5;

		// Token: 0x04000A24 RID: 2596
		[Range(0f, 1f)]
		public float mobsDamageResistance = 1f;

		// Token: 0x04000A25 RID: 2597
		public BuffsGeneratorBuilderAsset.Reference[] buffsGenerators;

		// Token: 0x04000A26 RID: 2598
		public AssetBasedBuffsBlockersCollection ignorableBuffs;

		// Token: 0x04000A27 RID: 2599
		private readonly Dictionary<int, GameMobHangingPlatform.MobState> mobStates = new Dictionary<int, GameMobHangingPlatform.MobState>(32);

		// Token: 0x04000A28 RID: 2600
		private IBuffsGenerator[] instantiatedBuffsGenerators;

		// Token: 0x020004B8 RID: 1208
		private struct MobState
		{
			// Token: 0x06002502 RID: 9474 RVA: 0x0007300F File Offset: 0x0007120F
			public MobState(BaseGameMob mob)
			{
				this = default(GameMobHangingPlatform.MobState);
				this.mob = mob;
				this.appliedBuffs = new List<IBuff>(4);
			}

			// Token: 0x06002503 RID: 9475 RVA: 0x0007302C File Offset: 0x0007122C
			public void TryOverrideMobNavmeshAgentPriority(int priorityOverride)
			{
				if (priorityOverride < 0 || this.mob.NavMeshAgent == null)
				{
					return;
				}
				this.storedNavmeshAgentPriority = new int?(this.mob.NavMeshAgent.avoidancePriority);
				this.mob.NavMeshAgent.avoidancePriority = priorityOverride;
			}

			// Token: 0x06002504 RID: 9476 RVA: 0x0007307D File Offset: 0x0007127D
			public void ResetMobNavmeshAgentPriority()
			{
				if (this.storedNavmeshAgentPriority == null)
				{
					return;
				}
				this.mob.NavMeshAgent.avoidancePriority = this.storedNavmeshAgentPriority.Value;
				this.storedNavmeshAgentPriority = null;
			}

			// Token: 0x06002505 RID: 9477 RVA: 0x000730B4 File Offset: 0x000712B4
			public void SetBuffsActive(GameMobHangingPlatform platform, bool isActive)
			{
				IBuffsController buffsController = this.mob.BuffsController;
				if (buffsController == null)
				{
					return;
				}
				if (isActive)
				{
					IBuffsGenerator[] instantiatedBuffsGenerators = platform.instantiatedBuffsGenerators;
					for (int i = 0; i < instantiatedBuffsGenerators.Length; i++)
					{
						IBuff buff = instantiatedBuffsGenerators[i].GenerateBuff(platform, true);
						if (buffsController.AddBuff(buff))
						{
							this.appliedBuffs.Add(buff);
						}
					}
					return;
				}
				for (int j = 0; j < this.appliedBuffs.Count; j++)
				{
					this.appliedBuffs[j].Complete();
				}
				this.appliedBuffs.Clear();
			}

			// Token: 0x0400197E RID: 6526
			public BaseGameMob mob;

			// Token: 0x0400197F RID: 6527
			public int? storedNavmeshAgentPriority;

			// Token: 0x04001980 RID: 6528
			public List<IBuff> appliedBuffs;
		}

		// Token: 0x020004B9 RID: 1209
		public sealed class FallingMotion : GameMobKinematicMotionBase
		{
			// Token: 0x06002506 RID: 9478 RVA: 0x00073144 File Offset: 0x00071344
			public FallingMotion(GameMobMotionControllerBase motionController, GameMobHangingPlatform platform) : base(motionController, platform)
			{
				this.gravity = platform.GetGravity();
				this.height = platform.Height;
				if (platform.fallDamage > 0f)
				{
					this.fallDamage = new DamageGenerator.DamageSendingArgs
					{
						amount = platform.fallDamage
					};
				}
				this.duration = FallMotion.GetFallTime(this.height, this.gravity);
			}

			// Token: 0x06002507 RID: 9479 RVA: 0x000731AC File Offset: 0x000713AC
			protected override bool Start()
			{
				return this.height * this.gravity > 0f;
			}

			// Token: 0x06002508 RID: 9480 RVA: 0x000731C4 File Offset: 0x000713C4
			public override void Update(float t, out Vector3 velocity)
			{
				float fallVelocity = FallMotion.GetFallVelocity(this.height, t, this.gravity);
				velocity = new Vector3
				{
					y = fallVelocity,
					z = fallVelocity
				};
			}

			// Token: 0x06002509 RID: 9481 RVA: 0x00073204 File Offset: 0x00071404
			protected override void OnMotionCompleted()
			{
				if (this.fallDamage == null)
				{
					return;
				}
				IDamageable hitPointsController = this.MotionController.ControllerOwner.HitPointsController;
				if (hitPointsController != null)
				{
					hitPointsController.ModifyHitPoints(this, this.fallDamage);
				}
			}

			// Token: 0x04001981 RID: 6529
			private readonly float gravity;

			// Token: 0x04001982 RID: 6530
			private readonly float height;

			// Token: 0x04001983 RID: 6531
			private readonly DamageGenerator.DamageSendingArgs fallDamage;
		}
	}
}
