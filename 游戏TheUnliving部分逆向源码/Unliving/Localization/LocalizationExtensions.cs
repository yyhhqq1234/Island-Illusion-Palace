using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Core;
using Game.InputManager;
using Game.Localization;
using Game.PassiveAbilities;
using Unliving.Abilities;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.MobsStats;
using Unliving.PassiveAbilities;
using Unliving.Pickables;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Localization
{
	// Token: 0x0200024A RID: 586
	public static class LocalizationExtensions
	{
		// Token: 0x060013AB RID: 5035 RVA: 0x0003D994 File Offset: 0x0003BB94
		public static string TryGetTitleWithItemLevel(Metadata itemMetadata, int itemLevel)
		{
			string text = itemMetadata.Title ?? string.Empty;
			if (itemLevel > 1)
			{
				text += string.Format(" lv. {0}", itemLevel);
			}
			return text;
		}

		// Token: 0x060013AC RID: 5036 RVA: 0x0003D9D0 File Offset: 0x0003BBD0
		public static string TryGetTitleWithItemLevel(this object item, Metadata itemMetadata)
		{
			IItemLevelProvider itemLevelProvider = item as IItemLevelProvider;
			int itemLevel;
			if (itemLevelProvider != null)
			{
				itemLevelProvider.TryGetItemLevel(out itemLevel, 2);
			}
			else
			{
				itemLevel = 0;
			}
			return LocalizationExtensions.TryGetTitleWithItemLevel(itemMetadata, itemLevel);
		}

		// Token: 0x060013AD RID: 5037 RVA: 0x0003D9FC File Offset: 0x0003BBFC
		public static string GetSpriteKey(CurrencyID currencyID)
		{
			switch (currencyID)
			{
			case CurrencyID.Gold:
				return "gold_sprite";
			case CurrencyID.Meta:
				return "meta_currency_sprite";
			case CurrencyID.Ash:
				return "ash_sprite";
			case CurrencyID.Cinder:
				return "cinder_sprite";
			}
			return string.Empty;
		}

		// Token: 0x060013AE RID: 5038 RVA: 0x0003DA3C File Offset: 0x0003BC3C
		public static string GetInputElementsString(this IList<InputElement> inputElements, LocalizationManager localizationManager, string separator = ", ")
		{
			if (inputElements == null || inputElements.Count == 0)
			{
				return "?";
			}
			string empty = string.Empty;
			List<string> list = new List<string>();
			for (int i = 0; i < inputElements.Count; i++)
			{
				InputElement inputElement = inputElements[i];
				PlayerAction actionID = (PlayerAction)inputElement.actionID;
				if (Enum.IsDefined(typeof(PlayerAction), actionID))
				{
					string text = string.Empty;
					text = ((inputElement.axisContribution == InputAxisContribution.Positive) ? "+" : "-");
					string description = localizationManager.GetMetadata<PlayerAction>(actionID, new string[]
					{
						text
					}).Description;
					if (!description.Contains("<hide>"))
					{
						list.Add(description);
					}
				}
			}
			return string.Join(separator, list);
		}

		// Token: 0x060013AF RID: 5039 RVA: 0x0003DAF0 File Offset: 0x0003BCF0
		public static string GetPriceString(this ICurrencyOperationArgs args, LocalizationManager localizationManager, bool inverseOrder = false)
		{
			string title = localizationManager.GetTitle(LocalizationExtensions.GetSpriteKey(args.CurrencyID));
			if (inverseOrder)
			{
				return args.Amount.ToString() + title;
			}
			return title + args.Amount.ToString();
		}

		// Token: 0x060013B0 RID: 5040 RVA: 0x0003DB3C File Offset: 0x0003BD3C
		public static CurrencyOperationArgs GetPriceArgs(this IPickableObject pickable, MultiRepresentationObjectInstantiator.ObjectType? contextOverride = null)
		{
			CurrencyOperationArgs result = default(CurrencyOperationArgs);
			PickableObjectBase pickableObjectBase = pickable as PickableObjectBase;
			if (pickableObjectBase != null)
			{
				MultiRepresentationObjectInstantiator.ObjectType objectType = contextOverride ?? pickableObjectBase.CurrentPickingContext;
				IPurchasable purchasableData = pickableObjectBase.PurchasableData;
				if (objectType == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
				{
					if (purchasableData.Purchased)
					{
						return result;
					}
					result = purchasableData.UnlockArgs;
				}
				else if (objectType == MultiRepresentationObjectInstantiator.ObjectType.StoreObject)
				{
					result = purchasableData.BuyArgs;
				}
			}
			return result;
		}

		// Token: 0x060013B1 RID: 5041 RVA: 0x0003DBA4 File Offset: 0x0003BDA4
		public static string GetPriceString(this IPickableObject pickable, bool withCurrencySprite, MultiRepresentationObjectInstantiator.ObjectType? contextOverride = null)
		{
			CurrencyOperationArgs priceArgs = pickable.GetPriceArgs(contextOverride);
			PickableObjectBase pickableObjectBase = pickable as PickableObjectBase;
			if (pickableObjectBase != null)
			{
				return priceArgs.GetPriceString(pickableObjectBase.CurrentGame, withCurrencySprite);
			}
			return string.Empty;
		}

		// Token: 0x060013B2 RID: 5042 RVA: 0x0003DBDC File Offset: 0x0003BDDC
		public static string GetPriceString(this ICurrencyOperationArgs args, IGame game, bool withCurrencySprite)
		{
			LocalizationManager localizationManager;
			if (args.CurrencyID == CurrencyID.None || !game.Services.TryGet<LocalizationManager>(out localizationManager))
			{
				return string.Empty;
			}
			if (withCurrencySprite)
			{
				return args.GetPriceString(localizationManager, false);
			}
			return args.Amount.ToString();
		}

		// Token: 0x060013B3 RID: 5043 RVA: 0x0003DC20 File Offset: 0x0003BE20
		public static Metadata GetPassiveAbilityMetadata(this LocalizationManager localizationManager, IPassiveAbility passiveAbility)
		{
			if (localizationManager == null)
			{
				return null;
			}
			string text = string.Empty;
			PassiveAbilityID numericID = (PassiveAbilityID)passiveAbility.NumericID;
			Metadata metadata = localizationManager.GetMetadata<PassiveAbilityID>(numericID, Array.Empty<string>());
			PassiveAbility passiveAbility2 = passiveAbility as PassiveAbility;
			if (passiveAbility2 != null)
			{
				string newLine = Environment.NewLine;
				for (int i = 0; i < passiveAbility2.StatModifiers.Length; i++)
				{
					Metadata metadata2 = localizationManager.GetMetadata<MobStatID>(passiveAbility2.StatModifiers[i].targetStat, new string[]
					{
						passiveAbility2.StatModifiers[i].ToString()
					});
					string text2 = ((metadata2 != null) ? metadata2.Description : null) + newLine;
					if (text2 != null)
					{
						text += text2;
					}
				}
			}
			if (metadata != null)
			{
				Metadata metadata3 = metadata;
				metadata3.Description += text;
			}
			return metadata;
		}

		// Token: 0x060013B4 RID: 5044 RVA: 0x0003DCEC File Offset: 0x0003BEEC
		public static string GetSpecialBehaviourDescriptionLocalization(this AbilitySpecialBehaviourDescription specialBehaviourDescription, LocalizationManager localizationManager, bool withEnchantmentTitle)
		{
			AbilitySpecialBehaviourID behaviourID = specialBehaviourDescription.behaviourID;
			AbilitySpecialBehaviourActivatorID behaviourActivatorID = specialBehaviourDescription.behaviourActivatorID;
			string newLine = Environment.NewLine;
			if (specialBehaviourDescription.IsBlank() || (behaviourID == (AbilitySpecialBehaviourID)(-1) && behaviourActivatorID == (AbilitySpecialBehaviourActivatorID)(-1)))
			{
				return newLine + localizationManager.GetMetadata<AbilitySpecialBehaviourID>(AbilitySpecialBehaviourID.None, Array.Empty<string>()).Description;
			}
			string text = string.Empty;
			if (withEnchantmentTitle)
			{
				text = localizationManager.GetMetadata("ui_enchatment_title", Array.Empty<string>()).Title + newLine;
			}
			text = text + localizationManager.GetMetadata("ui_enchatment_activator", Array.Empty<string>()).Title + " ";
			if (behaviourActivatorID == AbilitySpecialBehaviourActivatorID.None)
			{
				text += localizationManager.GetTitle("ui_on_hit");
			}
			else
			{
				text += localizationManager.GetMetadata<AbilitySpecialBehaviourActivatorID>(specialBehaviourDescription.behaviourActivatorID, Array.Empty<string>()).Title;
			}
			if (behaviourID != AbilitySpecialBehaviourID.None)
			{
				text = string.Concat(new string[]
				{
					text,
					newLine,
					localizationManager.GetMetadata("ui_enchatment_effect", Array.Empty<string>()).Title,
					" ",
					localizationManager.GetMetadata<AbilitySpecialBehaviourID>(specialBehaviourDescription.behaviourID, Array.Empty<string>()).Description
				});
			}
			return text;
		}

		// Token: 0x060013B5 RID: 5045 RVA: 0x0003DE00 File Offset: 0x0003C000
		public static string GetTestSpecialBehaviourDescription(this AbilitySpecialBehaviourDescription specialBehaviourDescription, bool isSeparateLine = true)
		{
			string text = (specialBehaviourDescription != null) ? specialBehaviourDescription.ToString() : null;
			if (text == null)
			{
				text = string.Empty;
			}
			else if (isSeparateLine)
			{
				text += Environment.NewLine;
			}
			return text;
		}

		// Token: 0x060013B6 RID: 5046 RVA: 0x0003DE35 File Offset: 0x0003C035
		public static string GetTestSpecialBehaviourDescription(this IGameAbilitiesFactory abilitiesFactory, BaseAbility ability, bool isSeparateLine = true)
		{
			return abilitiesFactory.GetAbilitySpecialBehaviourDescription(ability).GetTestSpecialBehaviourDescription(isSeparateLine);
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x0003DE44 File Offset: 0x0003C044
		public static string GetAbilityPropertyOverrideDescription(this object itemID, string propertyOwner, AbilityPropertyID propertyID, LocalizationManager localizationManager, string propertyValue = null)
		{
			string[] array = new string[]
			{
				string.Format("{0}{1}_{2}", "abilityProperty_", propertyOwner, propertyID).Replace(' ', '_'),
				string.Format("{0}{1}_{2}", "abilityProperty_", itemID, propertyID).Replace(' ', '_'),
				string.Format("{0}{1}", "abilityProperty_", propertyID).Replace(' ', '_')
			};
			if (array == null || array.Length == 0)
			{
				return string.Empty;
			}
			int i = 0;
			while (i < array.Length)
			{
				Metadata metadata;
				if (localizationManager.TryGetMetadata(array[i], out metadata))
				{
					if (!string.IsNullOrEmpty(propertyValue))
					{
						return string.Format(metadata.Description, propertyValue);
					}
					int num = metadata.Description.IndexOf(':');
					if (num >= 0)
					{
						return metadata.Description.Remove(num);
					}
					return metadata.Description;
				}
				else
				{
					i++;
				}
			}
			return array[0];
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x0003DF28 File Offset: 0x0003C128
		public static string ToRoman(this int number)
		{
			if (number < 1 || number > 3999)
			{
				return string.Empty;
			}
			if (number >= 1000)
			{
				return "M" + (number - 1000).ToRoman();
			}
			if (number >= 900)
			{
				return "CM" + (number - 900).ToRoman();
			}
			if (number >= 500)
			{
				return "D" + (number - 500).ToRoman();
			}
			if (number >= 400)
			{
				return "CD" + (number - 400).ToRoman();
			}
			if (number >= 100)
			{
				return "C" + (number - 100).ToRoman();
			}
			if (number >= 90)
			{
				return "XC" + (number - 90).ToRoman();
			}
			if (number >= 50)
			{
				return "L" + (number - 50).ToRoman();
			}
			if (number >= 40)
			{
				return "XL" + (number - 40).ToRoman();
			}
			if (number >= 10)
			{
				return "X" + (number - 10).ToRoman();
			}
			if (number >= 9)
			{
				return "IX" + (number - 9).ToRoman();
			}
			if (number >= 5)
			{
				return "V" + (number - 5).ToRoman();
			}
			if (number >= 4)
			{
				return "IV" + (number - 4).ToRoman();
			}
			if (number >= 1)
			{
				return "I" + (number - 1).ToRoman();
			}
			return string.Empty;
		}

		// Token: 0x04000B75 RID: 2933
		private const string MetaCurrencySpriteKey = "meta_currency_sprite";

		// Token: 0x04000B76 RID: 2934
		private const string GoldSpriteKey = "gold_sprite";

		// Token: 0x04000B77 RID: 2935
		private const string AshSpriteKey = "ash_sprite";

		// Token: 0x04000B78 RID: 2936
		private const string CinderSpriteKey = "cinder_sprite";
	}
}
