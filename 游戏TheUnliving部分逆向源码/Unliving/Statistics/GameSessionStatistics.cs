using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameScene;
using Unliving.GameSession.DifficultyAdvancing;
using Unliving.LevelGeneration;
using Unliving.Player;

namespace Unliving.Statistics
{
	// Token: 0x02000129 RID: 297
	[Serializable]
	public class GameSessionStatistics : StatisticsBase<GameSessionStatistics.GameSessionStatData>
	{
		// Token: 0x0600077A RID: 1914 RVA: 0x00018020 File Offset: 0x00016220
		public override void SetGameData(IGame game, PlayerStatisticsManager.Data playerStatsData)
		{
			base.SetGameData(game, playerStatsData);
			if (game.Services.TryGet<GameManager>(out this.gameManager))
			{
				this.gameManager.GameStarted += this.OnGameStarted;
				this.OnGameStarted(this.gameManager);
			}
			playerStatsData.GetPlayingTime = new Func<float>(this.data.GetPlayingTime);
			playerStatsData.GetGamerunsCount = new Func<int>(this.data.GetGamerunsCount);
			playerStatsData.GetLocationRunsCount = new Func<GameLocation.TypeID, int, int>(this.data.GetLocationRunsCount);
			playerStatsData.IsLocationExlored = new Func<GameLocation.TypeID, bool>(this.data.IsLocationExlored);
			playerStatsData.GetLocationWinsCount = new Func<GameLocation.TypeID, int, int>(this.data.GetLocationWinsCount);
			playerStatsData.GetChunksVisitedCount = new Func<IList<LocationChunk.TypeID>, int>(this.data.GetVisitedChunksCount);
			playerStatsData.GetZeroSacrificedMobsChunksCount = new Func<IList<LocationChunk.TypeID>, int>(this.data.GetZeroSacrificedMobsChunksCount);
		}

		// Token: 0x0600077B RID: 1915 RVA: 0x0001810C File Offset: 0x0001630C
		public override void OnSceneLoaded(IStoreStatisticsProvider storeStatisticsProvider)
		{
			base.OnSceneLoaded(storeStatisticsProvider);
			if (this.currentGame == null)
			{
				return;
			}
			if (this.currentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager))
			{
				this.gameSessionManager.SessionStateChanged += this.OnGameSessionStateChanged;
			}
			if (this.currentGame.Services.TryGet<IGameLocationProvider>(out this.gameLocationProvider))
			{
				GameSceneManager gameSceneManager = this.gameLocationProvider as GameSceneManager;
				if (gameSceneManager != null)
				{
					gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
				}
				int currentDifficultyLevel = this.GetCurrentDifficultyLevel();
				GameLocation.TypeID locationType = this.gameLocationProvider.LocationType;
				for (int i = 0; i < this.data.locationsData.Count; i++)
				{
					GameSessionStatistics.GameSessionStatData.LocationData locationData = this.data.locationsData[i];
					if (locationData.locationType == locationType && locationData.difficultyLevel == currentDifficultyLevel)
					{
						locationData.attemptCount++;
						this.data.locationsData[i] = locationData;
						return;
					}
				}
				this.data.locationsData.Add(new GameSessionStatistics.GameSessionStatData.LocationData
				{
					locationType = locationType,
					difficultyLevel = currentDifficultyLevel,
					attemptCount = 1
				});
			}
			if (this.currentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
		}

		// Token: 0x0600077C RID: 1916 RVA: 0x00018290 File Offset: 0x00016490
		private void OnGameSessionStateChanged(IGameSessionManager sessionManager, SessionState state)
		{
			if (state == SessionState.Defeat || state == SessionState.Victory || state == SessionState.Interrupted)
			{
				this.data.playingTime += Time.time - this.data.lastGameSessionStartTime;
			}
			if (state == SessionState.VictoryCutscene || state == SessionState.Victory)
			{
				IGameLocationProvider gameLocationProvider = this.gameLocationProvider;
				this.OnLocationComplete((gameLocationProvider != null) ? gameLocationProvider.CurrentLocation.Type : GameLocation.TypeID.Undefined);
			}
		}

