using System;
using Game.Core;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace Unliving
{
	// Token: 0x02000014 RID: 20
	public interface IGameManager : IGameSessionInitializationManager
	{
		// Token: 0x1700004E RID: 78
		// (get) Token: 0x06000105 RID: 261
		bool IsSceneLoadingInProgress { get; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x06000106 RID: 262
		bool IsMainMenuLoaded { get; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x06000107 RID: 263
		bool IsHomespaceLoaded { get; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x06000108 RID: 264
		bool IsStartSceneLoaded { get; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x06000109 RID: 265
		IGameSessionManager GameSessionManager { get; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x0600010A RID: 266
		bool IsGameSessionInProgress { get; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x0600010B RID: 267
		bool IsNewGameRun { get; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x0600010C RID: 268
		bool IsProperGameRun { get; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x0600010D RID: 269
		bool IsRunningProperGameSession { get; }

		// Token: 0x14000015 RID: 21
		// (add) Token: 0x0600010E RID: 270
		// (remove) Token: 0x0600010F RID: 271
		event Action<IGameManager, AsyncOperationHandle<SceneInstance>, string, string> StartLoadingScene;

		// Token: 0x14000016 RID: 22
		// (add) Token: 0x06000110 RID: 272
		// (remove) Token: 0x06000111 RID: 273
		event Action<IGameManager, string> RestartingGameScene;

		// Token: 0x14000017 RID: 23
		// (add) Token: 0x06000112 RID: 274
		// (remove) Token: 0x06000113 RID: 275
		event Action<IGameManager, string> ChangingGameScene;

		// Token: 0x14000018 RID: 24
		// (add) Token: 0x06000114 RID: 276
		// (remove) Token: 0x06000115 RID: 277
		event Action<IGameManager> GameStarted;

		// Token: 0x06000116 RID: 278
		void LoadScene(string newSceneName, Action beforeLoadCallback = null);

		// Token: 0x06000117 RID: 279
		void PostponeSceneLoading(float delay = 0f);

		// Token: 0x06000118 RID: 280
		void LoadMainMenu();

		// Token: 0x06000119 RID: 281
		void LoadHomespace();

		// Token: 0x0600011A RID: 282
		void StartNewGame(bool forceProperGameRun = false);

		// Token: 0x0600011B RID: 283
		void ContinueOrStartNewGame();
	}
}
