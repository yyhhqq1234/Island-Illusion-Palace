using System;
using System.Text;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F5 RID: 757
	public abstract class PlotItemStateTriggerBase : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019DC RID: 6620
		protected abstract ICharacterPlotItemArgs GetTargetPlotItemArgs();

		// Token: 0x060019DD RID: 6621
		protected abstract ICharacterPlotProgress GetTargetCharacterPlotProgress(CharacterPlotContext context);

		// Token: 0x060019DE RID: 6622 RVA: 0x00050F09 File Offset: 0x0004F109
		protected override bool ShouldBeIgnored()
		{
			return this.GetTargetPlotItemArgs() == null;
		}

		// Token: 0x060019DF RID: 6623 RVA: 0x00050F14 File Offset: 0x0004F114
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Format("{0}({1}, {2})", base.GetType().Name, CharacterPlotItemTriggerBase.ObjArgToString(this.GetTargetPlotItemArgs(), null), this.requiredState);
		}

		// Token: 0x060019E0 RID: 6624 RVA: 0x00050F44 File Offset: 0x0004F144
		protected override bool GetState(CharacterPlotContext context)
		{
			PlotItemStateTriggerBase.RequiredState requiredState = this.requiredState;
			if (requiredState == PlotItemStateTriggerBase.RequiredState.Completed)
			{
				ICharacterPlotProgress targetCharacterPlotProgress = this.GetTargetCharacterPlotProgress(context);
				return targetCharacterPlotProgress != null && targetCharacterPlotProgress.IsCompletedPlotItem(this.GetTargetPlotItemArgs());
			}
			if (requiredState != PlotItemStateTriggerBase.RequiredState.InProgress)
			{
				return false;
			}
			ICharacterPlotProgress targetCharacterPlotProgress2 = this.GetTargetCharacterPlotProgress(context);
			return targetCharacterPlotProgress2 != null && targetCharacterPlotProgress2.IsPlotItemInProgress(this.GetTargetPlotItemArgs());
		}

		// Token: 0x04000E6D RID: 3693
		public PlotItemStateTriggerBase.RequiredState requiredState;

		// Token: 0x0200053F RID: 1343
		public enum RequiredState
		{
			// Token: 0x04001B8B RID: 7051
			Completed,
			// Token: 0x04001B8C RID: 7052
			InProgress
		}
	}
}
