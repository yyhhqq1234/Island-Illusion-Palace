using System;
using System.Collections.Generic;
using Game.Abilities;

namespace Unliving.AbilityResources
{
	// Token: 0x02000365 RID: 869
	public interface IAbilityResourcesConsumer
	{
		// Token: 0x170005E3 RID: 1507
		// (get) Token: 0x06001C79 RID: 7289
		AbilityResourcesCollector.RequiredResourceInfo[] RequiredResources { get; }

		// Token: 0x06001C7A RID: 7290
		float GetResourcesCollectionRange(BaseAbility consumingAbility);

		// Token: 0x06001C7B RID: 7291
		IReadOnlyList<CollectableAbilityResource> GetCollectedResources(object context);
	}
}
