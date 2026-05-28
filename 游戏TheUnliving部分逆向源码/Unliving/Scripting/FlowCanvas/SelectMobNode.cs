using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C2 RID: 194
	[Name("Select Mob", 0)]
	[Category("Unliving/Mobs")]
	public sealed class SelectMobNode : FlowControlNode
	{
		// Token: 0x060004DF RID: 1247 RVA: 0x000119F2 File Offset: 0x0000FBF2
		private bool IsSelectableMob(BaseGameMob mob)
		{
			return mob != this.ignorableMob.value;
		}

		// Token: 0x060004E0 RID: 1248 RVA: 0x00011A08 File Offset: 0x0000FC08
		private void SelectMob(Flow flow)
		{
			this.selectedMob = null;
			this.mobWasSelected = false;
			if (this.mobs.value == null || this.mobs.value.Count == 0)
			{
				return;
			}
			if (this.mobSelector == null)
			{
				this.mobSelector = new GameMobTargetSelector
				{
					skipTargetInRangeCheck = true,
					AdditionalTargetValidator = new Predicate<BaseGameMob>(this.IsSelectableMob)
				};
			}
			this.mobSelector.targetSelectionPoint = new Vector2?(this.useCustomSearchPosition.value ? this.customSearchPosition.value : base.graphAgent.transform.position);
			this.mobSelector.targetSelectionRadius = this.searchRadius.value;
			this.mobSelector.SetTargetsEstimationParams(this.mainSelectionMethod.value, this.additionalSelectionMethod.value);
			this.selectedMob = this.mobSelector.FindNewTarget(this.mobs.value, -1);
			this.mobWasSelected = (this.selectedMob != null);
			this.flowOut.Call(flow);
		}

		// Token: 0x060004E1 RID: 1249 RVA: 0x00011B20 File Offset: 0x0000FD20
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SelectMob), "");
			this.searchRadius = base.AddValueInput<float>("searchRadius", "");
			this.customSearchPosition = base.AddValueInput<Vector3>("customSearchPosition", "");
			this.useCustomSearchPosition = base.AddValueInput<bool>("useCustomSearchPosition", "");
			this.mobs = base.AddValueInput<IReadOnlyList<BaseGameMob>>("mobs", "");
			this.ignorableMob = base.AddValueInput<BaseGameMob>("ignorableMob", "");
			this.ignorableMob.skipSelfInstanceAssignment = true;
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<bool>("mobWasSelected", () => this.mobWasSelected, "");
			base.AddValueOutput<BaseGameMob>("selectedMob", () => this.selectedMob, "");
		}

		// Token: 0x04000347 RID: 839
		public BBParameter<GameMobTargetSelector.SelectionMethod> mainSelectionMethod;

		// Token: 0x04000348 RID: 840
		public BBParameter<GameMobTargetSelector.PrioritySelector> additionalSelectionMethod;

		// Token: 0x04000349 RID: 841
		private ValueInput<float> searchRadius;

		// Token: 0x0400034A RID: 842
		private ValueInput<Vector3> customSearchPosition;

		// Token: 0x0400034B RID: 843
		private ValueInput<bool> useCustomSearchPosition;

		// Token: 0x0400034C RID: 844
		private ValueInput<IReadOnlyList<BaseGameMob>> mobs;

		// Token: 0x0400034D RID: 845
		private ValueInput<BaseGameMob> ignorableMob;

		// Token: 0x0400034E RID: 846
		private FlowOutput flowOut;

		// Token: 0x0400034F RID: 847
		private GameMobTargetSelector mobSelector;

		// Token: 0x04000350 RID: 848
		private BaseGameMob selectedMob;

		// Token: 0x04000351 RID: 849
		private bool mobWasSelected;
	}
}
