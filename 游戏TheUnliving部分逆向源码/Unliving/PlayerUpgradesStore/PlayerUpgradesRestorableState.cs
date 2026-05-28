using System;
using System.Collections.Generic;
using Common;
using Common.RestorableState;
using UnityEngine;
using Unliving.Player.Upgrades;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x02000119 RID: 281
	[Serializable]
	public sealed class PlayerUpgradesRestorableState : RestorableStateBase<IPlayerUpgradesStoreManager>, ICloneable<PlayerUpgradesRestorableState>
	{
		// Token: 0x17000111 RID: 273
		// (get) Token: 0x060006BB RID: 1723 RVA: 0x00015DC6 File Offset: 0x00013FC6
		public IReadOnlyList<PlayerUpgradeInfo> StoredUpgradesInfo
		{
			get
			{
				return this.storedUpgradesInfo;
			}
		}

		// Token: 0x060006BC RID: 1724 RVA: 0x00015DCE File Offset: 0x00013FCE
		public PlayerUpgradesRestorableState() : base(null)
		{
		}

		// Token: 0x060006BD RID: 1725 RVA: 0x00015DD7 File Offset: 0x00013FD7
		public PlayerUpgradesRestorableState(IPlayerUpgradesStoreManager storeManager) : base(storeManager)
		{
		}

		// Token: 0x060006BE RID: 1726 RVA: 0x00015DE0 File Offset: 0x00013FE0
		public PlayerUpgradesRestorableState Clone()
		{
			return new PlayerUpgradesRestorableState
			{
				storedUpgradesInfo = new List<PlayerUpgradeInfo>(this.storedUpgradesInfo)
			};
		}

		// Token: 0x060006BF RID: 1727 RVA: 0x00015DF8 File Offset: 0x00013FF8
		public override void Store(IPlayerUpgradesStoreManager storeManager)
		{
			this.storedUpgradesInfo.Clear();
			IReadOnlyList<PurchasablePlayerUpgradesSequence> upgradesData = storeManager.UpgradesData;
			for (int i = 0; i < upgradesData.Count; i++)
			{
				PurchasablePlayerUpgradesSequence purchasablePlayerUpgradesSequence = upgradesData[i];
				if (purchasablePlayerUpgradesSequence.IsAvailable)
				{
					IReadOnlyList<PurchasablePlayerUpgradeData> upgrades = purchasablePlayerUpgradesSequence.Upgrades;
					for (int j = 0; j < upgrades.Count; j++)
					{
						PlayerUpgradeInfo item;
						if (upgrades[j].TryGetPlayerUpgradeInfo(out item))
						{
							this.storedUpgradesInfo.Add(item);
						}
					}
				}
			}
		}

		// Token: 0x060006C0 RID: 1728 RVA: 0x00015E72 File Offset: 0x00014072
		public void Restore(IPlayerUpgradesStoreManager storeManager, IReadOnlyList<PlayerUpgradeInfo> selectedUpgrades)
		{
			storeManager.LoadData(this.storedUpgradesInfo, selectedUpgrades);
		}

		// Token: 0x060006C1 RID: 1729 RVA: 0x00015E81 File Offset: 0x00014081
		public override void Restore(IPlayerUpgradesStoreManager storeManager, object args = null)
		{
			this.Restore(storeManager, args as IReadOnlyList<PlayerUpgradeInfo>);
		}

		// Token: 0x04000416 RID: 1046
		[SerializeField]
		private List<PlayerUpgradeInfo> storedUpgradesInfo;
	}
}
