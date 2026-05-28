using System;
using System.Collections.Generic;
using FlowCanvas;
using FlowCanvas.Nodes;
using Game.Abilities;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.MobsStats;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000081 RID: 129
	[Name("Get Ability Effect", 0)]
	[Category("Unliving/Abilities")]
	public sealed class GetAbilityEffectNode : FlowControlNode
	{
		// Token: 0x06000393 RID: 915 RVA: 0x0000C1D4 File Offset: 0x0000A3D4
		private void GetEffect(Flow flow)
		{
			Type value = this.effectType.value;
			this.currentEffect = null;
			if (value != null)
			{
				IAbilityEffectsListProvider value2 = this.effectsSource.value;
				IReadOnlyList<AbilityEffectBase> readOnlyList = (value2 != null) ? value2.AbilityEffects : null;
				if (readOnlyList != null)
				{
					MobStatID value3 = this.statEffectID.value;
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						if (value3 != MobStatID.Undefined)
						{
							StatModificationAbilityEffect statModificationAbilityEffect = readOnlyList[i] as StatModificationAbilityEffect;
							if (statModificationAbilityEffect != null && statModificationAbilityEffect.statModifier.targetStat == value3)
							{
								this.currentEffect = statModificationAbilityEffect;
								break;
							}
						}
						else
						{
							AbilityEffectBase abilityEffectBase = readOnlyList[i];
							if (((abilityEffectBase != null) ? abilityEffectBase.GetType() : null) == value)
							{
								this.currentEffect = readOnlyList[i];
								break;
							}
						}
					}
				}
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000394 RID: 916 RVA: 0x0000C2A0 File Offset: 0x0000A4A0
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.GetEffect), "");
			this.effectsSource = base.AddValueInput<IAbilityEffectsListProvider>("effectsSource", "");
			this.effectType = base.AddValueInput<Type>("effectType", "");
			this.statEffectID = base.AddValueInput<MobStatID>("statEffectID", "");
			this.statEffectID.SetDefaultAndSerializedValue(MobStatID.Undefined);
			base.AddValueOutput<AbilityEffectBase>("Effect", () => this.currentEffect, "");
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400021E RID: 542
		private ValueInput<IAbilityEffectsListProvider> effectsSource;

		// Token: 0x0400021F RID: 543
		private ValueInput<Type> effectType;

		// Token: 0x04000220 RID: 544
		private ValueInput<MobStatID> statEffectID;

		// Token: 0x04000221 RID: 545
		private FlowOutput flowOut;

		// Token: 0x04000222 RID: 546
		private AbilityEffectBase currentEffect;
	}
}
