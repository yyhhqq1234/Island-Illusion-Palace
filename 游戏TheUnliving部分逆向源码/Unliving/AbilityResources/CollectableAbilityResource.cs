using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common;
using Common.UnityExtensions;
using Game.Core;
using Game.Gameplay;
using Game.LevelGeneration;
using Game.ObjectPool;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs.VFX;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035C RID: 860
	public sealed class CollectableAbilityResource : MonoBehaviour, ILocationObject, IDestroyable
	{
		// Token: 0x170005CC RID: 1484
		// (get) Token: 0x06001C15 RID: 7189 RVA: 0x00058B07 File Offset: 0x00056D07
		// (set) Token: 0x06001C16 RID: 7190 RVA: 0x00058B27 File Offset: 0x00056D27
		public float GatheringMotionDuration
		{
			get
			{
				if (this.type != AbilityResourceType.Cadaver && this.type != AbilityResourceType.Corpse)
				{
					return this.gatheringMotionDuration;
				}
				return 0f;
			}
			set
			{
				this.gatheringMotionDuration = value;
			}
		}

		// Token: 0x170005CD RID: 1485
		// (get) Token: 0x06001C17 RID: 7191 RVA: 0x00058B30 File Offset: 0x00056D30
		// (set) Token: 0x06001C18 RID: 7192 RVA: 0x00058B38 File Offset: 0x00056D38
		public MonoBehaviour Owner
		{
			get
			{
				return this.owner;
			}
			set
			{
				if (this.owner == value)
				{
					return;
				}
				if (value != null)
				{
					this.fallHeight = 0f;
					if (!value.HasSameGameObject(this))
					{
						Vector3 position = value.transform.position;
						IBoundingCircle boundingCircle = value as IBoundingCircle;
						if (boundingCircle != null && boundingCircle.Radius > 0f)
						{
							Vector2 position2 = boundingCircle.Position;
							float radius = boundingCircle.Radius;
							Vector3 vector = position2 + UnityEngine.Random.insideUnitCircle * radius;
							vector.z = position.z;
							this.fallHeight = position2.y + radius - vector.y;
							base.transform.position = vector;
						}
						else
						{
							this.fallHeight = UnityEngine.Random.Range(0.1f, 1.3f);
							base.transform.position = position;
						}
						ILocationChunkVisitor locationChunkVisitor = value as ILocationChunkVisitor;
						this.ownerChunk = ((locationChunkVisitor != null) ? locationChunkVisitor.CurrentLocationChunk : null);
						Pseudo3DGravityMotion pseudo3DGravityMotion = this.motionController;
						if (pseudo3DGravityMotion != null)
						{
							pseudo3DGravityMotion.Initialize(this.fallHeight);
						}
					}
				}
				else
				{
					this.SetRegisteredInOwnerChunk(false);
				}
				this.owner = value;
			}
		}

		// Token: 0x170005CE RID: 1486
		// (get) Token: 0x06001C19 RID: 7193 RVA: 0x00058C52 File Offset: 0x00056E52
		public Collider2D Collider
		{
			get
			{
				return this.collider;
			}
		}

		// Token: 0x170005CF RID: 1487
		// (get) Token: 0x06001C1A RID: 7194 RVA: 0x00058C5A File Offset: 0x00056E5A
		public SpriteRenderer ResourceRenderer
		{
			get
			{
				return this.resourceRenderer;
			}
		}

		// Token: 0x170005D0 RID: 1488
		// (get) Token: 0x06001C1B RID: 7195 RVA: 0x00058C62 File Offset: 0x00056E62
		public Pseudo3DGravityMotion MotionController
		{
			get
			{
				return this.motionController;
			}
		}

		// Token: 0x170005D1 RID: 1489
		// (get) Token: 0x06001C1C RID: 7196 RVA: 0x00058C6A File Offset: 0x00056E6A
		public bool HasPool
		{
			get
			{
				return this.CurrentPool != null;
			}
		}

		// Token: 0x170005D2 RID: 1490
		// (get) Token: 0x06001C1D RID: 7197 RVA: 0x00058C75 File Offset: 0x00056E75
		public object CurrentCollector
		{
			get
			{
				return this.currentCollector;
			}
		}

		// Token: 0x170005D3 RID: 1491
		// (get) Token: 0x06001C1E RID: 7198 RVA: 0x00058C7D File Offset: 0x00056E7D
		// (set) Token: 0x06001C1F RID: 7199 RVA: 0x00058C85 File Offset: 0x00056E85
		public GameLocation CurrentLocation { get; internal set; }

		// Token: 0x170005D4 RID: 1492
		// (get) Token: 0x06001C20 RID: 7200 RVA: 0x00058C8E File Offset: 0x00056E8E
		// (set) Token: 0x06001C21 RID: 7201 RVA: 0x00058C96 File Offset: 0x00056E96
		public ILocationChunk CurrentLocationChunk { get; set; }

		// Token: 0x170005D5 RID: 1493
		// (get) Token: 0x06001C22 RID: 7202 RVA: 0x00058C9F File Offset: 0x00056E9F
		public bool IsGrounded
		{
			get
			{
				return this.isGrounded;
			}
		}

		// Token: 0x170005D6 RID: 1494
		// (get) Token: 0x06001C23 RID: 7203 RVA: 0x00058CA7 File Offset: 0x00056EA7
		public bool IsDestroyed
		{
			get
			{
				return this.isDestroyed;
			}
		}

		// Token: 0x170005D7 RID: 1495
		// (get) Token: 0x06001C24 RID: 7204 RVA: 0x00058CAF File Offset: 0x00056EAF
		// (set) Token: 0x06001C25 RID: 7205 RVA: 0x00058CB7 File Offset: 0x00056EB7
		public int CollectionPriority
		{
			get
			{
				return this.collectionPriority;
			}
			internal set
			{
				this.collectionPriority = value;
			}
		}

		// Token: 0x170005D8 RID: 1496
		// (get) Token: 0x06001C26 RID: 7206 RVA: 0x00058CC0 File Offset: 0x00056EC0
		bool ILocationObject.IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170005D9 RID: 1497
		// (get) Token: 0x06001C27 RID: 7207 RVA: 0x00058CC3 File Offset: 0x00056EC3
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return new int?(512);
			}
		}

		// Token: 0x170005DA RID: 1498
		// (get) Token: 0x06001C28 RID: 7208 RVA: 0x00058CCF File Offset: 0x00056ECF
		Vector2 ILocationObject.Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170005DB RID: 1499
		// (get) Token: 0x06001C29 RID: 7209 RVA: 0x00058CE1 File Offset: 0x00056EE1
		float ILocationObject.Orientation
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170005DC RID: 1500
		// (get) Token: 0x06001C2A RID: 7210 RVA: 0x00058CE8 File Offset: 0x00056EE8
		// (set) Token: 0x06001C2B RID: 7211 RVA: 0x00058CF0 File Offset: 0x00056EF0
		internal IUnityObjectPool<CollectableAbilityResource> CurrentPool { get; set; }

		// Token: 0x14000110 RID: 272
		// (add) Token: 0x06001C2C RID: 7212 RVA: 0x00058CFC File Offset: 0x00056EFC
		// (remove) Token: 0x06001C2D RID: 7213 RVA: 0x00058D34 File Offset: 0x00056F34
		public event Action<CollectableAbilityResource, object> Collected;

		// Token: 0x14000111 RID: 273
		// (add) Token: 0x06001C2E RID: 7214 RVA: 0x00058D6C File Offset: 0x00056F6C
		// (remove) Token: 0x06001C2F RID: 7215 RVA: 0x00058DA4 File Offset: 0x00056FA4
		public event Action<object> Destroyed;

		// Token: 0x06001C30 RID: 7216 RVA: 0x00058DD9 File Offset: 0x00056FD9
		private void SetRandomSprite(Sprite[] spritesArray)
		{
			if (spritesArray != null && spritesArray.Length != 0 && this.resourceRenderer != null)
			{
				this.resourceRenderer.sprite = spritesArray[UnityEngine.Random.Range(0, spritesArray.Length)];
			}
		}

		// Token: 0x06001C31 RID: 7217 RVA: 0x00058E08 File Offset: 0x00057008
		private void SetKinematic(bool isKinematic)
		{
			Pseudo3DGravityMotion pseudo3DGravityMotion = this.motionController;
			Rigidbody2D rigidbody2D = (pseudo3DGravityMotion != null) ? pseudo3DGravityMotion.Rigidbody : null;
			if (isKinematic)
			{
				if (this.motionController != null)
				{
					this.motionController.IsActive = false;
					if (rigidbody2D != null)
					{
						rigidbody2D.velocity = default(Vector2);
						rigidbody2D.angularVelocity = 0f;
						rigidbody2D.isKinematic = true;
					}
				}
				base.enabled = false;
				return;
			}
			if (this.motionController != null)
			{
				this.motionController.IsActive = true;
				if (rigidbody2D != null)
				{
					rigidbody2D.isKinematic = false;
				}
			}
		}

		// Token: 0x06001C32 RID: 7218 RVA: 0x00058E98 File Offset: 0x00057098
		private void TrySetGroundedSortingLayer()
		{
			if (string.IsNullOrEmpty(this.groundedStateSortingLayer))
			{
				return;
			}
			if (this.resourceRenderer != null)
			{
				this.resourceRenderer.sortingLayerName = this.groundedStateSortingLayer;
				this.resourceRenderer.sortingOrder = this.groundedStateSortingOrder;
			}
			Renderer renderer;
			if (this.shadowTransform != null && this.shadowTransform.TryGetComponent<Renderer>(out renderer))
			{
				renderer.sortingLayerName = this.groundedStateSortingLayer;
				renderer.sortingOrder = this.groundedStateSortingOrder - 1;
			}
		}

		// Token: 0x06001C33 RID: 7219 RVA: 0x00058F1A File Offset: 0x0005711A
		private void ResetSortingLayer()
		{
			if (this.resourceRenderer == null)
			{
				return;
			}
			this.resourceRenderer.sortingLayerID = this.initialSortingLayerID;
			this.resourceRenderer.sortingOrder = this.initialSortingOrder;
		}

		// Token: 0x06001C34 RID: 7220 RVA: 0x00058F4D File Offset: 0x0005714D
		private void SetRendererVisible(bool isVisible)
		{
			if (this.resourceRenderer == null)
			{
				return;
			}
			this.resourceRenderer.color = this.resourceRenderer.color.SetA(isVisible ? 1f : 0f);
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x00058F88 File Offset: 0x00057188
		private async void AddInitialImpulse()
		{
			await Task.Yield();
			if (GameApplication.IsGameLoopRunning())
			{
				if (this.motionController.Rigidbody != null)
				{
					this.motionController.Rigidbody.AddTorque(this.initialAngularImpulse * 0.017453292f, ForceMode2D.Impulse);
				}
				this.motionController.AddPseudo3DImpulse(this.initialImpulse);
			}
		}

		// Token: 0x06001C36 RID: 7222 RVA: 0x00058FC4 File Offset: 0x000571C4
		private void SetGrounded()
		{
			if (this.isGrounded)
			{
				return;
			}
			this.SetKinematic(true);
			this.SetRandomSprite(this.randomGroundedStateSprites);
			this.TrySetGroundedSortingLayer();
			if (this.gatheringMotionComponent != null)
			{
				this.gatheringMotionComponent.StopVFXRendererEmission();
			}
			if (this.shadowTransform != null)
			{
				GameMobVFXController.UpdateShadowTransform(this.shadowTransform, 0f, this.shadowOffset, false);
			}
			this.SetRegisteredInOwnerChunk(true);
			this.isGrounded = true;
		}

		// Token: 0x06001C37 RID: 7223 RVA: 0x00059040 File Offset: 0x00057240
		private void SetRegisteredInOwnerChunk(bool isRegistered)
		{
			if (isRegistered)
			{
				if (this.ownerChunk == null)
				{
					GameLocation currentLocation = this.CurrentLocation;
					if ((this.ownerChunk = ((currentLocation != null) ? currentLocation.GetLocationChunkAtPoint(this.GetPosition(true), false) : null)) == null)
					{
						return;
					}
				}
				Component component = this.ownerChunk as Component;
				if (component != null)
				{
					base.transform.parent = component.transform;
				}
				this.ownerChunk.AddEnvironmentObject(this);
				return;
			}
			if (this.ownerChunk != null)
			{
				this.ownerChunk.RemoveEnvironmentObject(this);
				this.ownerChunk = null;
			}
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x000590CB File Offset: 0x000572CB
		internal void ResetState()
		{
			this.Owner = null;
			this.SetRegisteredInOwnerChunk(false);
			this.currentCollector = null;
			this.isGrounded = false;
			this.isInitialized = false;
			this.isDestroyed = false;
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x000590F8 File Offset: 0x000572F8
		public Vector3 GetPosition(bool tryGetLandingPosition = false)
		{
			Vector3 position = base.transform.position;
			if (tryGetLandingPosition && this.motionController != null && !this.motionController.IsGrounded)
			{
				position.y -= this.motionController.CurrentHeight;
			}
			return position;
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x00059140 File Offset: 0x00057340
		public bool HasCollectionPriority()
		{
			return this.collectionPriority > 0;
		}

		// Token: 0x06001C3B RID: 7227 RVA: 0x0005914B File Offset: 0x0005734B
		public void SetReservedForCollector(object collector)
		{
			if (this.currentCollector == null)
			{
				this.currentCollector = collector;
			}
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x0005915C File Offset: 0x0005735C
		public void TryResetCollector(object collector)
		{
			if (this.currentCollector == collector)
			{
				this.currentCollector = null;
			}
		}

		// Token: 0x06001C3D RID: 7229 RVA: 0x0005916E File Offset: 0x0005736E
		public bool CanBeCollected(object collector)
		{
			return !this.isDestroyed && (this.currentCollector == null || this.currentCollector == collector);
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x00059190 File Offset: 0x00057390
		public float StartGatheringMotion(object collector, Transform collectionTarget, float durationOverride = 0f)
		{
			if (this.CanBeCollected(collector))
			{
				float num = (durationOverride > 0f) ? durationOverride : this.GatheringMotionDuration;
				if (num > 0f && this.gatheringMotionComponent != null && this.gatheringMotionComponent.TryStartGatheringMotion(collectionTarget, num, new Vector3?(this.GetPosition(false))))
				{
					return num;
				}
			}
			return 0f;
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x000591F0 File Offset: 0x000573F0
		public bool Collect(object collector)
		{
			if (!this.CanBeCollected(collector))
			{
				return false;
			}
			this.currentCollector = collector;
			Action<CollectableAbilityResource, object> collected = this.Collected;
			if (collected != null)
			{
				collected(this, collector);
			}
			if (this.gatheringMotionComponent != null && !this.HasPool)
			{
				this.gatheringMotionComponent.DestroyVFXRendererAfterEmission();
			}
			this.Destroy();
			return true;
		}

		// Token: 0x06001C40 RID: 7232 RVA: 0x0005924A File Offset: 0x0005744A
		public void Destroy()
		{
			if (this.isDestroyed)
			{
				return;
			}
			this.isDestroyed = true;
			if (this.HasPool)
			{
				this.CurrentPool.ReturnObject(this);
				return;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06001C41 RID: 7233 RVA: 0x0005927C File Offset: 0x0005747C
		private void OnGatheringMotionStarted(GatheringMotionComponent obj)
		{
			this.SetKinematic(true);
			this.ResetSortingLayer();
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x0005928B File Offset: 0x0005748B
		private void OnGatheringMotionCompleted(GatheringMotionComponent obj)
		{
			this.SetRendererVisible(false);
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x00059294 File Offset: 0x00057494
		private void Awake()
		{
			base.TryGetComponent<Collider2D>(out this.collider);
			if ((this.resourceRenderer = base.GetComponentInChildren<SpriteRenderer>()) != null)
			{
				this.initialSortingLayerID = this.resourceRenderer.sortingLayerID;
				this.initialSortingOrder = this.resourceRenderer.sortingOrder;
			}
			if (base.TryGetComponent<GatheringMotionComponent>(out this.gatheringMotionComponent))
			{
				this.GatheringMotionDuration = this.gatheringMotionComponent.motionDuration;
				this.gatheringMotionComponent.MotionStarted += this.OnGatheringMotionStarted;
				this.gatheringMotionComponent.MotionCompleted += this.OnGatheringMotionCompleted;
			}
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x00059334 File Offset: 0x00057534
		private void OnEnable()
		{
			if (!this.isInitialized)
			{
				this.SetRendererVisible(true);
				this.ResetSortingLayer();
				this.SetRandomSprite(this.randomSprites);
				this.SetKinematic(false);
				if (this.shadowTransform != null)
				{
					this.shadowOffset = this.shadowTransform.localPosition.y;
				}
				if (this.hasMotion)
				{
					if (this.motionController == null)
					{
						Rigidbody2D motionComponent;
						if (base.TryGetComponent<Rigidbody2D>(out motionComponent))
						{
							float? customMass = (this.massOverride > 0f) ? new float?(this.massOverride) : null;
							this.motionController = new Pseudo3DGravityMotion(motionComponent, this.gravity, this.bounciness, this.fallHeight, customMass);
						}
						else
						{
							this.motionController = new Pseudo3DGravityMotion(this, this.gravity, this.bounciness, this.fallHeight, new float?(Mathf.Max(this.massOverride, 0.0001f)));
						}
					}
					this.AddInitialImpulse();
					if (this.gatheringMotionComponent != null)
					{
						this.gatheringMotionComponent.StartVFXRendererEmission();
					}
				}
				else
				{
					this.<OnEnable>g__SetGroundedAsync|97_0();
				}
			}
			this.isInitialized = true;
		}

		// Token: 0x06001C45 RID: 7237 RVA: 0x00059458 File Offset: 0x00057658
		private void FixedUpdate()
		{
			this.motionController.UpdateVelocity(Time.deltaTime);
			if (this.motionController.IsGrounded && this.motionController.DesiredVelocity.sqrMagnitude < 0.0001f)
			{
				this.SetGrounded();
				return;
			}
			if (this.shadowTransform != null)
			{
				GameMobVFXController.UpdateShadowTransform(this.shadowTransform, this.motionController.CurrentHeight, this.shadowOffset, true);
			}
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x000594D0 File Offset: 0x000576D0
		private void OnDestroy()
		{
			this.isDestroyed = true;
			this.Owner = null;
			if (this.gatheringMotionComponent != null)
			{
				this.gatheringMotionComponent.MotionStarted -= this.OnGatheringMotionStarted;
				this.gatheringMotionComponent.MotionCompleted -= this.OnGatheringMotionCompleted;
			}
			Action<object> destroyed = this.Destroyed;
			if (destroyed == null)
			{
				return;
			}
			destroyed(this);
		}

		// Token: 0x06001C48 RID: 7240 RVA: 0x00059560 File Offset: 0x00057760
		[CompilerGenerated]
		private async void <OnEnable>g__SetGroundedAsync|97_0()
		{
			await new WaitForSeconds(0.1f);
			if (GameApplication.IsGameLoopRunning())
			{
				this.SetGrounded();
			}
		}

		// Token: 0x04000FE1 RID: 4065
		public AbilityResourceType type;

		// Token: 0x04000FE2 RID: 4066
		[Space]
		public bool hasMotion = true;

		// Token: 0x04000FE3 RID: 4067
		public float gravity = 10f;

		// Token: 0x04000FE4 RID: 4068
		public float bounciness;

		// Token: 0x04000FE5 RID: 4069
		public float massOverride;

		// Token: 0x04000FE6 RID: 4070
		public Vector3 initialImpulse;

		// Token: 0x04000FE7 RID: 4071
		public float initialAngularImpulse;

		// Token: 0x04000FE8 RID: 4072
		[Space]
		public Sprite[] randomSprites;

		// Token: 0x04000FE9 RID: 4073
		public Sprite[] randomGroundedStateSprites;

		// Token: 0x04000FEA RID: 4074
		public string groundedStateSortingLayer;

		// Token: 0x04000FEB RID: 4075
		public int groundedStateSortingOrder;

		// Token: 0x04000FEC RID: 4076
		[Space]
		public Transform shadowTransform;

		// Token: 0x04000FED RID: 4077
		private Collider2D collider;

		// Token: 0x04000FEE RID: 4078
		private MonoBehaviour owner;

		// Token: 0x04000FEF RID: 4079
		private int collectionPriority;

		// Token: 0x04000FF0 RID: 4080
		private ILocationChunk ownerChunk;

		// Token: 0x04000FF1 RID: 4081
		private object currentCollector;

		// Token: 0x04000FF2 RID: 4082
		private float gatheringMotionDuration;

		// Token: 0x04000FF3 RID: 4083
		private float fallHeight = 0.1f;

		// Token: 0x04000FF4 RID: 4084
		private Pseudo3DGravityMotion motionController;

		// Token: 0x04000FF5 RID: 4085
		private GatheringMotionComponent gatheringMotionComponent;

		// Token: 0x04000FF6 RID: 4086
		private SpriteRenderer resourceRenderer;

		// Token: 0x04000FF7 RID: 4087
		private int initialSortingLayerID;

		// Token: 0x04000FF8 RID: 4088
		private int initialSortingOrder;

		// Token: 0x04000FF9 RID: 4089
		private float shadowOffset;

		// Token: 0x04000FFA RID: 4090
		private bool isInitialized;

		// Token: 0x04000FFB RID: 4091
		private bool isGrounded;

		// Token: 0x04000FFC RID: 4092
		private bool isDestroyed;
	}
}
