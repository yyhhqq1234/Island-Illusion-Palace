using System;
using System.Collections.Generic;
using Game.Abilities;

namespace Unliving.AbilityResources
{
	// Token: 0x02000364 RID: 868
	public interface IAbilityResourcesCollectingEntity
	{
		// Token: 0x170005E2 RID: 1506
		// (get) Token: 0x06001C75 RID: 7285
		// (set) Token: 0x06001C76 RID: 7286
		float ResourcesGatheringDurationOverride { get; set; }

		// Token: 0x06001C77 RID: 7287
		void OnCollectingResources(AbilityResourcesCollector resourcesCollector, float collectionDuration);

		// Token: 0x06001C78 RID: 7288
		void OnConsumingResources(BaseAbility consumingAbility, IReadOnlyList<CollectableAbilityResource> resources);
	}
}