		// Token: 0x0600077D RID: 1917 RVA: 0x000182F4 File Offset: 0x000164F4
		private void OnLocationComplete(GameLocation.TypeID locationType)
		{
			if (this.locationCompletionRegistered)
			{
				return;
			}
			this.locationCompletionRegistered = true;
			int currentDifficultyLevel = this.GetCurrentDifficultyLevel();
			int i = 0;
			while (i < this.data.locationsData.Count)
			{
				GameSessionStatistics.GameSessionStatData.LocationData locationData = this.data.locationsData[i];
				if (locationData.locationType == locationType && locationData.difficultyLevel == currentDifficultyLevel)
				{
					locationData.finishCount++;
					this.data.locationsData[i] = locationData;
					IStoreStatisticsProvider storeStatisticsProvider = this.storeStatisticsProvider;
					if (storeStatisticsProvider == null)
					{
						return;
					}
					storeStatisticsProvider.SetStatValue(locationData.LocationFinishedCountStatID, locationData.finishCount);
					return;
				}
				else
				{
					i++;
				}
			}
			GameSessionStatistics.GameSessionStatData.LocationData locationData2 = new GameSessionStatistics.GameSessionStatData.LocationData
			{
				locationType = locationType,
				difficultyLevel = currentDifficultyLevel,
				finishCount = 1
			};
			this.data.locationsData.Add(locationData2);
			IStoreStatisticsProvider storeStatisticsProvider2 = this.storeStatisticsProvider;
			if (storeStatisticsProvider2 == null)
			{
				return;
			}
			storeStatisticsProvider2.SetStatValue(locationData2.LocationFinishedCountStatID, locationData2.finishCount);
		}

		// Token: 0x0600077E RID: 1918 RVA: 0x000183E8 File Offset: 0x000165E8
		private int GetCurrentDifficultyLevel()
		{
			int result = 0;
			DifficultyLevelManager difficultyLevelManager;
			if (this.currentGame.Services.TryGet<DifficultyLevelManager>(out difficultyLevelManager))
			{
				result = difficultyLevelManager.CurrentDifficultyLevel;
			}
			return result;
		}

		// Token: 0x0600077F RID: 1919 RVA: 0x00018414 File Offset: 0x00016614
		private void OnLocationGenerated(GameSceneManager gameSceneManager)
		{
			this.locationCompletionRegistered = false;
			int num = (from c in gameSceneManager.GeneratedLocation.Chunks
			where c.IsCoreChunk
			select c).Count<ILocationChunk>();
			this.data.chunksGenerated += num;
		}

		// Token: 0x06000780 RID: 1920 RVA: 0x00018470 File Offset: 0x00016670
		private void OnGameStarted(IGameManager gameManager)
		{
			this.data.lastGameSessionStartTime = Time.time;
			this.data.gamerunsCount++;
		}

