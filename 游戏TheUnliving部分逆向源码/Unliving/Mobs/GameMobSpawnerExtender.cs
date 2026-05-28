using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Buffs;
using Game.LevelGeneration;
using Game.Stats;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001BA RID: 442
	[DisallowMultipleComponent]
	public sealed class GameMobSpawnerExtender : MonoBehaviour
	{
		// Token: 0x06000D95 RID: 3477 RVA: 0x0002AFA0 File Offset: 0x000291A0
		private static void ModifyRandomSpawnedMobs(GameMobsGroupControllerBase spawnedGroup, int maxAffectedMobs, Func<BaseGameMob, int, bool> action)
		{
			IReadOnlyList<BaseGameMob> mobs = spawnedGroup.Mobs;
			int count = mobs.Count;
			int num = (maxAffectedMobs <= 0 || maxAffectedMobs > count) ? count : maxAffectedMobs;
			GameMobSpawnerExtender.AffectedMobsBuffer.Clear();
			while (num != 0 && GameMobSpawnerExtender.AffectedMobsBuffer.Count < count)
			{
				int num2 = UnityEngine.Random.Range(0, count);
				if (GameMobSpawnerExtender.AffectedMobsBuffer.Add(num2) && action(mobs[num2], num))
				{
					num--;
				}
			}
		}

		// Token: 0x1700028C RID: 652
		// (get) Token: 0x06000D96 RID: 3478 RVA: 0x0002B00D File Offset: 0x0002920D
		// (set) Token: 0x06000D97 RID: 3479 RVA: 0x0002B02A File Offset: 0x0002922A
		public ILocationChunk CurrentChunk
		{
			get
			{
				ILocationChunk currentLocationChunk;
				if ((currentLocationChunk = this.currentChunk) == null)
				{
					MobBehaviourSpawner mobBehaviourSpawner = this.targetSpawner;
					if (mobBehaviourSpawner == null)
					{
						return null;
					}
					currentLocationChunk = ((ILocationObject)mobBehaviourSpawner).CurrentLocationChunk;
				}
				return currentLocationChunk;
			}
			set
			{
				this.currentChunk = value;
			}
		}

		// Token: 0x06000D98 RID: 3480 RVA: 0x0002B034 File Offset: 0x00029234
		private void ApplyStatsModifiers(BaseGameMob spawnedMob)
		{
			StatsControllerBase<MobStatModifier> statsController = spawnedMob.StatsController;
			MobBehaviour mobBehaviour = spawnedMob as MobBehaviour;
			TargetedMobStatModifier[] array = (mobBehaviour != null && mobBehaviour.IsBoss) ? this.spawnedBossesStatsModifiers : this.spawnedMobsStatsModifiers;
			if (array != null && statsController != null)
			{
				foreach (TargetedMobStatModifier targetedMobStatModifier in array)
				{
					statsController.AddModifier((int)targetedMobStatModifier.targetStat, targetedMobStatModifier);
				}
			}
		}

		// Token: 0x06000D99 RID: 3481 RVA: 0x0002B09C File Offset: 0x0002929C
		private bool SpawnAdditionalMob(BaseGameMob spawnedMob, int remainingMobsCount)
		{
			Vector2 v = spawnedMob.Position + new Vector2(UnityEngine.Random.value * 2f - 1f, UnityEngine.Random.value * 2f - 1f).normalized * spawnedMob.Radius;
			MobBehaviour mobBehaviour = this.targetSpawner.SpawnIndividualMob(this.additionalMobsToSpawn[this.additionalMobsToSpawn.Length - remainingMobsCount], v);
			if (mobBehaviour != null)
			{
				spawnedMob.Group.AddMob(mobBehaviour, null);
			}
			return true;
		}

		// Token: 0x06000D9A RID: 3482 RVA: 0x0002B12C File Offset: 0x0002932C
		private void TryPlaceSpawnedGroup(GameMobsGroupControllerBase spawnedGroup)
		{
			if (this.groupPlacementArea == null)
			{
				return;
			}
			IReadOnlyList<BaseGameMob> mobs = spawnedGroup.Mobs;
			int count = mobs.Count;
			int i = 0;
			while (i < count)
			{
				Vector2 randomPointInCollider = this.groupPlacementArea.GetRandomPointInCollider();
				if (this.maxMobsPerPlacementPoint - this.minMobsPerPlacementPoint < 2)
				{
					mobs[i].Position = randomPointInCollider;
					i++;
				}
				else
				{
					int num = Mathf.Min(UnityEngine.Random.Range(this.minMobsPerPlacementPoint, this.maxMobsPerPlacementPoint), count - i);
					for (int j = 0; j < num; j++)
					{
						BaseGameMob baseGameMob = mobs[i + j];
						baseGameMob.Position = randomPointInCollider;
						MobBehaviourSpawner.AddRandomOffset(baseGameMob);
					}
					i += num;
				}
			}
		}

		// Token: 0x06000D9B RID: 3483 RVA: 0x0002B1D4 File Offset: 0x000293D4
		private void OnMobSpawned(BaseGameMob spawnedMob)
		{
			this.ApplyStatsModifiers(spawnedMob);
			if (this.buffGeneratorsData != null)
			{
				for (int i = 0; i < this.buffGeneratorsData.Length; i++)
				{
					this.buffGeneratorsData[i].ApplyBuffs(this, spawnedMob);
				}
			}
		}

		// Token: 0x06000D9C RID: 3484 RVA: 0x0002B212 File Offset: 0x00029412
		private void OnGroupSpawned(GameMobsGroupControllerBase spawnedGroup)
		{
			this.TryPlaceSpawnedGroup(spawnedGroup);
			if (this.additionalMobsToSpawn != null && this.additionalMobsToSpawn.Length != 0)
			{
				GameMobSpawnerExtender.ModifyRandomSpawnedMobs(spawnedGroup, this.additionalMobsToSpawn.Length, new Func<BaseGameMob, int, bool>(this.SpawnAdditionalMob));
			}
		}

		// Token: 0x06000D9D RID: 3485 RVA: 0x0002B248 File Offset: 0x00029448
		private void Start()
		{
			if (this.targetSpawner != null || base.TryGetComponent<MobBehaviourSpawner>(out this.targetSpawner))
			{
				GameMobsGroupControllerBase spawnedGroup = this.targetSpawner.SpawnedGroup;
				IReadOnlyList<BaseGameMob> readOnlyList = (spawnedGroup != null) ? spawnedGroup.Mobs : null;
				if (readOnlyList != null)
				{
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						this.OnMobSpawned(readOnlyList[i]);
					}
				}
				if (this.targetSpawner.IsGroupSpawned)
				{
					this.OnGroupSpawned(this.targetSpawner.SpawnedGroup);
				}
				else
				{
					this.targetSpawner.GroupSpawned += this.OnGroupSpawned;
				}
				this.targetSpawner.GroupMobSpawned += this.OnMobSpawned;
			}
		}

		// Token: 0x06000D9E RID: 3486 RVA: 0x0002B2FB File Offset: 0x000294FB
		private void OnDestroy()
		{
			if (this.targetSpawner != null)
			{
				this.targetSpawner.GroupMobSpawned -= this.OnMobSpawned;
				this.targetSpawner.GroupSpawned -= this.OnGroupSpawned;
			}
		}

		// Token: 0x040007CE RID: 1998
		private static readonly HashSet<int> AffectedMobsBuffer = new HashSet<int>();

		// Token: 0x040007CF RID: 1999
		public MobBehaviourSpawner targetSpawner;

		// Token: 0x040007D0 RID: 2000
		[Space(5f)]
		public TargetedMobStatModifier[] spawnedMobsStatsModifiers;

		// Token: 0x040007D1 RID: 2001
		public TargetedMobStatModifier[] spawnedBossesStatsModifiers;

		// Token: 0x040007D2 RID: 2002
		[Space(5f)]
		public GameMobSpawnerExtender.BuffsData[] buffGeneratorsData;

		// Token: 0x040007D3 RID: 2003
		[Space(5f)]
		public MobBehaviour.ID[] additionalMobsToSpawn;

		// Token: 0x040007D4 RID: 2004
		[Space(5f)]
		public Collider2D groupPlacementArea;

		// Token: 0x040007D5 RID: 2005
		public int minMobsPerPlacementPoint;

		// Token: 0x040007D6 RID: 2006
		public int maxMobsPerPlacementPoint;

		// Token: 0x040007D7 RID: 2007
		private ILocationChunk currentChunk;

		// Token: 0x02000486 RID: 1158
		[Serializable]
		public sealed class BuffsData
		{
			// Token: 0x17000753 RID: 1875
			// (get) Token: 0x06002422 RID: 9250 RVA: 0x0006FD95 File Offset: 0x0006DF95
			public int BuffTargetsCount
			{
				get
				{
					return this.buffTargetsCount;
				}
			}

			// Token: 0x06002423 RID: 9251 RVA: 0x0006FDA0 File Offset: 0x0006DFA0
			public bool TryAddBuffTargetsCount(int count, out int addedCount)
			{
				if (this.maxBuffTargetsCount <= 0)
				{
					this.buffTargetsCount += count;
					addedCount = count;
					return true;
				}
				addedCount = Mathf.Min(count, this.maxBuffTargetsCount - this.buffTargetsCount);
				if (addedCount > 0)
				{
					this.buffTargetsCount += addedCount;
					return true;
				}
				addedCount = 0;
				return false;
			}

			// Token: 0x06002424 RID: 9252 RVA: 0x0006FDF8 File Offset: 0x0006DFF8
			public void ApplyBuffs(GameMobSpawnerExtender spawnerExtender, BaseGameMob targetMob)
			{
				int num = spawnerExtender.targetSpawner.ForceGetInitialSpawningCount();
				float num2 = Mathf.Clamp01((float)this.buffTargetsCount / (float)num);
				if (UnityEngine.Random.value <= num2)
				{
					if (this.instantiatedBuffsGenerators == null)
					{
						BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffGeneratorAssets;
						generatorsBuilders.Instantiate(out this.instantiatedBuffsGenerators);
					}
					for (int i = 0; i < this.instantiatedBuffsGenerators.Length; i++)
					{
						IBuffsController buffsController = targetMob.BuffsController;
						if (buffsController != null)
						{
							IBuffsGenerator buffsGenerator = this.instantiatedBuffsGenerators[i];
							buffsController.AddBuff((buffsGenerator != null) ? buffsGenerator.GenerateBuff(spawnerExtender, false) : null);
						}
					}
				}
			}

			// Token: 0x040017AD RID: 6061
			public BuffsGeneratorBuilderAsset.Reference[] buffGeneratorAssets;

			// Token: 0x040017AE RID: 6062
			[SerializeField]
			private int buffTargetsCount;

			// Token: 0x040017AF RID: 6063
			[NonSerialized]
			public int maxBuffTargetsCount;

			// Token: 0x040017B0 RID: 6064
			private IBuffsGenerator[] instantiatedBuffsGenerators;
		}
	}
}
