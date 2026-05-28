using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Core;
using Game.LevelGeneration;
using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unliving.LevelGeneration;

namespace Unliving.Mobs
{
	// Token: 0x020001EC RID: 492
	public sealed class LocationChunkMobsGridController : GameBehaviourBase
	{
		// Token: 0x0600104C RID: 4172 RVA: 0x00032B20 File Offset: 0x00030D20
		private static float GetMobRadius(BaseGameMob mob, out Bounds crowdObstacleBounds)
		{
			crowdObstacleBounds = default(Bounds);
			if (mob.IsStaticCrowdObstacle && mob.HitCollider != null)
			{
				crowdObstacleBounds = mob.HitCollider.bounds;
				Vector2 vector = crowdObstacleBounds.extents;
				return Mathf.Min(vector.x, vector.y) + 0.1f;
			}
			return mob.GetCrowdInteractionRadius();
		}

		// Token: 0x17000353 RID: 851
		// (get) Token: 0x0600104D RID: 4173 RVA: 0x00032B85 File Offset: 0x00030D85
		public LocationChunk CurrentLocationChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
		}

		// Token: 0x17000354 RID: 852
		// (get) Token: 0x0600104E RID: 4174 RVA: 0x00032B8D File Offset: 0x00030D8D
		public UniformGrid2D Grid
		{
			get
			{
				return this.grid;
			}
		}

		// Token: 0x0600104F RID: 4175 RVA: 0x00032B95 File Offset: 0x00030D95
		private void TryUpdateMaxAgentRadius(float agentRadius)
		{
			if (agentRadius > this.maxGridAgentRadius)
			{
				this.maxGridAgentRadius = agentRadius;
			}
		}

		// Token: 0x06001050 RID: 4176 RVA: 0x00032BA8 File Offset: 0x00030DA8
		private void TryAddAgentToGrid(LocationChunkMobsGridController.GridAgent agent, float agentRadius = 0f)
		{
			if (this.grid == null)
			{
				Bounds bounds;
				this.TryUpdateMaxAgentRadius((agentRadius <= 0f) ? LocationChunkMobsGridController.GetMobRadius(agent.LinkedMob, out bounds) : agentRadius);
				List<LocationChunkMobsGridController.GridAgent> list;
				if ((list = this.initialAgentsBuffer) == null)
				{
					list = (this.initialAgentsBuffer = new List<LocationChunkMobsGridController.GridAgent>(64));
				}
				list.Add(agent);
				return;
			}
			this.grid.AddAgent(agent);
		}

		// Token: 0x06001051 RID: 4177 RVA: 0x00032C08 File Offset: 0x00030E08
		public void RegisterMob(BaseGameMob mob)
		{
			if (mob == null)
			{
				return;
			}
			int instanceID = mob.GetInstanceID();
			if (this.registeredAgents.ContainsKey(instanceID))
			{
				return;
			}
			Bounds bounds;
			float mobRadius = LocationChunkMobsGridController.GetMobRadius(mob, out bounds);
			LocationChunkMobsGridController.GridAgent gridAgent;
			if (bounds.size != default(Vector3))
			{
				Vector2 vector = bounds.extents;
				float num;
				Vector2 vector2;
				if (vector.x > vector.y)
				{
					num = vector.x;
					vector2 = new Vector2
					{
						x = 1f
					};
				}
				else
				{
					num = vector.y;
					vector2 = new Vector2
					{
						y = 1f
					};
				}
				int num2 = (int)(num * 2f / mobRadius);
				if (num2 > 1)
				{
					Vector2 vector3 = bounds.center - (Vector2.Dot(vector2, vector) - mobRadius) * vector2;
					float num3 = (float)(num2 + 2) * mobRadius;
					float num4 = num * 2f - num3;
					Vector2 b = vector2 * (mobRadius + num4 / (float)num2);
					LocationChunkMobsGridController.GridAgent[] array = new LocationChunkMobsGridController.GridAgent[num2];
					gridAgent = new LocationChunkMobsGridController.GridAgent(this, mob, vector3, mobRadius, array);
					this.TryAddAgentToGrid(gridAgent, mobRadius);
					for (int i = 0; i < num2; i++)
					{
						vector3 += b;
						array[i] = new LocationChunkMobsGridController.GridAgent(gridAgent, vector3, mobRadius);
						this.TryAddAgentToGrid(array[i], mobRadius);
					}
				}
				else
				{
					gridAgent = new LocationChunkMobsGridController.GridAgent(this, mob, bounds.center, mobRadius, Array.Empty<LocationChunkMobsGridController.GridAgent>());
					this.TryAddAgentToGrid(gridAgent, mobRadius);
				}
			}
			else
			{
				gridAgent = new LocationChunkMobsGridController.GridAgent(this, mob, mobRadius);
				this.TryAddAgentToGrid(gridAgent, mobRadius);
			}
			this.registeredAgents.Add(instanceID, gridAgent);
		}

