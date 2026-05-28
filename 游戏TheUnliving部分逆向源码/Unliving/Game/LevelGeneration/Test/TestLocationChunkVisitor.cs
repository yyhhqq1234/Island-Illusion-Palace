using System;
using Common.UnityExtensions;
using UnityEngine;
using UnityEngine.AI;

namespace Game.LevelGeneration.Test
{
	// Token: 0x02000006 RID: 6
	public sealed class TestLocationChunkVisitor : MonoBehaviour, ILocationChunkVisitor, ILocationObject
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000010 RID: 16 RVA: 0x00002515 File Offset: 0x00000715
		public bool AffectLocationChunkVisibility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000011 RID: 17 RVA: 0x00002518 File Offset: 0x00000718
		// (set) Token: 0x06000012 RID: 18 RVA: 0x00002520 File Offset: 0x00000720
		public ILocationChunk CurrentLocationChunk
		{
			get
			{
				return this._currentLocationChunk;
			}
			set
			{
				if (this._currentLocationChunk != value)
				{
					if (this.isLocationExplorer)
					{
						IGameLocation gameLocation = (value != null) ? value.CurrentLocation : null;
						if (gameLocation != null)
						{
							gameLocation.LocationExplorer = this;
						}
					}
					this._currentLocationChunk = value;
				}
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000013 RID: 19 RVA: 0x0000255C File Offset: 0x0000075C
		// (set) Token: 0x06000014 RID: 20 RVA: 0x0000256E File Offset: 0x0000076E
		public Vector2 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000015 RID: 21 RVA: 0x00002581 File Offset: 0x00000781
		public float Orientation
		{
			get
			{
				return base.transform.GetRotation2D(false);
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x06000016 RID: 22 RVA: 0x00002590 File Offset: 0x00000790
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000017 RID: 23 RVA: 0x000025A6 File Offset: 0x000007A6
		bool ILocationObject.IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000018 RID: 24 RVA: 0x000025AC File Offset: 0x000007AC
		private void SetDestination(Vector2? newDestination, bool force = false)
		{
			if (this.navMeshAgent == null)
			{
				return;
			}
			if (force || this.destination != newDestination)
			{
				if (newDestination != null)
				{
					if (!this.CurrentLocationChunk.ContainsPoint(newDestination.Value))
					{
						return;
					}
					this.desiredVelocity = default(Vector3);
					if (this.rigidbody != null)
					{
						this.rigidbody.velocity = default(Vector2);
					}
					this.navMeshAgent.enabled = true;
					this.navMeshAgent.SetDestination(newDestination.Value);
				}
				else if (this.navMeshAgent.enabled)
				{
					this.navMeshAgent.ResetPath();
					this.navMeshAgent.enabled = false;
				}
				this.destination = newDestination;
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x000026A8 File Offset: 0x000008A8
		public bool CanVisitLocationChunk(ILocationChunk locationChunk)
		{
			return locationChunk != null;
		}

		// Token: 0x0600001A RID: 26 RVA: 0x000026B0 File Offset: 0x000008B0
		public void OnReachingLocationChunkEntrancePoint(ILocationChunkEntrancePoint entrancePoint)
		{
			this.SetDestination(null, true);
		}

		// Token: 0x0600001B RID: 27 RVA: 0x000026CD File Offset: 0x000008CD
		public void OnAddedToLocationChunk(ILocationChunk locationChunk)
		{
		}

		// Token: 0x0600001C RID: 28 RVA: 0x000026CF File Offset: 0x000008CF
		void ILocationChunkVisitor.OnChunkTransitionStateChanged(ILocationChunkTransitionArea transitionArea, bool isActive)
		{
		}

		// Token: 0x0600001D RID: 29 RVA: 0x000026D4 File Offset: 0x000008D4
		private void Awake()
		{
			this.rigidbody = base.GetComponent<Rigidbody2D>();
			this.navMeshAgent = base.GetComponent<NavMeshAgent>();
			if (this.navMeshAgent != null)
			{
				this.navMeshAgent.updateUpAxis = false;
				this.navMeshAgent.updateRotation = false;
			}
			this.SetDestination(null, true);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00002730 File Offset: 0x00000930
		private void Update()
		{
			float deltaTime = Time.deltaTime;
			Vector3 vector = default(Vector3);
			vector.x = Input.GetAxisRaw("Horizontal");
			vector.y = Input.GetAxisRaw("Vertical");
			this.desiredVelocity = vector.normalized * this.movementSpeed;
			if (this.desiredVelocity.x != 0f || this.desiredVelocity.y != 0f)
			{
				this.SetDestination(null, false);
			}
			if (Input.GetMouseButtonDown(0))
			{
				this.SetDestination(new Vector2?(Camera.main.ScreenToWorldPoint(Input.mousePosition)), false);
			}
			if (!(this.rigidbody != null))
			{
				base.transform.position += this.desiredVelocity;
				return;
			}
			if (this.rigidbody.isKinematic)
			{
				this.rigidbody.MovePosition(this.rigidbody.position + this.desiredVelocity * deltaTime);
				return;
			}
			this.rigidbody.velocity = this.desiredVelocity;
		}

		// Token: 0x0400000C RID: 12
		public float movementSpeed = 5f;

		// Token: 0x0400000D RID: 13
		public bool isLocationExplorer = true;

		// Token: 0x0400000E RID: 14
		private Rigidbody2D rigidbody;

		// Token: 0x0400000F RID: 15
		private NavMeshAgent navMeshAgent;

		// Token: 0x04000010 RID: 16
		private Vector2? destination;

		// Token: 0x04000011 RID: 17
		private Vector3 desiredVelocity;

		// Token: 0x04000012 RID: 18
		private ILocationChunk _currentLocationChunk;
	}
}
