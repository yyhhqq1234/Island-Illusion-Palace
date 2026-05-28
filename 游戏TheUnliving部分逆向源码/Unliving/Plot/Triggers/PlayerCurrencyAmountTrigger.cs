using System;
using System.Text;
using Unliving.Currencies;
using Unliving.PlayerProfileManagement;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F3 RID: 755
	[Serializable]
	public sealed class PlayerCurrencyAmountTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019D3 RID: 6611 RVA: 0x00050DB9 File Offset: 0x0004EFB9
		protected override bool ShouldBeIgnored()
		{
			return this.requiredGoldAmount <= 0 && this.requiredMetaCurrencyAmount <= 0;
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x00050DD2 File Offset: 0x0004EFD2
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}(gold: {1}, currency: {2})", base.GetType().Name, this.requiredGoldAmount, this.requiredMetaCurrencyAmount);
		}

		// Token: 0x060019D5 RID: 6613 RVA: 0x00050E00 File Offset: 0x0004F000
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerProfileManager playerProfileManager;
			if (context.currentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				return currentPlayerProfile.GetCurrencyAmount(CurrencyID.Gold) >= this.requiredGoldAmount && currentPlayerProfile.GetCurrencyAmount(CurrencyID.Meta) >= this.requiredMetaCurrencyAmount;
			}
			return false;
		}

		// Token: 0x04000E6A RID: 3690
		public int requiredGoldAmount;

		// Token: 0x04000E6B RID: 3691
		public int requiredMetaCurrencyAmount;
	}
}
