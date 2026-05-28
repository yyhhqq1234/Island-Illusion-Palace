using System;
using Common.Editor;

namespace Unliving.LeveledItems
{
	// Token: 0x02000250 RID: 592
	[Serializable]
	public struct AbilityPropertyDescription : IEquatable<AbilityPropertyDescription>
	{
		// Token: 0x060013E7 RID: 5095 RVA: 0x0003EDC4 File Offset: 0x0003CFC4
		public static explicit operator AbilityPropertyDescription(AbilityPropertyID propertyID)
		{
			return new AbilityPropertyDescription(propertyID);
		}

		// Token: 0x060013E8 RID: 5096 RVA: 0x0003EDCC File Offset: 0x0003CFCC
		public AbilityPropertyDescription(AbilityPropertyID propertyID, int buffsGeneratorIndex)
		{
			this.propertyID = propertyID;
			this.buffsGeneratorIndex = buffsGeneratorIndex;
		}

		// Token: 0x060013E9 RID: 5097 RVA: 0x0003EDDC File Offset: 0x0003CFDC
		public AbilityPropertyDescription(AbilityPropertyID propertyID)
		{
			this = new AbilityPropertyDescription(propertyID, -1);
		}

		// Token: 0x060013EA RID: 5098 RVA: 0x0003EDE6 File Offset: 0x0003CFE6
		public bool Equals(AbilityPropertyDescription other)
		{
			return other.propertyID == this.propertyID && other.buffsGeneratorIndex == this.buffsGeneratorIndex;
		}

		// Token: 0x060013EB RID: 5099 RVA: 0x0003EE08 File Offset: 0x0003D008
		public override bool Equals(object obj)
		{
			if (obj is AbilityPropertyDescription)
			{
				AbilityPropertyDescription other = (AbilityPropertyDescription)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x060013EC RID: 5100 RVA: 0x0003EE2D File Offset: 0x0003D02D
		public override int GetHashCode()
		{
			return (-268791140 * -1521134295 + this.propertyID.GetHashCode()) * -1521134295 + this.buffsGeneratorIndex.GetHashCode();
		}

		// Token: 0x04000B91 RID: 2961
		public static readonly AbilityPropertyDescription Undefined = new AbilityPropertyDescription(AbilityPropertyID.Undefined);

		// Token: 0x04000B92 RID: 2962
		[EnumPopup]
		public AbilityPropertyID propertyID;

		// Token: 0x04000B93 RID: 2963
		public int buffsGeneratorIndex;
	}
}
