using System;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Currencies;
using Unliving.PlayerProfileManagement;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008E RID: 142
	[Name("Change Player Money", 0)]
	[Category("Unliving/Player")]
	public sealed class ChangeMoneyNode : GameContextDependentNodeBase
	{
		// Token: 0x060003D9 RID: 985 RVA: 0x0000D51C File Offset: 0x0000B71C
		private void ModifyPlayerCurrency(Flow flow)
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				if (currentPlayerProfile != null)
				{
					CurrencyOperationArgs currencyOperationArgs = default(CurrencyOperationArgs);
					currencyOperationArgs.currencyID = this.currencyID.value;
					currencyOperationArgs.amount = (float)this.amount.value;
					currencyOperationArgs.spending = this.spending.value;
					Component graphAgent = base.graphAgent;
					currencyOperationArgs.sender = ((graphAgent != null) ? graphAgent.gameObject : null);
					this.args = currencyOperationArgs;
					currentPlayerProfile.TryExecuteCurrencyOperation(this.args);
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060003DA RID: 986 RVA: 0x0000D5C4 File Offset: 0x0000B7C4
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ModifyPlayerCurrency), "");
			this.currencyID = base.AddValueInput<CurrencyID>("currencyID", "");
			this.currencyID.SetDefaultAndSerializedValue(CurrencyID.Meta);
			this.amount = base.AddValueInput<int>("amount", "");
			this.amount.SetDefaultAndSerializedValue(0);
			this.spending = base.AddValueInput<bool>("spending", "");
			this.spending.SetDefaultAndSerializedValue(false);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000259 RID: 601
		private ValueInput<CurrencyID> currencyID;

		// Token: 0x0400025A RID: 602
		private ValueInput<int> amount;

		// Token: 0x0400025B RID: 603
		private ValueInput<bool> spending;

		// Token: 0x0400025C RID: 604
		private FlowOutput flowOut;

		// Token: 0x0400025D RID: 605
		private CurrencyOperationArgs args;
	}
}
