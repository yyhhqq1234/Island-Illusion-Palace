using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.UnityExtensions;
using Game.Core;
using Game.Damage;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.VFX;
using Unliving.Pickables;
using Unliving.Player;

namespace Unliving.Challenges
{
	// Token: 0x02000353 RID: 851
	public class ResourceDevourer : GameBehaviourBase, IProgressBasedAction, IRevivableGameMob
	{
		// Token: 0x170005B9 RID: 1465
		// (get) Token: 0x06001B84 RID: 7044 RVA: 0x00056923 File Offset: 0x00054B23
		// (set) Token: 0x06001B85 RID: 7045 RVA: 0x0005692B File Offset: 0x00054B2B
		public float CurrentProgress { get; private set; }

		// Token: 0x170005BA RID: 1466
		// (get) Token: 0x06001B86 RID: 7046 RVA: 0x00056934 File Offset: 0x00054B34
		public bool IsActive
		{
			get
			{
				return this.currentState == ResourceDevourer.State.READY_FOR_ACTION || this.currentState == ResourceDevourer.State.ACTIVE;
			}
		}

		// Token: 0x170005BB RID: 1467
		// (get) Token: 0x06001B87 RID: 7047 RVA: 0x0005694A File Offset: 0x00054B4A
		// (set) Token: 0x06001B88 RID: 7048 RVA: 0x00056954 File Offset: 0x00054B54
		public int CollectedResourcesCount
		{
			get
			{
				return this.collectedResourcesCount;
			}
			private set
			{
				if (this.collectedResourcesCount == value)
				{
					return;
				}
				this.collectedResourcesCount = value;
				this.CurrentProgress = Mathf.Clamp01((float)this.collectedResourcesCount / (float)this.targetResourcesCount);
				Action resourceCountChanged = this.ResourceCountChanged;
				if (resourceCountChanged != null)
				{
					resourceCountChanged();
				}
				if (this.currentState == ResourceDevourer.State.READY_FOR_ACTION && this.collectedResourcesCount == 1)
				{
					this.CurrentState = ResourceDevourer.State.ACTIVE;
				}
				if (this.collectedResourcesCount >= this.targetResourcesCount)
				{
					if (this.manualRewardPickingUp)
					{
						this.CurrentState = ResourceDevourer.State.FULL;
						return;
					}
					this.SpawnReward();
				}
			}
		}

		// Token: 0x14000108 RID: 264
		// (add) Token: 0x06001B89 RID: 7049 RVA: 0x000569DC File Offset: 0x00054BDC
		// (remove) Token: 0x06001B8A RID: 7050 RVA: 0x00056A14 File Offset: 0x00054C14
		public event Action<ResourceDevourer.State> CurrentStateChanged;

		// Token: 0x14000109 RID: 265
		// (add) Token: 0x06001B8B RID: 7051 RVA: 0x00056A4C File Offset: 0x00054C4C
		// (remove) Token: 0x06001B8C RID: 7052 RVA: 0x00056A84 File Offset: 0x00054C84
		public event Action ActivationFailed;

		// Token: 0x1400010A RID: 266
		// (add) Token: 0x06001B8D RID: 7053 RVA: 0x00056ABC File Offset: 0x00054CBC
		// (remove) Token: 0x06001B8E RID: 7054 RVA: 0x00056AF4 File Offset: 0x00054CF4
		public event Action ResourceCountChanged;

		// Token: 0x1400010B RID: 267
		// (add) Token: 0x06001B8F RID: 7055 RVA: 0x00056B2C File Offset: 0x00054D2C
		// (remove) Token: 0x06001B90 RID: 7056 RVA: 0x00056B64 File Offset: 0x00054D64
		public event Action<BaseGameMob, BaseGameMob> Revived;

		// Token: 0x170005BC RID: 1468
		// (get) Token: 0x06001B91 RID: 7057 RVA: 0x00056B99 File Offset: 0x00054D99
		// (set) Token: 0x06001B92 RID: 7058 RVA: 0x00056BA1 File Offset: 0x00054DA1
		public ResourceDevourer.State CurrentState
		{
			get
			{
				return this.currentState;
			}
			set
			{
				if (this.currentState == value)
				{
					return;
				}
				this.currentState = value;
				Action<ResourceDevourer.State> currentStateChanged = this.CurrentStateChanged;
				if (currentStateChanged == null)
				{
					return;
				}
				currentStateChanged(this.currentState);
			}
		}

