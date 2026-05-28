using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.Factories;
using Unliving.LevelGeneration;

namespace Unliving.Essence
{
	// Token: 0x020002CD RID: 717
	[CreateAssetMenu(fileName = "EssenceFactoryBuilder", menuName = "Game/Factories/Essence Factory Builder")]
	public sealed class EssenceFactoryBuilder : PrototypeBasedFactoryBuilder<EssenceFactory.PrototypeInfo, EssenceBase>
	{
		// Token: 0x1700054B RID: 1355
		// (get) Token: 0x060018F7 RID: 6391 RVA: 0x0004EC5E File Offset: 0x0004CE5E
		// (set) Token: 0x060018F8 RID: 6392 RVA: 0x0004EC6B File Offset: 0x0004CE6B
		public override EssenceFactory.PrototypeInfo[] FactoryData
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

		// Token: 0x060018F9 RID: 6393 RVA: 0x0004EC79 File Offset: 0x0004CE79
		protected override PrototypeBasedFactory<EssenceFactory.PrototypeInfo, EssenceBase> CreateFactoryInternal()
		{
			return new EssenceFactory
			{
				defaultEssenceData = this.defaultEssenceData,
				locationsLevelsData = this.locationsLevelsData,
				levelGenerationSettings = this.levelGenerationSettings
			};
		}

		// Token: 0x04000E13 RID: 3603
		public MultiRepresentationObjectInstantiator.DefaultData defaultEssenceData;

		// Token: 0x04000E14 RID: 3604
		public EssenceFactoryBuilder.LocationLevelData[] locationsLevelsData;

		// Token: 0x04000E15 RID: 3605
		public EssenceFactoryBuilder.LevelGenerationSettings levelGenerationSettings;

		// Token: 0x04000E16 RID: 3606
		[SerializeField]
		[CustomReorderableList]
		private ReorderableListAdapter<EssenceFactory.PrototypeInfo[]> factoryData;

		// Token: 0x02000536 RID: 1334
		[Serializable]
		public struct LocationLevelData
		{
			// Token: 0x04001B74 RID: 7028
			public GameLocation.TypeID location;

			// Token: 0x04001B75 RID: 7029
			public int minLevel;
		}

		// Token: 0x02000537 RID: 1335
		[Serializable]
		public class LevelGenerationSettings
		{
			// Token: 0x04001B76 RID: 7030
			public int maxItemLevel = 6;

			// Token: 0x04001B77 RID: 7031
			public int noNoveltyBonus = 1;

			// Token: 0x04001B78 RID: 7032
			public int sameBase = 1;

			// Token: 0x04001B79 RID: 7033
			public int sameRandomBonus = 1;

			// Token: 0x04001B7A RID: 7034
			public float equalLevelRandomChance = 0.1f;

			// Token: 0x04001B7B RID: 7035
			public float sameRandomChance = 0.2f;

			// Token: 0x04001B7C RID: 7036
			public int equalLevelRandomBonus = 1;
		}
	}
}
