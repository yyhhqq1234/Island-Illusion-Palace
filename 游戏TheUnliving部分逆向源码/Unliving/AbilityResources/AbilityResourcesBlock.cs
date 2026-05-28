using System;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x02000355 RID: 853
	public struct AbilityResourcesBlock : IEquatable<AbilityResourcesBlock>
	{
		// Token: 0x170005BE RID: 1470
		public unsafe int this[AbilityResourceType resourceType]
		{
			get
			{
				return *this.GetResourceAmountRef((int)resourceType);
			}
			set
			{
				*this.GetResourceAmountRef((int)resourceType) = value;
			}
		}

		// Token: 0x06001BA6 RID: 7078 RVA: 0x00057158 File Offset: 0x00055358
		private unsafe ref int GetResourceAmountRef(int resourceType)
		{
			fixed (int* ptr = &this.bloodAmount)
			{
				return ref ptr[resourceType];
			}
		}

		// Token: 0x06001BA7 RID: 7079 RVA: 0x00057173 File Offset: 0x00055373
		private bool HasAllowedResourcesMask()
		{
			return this.allowedResourcesMask != 0;
		}

		// Token: 0x06001BA8 RID: 7080 RVA: 0x0005717E File Offset: 0x0005537E
		private bool IsAllowedResource(int resourceType)
		{
			return (1 << resourceType & this.allowedResourcesMask) != 0;
		}

		// Token: 0x06001BA9 RID: 7081 RVA: 0x00057190 File Offset: 0x00055390
		public unsafe AbilityResourcesBlock(AbilityResourcesCollector.RequiredResourceInfo[] requiredResources, bool setAmounts = true)
		{
			this = default(AbilityResourcesBlock);
			this.allowedResourcesMask = 0;
			foreach (AbilityResourcesCollector.RequiredResourceInfo requiredResourceInfo in requiredResources)
			{
				int resourceType = (int)requiredResourceInfo.resourceType;
				if (setAmounts)
				{
					*this.GetResourceAmountRef(resourceType) = requiredResourceInfo.requiredAmount;
				}
				this.allowedResourcesMask |= 1 << resourceType;
			}
		}

		// Token: 0x06001BAA RID: 7082 RVA: 0x000571EC File Offset: 0x000553EC
		public bool Equals(ref AbilityResourcesBlock block)
		{
			return this.bloodAmount == block.bloodAmount && this.boneAmount == block.boneAmount && this.echoAmount == block.echoAmount && this.cadaverAmount == block.cadaverAmount && this.corpseAmount == block.corpseAmount;
		}

		// Token: 0x06001BAB RID: 7083 RVA: 0x00057244 File Offset: 0x00055444
		public override bool Equals(object obj)
		{
			if (obj is AbilityResourcesBlock)
			{
				AbilityResourcesBlock abilityResourcesBlock = (AbilityResourcesBlock)obj;
				return this.Equals(ref abilityResourcesBlock);
			}
			return false;
		}

		// Token: 0x06001BAC RID: 7084 RVA: 0x0005726A File Offset: 0x0005546A
		bool IEquatable<AbilityResourcesBlock>.Equals(AbilityResourcesBlock other)
		{
			return this.Equals(ref other);
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x00057274 File Offset: 0x00055474
		public bool IsAllowedResource(AbilityResourceType resourceType)
		{
			return !this.HasAllowedResourcesMask() || this.IsAllowedResource((int)resourceType);
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x00057288 File Offset: 0x00055488
		public unsafe void ResetAmounts()
		{
			for (int i = 0; i < 5; i++)
			{
				*this.GetResourceAmountRef(i) = 0;
			}
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x000572AC File Offset: 0x000554AC
		public unsafe void Add(int amount, bool modifyAllowedResourcesOnly = false)
		{
			for (int i = 0; i < 5; i++)
			{
				if (!modifyAllowedResourcesOnly || this.IsAllowedResource(i))
				{
					*this.GetResourceAmountRef(i) += amount;
				}
			}
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x000572DD File Offset: 0x000554DD
		public unsafe void Add(AbilityResourceType resourceType, int amount, bool modifyAllowedResourcesOnly = false)
		{
			if (!modifyAllowedResourcesOnly || this.IsAllowedResource(resourceType))
			{
				*this.GetResourceAmountRef((int)resourceType) += amount;
			}
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x000572F8 File Offset: 0x000554F8
		public unsafe void Add(ref AbilityResourcesBlock block, bool modifyAllowedResourcesOnly = false)
		{
			modifyAllowedResourcesOnly &= this.HasAllowedResourcesMask();
			for (int i = 0; i < 5; i++)
			{
				if (!modifyAllowedResourcesOnly || this.IsAllowedResource(i))
				{
					*this.GetResourceAmountRef(i) += *block.GetResourceAmountRef(i);
				}
			}
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x0005733C File Offset: 0x0005553C
		public unsafe void GetMinMax([TupleElementNames(new string[]
		{
			"type",
			"amount"
		})] out ValueTuple<AbilityResourceType, int> min, [TupleElementNames(new string[]
		{
			"type",
			"amount"
		})] out ValueTuple<AbilityResourceType, int> max)
		{
			min = new ValueTuple<AbilityResourceType, int>(AbilityResourceType.Undefined, int.MaxValue);
			max = new ValueTuple<AbilityResourceType, int>(AbilityResourceType.Undefined, int.MinValue);
			for (int i = 0; i < 5; i++)
			{
				int num = *this.GetResourceAmountRef(i);
				if (num < min.Item2)
				{
					min.Item2 = num;
					min.Item1 = (AbilityResourceType)i;
				}
				if (num > max.Item2)
				{
					max.Item2 = num;
					max.Item1 = (AbilityResourceType)i;
				}
			}
		}

		// Token: 0x06001BB3 RID: 7091 RVA: 0x000573B0 File Offset: 0x000555B0
		public unsafe int GetTotalRequiredAmount()
		{
			bool flag = this.HasAllowedResourcesMask();
			int num = 0;
			for (int i = 0; i < 5; i++)
			{
				if (!flag || this.IsAllowedResource(i))
				{
					num += *this.GetResourceAmountRef(i);
				}
			}
			return num;
		}

		// Token: 0x06001BB4 RID: 7092 RVA: 0x000573EC File Offset: 0x000555EC
		public float GetRequiredAmountsEstimation(ref AbilityResourcesBlock requiredResources)
		{
			int num = 0;
			float num2 = 0f;
			bool flag = this.HasAllowedResourcesMask();
			for (int i = 0; i < 5; i++)
			{
				ref int resourceAmountRef = ref requiredResources.GetResourceAmountRef(i);
				if (resourceAmountRef > 0 && (!flag || this.IsAllowedResource(i)))
				{
					ref int resourceAmountRef2 = ref this.GetResourceAmountRef(i);
					num2 += Mathf.Clamp01((float)resourceAmountRef2 / (float)resourceAmountRef);
					num++;
				}
			}
			if (num <= 0)
			{
				return 0f;
			}
			return num2 / (float)num;
		}

		// Token: 0x06001BB5 RID: 7093 RVA: 0x0005745C File Offset: 0x0005565C
		public override int GetHashCode()
		{
			return ((((-2055464730 * -1521134295 + this.bloodAmount.GetHashCode()) * -1521134295 + this.boneAmount.GetHashCode()) * -1521134295 + this.echoAmount.GetHashCode()) * -1521134295 + this.cadaverAmount.GetHashCode()) * -1521134295 + this.corpseAmount.GetHashCode();
		}

		// Token: 0x06001BB6 RID: 7094 RVA: 0x000574C8 File Offset: 0x000556C8
		public override string ToString()
		{
			AbilityResourcesBlock.ToStringBuffer.Clear();
			AbilityResourcesBlock.ToStringBuffer.Append("(");
			AbilityResourcesBlock.ToStringBuffer.Append(string.Format("Blood: {0}, ", this.bloodAmount));
			AbilityResourcesBlock.ToStringBuffer.Append(string.Format("Bone: {0}, ", this.boneAmount));
			AbilityResourcesBlock.ToStringBuffer.Append(string.Format("Echo: {0}, ", this.echoAmount));
			AbilityResourcesBlock.ToStringBuffer.Append(string.Format("Cadaver: {0}, ", this.cadaverAmount));
			AbilityResourcesBlock.ToStringBuffer.Append(string.Format("Corpse: {0})", this.corpseAmount));
			return AbilityResourcesBlock.ToStringBuffer.ToString();
		}

		// Token: 0x04000FA6 RID: 4006
		public const int ResourceTypeCount = 5;

		// Token: 0x04000FA7 RID: 4007
		private static readonly StringBuilder ToStringBuffer = new StringBuilder(256);

		// Token: 0x04000FA8 RID: 4008
		public int bloodAmount;

		// Token: 0x04000FA9 RID: 4009
		public int boneAmount;

		// Token: 0x04000FAA RID: 4010
		public int echoAmount;

		// Token: 0x04000FAB RID: 4011
		public int cadaverAmount;

		// Token: 0x04000FAC RID: 4012
		public int corpseAmount;

		// Token: 0x04000FAD RID: 4013
		private readonly int allowedResourcesMask;
	}
}
