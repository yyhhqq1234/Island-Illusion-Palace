using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Player;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000214 RID: 532
	public sealed class GameMobSteeringContext
	{
		// Token: 0x170003E0 RID: 992
		// (get) Token: 0x0600123D RID: 4669 RVA: 0x000394F7 File Offset: 0x000376F7
		public IReadOnlyList<Vector2> SteeringDirections
		{
			get
			{
				return GameMobSteeringContext.Directions;
			}
		}

		// Token: 0x170003E1 RID: 993
		// (get) Token: 0x0600123E RID: 4670 RVA: 0x000394FE File Offset: 0x000376FE
		public IReadOnlyList<float> InterestMap
		{
			get
			{
				return this.interestMap;
			}
		}

		// Token: 0x170003E2 RID: 994
		// (get) Token: 0x0600123F RID: 4671 RVA: 0x00039506 File Offset: 0x00037706
		public IReadOnlyList<float> DangerMap
		{
			get
			{
				return this.dangerMap;
			}
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x00039510 File Offset: 0x00037710
		private void UpdateMap(Vector2 direction, bool isInterest, bool isAdditive)
		{
			float[] map = isInterest ? this.interestMap : this.dangerMap;
			for (int i = 0; i < 8; i++)
			{
				ref Vector2 ptr = ref GameMobSteeringContext.Directions[i];
				float newValue = ptr.x * direction.x + ptr.y * direction.y;
				GameMobSteeringContext.<UpdateMap>g__UpdateMap|17_0(map, i, newValue, isAdditive);
			}
		}

		// Token: 0x06001241 RID: 4673 RVA: 0x0003956C File Offset: 0x0003776C
		private bool IsBlockingMob(BaseGameMob mob, Vector2 destination, out bool isStaticObstacle, out bool isStrongBlockingMob, out float blockingMobWeight)
		{
			isStaticObstacle = false;
			isStrongBlockingMob = false;
			blockingMobWeight = 1f;
			bool flag = this.currentGroup != null && this.currentGroup.IsRetreating;
			if (flag && mob.Faction != this.currentMob.Faction)
			{
				return false;
			}
			if (!mob.isCrowdObstacle || !this.motionController.HasSamePlatform(mob))
			{
				return false;
			}
			if (this.currentMob.Faction == mob.Faction)
			{
				float num = mob.Radius + 0.1f;
				num *= num;
				if ((destination - mob.Position).SqrMagnitude() < num)
				{
					isStrongBlockingMob = true;
					blockingMobWeight = 4f;
					return true;
				}
			}
			PlayerBehaviour playerBehaviour = mob as PlayerBehaviour;
			if (playerBehaviour != null)
			{
				if (playerBehaviour.MotionController.CurrentVelocity.SqrMagnitude() < 0.01f)
				{
					isStrongBlockingMob = (this.currentGroup != playerBehaviour.Group);
					return true;
				}
				return false;
			}
			else
			{
				GameMobsGroupControllerBase group = mob.Group;
				GameMobMotionControllerBase gameMobMotionControllerBase = mob.MotionController;
				bool flag2 = gameMobMotionControllerBase != null;
				isStaticObstacle = (!flag2 || mob.IsStaticCrowdObstacle);
				isStrongBlockingMob = isStaticObstacle;
				bool flag3 = this.motionController.CurrentBlockingMob == mob || (flag2 && gameMobMotionControllerBase.IsMovementFreezed);
				if (flag2 && !flag3 && Vector2.Dot(this.motionController.CurrentVelocity, gameMobMotionControllerBase.CurrentVelocity) > 0.5f)
				{
					return false;
				}
				if (this.currentGroup != null && group != null && this.currentGroup.GroupID == group.GroupID)
				{
					if (!isStaticObstacle)
					{
						GameMobMotionController gameMobMotionController = gameMobMotionControllerBase as GameMobMotionController;
						if (gameMobMotionController != null)
						{
							if (gameMobMotionController.IsGroupDestinationReached)
							{
								isStrongBlockingMob = true;
								goto IL_1B4;
							}
							goto IL_1B4;
						}
					}
					isStrongBlockingMob |= (mob.CrowdPassPriority >= this.currentMob.CrowdPassPriority);
				}
				IL_1B4:
				if (flag)
				{
					return isStrongBlockingMob || flag3;
				}
				if (!isStrongBlockingMob)
				{
					GameMobAIController aicontroller = mob.AIController;
					if (flag2)
					{
						BaseGameMob currentBlockingMob = gameMobMotionControllerBase.CurrentBlockingMob;
						if (currentBlockingMob != null && currentBlockingMob.isCrowdObstacle)
						{
							BaseGameMob currentBlockingMob2 = gameMobMotionControllerBase.CurrentBlockingMob;
							GameMobMotionControllerBase gameMobMotionControllerBase2 = (currentBlockingMob2 != null) ? currentBlockingMob2.MotionController : null;
							isStrongBlockingMob |= (gameMobMotionControllerBase.CurrentBlockingMob.IsStaticCrowdObstacle || gameMobMotionControllerBase2 == null || gameMobMotionControllerBase2.IsMovementFreezed);
						}
					}
					if (this.currentAIController != null)
					{
						IGameMob currentAttackTarget = this.currentAIController.CurrentAttackTarget;
						isStrongBlockingMob |= (currentAttackTarget == mob || this.currentAIController.IsEnemyTarget(mob));
					}
					if (aicontroller != null)
					{
						isStrongBlockingMob |= aicontroller.HasBlockingAttackTarget();
					}
				}
				return isStrongBlockingMob || flag3;
			}
		}

		// Token: 0x06001242 RID: 4674 RVA: 0x000397E0 File Offset: 0x000379E0
		private void Reset()
		{
			this.maxBlockingMobWeight = 0f;
			for (int i = 0; i < 8; i++)
			{
				this.interestMap[i] = 0f;
				this.dangerMap[i] = 0f;
			}
		}

		// Token: 0x06001243 RID: 4675 RVA: 0x0003981E File Offset: 0x00037A1E
		public GameMobSteeringContext(BaseGameMob mob, GameMobMotionController motionController)
		{
			this.currentMob = mob;
			this.motionController = motionController;
			this.interestMap = new float[8];
			this.dangerMap = new float[8];
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x0003985C File Offset: 0x00037A5C
		public void Update(LocationChunkMobsGridController.GridAgent currentGridAgent, out BaseGameMob blockingMob, out bool isStrongBlocking, out float speedCoeff, out bool isFollowingLeader)
		{
			this.Reset();
			blockingMob = null;
			isStrongBlocking = false;
			speedCoeff = 1f;
			isFollowingLeader = false;
			Vector2 position = this.currentMob.Position;
			Vector2 waypointPosition = this.motionController.GetWaypointPosition();
			Vector2 vector = waypointPosition - position;
			vector.Normalize();
			int movementBlockingMobLayers = this.motionController.movementBlockingMobLayers;
			UniformGrid2D grid = currentGridAgent.Controller.Grid;
			this.currentGroup = (this.currentMob.Group as GameMobGroupController);
			this.currentAIController = this.currentMob.AIController;
			this.UpdateMap(vector, true, false);
			if (this.avoidNavmeshEdges)
			{
				NavMeshAgent navMeshAgent = this.currentMob.NavMeshAgent;
				NavMeshHit navMeshHit;
				if (navMeshAgent != null && navMeshAgent.FindClosestEdge(out navMeshHit) && navMeshHit.distance < this.currentMob.Radius + 0.7f)
				{
					this.UpdateMap(navMeshHit.normal, false, true);
				}
			}
			if (grid != null)
			{
				float radius = this.currentMob.Radius;
				float range = radius + 0.5f;
				int[] array;
				int agentsInRange = grid.GetAgentsInRange(position, range, out array, movementBlockingMobLayers);
				bool flag = this.currentGroup != null && this.currentGroup.IsFollowingGroupLeader;
				IGameMob gameMob = flag ? this.currentGroup.Leader : null;
				for (int i = 0; i < agentsInRange; i++)
				{
					LocationChunkMobsGridController.GridAgent gridAgent = (LocationChunkMobsGridController.GridAgent)grid.GetAgent(array[i]);
					BaseGameMob linkedMob = gridAgent.LinkedMob;
					if (!(linkedMob == this.currentMob) && !linkedMob.IsKilled && !linkedMob.IsKinematic)
					{
						Vector2 a = (gridAgent.IsCompoundAgent ? gridAgent.GetPosition() : linkedMob.Position) - position;
						float magnitude = a.magnitude;
						Vector2 vector2 = a / magnitude;
						speedCoeff = 1f;
						if (flag && !isFollowingLeader && !(isFollowingLeader = (linkedMob == gameMob)) && linkedMob.Group == this.currentGroup)
						{
							GameMobMotionController gameMobMotionController = linkedMob.MotionController as GameMobMotionController;
							isFollowingLeader = (gameMobMotionController != null && gameMobMotionController.IsFollowingGroupLeader);
						}
						bool flag2;
						bool flag3;
						float d;
						if (this.IsBlockingMob(linkedMob, waypointPosition, out flag2, out flag3, out d))
						{
							if (Vector2.Dot(vector2, vector) > 0.5f)
							{
								float num = Mathf.Clamp((magnitude - (radius + linkedMob.Radius)) / 0.25f, 0.2f, 1f);
								if (num < speedCoeff)
								{
									speedCoeff = num;
								}
							}
							if (this.avoidObstacles)
							{
								this.UpdateMap(vector2 * d, false, flag3);
							}
							float num2 = Vector2.Dot(vector2, vector);
							if (num2 > 0.3f)
							{
								if (flag3)
								{
									num2 += (flag2 ? 1.3f : 1f);
								}
								if (num2 > this.maxBlockingMobWeight)
								{
									blockingMob = linkedMob;
									isStrongBlocking = flag3;
									this.maxBlockingMobWeight = num2;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x00039B38 File Offset: 0x00037D38
		public bool TryGetMovementDirection(out Vector2 movementDirection)
		{
			movementDirection = default(Vector2);
			bool flag = false;
			float num = 0f;
			bool flag2 = this.motionController.IsGroupDestinationReached && (this.currentGroup == null || !this.currentGroup.InBattle);
			for (int i = 0; i < 8; i++)
			{
				float num2 = this.dangerMap[i];
				float num3 = this.interestMap[i] - num2;
				if (flag2)
				{
					num += Mathf.Clamp01(num2);
					if (num > 4.3f)
					{
						movementDirection = default(Vector2);
						return false;
					}
				}
				if (num3 > 0f)
				{
					movementDirection += GameMobSteeringContext.Directions[i] * num3;
					flag = true;
				}
			}
			if (flag)
			{
				movementDirection.Normalize();
			}
			return flag;
		}

		// Token: 0x06001246 RID: 4678 RVA: 0x00039BF8 File Offset: 0x00037DF8
		public void ShowDebugGizmo(float scale = 1f)
		{
			if (!Debug.isDebugBuild)
			{
				return;
			}
			Vector2 position = this.currentMob.Position;
			float radius = this.currentMob.Radius;
			if (this.currentMob.IsActiveNavMeshAgent())
			{
				Debug.DrawLine(position, this.motionController.GetWaypointPosition(), Color.white.SetA(0.5f));
			}
			for (int i = 0; i < 8; i++)
			{
				float num = this.dangerMap[i];
				float num2 = this.interestMap[i];
				Vector2 a = GameMobSteeringContext.Directions[i];
				Vector2 v = position + a * radius;
				Debug.DrawRay(v, num2 * scale * a, Color.blue);
				Debug.DrawRay(v, num * scale * a, Color.red);
			}
			Vector2 a2;
			if (this.TryGetMovementDirection(out a2))
			{
				Debug.DrawRay(position + a2 * radius, a2 * scale * 1.25f, Color.cyan);
			}
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x00039DFC File Offset: 0x00037FFC
		[CompilerGenerated]
		internal static void <UpdateMap>g__UpdateMap|17_0(float[] map, int index, float newValue, bool additive)
		{
			ref float ptr = ref map[index];
			if (additive)
			{
				ptr += Mathf.Max(newValue, 0f);
				return;
			}
			if (newValue > ptr)
			{
				ptr = newValue;
			}
		}

		// Token: 0x04000A6E RID: 2670
		public const int DirectionsCount = 8;

		// Token: 0x04000A6F RID: 2671
		private static readonly Vector2[] Directions = new Vector2[]
		{
			new Vector2(-1f, 0f),
			new Vector2(-1f, 1f).normalized,
			new Vector2(0f, 1f),
			new Vector2(1f, 1f).normalized,
			new Vector2(1f, 0f),
			new Vector2(1f, -1f).normalized,
			new Vector2(0f, -1f),
			new Vector2(-1f, -1f).normalized
		};

		// Token: 0x04000A70 RID: 2672
		public bool avoidObstacles = true;

		// Token: 0x04000A71 RID: 2673
		public bool avoidNavmeshEdges = true;

		// Token: 0x04000A72 RID: 2674
		private readonly BaseGameMob currentMob;

		// Token: 0x04000A73 RID: 2675
		private readonly GameMobMotionController motionController;

		// Token: 0x04000A74 RID: 2676
		private readonly float[] interestMap;

		// Token: 0x04000A75 RID: 2677
		private readonly float[] dangerMap;

		// Token: 0x04000A76 RID: 2678
		private GameMobGroupController currentGroup;

		// Token: 0x04000A77 RID: 2679
		private GameMobAIController currentAIController;

		// Token: 0x04000A78 RID: 2680
		private float maxBlockingMobWeight;
	}
}
