using System;

namespace Unliving.AbilityResources
{
	// Token: 0x02000361 RID: 865
	public interface ICollectableAbilityResourcesFactory
	{
		// Token: 0x14000113 RID: 275
		// (add) Token: 0x06001C60 RID: 7264
		// (remove) Token: 0x06001C61 RID: 7265
		event Action<CollectableAbilityResourcesFactoryArgs, CollectableAbilityResource> ResourceCreated;

		// Token: 0x06001C62 RID: 7266
		CollectableAbilityResource Create(CollectableAbilityResourcesFactoryArgs args);
	}
}
