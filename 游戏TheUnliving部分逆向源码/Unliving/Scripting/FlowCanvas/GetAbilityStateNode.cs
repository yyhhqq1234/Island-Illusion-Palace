using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000097 RID: 151
	[Name("Get Ability State", 0)]
	[Category("Unliving/Abilities")]
	public sealed class GetAbilityStateNode : FlowControlNode
	{
		// Token: 0x06000401 RID: 1025 RVA: 0x0000DFEC File Offset: 0x0000C1EC
		private bool IsReloading()
		{
			return this.targetAbility.value.IsReloading();
		}

		// Token: 0x06000402 RID: 1026 RVA: 0x0000DFFE File Offset: 0x0000C1FE
		private bool IsPrepInProgress()
		{
			return this.targetAbility.value.IsPrepInProgress();
		}

		// Token: 0x06000403 RID: 1027 RVA: 0x0000E010 File Offset: 0x0000C210
		private bool IsActivated()
		{
			return this.targetAbility.value.IsActivated;
		}

		// Token: 0x06000404 RID: 1028 RVA: 0x0000E022 File Offset: 0x0000C222
		private bool InUse()
		{
			return this.targetAbility.value.IsBusy();
		}

		// Token: 0x06000405 RID: 1029 RVA: 0x0000E034 File Offset: 0x0000C234
		private bool IsReadyToUse()
		{
			return !this.IsReloading() && !this.IsPrepInProgress() && !this.IsActivated();
		}

		// Token: 0x06000406 RID: 1030 RVA: 0x0000E051 File Offset: 0x0000C251
		private float GetReloadingProgress()
		{
			return this.targetAbility.value.ReloadingProgress;
		}

		// Token: 0x06000407 RID: 1031 RVA: 0x0000E063 File Offset: 0x0000C263
		private float GetPrepProgress()
		{
			return this.targetAbility.value.PrepProgress;
		}

		// Token: 0x06000408 RID: 1032 RVA: 0x0000E078 File Offset: 0x0000C278
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", delegate(Flow <p0>)
			{
			}, "");
			this.targetAbility = base.AddValueInput<BaseAbility>("targetAbility", "");
			base.AddFlowOutput("", "");
			base.AddValueOutput<bool>("IsReloading", new ValueHandler<bool>(this.IsReloading), "");
			base.AddValueOutput<bool>("IsPrepInProgress", new ValueHandler<bool>(this.IsPrepInProgress), "");
			base.AddValueOutput<bool>("IsActivated", new ValueHandler<bool>(this.IsActivated), "");
			base.AddValueOutput<bool>("InUse", new ValueHandler<bool>(this.InUse), "");
			base.AddValueOutput<bool>("IsReadyToUse", new ValueHandler<bool>(this.IsReadyToUse), "");
			base.AddValueOutput<float>("ReloadingProgress", new ValueHandler<float>(this.GetReloadingProgress), "");
			base.AddValueOutput<float>("PrepProgress", new ValueHandler<float>(this.GetPrepProgress), "");
		}

		// Token: 0x04000282 RID: 642
		private ValueInput<BaseAbility> targetAbility;
	}
}
