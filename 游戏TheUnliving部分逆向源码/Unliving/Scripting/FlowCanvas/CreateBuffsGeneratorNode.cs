using System;
using Common;
using FlowCanvas.Nodes;
using Game.Buffs;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200008B RID: 139
	[Name("Create Buffs Generator", 0)]
	[Category("Unliving/Buffs")]
	public sealed class CreateBuffsGeneratorNode : CallableFunctionNode<IBuffsGenerator>
	{
		// Token: 0x060003D2 RID: 978 RVA: 0x0000D3D8 File Offset: 0x0000B5D8
		public override IBuffsGenerator Invoke()
		{
			if (this.buffsGenerator == null)
			{
				if (this.buffsGeneratorBuilder.value == null)
				{
					Debug.LogError(this.name + " doesn't have Buffs Generator Builder.");
					return null;
				}
				this.buffsGenerator = this.buffsGeneratorBuilder.value.InstantiateBuffsGenerator(this.buffsOverrides.value);
				IInitializable initializable = this.buffsGenerator as IInitializable;
				if (initializable != null)
				{
					initializable.Initialize();
				}
			}
			return this.buffsGenerator;
		}

		// Token: 0x04000254 RID: 596
		public BBParameter<BuffsGeneratorBuilderAsset> buffsGeneratorBuilder;

		// Token: 0x04000255 RID: 597
		public BBParameter<BuffsGeneratorBase.BuffsOverrides> buffsOverrides;

		// Token: 0x04000256 RID: 598
		private IBuffsGenerator buffsGenerator;
	}
}
