using System;
using Common.Tiles;
using Common.UnityExtensions;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C6 RID: 198
	[Name("Set Tilemap Animation State", 0)]
	[Category("Unliving/Tiles")]
	public sealed class SetTilemapAnimationStateNode : GameContextDependentNodeBase
	{
		// Token: 0x060004F3 RID: 1267 RVA: 0x000120AC File Offset: 0x000102AC
		private void SetAnimationState(Flow flow)
		{
			if (this.tilemap.value.IsNull())
			{
				Debug.LogError("tilemap is empty!");
				return;
			}
			this.tilemap.value.ActivateState(this.animationState.value);
			this.flowOut.Call(flow);
		}

		// Token: 0x060004F4 RID: 1268 RVA: 0x00012100 File Offset: 0x00010300
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SetAnimationState), "");
			this.animationState = base.AddValueInput<string>("animationState", "");
			this.tilemap = base.AddValueInput<MultiAnimatedTilemap>("tilemap", "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x04000362 RID: 866
		private ValueInput<MultiAnimatedTilemap> tilemap;

		// Token: 0x04000363 RID: 867
		private ValueInput<string> animationState;

		// Token: 0x04000364 RID: 868
		private FlowOutput flowOut;
	}
}
