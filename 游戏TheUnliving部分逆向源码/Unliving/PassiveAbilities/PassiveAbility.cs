using System;
using System.Collections.Generic;
using Common.Editor;
using Game.Abilities;
using Game.PassiveAbilities;
using Game.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A8 RID: 424
	[CreateAssetMenu(fileName = "PassiveAbility", menuName = "Abilities/Passive Ability")]
	public sealed class PassiveAbility : PassiveAbilityBase
	{
		// Token: 0x17000223 RID: 547
		// (get) Token: 0x06000C0C RID: 3084 RVA: 0x00025EC0 File Offset: 0x000240C0
		// (set) Token: 0x06000C0D RID: 3085 RVA: 0x00025EC8 File Offset: 0x000240C8
		public PassiveAbility.StatModifier[] StatModifiers
		{
			get
			{
				return this._statModifiers;
			}
			set
			{
				this._statModifiers = value;
			}
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x06000C0E RID: 3086 RVA: 0x00025ED1 File Offset: 0x000240D1
		// (set) Token: 0x06000C0F RID: 3087 RVA: 0x00025ED9 File Offset: 0x000240D9
		public BaseAbility OwnerActiveAbilityPrototype
		{
			get
			{
				return this._ownerActiveAbilityPrototype;
			}
			set
			{
				this._ownerActiveAbilityPrototype = value;
			}
		}

		// Token: 0x17000225 RID: 549
		// (get) Token: 0x06000C10 RID: 3088 RVA: 0x00025EE2 File Offset: 0x000240E2
		// (set) Token: 0x06000C11 RID: 3089 RVA: 0x00025EEA File Offset: 0x000240EA
		public BaseAbility GroupActiveAbilityPrototype
		{
			get
			{
				return this._groupActiveAbilityPrototype;
			}
			set
			{
				this._groupActiveAbilityPrototype = value;
			}
		}

		// Token: 0x06000C12 RID: 3090 RVA: 0x00025EF4 File Offset: 0x000240F4
		private void SetOwnerGroup(GameMobsGroupControllerBase newOwnerGroup)
		{
			if (this._groupActiveAbilityPrototype != null)
			{
				if (this.ownerGroup != null)
				{
					this.ownerGroup.MobAdded -= this.OnMobAddedToOwnerGroup;
					this.ownerGroup.MobRemoved -= this.OnMobRemovedFromOwnerGroup;
				}
				if (newOwnerGroup != null)
				{
					newOwnerGroup.MobAdded += this.OnMobAddedToOwnerGroup;
					newOwnerGroup.MobRemoved += this.OnMobRemovedFromOwnerGroup;
				}
			}
			this.ownerGroup = newOwnerGroup;
		}

		// Token: 0x06000C13 RID: 3091 RVA: 0x00025F74 File Offset: 0x00024174
		private void ModifyStats(bool addModifiers)
		{
			if (this._statModifiers != null && this._statModifiers.Length != 0)
			{
				IStatsOwner<MobStatModifier> statsOwner = base.AbilityOwner as IStatsOwner<MobStatModifier>;
				if (statsOwner != null)
				{
					IStatsController<MobStatModifier> statsController = statsOwner.StatsController;
					if (statsController != null)
					{
						foreach (PassiveAbility.StatModifier statModifier in this._statModifiers)
						{
							if (addModifiers)
							{
								statsController.AddModifier((int)statModifier.targetStat, statModifier);
							}
							else
							{
								statsController.RemoveModifier((int)statModifier.targetStat, statModifier);
							}
						}
					}
				}
			}
		}

		// Token: 0x06000C14 RID: 3092 RVA: 0x00025FF8 File Offset: 0x000241F8
		private BaseAbility AddActiveAbility(BaseAbility abilityPrototype, BaseGameMob mob)
		{
			if (abilityPrototype != null && ((mob != null) ? mob.AbilitiesController : null) != null)
			{
				BaseAbility baseAbility = (BaseAbility)mob.CurrentGame.Services.Get<IGameAbilitiesFactory>().Create(new AbilityFactoryArgs
				{
					abilityPrototype = abilityPrototype
				});
				if (mob.AbilitiesController.AddAbility(baseAbility))
				{
					return baseAbility;
				}
				baseAbility.Destroy();
			}
			return null;
		}

		// Token: 0x06000C15 RID: 3093 RVA: 0x0002605A File Offset: 0x0002425A
		private void SetGroupMobAbilityActive(BaseGameMob groupMob, bool isActive)
		{
			if (isActive)
			{
				this.AddActiveAbility(this._groupActiveAbilityPrototype, groupMob);
				return;
			}
			if (!groupMob.IsKilled || !this._groupActiveAbilityPrototype.IsPostMortemAbility)
			{
				GameAbilitiesController abilitiesController = groupMob.AbilitiesController;
				if (abilitiesController == null)
				{
					return;
				}
				abilitiesController.RemoveAbilityWithPrototype(this._groupActiveAbilityPrototype);
			}
		}

		// Token: 0x06000C16 RID: 3094 RVA: 0x0002609C File Offset: 0x0002429C
		private void SetActive(bool isActive)
		{
			if (this._groupActiveAbilityPrototype != null)
			{
				GameMobsGroupControllerBase gameMobsGroupControllerBase = this.ownerGroup;
				IReadOnlyList<BaseGameMob> readOnlyList = (gameMobsGroupControllerBase != null) ? gameMobsGroupControllerBase.Mobs : null;
				if (readOnlyList != null)
				{
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						this.SetGroupMobAbilityActive(readOnlyList[i], isActive);
					}
				}
			}
			if (isActive)
			{
				this.addedOwnerAbility = this.AddActiveAbility(this._ownerActiveAbilityPrototype, this.ownerMob);
			}
			else
			{
				if (this.addedOwnerAbility != null)
				{
					BaseGameMob baseGameMob = this.ownerMob;
					if (baseGameMob != null)
					{
						GameAbilitiesController abilitiesController = baseGameMob.AbilitiesController;
						if (abilitiesController != null)
						{
							abilitiesController.RemoveAbility(this.addedOwnerAbility);
						}
					}
				}
				this.addedOwnerAbility = null;
				this.SetOwnerGroup(null);
			}
			this.ModifyStats(isActive);
		}

		// Token: 0x06000C17 RID: 3095 RVA: 0x0002614F File Offset: 0x0002434F
		private void OnMobAddedToOwnerGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.SetGroupMobAbilityActive(mob, true);
		}

		// Token: 0x06000C18 RID: 3096 RVA: 0x00026159 File Offset: 0x00024359
		private void OnMobRemovedFromOwnerGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.SetGroupMobAbilityActive(mob, false);
		}

		// Token: 0x06000C19 RID: 3097 RVA: 0x00026163 File Offset: 0x00024363
		public override void OnAddedToController(IPassiveAbilitiesController controller)
		{
			this.ownerMob = (base.AbilityOwner as BaseGameMob);
			BaseGameMob baseGameMob = this.ownerMob;
			this.SetOwnerGroup((baseGameMob != null) ? baseGameMob.Group : null);
			this.SetActive(true);
		}

		// Token: 0x06000C1A RID: 3098 RVA: 0x00026195 File Offset: 0x00024395
		public override void OnRemovedFromController(IPassiveAbilitiesController controller)
		{
			this.SetActive(false);
		}

		// Token: 0x06000C1B RID: 3099 RVA: 0x0002619E File Offset: 0x0002439E
		protected override void OnDestroy()
		{
			this.SetActive(false);
			base.OnDestroy();
		}

		// Token: 0x040006EC RID: 1772
		[SerializeField]
		[FormerlySerializedAs("statModifiers")]
		private PassiveAbility.StatModifier[] _statModifiers;

		// Token: 0x040006ED RID: 1773
		[SerializeField]
		[FormerlySerializedAs("_activeAbilityToAdd")]
		private BaseAbility _ownerActiveAbilityPrototype;

		// Token: 0x040006EE RID: 1774
		[SerializeField]
		private BaseAbility _groupActiveAbilityPrototype;

		// Token: 0x040006EF RID: 1775
		private BaseGameMob ownerMob;

		// Token: 0x040006F0 RID: 1776
		private GameMobsGroupControllerBase ownerGroup;

		// Token: 0x040006F1 RID: 1777
		private BaseAbility addedOwnerAbility;

		// Token: 0x0200047B RID: 1147
		[Serializable]
		public struct StatModifier : ITargetedStatModifier<MobStatModifier>
		{
			// Token: 0x060023ED RID: 9197 RVA: 0x0006F297 File Offset: 0x0006D497
			public static implicit operator MobStatModifier(PassiveAbility.StatModifier modifier)
			{
				return modifier.ToStatModifier();
			}

			// Token: 0x1700074A RID: 1866
			// (get) Token: 0x060023EE RID: 9198 RVA: 0x0006F2A0 File Offset: 0x0006D4A0
			// (set) Token: 0x060023EF RID: 9199 RVA: 0x0006F2A8 File Offset: 0x0006D4A8
			public int TargetStatID
			{
				get
				{
					return (int)this.targetStat;
				}
				set
				{
					this.targetStat = (MobStatID)value;
				}
			}

			// Token: 0x060023F0 RID: 9200 RVA: 0x0006F2B4 File Offset: 0x0006D4B4
			public TargetedMobStatModifier ToTargetedStatModifier()
			{
				return new TargetedMobStatModifier
				{
					targetStat = this.targetStat,
					modifierType = this.modifierType,
					value = this.value
				};
			}

			// Token: 0x060023F1 RID: 9201 RVA: 0x0006F2F1 File Offset: 0x0006D4F1
			public MobStatModifier ToStatModifier()
			{
				return this.modifierType.ToStatModifier(this.value);
			}

			// Token: 0x060023F2 RID: 9202 RVA: 0x0006F304 File Offset: 0x0006D504
			public void Invalidate()
			{
				this.targetStat = MobStatID.Undefined;
				this.value = 0f;
			}

			// Token: 0x060023F3 RID: 9203 RVA: 0x0006F318 File Offset: 0x0006D518
			public override string ToString()
			{
				return string.Format("{0}: {1}", this.targetStat, this.ToStatModifier());
			}

			// Token: 0x04001780 RID: 6016
			[EnumPopup]
			public MobStatID targetStat;

			// Token: 0x04001781 RID: 6017
			public MobStatModifierType modifierType;

			// Token: 0x04001782 RID: 6018
			public float value;
		}
	}
}
