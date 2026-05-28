using System;
using System.Text;
using Common.Editor;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003E6 RID: 998
	[Serializable]
	public class AbilitySpecialBehaviourDescription : IEquatable<AbilitySpecialBehaviourDescription>
	{
		// Token: 0x060021BF RID: 8639 RVA: 0x0006962B File Offset: 0x0006782B
		public static bool operator ==(AbilitySpecialBehaviourDescription description0, AbilitySpecialBehaviourDescription description1)
		{
			if (description0 != null)
			{
				return description0.Equals(description1);
			}
			return description1 == null;
		}

		// Token: 0x060021C0 RID: 8640 RVA: 0x0006963C File Offset: 0x0006783C
		public static bool operator !=(AbilitySpecialBehaviourDescription description0, AbilitySpecialBehaviourDescription description1)
		{
			return !(description0 == description1);
		}

		// Token: 0x060021C1 RID: 8641 RVA: 0x00069648 File Offset: 0x00067848
		public bool IsBlank()
		{
			return this.behaviourID == AbilitySpecialBehaviourID.None && this.behaviourActivatorID == AbilitySpecialBehaviourActivatorID.None;
		}

		// Token: 0x060021C2 RID: 8642 RVA: 0x0006965D File Offset: 0x0006785D
		public bool Equals(AbilitySpecialBehaviourDescription otherDescription)
		{
			return !(otherDescription == null) && this.behaviourID == otherDescription.behaviourID && this.behaviourActivatorID == otherDescription.behaviourActivatorID;
		}

		// Token: 0x060021C3 RID: 8643 RVA: 0x00069688 File Offset: 0x00067888
		public override bool Equals(object obj)
		{
			AbilitySpecialBehaviourDescription abilitySpecialBehaviourDescription = obj as AbilitySpecialBehaviourDescription;
			return abilitySpecialBehaviourDescription != null && this.Equals(abilitySpecialBehaviourDescription);
		}

		// Token: 0x060021C4 RID: 8644 RVA: 0x000696A8 File Offset: 0x000678A8
		public override int GetHashCode()
		{
			return (2101465619 * -1521134295 + this.behaviourID.GetHashCode()) * -1521134295 + this.behaviourActivatorID.GetHashCode();
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x000696E0 File Offset: 0x000678E0
		public override string ToString()
		{
			AbilitySpecialBehaviourDescription.ToStringBuffer.Clear();
			AbilitySpecialBehaviourDescription.ToStringBuffer.Append("(");
			AbilitySpecialBehaviourDescription.ToStringBuffer.Append(this.behaviourID);
			AbilitySpecialBehaviourDescription.ToStringBuffer.Append(" ");
			AbilitySpecialBehaviourDescription.ToStringBuffer.Append(this.behaviourActivatorID);
			AbilitySpecialBehaviourDescription.ToStringBuffer.Append(")");
			return AbilitySpecialBehaviourDescription.ToStringBuffer.ToString();
		}

		// Token: 0x04001529 RID: 5417
		protected static readonly StringBuilder ToStringBuffer = new StringBuilder(64);

		// Token: 0x0400152A RID: 5418
		[EnumPopup]
		public AbilitySpecialBehaviourID behaviourID;

		// Token: 0x0400152B RID: 5419
		[EnumPopup]
		public AbilitySpecialBehaviourActivatorID behaviourActivatorID;
	}
}
