using System;
using Game.Core;
using UnityEngine;
using Unliving.GameScene;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000277 RID: 631
	[CreateAssetMenu(fileName = "TestDifficultyLevelManager", menuName = "Game/Global/Test Difficulty Level Manager")]
	public sealed class TestDifficultyLevelManager : GlobalManagerBase
	{
		// Token: 0x06001593 RID: 5523 RVA: 0x00045235 File Offset: 0x00043435
		public override void Initialize(IGame currentGame)
		{
			GameBehaviourBase.Created -= this.OnGameBehaviourCreated;
			base.Initialize(currentGame);
			this.currentDifficultyLevel = this.startDifficultyLevel;
			if (this.isActive)
			{
				GameBehaviourBase.Created += this.OnGameBehaviourCreated;
			}
		}

		// Token: 0x06001594 RID: 5524 RVA: 0x00045274 File Offset: 0x00043474
		private void OnGameBehaviourCreated(GameBehaviourBase behaviour)
		{
			GameSceneManager gameSceneManager = behaviour as GameSceneManager;
			if (gameSceneManager != null)
			{
				TestDifficultyLevelAdvancer testDifficultyLevelAdvancer;
				if (!gameSceneManager.TryGetComponent<TestDifficultyLevelAdvancer>(out testDifficultyLevelAdvancer))
				{
					testDifficultyLevelAdvancer = gameSceneManager.gameObject.AddComponent<TestDifficultyLevelAdvancer>();
				}
				GameSessionManager gameSessionManager;
				if (this.currentDifficultyLevel < 32 && base.CurrentGame.Services.TryGet<GameSessionManager>(out gameSessionManager))
				{
					gameSessionManager.SessionStateChanged += this.OnGameSessionStateChanged;
				}
				testDifficultyLevelAdvancer.CurrentDifficultyLevel = this.currentDifficultyLevel;
				testDifficultyLevelAdvancer.CurrentData = this.difficultyAdvancerData;
				GameBehaviourBase.Created -= this.OnGameBehaviourCreated;
			}
		}

		// Token: 0x06001595 RID: 5525 RVA: 0x000452FA File Offset: 0x000434FA
		private void OnGameSessionStateChanged(IGameSessionManager sessionManager, SessionState sessionState)
		{
			if (this.advanceDifficultyOnVictory && sessionState == SessionState.Victory)
			{
				this.currentDifficultyLevel++;
			}
			if (!sessionManager.IsGameSessionInProgress)
			{
				sessionManager.SessionStateChanged -= this.OnGameSessionStateChanged;
			}
		}

		// Token: 0x04000C84 RID: 3204
		public bool isActive = true;

		// Token: 0x04000C85 RID: 3205
		public bool advanceDifficultyOnVictory;

		// Token: 0x04000C86 RID: 3206
		[Range(1f, 32f)]
		public int startDifficultyLevel = 1;

		// Token: 0x04000C87 RID: 3207
		public TestDifficultyLevelAdvancer.Data difficultyAdvancerData;

		// Token: 0x04000C88 RID: 3208
		private int currentDifficultyLevel;
	}
}