		// Token: 0x170005BD RID: 1469
		// (get) Token: 0x06001B93 RID: 7059 RVA: 0x00056BCA File Offset: 0x00054DCA
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x00056BCD File Offset: 0x00054DCD
		private bool IsCollectableResource(CollectableAbilityResource resource)
		{
			return resource.type == this.resourceType && (!this.collectSameChunkResourcesOnly || this.currentChunk == resource.CurrentLocationChunk);
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x00056BF8 File Offset: 0x00054DF8
		private void Activate()
		{
			if (this.IsActive)
			{
				this.resourcesInRangeCount = Physics2D.OverlapCircleNonAlloc(base.transform.position, this.radius, ResourceDevourer.TempResourcesBuffer, this.collectableResourcesLayerMask);
				for (int i = 0; i < this.resourcesInRangeCount; i++)
				{
					CollectableAbilityResource collectableAbilityResource;
					if (this.CollectedResourcesCount + ResourceDevourer.CollectInProgressList.Count < this.targetResourcesCount && ResourceDevourer.TempResourcesBuffer[i].TryGetComponent<CollectableAbilityResource>(out collectableAbilityResource) && this.IsCollectableResource(collectableAbilityResource) && collectableAbilityResource.CanBeCollected(this) && !ResourceDevourer.CollectInProgressList.Contains(collectableAbilityResource))
					{
						collectableAbilityResource.SetReservedForCollector(this);
						ResourceDevourer.CollectInProgressList.Add(collectableAbilityResource);
						base.StartCoroutine(this.CollectResourceRoutine(collectableAbilityResource, this.moveDurationOverride));
					}
				}
				return;
			}
			if (this.manualRewardPickingUp && this.CurrentState == ResourceDevourer.State.FULL)
			{
				this.SpawnReward();
			}
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x00056CD8 File Offset: 0x00054ED8
		private IEnumerator CollectResourceRoutine(CollectableAbilityResource resource, float durationOverride)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 1f));
			float seconds = resource.StartGatheringMotion(this, base.transform, durationOverride);
			yield return new WaitForSeconds(seconds);
			int num = this.CollectedResourcesCount;
			this.CollectedResourcesCount = num + 1;
			ResourceDevourer.CollectInProgressList.Remove(resource);
			resource.Collect(this);
			yield break;
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x00056CF5 File Offset: 0x00054EF5
		private void SpawnReward()
		{
			if (this.CurrentState == ResourceDevourer.State.REWARD_SPAWNED)
			{
				return;
			}
			this.CurrentState = ResourceDevourer.State.REWARD_SPAWNED;
			this.reward.CreateReward(this.currentPlayer.TempItemsStorageController, ResourceDevourer.ValidChunkTypes);
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x00056D24 File Offset: 0x00054F24
		private void Start()
		{
			IGameLocationProvider gameLocationProvider = base.CurrentGame.Services.Get<IGameLocationProvider>();
			GameLocation gameLocation = (gameLocationProvider != null) ? gameLocationProvider.CurrentLocation : null;
			this.currentChunk = ((gameLocation != null) ? gameLocation.GetLocationChunkAtPoint(base.transform.position, false) : null);
			IPlayerProvider playerProvider = base.CurrentGame.Services.Get<IPlayerProvider>();
			this.currentPlayer = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
			base.TryGetComponent<HitPointsController>(out this.hitPointsController);
			base.TryGetComponent<Collider2D>(out this.collider);
			if (base.TryGetComponent<DefaultObjectPickable>(out this.pickable))
			{
				this.pickable.PickedUp += this.OnPickablePickedUp;
			}
			StaticGameMob component = base.GetComponent<StaticGameMob>();
			GameMobVFXController component2 = base.GetComponent<GameMobVFXController>();
			switch (this.activationType)
			{
			case ResourceDevourer.ActivationType.Cooldown:
				UnityEngine.Object.Destroy(this.hitPointsController);
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(component2);
				UnityEngine.Object.Destroy(this.collider);
				break;
			case ResourceDevourer.ActivationType.OnClick:
				UnityEngine.Object.Destroy(this.hitPointsController);
				UnityEngine.Object.Destroy(component);
				UnityEngine.Object.Destroy(component2);
				UnityEngine.Object.Destroy(this.collider);
				break;
			case ResourceDevourer.ActivationType.OnDamage:
				UnityEngine.Object.Destroy(this.collider);
				this.hitPointsController.HitPointsChanged += this.OnHitPointsChanged;
				break;
			case ResourceDevourer.ActivationType.OnResurrection:
				base.gameObject.layer = LayerMask.NameToLayer("RevivableTarget");
				break;
			}
			this.CurrentState = ResourceDevourer.State.READY_FOR_ACTION;
			for (int i = 0; i < this.fullFillSpawners.Count; i++)
			{
				this.fullFillSpawners[i].GroupDestroyed += this.OnSpawnerGroupDestroyed;
			}
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x00056EC2 File Offset: 0x000550C2
		private void OnPickablePickedUp(IPickableObject obj, IPickableObjectCollector collector)
		{
			if (this.activationType == ResourceDevourer.ActivationType.OnClick)
			{
				this.Activate();
				return;
			}
			if (this.manualRewardPickingUp)
			{
				if (this.currentState == ResourceDevourer.State.FULL)
				{
					this.Activate();
					return;
				}
				Action activationFailed = this.ActivationFailed;
				if (activationFailed == null)
				{
					return;
				}
				activationFailed();
			}
		}

		// Token: 0x06001B9A RID: 7066 RVA: 0x00056EFC File Offset: 0x000550FC
		private void OnHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (this.activationType != ResourceDevourer.ActivationType.OnDamage || sender != this.currentPlayer)
			{
				return;
			}
			this.Activate();
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x00056F18 File Offset: 0x00055118
		private void Update()
		{
			if (!this.IsActive)
			{
				return;
			}
			if ((this.activationType == ResourceDevourer.ActivationType.Cooldown || this.activationType == ResourceDevourer.ActivationType.OnResurrection) && this.nextCollectTime <= Time.time)
			{
				this.Activate();
				this.nextCollectTime = Time.time + this.collectCooldown;
			}
			if (this.CollectedResourcesCount > 0)
			{
				this.currentResourseDecreaseAmount += this.resourseDecreasePerSecond * Time.deltaTime;
				int num = Mathf.FloorToInt(this.currentResourseDecreaseAmount);
				if (num > 0)
				{
					this.currentResourseDecreaseAmount -= (float)num;
					this.CollectedResourcesCount -= num;
					return;
				}
			}
			else
			{
				this.currentResourseDecreaseAmount = 0f;
			}
		}

		// Token: 0x06001B9C RID: 7068 RVA: 0x00056FC0 File Offset: 0x000551C0
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.hitPointsController.IsNull())
			{
				this.hitPointsController.HitPointsChanged -= this.OnHitPointsChanged;
			}
			if (!this.pickable.IsNull())
			{
				this.pickable.PickedUp -= this.OnPickablePickedUp;
			}
			for (int i = 0; i < this.fullFillSpawners.Count; i++)
			{
				this.fullFillSpawners[i].GroupDestroyed -= this.OnSpawnerGroupDestroyed;
			}
		}

