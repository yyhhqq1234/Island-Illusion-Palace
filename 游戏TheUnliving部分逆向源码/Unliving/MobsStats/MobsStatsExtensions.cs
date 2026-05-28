using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Stats;
using UnityEngine;
using Unliving.GameSession.PlayerLeveling;
using Unliving.Mobs;
using Unliving.Stores;

namespace Unliving.MobsStats
{
	// Token: 0x02000069 RID: 105
	public static class MobsStatsExtensions
	{
		// Token: 0x060002F6 RID: 758 RVA: 0x0000B221 File Offset: 0x00009421
		public static bool IsProjectileStat(this MobStatID statID)
		{
			return statID - MobStatID.MaxProjectileHits <= 2;
		}

		// Token: 0x060002F7 RID: 759 RVA: 0x0000B22D File Offset: 0x0000942D
		public static ProxyStat<MobStatModifier> GetProxyStat(this StatsController statsController, MobStatID statID)
		{
			if (statID != MobStatID.Undefined)
			{
				return statsController.GetStat((int)statID) as ProxyStat<MobStatModifier>;
			}
			return null;
		}

		// Token: 0x060002F8 RID: 760 RVA: 0x0000B244 File Offset: 0x00009444
		public static bool TryGetStat(this object targetObject, MobStatID statID, out IModifiableStat<MobStatModifier> stat)
		{
			stat = null;
			IStatsOwner<MobStatModifier> statsOwner = targetObject as IStatsOwner<MobStatModifier>;
			if (statsOwner != null)
			{
				stat = statsOwner.StatsController.GetStat((int)statID);
			}
			else
			{
				IStatsListProvider<MobStatModifier> statsListProvider = targetObject as IStatsListProvider<MobStatModifier>;
				if (statsListProvider != null)
				{
					IReadOnlyList<IModifiableStat<MobStatModifier>> stats = statsListProvider.Stats;
					for (int i = 0; i < stats.Count; i++)
					{
						IModifiableStat<MobStatModifier> modifiableStat = stats[i];
						if (modifiableStat.ID == (int)statID)
						{
							stat = modifiableStat;
							break;
						}
					}
				}
			}
			return stat != null;
		}

		// Token: 0x060002F9 RID: 761 RVA: 0x0000B2B8 File Offset: 0x000094B8
		public static MobStatModifier GetStatModifiersSum(this IStatsController<MobStatModifier> statsController, MobStatID statID)
		{
			IReadOnlyList<MobStatModifier> statModifiers = statsController.GetStatModifiers((int)statID);
			MobStatModifier mobStatModifier = MobStatModifier.Neutral;
			if (statModifiers != null)
			{
				for (int i = 0; i < statModifiers.Count; i++)
				{
					mobStatModifier += statModifiers[i];
				}
			}
			return mobStatModifier;
		}

		// Token: 0x060002FA RID: 762 RVA: 0x0000B2F8 File Offset: 0x000094F8
		public static MobStatModifier GetTotalDamageModifier(this IGameMob statsOwner)
		{
			IStatsOwner<MobStatModifier> statsOwner2 = statsOwner as IStatsOwner<MobStatModifier>;
			IStatsController<MobStatModifier> statsController = (statsOwner2 != null) ? statsOwner2.StatsController : null;
			MobStatModifier mobStatModifier = MobStatModifier.Neutral;
			if (statsController != null)
			{
				if (statsOwner.IsSacrificed)
				{
					mobStatModifier += statsController.GetStatModifiersSum(MobStatID.MobActivationDamage);
				}
				else
				{
					mobStatModifier += statsController.GetStatModifiersSum(MobStatID.MobDamage);
					mobStatModifier += statsController.GetStatModifiersSum(MobStatID.SlotPlayerAbilitiesDamage);
				}
			}
			return mobStatModifier;
		}

		// Token: 0x060002FB RID: 763 RVA: 0x0000B358 File Offset: 0x00009558
		public static MobStatModifier GetOwnerTotalDamageModifier(this IAbility ability)
		{
			return (ability.Owner as IGameMob).GetTotalDamageModifier();
		}

