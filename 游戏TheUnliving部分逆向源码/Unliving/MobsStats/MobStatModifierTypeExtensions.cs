using System;

namespace Unliving.MobsStats
{
	// Token: 0x02000068 RID: 104
	public static class MobStatModifierTypeExtensions
	{
		// Token: 0x060002F5 RID: 757 RVA: 0x0000B1D0 File Offset: 0x000093D0
		public static MobStatModifier ToStatModifier(this MobStatModifierType modifierType, float value)
		{
			switch (modifierType)
			{
			case MobStatModifierType.BaseValue:
				return new MobStatModifier(value, 0f);
			case MobStatModifierType.BaseModifier:
				return new MobStatModifier(0f, value);
			case MobStatModifierType.ExtraModifier:
				return new MobStatModifier(0f, 0f, value);
			default:
				return MobStatModifier.Neutral;
			}
		}
	}
}