		// Token: 0x06001052 RID: 4178 RVA: 0x00032DB4 File Offset: 0x00030FB4
		public void UnregisterMob(BaseGameMob mob)
		{
			if (mob == null)
			{
				return;
			}
			int instanceID = mob.GetInstanceID();
			LocationChunkMobsGridController.GridAgent gridAgent;
			if (!this.registeredAgents.TryGetValue(instanceID, out gridAgent))
			{
				return;
			}
			this.registeredAgents.Remove(instanceID);
			if (this.grid != null)
			{
				this.grid.RemoveAgent(gridAgent.AgentID);
				return;
			}
			List<LocationChunkMobsGridController.GridAgent> list = this.initialAgentsBuffer;
			if (list == null)
			{
				return;
			}
			list.Remove(gridAgent);
		}

		// Token: 0x06001053 RID: 4179 RVA: 0x00032E20 File Offset: 0x00031020
		public LocationChunkMobsGridController.GridAgent GetGridAgent(BaseGameMob mob)
		{
			LocationChunkMobsGridController.GridAgent result;
			this.registeredAgents.TryGetValue(mob.GetInstanceID(), out result);
			return result;
		}

		// Token: 0x06001054 RID: 4180 RVA: 0x00032E42 File Offset: 0x00031042
		private void OnBeforeLocationChunkVisitorAdded(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.RegisterMob(visitor as BaseGameMob);
		}

		// Token: 0x06001055 RID: 4181 RVA: 0x00032E50 File Offset: 0x00031050
		private void OnLocationChunkVisitorRemoved(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.UnregisterMob(visitor as BaseGameMob);
		}

