using System;
using Common.ServiceRegistry;
using Game.Buffs;
using Game.Metadata;
using UnityEngine;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003DD RID: 989
	[CreateAssetMenu(fileName = "BuffsMetadataStorageManager", menuName = "Game/Metadata Storage/Buffs")]
	[Service(typeof(IMetadataStorageManager<int, IBuff>), new Type[]
	{

	})]
	public class BuffsMetadataStorageManager : MetadataStorageManagerBase<int, IBuff>
	{
		// Token: 0x170006D3 RID: 1747
		// (get) Token: 0x06002198 RID: 8600 RVA: 0x00068F74 File Offset: 0x00067174
		public override Metadata<int>[] Metadata
		{
			get
			{
				return this.buffsMetadata;
			}
		}

		// Token: 0x040014F3 RID: 5363
		public BuffsMetadataStorageManager.BuffMetadata[] buffsMetadata;

		// Token: 0x02000597 RID: 1431
		[Serializable]
		public class BuffMetadata : Metadata<int>
		{
			// Token: 0x17000817 RID: 2071
			// (get) Token: 0x060027B7 RID: 10167 RVA: 0x0007C57D File Offset: 0x0007A77D
			public override int ID
			{
				get
				{
					return this.generatorBuilderAsset.GetBuffsID();
				}
			}

			// Token: 0x060027B8 RID: 10168 RVA: 0x0007C58A File Offset: 0x0007A78A
			public override string GetLocalizationString()
			{
				if (string.IsNullOrEmpty(this.localizationID))
				{
					return "buffID_" + this.generatorBuilderAsset.name;
				}
				return this.localizationID;
			}

			// Token: 0x04001D03 RID: 7427
			[SerializeField]
			private string localizationID;

			// Token: 0x04001D04 RID: 7428
			public BuffsGeneratorBuilderAsset generatorBuilderAsset;
		}
	}
}
