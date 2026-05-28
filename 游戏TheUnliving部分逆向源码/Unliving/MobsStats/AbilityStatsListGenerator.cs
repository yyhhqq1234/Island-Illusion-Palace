using System;
using System.Collections.Generic;
using Common;
using Game.Abilities;
using Game.Buffs;
using Game.Damage;
using Game.Stats;
using Unliving.Abilities;
using Unliving.Player;

namespace Unliving.MobsStats
{
	// Token: 0x02000053 RID: 83
	public sealed class AbilityStatsListGenerator
	{
		// Token: 0x17000082 RID: 130
		// (get) Token: 0x060002A0 RID: 672 RVA: 0x0000A5BC File Offset: 0x000087BC
		public List<IModifiableStat<MobStatModifier>> Stats
		{
			get
			{
				if (this.stats == null && this.targetAbility != null)
				{
					this.stats = new List<IModifiableStat<MobStatModifier>>(32)
					{
						new MobAbilityRangeStat(this.targetAbility),
						new MobAbilityCooldownStat(this.targetAbility),
						new MobAbilityUsingSpeedStat(MobStatID.MobAbilityUsingTimeout, this.targetAbility),
						new MobAbilityUsingCostStat(this.targetAbility)
					};
					bool flag = this.targetAbility.IsMobActivationAbility();
					IDamageSender damageSender = this.targetAbility as IDamageSender;
					if (damageSender != null && damageSender.DamageGenerator != null)
					{
						if (flag)
						{
							this.stats.Add(new MobDamageStat(MobStatID.MobActivationDamage, damageSender, damageSender));
						}
						else
						{
							this.stats.Add(new MobDamageStat(damageSender));
							this.stats.Add(new MobAbilityUsingSpeedStat(MobStatID.MobAttackSpeed, this.targetAbility));
							bool flag2 = this.targetAbility.IsPlayerMainBattleAbility();
							if (this.targetAbility.GetController() is IPlayerAbilitiesController)
							{
								if (flag2)
								{
									this.stats.Add(new MobDamageStat(MobStatID.MainPlayerDamage, damageSender, damageSender));
									if (this.targetAbility is ProjectileAbilityBase)
									{
										this.stats.Add(new MobDamageStat(MobStatID.MainPlayerRangedDamage, damageSender, damageSender));
									}
									else
									{
										this.stats.Add(new MobDamageStat(MobStatID.MainPlayerMeleeDamage, damageSender, damageSender));
									}
								}
								else
								{
									this.stats.Add(new MobDamageStat(MobStatID.SlotPlayerAbilitiesDamage, damageSender, damageSender));
								}
							}
						}
					}
					this.AddAbilityEffectPowerStats(this.targetAbility, null);
					if (this.targetAbility.CanGenerateBuffs)
					{
						MobProxyStat mobProxyStat = new MobProxyStat(MobStatID.BuffsDuration, this.targetAbility);
						MobProxyStat mobProxyStat2 = new MobProxyStat(MobStatID.MobActivationBuffsDuration, this.targetAbility);
						foreach (IBuffsGenerator buffsGenerator in this.targetAbility.BuffGenerators)
						{
							mobProxyStat.AddStat(new BuffDurationStat(this.targetAbility, buffsGenerator));
							if (flag)
							{
								mobProxyStat2.AddStat(new BuffDurationStat(this.targetAbility, buffsGenerator));
							}
							this.AddAbilityEffectPowerStats(buffsGenerator, null);
						}
						this.stats.Add(mobProxyStat);
						if (flag)
						{
							this.stats.Add(mobProxyStat2);
						}
					}
					ProjectileAbilityBase projectileAbilityBase = this.targetAbility as ProjectileAbilityBase;
					if (projectileAbilityBase != null)
					{
						this.stats.Add(new ProjectileSpeedStat(projectileAbilityBase));
						this.stats.Add(new ProjectileMaxHitCountStat(projectileAbilityBase));
					}
				}
				return this.stats;
			}
		}

		// Token: 0x060002A1 RID: 673 RVA: 0x0000A814 File Offset: 0x00008A14
		private void AddAbilityEffectsPowerStats(IReadOnlyList<AbilityEffectBase> abilityEffects, Predicate<AbilityEffectBase> effectsFilter = null)
		{
			if (abilityEffects == null)
			{
				return;
			}
			for (int i = 0; i < abilityEffects.Count; i++)
			{
				AbilityEffectBase abilityEffectBase = abilityEffects[i];
				if (effectsFilter == null || effectsFilter(abilityEffectBase))
				{
					IAmountBased amountBased = abilityEffectBase;
					if (amountBased != null && amountBased.Amount > 0f)
					{
						this.stats.Add(new AmountStat(MobStatID.AbilityEffectsPower, this.targetAbility, amountBased));
					}
				}
			}
		}

		// Token: 0x060002A2 RID: 674 RVA: 0x0000A874 File Offset: 0x00008A74
		private void AddAbilityEffectPowerStats(object effectsListProvider, Predicate<AbilityEffectBase> effectsFilter = null)
		{
			IAbilityEffectsListProvider abilityEffectsListProvider = effectsListProvider as IAbilityEffectsListProvider;
			this.AddAbilityEffectsPowerStats((abilityEffectsListProvider != null) ? abilityEffectsListProvider.AbilityEffects : null, effectsFilter);
		}

		// Token: 0x060002A3 RID: 675 RVA: 0x0000A88F File Offset: 0x00008A8F
		public AbilityStatsListGenerator(BaseAbility targetAbility)
		{
			this.targetAbility = targetAbility;
		}

		// Token: 0x0400018D RID: 397
		private readonly BaseAbility targetAbility;

		// Token: 0x0400018E RID: 398
		private List<IModifiableStat<MobStatModifier>> stats;
	}
}
