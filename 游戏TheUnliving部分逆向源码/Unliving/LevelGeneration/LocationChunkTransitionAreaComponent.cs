using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Game.Core;
using Game.LevelGeneration;
using Game.LevelGeneration.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unliving.GameScene;
using Unliving.Mobs;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026D RID: 621
	public sealed class LocationChunkTransitionAreaComponent : MonoBehaviour, ILocationChunkTransitionArea
	{
		// Token: 0x06001544 RID: 5444 RVA: 0x00043D7C File Offset: 0x00041F7C
		private static GameObject GetAgentObject(Collider2D agentCollider)
		{
			Rigidbody2D attachedRigidbody = agentCollider.attachedRigidbody;
			if (!(attachedRigidbody != null))
			{
				return agentCollider.gameObject;
			}
			return attachedRigidbody.gameObject;
		}

		// Token: 0x17000483 RID: 1155
		// (get) Token: 0x06001545 RID: 5445 RVA: 0x00043DA6 File Offset: 0x00041FA6
		public bool IsActive
		{
			get
			{
				return base.enabled;
			}
		}

		// Token: 0x17000484 RID: 1156
		// (get) Token: 0x06001546 RID: 5446 RVA: 0x00043DAE File Offset: 0x00041FAE
		public Collider2D Area
		{
			get
			{
				return this.area;
			}
		}

		// Token: 0x17000485 RID: 1157
		// (get) Token: 0x06001547 RID: 5447 RVA: 0x00043DB6 File Offset: 0x00041FB6
		public Rect AreaRect
		{
			get
			{
				return this.areaRect;
			}
		}

		// Token: 0x06001548 RID: 5448 RVA: 0x00043DC0 File Offset: 0x00041FC0
		private bool IsValidAgent(GameObject agentObject)
		{
			int num = 1 << agentObject.layer;
			return (this.allowedAgentLayers.value & num) != 0;
		}

		// Token: 0x06001549 RID: 5449 RVA: 0x00043DEC File Offset: 0x00041FEC
		private void SetAgentState(GameObject agentObject, bool isRegistered)
		{
			int instanceID = agentObject.GetInstanceID();
			NavMeshAgent navMeshAgent;
			if (!agentObject.TryGetComponent<NavMeshAgent>(out navMeshAgent))
			{
				return;
			}
			int num;
			bool flag = this.storedAgentTypes.TryGetValue(instanceID, out num);
			IGameMob gameMob;
			navMeshAgent.TryGetComponent<IGameMob>(out gameMob);
			if (!isRegistered)
			{
				if (flag)
				{
					if (gameMob != null)
					{
						gameMob.Killed -= this.OnMobKilled;
					}
					navMeshAgent.agentTypeID = this.storedAgentTypes[instanceID];
					this.storedAgentTypes.Remove(instanceID);
					ILocationChunkVisitor locationChunkVisitor = gameMob as ILocationChunkVisitor;
					if (locationChunkVisitor != null)
					{
						if (!gameMob.IsKilled)
						{
							IGameBehaviour gameBehaviour = gameMob as IGameBehaviour;
							object obj;
							if (gameBehaviour == null)
							{
								obj = null;
							}
							else
							{
								IGameLocationProvider gameLocationProvider = gameBehaviour.CurrentGame.Services.Get<IGameLocationProvider>();
								obj = ((gameLocationProvider != null) ? gameLocationProvider.CurrentLocation : null);
							}
							ILocationChunk currentLocationChunk = locationChunkVisitor.CurrentLocationChunk;
							object obj2 = obj;
							ILocationChunk locationChunk = (obj2 != null) ? obj2.GetNextIntersectedChunk(locationChunkVisitor, true) : null;
							if (locationChunk != null && currentLocationChunk != locationChunk)
							{
								if (currentLocationChunk != null)
								{
									currentLocationChunk.RemoveVisitor(locationChunkVisitor);
								}
								locationChunk.AddVisitor(locationChunkVisitor);
							}
						}
						locationChunkVisitor.OnChunkTransitionStateChanged(this, false);
					}
				}
				return;
			}
			if (flag || !this.IsValidAgent(agentObject))
			{
				return;
			}
			if (gameMob != null)
			{
				if (gameMob.IsKilled)
				{
					return;
				}
				gameMob.Killed += this.OnMobKilled;
			}
			this.storedAgentTypes.Add(instanceID, navMeshAgent.agentTypeID);
			navMeshAgent.agentTypeID = this.targetAgentTypeID.Value;
			navMeshAgent.Warp(gameMob.Position);
			ILocationChunkVisitor locationChunkVisitor2 = gameMob as ILocationChunkVisitor;
			if (locationChunkVisitor2 == null)
			{
				return;
			}
			locationChunkVisitor2.OnChunkTransitionStateChanged(this, true);
		}

		// Token: 0x0600154A RID: 5450 RVA: 0x00043F52 File Offset: 0x00042152
		private void SyncWithGateways()
		{
			base.enabled = ((this.gateway0 == null || !this.gateway0.IsLocked) && (this.gateway1 == null || !this.gateway1.IsLocked));
		}

		// Token: 0x0600154B RID: 5451 RVA: 0x00043F8C File Offset: 0x0004218C
		public void SetSize(Vector2 size)
		{
			BoxCollider2D boxCollider2D;
			if (!base.TryGetComponent<BoxCollider2D>(out boxCollider2D))
			{
				return;
			}
			base.transform.localScale = Vector3.one;
			boxCollider2D.size = size;
		}

		// Token: 0x0600154C RID: 5452 RVA: 0x00043FBC File Offset: 0x000421BC
		public void SetGateways(ILocationChunkGateway gateway0, ILocationChunkGateway gateway1)
		{
			if (this.gateway0 != null)
			{
				this.gateway0.LockStateChanged -= this.OnGatewayLockStateChanged;
			}
			if (this.gateway1 != null)
			{
				this.gateway1.LockStateChanged -= this.OnGatewayLockStateChanged;
			}
			if (gateway0 != null)
			{
				gateway0.LockStateChanged += this.OnGatewayLockStateChanged;
			}
			if (gateway1 != null)
			{
				gateway1.LockStateChanged += this.OnGatewayLockStateChanged;
			}
			this.gateway0 = gateway0;
			this.gateway1 = gateway1;
			this.SyncWithGateways();
		}

		// Token: 0x0600154D RID: 5453 RVA: 0x00044048 File Offset: 0x00042248
		public bool IsIntersected(ILocationChunkVisitor visitor)
		{
			if (base.enabled)
			{
				float num = 0f;
				IBoundingCircle boundingCircle = visitor as IBoundingCircle;
				Vector2 position;
				if (boundingCircle != null)
				{
					position = boundingCircle.Position;
					num = boundingCircle.Radius;
				}
				else
				{
					position = visitor.Position;
				}
				Vector2 min = this.areaRect.min;
				Vector2 max = this.areaRect.max;
				if (num > 0f)
				{
					min.x -= num;
					min.y -= num;
					max.x += num;
					max.y += num;
				}
				return position.x >= min.x && position.x <= max.x && position.y >= min.y && position.y <= max.y;
			}
			return false;
		}

		// Token: 0x0600154E RID: 5454 RVA: 0x0004411C File Offset: 0x0004231C
		private bool TryGetVisitorObject(ILocationChunkVisitor visitor, out GameObject visitorObject)
		{
			Component component = visitor as Component;
			if (component != null)
			{
				visitorObject = component.gameObject;
				return true;
			}
			visitorObject = null;
			return false;
		}

		// Token: 0x0600154F RID: 5455 RVA: 0x00044144 File Offset: 0x00042344
		public bool IsInTransition(ILocationChunkVisitor visitor)
		{
			GameObject gameObject;
			int num;
			return this.TryGetVisitorObject(visitor, out gameObject) && this.storedAgentTypes.TryGetValue(gameObject.GetInstanceID(), out num);
		}

		// Token: 0x06001550 RID: 5456 RVA: 0x00044174 File Offset: 0x00042374
		public void ForceStartTransition(ILocationChunkVisitor visitor)
		{
			GameObject agentObject;
			if (this.TryGetVisitorObject(visitor, out agentObject))
			{
				this.SetAgentState(agentObject, true);
			}
		}

		// Token: 0x06001551 RID: 5457 RVA: 0x00044194 File Offset: 0x00042394
		public void CancelTransition(ILocationChunkVisitor visitor)
		{
			GameObject agentObject;
			if (this.TryGetVisitorObject(visitor, out agentObject))
			{
				this.SetAgentState(agentObject, false);
			}
		}

		// Token: 0x06001552 RID: 5458 RVA: 0x000441B4 File Offset: 0x000423B4
		private void OnGatewayLockStateChanged(bool isLocked)
		{
			this.SyncWithGateways();
		}

		// Token: 0x06001553 RID: 5459 RVA: 0x000441BC File Offset: 0x000423BC
		private void OnMobKilled(IGameMob mob)
		{
			this.SetAgentState(mob.GameObject, false);
		}

		// Token: 0x06001554 RID: 5460 RVA: 0x000441CB File Offset: 0x000423CB
		private void Awake()
		{
			base.TryGetComponent<Collider2D>(out this.area);
		}

		// Token: 0x06001555 RID: 5461 RVA: 0x000441DA File Offset: 0x000423DA
		private void OnEnable()
		{
			if (this.area != null)
			{
				this.area.isTrigger = true;
				this.area.enabled = true;
			}
		}

		// Token: 0x06001556 RID: 5462 RVA: 0x00044202 File Offset: 0x00042402
		private void OnDisable()
		{
			if (this.area != null)
			{
				this.area.enabled = false;
			}
		}

		// Token: 0x06001557 RID: 5463 RVA: 0x00044220 File Offset: 0x00042420
		private void Start()
		{
			Bounds bounds = this.area.bounds;
			this.areaRect = new Rect(bounds.min, bounds.size);
			if (!string.IsNullOrEmpty(this.agentTypeOverride))
			{
				int settingsCount = NavMesh.GetSettingsCount();
				for (int i = 0; i < settingsCount; i++)
				{
					NavMeshBuildSettings settingsByIndex = NavMesh.GetSettingsByIndex(i);
					if (settingsByIndex.HasAgentType(this.agentTypeOverride))
					{
						this.targetAgentTypeID = new int?(settingsByIndex.agentTypeID);
						break;
					}
				}
			}
			if (this.targetAgentTypeID == null)
			{
				base.enabled = false;
				return;
			}
		}

		// Token: 0x06001558 RID: 5464 RVA: 0x000442B9 File Offset: 0x000424B9
		private void OnTriggerEnter2D(Collider2D collider)
		{
			this.SetAgentState(LocationChunkTransitionAreaComponent.GetAgentObject(collider), true);
		}

		// Token: 0x06001559 RID: 5465 RVA: 0x000442C8 File Offset: 0x000424C8
		private void OnTriggerExit2D(Collider2D collider)
		{
			GameObject agentObject = LocationChunkTransitionAreaComponent.GetAgentObject(collider);
			this.SetAgentState(agentObject, false);
			agentObject.GetComponentInParent<BaseGameMob>() != null;
		}

		// Token: 0x0600155A RID: 5466 RVA: 0x000442F4 File Offset: 0x000424F4
		private void OnDrawGizmosSelected()
		{
			LocationChunkTransitionAreaComponent.<OnDrawGizmosSelected>g__DrawGatewayLink|33_0(base.transform.position, this.gateway0);
			LocationChunkTransitionAreaComponent.<OnDrawGizmosSelected>g__DrawGatewayLink|33_0(base.transform.position, this.gateway1);
			Gizmos.DrawWireCube(this.areaRect.center, this.areaRect.size);
		}

		// Token: 0x0600155C RID: 5468 RVA: 0x00044374 File Offset: 0x00042574
		[CompilerGenerated]
		internal static void <OnDrawGizmosSelected>g__DrawGatewayLink|33_0(Vector3 position, ILocationChunkGateway gateway)
		{
			if (gateway == null)
			{
				return;
			}
			Gizmos.color = (gateway.IsLocked ? Color.red : Color.green);
			Gizmos.DrawRay(position, (gateway.Position - position).normalized * 2f);
		}

		// Token: 0x04000C54 RID: 3156
		public string agentTypeOverride;

		// Token: 0x04000C55 RID: 3157
		public LayerMask allowedAgentLayers = -1;

		// Token: 0x04000C56 RID: 3158
		private readonly Dictionary<int, int> storedAgentTypes = new Dictionary<int, int>(16);

		// Token: 0x04000C57 RID: 3159
		private Collider2D area;

		// Token: 0x04000C58 RID: 3160
		private Rect areaRect;

		// Token: 0x04000C59 RID: 3161
		private int? targetAgentTypeID;

		// Token: 0x04000C5A RID: 3162
		private ILocationChunkGateway gateway0;

		// Token: 0x04000C5B RID: 3163
		private ILocationChunkGateway gateway1;
	}
}
