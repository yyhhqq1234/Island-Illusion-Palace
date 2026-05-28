using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000142 RID: 322
	[CreateAssetMenu(fileName = "PlayerFactoryBuilder", menuName = "Game/Factories/Player Factory Builder")]
	public sealed class PlayerFactoryBuilder : PrototypeBasedFactoryBuilder<PlayerBehaviour.FactoryPrototype, IGameMob>
	{
		// Token: 0x1700015D RID: 349
		// (get) Token: 0x0600085B RID: 2139 RVA: 0x0001BFC6 File Offset: 0x0001A1C6
		// (set) Token: 0x0600085C RID: 2140 RVA: 0x0001BFD3 File Offset: 0x0001A1D3
		public override PlayerBehaviour.FactoryPrototype[] FactoryData
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

		// Token: 0x0600085D RID: 2141 RVA: 0x0001BFE1 File Offset: 0x0001A1E1
		protected override PrototypeBasedFactory<PlayerBehaviour.FactoryPrototype, IGameMob> CreateFactoryInternal()
		{
			return new PlayerFactory(this.locationDependentData)
			{
				useAttachableMobsEffectsPool = this.useAttachableEffectsPool
			};
		}

		// Token: 0x040004BF RID: 1215
		public bool useAttachableEffectsPool;

		// Token: 0x040004C0 RID: 1216
		public LocationDependentObjectAttacher.Data[] locationDependentData;

		// Token: 0x040004C1 RID: 1217
		[SerializeField]
		[CustomReorderableList(null, "objectPrefab", true, true)]
		private ReorderableListAdapter<PlayerBehaviour.FactoryPrototype[]> factoryData;
	}
}
