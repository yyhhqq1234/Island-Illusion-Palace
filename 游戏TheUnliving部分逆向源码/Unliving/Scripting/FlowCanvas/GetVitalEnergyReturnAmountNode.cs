using System;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A0 RID: 160
	[Name("Get Vital Energy Return Amount", 0)]
	[Category("Unliving/Mobs")]
	public sealed class GetVitalEnergyReturnAmountNode : PureFunctionNode<float, BaseGameMob>
	{
		// Token: 0x06000436 RID: 1078 RVA: 0x0000ECF4 File Offset: 0x0000CEF4
		public override float Invoke(BaseGameMob mob)
		{
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour == null)
			{
				return 0f;
			}
			return mobBehaviour.activationEnergyReturnAmount;
		}
	}
}
