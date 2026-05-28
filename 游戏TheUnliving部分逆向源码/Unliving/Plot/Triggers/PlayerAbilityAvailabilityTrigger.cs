using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Game.Abilities;
using Unliving.Abilities;
using Unliving.Player;

namespace Unliving.Plot.Triggers
{
	// Token: 0x020002F2 RID: 754
	[Serializable]
	public sealed class PlayerAbilityAvailabilityTrigger : CharacterPlotItemTriggerBase
	{
		// Token: 0x060019CE RID: 6606 RVA: 0x00050C22 File Offset: 0x0004EE22
		protected override bool ShouldBeIgnored()
		{
			return this.requiredAbility == null && this.requiredAbilityExtender == null;
		}

		// Token: 0x060019CF RID: 6607 RVA: 0x00050C40 File Offset: 0x0004EE40
		protected override string ToFormattedString(StringBuilder stringBuilder)
		{
			return string.Concat(new string[]
			{
				base.GetType().Name,
				"(ability: ",
				CharacterPlotItemTriggerBase.ObjArgToString(this.requiredAbility, null),
				", extender: ",
				CharacterPlotItemTriggerBase.ObjArgToString(this.requiredAbilityExtender, null),
				")"
			});
		}

		// Token: 0x060019D0 RID: 6608 RVA: 0x00050C9C File Offset: 0x0004EE9C
		protected override bool GetState(CharacterPlotContext context)
		{
			PlayerBehaviour playerBehaviour;
			if (context.currentGame.TryGetPlayer(out playerBehaviour))
			{
				GameAbilitiesController abilitiesController = playerBehaviour.AbilitiesController;
				IReadOnlyList<BaseAbility> readOnlyList = (abilitiesController != null) ? abilitiesController.Abilities : null;
				if (readOnlyList != null)
				{
					BaseAbility baseAbility = this.requiredAbility;
					Type extenderType = (baseAbility != null) ? baseAbility.GetType() : null;
					bool flag = false;
					bool flag2 = false;
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						BaseAbility baseAbility2 = readOnlyList[i];
						if (this.requiredAbility != null)
						{
							if (flag = BaseAbility.AreEqualOrHaveSamePrototype(baseAbility2, this.requiredAbility))
							{
								flag2 = PlayerAbilityAvailabilityTrigger.<GetState>g__HasBehaviourExtender|4_0(baseAbility2, extenderType);
								break;
							}
						}
						else if (!flag2)
						{
							flag2 = PlayerAbilityAvailabilityTrigger.<GetState>g__HasBehaviourExtender|4_0(baseAbility2, extenderType);
						}
					}
					return (flag || this.requiredAbility == null) && (flag2 || this.requiredAbilityExtender == null);
				}
			}
			return false;
		}

		// Token: 0x060019D2 RID: 6610 RVA: 0x00050D74 File Offset: 0x0004EF74
		[CompilerGenerated]
		internal static bool <GetState>g__HasBehaviourExtender|4_0(BaseAbility ability, Type extenderType)
		{
			if (extenderType != null)
			{
				IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
				for (int i = 0; i < extensions.Count; i++)
				{
					if (extensions[i].GetType() == extenderType)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x04000E68 RID: 3688
		public BaseAbility requiredAbility;

		// Token: 0x04000E69 RID: 3689
		public AbilityExtensionAssetBase requiredAbilityExtender;
	}
}
