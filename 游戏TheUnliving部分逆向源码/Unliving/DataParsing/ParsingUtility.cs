using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using RedScarf.EasyCSV;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Unliving.LeveledItems;
using Unliving.MobsStats;

namespace Unliving.DataParsing
{
	// Token: 0x020002D0 RID: 720
	public static class ParsingUtility
	{
		// Token: 0x0600190C RID: 6412 RVA: 0x0004EF60 File Offset: 0x0004D160
		private static Dictionary<string, int> GetEnumValues(Type enumType)
		{
			Dictionary<string, int> dictionary;
			if (!ParsingUtility.CachedEnumValues.TryGetValue(enumType, out dictionary))
			{
				string[] names = Enum.GetNames(enumType);
				Array values = Enum.GetValues(enumType);
				dictionary = new Dictionary<string, int>(names.Length, StringComparer.OrdinalIgnoreCase);
				for (int i = 0; i < names.Length; i++)
				{
					dictionary.Add(names[i], (int)values.GetValue(i));
				}
				ParsingUtility.CachedEnumValues.Add(enumType, dictionary);
			}
			return dictionary;
		}

		// Token: 0x0600190D RID: 6413 RVA: 0x0004EFC8 File Offset: 0x0004D1C8
		public static ICollection<string> GetEnumNames(Type enumType)
		{
			return ParsingUtility.GetEnumValues(enumType).Keys;
		}

		// Token: 0x0600190E RID: 6414 RVA: 0x0004EFD8 File Offset: 0x0004D1D8
		public static int GetEnumValue(string enumString, Type enumType)
		{
			if (string.IsNullOrWhiteSpace(enumString))
			{
				return int.MinValue;
			}
			int result;
			if (int.TryParse(enumString, out result))
			{
				return result;
			}
			int result2;
			if (!ParsingUtility.GetEnumValues(enumType).TryGetValue(enumString, out result2))
			{
				return int.MinValue;
			}
			return result2;
		}

		// Token: 0x0600190F RID: 6415 RVA: 0x0004F018 File Offset: 0x0004D218
		public unsafe static bool TryParseEnum<T>(string enumString, out T enumValue) where T : Enum
		{
			int enumValue2 = ParsingUtility.GetEnumValue(enumString, typeof(T));
			if (enumValue2 > -2147483648)
			{
				enumValue = *UnsafeUtility.As<int, T>(ref enumValue2);
				return true;
			}
			enumValue = default(T);
			return false;
		}

		// Token: 0x06001910 RID: 6416 RVA: 0x0004F05A File Offset: 0x0004D25A
		public static bool TryParseStatID(string statIDString, out MobStatID statID)
		{
			if (ParsingUtility.TryParseEnum<MobStatID>(statIDString, out statID))
			{
				return true;
			}
			statID = MobStatID.Undefined;
			return false;
		}

		// Token: 0x06001911 RID: 6417 RVA: 0x0004F06C File Offset: 0x0004D26C
		public static bool TryGetCsvTableDelimiter(string table, out char delimiter)
		{
			delimiter = '\0';
			foreach (char c in table)
			{
				if (Array.IndexOf<char>(ParsingUtility.CsvTableDelimiters, c) >= 0)
				{
					delimiter = c;
					break;
				}
			}
			return delimiter != '\0';
		}