		// Token: 0x06001B9D RID: 7069 RVA: 0x0005704E File Offset: 0x0005524E
		public bool CanBeRevived(BaseGameMob reviver, object context)
		{
			return this.activationType == ResourceDevourer.ActivationType.OnResurrection && this.CurrentState == ResourceDevourer.State.FULL;
		}

		// Token: 0x06001B9E RID: 7070 RVA: 0x00057064 File Offset: 0x00055264
		public BaseGameMob Revive(BaseGameMob reviver, object context, bool destroySourceMob = true)
		{
			this.Activate();
			return null;
		}

		// Token: 0x06001B9F RID: 7071 RVA: 0x00057070 File Offset: 0x00055270
		private void OnSpawnerGroupDestroyed(MobBehaviourSpawner spawner, GameMobsGroupControllerBase group)
		{
			spawner.GroupDestroyed -= this.OnSpawnerGroupDestroyed;
			if (this.fullFillSpawners.Contains(spawner))
			{
				this.fullFillSpawners.Remove(spawner);
				if (this.fullFillSpawners.Count == 0)
				{
					this.ForceFullFill();
				}
			}
		}

		// Token: 0x06001BA0 RID: 7072 RVA: 0x000570BD File Offset: 0x000552BD
		private void ForceFullFill()
		{
			if (this.CurrentState != ResourceDevourer.State.ACTIVE)
			{
				return;
			}
			base.StartCoroutine(this.FullFillRoutine());
		}

