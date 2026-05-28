using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Damage;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B2 RID: 178
	[Name("Modify HP Resistance", 0)]
	[Category("Unliving/Mobs")]
	public sealed class ModifyHitPointsResistanceNode : FlowControlNode
	{
		// Token: 0x06000487 RID: 1159 RVA: 0x0000FF7C File Offset: 0x0000E17C
		private void ModifyResistance(Flow flow)
		{
			IResistableDamageable resistableDamageable;
			if (this.destructibleObject.value != null && this.destructibleObject.value.TryGetComponent<IResistableDamageable>(out resistableDamageable))
			{
				resistableDamageable.ModifyDamageResistance(this.damageResistance.value);
				resistableDamageable.ModifyHealingResistance(this.healingResistance.value);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000488 RID: 1160 RVA: 0x0000FFE0 File Offset: 0x0000E1E0
		protected override void RegisterPorts()
		{
			this.destructibleObject = base.AddValueInput<GameObject>("destructibleObject", "");
			this.damageResistance = base.AddValueInput<float>("damageResistance", "");
			this.healingResistance = base.AddValueInput<float>("healingResistance", "");
			base.AddFlowInput("", new FlowHandler(this.ModifyResistance), "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x040002EB RID: 747
		private ValueInput<GameObject> destructibleObject;

		// Token: 0x040002EC RID: 748
		private ValueInput<float> damageResistance;

		// Token: 0x040002ED RID: 749
		private ValueInput<float> healingResistance;

		// Token: 0x040002EE RID: 750
		private FlowOutput flowOut;
	}
}
