using System;
using ParadoxNotion.Design;
using Unliving.Abilities;
using Unliving.DataParsing;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C9 RID: 201
	[Name("To Ability ID", 0)]
	[Category("Unliving/Conversion")]
	public sealed class StringToAbilityIDNode : ConversionNodeBase<AbilityID>
	{
		// Token: 0x060004FB RID: 1275 RVA: 0x00012238 File Offset: 0x00010438
		protected override AbilityID Convert(string inputValue)
		{
			if (string.IsNullOrEmpty(inputValue))
			{
				return AbilityID.None;
			}
			AbilityID result;
			if (!ParsingUtility.TryParseEnum<AbilityID>(inputValue, out result))
			{
				return AbilityID.None;
			}
			return result;
		}
	}
}
