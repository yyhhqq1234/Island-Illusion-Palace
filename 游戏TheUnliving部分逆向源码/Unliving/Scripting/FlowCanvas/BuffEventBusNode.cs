using System;
using FlowCanvas;
using Game.Buffs;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008A RID: 138
	[Name("Buff Events", 0)]
	[Category("Unliving/Buffs")]
	public sealed class BuffEventBusNode : ObjectEventBusNodeBase
	{
		// Token: 0x060003CB RID: 971 RVA: 0x0000D241 File Offset: 0x0000B441
		protected override GameObject GetEventsSourceObject()
		{
			return null;
		}

		// Token: 0x060003CC RID: 972 RVA: 0x0000D244 File Offset: 0x0000B444
		private void OnBuffCompleted(IBuff buff)
		{
			this.completed.Call(default(Flow));
			buff.Completed -= this.OnBuffCompleted;
			this.currentBuff = null;
		}

		// Token: 0x060003CD RID: 973 RVA: 0x0000D280 File Offset: 0x0000B480
		private void OnBuffActivated(IBuff buff)
		{
			this.activated.Call(default(Flow));
			buff.Activated -= this.OnBuffCompleted;
		}

		// Token: 0x060003CE RID: 974 RVA: 0x0000D2B4 File Offset: 0x0000B4B4
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.targetBuff = base.AddValueInput<IBuff>("targetBuff", "");
			this.activated = base.AddFlowOutput("activated", "");
			this.completed = base.AddFlowOutput("completed", "");
		}

		// Token: 0x060003CF RID: 975 RVA: 0x0000D30C File Offset: 0x0000B50C
		protected override void OnInitialize(Flow flow)
		{
			this.currentBuff = this.targetBuff.value;
			if (this.currentBuff == null)
			{
				return;
			}
			if (this.currentBuff.IsActivated)
			{
				this.OnBuffActivated(this.currentBuff);
			}
			else
			{
				this.currentBuff.Activated += this.OnBuffActivated;
			}
			if (this.currentBuff.IsCompleted)
			{
				this.OnBuffCompleted(this.currentBuff);
				return;
			}
			this.currentBuff.Completed += this.OnBuffCompleted;
		}

		// Token: 0x060003D0 RID: 976 RVA: 0x0000D396 File Offset: 0x0000B596
		protected override void OnFinalize()
		{
			if (this.currentBuff == null)
			{
				return;
			}
			this.currentBuff.Activated -= this.OnBuffActivated;
			this.currentBuff.Completed -= this.OnBuffCompleted;
		}

		// Token: 0x04000250 RID: 592
		private ValueInput<IBuff> targetBuff;

		// Token: 0x04000251 RID: 593
		private FlowOutput activated;

		// Token: 0x04000252 RID: 594
		private FlowOutput completed;

		// Token: 0x04000253 RID: 595
		private IBuff currentBuff;
	}
}
