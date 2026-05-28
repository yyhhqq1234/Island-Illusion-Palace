using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Common.CollectionsExtensions;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Buffs;
using Game.Core;
using Game.Factories;
using Game.LevelGeneration;
using RedScarf.EasyCSV;
using UnityEngine;
using Unliving.Gameplay;
using Unliving.GameScene;
using Unliving.Mobs;
using Unliving.MobsStats;
using Unliving.PlayerProfileManagement;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000276 RID: 630
	[Service(typeof(TestDifficultyLevelAdvancer), new Type[]
	{
		typeof(IGameDifficultyController)
	})]
	[DisallowMultipleComponent]
	public sealed class TestDifficultyLevelAdvancer : GlobalSceneManagerBase, IGameDifficultyController
	{
		// Token: 0x06001581 RID: 5505 RVA: 0x00044C74 File Offset: 0x00042E74
		private static MobBehaviourSpawner[] GetMobSpawners(ILocationChunk locationChunk, out int spawnersCount, bool sortByMobsCount)
		{
			spawnersCount = 0;
			GameSessionManager gameSessionManager = TestDifficultyLevelAdvancer.gameSessionManager;
			GameSessionRules gameSessionRules = (gameSessionManager != null) ? gameSessionManager.CurrentGameRules : null;
			foreach (ILocationObject locationObject in locationChunk.EnvironmentObjects)
			{
				MobBehaviourSpawner mobBehaviourSpawner = locationObject as MobBehaviourSpawner;
				if (mobBehaviourSpawner != null && !mobBehaviourSpawner.SpawnEnvironmentMobs && (gameSessionRules == null || gameSessionRules.IsPlayerEnemyFaction(mobBehaviourSpawner.GroupOwner)))
				{
					MobBehaviourSpawner[] mobSpawnersBuffer = TestDifficultyLevelAdvancer.MobSpawnersBuffer;
					int num = spawnersCount;
					spawnersCount = num + 1;
					mobSpawnersBuffer[num] = mobBehaviourSpawner;
				}
			}
			if (sortByMobsCount && spawnersCount != 0)
			{
				Array.Sort<MobBehaviourSpawner>(TestDifficultyLevelAdvancer.MobSpawnersBuffer, 0, spawnersCount, TestDifficultyLevelAdvancer.MobSpawnersSorter.Default);
			}
			return TestDifficultyLevelAdvancer.MobSpawnersBuffer;
		}

		// Token: 0x1700048C RID: 1164
		// (get) Token: 0x06001582 RID: 5506 RVA: 0x00044D24 File Offset: 0x00042F24
		// (set) Token: 0x06001583 RID: 5507 RVA: 0x00044D2C File Offset: 0x00042F2C
		public int CurrentDifficultyLevel
		{
			get
			{
				return this._currentDifficultyLevel;
			}
			set
			{
				this._currentDifficultyLevel = Mathf.Clamp(value, 1, 32);
			}
		}

		// Token: 0x1700048D RID: 1165
		// (get) Token: 0x06001584 RID: 5508 RVA: 0x00044D3D File Offset: 0x00042F3D
		// (set) Token: 0x06001585 RID: 5509 RVA: 0x00044D45 File Offset: 0x00042F45
		public TestDifficultyLevelAdvancer.Data CurrentData
		{
			get
			{
				return this._currentData;
			}
			set
			{
				this._currentData = value;
			}
		}

		// Token: 0x06001586 RID: 5510 RVA: 0x00044D50 File Offset: 0x00042F50
		private void LoadCountBasedGainsData(ref TestDifficultyLevelAdvancer.CountBasedGains[] data, TextAsset tableAsset)
		{
			if (tableAsset == null)
			{
				return;
			}
			CsvTable csvTable = CsvHelper.Create("", tableAsset.text, true, true);
			int num = csvTable.RowCount - 1;
			data = new TestDifficultyLevelAdvancer.CountBasedGains[num];
			string[] array = new string[32];
			List<List<string>> rawDataList = csvTable.RawDataList;
			for (int i = 0; i < num; i++)
			{
				List<string> list = rawDataList[i + 1];
				int b = list.Count - 1;
				for (int j = 0; j < array.Length; j++)
				{
					array[j] = list[Mathf.Min(j + 1, b)];
				}
				data[i] = new TestDifficultyLevelAdvancer.CountBasedGains(array);
			}
		}

		// Token: 0x06001587 RID: 5511 RVA: 0x00044DF0 File Offset: 0x00042FF0
		private void LoadData()
		{
			TestDifficultyLevelAdvancer.Data currentData = this._currentData;
			if (!string.IsNullOrEmpty((currentData != null) ? currentData.customTableValuesSeparator : null))
			{
				CsvHelper.Init(this._currentData.customTableValuesSeparator[0]);
			}
			TestDifficultyLevelAdvancer.Data currentData2 = this._currentData;
			if (((currentData2 != null) ? currentData2.mobsStatsGainTable : null) != null)
			{
				CsvTable csvTable = CsvHelper.Create("", this._currentData.mobsStatsGainTable.text, true, true);
				int b = csvTable.RowCount - 1;
				this.statsGainsData = new TestDifficultyLevelAdvancer.MobStatsGains[32];
				for (int i = 0; i < this.statsGainsData.Length; i++)
				{
					int row = Mathf.Min(i + 1, b);
					this.statsGainsData[i] = new TestDifficultyLevelAdvancer.MobStatsGains(csvTable.Read(row, 1), csvTable.Read(row, 2), csvTable.Read(row, 3), csvTable.Read(row, 4));
				}
			}
			TestDifficultyLevelAdvancer.Data currentData3 = this._currentData;
			this.LoadCountBasedGainsData(ref this.additionalSpawningData, (currentData3 != null) ? currentData3.additionalMobsSpawningTable : null);
			TestDifficultyLevelAdvancer.Data currentData4 = this._currentData;
			this.LoadCountBasedGainsData(ref this.additionalBuffsData, (currentData4 != null) ? currentData4.additionalBuffsTable : null);
		}

		// Token: 0x06001588 RID: 5512 RVA: 0x00044F08 File Offset: 0x00043108
		private void SetMobsStatsGains(GameMobSpawnerExtender spawnerExtender)
		{
			if (this.statsGainsData == null)
			{
				return;
			}
			int num = this._currentDifficultyLevel - 1;
			if (num < this.statsGainsData.Length)
			{
				TestDifficultyLevelAdvancer.MobStatsGains mobStatsGains = this.statsGainsData[num];
				spawnerExtender.spawnedMobsStatsModifiers = mobStatsGains.ToMobStatsModifiers();
				spawnerExtender.spawnedBossesStatsModifiers = mobStatsGains.ToBossStatsModifiers();
			}
		}

		// Token: 0x06001589 RID: 5513 RVA: 0x00044F5C File Offset: 0x0004315C
		private void SetAdditionalSpawningData(GameMobSpawnerExtender spawnerExtender, int mobsCount)
		{
			TestDifficultyLevelAdvancer.Data currentData = this._currentData;
			if (((currentData != null) ? currentData.additionalMobs : null) == null || this._currentData.additionalMobs.Length == 0)
			{
				return;
			}
			MobBehaviour.ID[] array = new MobBehaviour.ID[mobsCount];
			for (int i = 0; i < mobsCount; i++)
			{
				array[i] = this._currentData.additionalMobs.GetRandomItem(0, -1);
			}
			spawnerExtender.additionalMobsToSpawn = array;
		}

		// Token: 0x0600158A RID: 5514 RVA: 0x00044FBB File Offset: 0x000431BB
		private void SetAdditionalBuffsData(GameMobSpawnerExtender spawnerExtender, int buffsTargetsCount)
		{
		}

		// Token: 0x0600158B RID: 5515 RVA: 0x00044FC0 File Offset: 0x000431C0
		[return: TupleElementNames(new string[]
		{
			"gainsCount",
			"gainsCountPerSpawner"
		})]
		private ValueTuple<int, int> GetGainsCount(TestDifficultyLevelAdvancer.CountBasedGains[] gains, int chunkLevel, int spawnersCount)
		{
			int num = chunkLevel - 1;
			if (num >= 0 && num < gains.Length)
			{
				int num2 = gains[num].counts[this._currentDifficultyLevel - 1];
				return new ValueTuple<int, int>(num2, (num2 > 0) ? (num2 / spawnersCount + 1) : 0);
			}
			return new ValueTuple<int, int>(-1, -1);
		}

		// Token: 0x0600158C RID: 5516 RVA: 0x0004500C File Offset: 0x0004320C
		private void SetCounts(MobBehaviourSpawner[] spawners, int spawnersCount, int totalCount, int countPerSpawner, Action<GameMobSpawnerExtender, int> countsSetter)
		{
			int num = 0;
			while (num < spawnersCount && totalCount > 0)
			{
				GameMobSpawnerExtender arg;
				if (spawners[num].TryGetComponent<GameMobSpawnerExtender>(out arg))
				{
					int num2 = Mathf.Min(totalCount, countPerSpawner);
					countsSetter(arg, num2);
					totalCount -= num2;
				}
				num++;
			}
		}

		// Token: 0x0600158D RID: 5517 RVA: 0x0004504C File Offset: 0x0004324C
		void IGameDifficultyController.TryTakeDifficultyLevelCompletionReward(PlayerProfile playerProfile, GameLocation.TypeID locationID)
		{
		}

		// Token: 0x0600158E RID: 5518 RVA: 0x00045050 File Offset: 0x00043250
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			foreach (ILocationChunk locationChunk in sceneManager.GeneratedLocation.Chunks)
			{
				if (locationChunk.HasType(LocationChunk.TypeID.BattleChunk) || locationChunk.HasType(LocationChunk.TypeID.BossChunk))
				{
					if (locationChunk.IsVisible)
					{
						this.OnBattleChunkVisibilityChanged(locationChunk, true);
					}
					else
					{
						locationChunk.VisibilityChanged += this.OnBattleChunkVisibilityChanged;
					}
				}
			}
		}

		// Token: 0x0600158F RID: 5519 RVA: 0x000450D4 File Offset: 0x000432D4
		private void OnBattleChunkVisibilityChanged(ILocationChunk locationChunk, bool isVisible)
		{
			if (!isVisible)
			{
				return;
			}
			int num;
			MobBehaviourSpawner[] mobSpawners = TestDifficultyLevelAdvancer.GetMobSpawners(locationChunk, out num, true);
			if (num == 0)
			{
				return;
			}
			ValueTuple<int, int> gainsCount = this.GetGainsCount(this.additionalSpawningData, locationChunk.Level, num);
			int item = gainsCount.Item1;
			int item2 = gainsCount.Item2;
			ValueTuple<int, int> gainsCount2 = this.GetGainsCount(this.additionalBuffsData, locationChunk.Level, num);
			int item3 = gainsCount2.Item1;
			int item4 = gainsCount2.Item2;
			for (int i = 0; i < num; i++)
			{
				MobBehaviourSpawner mobBehaviourSpawner = mobSpawners[i];
				GameMobSpawnerExtender orAddComponent = mobBehaviourSpawner.gameObject.GetOrAddComponent<GameMobSpawnerExtender>();
				orAddComponent.targetSpawner = mobBehaviourSpawner;
				this.SetMobsStatsGains(orAddComponent);
			}
			this.SetCounts(mobSpawners, num, item, item2, new Action<GameMobSpawnerExtender, int>(this.SetAdditionalSpawningData));
			this.SetCounts(mobSpawners, num, item3, item4, new Action<GameMobSpawnerExtender, int>(this.SetAdditionalBuffsData));
			locationChunk.VisibilityChanged -= this.OnBattleChunkVisibilityChanged;
		}

		// Token: 0x06001590 RID: 5520 RVA: 0x000451AC File Offset: 0x000433AC
		private void Start()
		{
			if (TestDifficultyLevelAdvancer.gameSessionManager == null)
			{
				base.CurrentGame.Services.TryGet<GameSessionManager>(out TestDifficultyLevelAdvancer.gameSessionManager);
			}
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out this.sceneManager))
			{
				this.LoadData();
				this.sceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
			this.isInitialized = true;
		}

		// Token: 0x04000C79 RID: 3193
		public const int MinDifficultyLevel = 1;

		// Token: 0x04000C7A RID: 3194
		public const int MaxDifficultyLevel = 32;

		// Token: 0x04000C7B RID: 3195
		private static readonly MobBehaviourSpawner[] MobSpawnersBuffer = new MobBehaviourSpawner[64];

		// Token: 0x04000C7C RID: 3196
		private static GameSessionManager gameSessionManager;

		// Token: 0x04000C7D RID: 3197
		[SerializeField]
		[Range(1f, 32f)]
		private int _currentDifficultyLevel = 1;

		// Token: 0x04000C7E RID: 3198
		[Space(5f)]
		[SerializeField]
		private TestDifficultyLevelAdvancer.Data _currentData;

		// Token: 0x04000C7F RID: 3199
		private GameSceneManager sceneManager;

		// Token: 0x04000C80 RID: 3200
		private TestDifficultyLevelAdvancer.MobStatsGains[] statsGainsData;

		// Token: 0x04000C81 RID: 3201
		private TestDifficultyLevelAdvancer.CountBasedGains[] additionalSpawningData;

		// Token: 0x04000C82 RID: 3202
		private TestDifficultyLevelAdvancer.CountBasedGains[] additionalBuffsData;

		// Token: 0x04000C83 RID: 3203
		private bool isInitialized;

		// Token: 0x020004FD RID: 1277
		private struct MobStatsGains
		{
			// Token: 0x060025D7 RID: 9687 RVA: 0x000762D4 File Offset: 0x000744D4
			private TargetedMobStatModifier ToStatModifier(float value, MobStatID statID)
			{
				return new TargetedMobStatModifier
				{
					modifierType = MobStatModifierType.ExtraModifier,
					targetStat = ((value > 0f) ? statID : MobStatID.Undefined),
					value = value
				};
			}

			// Token: 0x060025D8 RID: 9688 RVA: 0x00076310 File Offset: 0x00074510
			public MobStatsGains(string hitPointsModifier, string damageModifier, string bossesHitPointsModifier, string bossesDamageModifier)
			{
				float.TryParse(hitPointsModifier, NumberStyles.Float, CultureInfo.InvariantCulture, out this.hitPointsModifier);
				float.TryParse(damageModifier, NumberStyles.Float, CultureInfo.InvariantCulture, out this.damageModifier);
				float.TryParse(bossesHitPointsModifier, NumberStyles.Float, CultureInfo.InvariantCulture, out this.bossesHitPointsModifier);
				float.TryParse(bossesDamageModifier, NumberStyles.Float, CultureInfo.InvariantCulture, out this.bossesDamageModifier);
			}

			// Token: 0x060025D9 RID: 9689 RVA: 0x0007637A File Offset: 0x0007457A
			public TargetedMobStatModifier[] ToMobStatsModifiers()
			{
				return new TargetedMobStatModifier[]
				{
					this.ToStatModifier(this.hitPointsModifier, MobStatID.MobHealth),
					this.ToStatModifier(this.damageModifier, MobStatID.MobDamage)
				};
			}

			// Token: 0x060025DA RID: 9690 RVA: 0x000763AA File Offset: 0x000745AA
			public TargetedMobStatModifier[] ToBossStatsModifiers()
			{
				return new TargetedMobStatModifier[]
				{
					this.ToStatModifier(this.bossesHitPointsModifier, MobStatID.MobHealth),
					this.ToStatModifier(this.bossesDamageModifier, MobStatID.MobDamage)
				};
			}

			// Token: 0x060025DB RID: 9691 RVA: 0x000763DC File Offset: 0x000745DC
			public override string ToString()
			{
				return string.Format("hp: {0} damage: {1} boss_hp: {2} boss_damage: {3}", new object[]
				{
					this.hitPointsModifier,
					this.damageModifier,
					this.bossesHitPointsModifier,
					this.bossesDamageModifier
				});
			}

			// Token: 0x04001AA5 RID: 6821
			public float hitPointsModifier;

			// Token: 0x04001AA6 RID: 6822
			public float damageModifier;

			// Token: 0x04001AA7 RID: 6823
			public float bossesHitPointsModifier;

			// Token: 0x04001AA8 RID: 6824
			public float bossesDamageModifier;
		}

		// Token: 0x020004FE RID: 1278
		private struct CountBasedGains
		{
			// Token: 0x060025DC RID: 9692 RVA: 0x00076434 File Offset: 0x00074634
			public CountBasedGains(string[] difficultyLevelCounts)
			{
				this.counts = new int[difficultyLevelCounts.Length];
				for (int i = 0; i < this.counts.Length; i++)
				{
					int.TryParse(difficultyLevelCounts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out this.counts[i]);
				}
			}

			// Token: 0x060025DD RID: 9693 RVA: 0x0007647D File Offset: 0x0007467D
			public override string ToString()
			{
				return string.Join<int>(" ", this.counts);
			}

			// Token: 0x04001AA9 RID: 6825
			public int[] counts;
		}

		// Token: 0x020004FF RID: 1279
		private sealed class MobSpawnersSorter : IComparer<MobBehaviourSpawner>
		{
			// Token: 0x060025DE RID: 9694 RVA: 0x00076490 File Offset: 0x00074690
			public static int GetMobsCount(MobBehaviourSpawner spawner)
			{
				GameMobsGroupControllerBase spawnedGroup = spawner.SpawnedGroup;
				return ((spawnedGroup != null) ? spawnedGroup.Mobs.Count : 0) + spawner.RemainingSpawningCount;
			}

			// Token: 0x060025DF RID: 9695 RVA: 0x000764BC File Offset: 0x000746BC
			public int Compare(MobBehaviourSpawner s0, MobBehaviourSpawner s1)
			{
				return TestDifficultyLevelAdvancer.MobSpawnersSorter.GetMobsCount(s1).CompareTo(TestDifficultyLevelAdvancer.MobSpawnersSorter.GetMobsCount(s0));
			}

			// Token: 0x04001AAA RID: 6826
			public static readonly TestDifficultyLevelAdvancer.MobSpawnersSorter Default = new TestDifficultyLevelAdvancer.MobSpawnersSorter();
		}

		// Token: 0x02000500 RID: 1280
		[Serializable]
		public sealed class Data
		{
			// Token: 0x04001AAB RID: 6827
			[ObjectFactoryIDPopup(typeof(IGameMob))]
			public MobBehaviour.ID[] additionalMobs;

			// Token: 0x04001AAC RID: 6828
			public BuffsGeneratorBuilderAsset.Reference[] additionalBuffs;

			// Token: 0x04001AAD RID: 6829
			[Space(5f)]
			public string customTableValuesSeparator = ";";

			// Token: 0x04001AAE RID: 6830
			public TextAsset mobsStatsGainTable;

			// Token: 0x04001AAF RID: 6831
			public TextAsset additionalMobsSpawningTable;

			// Token: 0x04001AB0 RID: 6832
			public TextAsset additionalBuffsTable;
		}
	}
}
