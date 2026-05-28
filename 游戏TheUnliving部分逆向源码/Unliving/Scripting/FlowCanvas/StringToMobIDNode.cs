using System;
using ParadoxNotion.Design;
using Unliving.DataParsing;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000CA RID: 202
	[Name("To Mob ID", 0)]
	[Category("Unliving/Conversion")]
	public sealed class StringToMobIDNode : ConversionNodeBase<MobBehaviour.ID>
	{
		// Token: 0x060004FD RID: 1277 RVA: 0x00012264 File Offset: 0x00010464
		protected override MobBehaviour.ID Convert(string inputValue)
		{
			if (string.IsNullOrEmpty(inputValue))
			{
				return MobBehaviour.ID.None;
			}
			MobBehaviour.ID result;
			if (!ParsingUtility.TryParseEnum<MobBehaviour.ID>(inputValue, out result))
			{
				return MobBehaviour.ID.None;
			}
			return result;
		}
	}
}
