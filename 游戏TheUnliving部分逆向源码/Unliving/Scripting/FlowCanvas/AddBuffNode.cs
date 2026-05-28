using System;
using Common;
using FlowCanvas.Nodes;
using Game.Buffs;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000086 RID: 134
	[Name("Add Buff", 0)]
	[Category("Unliving/Buffs")]
	public sealed class AddBuffNode : CallableFunctionNode<IBuff, BaseGameMob, UnityEngine.Object, IBuffsGenerator, float, bool>
	{
		// Token: 0x060003BC RID: 956 RVA: 0x0000CD60 File Offset: 0x0000AF60
		public override IBuff Invoke(BaseGameMob targetMob, UnityEngine.Object buffSender, IBuffsGenerator externalBuffsGenerator, float durationOverride = 0f, bool addConstantBuff = false)
		{
			IBuffsController buffsController;
			if (targetMob != null && ScriptingUtility.TryGetBuffsController(targetMob, out buffsController))
			{
				if (this.buffsGenerator == null)
				{
					if (externalBuffsGenerator != null)
					{
						this.buffsGenerator = externalBuffsGenerator;
					}
					else
					{
						this.buffsGenerator = this.buffsGeneratorBuilder.value.InstantiateBuffsGenerator(this.buffsOverrides.value);
						IInitializable initializable = this.buffsGenerator as IInitializable;
						if (initializable != null)
						{
							initializable.Initialize();
						}
					}
					if (this.buffsGenerator == null)
					{
						Debug.LogError(string.Format("{0} ({1}) doesn't have Buff Generator.", this.name, buffSender));
						return null;
					}
				}
				if (durationOverride > 0f)
				{
					if (this.buffsGenerator.BuffParametersOverrides != null)
					{
						this.buffsGenerator.BuffParametersOverrides.DurationOverride = durationOverride;
					}
					else
					{
						this.buffsGenerator.BuffDuration = durationOverride;
					}
				}
				IBuff buff = this.buffsGenerator.GenerateBuff((buffSender != null) ? buffSender : this, addConstantBuff);
				if (buffsController.AddBuff(buff))
				{
					return buff;
				}
			}
			return null;
		}

		// Token: 0x04000240 RID: 576
		public BBParameter<BuffsGeneratorBuilderAsset> buffsGeneratorBuilder;

		// Token: 0x04000241 RID: 577
		public BBParameter<BuffsGeneratorBase.BuffsOverrides> buffsOverrides;

		// Token: 0x04000242 RID: 578
		private IBuffsGenerator buffsGenerator;
	}
}
