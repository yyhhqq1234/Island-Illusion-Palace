using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x02000360 RID: 864
	[CreateAssetMenu(fileName = "CollectableAbilityResourcesFactoryBuilder", menuName = "Game/Factories/Collectable Resources Factory Builder")]
	public sealed class CollectableAbilityResourcesFactoryBuilder : PrototypeBasedFactoryBuilder<CollectableAbilityResourcesFactory.PrototypeInfo, CollectableAbilityResource>
	{
		// Token: 0x170005DE RID: 1502
		// (get) Token: 0x06001C5C RID: 7260 RVA: 0x00059A86 File Offset: 0x00057C86
		// (set) Token: 0x06001C5D RID: 7261 RVA: 0x00059A93 File Offset: 0x00057C93
		public override CollectableAbilityResourcesFactory.PrototypeInfo[] FactoryData
		{
			get
			{
				return this.factoryData;
			}
			set
			{
				this.factoryData.list = value;
			}
		}

		// Token: 0x06001C5E RID: 7262 RVA: 0x00059AA1 File Offset: 0x00057CA1
		protected override PrototypeBasedFactory<CollectableAbilityResourcesFactory.PrototypeInfo, CollectableAbilityResource> CreateFactoryInternal()
		{
			return new CollectableAbilityResourcesFactory();
		}

		// Token: 0x0400100C RID: 4108
		[SerializeField]
		[CustomReorderableList]
		private ReorderableListAdapter<CollectableAbilityResourcesFactory.PrototypeInfo[]> factoryData;
	}
}
