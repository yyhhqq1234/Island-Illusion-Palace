using System;
using Common;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Core;
using Game.ObjectPool;
using UnityEngine;
using Unliving.GameScene;
using Unliving.LevelGeneration;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035E RID: 862
	[Service(typeof(CollectableAbilityResourcesFactory), new Type[]
	{
		typeof(ICollectableAbilityResourcesFactory),
		typeof(IObjectFactory<CollectableAbilityResource>),
		typeof(IFactory<int, CollectableAbilityResource>)
	})]
	public sealed class CollectableAbilityResourcesFactory : PrototypeBasedFactory<CollectableAbilityResourcesFactory.PrototypeInfo, CollectableAbilityResource>, ICollectableAbilityResourcesFactory, IInitializable<IGame>
	{
		// Token: 0x14000112 RID: 274
		// (add) Token: 0x06001C51 RID: 7249 RVA: 0x00059760 File Offset: 0x00057960
		// (remove) Token: 0x06001C52 RID: 7250 RVA: 0x00059798 File Offset: 0x00057998
		public event Action<CollectableAbilityResourcesFactoryArgs, CollectableAbilityResource> ResourceCreated;

		// Token: 0x06001C53 RID: 7251 RVA: 0x000597D0 File Offset: 0x000579D0
		private CollectableAbilityResource CreateResource(GameObject resourcePrefab, GameObject arbitraryResourceObject, AbilityResourceType resourceType)
		{
			CollectableAbilityResource collectableAbilityResource = null;
			if (arbitraryResourceObject != null)
			{
				collectableAbilityResource = arbitraryResourceObject.AddComponent<CollectableAbilityResource>();
			}
			else if (resourcePrefab != null)
			{
				if (resourceType != AbilityResourceType.Corpse && (this.resourcesPool != null || this.currentGame.Services.TryGet<IUnityObjectPool<CollectableAbilityResource>>(out this.resourcesPool)))
				{
					CollectableAbilityResourcesFactory.ResourcesPoolArgs.unityObjectPrototype = resourcePrefab;
					collectableAbilityResource = this.resourcesPool.TakeObject(CollectableAbilityResourcesFactory.ResourcesPoolArgs);
					collectableAbilityResource.CurrentPool = this.resourcesPool;
				}
				else
				{
					collectableAbilityResource = UnityEngine.Object.Instantiate<GameObject>(resourcePrefab).GetComponent<CollectableAbilityResource>();
				}
			}
			return collectableAbilityResource;
		}

		// Token: 0x06001C54 RID: 7252 RVA: 0x00059858 File Offset: 0x00057A58
		protected override CollectableAbilityResource Create(CollectableAbilityResourcesFactory.PrototypeInfo data, IBaseObjectDescription args)
		{
			CollectableAbilityResourcesFactoryArgs collectableAbilityResourcesFactoryArgs = (CollectableAbilityResourcesFactoryArgs)args;
			IGameLocationProvider gameLocationProvider = this.currentGame.Services.Get<IGameLocationProvider>();
			CollectableAbilityResource collectableAbilityResource = null;
			LocationDependentObjectInfo locationDependentObjectInfo;
			if (collectableAbilityResourcesFactoryArgs.arbitraryResourceObject != null)
			{
				collectableAbilityResource = this.CreateResource(null, collectableAbilityResourcesFactoryArgs.arbitraryResourceObject, collectableAbilityResourcesFactoryArgs.resourceType);
			}
			else if (LocationDependentDataSelector<LocationDependentObjectInfo>.TryGetData(data.resourcePrefabs, gameLocationProvider, out locationDependentObjectInfo))
			{
				collectableAbilityResource = this.CreateResource(locationDependentObjectInfo.gameObject, null, collectableAbilityResourcesFactoryArgs.resourceType);
			}
			if (collectableAbilityResource != null)
			{
				MonoBehaviour monoBehaviour = collectableAbilityResourcesFactoryArgs.resourceOwner as MonoBehaviour;
				Vector2? position = collectableAbilityResourcesFactoryArgs.position;
				if (collectableAbilityResource.hasMotion = !collectableAbilityResourcesFactoryArgs.isStatic)
				{
					if (collectableAbilityResourcesFactoryArgs.impulse.sqrMagnitude > 1E-05f || collectableAbilityResourcesFactoryArgs.angularImpulse != 0f)
					{
						collectableAbilityResource.initialImpulse = collectableAbilityResourcesFactoryArgs.impulse;
					}
					else
					{
						Vector3 minInitialImpulse = data.minInitialImpulse;
						Vector3 maxInitialImpulse = data.maxInitialImpulse;
						Vector3 initialImpulse = new Vector3
						{
							x = UnityEngine.Random.Range(minInitialImpulse.x, maxInitialImpulse.x) * ((UnityEngine.Random.value > 0.5f) ? 1f : -1f),
							y = UnityEngine.Random.Range(minInitialImpulse.y, maxInitialImpulse.y),
							z = UnityEngine.Random.Range(minInitialImpulse.z, maxInitialImpulse.z)
						};
						collectableAbilityResource.initialImpulse = initialImpulse;
						collectableAbilityResource.initialAngularImpulse = (UnityEngine.Random.value * 2f - 1f) * data.maxInitialAngularImpulse;
					}
				}
				collectableAbilityResource.type = collectableAbilityResourcesFactoryArgs.resourceType;
				collectableAbilityResource.Owner = monoBehaviour;
				if (position != null)
				{
					collectableAbilityResource.transform.position = position.Value;
				}
				if (monoBehaviour == null && gameLocationProvider != null)
				{
					collectableAbilityResource.CurrentLocation = gameLocationProvider.CurrentLocation;
				}
				Action<CollectableAbilityResourcesFactoryArgs, CollectableAbilityResource> resourceCreated = this.ResourceCreated;
				if (resourceCreated != null)
				{
					resourceCreated(collectableAbilityResourcesFactoryArgs, collectableAbilityResource);
				}
			}
			return collectableAbilityResource;
		}

		// Token: 0x06001C55 RID: 7253 RVA: 0x00059A3B File Offset: 0x00057C3B
		public CollectableAbilityResource Create(CollectableAbilityResourcesFactoryArgs args)
		{
			return this.Create(base.GetObjectPrototype((int)args.resourceType), args);
		}

		// Token: 0x06001C56 RID: 7254 RVA: 0x00059A50 File Offset: 0x00057C50
		void IInitializable<IGame>.Initialize(IGame game)
		{
			this.currentGame = game;
		}

		// Token: 0x04001001 RID: 4097
		private static readonly UnityObjectPoolArgs ResourcesPoolArgs = new UnityObjectPoolArgs();

		// Token: 0x04001003 RID: 4099
		private IGame currentGame;

		// Token: 0x04001004 RID: 4100
		private IUnityObjectPool<CollectableAbilityResource> resourcesPool;

		// Token: 0x02000560 RID: 1376
		[Serializable]
		public sealed class PrototypeInfo : IUnityObjectDescription, IBaseObjectDescription
		{
			// Token: 0x170007EB RID: 2027
			// (get) Token: 0x060026F9 RID: 9977 RVA: 0x0007948E File Offset: 0x0007768E
			// (set) Token: 0x060026FA RID: 9978 RVA: 0x00079496 File Offset: 0x00077696
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.resourceType;
				}
				set
				{
					this.resourceType = (AbilityResourceType)value;
				}
			}

			// Token: 0x170007EC RID: 2028
			// (get) Token: 0x060026FB RID: 9979 RVA: 0x0007949F File Offset: 0x0007769F
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					if (this.resourcePrefabs.Length == 0)
					{
						return null;
					}
					return this.resourcePrefabs[0].gameObject;
				}
			}

			// Token: 0x04001BF7 RID: 7159
			public AbilityResourceType resourceType;

			// Token: 0x04001BF8 RID: 7160
			public LocationDependentObjectInfo[] resourcePrefabs;

			// Token: 0x04001BF9 RID: 7161
			public Vector3 minInitialImpulse;

			// Token: 0x04001BFA RID: 7162
			public Vector3 maxInitialImpulse;

			// Token: 0x04001BFB RID: 7163
			public float maxInitialAngularImpulse;
		}
	}
}
