using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Damage;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.Abilities;
using Unliving.LevelGeneration;

namespace Unliving.Mobs
{
	// Token: 0x020001D3 RID: 467
	public static class GameMobExtensions
	{
		// Token: 0x06000EF0 RID: 3824 RVA: 0x0002F7A2 File Offset: 0x0002D9A2
		public static LocationChunkMobsGridController.GridAgent ForceGetChunkGridAgent(this BaseGameMob mob)
		{
			LocationChunk locationChunk = mob.CurrentLocationChunk as LocationChunk;
			if (locationChunk == null)
			{
				return null;
			}
			LocationChunkMobsGridController mobsGrid = locationChunk.MobsGrid;
			if (mobsGrid == null)
			{
				return null;
			}
			return mobsGrid.GetGridAgent(mob);
		}

		// Token: 0x06000EF1 RID: 3825 RVA: 0x0002F7C6 File Offset: 0x0002D9C6
		public static bool IsLayerInMask(this IGameMob mob, int mask)
		{
			return (mob.LayerMask & mask) != 0;
		}

		// Token: 0x06000EF2 RID: 3826 RVA: 0x0002F7D3 File Offset: 0x0002D9D3
		public static bool IsCharacterByDefault(this IGameMob mob)
		{
			return mob.IsCharacter && !mob.IsSummoned;
		}

		// Token: 0x06000EF3 RID: 3827 RVA: 0x0002F7E8 File Offset: 0x0002D9E8
		public static bool IsUndead(this IGameMob mob)
		{
			return mob.IsRevived || mob.IsPlayerMob;
		}

		// Token: 0x06000EF4 RID: 3828 RVA: 0x0002F7FC File Offset: 0x0002D9FC
		public static bool IsAlive(this IGameMob mob)
		{
			if (!mob.IsNull() && !mob.IsKilled)
			{
				IDestroyable destroyable = mob as IDestroyable;
				if (destroyable == null || !destroyable.IsDestroyed)
				{
					IDamageable hitPointsController = mob.HitPointsController;
					return hitPointsController == null || hitPointsController.IsAlive;
				}
			}
			return false;
		}

		// Token: 0x06000EF5 RID: 3829 RVA: 0x0002F840 File Offset: 0x0002DA40
		public static bool IsSpawnedAsDeadMob(this BaseGameMob mob)
		{
			if (mob == null)
			{
				return false;
			}
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			MobBehaviourSpawner mobBehaviourSpawner = (mobBehaviour != null) ? mobBehaviour.Spawner : null;
			return mobBehaviourSpawner != null && mobBehaviourSpawner.spawnDeadMobs;
		}

		// Token: 0x06000EF6 RID: 3830 RVA: 0x0002F87C File Offset: 0x0002DA7C
		public static bool IsValidReviver(this BaseGameMob reviver, IRevivableGameMob revivableMob)
		{
			if (reviver == null || revivableMob == null || reviver.Group == null)
			{
				return false;
			}
			ILocationChunk currentLocationChunk = reviver.CurrentLocationChunk;
			ILocationObject locationObject = revivableMob as ILocationObject;
			if (locationObject != null)
			{
				return currentLocationChunk.HasPassageToChunk(locationObject.CurrentLocationChunk);
			}
			ILocationChunk locationChunk;
			return currentLocationChunk.HasPassageToChunk(revivableMob.Component.transform.position, out locationChunk);
		}

		// Token: 0x06000EF7 RID: 3831 RVA: 0x0002F8D5 File Offset: 0x0002DAD5
		public static void PrepareRevivedMob(this IGameMob revivedMob, UnityEngine.Object revivingSource, Vector2 targetPosition)
		{
			revivedMob.SetCreationType(GameMobCreationType.Revived, revivingSource);
			revivedMob.Position = targetPosition;
		}

		// Token: 0x06000EF8 RID: 3832 RVA: 0x0002F8E6 File Offset: 0x0002DAE6
		public static void SetTotalBuffsImmunityActive(this IGameMob mob, bool isActive)
		{
			if (mob.BuffsController == null)
			{
				return;
			}
			if (isActive)
			{
				mob.BuffsController.AddBuffsBlocker(GameMobExtensions.TotalBuffsBlocker);
				return;
			}
			mob.BuffsController.RemoveBuffsBlocker(GameMobExtensions.TotalBuffsBlocker);
		}

