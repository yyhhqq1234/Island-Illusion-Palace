using System;

namespace Unliving.LeveledItems
{
	// Token: 0x02000252 RID: 594
	public readonly struct AbilityPropertyMetadata
	{
		// Token: 0x060013EE RID: 5102 RVA: 0x0003EE6B File Offset: 0x0003D06B
		public AbilityPropertyMetadata(string propertyName, Type abilityEffectType = null)
		{
			this.PropertyName = propertyName;
			this.AbilityEffectType = abilityEffectType;
		}

		// Token: 0x060013EF RID: 5103 RVA: 0x0003EE7B File Offset: 0x0003D07B
		public AbilityPropertyMetadata(Type abilityEffectType)
		{
			this.PropertyName = abilityEffectType.Name + "." + AbilityPropertyMetadata.EffectAmountPropName;
			this.AbilityEffectType = abilityEffectType;
		}

		// Token: 0x04000BAC RID: 2988
		private static readonly string EffectAmountPropName = "Amount";

		// Token: 0x04000BAD RID: 2989
		public readonly string PropertyName;

		// Token: 0x04000BAE RID: 2990
		public readonly Type AbilityEffectType;
	}
}
