using System;
using Common.UnityExtensions;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.AbilityResources
{
	// Token: 0x02000359 RID: 857
	public sealed class AbilityResourcesGenerator : IDisposable
	{
		// Token: 0x170005C8 RID: 1480
		// (get) Token: 0x06001BE3 RID: 7139 RVA: 0x0005802C File Offset: 0x0005622C
		public object Owner
		{
			get
			{
				return this.owner;
			}
		}

		// Token: 0x170005C9 RID: 1481
		// (get) Token: 0x06001BE4 RID: 7140 RVA: 0x00058034 File Offset: 0x00056234
		// (set) Token: 0x06001BE5 RID: 7141 RVA: 0x0005803C File Offset: 0x0005623C
		public bool IsActive { get; set; }

		// Token: 0x1400010D RID: 269
		// (add) Token: 0x06001BE6 RID: 7142 RVA: 0x00058048 File Offset: 0x00056248
		// (remove) Token: 0x06001BE7 RID: 7143 RVA: 0x00058080 File Offset: 0x00056280
		public event Action<AbilityResourcesGenerator, AbilityResourcesGenerator.GeneratedResourceInfo> ResourceGenerated;

		// Token: 0x06001BE8 RID: 7144 RVA: 0x000580B8 File Offset: 0x000562B8
		private void UpdateDamageReceiverSubscriptions(bool subscribe)
		{
			if (this.damageReceiver == null)
			{
				return;
			}
			if (subscribe)
			{
				this.damageReceiver.HitReceived += this.OnOwnerHitReceived;
				this.damageReceiver.TotallyDestroyed += this.OnOwnerTotallyDestroyed;
				return;
			}
			this.damageReceiver.HitReceived -= this.OnOwnerHitReceived;
			this.damageReceiver.TotallyDestroyed -= this.OnOwnerTotallyDestroyed;
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x00058130 File Offset: 0x00056330
		private AbilityResourcesGenerator.GeneratedResourceInfo GenerateResource(AbilityResourceType resourceType, GameObject arbitraryResourceObject = null)
		{
			if (GameApplication.IsGameStateChanging || !this.IsActive || resourceType == AbilityResourceType.Undefined)
			{
				return AbilityResourcesGenerator.GeneratedResourceInfo.Invalid;
			}
			AbilityResourcesGenerator.ResourcesFactoryArgs.resourceType = resourceType;
			AbilityResourcesGenerator.ResourcesFactoryArgs.resourceOwner = this.owner;
			AbilityResourcesGenerator.ResourcesFactoryArgs.arbitraryResourceObject = arbitraryResourceObject;
			AbilityResourcesGenerator.ResourcesFactoryArgs.isStatic = (resourceType == AbilityResourceType.Corpse);
			CollectableAbilityResource collectableResource = this.resourcesFactory.Create(AbilityResourcesGenerator.ResourcesFactoryArgs);
			AbilityResourcesGenerator.GeneratedResourceInfo generatedResourceInfo = new AbilityResourcesGenerator.GeneratedResourceInfo(resourceType, collectableResource);
			Action<AbilityResourcesGenerator, AbilityResourcesGenerator.GeneratedResourceInfo> resourceGenerated = this.ResourceGenerated;
			if (resourceGenerated != null)
			{
				resourceGenerated(this, generatedResourceInfo);
			}
			return generatedResourceInfo;
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x000581B8 File Offset: 0x000563B8
		private AbilityResourcesGenerator.GeneratedResourceInfo TryGenerateResource(GeneratableAbilityResource resourceInfo, float randomValueOverride = -1f)
		{
			if (resourceInfo.resourceType != AbilityResourceType.Undefined)
			{
				float randomValue = (randomValueOverride > 0f) ? randomValueOverride : UnityEngine.Random.value;
				AbilityResourceType resourceType;
				if (resourceInfo.TryGet(randomValue, out resourceType))
				{
					return this.GenerateResource(resourceType, null);
				}
			}
			return AbilityResourcesGenerator.GeneratedResourceInfo.Invalid;
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x000581FC File Offset: 0x000563FC
		private void GenerateResources(GeneratableAbilityResource[] resourcesInfo)
		{
			if (resourcesInfo != null && resourcesInfo.Length != 0)
			{
				float value = UnityEngine.Random.value;
				for (int i = 0; i < resourcesInfo.Length; i++)
				{
					this.TryGenerateResource(resourcesInfo[i], value);
				}
			}
		}

		// Token: 0x06001BEC RID: 7148 RVA: 0x00058233 File Offset: 0x00056433
		public AbilityResourcesGenerator(object owner, AbilityResourcesGenerator.Parameters parameters, IDamageable damageReceiver, ICollectableAbilityResourcesFactory resourcesFactory)
		{
			this.owner = owner;
			this.currentParams = parameters;
			this.damageReceiver = damageReceiver;
			this.resourcesFactory = resourcesFactory;
			this.UpdateDamageReceiverSubscriptions(true);
			this.IsActive = true;
		}

		// Token: 0x06001BED RID: 7149 RVA: 0x00058268 File Offset: 0x00056468
		public AbilityResourcesGenerator(object owner, ICollectableAbilityResourcesFactory resourcesFactory) : this(owner, default(AbilityResourcesGenerator.Parameters), null, resourcesFactory)
		{
		}

		// Token: 0x06001BEE RID: 7150 RVA: 0x00058287 File Offset: 0x00056487
		public bool TryGenerateResource(GeneratableAbilityResource resourceInfo, out AbilityResourcesGenerator.GeneratedResourceInfo generatedResource)
		{
			generatedResource = this.TryGenerateResource(resourceInfo, -1f);
			return generatedResource.IsValid();
		}

		// Token: 0x06001BEF RID: 7151 RVA: 0x000582A4 File Offset: 0x000564A4
		public void GenerateResources(AbilityResourceType resourceType, int count, Action<AbilityResourcesGenerator.GeneratedResourceInfo> generationCallback = null)
		{
			for (int i = 0; i < count; i++)
			{
				AbilityResourcesGenerator.GeneratedResourceInfo obj = this.GenerateResource(resourceType, null);
				if (obj.IsValid() && generationCallback != null)
				{
					generationCallback(obj);
				}
			}
		}

		// Token: 0x06001BF0 RID: 7152 RVA: 0x000582DC File Offset: 0x000564DC
		public bool TryGenerateCorpseResource(BaseGameMob mob, out AbilityResourcesGenerator.GeneratedResourceInfo generatedResource)
		{
			if (mob.IsKilled)
			{
				generatedResource = this.GenerateResource(AbilityResourceType.Corpse, mob.gameObject);
				CollectableAbilityResource collectableResource = generatedResource.CollectableResource;
				if (collectableResource != null)
				{
					float maxHitPoints = mob.MaxHitPoints;
					if (maxHitPoints > 0f && !collectableResource.HasCollectionPriority())
					{
						collectableResource.CollectionPriority = 100000 - (int)maxHitPoints;
					}
					return true;
				}
			}
			generatedResource = AbilityResourcesGenerator.GeneratedResourceInfo.Invalid;
			return false;
		}

		// Token: 0x06001BF1 RID: 7153 RVA: 0x00058347 File Offset: 0x00056547
		public void Dispose()
		{
			this.UpdateDamageReceiverSubscriptions(false);
			this.owner = null;
		}

		// Token: 0x06001BF2 RID: 7154 RVA: 0x00058357 File Offset: 0x00056557
		private void OnOwnerHitReceived(IDamageable hitPointsController, object attacker, IHitPointsChangingArgs args)
		{
			if (!args.IsDamage || args.Amount <= 0f || args.IsSilentHitPointsModification)
			{
				return;
			}
			this.GenerateResources(this.currentParams.ownerImpactResources);
		}

		// Token: 0x06001BF3 RID: 7155 RVA: 0x00058388 File Offset: 0x00056588
		private void OnOwnerTotallyDestroyed(IDamageable damageReceiver)
		{
			this.GenerateResources(this.currentParams.ownerDeathResources);
		}

		// Token: 0x04000FC9 RID: 4041
		private static readonly CollectableAbilityResourcesFactoryArgs ResourcesFactoryArgs = new CollectableAbilityResourcesFactoryArgs();

		// Token: 0x04000FCC RID: 4044
		private readonly AbilityResourcesGenerator.Parameters currentParams;

		// Token: 0x04000FCD RID: 4045
		private readonly IDamageable damageReceiver;

		// Token: 0x04000FCE RID: 4046
		private readonly ICollectableAbilityResourcesFactory resourcesFactory;

		// Token: 0x04000FCF RID: 4047
		private object owner;

		// Token: 0x0200055B RID: 1371
		[Serializable]
		public struct Parameters
		{
			// Token: 0x04001BE6 RID: 7142
			public GeneratableAbilityResource[] ownerImpactResources;

			// Token: 0x04001BE7 RID: 7143
			public GeneratableAbilityResource[] ownerDeathResources;

			// Token: 0x04001BE8 RID: 7144
			public GeneratableAbilityResource[] energyRestoreResources;
		}

		// Token: 0x0200055C RID: 1372
		public readonly struct GeneratedResourceInfo
		{
			// Token: 0x060026F1 RID: 9969 RVA: 0x000791FB File Offset: 0x000773FB
			public bool IsValid()
			{
				return !this.CollectableResource.IsNull() && this.Type != AbilityResourceType.Undefined;
			}

			// Token: 0x060026F2 RID: 9970 RVA: 0x00079218 File Offset: 0x00077418
			public GeneratedResourceInfo(AbilityResourceType type, CollectableAbilityResource collectableResource)
			{
				this.Type = type;
				this.CollectableResource = collectableResource;
			}

			// Token: 0x04001BE9 RID: 7145
			public static readonly AbilityResourcesGenerator.GeneratedResourceInfo Invalid = new AbilityResourcesGenerator.GeneratedResourceInfo(AbilityResourceType.Undefined, null);

			// Token: 0x04001BEA RID: 7146
			public readonly AbilityResourceType Type;

			// Token: 0x04001BEB RID: 7147
			public readonly CollectableAbilityResource CollectableResource;
		}
	}
}
