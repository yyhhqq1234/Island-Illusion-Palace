using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Stats;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.LeveledItems
{
	// Token: 0x0200024C RID: 588
	[CreateAssetMenu(fileName = "AbilityLevelBasedPropertiesModifier", menuName = "Abilities/Leveling/Level Based Properties Modifier")]
	public sealed class AbilityLevelBasedPropertiesModifier : AbilityExtensionAssetBase, IAbilityLevelingController
	{
		// Token: 0x060013C4 RID: 5060 RVA: 0x0003E31C File Offset: 0x0003C51C
		private static bool TryGetParentAbilityLevel(BaseAbility ability, out int parentAbilityLevel)
		{
			IGameMob gameMob = ability.OwnerBehaviour as IGameMob;
			GameMobSummoningContext gameMobSummoningContext = (gameMob != null) ? gameMob.SummonerInfo : null;
			if (gameMobSummoningContext != null)
			{
				if (gameMobSummoningContext.abilitiesLevelOverride > 0)
				{
					parentAbilityLevel = gameMobSummoningContext.abilitiesLevelOverride;
					return true;
				}
				BaseAbility baseAbility = gameMobSummoningContext.summoningSource as BaseAbility;
				if (baseAbility != null && baseAbility.TryGetAbilityLevel(out parentAbilityLevel, 1))
				{
					return true;
				}
			}
			parentAbilityLevel = -1;
			return false;
		}

		// Token: 0x060013C5 RID: 5061 RVA: 0x0003E37C File Offset: 0x0003C57C
		private static void UpdateStatsBaseValue(IReadOnlyList<IModifiableStat<MobStatModifier>> stats)
		{
			AbilityLevelBasedPropertiesModifier.StatsModifiersRestoringBuffer.Clear();
			for (int i = 0; i < stats.Count; i++)
			{
				IModifiableStat<MobStatModifier> modifiableStat = stats[i];
				MobStatBase mobStatBase = modifiableStat as MobStatBase;
				if (mobStatBase != null)
				{
					MobStatModifier currentModifiers = mobStatBase.CurrentModifiers;
					if (!currentModifiers.IsNeutral())
					{
						AbilityLevelBasedPropertiesModifier.StatsModifiersRestoringBuffer.Add(new ValueTuple<int, MobStatModifier>(i, currentModifiers));
					}
					mobStatBase.UpdateInitialValue(false);
				}
				else
				{
					modifiableStat.UpdateInitialValue();
				}
			}
			for (int j = 0; j < AbilityLevelBasedPropertiesModifier.StatsModifiersRestoringBuffer.Count; j++)
			{
				ValueTuple<int, MobStatModifier> valueTuple = AbilityLevelBasedPropertiesModifier.StatsModifiersRestoringBuffer[j];
				int item = valueTuple.Item1;
				MobStatModifier item2 = valueTuple.Item2;
				stats[item].AddModifier(item2);
			}
		}

		// Token: 0x17000436 RID: 1078
		// (get) Token: 0x060013C6 RID: 5062 RVA: 0x0003E429 File Offset: 0x0003C629
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x060013C7 RID: 5063 RVA: 0x0003E42C File Offset: 0x0003C62C
		// (set) Token: 0x060013C8 RID: 5064 RVA: 0x0003E434 File Offset: 0x0003C634
		public AbilityPropertyValuesOverrides[] AbilityPropertiesOverrides
		{
			get
			{
				return this.GetActualPropertiesOverrides();
			}
			set
			{
				this.overriddenPropertiesOverrides = value;
			}
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x0003E43D File Offset: 0x0003C63D
		private AbilityPropertyValuesOverrides[] GetActualPropertiesOverrides()
		{
			return this.overriddenPropertiesOverrides ?? this.abilityPropertiesOverrides;
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x0003E450 File Offset: 0x0003C650
		private async void OverrideAbilityPropertiesAsync(BaseAbility ability)
		{
			int lastFrame = Time.frameCount;
			while (!GameApplication.IsGameStateChanging && ability.CurrentController == null && Time.frameCount - lastFrame < 1)
			{
				await Task.Yield();
			}
			if (!ability.IsNull())
			{
				int abilityLevel;
				if (this.GetAbilityLevel(ability, out abilityLevel))
				{
					AbilityPropertyValuesOverrides[] actualPropertiesOverrides = this.GetActualPropertiesOverrides();
					if (actualPropertiesOverrides != null)
					{
						bool flag = false;
						for (int i = 0; i < actualPropertiesOverrides.Length; i++)
						{
							float value;
							if (actualPropertiesOverrides[i].TryGetPropertyValue(abilityLevel, out value))
							{
								IAbilityPropertyAccessor abilityPropertyAccessor = AbilityPropertiesAccessors.Get(ability, actualPropertiesOverrides[i].PropertyDescription);
								if (abilityPropertyAccessor != null)
								{
									abilityPropertyAccessor.SetValue(ability, value);
								}
								flag = true;
							}
						}
						if (flag)
						{
							IMobStatsListProvider mobStatsListProvider = ability as IMobStatsListProvider;
							if (mobStatsListProvider != null)
							{
								AbilityLevelBasedPropertiesModifier.UpdateStatsBaseValue(mobStatsListProvider.Stats);
							}
						}
					}
				}
			}
		}

		// Token: 0x060013CB RID: 5067 RVA: 0x0003E491 File Offset: 0x0003C691
		public bool GetAbilityLevel(BaseAbility ability, out int level)
		{
			if (!ability.IsInstantiated())
			{
				level = 1;
				return true;
			}
			return this.levelsOverrides.TryGetValue(ability.GetInstanceID(), out level) || (this.tryInheritLevelFromParentAbility && AbilityLevelBasedPropertiesModifier.TryGetParentAbilityLevel(ability, out level)) || ability.TryGetAbilityLevel(out level, 1);
		}

		// Token: 0x060013CC RID: 5068 RVA: 0x0003E4D1 File Offset: 0x0003C6D1
		public void SetAbilityLevelOverride(BaseAbility ability, int level)
		{
			this.levelsOverrides[ability.GetInstanceID()] = level;
		}

		// Token: 0x060013CD RID: 5069 RVA: 0x0003E4E5 File Offset: 0x0003C6E5
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.OverrideAbilityPropertiesAsync(ability);
		}

		// Token: 0x04000B82 RID: 2946
		private static readonly List<ValueTuple<int, MobStatModifier>> StatsModifiersRestoringBuffer = new List<ValueTuple<int, MobStatModifier>>(32);

		// Token: 0x04000B83 RID: 2947
		public bool tryInheritLevelFromParentAbility = true;

		// Token: 0x04000B84 RID: 2948
		[SerializeField]
		[FormerlySerializedAs("propertiesOverrides")]
		private AbilityPropertyValuesOverrides[] abilityPropertiesOverrides;

		// Token: 0x04000B85 RID: 2949
		private readonly Dictionary<int, int> levelsOverrides = new Dictionary<int, int>(16);

		// Token: 0x04000B86 RID: 2950
		[NonSerialized]
		private AbilityPropertyValuesOverrides[] overriddenPropertiesOverrides;
	}
}
