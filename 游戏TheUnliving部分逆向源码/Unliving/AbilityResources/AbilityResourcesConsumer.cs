using System;
using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Abilities.TargetsCollection;
using UnityEngine;
using Unliving.Player;

namespace Unliving.AbilityResources
{
	// Token: 0x02000357 RID: 855
	[Serializable]
	public sealed class AbilityResourcesConsumer
	{
		// Token: 0x170005C2 RID: 1474
		// (get) Token: 0x06001BC5 RID: 7109 RVA: 0x00057B48 File Offset: 0x00055D48
		public static int ResourcesLayer
		{
			get
			{
				return LayerMask.NameToLayer("CollectableAbilityResource");
			}
		}

		// Token: 0x170005C3 RID: 1475
		// (get) Token: 0x06001BC6 RID: 7110 RVA: 0x00057B54 File Offset: 0x00055D54
		public static int CorpseResourcesLayer
		{
			get
			{
				return LayerMask.NameToLayer("RevivableTarget");
			}
		}

		// Token: 0x170005C4 RID: 1476
		// (get) Token: 0x06001BC7 RID: 7111 RVA: 0x00057B60 File Offset: 0x00055D60
		public static int CollectableResourcesLayerMask
		{
			get
			{
				if (AbilityResourcesConsumer.collectableResourcesLayerMask == 0)
				{
					AbilityResourcesConsumer.collectableResourcesLayerMask = (1 << AbilityResourcesConsumer.ResourcesLayer | 1 << AbilityResourcesConsumer.CorpseResourcesLayer);
				}
				return AbilityResourcesConsumer.collectableResourcesLayerMask;
			}
		}

		// Token: 0x06001BC8 RID: 7112 RVA: 0x00057B88 File Offset: 0x00055D88
		private static bool IsCollectableResource(AbilityResourcesCollector resourcesCollector, CollectableAbilityResource abilityResource)
		{
			MonoBehaviour ownerBehaviour = resourcesCollector.Ability.OwnerBehaviour;
			return ownerBehaviour is PlayerBehaviour || ownerBehaviour != abilityResource.Owner;
		}

		// Token: 0x170005C5 RID: 1477
		// (get) Token: 0x06001BC9 RID: 7113 RVA: 0x00057BB7 File Offset: 0x00055DB7
		// (set) Token: 0x06001BCA RID: 7114 RVA: 0x00057BD5 File Offset: 0x00055DD5
		public int ResourcesCheckRate
		{
			get
			{
				if (this.resourcesCheckTimestep <= 0f)
				{
					return 0;
				}
				return (int)(1f / this.resourcesCheckTimestep);
			}
			set
			{
				this.resourcesCheckTimestep = ((value > 0) ? (1f / (float)value) : 0f);
			}
		}

		// Token: 0x06001BCB RID: 7115 RVA: 0x00057BF0 File Offset: 0x00055DF0
		public AbilityResourcesCollector CreateResourcesCollector(BaseAbility ability)
		{
			AbilityResourcesCollector.Parameters parameters = this.resourcesCollectionParams.Clone();
			if (this.collectResourcesInAbilityRange)
			{
				parameters.collectionRange = ability.Range;
			}
			AbilityResourcesCollector abilityResourcesCollector = new AbilityResourcesCollector(ability, parameters, null);
			if (!this.collectResourcesProducedByOwner)
			{
				abilityResourcesCollector.resourcesFilter = new Func<AbilityResourcesCollector, CollectableAbilityResource, bool>(AbilityResourcesConsumer.IsCollectableResource);
			}
			if (this.sortCollectedResources)
			{
				abilityResourcesCollector.resourcesSortingComparer = AbilityResourcesConsumer.SortingComparer;
			}
			return abilityResourcesCollector;
		}

		// Token: 0x06001BCC RID: 7116 RVA: 0x00057C54 File Offset: 0x00055E54
		public Vector2 GetResourcesCollectionPosition(AbilityResourcesCollector collector, BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (this.resourcesCollectionPosition == AbilityResourcesConsumer.CollectionPositionSource.AbilityOwnerPosition || abilityUsingArgs == null)
			{
				return collector.Ability.OwnerPosition;
			}
			return abilityUsingArgs.targetPosition;
		}

		// Token: 0x06001BCD RID: 7117 RVA: 0x00057C80 File Offset: 0x00055E80
		public bool HasResourcesInRange(AbilityResourcesCollector collector, BaseAbility.UsingArgs abilityUsingArgs)
		{
			Vector2 v = this.GetResourcesCollectionPosition(collector, abilityUsingArgs);
			return collector.HasResourcesInRange(v, this.requiredResources, this.resourcesCheckTimestep);
		}

		// Token: 0x06001BCE RID: 7118 RVA: 0x00057CB0 File Offset: 0x00055EB0
		public bool CollectResources(AbilityResourcesCollector collector, BaseAbility.UsingArgs abilityUsingArgs, int maxUsingCount, out AbilityResourcesBlock lackOfResources)
		{
			Vector2 position = this.GetResourcesCollectionPosition(collector, abilityUsingArgs);
			AbilityResourcesCollector.CollectedResourcesInfo collectedResourcesInfo;
			return collector.CollectResources(position, this.requiredResources, out collectedResourcesInfo, out lackOfResources, maxUsingCount);
		}