		// Token: 0x06001BA1 RID: 7073 RVA: 0x000570D6 File Offset: 0x000552D6
		private IEnumerator FullFillRoutine()
		{
			if (this.fullFillTime <= 0f)
			{
				this.CollectedResourcesCount = this.targetResourcesCount;
			}
			else
			{
				int num = this.targetResourcesCount - this.collectedResourcesCount;
				float timeStep = this.fullFillTime / (float)num;
				while (this.CurrentProgress < 1f)
				{
					yield return new WaitForSeconds(timeStep);
					int num2 = this.CollectedResourcesCount;
					this.CollectedResourcesCount = num2 + 1;
				}
			}
			yield break;
		}

		// Token: 0x04000F80 RID: 3968
		private static readonly LocationChunk.TypeID[] ValidChunkTypes = new LocationChunk.TypeID[]
		{
			LocationChunk.TypeID.BattleChunk,
			LocationChunk.TypeID.BossChunk
		};

		// Token: 0x04000F81 RID: 3969
		private static readonly Collider2D[] TempResourcesBuffer = new Collider2D[100];

		// Token: 0x04000F82 RID: 3970
		private static readonly List<CollectableAbilityResource> CollectInProgressList = new List<CollectableAbilityResource>(100);

		// Token: 0x04000F88 RID: 3976
		public float moveDurationOverride;

		// Token: 0x04000F89 RID: 3977
		public LayerMask collectableResourcesLayerMask;

		// Token: 0x04000F8A RID: 3978
		public bool collectSameChunkResourcesOnly = true;

		// Token: 0x04000F8B RID: 3979
		public ResourceDevourer.ActivationType activationType;

		// Token: 0x04000F8C RID: 3980
		public bool manualRewardPickingUp;

		// Token: 0x04000F8D RID: 3981
		public float radius = 5f;

		// Token: 0x04000F8E RID: 3982
		public float collectCooldown = 1f;

		// Token: 0x04000F8F RID: 3983
		public float resourseDecreasePerSecond;

		// Token: 0x04000F90 RID: 3984
		public AbilityResourceType resourceType;

		// Token: 0x04000F91 RID: 3985
		public int targetResourcesCount;

		// Token: 0x04000F92 RID: 3986
		public ChallengeRewardInfo reward;

		// Token: 0x04000F93 RID: 3987
		[Tooltip("Время автонаполнения, если <=0, то заполняется мгновенно")]
		public float fullFillTime = -1f;

		// Token: 0x04000F94 RID: 3988
		public List<MobBehaviourSpawner> fullFillSpawners;

		// Token: 0x04000F95 RID: 3989
		private ILocationChunk currentChunk;

		// Token: 0x04000F96 RID: 3990
		private int collectedResourcesCount;

		// Token: 0x04000F97 RID: 3991
		private float nextCollectTime;

		// Token: 0x04000F98 RID: 3992
		private int resourcesInRangeCount;

		// Token: 0x04000F99 RID: 3993
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000F9A RID: 3994
		private HitPointsController hitPointsController;

		// Token: 0x04000F9B RID: 3995
		private DefaultObjectPickable pickable;

		// Token: 0x04000F9C RID: 3996
		private float currentResourseDecreaseAmount;

		// Token: 0x04000F9D RID: 3997
		private ResourceDevourer.State currentState;

		// Token: 0x04000F9E RID: 3998
		private Collider2D collider;

		// Token: 0x02000550 RID: 1360
		public enum ActivationType
		{
			// Token: 0x04001BC3 RID: 7107
			Cooldown,
			// Token: 0x04001BC4 RID: 7108
			OnClick,
			// Token: 0x04001BC5 RID: 7109
			OnDamage,
			// Token: 0x04001BC6 RID: 7110
			OnResurrection
		}

		// Token: 0x02000551 RID: 1361
		public enum State
		{
			// Token: 0x04001BC8 RID: 7112
			NOT_INITIALIZED,
			// Token: 0x04001BC9 RID: 7113
			READY_FOR_ACTION,
			// Token: 0x04001BCA RID: 7114
			ACTIVE,
			// Token: 0x04001BCB RID: 7115
			FULL,
			// Token: 0x04001BCC RID: 7116
			REWARD_SPAWNED
		}
	}
}
