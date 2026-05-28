using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Buffs;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000098 RID: 152
	[Name("Get Buff", 0)]
	[Category("Unliving/Buffs")]
	public sealed class GetBuffNode : FlowControlNode
	{
		// Token: 0x0600040A RID: 1034 RVA: 0x0000E1AF File Offset: 0x0000C3AF
		private void UpdateOutValue(Flow flow, IBuff buff)
		{
			this.buff = buff;
			this.flowOut.Call(flow);
		}

		// Token: 0x0600040B RID: 1035 RVA: 0x0000E1C4 File Offset: 0x0000C3C4
		private void GetBuff(Flow flow)
		{
			BaseGameMob value = this.buffableMob.value;
			GameObject value2 = this.buffableObject.value;
			IBuffsController buffsController = null;
			IBuffableObject buffableObject;
			if (value != null)
			{
				buffsController = value.BuffsController;
			}
			else if (value2 != null && value2.TryGetComponent<IBuffableObject>(out buffableObject))
			{
				buffsController = buffableObject.BuffsController;
			}
			if (buffsController != null)
			{
				BuffsGeneratorBuilderAsset value3 = this.buffGenerator.value;
				object value4 = this.buffSender.value;
				bool value5 = this.getFirstBuffOnly.value;
				foreach (IBuff buff in buffsController.GetCurrentBuffs())
				{
					if ((!(value3 != null) || value3.IsRelatedBuff(buff)) && (value4 == null || buff.Sender == value4))
					{
						this.UpdateOutValue(flow, buff);
						if (value5)
						{
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600040C RID: 1036 RVA: 0x0000E2B8 File Offset: 0x0000C4B8
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.GetBuff), "");
			this.buffableObject = base.AddValueInput<GameObject>("buffableObject", "");
			this.buffableMob = base.AddValueInput<BaseGameMob>("buffableMob", "");
			this.buffGenerator = base.AddValueInput<BuffsGeneratorBuilderAsset>("buffGenerator", "");
			this.buffSender = base.AddValueInput<object>("buffSender", "");
			this.getFirstBuffOnly = base.AddValueInput<bool>("getFirstBuffOnly", "");
			this.getFirstBuffOnly.SetDefaultAndSerializedValue(true);
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<IBuff>("Buff", () => this.buff, "");
		}

		// Token: 0x04000283 RID: 643
		private ValueInput<GameObject> buffableObject;

		// Token: 0x04000284 RID: 644
		private ValueInput<BaseGameMob> buffableMob;

		// Token: 0x04000285 RID: 645
		private ValueInput<BuffsGeneratorBuilderAsset> buffGenerator;

		// Token: 0x04000286 RID: 646
		private ValueInput<object> buffSender;

		// Token: 0x04000287 RID: 647
		private ValueInput<bool> getFirstBuffOnly;

		// Token: 0x04000288 RID: 648
		private FlowOutput flowOut;

		// Token: 0x04000289 RID: 649
		private IBuff buff;
	}
}
