using System;
using System.Collections;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.LeveledItems;
using Unliving.PlayerProfileManagement;
using Unliving.Plot.Milestones;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016D RID: 365
	[CreateAssetMenu(fileName = "RerollCurrencyPlayerUpgrade", menuName = "Game/Player/Player Upgrade/Reroll Currency Player Upgrade")]
	public class RerollCurrencyPlayerUpgrade : ScriptableObject, IPlayerUpgrade, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06000A20 RID: 2592 RVA: 0x00021FB5 File Offset: 0x000201B5
		// (set) Token: 0x06000A21 RID: 2593 RVA: 0x00021FBD File Offset: 0x000201BD
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06000A22 RID: 2594 RVA: 0x00021FC6 File Offset: 0x000201C6
		// (set) Token: 0x06000A23 RID: 2595 RVA: 0x00021FC9 File Offset: 0x000201C9
		public int ItemLevel
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06000A24 RID: 2596 RVA: 0x00021FCB File Offset: 0x000201CB
		// (set) Token: 0x06000A25 RID: 2597 RVA: 0x00021FD3 File Offset: 0x000201D3
		public bool IsActivated { get; private set; }

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06000A26 RID: 2598 RVA: 0x00021FDC File Offset: 0x000201DC
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return 0;
			}
		}

		// Token: 0x06000A27 RID: 2599 RVA: 0x00021FDF File Offset: 0x000201DF
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			this.IsActivated = true;
			this.currentGame = (activationContext as IGame);
			IPlotMilestoneManager plotMilestoneManager;
			if (this.currentGame.Services.TryGet<IPlotMilestoneManager>(out plotMilestoneManager))
			{
				this.rerollCurrencyArgs.amount = 0f;
				for (int i = 0; i < this.rerollsData.Length; i++)
				{
					RerollCurrencyPlayerUpgrade.RerollData rerollData = this.rerollsData[i];
					if (plotMilestoneManager.IsMilestoneReached(rerollData.milestoneID))
					{
						this.rerollCurrencyArgs.amount = this.rerollCurrencyArgs.amount + (float)rerollData.rerollsCount;
					}
				}
				PlayerProfileManager playerProfileManager;
				if (this.currentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
				{
					PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
					if (currentPlayerProfile != null)
					{
						currentPlayerProfile.TryExecuteCurrencyOperation(this.rerollCurrencyArgs);
					}
				}
			}
			yield break;
		}

		// Token: 0x06000A28 RID: 2600 RVA: 0x00021FF5 File Offset: 0x000201F5
		public IPlayerUpgrade Clone()
		{
			RerollCurrencyPlayerUpgrade rerollCurrencyPlayerUpgrade = (RerollCurrencyPlayerUpgrade)base.MemberwiseClone();
			rerollCurrencyPlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			return rerollCurrencyPlayerUpgrade;
		}

		// Token: 0x040005FB RID: 1531
		public RerollCurrencyPlayerUpgrade.RerollData[] rerollsData;

		// Token: 0x040005FC RID: 1532
		private IGame currentGame;

		// Token: 0x040005FD RID: 1533
		private CurrencyOperationArgs rerollCurrencyArgs = new CurrencyOperationArgs
		{
			currencyID = CurrencyID.RerollCurrency
		};

		// Token: 0x0200046E RID: 1134
		[Serializable]
		public struct RerollData
		{
			// Token: 0x04001752 RID: 5970
			public string milestoneID;

			// Token: 0x04001753 RID: 5971
			public int rerollsCount;
		}
	}
}
