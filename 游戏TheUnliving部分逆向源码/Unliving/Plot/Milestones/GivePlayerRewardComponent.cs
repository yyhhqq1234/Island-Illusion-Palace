using System;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.PlayerProfileManagement;

namespace Unliving.Plot.Milestones
{
	// Token: 0x0200030C RID: 780
	public class GivePlayerRewardComponent : GameBehaviourBase
	{
		// Token: 0x06001A63 RID: 6755 RVA: 0x000526D0 File Offset: 0x000508D0
		public void Start()
		{
			if (this.activateOnStart)
			{
				this.GiveCurrency();
			}
		}

		// Token: 0x06001A64 RID: 6756 RVA: 0x000526E0 File Offset: 0x000508E0
		public void GiveCurrency()
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				if (currentPlayerProfile != null)
				{
					this.args = new CurrencyOperationArgs
					{
						currencyID = this.currencyID,
						amount = (float)this.amount,
						spending = this.isSpending,
						sender = base.gameObject
					};
					currentPlayerProfile.TryExecuteCurrencyOperation(this.args);
				}
			}
		}

		// Token: 0x04000EA7 RID: 3751
		[SerializeField]
		private bool activateOnStart = true;

		// Token: 0x04000EA8 RID: 3752
		[SerializeField]
		private CurrencyID currencyID;

		// Token: 0x04000EA9 RID: 3753
		[SerializeField]
		private int amount;

		// Token: 0x04000EAA RID: 3754
		[SerializeField]
		private bool isSpending;

		// Token: 0x04000EAB RID: 3755
		private CurrencyOperationArgs args;
	}
}
