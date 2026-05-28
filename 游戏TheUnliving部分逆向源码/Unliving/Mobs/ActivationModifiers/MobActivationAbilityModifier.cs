using System;
using Common.Editor;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Abilities.Modifiers;
using Unliving.LeveledItems;

namespace Unliving.Mobs.ActivationModifiers
{
	// Token: 0x02000220 RID: 544
	[CreateAssetMenu(fileName = "MobActivationAbilityModifier", menuName = "Abilities/Mob Activation Ability Modifier")]
	public sealed class MobActivationAbilityModifier : AbilityModifiersController, IAbilityLevelingControllerDependent
	{
		// Token: 0x170003F0 RID: 1008
		// (get) Token: 0x060012A2 RID: 4770 RVA: 0x0003B2D6 File Offset: 0x000394D6
		// (set) Token: 0x060012A3 RID: 4771 RVA: 0x0003B2DE File Offset: 0x000394DE
		public MobActivationModifierID ModifierID { get; set; }

		// Token: 0x060012A4 RID: 4772 RVA: 0x0003B2E8 File Offset: 0x000394E8
		protected override bool CanBeAdded(BaseAbility targetAbility)
		{
			ITypedMobActivationAbility typedMobActivationAbility = targetAbility as ITypedMobActivationAbility;
			return typedMobActivationAbility != null && this.CanBeUsedAsMobActivationModifier(typedMobActivationAbility.ActivationAbilityType);
		}

		// Token: 0x060012A5 RID: 4773 RVA: 0x0003B30D File Offset: 0x0003950D
		public bool CanBeUsedAsMobActivationModifier(MobActivationAbilityType mobActivationTypes)
		{
			return this.allowedActivationAbilityTypes == MobActivationAbilityType.None || (this.allowedActivationAbilityTypes & mobActivationTypes) > MobActivationAbilityType.None;
		}

		// Token: 0x060012A6 RID: 4774 RVA: 0x0003B324 File Offset: 0x00039524
		IAbilityLevelingController IAbilityLevelingControllerDependent.GetLevelingController()
		{
			return this.modifierLevelingController;
		}

		// Token: 0x04000AC7 RID: 2759
		[EnumFlags]
		[Space(5f)]
		public MobActivationAbilityType allowedActivationAbilityTypes;

		// Token: 0x04000AC8 RID: 2760
		public AbilityLevelBasedPropertiesModifier modifierLevelingController;
	}
}
