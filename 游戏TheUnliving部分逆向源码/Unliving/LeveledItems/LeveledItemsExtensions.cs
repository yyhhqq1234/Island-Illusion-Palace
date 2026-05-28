using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Abilities;
using Unliving.Abilities;
using Unliving.AbilityResources;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Player.Upgrades;
using Unliving.Scripting;

namespace Unliving.LeveledItems
{
	// Token: 0x0200025A RID: 602
	public static class LeveledItemsExtensions
	{
		// Token: 0x06001402 RID: 5122 RVA: 0x0003EF50 File Offset: 0x0003D150
		public static bool TryGetItemLevel(this IItemLevelProvider item, out int itemLevel, int minLevel = 1)
		{
			if (item != null && item.ItemLevel >= minLevel)
			{
				itemLevel = item.ItemLevel;
				return true;
			}
			itemLevel = -1;
			return false;
		}

		// Token: 0x06001403 RID: 5123 RVA: 0x0003EF6C File Offset: 0x0003D16C
		public static bool TryGetAbilityLevel(this BaseAbility ability, out int abilityLevel, int minLevel = 1)
		{
			if (!ability.IsInstantiated())
			{
				abilityLevel = 1;
				return true;
			}
			return (ability as ILeveledItem).TryGetItemLevel(out abilityLevel, minLevel);
		}