		// Token: 0x06001BCF RID: 7119 RVA: 0x00057CD8 File Offset: 0x00055ED8
		public void StartResourcesConsumptionMotion(in AbilityResourcesCollector.CollectedResourcesInfo collectedResourcesInfo, float durationOverride = 0f)
		{
			BaseAbility ability = collectedResourcesInfo.Collector.Ability;
			IAbilityResourcesCollectingEntity abilityResourcesCollectingEntity = ability.Owner as IAbilityResourcesCollectingEntity;
			if (durationOverride <= 0f && abilityResourcesCollectingEntity != null)
			{
				durationOverride = abilityResourcesCollectingEntity.ResourcesGatheringDurationOverride;
			}
			Transform transform = ability.OwnerBehaviour.transform;
			float num = 0f;
			List<CollectableAbilityResource> resources = collectedResourcesInfo.Resources;
			for (int i = 0; i < resources.Count; i++)
			{
				float num2 = resources[i].StartGatheringMotion(ability, transform, durationOverride);
				if (num2 > num)
				{
					num = num2;
				}
			}
			if (num > ability.UsingDelay)
			{
				ability.UsingDelay = num;
			}
			if (abilityResourcesCollectingEntity != null)
			{
				abilityResourcesCollectingEntity.OnCollectingResources(collectedResourcesInfo.Collector, num);
			}
		}

		// Token: 0x06001BD0 RID: 7120 RVA: 0x00057D7C File Offset: 0x00055F7C
		public void StartResourcesConsumptionMotion(AbilityResourcesCollector resourcesCollector, float durationOverride = 0f)
		{
			AbilityResourcesCollector.CollectedResourcesInfo lastCollectedResourcesInfo = resourcesCollector.LastCollectedResourcesInfo;
			this.StartResourcesConsumptionMotion(lastCollectedResourcesInfo, durationOverride);
		}

		// Token: 0x06001BD1 RID: 7121 RVA: 0x00057D9C File Offset: 0x00055F9C
		public void ConsumeResources(in AbilityResourcesCollector.CollectedResourcesInfo collectedResourcesInfo)
		{
			BaseAbility ability = collectedResourcesInfo.Collector.Ability;
			List<CollectableAbilityResource> resources = collectedResourcesInfo.Resources;
			IAbilityResourcesCollectingEntity abilityResourcesCollectingEntity = ability.Owner as IAbilityResourcesCollectingEntity;
			if (abilityResourcesCollectingEntity != null)
			{
				abilityResourcesCollectingEntity.OnConsumingResources(ability, resources);
			}
			for (int i = 0; i < resources.Count; i++)
			{
				resources[i].Collect(ability);
			}
		}

