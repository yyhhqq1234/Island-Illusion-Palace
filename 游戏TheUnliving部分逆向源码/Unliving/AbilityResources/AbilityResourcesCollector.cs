using System;
using System.Collections.Generic;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Abilities.TargetsCollection;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x02000356 RID: 854
	public sealed class AbilityResourcesCollector
	{
		// Token: 0x170005BF RID: 1471
		// (get) Token: 0x06001BB8 RID: 7096 RVA: 0x000575AB File Offset: 0x000557AB
		private static Collider2D[] DefaultQueriesBuffer
		{
			get
			{
				Collider2D[] result;
				if ((result = AbilityResourcesCollector.defaultQueriesBuffer) == null)
				{
					result = (AbilityResourcesCollector.defaultQueriesBuffer = new Collider2D[2048]);
				}
				return result;
			}
		}

		// Token: 0x06001BB9 RID: 7097 RVA: 0x000575C8 File Offset: 0x000557C8
		static AbilityResourcesCollector()
		{
			Array values = Enum.GetValues(typeof(AbilityResourceType));
			int num = -1;
			for (int i = 0; i < values.Length; i++)
			{
				int num2 = (int)values.GetValue(i);
				if (num2 > num)
				{
					num = num2;
				}
			}
			AbilityResourcesCollector.CollectedResourcesCounters = new AbilityResourcesCollector.CollectedResourcesCounter[num + 1];
		}

		// Token: 0x170005C0 RID: 1472
		// (get) Token: 0x06001BBA RID: 7098 RVA: 0x00057618 File Offset: 0x00055818
		public float CollectionRange
		{
			get
			{
				return this.currentParams.collectionRange;
			}
		}

		// Token: 0x170005C1 RID: 1473
		// (get) Token: 0x06001BBB RID: 7099 RVA: 0x00057625 File Offset: 0x00055825
		public AbilityResourcesCollector.CollectedResourcesInfo LastCollectedResourcesInfo
		{
			get
			{
				return this.lastCollectedResourcesInfo;
			}
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x0005762D File Offset: 0x0005582D
		private int GetResourceMask(AbilityResourceType resourceType)
		{
			return 1 << (int)resourceType;
		}

		// Token: 0x06001BBD RID: 7101 RVA: 0x00057635 File Offset: 0x00055835
		private ref AbilityResourcesCollector.CollectedResourcesCounter GetResourcesCounter(AbilityResourceType resourceType)
		{
			return ref AbilityResourcesCollector.CollectedResourcesCounters[(int)resourceType];
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x00057644 File Offset: 0x00055844
		private Collider2D[] FindResources(Vector3 position, AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, out int foundResourcesCount, bool sortResources = true)
		{
			foundResourcesCount = 0;
			if (this.currentParams.resourcesCollectionLayers == 0 || this.currentParams.collectionRange <= 0f)
			{
				return null;
			}
			if (requiredResources == null || requiredResources.Length == 0)
			{
				return null;
			}
			int num = 0;
			foreach (AbilityResourcesCollector.RequiredResourceInfo requiredResourceInfo in requiredResources)
			{
				if (requiredResourceInfo.IsValid())
				{
					AbilityResourcesCollector.CollectedResourcesCounters[(int)requiredResourceInfo.resourceType] = new AbilityResourcesCollector.CollectedResourcesCounter(requiredResourceInfo.requiredAmount);
					num |= this.GetResourceMask(requiredResourceInfo.resourceType);
				}
			}
			if (num == 0)
			{
				return null;
			}
			Collider2D[] array = this.collectionQueriesBuffer ?? AbilityResourcesCollector.DefaultQueriesBuffer;
			int num2 = Physics2D.OverlapCircleNonAlloc(position, this.currentParams.collectionRange, array, this.currentParams.resourcesCollectionLayers);
			if (num2 == 0)
			{
				return null;
			}
			for (int j = 0; j < num2; j++)
			{
				CollectableAbilityResource collectableAbilityResource;
				if (array[j].TryGetComponent<CollectableAbilityResource>(out collectableAbilityResource))
				{
					AbilityResourceType type = collectableAbilityResource.type;
					if (type != AbilityResourceType.Undefined && (this.resourcesFilter == null || this.resourcesFilter(this, collectableAbilityResource)) && collectableAbilityResource.CurrentCollector == null && (num & this.GetResourceMask(type)) != 0)
					{
						ref AbilityResourcesCollector.CollectedResourcesCounter resourcesCounter = ref this.GetResourcesCounter(type);
						Collider2D[] array2 = array;
						int num3 = foundResourcesCount;
						foundResourcesCount = num3 + 1;
						array2[num3] = array[j];
						resourcesCounter.collectedAmount++;
					}
				}
			}
			if (sortResources && this.resourcesSortingComparer != null)
			{
				IDistanceBasedComparer distanceBasedComparer = this.resourcesSortingComparer as IDistanceBasedComparer;
				if (distanceBasedComparer != null)
				{
					distanceBasedComparer.SortingPoint = position;
				}
				Component[] array3 = array;
				Array.Sort<Component>(array3, 0, foundResourcesCount, this.resourcesSortingComparer);
			}
			return array;
		}

		// Token: 0x06001BBF RID: 7103 RVA: 0x000577CC File Offset: 0x000559CC
		private bool SetRequiredResourcesAmounts(AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, int maxUsingCount, out int finalUsingCount, out int targetResourcesCount)
		{
			finalUsingCount = 0;
			targetResourcesCount = 0;
			int num = int.MaxValue;
			for (int i = 0; i < requiredResources.Length; i++)
			{
				int usingCount = AbilityResourcesCollector.CollectedResourcesCounters[(int)requiredResources[i].resourceType].GetUsingCount();
				if (usingCount < num)
				{
					num = usingCount;
				}
			}
			if (num == 0)
			{
				return false;
			}
			if (maxUsingCount > 0 && num > maxUsingCount)
			{
				num = maxUsingCount;
			}
			for (int j = 0; j < requiredResources.Length; j++)
			{
				ref AbilityResourcesCollector.CollectedResourcesCounter resourcesCounter = ref this.GetResourcesCounter(requiredResources[j].resourceType);
				resourcesCounter.targetAmount = num * resourcesCounter.usingCost;
				targetResourcesCount += resourcesCounter.targetAmount;
				resourcesCounter.collectedAmount = 0;
			}
			finalUsingCount = num;
			return true;
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x00057874 File Offset: 0x00055A74
		private AbilityResourcesCollector.CollectedResourcesInfo CollectResources(Vector3 position, AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, int maxUsingCount)
		{
			this.collectedResources.Clear();
			this.lastCollectedResourcesInfo = default(AbilityResourcesCollector.CollectedResourcesInfo);
			int num;
			Collider2D[] array = this.FindResources(position, requiredResources, out num, true);
			int usingCount;
			int num2;
			if (num != 0 && this.SetRequiredResourcesAmounts(requiredResources, maxUsingCount, out usingCount, out num2))
			{
				int num3 = 0;
				while (num3 < num && this.collectedResources.Count < num2)
				{
					CollectableAbilityResource collectableAbilityResource;
					array[num3].TryGetComponent<CollectableAbilityResource>(out collectableAbilityResource);
					ref AbilityResourcesCollector.CollectedResourcesCounter resourcesCounter = ref this.GetResourcesCounter(collectableAbilityResource.type);
					if (!resourcesCounter.IsTargetCountReached())
					{
						this.collectedResources.Add(collectableAbilityResource);
						resourcesCounter.collectedAmount++;
					}
					num3++;
				}
				this.lastCollectedResourcesInfo = new AbilityResourcesCollector.CollectedResourcesInfo(this, requiredResources, usingCount);
				if (this.lastCollectedResourcesInfo.IsCollected())
				{
					foreach (CollectableAbilityResource collectableAbilityResource2 in this.lastCollectedResourcesInfo.Resources)
					{
						collectableAbilityResource2.SetReservedForCollector(this.Ability);
					}
				}
			}
			return this.lastCollectedResourcesInfo;
		}

		// Token: 0x06001BC1 RID: 7105 RVA: 0x00057988 File Offset: 0x00055B88
		public AbilityResourcesCollector(BaseAbility ability, AbilityResourcesCollector.Parameters parameters, Collider2D[] collectionQueriesBuffer = null)
		{
			this.Ability = ability;
			this.currentParams = parameters;
			this.collectionQueriesBuffer = collectionQueriesBuffer;
			this.nextResourcesCheckTime = Time.time + UnityEngine.Random.value * 0.5f;
		}

		// Token: 0x06001BC2 RID: 7106 RVA: 0x000579D8 File Offset: 0x00055BD8
		public bool HasResourcesInRange(Vector3 position, AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, float timeStep = 0.1f)
		{
			float time = Time.time;
			if (time >= this.nextResourcesCheckTime)
			{
				this.nextResourcesCheckTime = time + timeStep;
				int num;
				this.hasResourcesInRange = (this.FindResources(position, requiredResources, out num, true) != null);
				if (this.hasResourcesInRange)
				{
					foreach (AbilityResourcesCollector.RequiredResourceInfo requiredResourceInfo in requiredResources)
					{
						if (this.GetResourcesCounter(requiredResourceInfo.resourceType).collectedAmount < requiredResourceInfo.requiredAmount)
						{
							this.hasResourcesInRange = false;
							break;
						}
					}
				}
			}
			return this.hasResourcesInRange;
		}

		// Token: 0x06001BC3 RID: 7107 RVA: 0x00057A58 File Offset: 0x00055C58
		public bool CollectResources(Vector2 position, AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, out AbilityResourcesCollector.CollectedResourcesInfo collectedResources, out AbilityResourcesBlock lackOfResources, int maxUsingCount = 0)
		{
			collectedResources = this.CollectResources(position, requiredResources, maxUsingCount);
			lackOfResources = default(AbilityResourcesBlock);
			if (!collectedResources.IsCollected())
			{
				foreach (AbilityResourcesCollector.RequiredResourceInfo requiredResourceInfo in requiredResources)
				{
					ref AbilityResourcesCollector.CollectedResourcesCounter resourcesCounter = ref this.GetResourcesCounter(requiredResourceInfo.resourceType);
					int num = requiredResourceInfo.requiredAmount - resourcesCounter.collectedAmount;
					if (num > 0)
					{
						lackOfResources[requiredResourceInfo.resourceType] = num;
					}
				}
				return false;
			}
			return true;
		}

		// Token: 0x06001BC4 RID: 7108 RVA: 0x00057AD0 File Offset: 0x00055CD0
		public void ReleaseCollectedResources()
		{
			List<CollectableAbilityResource> resources = this.lastCollectedResourcesInfo.Resources;
			if (resources != null)
			{
				foreach (CollectableAbilityResource collectableAbilityResource in resources)
				{
					if (!collectableAbilityResource.IsNull())
					{
						collectableAbilityResource.TryResetCollector(this.Ability);
					}
				}
			}
			this.lastCollectedResourcesInfo = default(AbilityResourcesCollector.CollectedResourcesInfo);
		}

		// Token: 0x04000FAE RID: 4014
		private static readonly AbilityResourcesCollector.CollectedResourcesCounter[] CollectedResourcesCounters;

		// Token: 0x04000FAF RID: 4015
		private static Collider2D[] defaultQueriesBuffer;

		// Token: 0x04000FB0 RID: 4016
		public readonly BaseAbility Ability;

		// Token: 0x04000FB1 RID: 4017
		public IComparer<Component> resourcesSortingComparer;

		// Token: 0x04000FB2 RID: 4018
		public Func<AbilityResourcesCollector, CollectableAbilityResource, bool> resourcesFilter;

		// Token: 0x04000FB3 RID: 4019
		private readonly AbilityResourcesCollector.Parameters currentParams;

		// Token: 0x04000FB4 RID: 4020
		private readonly Collider2D[] collectionQueriesBuffer;

		// Token: 0x04000FB5 RID: 4021
		private readonly List<CollectableAbilityResource> collectedResources = new List<CollectableAbilityResource>(128);

		// Token: 0x04000FB6 RID: 4022
		private AbilityResourcesCollector.CollectedResourcesInfo lastCollectedResourcesInfo;

		// Token: 0x04000FB7 RID: 4023
		private float nextResourcesCheckTime;

		// Token: 0x04000FB8 RID: 4024
		private bool hasResourcesInRange;

		// Token: 0x02000554 RID: 1364
		[Serializable]
		public struct RequiredResourceInfo
		{
			// Token: 0x060026DE RID: 9950 RVA: 0x00078F4B File Offset: 0x0007714B
			public bool IsValid()
			{
				return this.resourceType != AbilityResourceType.Undefined && this.requiredAmount > 0;
			}

			// Token: 0x04001BD6 RID: 7126
			public AbilityResourceType resourceType;

			// Token: 0x04001BD7 RID: 7127
			public int requiredAmount;
		}

		// Token: 0x02000555 RID: 1365
		private struct CollectedResourcesCounter
		{
			// Token: 0x060026DF RID: 9951 RVA: 0x00078F61 File Offset: 0x00077161
			public CollectedResourcesCounter(int usingCost)
			{
				this.usingCost = usingCost;
				this.collectedAmount = 0;
				this.targetAmount = 0;
			}

			// Token: 0x060026E0 RID: 9952 RVA: 0x00078F78 File Offset: 0x00077178
			public int GetUsingCount()
			{
				if (this.usingCost <= 0 || this.collectedAmount < this.usingCost)
				{
					return 0;
				}
				return this.collectedAmount / this.usingCost;
			}

			// Token: 0x060026E1 RID: 9953 RVA: 0x00078FA0 File Offset: 0x000771A0
			public bool IsTargetCountReached()
			{
				return this.collectedAmount == this.targetAmount;
			}

			// Token: 0x04001BD8 RID: 7128
			public int collectedAmount;

			// Token: 0x04001BD9 RID: 7129
			public int usingCost;

			// Token: 0x04001BDA RID: 7130
			public int targetAmount;
		}

		// Token: 0x02000556 RID: 1366
		public readonly struct CollectedResourcesInfo
		{
			// Token: 0x060026E2 RID: 9954 RVA: 0x00078FB0 File Offset: 0x000771B0
			public CollectedResourcesInfo(AbilityResourcesCollector collector, AbilityResourcesCollector.RequiredResourceInfo[] requiredResourcesInfo, int usingCount)
			{
				this.Collector = collector;
				this.RequiredResourcesInfo = requiredResourcesInfo;
				this.Resources = collector.collectedResources;
				this.UsingCount = usingCount;
			}

			// Token: 0x060026E3 RID: 9955 RVA: 0x00078FD3 File Offset: 0x000771D3
			public bool IsCollected()
			{
				return this.Resources != null && this.UsingCount != 0;
			}

			// Token: 0x04001BDB RID: 7131
			public readonly AbilityResourcesCollector Collector;

			// Token: 0x04001BDC RID: 7132
			public readonly AbilityResourcesCollector.RequiredResourceInfo[] RequiredResourcesInfo;

			// Token: 0x04001BDD RID: 7133
			public readonly List<CollectableAbilityResource> Resources;

			// Token: 0x04001BDE RID: 7134
			public readonly int UsingCount;
		}

		// Token: 0x02000557 RID: 1367
		[Serializable]
		public sealed class Parameters : ICloneable<AbilityResourcesCollector.Parameters>
		{
			// Token: 0x060026E4 RID: 9956 RVA: 0x00078FE8 File Offset: 0x000771E8
			public AbilityResourcesCollector.Parameters Clone()
			{
				return new AbilityResourcesCollector.Parameters
				{
					resourcesCollectionLayers = this.resourcesCollectionLayers,
					collectionRange = this.collectionRange
				};
			}

			// Token: 0x04001BDF RID: 7135
			public LayerMask resourcesCollectionLayers;

			// Token: 0x04001BE0 RID: 7136
			public float collectionRange;
		}
	}
}
