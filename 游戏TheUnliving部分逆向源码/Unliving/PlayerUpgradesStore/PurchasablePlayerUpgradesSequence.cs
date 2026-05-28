using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unliving.Player.Upgrades;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x0200011E RID: 286
	[Serializable]
	public sealed class PurchasablePlayerUpgradesSequence
	{
		// Token: 0x17000123 RID: 291
		// (get) Token: 0x06000701 RID: 1793 RVA: 0x000167B5 File Offset: 0x000149B5
		// (set) Token: 0x06000702 RID: 1794 RVA: 0x000167BD File Offset: 0x000149BD
		public bool IsAvailable { get; private set; }

		// Token: 0x17000124 RID: 292
		// (get) Token: 0x06000703 RID: 1795 RVA: 0x000167C6 File Offset: 0x000149C6
		// (set) Token: 0x06000704 RID: 1796 RVA: 0x000167CE File Offset: 0x000149CE
		public bool HasAvailableUpgrades { get; private set; }

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x06000705 RID: 1797 RVA: 0x000167D7 File Offset: 0x000149D7
		public IReadOnlyList<PurchasablePlayerUpgradeData> Upgrades
		{
			get
			{
				return this.upgrades;
			}
		}

		// Token: 0x17000126 RID: 294
		// (get) Token: 0x06000706 RID: 1798 RVA: 0x000167DF File Offset: 0x000149DF
		public PurchasablePlayerUpgradeData SelectedUpgrade
		{
			get
			{
				if (this.selectedUpgradeIndex < 0 || this.selectedUpgradeIndex >= this.upgrades.Length)
				{
					return null;
				}
				return this.upgrades[this.selectedUpgradeIndex];
			}
		}

		// Token: 0x17000127 RID: 295
		// (get) Token: 0x06000707 RID: 1799 RVA: 0x00016809 File Offset: 0x00014A09
		// (set) Token: 0x06000708 RID: 1800 RVA: 0x00016811 File Offset: 0x00014A11
		public int SelectedUpgradeIndex
		{
			get
			{
				return this.selectedUpgradeIndex;
			}
			set
			{
				this.selectedUpgradeIndex = value;
			}
		}

		// Token: 0x06000709 RID: 1801 RVA: 0x0001681C File Offset: 0x00014A1C
		public int GetUpgradeIndex(PlayerUpgradeID upgradeID)
		{
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				if (this.upgrades[i].UpgradeID == upgradeID)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600070A RID: 1802 RVA: 0x0001684F File Offset: 0x00014A4F
		public bool HasUpgrade(PlayerUpgradeID upgradeID)
		{
			return this.GetUpgradeIndex(upgradeID) != -1;
		}

		// Token: 0x0600070B RID: 1803 RVA: 0x00016860 File Offset: 0x00014A60
		public bool TrySelectNextUpgrade()
		{
			int num = this.upgrades.Length;
			for (int i = 1; i < num; i++)
			{
				int num2 = (this.selectedUpgradeIndex + i) % num;
				if (this.upgrades[num2].IsAvailableAndUnlocked)
				{
					this.selectedUpgradeIndex = num2;
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600070C RID: 1804 RVA: 0x000168A8 File Offset: 0x00014AA8
		public void UpdateLockState(IReadOnlyList<string> reachedMilestones)
		{
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				for (int j = 0; j < reachedMilestones.Count; j++)
				{
					bool flag;
					this.upgrades[i].UpdateLockState(reachedMilestones[j], out flag);
					if (flag)
					{
						break;
					}
				}
			}
		}

		// Token: 0x0600070D RID: 1805 RVA: 0x000168F4 File Offset: 0x00014AF4
		public int GetTotalUpgradesCost()
		{
			int num = 0;
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				num += this.upgrades[i].GetSpentCurrencyAmount();
			}
			return num;
		}

		// Token: 0x0600070E RID: 1806 RVA: 0x00016928 File Offset: 0x00014B28
		public bool ResetUpgradesLevel()
		{
			bool flag = false;
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				flag |= this.upgrades[i].ResetLevel();
			}
			return flag;
		}

		// Token: 0x0600070F RID: 1807 RVA: 0x0001695C File Offset: 0x00014B5C
		public void Initialize([TupleElementNames(new string[]
		{
			"isAvailable",
			"upgradeLevel"
		})] Func<PurchasablePlayerUpgradesSequence, PurchasablePlayerUpgradeData, ValueTuple<bool, int>> upgradeInitializationFunc, int selectedUpgradeIndex = 0)
		{
			bool flag = false;
			for (int i = 0; i < this.upgrades.Length; i++)
			{
				PurchasablePlayerUpgradeData purchasablePlayerUpgradeData = this.upgrades[i];
				ValueTuple<bool, int> valueTuple = upgradeInitializationFunc(this, purchasablePlayerUpgradeData);
				bool item = valueTuple.Item1;
				int item2 = valueTuple.Item2;
				purchasablePlayerUpgradeData.Initialize(item, item2);
				flag = (flag || item);
			}
			if (flag)
			{
				this.SelectedUpgradeIndex = selectedUpgradeIndex;
				this.IsAvailable = (selectedUpgradeIndex >= 0);
			}
			else
			{
				this.SelectedUpgradeIndex = -1;
				this.IsAvailable = false;
			}
			this.HasAvailableUpgrades = flag;
		}

		// Token: 0x04000430 RID: 1072
		[SerializeField]
		private PurchasablePlayerUpgradeData[] upgrades;

		// Token: 0x04000431 RID: 1073
		private int selectedUpgradeIndex;
	}
}