		// Token: 0x06001BD2 RID: 7122 RVA: 0x00057DF4 File Offset: 0x00055FF4
		public void ConsumeResources(AbilityResourcesCollector resourcesCollector)
		{
			AbilityResourcesCollector.CollectedResourcesInfo lastCollectedResourcesInfo = resourcesCollector.LastCollectedResourcesInfo;
			this.ConsumeResources(lastCollectedResourcesInfo);
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x00057E10 File Offset: 0x00056010
		public void SyncCollectionLayers(UnityEngine.Object parentObject)
		{
		}

		// Token: 0x04000FB9 RID: 4025
		private static readonly DistanceBasedTargetsComparer SortingComparer = new DistanceBasedTargetsComparer(true);

		// Token: 0x04000FBA RID: 4026
		private static int collectableResourcesLayerMask;

		// Token: 0x04000FBB RID: 4027
		public AbilityResourcesCollector.RequiredResourceInfo[] requiredResources;

		// Token: 0x04000FBC RID: 4028
		public AbilityResourcesCollector.Parameters resourcesCollectionParams;

		// Token: 0x04000FBD RID: 4029
		public AbilityResourcesConsumer.CollectionPositionSource resourcesCollectionPosition;

		// Token: 0x04000FBE RID: 4030
		public bool collectResourcesInAbilityRange;

		// Token: 0x04000FBF RID: 4031
		public bool sortCollectedResources = true;

		// Token: 0x04000FC0 RID: 4032
		public bool collectResourcesProducedByOwner;

		// Token: 0x04000FC1 RID: 4033
		private AbilityResourcesConsumer.CollectionLayersUpdater collectionLayersUpdater;

		// Token: 0x04000FC2 RID: 4034
		private float resourcesCheckTimestep = 0.3f;

		// Token: 0x02000558 RID: 1368
		private sealed class CollectionLayersUpdater
		{
			// Token: 0x060026E6 RID: 9958 RVA: 0x0007900F File Offset: 0x0007720F
			private static int GetResourceLayer(AbilityResourceType resourceType)
			{
				if (resourceType <= AbilityResourceType.Cadaver)
				{
					return AbilityResourcesConsumer.ResourcesLayer;
				}
				if (resourceType != AbilityResourceType.Corpse)
				{
					return -1;
				}
				return AbilityResourcesConsumer.CorpseResourcesLayer;
			}

			// Token: 0x060026E7 RID: 9959 RVA: 0x00079028 File Offset: 0x00077228
			private void UpdateLastResourcesInfo(AbilityResourcesConsumer resourcesConsumer)
			{
				AbilityResourcesCollector.RequiredResourceInfo[] requiredResources = resourcesConsumer.requiredResources;
				if (this.lastResources == null || requiredResources.Length != this.lastResources.Length)
				{
					if (requiredResources == null)
					{
						this.lastResources = Array.Empty<AbilityResourcesCollector.RequiredResourceInfo>();
					}
					else
					{
						this.lastResources = new AbilityResourcesCollector.RequiredResourceInfo[requiredResources.Length];
					}
				}
				if (this.lastResources.Length != 0)
				{
					requiredResources.CopyTo(this.lastResources, 0);
				}
			}

			// Token: 0x060026E8 RID: 9960 RVA: 0x00079088 File Offset: 0x00077288
			private bool IsResourcesInfoModified(AbilityResourcesConsumer resourcesConsumer)
			{
				AbilityResourcesCollector.RequiredResourceInfo[] requiredResources = resourcesConsumer.requiredResources;
				return requiredResources != null && (this.lastResources == null || requiredResources.Length != this.lastResources.Length || !requiredResources.SequenceEqual(this.lastResources));
			}

			// Token: 0x060026E9 RID: 9961 RVA: 0x000790C8 File Offset: 0x000772C8
			public void UpdateCollectionLayers(UnityEngine.Object parentObject, AbilityResourcesConsumer resourcesConsumer)
			{
				if (this.IsResourcesInfoModified(resourcesConsumer))
				{
					int num = 0;
					AbilityResourcesCollector.RequiredResourceInfo[] requiredResources = resourcesConsumer.requiredResources;
					for (int i = 0; i < requiredResources.Length; i++)
					{
						int resourceLayer = AbilityResourcesConsumer.CollectionLayersUpdater.GetResourceLayer(requiredResources[i].resourceType);
						if (resourceLayer >= 0)
						{
							num |= 1 << resourceLayer;
						}
					}
					resourcesConsumer.resourcesCollectionParams.resourcesCollectionLayers = num;
					this.UpdateLastResourcesInfo(resourcesConsumer);
				}
			}

			// Token: 0x04001BE1 RID: 7137
			private AbilityResourcesCollector.RequiredResourceInfo[] lastResources;
		}

		// Token: 0x02000559 RID: 1369
		public enum CollectionPositionSource
		{
			// Token: 0x04001BE3 RID: 7139
			AbilityOwnerPosition,
			// Token: 0x04001BE4 RID: 7140
			AbilityUsingPoint
		}

		// Token: 0x0200055A RID: 1370
		private sealed class ResourcesSortingComparer : IComparer<CollectableAbilityResource>, IDistanceBasedComparer, IComparer<Component>
		{
			// Token: 0x060026EB RID: 9963 RVA: 0x00079134 File Offset: 0x00077334
			private static float GetResourcePriority(CollectableAbilityResource resource, ref Vector3 gatheringPoint)
			{
				Vector3 vector = resource.GetPosition(false) - gatheringPoint;
				float num = 1f - (vector.x * vector.x + vector.y * vector.y) / 10000f;
				if (num < 0f)
				{
					num = 0f;
				}
				if (resource.HasCollectionPriority())
				{
					num *= (float)resource.CollectionPriority;
				}
				return num;
			}

			// Token: 0x170007EA RID: 2026
			// (get) Token: 0x060026EC RID: 9964 RVA: 0x0007919D File Offset: 0x0007739D
			// (set) Token: 0x060026ED RID: 9965 RVA: 0x000791A5 File Offset: 0x000773A5
			Vector3 IDistanceBasedComparer.SortingPoint
			{
				get
				{
					return this.resourcesGatheringPoint;
				}
				set
				{
					this.resourcesGatheringPoint = value;
				}
			}

			// Token: 0x060026EE RID: 9966 RVA: 0x000791B0 File Offset: 0x000773B0
			public int Compare(CollectableAbilityResource resource0, CollectableAbilityResource resource1)
			{
				float resourcePriority = AbilityResourcesConsumer.ResourcesSortingComparer.GetResourcePriority(resource0, ref this.resourcesGatheringPoint);
				return AbilityResourcesConsumer.ResourcesSortingComparer.GetResourcePriority(resource1, ref this.resourcesGatheringPoint).CompareTo(resourcePriority);
			}

			// Token: 0x060026EF RID: 9967 RVA: 0x000791DF File Offset: 0x000773DF
			int IComparer<Component>.Compare(Component x, Component y)
			{
				return this.Compare((CollectableAbilityResource)x, (CollectableAbilityResource)y);
			}

			// Token: 0x04001BE5 RID: 7141
			public Vector3 resourcesGatheringPoint;
		}
	}
}
