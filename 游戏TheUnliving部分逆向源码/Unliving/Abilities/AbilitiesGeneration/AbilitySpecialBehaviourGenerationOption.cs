using System;
using Common;
using UnityEngine;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003E7 RID: 999
	[Serializable]
	public sealed class AbilitySpecialBehaviourGenerationOption : AbilitySpecialBehaviourDescription, IEquatable<AbilitySpecialBehaviourGenerationOption>, IWeighted
	{
		// Token: 0x060021C8 RID: 8648 RVA: 0x00069774 File Offset: 0x00067974
		public static bool operator ==(AbilitySpecialBehaviourGenerationOption option0, AbilitySpecialBehaviourGenerationOption option1)
		{
			if (option0 != null)
			{
				return option0.Equals(option1);
			}
			return option1 == null;
		}

		// Token: 0x060021C9 RID: 8649 RVA: 0x00069785 File Offset: 0x00067985
		public static bool operator !=(AbilitySpecialBehaviourGenerationOption option0, AbilitySpecialBehaviourGenerationOption option1)
		{
			return !(option0 == option1);
		}

		// Token: 0x170006D8 RID: 1752
		// (get) Token: 0x060021CA RID: 8650 RVA: 0x00069791 File Offset: 0x00067991
		// (set) Token: 0x060021CB RID: 8651 RVA: 0x00069799 File Offset: 0x00067999
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = value;
			}
		}

		// Token: 0x060021CC RID: 8652 RVA: 0x000697A2 File Offset: 0x000679A2
		public AbilitySpecialBehaviourGenerationOption()
		{
		}

		// Token: 0x060021CD RID: 8653 RVA: 0x000697AA File Offset: 0x000679AA
		public AbilitySpecialBehaviourGenerationOption(AbilitySpecialBehaviourGenerationOption otherOption)
		{
			this.behaviourID = otherOption.behaviourID;
			this.behaviourActivatorID = otherOption.behaviourActivatorID;
			this.weight = otherOption.Weight;
		}

		// Token: 0x060021CE RID: 8654 RVA: 0x000697D6 File Offset: 0x000679D6
		public bool Equals(AbilitySpecialBehaviourGenerationOption other)
		{
			return base.Equals(other) && other.weight == this.weight;
		}

		// Token: 0x060021CF RID: 8655 RVA: 0x000697F4 File Offset: 0x000679F4
		public override bool Equals(object obj)
		{
			AbilitySpecialBehaviourGenerationOption abilitySpecialBehaviourGenerationOption = obj as AbilitySpecialBehaviourGenerationOption;
			return abilitySpecialBehaviourGenerationOption != null && this.Equals(abilitySpecialBehaviourGenerationOption);
		}

		// Token: 0x060021D0 RID: 8656 RVA: 0x00069814 File Offset: 0x00067A14
		public override int GetHashCode()
		{
			return base.GetHashCode() * -1521134295 + this.weight.GetHashCode();
		}

		// Token: 0x0400152C RID: 5420
		[SerializeField]
		private float weight;
	}
}
