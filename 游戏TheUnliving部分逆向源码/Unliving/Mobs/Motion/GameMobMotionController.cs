using System;
using System.Collections.Generic;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000211 RID: 529
	public sealed class GameMobMotionController : GameMobMotionControllerBase
	{
		// Token: 0x170003C6 RID: 966
		// (get) Token: 0x060011D7 RID: 4567 RVA: 0x00037CA2 File Offset: 0x00035EA2
		// (set) Token: 0x060011D8 RID: 4568 RVA: 0x00037CAA File Offset: 0x00035EAA
		public Vector2? IndividualDestination
		{
			get
			{
				return this.individualDestination;
			}
			set
			{
				this.individualDestination = value;
			}
		}

		// Token: 0x170003C7 RID: 967
		// (get) Token: 0x060011D9 RID: 4569 RVA: 0x00037CB3 File Offset: 0x00035EB3
		public Vector2? CurrentDestination
		{
			get
			{
				return this.currentDestination.Value;
			}
		}

		// Token: 0x170003C8 RID: 968
		// (get) Token: 0x060011DA RID: 4570 RVA: 0x00037CC0 File Offset: 0x00035EC0
		public Vector2? DesiredDestination
		{
			get
			{
				return this.desiredDestination;
			}
		}

		// Token: 0x170003C9 RID: 969
		// (get) Token: 0x060011DB RID: 4571 RVA: 0x00037CC8 File Offset: 0x00035EC8
		public bool HasGroupDestination
		{
			get
			{
				return this.hasGroupDestination;
			}
		}

		// Token: 0x170003CA RID: 970
		// (get) Token: 0x060011DC RID: 4572 RVA: 0x00037CD0 File Offset: 0x00035ED0
		public bool IsFollowingGroupDestination
		{
			get
			{
				return this.isFollowingGroupDestination;
			}
		}

		// Token: 0x170003CB RID: 971
		// (get) Token: 0x060011DD RID: 4573 RVA: 0x00037CD8 File Offset: 0x00035ED8
		public bool IsGroupDestinationReached
		{
			get
			{
				return this.isGroupDestinationReached;
			}
		}

		// Token: 0x170003CC RID: 972
		// (get) Token: 0x060011DE RID: 4574 RVA: 0x00037CE0 File Offset: 0x00035EE0
		public bool IsDestinationPointReached
		{
			get
			{
				return this.isDestinationPointReached;
			}
		}

		// Token: 0x170003CD RID: 973
		// (get) Token: 0x060011DF RID: 4575 RVA: 0x00037CE8 File Offset: 0x00035EE8
		public bool IsDestinationBlocked
		{
			get
			{
				return this.isDestinationBlocked;
			}
		}

		// Token: 0x170003CE RID: 974
		// (get) Token: 0x060011E0 RID: 4576 RVA: 0x00037CF0 File Offset: 0x00035EF0
		public bool HasPath
		{
			get
			{
				NavMeshAgent navMeshAgentComponent = this.NavMeshAgentComponent;
				return navMeshAgentComponent != null && navMeshAgentComponent.hasPath;
			}
		}

		// Token: 0x170003CF RID: 975
		// (get) Token: 0x060011E1 RID: 4577 RVA: 0x00037D03 File Offset: 0x00035F03
		public bool IsFollowingGroupLeader
		{
			get
			{
				return this.isFollowingGroupLeader;
			}
		}

		// Token: 0x140000BE RID: 190
		// (add) Token: 0x060011E2 RID: 4578 RVA: 0x00037D0C File Offset: 0x00035F0C
		// (remove) Token: 0x060011E3 RID: 4579 RVA: 0x00037D44 File Offset: 0x00035F44
		public event Action<GameMobMotionController> GroupDestinationReached;

		// Token: 0x060011E4 RID: 4580 RVA: 0x00037D7C File Offset: 0x00035F7C
		private void UpdateNavMeshAgentDestination(GameMobDestinationPoint targetDestination)
		{
			if (!this.ControllerOwner.IsActiveNavMeshAgent())
			{
				return;
			}
			if (targetDestination != null && targetDestination.HasValue)
			{
				this.NavMeshAgentComponent.isStopped = false;
				this.NavMeshAgentComponent.destination = targetDestination.Value.Value;
				return;
			}
			this.NavMeshAgentComponent.ResetPath();
		}

		// Token: 0x060011E5 RID: 4581 RVA: 0x00037DD8 File Offset: 0x00035FD8
		private bool TryGetGroupDestination(out Vector2? destination, out bool isForcedDestination)
		{
			if (this.currentGroupDestinationSource != this.ControllerOwner)
			{
				GameMobsGroupControllerBase group = this.ControllerOwner.Group;
				if (group != null)
				{
					destination = group.GroupDestination;
					bool flag = destination != null && (!this.leaveReachedGroupDestination || !this.isGroupDestinationReached);
					isForcedDestination = group.HasForcedGroupDestination;
					if (flag && (isForcedDestination || (this.individualDestination == null && !group.IsGroupDestinationReached)))
					{
						return true;
					}
				}
			}
			isForcedDestination = false;
			destination = null;
			return false;
		}

		// Token: 0x060011E6 RID: 4582 RVA: 0x00037E60 File Offset: 0x00036060
		private void MoveWithLocalAvoidance()
		{
			this.isFollowingGroupLeader = false;
			LocationChunkMobsGridController.GridAgent locationChunkGridAgent = this.ControllerOwner.LocationChunkGridAgent;
			if (!this.isDestinationBlocked && locationChunkGridAgent != null && this.ControllerOwner.IsActiveNavMeshAgent())
			{
				BaseGameMob blockingMob;
				bool flag;
				float num;
				this.steeringContext.Update(locationChunkGridAgent, out blockingMob, out flag, out num, out this.isFollowingGroupLeader);
				Vector2 b;
				if (this.steeringContext.TryGetMovementDirection(out b))
				{
					this.steeringContext.ShowDebugGizmo(1f);
				}
				this.movementSpeedMultipliers = Vector2.Lerp(this.movementSpeedMultipliers, b, (flag ? 50f : 15f) * Time.deltaTime);
				base.SetBlockingMob(blockingMob, this.movementSpeedMultipliers.SqrMagnitude() < 1E-06f);
				if (!base.IsMovementFreezeActive())
				{
					float num2 = base.GetActualMobSpeed();
					if (this.isFollowingGroupLeader)
					{
						BaseGameMob baseGameMob = this.ControllerOwner.Group.Leader as BaseGameMob;
						if (baseGameMob != null && baseGameMob.Speed < num2)
						{
							num2 = baseGameMob.Speed * 0.9f;
						}
					}
					base.Move(num2 * num * this.movementSpeedMultipliers);
					return;
				}
			}
			else
			{
				base.ResetBlockingMob();
				this.movementSpeedMultipliers = default(Vector2);
			}
		}

		// Token: 0x060011E7 RID: 4583 RVA: 0x00037F90 File Offset: 0x00036190
		private bool TryGetChunkEscapePoint(Vector2 currentDestination, out GameMobMotionController.ChunkEscapePoint escapePoint)
		{
			if (this.currentChunkRect.size != Vector2.zero && !this.currentChunkRect.Contains(currentDestination) && !this.ControllerOwner.IsChunkTransitionInProgress)
			{
				for (int i = 0; i < this.chunkEscapePointsCount; i++)
				{
					if (this.chunkEscapePoints[i].IsValidEscapePoint(currentDestination))
					{
						escapePoint = this.chunkEscapePoints[i];
						return true;
					}
				}
			}
			escapePoint = default(GameMobMotionController.ChunkEscapePoint);
			return false;
		}

		// Token: 0x060011E8 RID: 4584 RVA: 0x00038010 File Offset: 0x00036210
		public GameMobMotionController(BaseGameMob mob) : base(mob)
		{
			if (this.NavMeshAgentComponent != null)
			{
				this.NavMeshAgentComponent.autoBraking = false;
				this.NavMeshAgentComponent.autoRepath = true;
				this.stoppingDistanceSquared = Mathf.Max(this.NavMeshAgentComponent.stoppingDistance, mob.Radius + 0.1f);
				this.stoppingDistanceSquared *= this.stoppingDistanceSquared;
				this.NavMeshAgentComponent.avoidancePriority = (int)Mathf.Max(50f - mob.CrowdPassPriority, 0f);
				if (this.NavMeshAgentComponent.obstacleAvoidanceType != ObstacleAvoidanceType.NoObstacleAvoidance)
				{
					this.NavMeshAgentComponent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
				}
			}
			if (this.RigidbodyComponent != null)
			{
				this.RigidbodyComponent.useFullKinematicContacts = false;
			}
			this.currentDestination = new GameMobDestinationPoint
			{
				newValueDistanceThreshold = 0.05f
			};
			this.steeringContext = new GameMobSteeringContext(mob, this);
			mob.LocationChunkEntered += this.OnLocationChunkEntered;
		}

		// Token: 0x060011E9 RID: 4585 RVA: 0x0003810C File Offset: 0x0003630C
		public void SetObstacleAvoidanceFlags(bool isObstacleAvoidanceActive, bool isNavmeshEdgesAvoidanceActive)
		{
			this.steeringContext.avoidObstacles = isObstacleAvoidanceActive;
			this.steeringContext.avoidNavmeshEdges = isNavmeshEdgesAvoidanceActive;
		}

		// Token: 0x060011EA RID: 4586 RVA: 0x00038126 File Offset: 0x00036326
		public bool HasDestination()
		{
			return this.currentDestination.HasValue;
		}

		// Token: 0x060011EB RID: 4587 RVA: 0x00038134 File Offset: 0x00036334
		public bool HasDestination(Vector2 destination)
		{
			Vector2 lhs;
			return this.currentDestination.TryGetValue(out lhs) && lhs == destination;
		}

		// Token: 0x060011EC RID: 4588 RVA: 0x0003815C File Offset: 0x0003635C
		public bool IsPointReached(Vector2 point, float arrivalRadius = 0f)
		{
			return (point - this.ControllerOwner.Position).SqrMagnitude() < Mathf.Max(this.stoppingDistanceSquared, arrivalRadius * arrivalRadius);
		}

		// Token: 0x060011ED RID: 4589 RVA: 0x00038192 File Offset: 0x00036392
		public bool IsPointReached(Vector2? point, float arrivalRadius = 0f)
		{
			return point != null && this.IsPointReached(point.Value, arrivalRadius);
		}

		// Token: 0x060011EE RID: 4590 RVA: 0x000381B0 File Offset: 0x000363B0
		public bool IsGroupDestinationReachedByNeighbour()
		{
			BaseGameMob currentBlockingMob = base.CurrentBlockingMob;
			if (currentBlockingMob == null || base.CurrentBlockingMob.MotionController == null || currentBlockingMob.Group != this.ControllerOwner.Group)
			{
				return false;
			}
			GameMobMotionController gameMobMotionController = currentBlockingMob.MotionController as GameMobMotionController;
			return gameMobMotionController != null && gameMobMotionController.IsGroupDestinationReached;
		}

		// Token: 0x060011EF RID: 4591 RVA: 0x00038206 File Offset: 0x00036406
		public Vector2 GetWaypointPosition()
		{
			if (this.ControllerOwner.IsChunkTransitionInProgress && this.currentDestination.HasValue)
			{
				return this.currentDestination.GetValue();
			}
			return this.NavMeshAgentComponent.steeringTarget;
		}

		// Token: 0x060011F0 RID: 4592 RVA: 0x0003823E File Offset: 0x0003643E
		public void BlockDestination()
		{
			this.isDestinationBlocked = true;
			this.destinationBlockResetTime = Time.time + 0.1f;
		}

		// Token: 0x060011F1 RID: 4593 RVA: 0x00038258 File Offset: 0x00036458
		public void LeaveReachedGroupDestination()
		{
			this.leaveReachedGroupDestination = true;
		}

		// Token: 0x060011F2 RID: 4594 RVA: 0x00038264 File Offset: 0x00036464
		private void OnLocationChunkEntered(ILocationChunk lastChunk, ILocationChunk newChunk)
		{
			this.currentChunkRect = newChunk.GetBoundsRect();
			this.chunkEscapePointsCount = 0;
			this.lastChunkEscapePoint = default(GameMobMotionController.ChunkEscapePoint);
			IList<ILocationChunkGateway> gateways = newChunk.Gateways;
			int count = gateways.Count;
			if (this.chunkEscapePoints == null)
			{
				this.chunkEscapePoints = new GameMobMotionController.ChunkEscapePoint[Mathf.Max(count, 4)];
			}
			else if (this.chunkEscapePoints.Length < count)
			{
				Array.Resize<GameMobMotionController.ChunkEscapePoint>(ref this.chunkEscapePoints, count);
			}
			for (int i = 0; i < count; i++)
			{
				ILocationChunkGateway gateway = gateways[i];
				ILocationChunk nextChunk = gateway.GetNextChunk();
				if (nextChunk != null)
				{
					GameMobMotionController.ChunkEscapePoint[] array = this.chunkEscapePoints;
					int num = this.chunkEscapePointsCount;
					this.chunkEscapePointsCount = num + 1;
					array[num] = new GameMobMotionController.ChunkEscapePoint(gateway, nextChunk);
				}
			}
		}

		// Token: 0x060011F3 RID: 4595 RVA: 0x00038318 File Offset: 0x00036518
		public void OnGroupDestinationChanged(GameMobsGroupControllerBase group, Vector2? newGroupDestination, object destinationSource)
		{
			this.currentGroupDestinationSource = destinationSource;
			if (newGroupDestination == this.lastGroupDestination)
			{
				return;
			}
			this.leaveReachedGroupDestination = false;
			this.isGroupDestinationReached = false;
			this.groupDestinationHoldPoint = null;
			this.lastGroupDestination = newGroupDestination;
		}

		// Token: 0x060011F4 RID: 4596 RVA: 0x0003838B File Offset: 0x0003658B
		public void OnGroupDestinationReached(Vector2 reachedGroupDestination)
		{
			this.isGroupDestinationReached = true;
			if (this.maxGroupDestinationQuitDistance > 0f)
			{
				this.groupDestinationHoldPoint = new Vector2?(this.ControllerOwner.Position);
			}
			Action<GameMobMotionController> groupDestinationReached = this.GroupDestinationReached;
			if (groupDestinationReached == null)
			{
				return;
			}
			groupDestinationReached(this);
		}

		// Token: 0x060011F5 RID: 4597 RVA: 0x000383C8 File Offset: 0x000365C8
		public override void OnUpdate()
		{
			this.hasGroupDestination = false;
			this.isFollowingGroupDestination = false;
			this.isDestinationPointReached = false;
			this.desiredDestination = null;
			BaseGameMob controllerOwner = this.ControllerOwner;
			if (!base.IsActive || controllerOwner.IsKinematic || controllerOwner.IsKilled)
			{
				return;
			}
			if (this.isDestinationBlocked)
			{
				this.isDestinationBlocked = (Time.time < this.destinationBlockResetTime);
				this.isDestinationPointReached = true;
				this.individualDestination = null;
				return;
			}
			GameMobsGroupControllerBase group = controllerOwner.Group;
			Vector2? vector = this.individualDestination;
			if (group != null)
			{
				Vector2? vector2;
				bool flag;
				if (this.TryGetGroupDestination(out vector2, out flag))
				{
					if (flag && vector != null && this.groupDestinationHoldPoint != null)
					{
						Vector2 value = this.groupDestinationHoldPoint.Value;
						Vector2 b = Vector2.ClampMagnitude(vector.Value - value, this.maxGroupDestinationQuitDistance);
						vector = new Vector2?(value + b);
					}
					else
					{
						vector = vector2;
					}
					this.individualDestination = null;
					this.hasGroupDestination = true;
					this.isFollowingGroupDestination = !this.isGroupDestinationReached;
				}
				if (this.maxWanderingDistance > 0f && vector != null)
				{
					Vector2 a = vector.Value - group.InitialPosition;
					float num = a.SqrMagnitude();
					if (num > this.maxWanderingDistance * this.maxWanderingDistance)
					{
						vector = new Vector2?(group.InitialPosition + a / Mathf.Sqrt(num) * this.maxWanderingDistance);
					}
				}
			}
			this.desiredDestination = vector;
			if (!this.hasGroupDestination && vector != null && this.currentDestination.CanBeUpdated(vector.Value))
			{
				IGameMobMovementPointLimiter currentMovementPointLimiter = base.CurrentMovementPointLimiter;
				if (currentMovementPointLimiter != null)
				{
					vector = currentMovementPointLimiter.LimitMovementPoint(vector.Value);
				}
			}
			GameMobMotionController.ChunkEscapePoint chunkEscapePoint = default(GameMobMotionController.ChunkEscapePoint);
			if (vector != null && this.TryGetChunkEscapePoint(vector.Value, out chunkEscapePoint))
			{
				vector = new Vector2?(chunkEscapePoint.GetPosition(controllerOwner.Position));
				if (!base.IsMovementFreezed && this.NavMeshAgentComponent.velocity.sqrMagnitude < 0.0001f)
				{
					Vector3 a2 = controllerOwner.Speed * chunkEscapePoint.NextChunkDirection;
					this.NavMeshAgentComponent.Warp(this.NavMeshAgentComponent.nextPosition + a2 * Time.deltaTime);
				}
				if (chunkEscapePoint.MobIntersectsEscapeArea(controllerOwner))
				{
					chunkEscapePoint.ForceStartChunkTransition(controllerOwner);
					this.lastChunkEscapePoint = chunkEscapePoint;
				}
			}
			if (this.lastChunkEscapePoint.IsValid && controllerOwner.IsChunkTransitionInProgress && !this.lastChunkEscapePoint.MobIntersectsEscapeArea(controllerOwner))
			{
				this.lastChunkEscapePoint.CancelChunkTransition(controllerOwner);
				this.lastChunkEscapePoint = default(GameMobMotionController.ChunkEscapePoint);
			}
			if (this.IsPointReached(vector, 0f))
			{
				this.isDestinationPointReached = true;
				if (chunkEscapePoint.IsValid)
				{
					base.MoveInDirection(chunkEscapePoint.NextChunkDirection);
				}
				else
				{
					base.FreezeMovement(0f, false);
				}
			}
			if (this.currentDestination.Update(vector, false) || (this.currentDestination.HasValue && !this.NavMeshAgentComponent.hasPath))
			{
				this.UpdateNavMeshAgentDestination(this.currentDestination);
			}
			this.MoveWithLocalAvoidance();
		}

		// Token: 0x060011F6 RID: 4598 RVA: 0x000386F9 File Offset: 0x000368F9
		protected override void OnMobTotallyDestroyed(object destroyedMob)
		{
			base.OnMobTotallyDestroyed(destroyedMob);
			this.ControllerOwner.LocationChunkEntered -= this.OnLocationChunkEntered;
		}

		// Token: 0x04000A33 RID: 2611
		public float maxWanderingDistance;

		// Token: 0x04000A34 RID: 2612
		public int movementBlockingMobLayers;

		// Token: 0x04000A35 RID: 2613
		public float maxGroupDestinationQuitDistance;

		// Token: 0x04000A36 RID: 2614
		private readonly float stoppingDistanceSquared;

		// Token: 0x04000A37 RID: 2615
		private readonly GameMobDestinationPoint currentDestination;

		// Token: 0x04000A38 RID: 2616
		private readonly GameMobSteeringContext steeringContext;

		// Token: 0x04000A39 RID: 2617
		private bool isGroupDestinationReached;

		// Token: 0x04000A3A RID: 2618
		private Vector2? individualDestination;

		// Token: 0x04000A3B RID: 2619
		private Vector2? desiredDestination;

		// Token: 0x04000A3C RID: 2620
		private Vector2? lastGroupDestination;

		// Token: 0x04000A3D RID: 2621
		private object currentGroupDestinationSource;

		// Token: 0x04000A3E RID: 2622
		private bool isDestinationPointReached;

		// Token: 0x04000A3F RID: 2623
		private bool isDestinationBlocked;

		// Token: 0x04000A40 RID: 2624
		private bool leaveReachedGroupDestination;

		// Token: 0x04000A41 RID: 2625
		private float destinationBlockResetTime;

		// Token: 0x04000A42 RID: 2626
		private bool hasGroupDestination;

		// Token: 0x04000A43 RID: 2627
		private bool isFollowingGroupDestination;

		// Token: 0x04000A44 RID: 2628
		private Vector2? groupDestinationHoldPoint;

		// Token: 0x04000A45 RID: 2629
		private Vector2 movementSpeedMultipliers;

		// Token: 0x04000A46 RID: 2630
		private bool isFollowingGroupLeader;

		// Token: 0x04000A47 RID: 2631
		private Rect currentChunkRect;

		// Token: 0x04000A48 RID: 2632
		private GameMobMotionController.ChunkEscapePoint[] chunkEscapePoints;

		// Token: 0x04000A49 RID: 2633
		private int chunkEscapePointsCount;

		// Token: 0x04000A4A RID: 2634
		private GameMobMotionController.ChunkEscapePoint lastChunkEscapePoint;

		// Token: 0x020004BA RID: 1210
		private readonly struct ChunkEscapePoint
		{
			// Token: 0x0600250A RID: 9482 RVA: 0x0007323C File Offset: 0x0007143C
			public ChunkEscapePoint(ILocationChunkGateway gateway, ILocationChunk nextChunk)
			{
				this.Position = gateway.Position;
				this.NextChunkDirection = gateway.TransitionDirection;
				this.size = gateway.WorldSize;
				this.nextChunkRect = nextChunk.GetBoundsRect();
				this.transitionArea = gateway.TransitionArea;
				this.IsValid = true;
			}

			// Token: 0x0600250B RID: 9483 RVA: 0x0007328C File Offset: 0x0007148C
			public bool IsValidEscapePoint(Vector2 destination)
			{
				return this.nextChunkRect.Contains(destination);
			}

			// Token: 0x0600250C RID: 9484 RVA: 0x000732A8 File Offset: 0x000714A8
			public bool MobIntersectsEscapeArea(BaseGameMob mob)
			{
				return this.transitionArea != null && this.transitionArea.IsIntersected(mob);
			}

			// Token: 0x0600250D RID: 9485 RVA: 0x000732C0 File Offset: 0x000714C0
			public Vector2 GetPosition(Vector2 mobPosition)
			{
				Vector2 lhs = mobPosition - this.Position;
				Vector2 nextChunkDirection = this.NextChunkDirection;
				Vector2 vector = new Vector2(nextChunkDirection.y, -nextChunkDirection.x);
				float num = Mathf.Abs(Vector2.Dot(this.size, vector)) * 0.35f;
				Vector2 b = -Mathf.Abs(Vector2.Dot(this.size, nextChunkDirection)) * nextChunkDirection;
				return this.Position + Mathf.Clamp(Vector2.Dot(lhs, vector), -num, num) * vector + b;
			}

			// Token: 0x0600250E RID: 9486 RVA: 0x0007334E File Offset: 0x0007154E
			public void ForceStartChunkTransition(BaseGameMob mob)
			{
				ILocationChunkTransitionArea locationChunkTransitionArea = this.transitionArea;
				if (locationChunkTransitionArea == null)
				{
					return;
				}
				locationChunkTransitionArea.ForceStartTransition(mob);
			}

			// Token: 0x0600250F RID: 9487 RVA: 0x00073361 File Offset: 0x00071561
			public void CancelChunkTransition(BaseGameMob mob)
			{
				ILocationChunkTransitionArea locationChunkTransitionArea = this.transitionArea;
				if (locationChunkTransitionArea == null)
				{
					return;
				}
				locationChunkTransitionArea.CancelTransition(mob);
			}

			// Token: 0x04001984 RID: 6532
			public readonly bool IsValid;

			// Token: 0x04001985 RID: 6533
			public readonly Vector2 Position;

			// Token: 0x04001986 RID: 6534
			public readonly Vector2 NextChunkDirection;

			// Token: 0x04001987 RID: 6535
			private readonly Vector2 size;

			// Token: 0x04001988 RID: 6536
			private readonly Rect nextChunkRect;

			// Token: 0x04001989 RID: 6537
			private readonly ILocationChunkTransitionArea transitionArea;
		}
	}
}