		// Token: 0x06000781 RID: 1921 RVA: 0x00018498 File Offset: 0x00016698
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkEntered -= this.OnPlayerLocationChunkEntered;
			}
			this.currentPlayer = player;
			this.currentPlayer.LocationChunkEntered += this.OnPlayerLocationChunkEntered;
			this.OnPlayerLocationChunkEntered(null, player.CurrentLocationChunk);
		}

		// Token: 0x06000782 RID: 1922 RVA: 0x000184F4 File Offset: 0x000166F4
		private void OnPlayerLocationChunkEntered(ILocationChunk oldChunk, ILocationChunk newChunk)
		{
			if (newChunk == null || !newChunk.IsCoreChunk)
			{
				return;
			}
			LocationChunk locationChunk = newChunk as LocationChunk;
			if (locationChunk != null && !this.data.visitedChunks.Contains(locationChunk))
			{
				if (!this.data.zeroSacrificedMobsChunks.Contains(locationChunk))
				{
					this.data.zeroSacrificedMobsChunks.Add(locationChunk);
				}
				this.data.visitedChunks.Add(locationChunk);
				this.data.chunksVisitedCount = this.data.visitedChunks.Count;
			}
		}

		// Token: 0x06000783 RID: 1923 RVA: 0x0001857C File Offset: 0x0001677C
		public override void Destroy()
		{
			if (!this.gameManager.IsNull())
			{
				this.gameManager.GameStarted -= this.OnGameStarted;
			}
			if (!this.gameSessionManager.IsNull())
			{
				this.gameSessionManager.SessionStateChanged -= this.OnGameSessionStateChanged;
			}
			if (!this.playerProvider.IsNull())
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkEntered -= this.OnPlayerLocationChunkEntered;
			}
		}

		// Token: 0x0400045C RID: 1116
		private GameManager gameManager;

		// Token: 0x0400045D RID: 1117
		private IGameLocationProvider gameLocationProvider;

		// Token: 0x0400045E RID: 1118
		private GameSessionManager gameSessionManager;

		// Token: 0x0400045F RID: 1119
		private IPlayerProvider playerProvider;

		// Token: 0x04000460 RID: 1120
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000461 RID: 1121
		private bool locationCompletionRegistered;

		// Token: 0x0200043B RID: 1083
		[Serializable]
		public class GameSessionStatData : StatisticsSerializationDataBase
		{
			// Token: 0x17000719 RID: 1817
			// (get) Token: 0x0600230D RID: 8973 RVA: 0x0006C99C File Offset: 0x0006AB9C
			public override Type StatisticsType
			{
				get
				{
					return typeof(GameSessionStatistics);
				}
			}

			// Token: 0x0600230E RID: 8974 RVA: 0x0006C9A8 File Offset: 0x0006ABA8
			public override IStatistics CreateInstance()
			{
				return new GameSessionStatistics();
			}

			// Token: 0x0600230F RID: 8975 RVA: 0x0006C9AF File Offset: 0x0006ABAF
			public override void Initialize()
			{
			}

			// Token: 0x06002310 RID: 8976 RVA: 0x0006C9B1 File Offset: 0x0006ABB1
			internal float GetPlayingTime()
			{
				return this.playingTime + Time.time - this.lastGameSessionStartTime;
			}

			// Token: 0x06002311 RID: 8977 RVA: 0x0006C9C6 File Offset: 0x0006ABC6
			internal int GetGamerunsCount()
			{
				return this.gamerunsCount;
			}

			// Token: 0x06002312 RID: 8978 RVA: 0x0006C9D0 File Offset: 0x0006ABD0
			internal bool IsLocationExlored(GameLocation.TypeID location)
			{
				return this.locationsData.Any((GameSessionStatistics.GameSessionStatData.LocationData data) => data.locationType == location);
			}

			// Token: 0x06002313 RID: 8979 RVA: 0x0006CA04 File Offset: 0x0006AC04
			internal int GetLocationWinsCount(GameLocation.TypeID locationType, int difficultyLevel)
			{
				int num = 0;
				for (int i = 0; i < this.locationsData.Count; i++)
				{
					GameSessionStatistics.GameSessionStatData.LocationData locationData = this.locationsData[i];
					if (locationData.locationType == locationType && locationData.IsValidDifficultyLevel(difficultyLevel))
					{
						num += locationData.finishCount;
					}
				}
				return num;
			}

			// Token: 0x06002314 RID: 8980 RVA: 0x0006CA54 File Offset: 0x0006AC54
			internal int GetLocationRunsCount(GameLocation.TypeID locationType, int difficultyLevel)
			{
				int num = 0;
				for (int i = 0; i < this.locationsData.Count; i++)
				{
					GameSessionStatistics.GameSessionStatData.LocationData locationData = this.locationsData[i];
					if (locationData.locationType == locationType && locationData.IsValidDifficultyLevel(difficultyLevel))
					{
						num += locationData.attemptCount;
					}
				}
				return num;
			}

			// Token: 0x06002315 RID: 8981 RVA: 0x0006CAA4 File Offset: 0x0006ACA4
			internal int GetVisitedChunksCount(IList<LocationChunk.TypeID> chunkTypes)
			{
				return this.visitedChunks.Count((LocationChunk c) => chunkTypes.Contains(c.Type));
			}

			// Token: 0x06002316 RID: 8982 RVA: 0x0006CAD8 File Offset: 0x0006ACD8
			internal int GetZeroSacrificedMobsChunksCount(IList<LocationChunk.TypeID> chunkTypes)
			{
				return this.zeroSacrificedMobsChunks.Count((LocationChunk c) => chunkTypes.Contains(c.Type));
			}

			// Token: 0x0400166B RID: 5739
			internal const string LocationFinishedCountStatIDPrefix = "finished_location_";

			// Token: 0x0400166C RID: 5740
			public readonly List<GameSessionStatistics.GameSessionStatData.LocationData> locationsData = new List<GameSessionStatistics.GameSessionStatData.LocationData>();

			// Token: 0x0400166D RID: 5741
			public float playingTime;

			// Token: 0x0400166E RID: 5742
			public int gamerunsCount;

			// Token: 0x0400166F RID: 5743
			public int chunksGenerated;

			// Token: 0x04001670 RID: 5744
			public int chunksVisitedCount;

			// Token: 0x04001671 RID: 5745
			public float lastGameSessionStartTime;

			// Token: 0x04001672 RID: 5746
			[NonSerialized]
			public readonly List<LocationChunk> zeroSacrificedMobsChunks = new List<LocationChunk>();

			// Token: 0x04001673 RID: 5747
			[NonSerialized]
			public readonly List<LocationChunk> visitedChunks = new List<LocationChunk>();

			// Token: 0x020005A7 RID: 1447
			public struct LocationData
			{
				// Token: 0x1700081C RID: 2076
				// (get) Token: 0x060027C8 RID: 10184 RVA: 0x0007C633 File Offset: 0x0007A833
				public readonly string LocationFinishedCountStatID
				{
					get
					{
						return "finished_location_" + this.locationType.ToString();
					}
				}

				// Token: 0x060027C9 RID: 10185 RVA: 0x0007C650 File Offset: 0x0007A850
				public readonly bool IsValidDifficultyLevel(int level)
				{
					return level < 0 || level == this.difficultyLevel;
				}

				// Token: 0x04001D10 RID: 7440
				public GameLocation.TypeID locationType;

				// Token: 0x04001D11 RID: 7441
				[OptionalField]
				public int difficultyLevel;

				// Token: 0x04001D12 RID: 7442
				public int attemptCount;

				// Token: 0x04001D13 RID: 7443
				public int finishCount;
			}
		}
	}
}