		// Token: 0x06001056 RID: 4182 RVA: 0x00032E60 File Offset: 0x00031060
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (LocationChunkMobsGridController.maxFactoryMobRadius < 0f)
			{
				GameMobsFactory gameMobsFactory = currentGame.Services.Get<GameMobsFactory>();
				if (gameMobsFactory != null)
				{
					foreach (MobBehaviour.FactoryPrototype factoryPrototype in gameMobsFactory.GetObjectPrototypes())
					{
						BaseGameMob baseGameMob;
						if (!(factoryPrototype.objectPrefab == null) && factoryPrototype.objectPrefab.TryGetComponent<BaseGameMob>(out baseGameMob))
						{
							float crowdInteractionRadius = baseGameMob.GetCrowdInteractionRadius();
							if (crowdInteractionRadius > LocationChunkMobsGridController.maxFactoryMobRadius)
							{
								LocationChunkMobsGridController.maxFactoryMobRadius = crowdInteractionRadius;
							}
						}
					}
				}
			}
			this.TryUpdateMaxAgentRadius(LocationChunkMobsGridController.maxFactoryMobRadius);
			if (base.TryGetComponent<LocationChunk>(out this.currentLocationChunk))
			{
				this.currentLocationChunk.BeforeVisitorAdded += this.OnBeforeLocationChunkVisitorAdded;
				this.currentLocationChunk.VisitorRemoved += this.OnLocationChunkVisitorRemoved;
			}
		}

		// Token: 0x06001057 RID: 4183 RVA: 0x00032F44 File Offset: 0x00031144
		private void Start()
		{
			if (this.currentLocationChunk == null)
			{
				base.enabled = false;
				return;
			}
			if (this.maxGridAgentRadius <= 0f)
			{
				base.enabled = false;
				return;
			}
			Rect boundsRect = this.currentLocationChunk.GetBoundsRect();
			this.grid = new UniformGrid2D(boundsRect.position, boundsRect.size, Mathf.Max(this.maxGridAgentRadius * 3f, 6f), 1024);
			this.grid.Origin = boundsRect.center - this.grid.Size * 0.5f;
			if (this.initialAgentsBuffer != null)
			{
				for (int i = 0; i < this.initialAgentsBuffer.Count; i++)
				{
					this.grid.AddAgent(this.initialAgentsBuffer[i]);
				}
				this.initialAgentsBuffer.Clear();
				this.initialAgentsBuffer.TrimExcess();
				this.initialAgentsBuffer = null;
			}
		}

		// Token: 0x06001058 RID: 4184 RVA: 0x0003303A File Offset: 0x0003123A
		private void LateUpdate()
		{
			UniformGrid2D uniformGrid2D = this.grid;
			if (uniformGrid2D == null)
			{
				return;
			}
			uniformGrid2D.UpdateGrid();
		}

		// Token: 0x06001059 RID: 4185 RVA: 0x0003304C File Offset: 0x0003124C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.currentLocationChunk != null)
			{
				this.currentLocationChunk.BeforeVisitorAdded -= this.OnBeforeLocationChunkVisitorAdded;
				this.currentLocationChunk.VisitorRemoved -= this.OnLocationChunkVisitorRemoved;
			}
		}

		// Token: 0x0600105A RID: 4186 RVA: 0x0003309B File Offset: 0x0003129B
		private void OnDrawGizmosSelected()
		{
			UniformGrid2D uniformGrid2D = this.grid;
			if (uniformGrid2D == null)
			{
				return;
			}
			uniformGrid2D.DrawGizmos(Color.white, Color.magenta, new Action<UniformGrid2D.IAgent, Vector2, Color>(LocationChunkMobsGridController.<OnDrawGizmosSelected>g__DrawAgent|24_0));
		}

		// Token: 0x0600105D RID: 4189 RVA: 0x000330F8 File Offset: 0x000312F8
		[CompilerGenerated]
		internal static void <OnDrawGizmosSelected>g__DrawAgent|24_0(UniformGrid2D.IAgent agent, Vector2 nodePosition, Color defaultColor)
		{
			LocationChunkMobsGridController.GridAgent gridAgent = agent as LocationChunkMobsGridController.GridAgent;
			if (gridAgent != null)
			{
				Gizmos.color = (gridAgent.IsCompoundAgent ? Color.magenta : Color.yellow);
				Gizmos.DrawWireSphere(gridAgent.GetPosition(), agent.Radius);
				return;
			}
			Gizmos.color = defaultColor;
			Gizmos.DrawWireSphere(nodePosition, agent.Radius);
		}

		// Token: 0x04000946 RID: 2374
		private static readonly LocationChunkMobsGridController.GridAgent[] QueriesBuffer = new LocationChunkMobsGridController.GridAgent[256];

		// Token: 0x04000947 RID: 2375
		private static float maxFactoryMobRadius = -1f;

		// Token: 0x04000948 RID: 2376
		private readonly Dictionary<int, LocationChunkMobsGridController.GridAgent> registeredAgents = new Dictionary<int, LocationChunkMobsGridController.GridAgent>(300);

		// Token: 0x04000949 RID: 2377
		private LocationChunk currentLocationChunk;

		// Token: 0x0400094A RID: 2378
		private float maxGridAgentRadius;

		// Token: 0x0400094B RID: 2379
		private List<LocationChunkMobsGridController.GridAgent> initialAgentsBuffer;

		// Token: 0x0400094C RID: 2380
		private UniformGrid2D grid;

		// Token: 0x020004A4 RID: 1188
		public sealed class GridAgent : UniformGrid2D.IAgent, IEquatable<LocationChunkMobsGridController.GridAgent>
		{
			// Token: 0x17000765 RID: 1893
			// (get) Token: 0x06002478 RID: 9336 RVA: 0x00070E9C File Offset: 0x0006F09C
			public LocationChunkMobsGridController Controller
			{
				get
				{
					return this.controller;
				}
			}

			// Token: 0x17000766 RID: 1894
			// (get) Token: 0x06002479 RID: 9337 RVA: 0x00070EA4 File Offset: 0x0006F0A4
			public BaseGameMob LinkedMob
			{
				get
				{
					return this.linkedMob;
				}
			}

			// Token: 0x17000767 RID: 1895
			// (get) Token: 0x0600247A RID: 9338 RVA: 0x00070EAC File Offset: 0x0006F0AC
			public int Layer
			{
				get
				{
					return this.linkedMob.gameObject.layer;
				}
			}

			// Token: 0x17000768 RID: 1896
			// (get) Token: 0x0600247B RID: 9339 RVA: 0x00070EBE File Offset: 0x0006F0BE
			public int AgentID
			{
				get
				{
					return this.agentID;
				}
			}

			// Token: 0x17000769 RID: 1897
			// (get) Token: 0x0600247C RID: 9340 RVA: 0x00070EC6 File Offset: 0x0006F0C6
			public float Radius
			{
				get
				{
					return this.radius;
				}
			}

			// Token: 0x1700076A RID: 1898
			// (get) Token: 0x0600247D RID: 9341 RVA: 0x00070ECE File Offset: 0x0006F0CE
			object UniformGrid2D.IAgent.ParentBehaviour
			{
				get
				{
					return this.linkedMob;
				}
			}

			// Token: 0x1700076B RID: 1899
			// (get) Token: 0x0600247E RID: 9342 RVA: 0x00070ED6 File Offset: 0x0006F0D6
			bool UniformGrid2D.IAgent.IsStatic
			{
				get
				{
					return this.IsCompoundAgent;
				}
			}

			// Token: 0x0600247F RID: 9343 RVA: 0x00070EE0 File Offset: 0x0006F0E0
			public GridAgent(LocationChunkMobsGridController controller, BaseGameMob mob, float customRadius = -1f)
			{
				this.controller = controller;
				this.linkedMob = mob;
				this.mobTransform = mob.transform;
				this.mobNavmeshAgent = mob.NavMeshAgent;
				this.radius = ((customRadius > 0f) ? customRadius : mob.GetCrowdInteractionRadius());
				this.agentID = -1;
				this.name = mob.CachedName;
			}

			// Token: 0x06002480 RID: 9344 RVA: 0x00070F43 File Offset: 0x0006F143
			public GridAgent(LocationChunkMobsGridController controller, BaseGameMob mob, Vector2 position, float radius, LocationChunkMobsGridController.GridAgent[] childAgents) : this(controller, mob, radius)
			{
				this.childAgents = childAgents;
				this.staticPosition = position;
				this.IsCompoundAgent = true;
			}

			// Token: 0x06002481 RID: 9345 RVA: 0x00070F68 File Offset: 0x0006F168
			public GridAgent(LocationChunkMobsGridController.GridAgent parentAgent, Vector2 position, float radius)
			{
				this.parentAgent = parentAgent;
				this.controller = parentAgent.Controller;
				this.linkedMob = parentAgent.LinkedMob;
				this.radius = radius;
				this.staticPosition = position;
				this.agentID = -1;
				this.IsCompoundAgent = true;
			}

			// Token: 0x06002482 RID: 9346 RVA: 0x00070FB6 File Offset: 0x0006F1B6
			private void RemoveCompoundChildFromGrid()
			{
				this.parentAgent = null;
				this.controller.grid.RemoveAgent(this.agentID);
			}

			// Token: 0x06002483 RID: 9347 RVA: 0x00070FD6 File Offset: 0x0006F1D6
			public bool Equals(LocationChunkMobsGridController.GridAgent other)
			{
				return this.linkedMob == ((other != null) ? other.LinkedMob : null);
			}

			// Token: 0x06002484 RID: 9348 RVA: 0x00070FEF File Offset: 0x0006F1EF
			public Vector2 GetPosition()
			{
				return this.currentPosition;
			}

			// Token: 0x06002485 RID: 9349 RVA: 0x00070FF8 File Offset: 0x0006F1F8
			public int GetIntersectingAgents(out LocationChunkMobsGridController.GridAgent[] agentsBuffer, int allowedLayers)
			{
				agentsBuffer = LocationChunkMobsGridController.QueriesBuffer;
				if (this.agentID != -1)
				{
					UniformGrid2D grid = this.controller.grid;
					int[] array;
					int intersectingAgents = grid.GetIntersectingAgents(this.agentID, out array, allowedLayers, default(Vector2));
					int num = 0;
					int num2 = 0;
					while (num2 < intersectingAgents && num < agentsBuffer.Length)
					{
						LocationChunkMobsGridController.GridAgent gridAgent = grid.GetAgent(array[num2]) as LocationChunkMobsGridController.GridAgent;
						if (gridAgent != null)
						{
							agentsBuffer[num++] = gridAgent;
						}
						num2++;
					}
					return num;
				}
				return 0;
			}

			// Token: 0x06002486 RID: 9350 RVA: 0x00071078 File Offset: 0x0006F278
			void UniformGrid2D.IAgent.UpdateState(ref Vector2 position)
			{
				if (this.IsCompoundAgent)
				{
					position = this.staticPosition;
				}
				else if (this.mobNavmeshAgent != null && this.mobNavmeshAgent.updatePosition)
				{
					position = this.mobNavmeshAgent.nextPosition;
				}
				else if (this.mobTransform != null)
				{
					position = this.mobTransform.position;
				}
				this.currentPosition = position;
			}

			// Token: 0x06002487 RID: 9351 RVA: 0x00071100 File Offset: 0x0006F300
			void UniformGrid2D.IAgent.OnAgentIndexChanged(int newAgentID)
			{
				this.agentID = newAgentID;
				if (this.IsCompoundAgent && newAgentID < 0)
				{
					if (this.parentAgent != null)
					{
						this.controller.UnregisterMob(this.linkedMob);
						return;
					}
					if (this.childAgents != null)
					{
						for (int i = 0; i < this.childAgents.Length; i++)
						{
							this.childAgents[i].RemoveCompoundChildFromGrid();
						}
					}
				}
			}

			// Token: 0x04001902 RID: 6402
			public readonly bool IsCompoundAgent;

			// Token: 0x04001903 RID: 6403
			public LocationChunkMobsGridController.GridAgent parentAgent;

			// Token: 0x04001904 RID: 6404
			private readonly LocationChunkMobsGridController controller;

			// Token: 0x04001905 RID: 6405
			private readonly BaseGameMob linkedMob;

			// Token: 0x04001906 RID: 6406
			private readonly Transform mobTransform;

			// Token: 0x04001907 RID: 6407
			private readonly NavMeshAgent mobNavmeshAgent;

			// Token: 0x04001908 RID: 6408
			private readonly float radius;

			// Token: 0x04001909 RID: 6409
			private readonly LocationChunkMobsGridController.GridAgent[] childAgents;

			// Token: 0x0400190A RID: 6410
			private readonly Vector2 staticPosition;

			// Token: 0x0400190B RID: 6411
			private readonly string name;

			// Token: 0x0400190C RID: 6412
			private int agentID;

			// Token: 0x0400190D RID: 6413
			private Vector2 currentPosition;
		}
	}
}
