using System;
using System.Text;
using Game.Abilities;
using Game.Factories;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.LeveledItems;

namespace Unliving.Abilities
{
	// Token: 0x0200036B RID: 875
	[Serializable]
	public struct AbilityInfo : IEquatable<AbilityInfo>
	{
		// Token: 0x06001CCF RID: 7375 RVA: 0x0005B09C File Offset: 0x0005929C
		public static explicit operator AbilityInfo(AbilityID abilityID)
		{
			return new AbilityInfo(abilityID);
		}

		// Token: 0x06001CD0 RID: 7376 RVA: 0x0005B0A4 File Offset: 0x000592A4
		public AbilityInfo(AbilityID abilityID, int abilityLevel)
		{
			this.abilityID = abilityID;
			this.abilityLevel = abilityLevel;
			this.specialBehaviourDescription = null;
		}

		// Token: 0x06001CD1 RID: 7377 RVA: 0x0005B0BB File Offset: 0x000592BB
		public AbilityInfo(AbilityID abilityID)
		{
			this.abilityID = abilityID;
			this.abilityLevel = 1;
			this.specialBehaviourDescription = null;
		}

		// Token: 0x06001CD2 RID: 7378 RVA: 0x0005B0D2 File Offset: 0x000592D2
		public AbilityInfo(BaseAbility ability)
		{
			this.abilityID = (AbilityID)((ability != null) ? ability.ID : 0);
			ability.TryGetAbilityLevel(out this.abilityLevel, 1);
			this.specialBehaviourDescription = null;
		}

		// Token: 0x06001CD3 RID: 7379 RVA: 0x0005B104 File Offset: 0x00059304
		public bool IsMatch(BaseAbility ability)
		{
			int num;
			return this.abilityID != AbilityID.None && !(ability == null) && ability.ID == (int)this.abilityID && (this.abilityLevel < 1 || (ability.TryGetAbilityLevel(out num, 1) && this.abilityLevel == num));
		}

		// Token: 0x06001CD4 RID: 7380 RVA: 0x0005B155 File Offset: 0x00059355
		public bool Equals(AbilityInfo abilityInfo)
		{
			return this.abilityID == abilityInfo.abilityID && this.abilityLevel == abilityInfo.abilityLevel && this.specialBehaviourDescription == abilityInfo.specialBehaviourDescription;
		}

		// Token: 0x06001CD5 RID: 7381 RVA: 0x0005B188 File Offset: 0x00059388
		public override bool Equals(object obj)
		{
			if (obj is AbilityInfo)
			{
				AbilityInfo abilityInfo = (AbilityInfo)obj;
				return this.Equals(abilityInfo);
			}
			return false;
		}

		// Token: 0x06001CD6 RID: 7382 RVA: 0x0005B1B0 File Offset: 0x000593B0
		public override int GetHashCode()
		{
			int num = -898696675;
			num = num * -1521134295 + this.abilityID.GetHashCode();
			num = num * -1521134295 + this.abilityLevel.GetHashCode();
			if (this.specialBehaviourDescription != null)
			{
				num = num * -1521134295 + this.specialBehaviourDescription.GetHashCode();
			}
			return num;
		}

		// Token: 0x06001CD7 RID: 7383 RVA: 0x0005B214 File Offset: 0x00059414
		public override string ToString()
		{
			AbilityInfo.ToStringBuffer.Clear();
			AbilityInfo.ToStringBuffer.Append("(");
			AbilityInfo.ToStringBuffer.Append(this.abilityID);
			AbilityInfo.ToStringBuffer.Append(", ");
			AbilityInfo.ToStringBuffer.Append(this.abilityLevel);
			if (this.specialBehaviourDescription != null)
			{
				AbilityInfo.ToStringBuffer.Append(", ");
				AbilityInfo.ToStringBuffer.Append(this.specialBehaviourDescription.ToString());
			}
			AbilityInfo.ToStringBuffer.Append(")");
			return AbilityInfo.ToStringBuffer.ToString();
		}

		// Token: 0x04001051 RID: 4177
		private static readonly StringBuilder ToStringBuffer = new StringBuilder(64);

		// Token: 0x04001052 RID: 4178
		public static readonly AbilityInfo Undefined = new AbilityInfo(AbilityID.None);

		// Token: 0x04001053 RID: 4179
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID abilityID;

		// Token: 0x04001054 RID: 4180
		public int abilityLevel;

		// Token: 0x04001055 RID: 4181
		public AbilitySpecialBehaviourDescription specialBehaviourDescription;
	}
}
