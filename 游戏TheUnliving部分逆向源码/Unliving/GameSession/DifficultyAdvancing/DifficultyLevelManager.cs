using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.CollectionsExtensions;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Currencies;
using Unliving.DataParsing;
using Unliving.Gameplay;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.MobsStats;
using Unliving.Pickables;
using Unliving.PlayerProfileManagement;

namespace Unliving.GameSession.DifficultyAdvancing
{
	// Token: 0x020002C3 RID: 707
	[CreateAssetMenu(fileName = "DifficultyLevelManager", menuName = "Game/Global/Difficulty Level Manager")]
	[Service(typeof(DifficultyLevelManager), new Type[]
	{
		typeof(IGameDifficultyController)
	})]
	public sealed class DifficultyLevelManager : GlobalManagerBase, IGameDifficultyController, ISerializationCallbackReceiver
	{
		// Token: 0x060018A1 RID: 6305 RVA: 0x0004D460 File Offset: 0x0004B660
		private static DifficultyLevelData.CurrencyValue[] ParseCurrencies(string str, bool isCurrencyModifiers, bool exactCount = false)
		{
			float[] array;
			int num = ParsingUtility.ParseFloats(str, out array, '_');
			if (num != 0)
			{
				float num2 = isCurrencyModifiers ? 0.01f : 1f;
				DifficultyLevelData.CurrencyValue[] array2 = new DifficultyLevelData.CurrencyValue[exactCount ? num : 3];
				for (int i = 0; i < array2.Length; i++)
				{
					array2[i] = new DifficultyLevelData.CurrencyValue(DifficultyLevelManager.<ParseCurrencies>g__GetCurrency|1_0(i), array[i % num] * num2);
				}
				return array2;
			}
			return Array.Empty<DifficultyLevelData.CurrencyValue>();
		}

		// Token: 0x1700053E RID: 1342
		// (get) Token: 0x060018A2 RID: 6306 RVA: 0x0004D4CD File Offset: 0x0004B6CD
		// (set) Token: 0x060018A3 RID: 6307 RVA: 0x0004D4D5 File Offset: 0x0004B6D5
		public int CurrentDifficultyLevel
		{
			get
			{
				return this.currentDifficultyLevel;
			}
			set
			{
				if (this.currentDifficultyLevel == value)
				{
					return;
				}
				this.SetDifficultyLevel(value);
			}
		}