		// Token: 0x06001912 RID: 6418 RVA: 0x0004F0B0 File Offset: 0x0004D2B0
		public static bool TryParseCsvTable(string table, out List<List<string>> parsedTable)
		{
			char separator;
			if (!string.IsNullOrWhiteSpace(table) && ParsingUtility.TryGetCsvTableDelimiter(table, out separator))
			{
				CsvHelper.Init(separator);
				parsedTable = CsvHelper.Create("", table, true, true).RawDataList;
				return parsedTable.Count != 0;
			}
			parsedTable = null;
			return false;
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x0004F0F8 File Offset: 0x0004D2F8
		public static bool TryParseCsvTable(TextAsset tableAsset, out List<List<string>> parsedTable)
		{
			parsedTable = null;
			return tableAsset != null && ParsingUtility.TryParseCsvTable(tableAsset.text, out parsedTable);
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x0004F114 File Offset: 0x0004D314
		public static bool TryParseFloat(string str, out float value)
		{
			return float.TryParse(str.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
		}

		// Token: 0x06001915 RID: 6421 RVA: 0x0004F130 File Offset: 0x0004D330
		public static int ParseFloats(string values, out float[] parsedValues, char delimiter = '_')
		{
			string[] array = values.Split(new char[]
			{
				delimiter
			});
			int result = 0;
			for (int i = 0; i < array.Length; i++)
			{
				string text = array[i].TrimStart(Array.Empty<char>()).TrimEnd(Array.Empty<char>());
				float num;
				if (ParsingUtility.TryParseFloat(text, out num))
				{
					ParsingUtility.FloatsBuffer[result++] = num;
				}
				else
				{
					Debug.LogError(string.Concat(new string[]
					{
						"Unable to parse ",
						text,
						" from ",
						values,
						"."
					}));
				}
			}
			parsedValues = ParsingUtility.FloatsBuffer;
			return result;
		}

		// Token: 0x06001916 RID: 6422 RVA: 0x0004F1C8 File Offset: 0x0004D3C8
		public static bool TryParseAbilityPropertyDescription(string description, out AbilityPropertyDescription propertyDescription)
		{
			ParsingUtility.ToStringBuffer.Clear();
			ParsingUtility.ToStringBuffer.Append(description);
			int buffsGeneratorIndex = -1;
			for (int i = ParsingUtility.ToStringBuffer.Length - 1; i >= 0; i--)
			{
				if (ParsingUtility.ToStringBuffer[i] == '_')
				{
					int num;
					if (int.TryParse(description.Remove(0, i + 1), out num) && num > 0)
					{
						buffsGeneratorIndex = num - 1;
					}
					ParsingUtility.ToStringBuffer.Length -= description.Length - i;
					break;
				}
			}
			for (int j = ParsingUtility.ToStringBuffer.Length - 1; j >= 0; j--)
			{
				if (ParsingUtility.ToStringBuffer[j] == ' ')
				{
					ParsingUtility.ToStringBuffer.Remove(j, 1);
				}
			}
			AbilityPropertyID propertyID;
			if (ParsingUtility.TryParseEnum<AbilityPropertyID>(ParsingUtility.ToStringBuffer.ToString(), out propertyID))
			{
				propertyDescription = new AbilityPropertyDescription(propertyID, buffsGeneratorIndex);
				return true;
			}
			propertyDescription = default(AbilityPropertyDescription);
			return false;
		}

		// Token: 0x06001917 RID: 6423 RVA: 0x0004F2B0 File Offset: 0x0004D4B0
		public static bool TryParseMobStatModifier(string modifierString, out MobStatModifier statModifier, MobStatModifierType explicitType = MobStatModifierType.None)
		{
			if (string.IsNullOrWhiteSpace(modifierString))
			{
				statModifier = MobStatModifier.Neutral;
				return false;
			}
			if (explicitType != MobStatModifierType.None)
			{
				float num;
				if (!ParsingUtility.TryParseFloat(modifierString, out num))
				{
					statModifier = MobStatModifier.Neutral;
					return false;
				}
				switch (explicitType)
				{
				case MobStatModifierType.BaseValue:
					statModifier = new MobStatModifier(num, 0f);
					return true;
				case MobStatModifierType.BaseModifier:
					statModifier = new MobStatModifier(0f, ParsingUtility.<TryParseMobStatModifier>g__GetBaseModifierValue|15_0(num));
					return true;
				case MobStatModifierType.ExtraModifier:
					statModifier = new MobStatModifier(0f, 0f, num);
					return true;
				}
			}
			int num2 = modifierString.Length - 1;
			char c = (num2 > 0) ? char.ToLower(modifierString[num2]) : ' ';
			if (c == '%')
			{
				return ParsingUtility.<TryParseMobStatModifier>g__TryParseBaseModifier|15_1(modifierString.Remove(num2, 1), out statModifier);
			}
			float extraModifier;
			if (c == 'x' && ParsingUtility.TryParseFloat(modifierString.Remove(num2, 1), out extraModifier))
			{
				statModifier = new MobStatModifier(0f, 0f, extraModifier);
				return true;
			}
			return ParsingUtility.<TryParseMobStatModifier>g__TryParseBaseModifier|15_1(modifierString, out statModifier);
		}

		// Token: 0x06001918 RID: 6424 RVA: 0x0004F3B0 File Offset: 0x0004D5B0
		public static bool TryParseMobStatModifier(MobStatID statID, string modifierString, out TargetedMobStatModifier statModifier, MobStatModifierType explicitType = MobStatModifierType.None)
		{
			MobStatModifier value;
			if (statID != MobStatID.Undefined && ParsingUtility.TryParseMobStatModifier(modifierString, out value, explicitType))
			{
				statModifier = new TargetedMobStatModifier
				{
					targetStat = statID
				};
				statModifier.SetValue(value);
				return true;
			}
			statModifier = TargetedMobStatModifier.GetDefault();
			return false;
		}

		// Token: 0x06001919 RID: 6425 RVA: 0x0004F3F8 File Offset: 0x0004D5F8
		public static bool TryParseMobStatModifier(string statIDString, string modifierString, out TargetedMobStatModifier statModifier, MobStatModifierType explicitType = MobStatModifierType.None)
		{
			MobStatID statID;
			if (ParsingUtility.TryParseStatID(statIDString, out statID))
			{
				return ParsingUtility.TryParseMobStatModifier(statID, modifierString, out statModifier, explicitType);
			}
			statModifier = TargetedMobStatModifier.GetDefault();
			return false;
		}

		// Token: 0x0600191B RID: 6427 RVA: 0x0004F460 File Offset: 0x0004D660
		[CompilerGenerated]
		internal static float <TryParseMobStatModifier>g__GetBaseModifierValue|15_0(float rawValue)
		{
			if (Mathf.Abs(rawValue) > 1f)
			{
				rawValue *= 0.01f;
			}
			return rawValue;
		}

		// Token: 0x0600191C RID: 6428 RVA: 0x0004F47C File Offset: 0x0004D67C
		[CompilerGenerated]
		internal static bool <TryParseMobStatModifier>g__TryParseBaseModifier|15_1(string str, out MobStatModifier baseModifier)
		{
			float rawValue;
			if (ParsingUtility.TryParseFloat(str, out rawValue))
			{
				baseModifier = new MobStatModifier(0f, ParsingUtility.<TryParseMobStatModifier>g__GetBaseModifierValue|15_0(rawValue), 1f);
				return true;
			}
			baseModifier = MobStatModifier.Neutral;
			return false;
		}

		// Token: 0x04000E1D RID: 3613
		private static readonly StringBuilder ToStringBuffer = new StringBuilder(64);

		// Token: 0x04000E1E RID: 3614
		private static readonly float[] FloatsBuffer = new float[8];

		// Token: 0x04000E1F RID: 3615
		private static readonly char[] CsvTableDelimiters = new char[]
		{
			',',
			';',
			'|'
		};

		// Token: 0x04000E20 RID: 3616
		private static readonly Dictionary<Type, Dictionary<string, int>> CachedEnumValues = new Dictionary<Type, Dictionary<string, int>>(16);
	}
}