		// Token: 0x060002FC RID: 764 RVA: 0x0000B36C File Offset: 0x0000956C
		public static bool TryGetStatModifier(this IMobStatsModifiersGenerator modifiersGenerator, object targetObject, MobStatID statID, out TargetedMobStatModifier statModifier)
		{
			if (modifiersGenerator != null)
			{
				TargetedMobStatModifier[] array;
				int currentStatsModifiers = modifiersGenerator.GetCurrentStatsModifiers(targetObject, out array);
				for (int i = 0; i < currentStatsModifiers; i++)
				{
					ref TargetedMobStatModifier ptr = ref array[i];
					if (ptr.TargetStatID == (int)statID)
					{
						statModifier = ptr;
						return true;
					}
				}
			}
			statModifier = default(TargetedMobStatModifier);
			return false;
		}

		// Token: 0x060002FD RID: 765 RVA: 0x0000B3C0 File Offset: 0x000095C0
		public static bool TryGetModifiedStatValue(this IMobStatsModifiersGenerator modifiersGenerator, object targetObject, MobStatID statID, out float statValue)
		{
			TargetedMobStatModifier targetedMobStatModifier;
			IModifiableStat<MobStatModifier> modifiableStat;
			if (modifiersGenerator.TryGetStatModifier(targetObject, statID, out targetedMobStatModifier) && targetObject.TryGetStat(statID, out modifiableStat))
			{
				statValue = targetedMobStatModifier.ToStatModifier().GetModifiedStatValue(modifiableStat.CurrentValue);
				return true;
			}
			statValue = float.NaN;
			return false;
		}

		// Token: 0x060002FE RID: 766 RVA: 0x0000B408 File Offset: 0x00009608
		public static float GetRuntimeStatValue(this PermanentUpgradesStoreManager permanentUpgradesStoreManager, MobStatID mobStatID, PlayerLevelRuntimeState runtimeState)
		{
			float num = permanentUpgradesStoreManager.GetStatModificationValue(mobStatID);
			MobStatModifier mobStatModifier;
			if (runtimeState.TryGetStatModifiers(mobStatID, out mobStatModifier))
			{
				num += mobStatModifier.BaseModifier;
			}
			return num;
		}

		// Token: 0x060002FF RID: 767 RVA: 0x0000B434 File Offset: 0x00009634
		public static bool ModifyStat(this IReadOnlyList<IModifiableStat<MobStatModifier>> stats, MobStatID targetStatID, MobStatModifier statModifier, bool addModifier)
		{
			if (stats != null)
			{
				for (int i = 0; i < stats.Count; i++)
				{
					IModifiableStat<MobStatModifier> modifiableStat = stats[i];
					if (modifiableStat.ID == (int)targetStatID)
					{
						if (addModifier)
						{
							modifiableStat.AddModifier(statModifier);
						}
						else
						{
							modifiableStat.RemoveModifier(statModifier);
						}
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06000300 RID: 768 RVA: 0x0000B47E File Offset: 0x0000967E
		public static bool ModifyStat(this IMobStatsListProvider statsListProvider, MobStatID targetStatID, MobStatModifier statModifier, bool addModifier)
		{
			return statsListProvider != null && statsListProvider.Stats.ModifyStat(targetStatID, statModifier, addModifier);
		}

		// Token: 0x06000301 RID: 769 RVA: 0x0000B494 File Offset: 0x00009694
		public static int ModifyStats<TModifier>(this TModifier[] modifiers, IReadOnlyList<IModifiableStat<MobStatModifier>> stats, bool addModifiers, int modifiersCountOverride = -1, Predicate<IStat> statsFilter = null) where TModifier : ITargetedStatModifier<MobStatModifier>
		{
			if (modifiers == null || stats == null || stats.Count == 0)
			{
				return 0;
			}
			int num = modifiers.Length;
			if (modifiersCountOverride > 0)
			{
				num = Mathf.Min(modifiersCountOverride, num);
			}
			bool flag = statsFilter != null;
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				ref TModifier ptr = ref modifiers[i];
				if (ptr.TargetStatID != -1)
				{
					for (int j = 0; j < stats.Count; j++)
					{
						IModifiableStat<MobStatModifier> modifiableStat = stats[j];
						if ((!flag || statsFilter(modifiableStat)) && ptr.TargetStatID == modifiableStat.ID)
						{
							if (addModifiers)
							{
								modifiableStat.AddModifier(ptr.ToStatModifier());
							}
							else
							{
								modifiableStat.RemoveModifier(ptr.ToStatModifier());
							}
							num2 |= 1 << i;
						}
					}
				}
			}
			return num2;
		}

		// Token: 0x06000302 RID: 770 RVA: 0x0000B570 File Offset: 0x00009770
		public static bool PushTempStatModifier(this object statsOwner, TargetedMobStatModifier tempStatModifier)
		{
			ITempMobStatsModifiersReceiver tempMobStatsModifiersReceiver = statsOwner as ITempMobStatsModifiersReceiver;
			if (tempMobStatsModifiersReceiver != null)
			{
				tempMobStatsModifiersReceiver.AddTempStatModifier(tempStatModifier);
				return true;
			}
			return false;
		}

		// Token: 0x06000303 RID: 771 RVA: 0x0000B594 File Offset: 0x00009794
		public static ProxyStat<MobStatModifier> GetOrCreateProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID)
		{
			ProxyStat<MobStatModifier> proxyStat = statsController.GetStat((int)proxyStatID) as ProxyStat<MobStatModifier>;
			if (proxyStat == null)
			{
				proxyStat = new MobProxyStat(proxyStatID, statsController.StatsOwner);
				statsController.AddStat(proxyStat);
			}
			return proxyStat;
		}

		// Token: 0x06000304 RID: 772 RVA: 0x0000B5C7 File Offset: 0x000097C7
		public static ProxyStat<MobStatModifier> GetOrCreateProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID, ref ProxyStat<MobStatModifier> currentStat)
		{
			if (currentStat == null)
			{
				currentStat = statsController.GetOrCreateProxyStat(proxyStatID);
			}
			return currentStat;
		}

		// Token: 0x06000305 RID: 773 RVA: 0x0000B5D8 File Offset: 0x000097D8
		public static void AddToProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID, IReadOnlyList<IModifiableStat<MobStatModifier>> sourceStats, Predicate<IModifiableStat<MobStatModifier>> sourceStatsFilter = null)
		{
			if (sourceStats == null || sourceStats.Count == 0)
			{
				return;
			}
			ProxyStat<MobStatModifier> orCreateProxyStat = statsController.GetOrCreateProxyStat(proxyStatID);
			for (int i = 0; i < sourceStats.Count; i++)
			{
				IModifiableStat<MobStatModifier> modifiableStat = sourceStats[i];
				if (modifiableStat != null && (sourceStatsFilter == null || sourceStatsFilter(modifiableStat)))
				{
					orCreateProxyStat.AddStat(modifiableStat);
				}
			}
		}