		// Token: 0x06001404 RID: 5124 RVA: 0x0003EF88 File Offset: 0x0003D188
		public static bool TrySetAbilityLevel(this BaseAbility ability, int level)
		{
			if (level > 0)
			{
				ILeveledItem leveledItem = ability as ILeveledItem;
				if (leveledItem != null)
				{
					leveledItem.ItemLevel = level;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001405 RID: 5125 RVA: 0x0003EFB0 File Offset: 0x0003D1B0
		public static bool TryGetLevelingController(this object obj, out IAbilityLevelingController levelingController)
		{
			BaseAbility baseAbility = obj as BaseAbility;
			if (baseAbility != null)
			{
				return baseAbility.TryGetExtension(out levelingController);
			}
			IAbilityLevelingControllerDependent abilityLevelingControllerDependent = obj as IAbilityLevelingControllerDependent;
			levelingController = ((abilityLevelingControllerDependent != null) ? abilityLevelingControllerDependent.GetLevelingController() : null);
			return levelingController != null;
		}

		// Token: 0x06001406 RID: 5126 RVA: 0x0003EFE8 File Offset: 0x0003D1E8
		public static IReadOnlyList<AbilityPropertyValuesOverrides> GetAbilityPropertiesOverrides(this IGameAbilitiesFactory factory, AbilityID abilityID)
		{
			IAbilityPropertiesOverridesSource abilityPropertiesOverridesSource = factory.AbilityPropertiesOverridesSource;
			if (abilityPropertiesOverridesSource != null)
			{
				AbilityFactoryPrototype abilityPrototypeData = factory.GetAbilityPrototypeData((int)abilityID);
				BaseAbility baseAbility = (abilityPrototypeData != null) ? abilityPrototypeData.abilityPrototype : null;
				if (baseAbility != null)
				{
					return abilityPropertiesOverridesSource.GetAbilityPropertiesOverrides(baseAbility.name);
				}
			}
			return null;
		}

		// Token: 0x06001407 RID: 5127 RVA: 0x0003F02C File Offset: 0x0003D22C
		public static AbilityResourcesBlock GetAbilityRequiredResources(this IGameAbilitiesFactory factory, AbilityID abilityID, int abilityLevel)
		{
			AbilityFactoryPrototype abilityPrototypeData = factory.GetAbilityPrototypeData((int)abilityID);
			BaseAbility baseAbility = (abilityPrototypeData != null) ? abilityPrototypeData.abilityPrototype : null;
			AbilityResourcesBlock result;
			IAbilityResourcesConsumer abilityResourcesConsumer;
			if (baseAbility != null && baseAbility.TryGetActivationResources(out result, out abilityResourcesConsumer, true))
			{
				factory.GetAbilityPropertiesOverrides(abilityID).ApplyOverridesToAbilityResourcesBlock(ref result, abilityLevel);
				return result;
			}
			return default(AbilityResourcesBlock);
		}

		// Token: 0x06001408 RID: 5128 RVA: 0x0003F080 File Offset: 0x0003D280
		public static IReadOnlyList<AbilityPropertyValuesOverrides> GetActivationModifierPropertiesOverrides(this MobsActivationModifiersFactory factory, MobActivationModifierID modifierID)
		{
			IAbilityPropertiesOverridesSource abilityPropertiesOverridesSource = factory.AbilityPropertiesOverridesSource;
			if (abilityPropertiesOverridesSource != null)
			{
				MobActivationAbilityModifier modifierPrototype = factory.GetObjectPrototype((int)modifierID).modifierPrototype;
				if (modifierPrototype != null)
				{
					return abilityPropertiesOverridesSource.GetAbilityPropertiesOverrides(modifierPrototype.name);
				}
			}
			return null;
		}

		// Token: 0x06001409 RID: 5129 RVA: 0x0003F0BC File Offset: 0x0003D2BC
		public static AbilityResourcesBlock GetActivationModifierRequiredResources(this MobsActivationModifiersFactory factory, MobActivationModifierID modifierID, int modifierLevel)
		{
			MobActivationAbilityModifier modifierPrototype = factory.GetObjectPrototype((int)modifierID).modifierPrototype;
			AbilityResourcesBlock result;
			IAbilityResourcesConsumer abilityResourcesConsumer;
			if (modifierPrototype != null && modifierPrototype.TryGetActivationResources(out result, out abilityResourcesConsumer, true))
			{
				factory.GetActivationModifierPropertiesOverrides(modifierID).ApplyOverridesToAbilityResourcesBlock(ref result, modifierLevel);
				return result;
			}
			return default(AbilityResourcesBlock);
		}

		// Token: 0x0600140A RID: 5130 RVA: 0x0003F108 File Offset: 0x0003D308
		public static IReadOnlyList<ValueTuple<string, float>> GetPlayerUpgradeProperties(this IPlayerUpgradesFactory upgradesFactory, PlayerUpgradeID upgradeID, int upgradeLevel)
		{
			LeveledItemsExtensions.PropertiesBuffer.Clear();
			PlayerUpgradeData playerUpgradeData = upgradesFactory.GetPlayerUpgradeData(upgradeID);
			IScript script = playerUpgradeData.upgradePrototype as IScript;
			if (script != null)
			{
				foreach (ValueTuple<string, object> valueTuple in script.GetVariables())
				{
					string item = valueTuple.Item1;
					object item2 = valueTuple.Item2;
					if (item2 is float)
					{
						float item3 = (float)item2;
						LeveledItemsExtensions.PropertiesBuffer.Add(new ValueTuple<string, float>(item, item3));
					}
				}
				ScriptVariablesOverrides propertiesOverrides = playerUpgradeData.GetPropertiesOverrides(upgradeLevel);
				IScriptVariableOverride[] array = (propertiesOverrides != null) ? propertiesOverrides.VariableOverrides : null;
				if (array != null)
				{
					for (int i = 0; i < array.Length; i++)
					{
						int index;
						if (LeveledItemsExtensions.<GetPlayerUpgradeProperties>g__TryGetProperty|9_0(array[i].VariableName, out index))
						{
							string item4 = LeveledItemsExtensions.PropertiesBuffer[index].Item1;
							LeveledItemsExtensions.PropertiesBuffer[index] = new ValueTuple<string, float>(item4, (float)array[i].Value);
						}
					}
				}
			}
			return LeveledItemsExtensions.PropertiesBuffer;
		}

		// Token: 0x0600140C RID: 5132 RVA: 0x0003F22C File Offset: 0x0003D42C
		[CompilerGenerated]
		internal static bool <GetPlayerUpgradeProperties>g__TryGetProperty|9_0(string propertyName, out int propertyIndex)
		{
			for (int i = 0; i < LeveledItemsExtensions.PropertiesBuffer.Count; i++)
			{
				if (string.Equals(LeveledItemsExtensions.PropertiesBuffer[i].Item1, propertyName, StringComparison.OrdinalIgnoreCase))
				{
					propertyIndex = i;
					return true;
				}
			}
			propertyIndex = -1;
			return false;
		}

		// Token: 0x04000BB3 RID: 2995
		private static readonly List<ValueTuple<string, float>> PropertiesBuffer = new List<ValueTuple<string, float>>(16);
	}
}
