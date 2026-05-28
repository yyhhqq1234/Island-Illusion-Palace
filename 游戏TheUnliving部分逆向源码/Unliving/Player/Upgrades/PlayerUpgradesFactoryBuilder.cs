using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000170 RID: 368
	[CreateAssetMenu(fileName = "PlayerUpgradesFactoryBuilder", menuName = "Game/Factories/Player Upgrades Factory Builder")]
	public sealed class PlayerUpgradesFactoryBuilder : PrototypeBasedFactoryBuilder<PlayerUpgradeData, IPlayerUpgrade>
	{
		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06000A38 RID: 2616 RVA: 0x000221CE File Offset: 0x000203CE
		// (set) Token: 0x06000A39 RID: 2617 RVA: 0x000221DB File Offset: 0x000203DB
		public override PlayerUpgradeData[] FactoryData
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

		// Token: 0x06000A3A RID: 2618 RVA: 0x000221E9 File Offset: 0x000203E9
		protected override PrototypeBasedFactory<PlayerUpgradeData, IPlayerUpgrade> CreateFactoryInternal()
		{
			return new PlayerUpgradesFactory(this.factoryParams);
		}

		// Token: 0x04000604 RID: 1540
		public PlayerUpgradesFactoryParams factoryParams;

		// Token: 0x04000605 RID: 1541
		[SerializeField]
		[CustomReorderableList(null, "upgradePrototype", true, true)]
		private ReorderableListAdapter<PlayerUpgradeData[]> factoryData;
	}
}