		// Token: 0x06000306 RID: 774 RVA: 0x0000B628 File Offset: 0x00009828
		public static void AddToProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID, IStatsController<MobStatModifier> sourceStatsController, MobStatID sourceStatsID)
		{
			statsController.AddToProxyStat(proxyStatID, (sourceStatsController != null) ? sourceStatsController.GetStats((int)sourceStatsID) : null, null);
		}

		// Token: 0x06000307 RID: 775 RVA: 0x0000B640 File Offset: 0x00009840
		public static void RemoveFromProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID, IReadOnlyList<IModifiableStat<MobStatModifier>> sourceStats, bool keepModifiers = false, Predicate<IModifiableStat<MobStatModifier>> sourceStatsFilter = null)
		{
			if (sourceStats == null || sourceStats.Count == 0)
			{
				return;
			}
			IReadOnlyList<IModifiableStat<MobStatModifier>> readOnlyList = (statsController != null) ? statsController.GetStats((int)proxyStatID) : null;
			if (readOnlyList == null || readOnlyList.Count == 0)
			{
				return;
			}
			ProxyStat<MobStatModifier> proxyStat = (ProxyStat<MobStatModifier>)readOnlyList[0];
			for (int i = 0; i < sourceStats.Count; i++)
			{
				IModifiableStat<MobStatModifier> modifiableStat = sourceStats[i];
				if (sourceStatsFilter == null || sourceStatsFilter(modifiableStat))
				{
					proxyStat.RemoveStat(modifiableStat, keepModifiers);
				}
			}
		}

		// Token: 0x06000308 RID: 776 RVA: 0x0000B6AF File Offset: 0x000098AF
		public static void RemoveFromProxyStat(this IStatsController<MobStatModifier> statsController, MobStatID proxyStatID, IStatsController<MobStatModifier> sourceStatsController, MobStatID sourceStatsID, bool keepModifiers = false, Predicate<IModifiableStat<MobStatModifier>> sourceStatsFilter = null)
		{
			statsController.RemoveFromProxyStat(proxyStatID, (sourceStatsController != null) ? sourceStatsController.GetStats((int)sourceStatsID) : null, keepModifiers, sourceStatsFilter);
		}
	}
}
