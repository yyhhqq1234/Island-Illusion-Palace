using System;
using FlowCanvas;
using ParadoxNotion.Design;
using Unliving.LevelGeneration;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000093 RID: 147
	[Name("Fake Win Location", 0)]
	[Category("Unliving/Test")]
	public class FakeWinLocationNode : GameContextDependentNodeBase
	{
		// Token: 0x060003EE RID: 1006 RVA: 0x0000DA29 File Offset: 0x0000BC29
		private void FakeWin(Flow flow)
		{
			this.flowOut.Call(flow);
		}

		// Token: 0x060003EF RID: 1007 RVA: 0x0000DA38 File Offset: 0x0000BC38
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.FakeWin), "");
			this.locationType = base.AddValueInput<GameLocation.TypeID>("locationType", "");
			this.locationType.SetDefaultAndSerializedValue(GameLocation.TypeID.Cemetery);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400026D RID: 621
		private ValueInput<GameLocation.TypeID> locationType;

		// Token: 0x0400026E RID: 622
		private FlowOutput flowOut;
	}
}