		// Token: 0x1700053F RID: 1343
		// (get) Token: 0x060018A4 RID: 6308 RVA: 0x0004D4E8 File Offset: 0x0004B6E8
		public IReadOnlyList<DifficultyLevelData> DifficultyLevelsData
		{
			get
			{
				return this.difficultyLevelsData;
			}
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x0004D4F0 File Offset: 0x0004B6F0
		[ContextMenu("Load Data From Table")]
		private void LoadDataFromTable()
		{
			List<List<string>> list;
			if (!ParsingUtility.TryParseCsvTable(this.dataTable, out list))
			{
				return;
			}
			List<int> list2 = new List<int>(4);
			List<DifficultyLevelData.LocationRewardData> list3 = new List<DifficultyLevelData.LocationRewardData>(4);
			this.difficultyLevelsData.Clear();
			for (int i = 1; i < list.Count; i++)
			{
				List<string> list4 = list[i];
				int num = Mathf.Min(list4.Count - 1, 4);
				int j = 1;
				DifficultyLevelManager.StatModifiersBuffer.Clear();
				list2.Clear();
				list3.Clear();
				while (j <= num)
				{
					GameLocation.TypeID locationID = (GameLocation.TypeID)j;
					float[] array;
					int num2 = ParsingUtility.ParseFloats(list4[j], out array, '_');
					if (num2 != 0)
					{
						TargetedMobStatModifier[] statModifiers = new TargetedMobStatModifier[]
						{
							new TargetedMobStatModifier
							{
								targetStat = MobStatID.MobHealth,
								modifierType = MobStatModifierType.ExtraModifier,
								value = array[0] * 0.01f
							},
							new TargetedMobStatModifier
							{
								targetStat = MobStatID.MobDamage,
								modifierType = MobStatModifierType.ExtraModifier,
								value = array[1 % num2] * 0.01f
							}
						};
						DifficultyLevelManager.StatModifiersBuffer.Add(new DifficultyLevelData.LocationMobStatModifiers
						{
							locationID = locationID,
							statModifiers = statModifiers
						});
					}
					j++;
				}
				DifficultyLevelData difficultyLevelData = new DifficultyLevelData
				{
					locationEnemyMobStatModifiers = DifficultyLevelManager.StatModifiersBuffer.ToArray()
				};
				if (j < list4.Count)
				{
					difficultyLevelData.playerCurrencyMultipliers = DifficultyLevelManager.ParseCurrencies(list4[j], true, false);
				}
				j++;
				float enemyMobsPowerCharacteristic;
				if (j < list4.Count && ParsingUtility.TryParseFloat(list4[j], out enemyMobsPowerCharacteristic))
				{
					difficultyLevelData.enemyMobsPowerCharacteristic = enemyMobsPowerCharacteristic;
				}
				j++;
				if (j < list4.Count)
				{
					int num3 = 1;
					int num4 = 5;
					while (num3 < num4 && j < list4.Count)
					{
						DifficultyLevelData.CurrencyValue[] rewards = DifficultyLevelManager.ParseCurrencies(list4[j], false, true);
						list3.Add(new DifficultyLevelData.LocationRewardData(num3, rewards));
						j++;
						num3++;
					}
					difficultyLevelData.completedLevelPlayerReward = list3.ToArray();
				}
				while (j < list4.Count)
				{
					float num5;
					if (ParsingUtility.TryParseFloat(list4[j], out num5))
					{
						if (num5 < 0f)
						{
							num5 = 0f;
						}
						list2.Add((int)num5);
					}
					j++;
				}
				difficultyLevelData.enemyMobBuffsCount = list2.ToArray();
				this.difficultyLevelsData.Add(difficultyLevelData);
			}
		}

		// Token: 0x060018A6 RID: 6310 RVA: 0x0004D75C File Offset: 0x0004B95C
		private void SetDifficultyLevel(int newLevel)
		{
			this.currentDifficultyLevel = Mathf.Clamp(newLevel, 1, this.difficultyLevelsData.Count);
			this.currentDifficultyLevelData = ((newLevel > 0) ? this.difficultyLevelsData[newLevel - 1] : null);
		}

		// Token: 0x060018A7 RID: 6311 RVA: 0x0004D794 File Offset: 0x0004B994
		private void SetMobGainCounts(List<GameMobSpawnerExtender> mobGainGenerators, int[] difficultyLevelGainedMobsCounts, int gainTypesCount)
		{
			int[] array = new int[gainTypesCount];
			for (int i = 0; i < gainTypesCount; i++)
			{
				if (this.enemyMobGains[i].IsAllowedLocation(this.currentLocationID))
				{
					array[i] = difficultyLevelGainedMobsCounts[i];
				}
				else
				{
					array[i] = 0;
				}
			}
			Dictionary<int, int[]> dictionary = new Dictionary<int, int[]>(16);
			while (mobGainGenerators.Count != 0)
			{
				int index = UnityEngine.Random.Range(0, mobGainGenerators.Count);
				GameMobSpawnerExtender gameMobSpawnerExtender = mobGainGenerators[index];
				GameMobSpawnerExtender.BuffsData[] buffGeneratorsData = gameMobSpawnerExtender.buffGeneratorsData;
				ILocationChunk currentChunk = gameMobSpawnerExtender.CurrentChunk;
				int chunkID = currentChunk.ChunkID;
				int[] array2;
				if (!dictionary.TryGetValue(chunkID, out array2))
				{
					array2 = new int[gainTypesCount];
					dictionary.Add(chunkID, array2);
				}
				bool flag = true;
				for (int j = 0; j < gainTypesCount; j++)
				{
					int num = array2[j];
					int num2 = array[j];
					int num3 = difficultyLevelGainedMobsCounts[j];
					if (num >= 0 && num2 != 0 && num3 >= 1 && this.enemyMobGains[j].IsAllowedChunk(currentChunk))
					{
						int num4 = Mathf.Max(Mathf.RoundToInt((float)num3 * 0.3f), 1);
						int num5 = UnityEngine.Random.Range(0, Mathf.Min(num2, num4 - num) + 1);
						if (num5 < 1)
						{
							flag = false;
						}
						else
						{
							num += num5;
							int num6;
							buffGeneratorsData[j].TryAddBuffTargetsCount(num5, out num6);
							if (num >= num4)
							{
								array2[j] = -1;
							}
							else
							{
								array2[j] = num;
								flag = false;
							}
							num2 -= num5;
							array[j] = num2;
						}
					}
				}
				if (flag)
				{
					mobGainGenerators.RemoveBySwap(index);
				}
			}
		}

		// Token: 0x060018A8 RID: 6312 RVA: 0x0004D910 File Offset: 0x0004BB10
		private void PrepareMobSpawners(IGameLocationProvider locationProvider)
		{
			GameLocation.TypeID locationType = locationProvider.LocationType;
			if (locationType == GameLocation.TypeID.Homespace)
			{
				return;
			}
			IReadOnlyList<ILocationChunk> chunks = locationProvider.CurrentLocation.Chunks;
			TargetedMobStatModifier[] spawnedMobsStatsModifiers;
			bool flag = this.currentDifficultyLevelData.TryGetLocationMobStatModifiers(locationType, out spawnedMobsStatsModifiers);
			int[] enemyMobBuffsCount = this.currentDifficultyLevelData.enemyMobBuffsCount;
			List<GameMobSpawnerExtender> list = new List<GameMobSpawnerExtender>(64);
			int num = enemyMobBuffsCount.Length;
			if (num > this.enemyMobGains.Length)
			{
				num = this.enemyMobGains.Length;
			}
			for (int i = 0; i < chunks.Count; i++)
			{
				ILocationChunk locationChunk = chunks[i];
				bool flag2 = locationChunk.HasType(LocationChunk.TypeID.BossChunk);
				if (locationChunk.HasType(LocationChunk.TypeID.BattleChunk) || flag2 || locationChunk.HasType(LocationChunk.TypeID.ArenaDeadEnd))
				{
					IList<ILocationObject> environmentObjects = locationChunk.EnvironmentObjects;
					for (int j = 0; j < environmentObjects.Count; j++)
					{
						MobBehaviourSpawner mobBehaviourSpawner = environmentObjects[j] as MobBehaviourSpawner;
						if (mobBehaviourSpawner != null)
						{
							GameMobSpawnerExtender orAddComponent = mobBehaviourSpawner.gameObject.GetOrAddComponent<GameMobSpawnerExtender>();
							orAddComponent.CurrentChunk = locationChunk;
							orAddComponent.targetSpawner = mobBehaviourSpawner;
							if (flag)
							{
								orAddComponent.spawnedMobsStatsModifiers = spawnedMobsStatsModifiers;
							}
							if (num != 0)
							{
								int num2 = flag2 ? 7 : 6;
								if (locationChunk.Level < num2 && mobBehaviourSpawner.ForceGetInitialSpawningCount() > 3)
								{
									GameMobSpawnerExtender.BuffsData[] array = new GameMobSpawnerExtender.BuffsData[num];
									for (int k = 0; k < num; k++)
									{
										array[k] = new GameMobSpawnerExtender.BuffsData
										{
											buffGeneratorAssets = this.enemyMobGains[k].buffGeneratorAssets
										};
									}
									orAddComponent.buffGeneratorsData = array;
									list.Add(orAddComponent);
								}
							}
						}
					}
				}
			}
			this.SetMobGainCounts(list, enemyMobBuffsCount, num);
		}

		// Token: 0x060018A9 RID: 6313 RVA: 0x0004DAA4 File Offset: 0x0004BCA4
		private void TryTakeLevelCompletionReward()
		{
			this.TryTakeLevelCompletionReward(this.currentLocationID, null);
		}

		// Token: 0x060018AA RID: 6314 RVA: 0x0004DAB4 File Offset: 0x0004BCB4
		public IReadOnlyList<DifficultyLevelData.CurrencyValue> GetLevelCompletionRewards(PlayerProfile profile, int level, GameLocation.TypeID locationID, out bool isRewardTaken)
		{
			PlayerProfileGameDifficultyRewardState playerProfileGameDifficultyRewardState;
			profile.TryGetDifficultyLevelRewardState(level, locationID, out playerProfileGameDifficultyRewardState);
			isRewardTaken = playerProfileGameDifficultyRewardState.isRewardTaken;
			DifficultyLevelData.CurrencyValue[] result;
			this.difficultyLevelsData[level - 1].TryGetCompletedLevelRewards(locationID, out result);
			return result;
		}

		// Token: 0x060018AB RID: 6315 RVA: 0x0004DAF0 File Offset: 0x0004BCF0
		public void TryTakeLevelCompletionReward(GameLocation.TypeID locationID, PlayerProfile profileOverride = null)
		{
			PlayerProfile playerProfile = profileOverride ?? this.currentPlayerProfile;
			if (playerProfile == null || locationID == GameLocation.TypeID.Undefined)
			{
				return;
			}
			int num = this.CurrentDifficultyLevel;
			bool flag;
			IReadOnlyList<DifficultyLevelData.CurrencyValue> levelCompletionRewards = this.GetLevelCompletionRewards(playerProfile, num, locationID, out flag);
			if (!flag)
			{
				for (int i = 0; i < levelCompletionRewards.Count; i++)
				{
					DifficultyLevelData.CurrencyValue currencyValue = levelCompletionRewards[i];
					CurrencyOperationArgs currencyOperationArgs = new CurrencyOperationArgs
					{
						currencyID = currencyValue.currencyID,
						amount = (float)((int)currencyValue.value)
					};
					playerProfile.TryExecuteCurrencyOperation(currencyOperationArgs);
				}
				playerProfile.UpdateDifficultyLevelRewardState(num, locationID, true);
			}
		}

		// Token: 0x060018AC RID: 6316 RVA: 0x0004DB86 File Offset: 0x0004BD86
		void IGameDifficultyController.TryTakeDifficultyLevelCompletionReward(PlayerProfile playerProfile, GameLocation.TypeID locationID)
		{
			this.TryTakeLevelCompletionReward(locationID, playerProfile);
		}

		// Token: 0x060018AD RID: 6317 RVA: 0x0004DB90 File Offset: 0x0004BD90
		private void OnPerformingCurrencyOperation(ICurrencyOperationArgs args)
		{
			if (args.Spending)
			{
				return;
			}
			GameObject gameObject = args.Sender as GameObject;
			PickableBase pickableBase;
			BaseGameMob baseGameMob;
			float num;
			if ((args.Sender is IAbilityActivatedContainer || (gameObject != null && (gameObject.TryGetComponent<PickableBase>(out pickableBase) || (gameObject.TryGetComponent<BaseGameMob>(out baseGameMob) && baseGameMob.IsPlayerMob)))) && this.currentDifficultyLevelData.TryGetCurrencyMultiplier(args.CurrencyID, out num))
			{
				int num2 = (int)(args.Amount * num);
				args.Amount = (float)num2;
			}
		}

		// Token: 0x060018AE RID: 6318 RVA: 0x0004DC10 File Offset: 0x0004BE10
		private void Reset()
		{
			this.waitForGameSessionInitialization = true;
		}

		// Token: 0x060018AF RID: 6319 RVA: 0x0004DC1C File Offset: 0x0004BE1C
		protected override void OnSceneLoaded(Scene scene)
		{
			if (!this.isActive)
			{
				return;
			}
			this.SetDifficultyLevel(this.currentDifficultyLevel);
			this.currentLocationID = GameLocation.TypeID.Undefined;
			if (this.currentDifficultyLevelData == null)
			{
				return;
			}
			IGameLocationProvider gameLocationProvider;
			if (base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				this.currentLocationID = gameLocationProvider.LocationType;
				this.PrepareMobSpawners(gameLocationProvider);
			}
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				this.currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				PlayerProfileGameDifficultyRewardState playerProfileGameDifficultyRewardState;
				if (this.currentDifficultyLevelData.completedLevelPlayerReward.Length != 0 && !this.currentPlayerProfile.TryGetDifficultyLevelRewardState(this.currentDifficultyLevel, this.currentLocationID, out playerProfileGameDifficultyRewardState))
				{
					this.currentPlayerProfile.UpdateDifficultyLevelRewardState(this.currentDifficultyLevel, this.currentLocationID, false);
				}
				if (this.currentDifficultyLevelData.playerCurrencyMultipliers.Length != 0)
				{
					this.currentPlayerProfile.PerformingCurrencyOperation += this.OnPerformingCurrencyOperation;
				}
			}
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out this.gameSessionManager))
			{
				this.gameSessionManager.SessionStateChanged += this.OnGameSessionStateChanged;
				this.gameSessionManager.Destroyed += this.OnGameSessionManagerDestroyed;
			}
		}

		// Token: 0x060018B0 RID: 6320 RVA: 0x0004DD44 File Offset: 0x0004BF44
		private void OnGameSessionStateChanged(IGameSessionManager manager, SessionState sessionState)
		{
			if (sessionState == SessionState.VictoryCutscene || sessionState == SessionState.Victory)
			{
				this.TryTakeLevelCompletionReward();
				manager.SessionStateChanged -= this.OnGameSessionStateChanged;
			}
		}

		// Token: 0x060018B1 RID: 6321 RVA: 0x0004DD66 File Offset: 0x0004BF66
		protected override void OnSceneUnloaded(Scene scene)
		{
			if (this.currentPlayerProfile != null)
			{
				this.currentPlayerProfile.PerformingCurrencyOperation -= this.OnPerformingCurrencyOperation;
			}
		}

		// Token: 0x060018B2 RID: 6322 RVA: 0x0004DD87 File Offset: 0x0004BF87
		private void OnGameSessionManagerDestroyed(object obj)
		{
			this.gameSessionManager.SessionStateChanged -= this.OnGameSessionStateChanged;
			this.gameSessionManager.Destroyed -= this.OnGameSessionManagerDestroyed;
			this.gameSessionManager = null;
		}

		// Token: 0x060018B3 RID: 6323 RVA: 0x0004DDBE File Offset: 0x0004BFBE
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		// Token: 0x060018B4 RID: 6324 RVA: 0x0004DDC0 File Offset: 0x0004BFC0
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x060018B7 RID: 6327 RVA: 0x0004DDE5 File Offset: 0x0004BFE5
		[CompilerGenerated]
		internal static CurrencyID <ParseCurrencies>g__GetCurrency|1_0(int index)
		{
			switch (index)
			{
			case 0:
				return CurrencyID.Prima;
			case 1:
				return CurrencyID.Meta;
			case 2:
				return CurrencyID.Cinder;
			default:
				return CurrencyID.None;
			}
		}

		// Token: 0x04000DDA RID: 3546
		private static readonly List<DifficultyLevelData.LocationMobStatModifiers> StatModifiersBuffer = new List<DifficultyLevelData.LocationMobStatModifiers>(8);

		// Token: 0x04000DDB RID: 3547
		public bool isActive = true;

		// Token: 0x04000DDC RID: 3548
		[SerializeField]
		private TextAsset dataTable;

		// Token: 0x04000DDD RID: 3549
		[SerializeField]
		private int currentDifficultyLevel = 1;

		// Token: 0x04000DDE RID: 3550
		[SerializeField]
		private List<DifficultyLevelData> difficultyLevelsData;

		// Token: 0x04000DDF RID: 3551
		[SerializeField]
		private DifficultyLevelData.MobGainData[] enemyMobGains;

		// Token: 0x04000DE0 RID: 3552
		[SerializeField]
		[HideInInspector]
		private string lastTableAssetHash;

		// Token: 0x04000DE1 RID: 3553
		private DifficultyLevelData currentDifficultyLevelData;

		// Token: 0x04000DE2 RID: 3554
		private PlayerProfile currentPlayerProfile;

		// Token: 0x04000DE3 RID: 3555
		private GameLocation.TypeID currentLocationID;

		// Token: 0x04000DE4 RID: 3556
		private IGameSessionManager gameSessionManager;
	}
}
