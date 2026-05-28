using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Factories;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Stats;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x020003A4 RID: 932
	public class GameAbilitiesController : BaseAbilitiesController, IGameAbilitiesController, IAbilitiesController
	{
		// Token: 0x06001E9B RID: 7835 RVA: 0x00060C30 File Offset: 0x0005EE30
		public static void HandleAbilityToMobStatsAttachment(IStatsController<MobStatModifier> statsController, IAbility ability, bool attachAbilityStats)
		{
			if (ability == null)
			{
				return;
			}
			if (statsController == null)
			{
				return;
			}
			IMobStatsListProvider mobStatsListProvider = ability as IMobStatsListProvider;
			IReadOnlyList<IModifiableStat<MobStatModifier>> readOnlyList = (mobStatsListProvider != null) ? mobStatsListProvider.Stats : null;
			if (readOnlyList != null)
			{
				ProxyStat<MobStatModifier> parentStat = null;
				ProxyStat<MobStatModifier> parentStat2 = null;
				ProxyStat<MobStatModifier> parentStat3 = null;
				ProxyStat<MobStatModifier> parentStat4 = null;
				ProxyStat<MobStatModifier> parentStat5 = null;
				ProxyStat<MobStatModifier> parentStat6 = null;
				ProxyStat<MobStatModifier> parentStat7 = null;
				ProxyStat<MobStatModifier> parentStat8 = null;
				ProxyStat<MobStatModifier> parentStat9 = null;
				ProxyStat<MobStatModifier> parentStat10 = null;
				ProxyStat<MobStatModifier> orCreateProxyStat = statsController.GetOrCreateProxyStat(MobStatID.MobDamage);
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					IModifiableStat<MobStatModifier> modifiableStat = readOnlyList[i];
					MobStatID id = (MobStatID)modifiableStat.ID;
					if (id <= MobStatID.MobAttackSpeed)
					{
						if (id != MobStatID.MobDamage)
						{
							if (id != MobStatID.AbilityEffectsPower)
							{
								if (id == MobStatID.MobAttackSpeed)
								{
									statsController.GetOrCreateProxyStat(MobStatID.MobAttackSpeed, ref parentStat8);
									GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat8, attachAbilityStats);
								}
							}
							else
							{
								statsController.GetOrCreateProxyStat(MobStatID.PlayerAbilitiesPower, ref parentStat7);
								GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat7, attachAbilityStats);
							}
						}
						else
						{
							GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, orCreateProxyStat, attachAbilityStats);
						}
					}
					else
					{
						switch (id)
						{
						case MobStatID.MainPlayerDamage:
							statsController.GetOrCreateProxyStat(MobStatID.MainPlayerDamage, ref parentStat3);
							GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat3, attachAbilityStats);
							break;
						case MobStatID.MainPlayerMeleeDamage:
							statsController.GetOrCreateProxyStat(MobStatID.MainPlayerMeleeDamage, ref parentStat4);
							GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat4, attachAbilityStats);
							break;
						case MobStatID.MainPlayerRangedDamage:
							statsController.GetOrCreateProxyStat(MobStatID.MainPlayerRangedDamage, ref parentStat5);
							GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat5, attachAbilityStats);
							break;
						case MobStatID.MobActivationCost:
							break;
						case MobStatID.MobActivationDamage:
							statsController.GetOrCreateProxyStat(MobStatID.MobActivationDamage, ref parentStat9);
							GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat9, attachAbilityStats);
							break;
						default:
							switch (id)
							{
							case MobStatID.SlotPlayerAbilitiesDamage:
								statsController.GetOrCreateProxyStat(MobStatID.SlotPlayerAbilitiesDamage, ref parentStat6);
								GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat6, attachAbilityStats);
								break;
							case MobStatID.AbilityUsingCost:
								statsController.GetOrCreateProxyStat(MobStatID.AbilityUsingCost, ref parentStat2);
								GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat2, attachAbilityStats);
								break;
							case MobStatID.AbilityCooldown:
								statsController.GetOrCreateProxyStat(MobStatID.AbilityCooldown, ref parentStat);
								GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat, attachAbilityStats);
								break;
							default:
								if (id == MobStatID.MobActivationBuffsDuration)
								{
									statsController.GetOrCreateProxyStat(MobStatID.MobActivationBuffsDuration, ref parentStat10);
									GameAbilitiesController.<HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(modifiableStat, parentStat10, attachAbilityStats);
								}
								break;
							}
							break;
						}
					}
				}
			}
		}

		// Token: 0x17000632 RID: 1586
		// (get) Token: 0x06001E9C RID: 7836 RVA: 0x00060E0E File Offset: 0x0005F00E
		// (set) Token: 0x06001E9D RID: 7837 RVA: 0x00060E16 File Offset: 0x0005F016
		public virtual int UsableAbilitiesCount
		{
			get
			{
				return this.usableAbilitiesCount;
			}
			protected set
			{
				this.usableAbilitiesCount = value;
			}
		}

		// Token: 0x14000117 RID: 279
		// (add) Token: 0x06001E9E RID: 7838 RVA: 0x00060E20 File Offset: 0x0005F020
		// (remove) Token: 0x06001E9F RID: 7839 RVA: 0x00060E58 File Offset: 0x0005F058
		public event Action<GameAbilitiesController, BaseAbility> AbilityReloaded;

		// Token: 0x14000118 RID: 280
		// (add) Token: 0x06001EA0 RID: 7840 RVA: 0x00060E90 File Offset: 0x0005F090
		// (remove) Token: 0x06001EA1 RID: 7841 RVA: 0x00060EC8 File Offset: 0x0005F0C8
		public event Action AllowedAbilityDescriptionChanged;

		// Token: 0x06001EA2 RID: 7842 RVA: 0x00060F00 File Offset: 0x0005F100
		private void TrySetUnallowedAbilityBlocked(BaseAbility ability, bool isBlocked, bool force = false)
		{
			if (isBlocked)
			{
				if (this.IsUnallowedAbility(ability) && this.blockedUnallowedAbilities.Add(ability))
				{
					ability.SetBlocked(true);
					return;
				}
			}
			else if ((force || !this.IsUnallowedAbility(ability)) && this.blockedUnallowedAbilities.Remove(ability))
			{
				ability.SetBlocked(false);
			}
		}

		// Token: 0x06001EA3 RID: 7843 RVA: 0x00060F50 File Offset: 0x0005F150
		public GameAbilitiesController(BaseGameMob abilitiesOwner, IGameAbilitiesFactory abilitiesFactory) : base(abilitiesOwner)
		{
			this.AbilitiesFactory = abilitiesFactory;
			this.usableAbilitiesCount = 0;
		}

		// Token: 0x06001EA4 RID: 7844 RVA: 0x00060F72 File Offset: 0x0005F172
		protected AbilityFactoryPrototype GetAbilityPrototype(int abilityID)
		{
			PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility> prototypeBasedFactory = this.AbilitiesFactory as PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility>;
			if (prototypeBasedFactory == null)
			{
				return null;
			}
			return prototypeBasedFactory.GetObjectPrototype(abilityID);
		}

		// Token: 0x06001EA5 RID: 7845 RVA: 0x00060F8C File Offset: 0x0005F18C
		protected override void AddAbilityToList(List<BaseAbility> abilities, BaseAbility ability)
		{
			if (ability.ShouldBeActivatedByOwner())
			{
				int num = this.UsableAbilitiesCount;
				this.UsableAbilitiesCount = num + 1;
				abilities.Insert(num, ability);
				return;
			}
			abilities.Add(ability);
		}

		// Token: 0x06001EA6 RID: 7846 RVA: 0x00060FC4 File Offset: 0x0005F1C4
		protected BaseAbility CreateAbility(IGameAbilitiesFactory factory, ref AbilityInfo abilityDescription)
		{
			if (factory != null)
			{
				GameAbilitiesController.AbilityFactoryArgs.Update(abilityDescription);
				GameAbilitiesController.AbilityFactoryArgs.preventRandomSpecialBehaviourGeneration = true;
				GameAbilitiesController.AbilityFactoryArgs.abilityOwner = this.abilitiesOwner;
				return (BaseAbility)factory.Create(GameAbilitiesController.AbilityFactoryArgs);
			}
			return null;
		}

		// Token: 0x06001EA7 RID: 7847 RVA: 0x00061011 File Offset: 0x0005F211
		protected BaseAbility CreateAbility(ref AbilityInfo abilityDescription)
		{
			return this.CreateAbility(this.AbilitiesFactory, ref abilityDescription);
		}

		// Token: 0x06001EA8 RID: 7848 RVA: 0x00061020 File Offset: 0x0005F220
		public bool IsUsableAbility(BaseAbility ability)
		{
			int num = this.UsableAbilitiesCount;
			if (num <= 0)
			{
				return false;
			}
			int num2 = this.abilities.IndexOf(ability);
			return num2 != -1 && num2 < num;
		}

		// Token: 0x06001EA9 RID: 7849 RVA: 0x00061054 File Offset: 0x0005F254
		public void RegisterAbilitiesStats()
		{
			IStatsOwner<MobStatModifier> statsOwner = this.abilitiesOwner as IStatsOwner<MobStatModifier>;
			IStatsController<MobStatModifier> statsController = (statsOwner != null) ? statsOwner.StatsController : null;
			if (statsController == null)
			{
				return;
			}
			foreach (BaseAbility ability in this.abilities)
			{
				GameAbilitiesController.HandleAbilityToMobStatsAttachment(statsController, ability, true);
			}
		}

		// Token: 0x06001EAA RID: 7850 RVA: 0x000610C4 File Offset: 0x0005F2C4
		public virtual bool IsSpecialAbility(int abilityID)
		{
			return false;
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x000610C8 File Offset: 0x0005F2C8
		public BaseAbility AddAbility(AbilityInfo abilityDescription)
		{
			BaseAbility baseAbility = this.CreateAbility(ref abilityDescription);
			if (!base.AddAbility(baseAbility))
			{
				return null;
			}
			return baseAbility;
		}

		// Token: 0x06001EAC RID: 7852 RVA: 0x000610EC File Offset: 0x0005F2EC
		public bool RemoveAbility(AbilityInfo abilityDescription)
		{
			for (int i = 0; i < this.abilities.Count; i++)
			{
				BaseAbility ability = this.abilities[i];
				if (abilityDescription.IsMatch(ability) && base.RemoveAbilityAt(i))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EAD RID: 7853 RVA: 0x00061134 File Offset: 0x0005F334
		private void BlockUnallowedAbilities()
		{
			for (int i = 0; i < this.abilities.Count; i++)
			{
				BaseAbility ability = this.abilities[i];
				if (!ability.IsPrepInProgress() && !ability.IsBusy())
				{
					this.TrySetUnallowedAbilityBlocked(ability, true, false);
				}
			}
		}

		// Token: 0x06001EAE RID: 7854 RVA: 0x0006117D File Offset: 0x0005F37D
		public void SetAllowedAbilityDescription(AbilityDescription abilityDescription, bool blockCurrentAbilities = true)
		{
			if (abilityDescription.IsBlank())
			{
				return;
			}
			this.allowedAbilitiesDescription = new AbilityDescription?(abilityDescription);
			if (blockCurrentAbilities)
			{
				this.BlockUnallowedAbilities();
			}
		}

		// Token: 0x06001EAF RID: 7855 RVA: 0x000611A0 File Offset: 0x0005F3A0
		public void ResetAllowedAbilitiesDescription()
		{
			this.allowedAbilitiesDescription = null;
			for (int i = 0; i < this.abilities.Count; i++)
			{
				this.TrySetUnallowedAbilityBlocked(this.abilities[i], false, false);
			}
		}

		// Token: 0x06001EB0 RID: 7856 RVA: 0x000611E4 File Offset: 0x0005F3E4
		public void AddUnallowedAbilitiesDescription(AbilityDescription abilityDescription, bool blockCurrentAbilities = true)
		{
			if (abilityDescription.IsBlank())
			{
				return;
			}
			if (this.unallowedAbilitiesDescriptions == null)
			{
				this.unallowedAbilitiesDescriptions = new AbilityDescription[4];
			}
			else if (this.unallowedAbilitiesDescriptionsCount == this.unallowedAbilitiesDescriptions.Length)
			{
				Array.Resize<AbilityDescription>(ref this.unallowedAbilitiesDescriptions, this.unallowedAbilitiesDescriptionsCount * 2);
			}
			AbilityDescription[] array = this.unallowedAbilitiesDescriptions;
			int num = this.unallowedAbilitiesDescriptionsCount;
			this.unallowedAbilitiesDescriptionsCount = num + 1;
			array[num] = abilityDescription;
			if (blockCurrentAbilities)
			{
				this.BlockUnallowedAbilities();
			}
			Action allowedAbilityDescriptionChanged = this.AllowedAbilityDescriptionChanged;
			if (allowedAbilityDescriptionChanged == null)
			{
				return;
			}
			allowedAbilityDescriptionChanged();
		}

		// Token: 0x06001EB1 RID: 7857 RVA: 0x0006126C File Offset: 0x0005F46C
		public bool RemoveUnallowedAbilitiesDescription(AbilityDescription abilityDescription)
		{
			if (this.unallowedAbilitiesDescriptionsCount != 0 && abilityDescription != default(AbilityDescription))
			{
				int num = Array.IndexOf<AbilityDescription>(this.unallowedAbilitiesDescriptions, abilityDescription);
				if (num >= 0)
				{
					int num2 = this.unallowedAbilitiesDescriptionsCount - 1;
					this.unallowedAbilitiesDescriptionsCount = num2;
					if (num2 > 0 && this.unallowedAbilitiesDescriptionsCount != num)
					{
						this.unallowedAbilitiesDescriptions[num] = this.unallowedAbilitiesDescriptions[this.unallowedAbilitiesDescriptionsCount];
					}
					for (int i = 0; i < this.abilities.Count; i++)
					{
						this.TrySetUnallowedAbilityBlocked(this.abilities[i], false, false);
					}
					Action allowedAbilityDescriptionChanged = this.AllowedAbilityDescriptionChanged;
					if (allowedAbilityDescriptionChanged != null)
					{
						allowedAbilityDescriptionChanged();
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EB2 RID: 7858 RVA: 0x00061324 File Offset: 0x0005F524
		public bool IsUnallowedAbility(AbilityID abilityID)
		{
			for (int i = 0; i < this.abilities.Count; i++)
			{
				BaseAbility baseAbility = this.abilities[i];
				if (!baseAbility.IsNull() && baseAbility.ID == (int)abilityID)
				{
					return this.IsUnallowedAbility(baseAbility);
				}
			}
			return false;
		}

		// Token: 0x06001EB3 RID: 7859 RVA: 0x00061370 File Offset: 0x0005F570
		public bool IsUnallowedAbility(BaseAbility ability)
		{
			if (this.allowedAbilitiesDescription != null && !this.allowedAbilitiesDescription.Value.IsMatch(ability))
			{
				return true;
			}
			if (this.unallowedAbilitiesDescriptionsCount != 0)
			{
				for (int i = 0; i < this.unallowedAbilitiesDescriptionsCount; i++)
				{
					if (this.unallowedAbilitiesDescriptions[i].IsMatch(ability))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001EB4 RID: 7860 RVA: 0x000613D2 File Offset: 0x0005F5D2
		protected virtual void OnAbilityReloaded(BaseAbility ability)
		{
			Action<GameAbilitiesController, BaseAbility> abilityReloaded = this.AbilityReloaded;
			if (abilityReloaded == null)
			{
				return;
			}
			abilityReloaded(this, ability);
		}

		// Token: 0x06001EB5 RID: 7861 RVA: 0x000613E6 File Offset: 0x0005F5E6
		protected override void OnAbilityAdded(BaseAbility ability)
		{
			this.TrySetUnallowedAbilityBlocked(ability, true, false);
			base.OnAbilityAdded(ability);
			ability.Reloaded += this.OnAbilityReloaded;
		}

		// Token: 0x06001EB6 RID: 7862 RVA: 0x0006140C File Offset: 0x0005F60C
		protected override void OnAbilityRemoved(BaseAbility ability, int abilityIndex)
		{
			ability.Reloaded -= this.OnAbilityReloaded;
			this.TrySetUnallowedAbilityBlocked(ability, false, true);
			if (this.UsableAbilitiesCount > 0 && ability.ShouldBeActivatedByOwner())
			{
				int num = this.UsableAbilitiesCount;
				this.UsableAbilitiesCount = num - 1;
			}
			base.OnAbilityRemoved(ability, abilityIndex);
		}

		// Token: 0x06001EB7 RID: 7863 RVA: 0x0006145E File Offset: 0x0005F65E
		protected override void OnAbilityCompleted(IAbility ability, object abilityUsingArgs)
		{
			this.TrySetUnallowedAbilityBlocked((BaseAbility)ability, true, false);
			base.OnAbilityCompleted(ability, abilityUsingArgs);
		}

		// Token: 0x06001EB9 RID: 7865 RVA: 0x00061482 File Offset: 0x0005F682
		[CompilerGenerated]
		internal static void <HandleAbilityToMobStatsAttachment>g__HandleStat|1_0(IModifiableStat<MobStatModifier> childStat, ProxyStat<MobStatModifier> parentStat, bool add)
		{
			if (add)
			{
				if (parentStat != null)
				{
					parentStat.AddStat(childStat);
					return;
				}
			}
			else if (parentStat != null)
			{
				parentStat.RemoveStat(childStat, false);
			}
		}

		// Token: 0x0400139D RID: 5021
		private static readonly AbilityFactoryArgs AbilityFactoryArgs = new AbilityFactoryArgs();

		// Token: 0x040013A0 RID: 5024
		protected readonly IGameAbilitiesFactory AbilitiesFactory;

		// Token: 0x040013A1 RID: 5025
		private readonly HashSet<BaseAbility> blockedUnallowedAbilities = new HashSet<BaseAbility>();

		// Token: 0x040013A2 RID: 5026
		private int usableAbilitiesCount;

		// Token: 0x040013A3 RID: 5027
		private AbilityDescription? allowedAbilitiesDescription;

		// Token: 0x040013A4 RID: 5028
		private AbilityDescription[] unallowedAbilitiesDescriptions;

		// Token: 0x040013A5 RID: 5029
		private int unallowedAbilitiesDescriptionsCount;
	}
}
