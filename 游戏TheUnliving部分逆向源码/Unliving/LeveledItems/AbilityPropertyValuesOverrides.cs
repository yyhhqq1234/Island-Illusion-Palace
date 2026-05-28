using System;
using UnityEngine;

namespace Unliving.LeveledItems
{
	// Token: 0x02000253 RID: 595
	[Serializable]
	public sealed class AbilityPropertyValuesOverrides
	{
		// Token: 0x1700043C RID: 1084
		// (get) Token: 0x060013F1 RID: 5105 RVA: 0x0003EEAB File Offset: 0x0003D0AB
		// (set) Token: 0x060013F2 RID: 5106 RVA: 0x0003EECC File Offset: 0x0003D0CC
		public AbilityPropertyDescription PropertyDescription
		{
			get
			{
				if (!this.isBuffsGeneratorProperty)
				{
					return new AbilityPropertyDescription(this.propertyDescription.propertyID);
				}
				return this.propertyDescription;
			}
			set
			{
				this.propertyDescription = value;
			}
		}

		// Token: 0x060013F3 RID: 5107 RVA: 0x0003EED5 File Offset: 0x0003D0D5
		public AbilityPropertyValuesOverrides()
		{
			this.propertyDescription = AbilityPropertyDescription.Undefined;
		}

		// Token: 0x060013F4 RID: 5108 RVA: 0x0003EEE8 File Offset: 0x0003D0E8
		public AbilityPropertyValuesOverrides(AbilityPropertyDescription propertyDescription, float[] values)
		{
			this.propertyDescription = propertyDescription;
			this.values = values;
		}

		// Token: 0x060013F5 RID: 5109 RVA: 0x0003EEFE File Offset: 0x0003D0FE
		public AbilityPropertyValuesOverrides(AbilityPropertyID propertyID, float[] values)
		{
			this.propertyDescription = new AbilityPropertyDescription(propertyID);
			this.values = values;
		}

		// Token: 0x060013F6 RID: 5110 RVA: 0x0003EF1C File Offset: 0x0003D11C
		public bool TryGetPropertyValue(int abilityLevel, out float propertyValue)
		{
			int num = abilityLevel - 1;
			if (num >= 0 && num < this.values.Length)
			{
				propertyValue = this.values[num];
				return true;
			}
			propertyValue = float.NaN;
			return false;
		}

		// Token: 0x04000BAF RID: 2991
		[SerializeField]
		private AbilityPropertyDescription propertyDescription;

		// Token: 0x04000BB0 RID: 2992
		public string propertyOwner;

		// Token: 0x04000BB1 RID: 2993
		public bool isBuffsGeneratorProperty;

		// Token: 0x04000BB2 RID: 2994
		[SerializeField]
		private float[] values;
	}
}
