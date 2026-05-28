using System;
using Game.Abilities;
using Game.Factories;

namespace Unliving.Abilities
{
	// Token: 0x02000368 RID: 872
	[Serializable]
	public struct AbilityDescription : IEquatable<AbilityDescription>
	{
		// Token: 0x06001CA6 RID: 7334 RVA: 0x0005A3A6 File Offset: 0x000585A6
		public static bool operator ==(AbilityDescription left, AbilityDescription right)
		{
			return left.Equals(ref right);
		}

		// Token: 0x06001CA7 RID: 7335 RVA: 0x0005A3B1 File Offset: 0x000585B1
		public static bool operator !=(AbilityDescription left, AbilityDescription right)
		{
			return !left.Equals(ref right);
		}

		// Token: 0x170005F7 RID: 1527
		// (get) Token: 0x06001CA8 RID: 7336 RVA: 0x0005A3BF File Offset: 0x000585BF
		public static AbilityDescription BlankDescription
		{
			get
			{
				return AbilityDescription.blankDescription;
			}
		}

		// Token: 0x06001CA9 RID: 7337 RVA: 0x0005A3C8 File Offset: 0x000585C8
		private static MobActivationAbilityType GetMobActivationAbilityType(BaseAbility ability)
		{
			ITypedMobActivationAbility typedMobActivationAbility = ability as ITypedMobActivationAbility;
			if (typedMobActivationAbility != null)
			{
				return typedMobActivationAbility.ActivationAbilityType;
			}
			return MobActivationAbilityType.None;
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x0005A3E8 File Offset: 0x000585E8
		public AbilityDescription(BaseAbility ability, bool isPrototypeAbility = false)
		{
			this.isAny = false;
			if (isPrototypeAbility)
			{
				this.abilityReference = null;
				this.abilityPrototypeReference = ability;
			}
			else
			{
				this.abilityReference = ability;
				this.abilityPrototypeReference = ability.Prototype;
			}
			BaseAbility parentAbility = ability.ParentAbility;
			this.parentAbilityPrototypeReference = ((parentAbility != null) ? parentAbility.Prototype : null);
			this.abilityID = (AbilityID)ability.ID;
			this.abilityType = (AbilityTypes)ability.Type;
			this.isProjectileAbility = ability.IsProjectileAbility(true);
			this.isZoneEffectAbility = ability.IsZoneEffectAbility;
			this.isPostMortemAbility = ability.IsPostMortemAbility;
			this.isMobActivationAbility = ability.IsMobActivationAbility();
			this.isSacrificationAbility = (ability is MobSacrificeAbility);
			this.mobActivationAbilityType = AbilityDescription.GetMobActivationAbilityType(ability);
			ability.TryGetExplicitUsingContext(out this.abilityUsingContext);
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x0005A4AC File Offset: 0x000586AC
		public override bool Equals(object obj)
		{
			if (obj is AbilityDescription)
			{
				AbilityDescription abilityDescription = (AbilityDescription)obj;
				return this.Equals(ref abilityDescription);
			}
			return false;
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x0005A4D4 File Offset: 0x000586D4
		public bool Equals(ref AbilityDescription other)
		{
			return this.isAny == other.isAny && this.abilityReference == other.abilityReference && this.abilityPrototypeReference == other.abilityPrototypeReference && this.parentAbilityPrototypeReference == other.parentAbilityPrototypeReference && this.abilityID == other.abilityID && this.abilityType == other.abilityType && this.isProjectileAbility == other.isProjectileAbility && this.isZoneEffectAbility == other.isZoneEffectAbility && this.isPostMortemAbility == other.isPostMortemAbility && this.isMobActivationAbility == other.isMobActivationAbility && this.isSacrificationAbility == other.isSacrificationAbility && this.mobActivationAbilityType == other.mobActivationAbilityType && this.abilityUsingContext == other.abilityUsingContext;
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x0005A5B1 File Offset: 0x000587B1
		bool IEquatable<AbilityDescription>.Equals(AbilityDescription other)
		{
			return this.Equals(ref other);
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x0005A5BC File Offset: 0x000587BC
		public override int GetHashCode()
		{
			int num = -1636724736;
			num = num * -1521134295 + this.isAny.GetHashCode();
			if (this.abilityReference != null)
			{
				num = num * -1521134295 + this.abilityReference.GetInstanceID();
			}
			if (this.abilityPrototypeReference != null)
			{
				num = num * -1521134295 + this.abilityPrototypeReference.GetInstanceID();
			}
			if (this.parentAbilityPrototypeReference != null)
			{
				num = num * -1521134295 + this.parentAbilityPrototypeReference.GetInstanceID();
			}
			num = num * -1521134295 + this.abilityID.GetHashCode();
			num = num * -1521134295 + this.abilityType.GetHashCode();
			num = num * -1521134295 + this.isProjectileAbility.GetHashCode();
			num = num * -1521134295 + this.isZoneEffectAbility.GetHashCode();
			num = num * -1521134295 + this.isPostMortemAbility.GetHashCode();
			num = num * -1521134295 + this.isMobActivationAbility.GetHashCode();
			num = num * -1521134295 + this.isSacrificationAbility.GetHashCode();
			num = num * -1521134295 + this.mobActivationAbilityType.GetHashCode();
			return num * -1521134295 + this.abilityUsingContext.GetHashCode();
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x0005A716 File Offset: 0x00058916
		public bool IsBlank()
		{
			return this.Equals(ref AbilityDescription.blankDescription);
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x0005A724 File Offset: 0x00058924
		public bool IsMatch(BaseAbility ability)
		{
			if (ability == null)
			{
				return false;
			}
			if (!this.isAny)
			{
				if (this.abilityReference != null && ability != this.abilityReference)
				{
					return false;
				}
				if (this.abilityPrototypeReference != null && ability.Prototype != this.abilityPrototypeReference)
				{
					return false;
				}
				if (this.parentAbilityPrototypeReference != null)
				{
					BaseAbility parentAbility = ability.ParentAbility;
					if (((parentAbility != null) ? parentAbility.Prototype : null) != this.parentAbilityPrototypeReference)
					{
						return false;
					}
				}
				if (this.abilityID != AbilityID.None && ability.ID != (int)this.abilityID)
				{
					return false;
				}
				if (this.abilityType != AbilityTypes.None && ability.Type != (int)this.abilityType)
				{
					return false;
				}
				if (this.isProjectileAbility && !ability.IsProjectileAbility(true))
				{
					return false;
				}
				if (this.isZoneEffectAbility && !ability.IsZoneEffectAbility)
				{
					return false;
				}
				if (this.isPostMortemAbility && !ability.IsPostMortemAbility)
				{
					return false;
				}
				if (this.isMobActivationAbility && !ability.IsMobActivationAbility())
				{
					return false;
				}
				if (this.isSacrificationAbility && !(ability is MobSacrificeAbility))
				{
					return false;
				}
				if (this.mobActivationAbilityType != MobActivationAbilityType.None && (AbilityDescription.GetMobActivationAbilityType(ability) & this.mobActivationAbilityType) != this.mobActivationAbilityType)
				{
					return false;
				}
				GameAbilityUsingContext gameAbilityUsingContext;
				if (this.abilityUsingContext != GameAbilityUsingContext.Auto && (!ability.TryGetExplicitUsingContext(out gameAbilityUsingContext) || gameAbilityUsingContext != this.abilityUsingContext))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400102C RID: 4140
		public static readonly AbilityDescription AnyAbility = new AbilityDescription
		{
			isAny = true
		};

		// Token: 0x0400102D RID: 4141
		private static AbilityDescription blankDescription = new AbilityDescription
		{
			abilityID = AbilityID.None,
			abilityType = AbilityTypes.None,
			mobActivationAbilityType = MobActivationAbilityType.None,
			abilityUsingContext = GameAbilityUsingContext.Auto
		};

		// Token: 0x0400102E RID: 4142
		public bool isAny;

		// Token: 0x0400102F RID: 4143
		[NonSerialized]
		public BaseAbility abilityReference;

		// Token: 0x04001030 RID: 4144
		public BaseAbility abilityPrototypeReference;

		// Token: 0x04001031 RID: 4145
		public BaseAbility parentAbilityPrototypeReference;

		// Token: 0x04001032 RID: 4146
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID abilityID;

		// Token: 0x04001033 RID: 4147
		public AbilityTypes abilityType;

		// Token: 0x04001034 RID: 4148
		public bool isProjectileAbility;

		// Token: 0x04001035 RID: 4149
		public bool isZoneEffectAbility;

		// Token: 0x04001036 RID: 4150
		public bool isPostMortemAbility;

		// Token: 0x04001037 RID: 4151
		public bool isMobActivationAbility;

		// Token: 0x04001038 RID: 4152
		public bool isSacrificationAbility;

		// Token: 0x04001039 RID: 4153
		public MobActivationAbilityType mobActivationAbilityType;

		// Token: 0x0400103A RID: 4154
		public GameAbilityUsingContext abilityUsingContext;
	}
}
