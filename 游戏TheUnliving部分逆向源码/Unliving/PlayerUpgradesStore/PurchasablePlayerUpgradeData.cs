using System;
using UnityEngine;
using Unliving.Player.Upgrades;

namespace Unliving.PlayerUpgradesStore
{
	// Token: 0x0200011D RID: 285
	[Serializable]
	public sealed class PurchasablePlayerUpgradeData
	{
		// Token: 0x1700011D RID: 285
		// (get) Token: 0x060006EF RID: 1775 RVA: 0x000165E2 File Offset: 0x000147E2
		// (set) Token: 0x060006F0 RID: 1776 RVA: 0x000165EA File Offset: 0x000147EA
		public int MaxLevel { get; private set; }

		// Token: 0x1700011E RID: 286
		// (get) Token: 0x060006F1 RID: 1777 RVA: 0x000165F3 File Offset: 0x000147F3
		public bool IsAvailableAndUnlocked
		{
			get
			{
				return this.isUnlocked && this.isAvailable;
			}
		}

		// Token: 0x1700011F RID: 287
		// (get) Token: 0x060006F2 RID: 1778 RVA: 0x00016605 File Offset: 0x00014805
		public bool IsLocked
		{
			get
			{
				return !this.isUnlocked;
			}
		}

		// Token: 0x17000120 RID: 288
		// (get) Token: 0x060006F3 RID: 1779 RVA: 0x00016610 File Offset: 0x00014810
		public string UnlockingMilestone
		{
			get
			{
				return this.unlockingMilestone;
			}
		}

		// Token: 0x17000121 RID: 289
		// (get) Token: 0x060006F4 RID: 1780 RVA: 0x00016618 File Offset: 0x00014818
		public PlayerUpgradeID UpgradeID
		{
			get
			{
				return this.upgradeID;
			}
		}

		// Token: 0x17000122 RID: 290
		// (get) Token: 0x060006F5 RID: 1781 RVA: 0x00016620 File Offset: 0x00014820
		public int CurrentLevel
		{
			get
			{
				return this.currentLevel;
			}
		}

		// Token: 0x060006F6 RID: 1782 RVA: 0x00016628 File Offset: 0x00014828
		internal void Initialize(bool isAvailable, int currentLevel, int maxLevel)
		{
			this.isAvailable = isAvailable;
			this.MaxLevel = maxLevel;
			this.currentLevel = (int)Mathf.Clamp((float)currentLevel, 0f, (float)maxLevel);
		}

		// Token: 0x060006F7 RID: 1783 RVA: 0x0001664D File Offset: 0x0001484D
		internal void Initialize(bool isAvailable, int currentLevel)
		{
			this.Initialize(isAvailable, currentLevel, this.costPerLevel.Length);
		}

		// Token: 0x060006F8 RID: 1784 RVA: 0x0001665F File Offset: 0x0001485F
		internal void UpdateLockState(string reachedMilestone, out bool isUnlocked)
		{
			isUnlocked = (string.IsNullOrEmpty(this.unlockingMilestone) || this.unlockingMilestone.Equals(reachedMilestone, StringComparison.OrdinalIgnoreCase));
			this.isUnlocked = isUnlocked;
		}

		// Token: 0x060006F9 RID: 1785 RVA: 0x00016688 File Offset: 0x00014888
		public bool IsMaxLevelReached()
		{
			return this.currentLevel == this.MaxLevel;
		}

		// Token: 0x060006FA RID: 1786 RVA: 0x00016698 File Offset: 0x00014898
		public int? GetNextLevel()
		{
			if (this.IsMaxLevelReached())
			{
				return null;
			}
			return new int?(this.currentLevel + 1);
		}

		// Token: 0x060006FB RID: 1787 RVA: 0x000166C4 File Offset: 0x000148C4
		public int? GetNextLevelCost()
		{
			if (this.IsMaxLevelReached() || this.currentLevel >= this.costPerLevel.Length)
			{
				return null;
			}
			return new int?(this.costPerLevel[this.currentLevel]);
		}

		// Token: 0x060006FC RID: 1788 RVA: 0x00016705 File Offset: 0x00014905
		public void AdvanceLevel()
		{
			this.currentLevel++;
		}

		// Token: 0x060006FD RID: 1789 RVA: 0x00016715 File Offset: 0x00014915
		public bool ResetLevel()
		{
			if (this.currentLevel > 0)
			{
				this.currentLevel = 0;
				return true;
			}
			return false;
		}

		// Token: 0x060006FE RID: 1790 RVA: 0x0001672C File Offset: 0x0001492C
		public int GetSpentCurrencyAmount()
		{
			int num = 0;
			for (int i = 0; i < this.currentLevel; i++)
			{
				num += this.costPerLevel[i];
			}
			return num;
		}

		// Token: 0x060006FF RID: 1791 RVA: 0x00016758 File Offset: 0x00014958
		public bool TryGetPlayerUpgradeInfo(out PlayerUpgradeInfo upgradeInfo)
		{
			if (this.isAvailable && this.currentLevel > 0)
			{
				upgradeInfo = new PlayerUpgradeInfo
				{
					upgradeID = this.upgradeID,
					upgradeLevel = this.currentLevel
				};
				return true;
			}
			upgradeInfo = PlayerUpgradeInfo.None;
			return false;
		}

		// Token: 0x04000428 RID: 1064
		[SerializeField]
		private PlayerUpgradeID upgradeID;

		// Token: 0x04000429 RID: 1065
		[SerializeField]
		private string unlockingMilestone;

		// Token: 0x0400042A RID: 1066
		[SerializeField]
		private int[] costPerLevel;

		// Token: 0x0400042B RID: 1067
		private int currentLevel;

		// Token: 0x0400042C RID: 1068
		private bool isUnlocked;

		// Token: 0x0400042D RID: 1069
		private bool isAvailable;
	}
}