		// Token: 0x06000EF9 RID: 3833 RVA: 0x0002F918 File Offset: 0x0002DB18
		public static Coroutine StartCoroutine(this IGameMob mob, IEnumerator routine)
		{
			MonoBehaviour monoBehaviour = mob as MonoBehaviour;
			if (monoBehaviour == null)
			{
				return null;
			}
			return monoBehaviour.StartCoroutine(routine);
		}

		// Token: 0x06000EFA RID: 3834 RVA: 0x0002F938 File Offset: 0x0002DB38
		public static void StopCoroutine(this IGameMob mob, Coroutine routine)
		{
			MonoBehaviour monoBehaviour = mob as MonoBehaviour;
			if (monoBehaviour != null)
			{
				monoBehaviour.StopCoroutine(routine);
			}
		}

		// Token: 0x06000EFB RID: 3835 RVA: 0x0002F958 File Offset: 0x0002DB58
		public static bool TryGetMobActivationAbility(this IGameMob mob, out BaseAbility activationAbility, out MobActivationAbilityType mobActivationType)
		{
			mobActivationType = MobActivationAbilityType.None;
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour != null && mobBehaviour.activationType != MobActivationAbilityType.None)
			{
				mobActivationType = mobBehaviour.activationType;
			}
			BaseGameMob baseGameMob = mob as BaseGameMob;
			IReadOnlyList<BaseAbility> readOnlyList;
			if (baseGameMob == null)
			{
				readOnlyList = null;
			}
			else
			{
				GameAbilitiesController abilitiesController = baseGameMob.AbilitiesController;
				readOnlyList = ((abilitiesController != null) ? abilitiesController.Abilities : null);
			}
			IReadOnlyList<BaseAbility> readOnlyList2 = readOnlyList;
			if (readOnlyList2 != null)
			{
				for (int i = 0; i < readOnlyList2.Count; i++)
				{
					BaseAbility baseAbility = readOnlyList2[i];
					MobActivationAbilityType mobActivationAbilityType;
					if (baseAbility.IsMobActivationAbility(out mobActivationAbilityType) && (mobActivationType == MobActivationAbilityType.None || mobActivationType == mobActivationAbilityType))
					{
						mobActivationType = mobActivationAbilityType;
						activationAbility = baseAbility;
						return true;
					}
				}
			}
			activationAbility = null;
			return false;
		}

		// Token: 0x06000EFC RID: 3836 RVA: 0x0002F9E0 File Offset: 0x0002DBE0
		public static bool TryGetMobActivationType(this IGameMob mob, out MobActivationAbilityType mobActivationType)
		{
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour != null && mobBehaviour.activationType != MobActivationAbilityType.None)
			{
				mobActivationType = mobBehaviour.activationType;
				return true;
			}
			BaseAbility baseAbility;
			return mob.TryGetMobActivationAbility(out baseAbility, out mobActivationType);
		}

		// Token: 0x06000EFD RID: 3837 RVA: 0x0002FA14 File Offset: 0x0002DC14
		public static int CopyMobsTo(this GameMobsGroupControllerBase group, IList<BaseGameMob> targetList, int mobsCount = -1, Predicate<BaseGameMob> mobsFilter = null)
		{
			IReadOnlyList<BaseGameMob> mobs = group.Mobs;
			if (mobsCount <= 0 || mobsCount > mobs.Count)
			{
				mobsCount = mobs.Count;
			}
			bool flag = mobsFilter != null;
			IList list = targetList as IList;
			bool flag2 = list == null || list.IsFixedSize;
			int num = 0;
			int num2 = 0;
			while (num2 < mobs.Count && num < mobsCount)
			{
				BaseGameMob baseGameMob = mobs[num2];
				if (!flag || mobsFilter(baseGameMob))
				{
					if (flag2)
					{
						targetList[num] = baseGameMob;
						num++;
					}
					else
					{
						targetList.Add(baseGameMob);
					}
				}
				num2++;
			}
			return num;
		}

		// Token: 0x040008D4 RID: 2260
		private static readonly GameMobExtensions.BuffsBlocker TotalBuffsBlocker = new GameMobExtensions.BuffsBlocker();

		// Token: 0x02000497 RID: 1175
		private sealed class BuffsBlocker : IBuffsBlocker
		{
			// Token: 0x06002459 RID: 9305 RVA: 0x0007095E File Offset: 0x0006EB5E
			public bool CanBlockBuff(IBuff buff)
			{
				return true;
			}

			// Token: 0x0600245A RID: 9306 RVA: 0x00070961 File Offset: 0x0006EB61
			public bool TryBlockBuff(IBuff buff)
			{
				return true;
			}
		}
	}
}
