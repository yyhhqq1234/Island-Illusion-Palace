using System;
using System.Runtime.CompilerServices;
using Common.Scene;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving
{
	// Token: 0x0200000C RID: 12
	[CreateAssetMenu(fileName = "GameManager", menuName = "Game/Global/Game Manager")]
	[Service(typeof(GameManager), new Type[]
	{
		typeof(IGameManager),
		typeof(IGameSessionInitializationManager)
	})]
	public sealed class GameManager : GlobalManagerBase, IGameManager, IGameSessionInitializationManager
	{
		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600002B RID: 43 RVA: 0x00002A3C File Offset: 0x00000C3C
		public static string CurrentSceneName
		{
			get
			{
				return SceneManager.GetActiveScene().name;
			}
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002A56 File Offset: 0x00000C56
		public static bool IsValidScene(string sceneName)
		{
			return !string.IsNullOrEmpty(sceneName) && SceneUtility.GetBuildIndexByScenePath(sceneName) >= 0;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00002A6E File Offset: 0x00000C6E
		public static void SetGameFreezed(bool isFreezed)
		{
			if (GameManager.defaultTimeScale == null)
			{
				GameManager.defaultTimeScale = new float?(Time.timeScale);
			}
			Time.timeScale = (isFreezed ? 1E-06f : GameManager.defaultTimeScale.Value);
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600002E RID: 46 RVA: 0x00002AA4 File Offset: 0x00000CA4
		public bool IsFullyInitialized
		{
			get
			{
				return base.CurrentGame != null && (this.IsMainMenuLoaded || this.IsInitializationSceneLoaded || ((!this.gameSessionManager.IsNull() || base.CurrentGame.Services.TryGet<IGameSessionManager>(out this.gameSessionManager)) && this.gameSessionManager.CurrentPlayer != null));
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600002F RID: 47 RVA: 0x00002B05 File Offset: 0x00000D05
		// (set) Token: 0x06000030 RID: 48 RVA: 0x00002B0D File Offset: 0x00000D0D
		public string ExpectedSceneName { get; private set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000031 RID: 49 RVA: 0x00002B16 File Offset: 0x00000D16
		public string MainMenuSceneName
		{
			get
			{
				return this.mainmenuScene.SceneName;
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000032 RID: 50 RVA: 0x00002B23 File Offset: 0x00000D23
		public string HomespaceSceneName
		{
			get
			{
				return this.homespaceScene.SceneName;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000033 RID: 51 RVA: 0x00002B30 File Offset: 0x00000D30
		public string StartSceneName
		{
			get
			{
				return this.startScene.SceneName;
			}
		}

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x06000034 RID: 52 RVA: 0x00002B3D File Offset: 0x00000D3D
		// (set) Token: 0x06000035 RID: 53 RVA: 0x00002B45 File Offset: 0x00000D45
		public bool IsSceneLoadingInProgress { get; private set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000036 RID: 54 RVA: 0x00002B4E File Offset: 0x00000D4E
		public bool IsInitializationSceneLoaded
		{
			get
			{
				return this.initializationScene.SceneName == GameManager.CurrentSceneName;
			}
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00002B65 File Offset: 0x00000D65
		public bool IsMainMenuLoaded
		{
			get
			{
				return this.mainmenuScene.SceneName == GameManager.CurrentSceneName;
			}
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000038 RID: 56 RVA: 0x00002B7C File Offset: 0x00000D7C
		public bool IsHomespaceLoaded
		{
			get
			{
				return this.homespaceScene.SceneName == GameManager.CurrentSceneName;
			}
		}

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002B93 File Offset: 0x00000D93
		public bool IsStartSceneLoaded
		{
			get
			{
				return this.startScene.SceneName == GameManager.CurrentSceneName;
			}
		}

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x0600003A RID: 58 RVA: 0x00002BAA File Offset: 0x00000DAA
		public IGameSessionManager GameSessionManager
		{
			get
			{
				return this.gameSessionManager;
			}
		}

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002BB2 File Offset: 0x00000DB2
		public bool IsGameSessionInProgress
		{
			get
			{
				IGameSessionManager gameSessionManager = this.gameSessionManager;
				return gameSessionManager != null && gameSessionManager.IsGameSessionInProgress;
			}
		}

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600003C RID: 60 RVA: 0x00002BC5 File Offset: 0x00000DC5
		public bool IsNewGameRun
		{
			get
			{
				return this.isNewGameRun;
			}
		}

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002BCD File Offset: 0x00000DCD
		public bool IsProperGameRun
		{
			get
			{
				return this.isProperGameRun;
			}
		}

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600003E RID: 62 RVA: 0x00002BD5 File Offset: 0x00000DD5
		public bool IsRunningProperGameSession
		{
			get
			{
				return this.isProperGameRun && this.IsGameSessionInProgress;
			}
		}

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x0600003F RID: 63 RVA: 0x00002BE8 File Offset: 0x00000DE8
		// (remove) Token: 0x06000040 RID: 64 RVA: 0x00002C20 File Offset: 0x00000E20
		public event Action<IGameManager, AsyncOperationHandle<SceneInstance>, string, string> StartLoadingScene;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000041 RID: 65 RVA: 0x00002C58 File Offset: 0x00000E58
		// (remove) Token: 0x06000042 RID: 66 RVA: 0x00002C90 File Offset: 0x00000E90
		public event Action<IGameManager, string> RestartingGameScene;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000043 RID: 67 RVA: 0x00002CC8 File Offset: 0x00000EC8
		// (remove) Token: 0x06000044 RID: 68 RVA: 0x00002D00 File Offset: 0x00000F00
		public event Action<IGameManager, string> ChangingGameScene;

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000045 RID: 69 RVA: 0x00002D38 File Offset: 0x00000F38
		// (remove) Token: 0x06000046 RID: 70 RVA: 0x00002D70 File Offset: 0x00000F70
		public event Action<IGameManager> GameStarted;

		// Token: 0x06000047 RID: 71 RVA: 0x00002DA8 File Offset: 0x00000FA8
		private void StartSceneLoading(string newSceneName)
		{
			GameManager.<StartSceneLoading>d__62 <StartSceneLoading>d__;
			<StartSceneLoading>d__.<>4__this = this;
			<StartSceneLoading>d__.newSceneName = newSceneName;
			<StartSceneLoading>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<StartSceneLoading>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <StartSceneLoading>d__.<>t__builder;
			<>t__builder.Start<GameManager.<StartSceneLoading>d__62>(ref <StartSceneLoading>d__);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00002DE9 File Offset: 0x00000FE9
		public bool IsInitializationScene(Scene scene)
		{
			return string.Equals(this.initializationScene.SceneName, scene.name);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00002E02 File Offset: 0x00001002
		private bool IsSpecialSceneLoaded()
		{
			return this.IsHomespaceLoaded || this.IsMainMenuLoaded;
		}

		// Token: 0x0600004A RID: 74 RVA: 0x00002E14 File Offset: 0x00001014
		private void StoreSceneState(GameSceneManager.RestorableState sceneState)
		{
			if (sceneState != null && sceneState.IsValid())
			{
				base.CurrentGame.AddGlobalData(sceneState.Clone());
			}
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00002E34 File Offset: 0x00001034
		private void RemoveStoredSceneState()
		{
			GameSceneManager.RestorableState globalData = base.CurrentGame.GetGlobalData<GameSceneManager.RestorableState>();
			base.CurrentGame.RemoveGlobalData(globalData);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00002E59 File Offset: 0x00001059
		private void ResetLocalPlayerProgress()
		{
			PlayerProfileManager playerProfileManager = this.playerProfileManager;
			if (playerProfileManager == null)
			{
				return;
			}
			playerProfileManager.ResetLocalPlayerProgress();
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00002E6C File Offset: 0x0000106C
		private bool TryExitGameSession()
		{
			if (this.gameSessionManager != null)
			{
				SessionState sessionState = SessionState.Exited;
				return this.gameSessionManager.CurrentSessionState == sessionState || this.gameSessionManager.SetSessionState(sessionState);
			}
			return false;
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00002EA4 File Offset: 0x000010A4
		public GameLocation.TypeID GetLocationID(string sceneName)
		{
			for (int i = 0; i < this.gameLocationScenes.Length; i++)
			{
				ref GameManager.LocationSceneInfo ptr = ref this.gameLocationScenes[i];
				if (ptr.scene.HasName(sceneName))
				{
					return ptr.locationID;
				}
			}
			return GameLocation.TypeID.Undefined;
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00002EE8 File Offset: 0x000010E8
		public string GetSceneName(GameLocation.TypeID locationID)
		{
			for (int i = 0; i < this.gameLocationScenes.Length; i++)
			{
				ref GameManager.LocationSceneInfo ptr = ref this.gameLocationScenes[i];
				if (ptr.locationID == locationID)
				{
					return ptr.scene.SceneName;
				}
			}
			return string.Empty;
		}

		// Token: 0x06000050 RID: 80 RVA: 0x00002F30 File Offset: 0x00001130
		public void LoadScene(string newSceneName, Action beforeLoadCallback = null)
		{
			if (this.IsSceneLoadingInProgress || GameApplication.IsGameStateChanging)
			{
				return;
			}
			if (GameManager.IsValidScene(newSceneName))
			{
				if (GameManager.CurrentSceneName == newSceneName)
				{
					PlayerProfileManager playerProfileManager = this.playerProfileManager;
					this.StoreSceneState((playerProfileManager != null) ? playerProfileManager.CurrentPlayerProfile.lastSceneInfo : null);
				}
				if (beforeLoadCallback != null)
				{
					beforeLoadCallback();
				}
				this.StartSceneLoading(newSceneName);
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x00002F90 File Offset: 0x00001190
		public void LoadScene(GameLocation.TypeID locationID, Action beforeLoadCallback = null)
		{
			string sceneName = this.GetSceneName(locationID);
			if (!string.IsNullOrEmpty(sceneName))
			{
				this.LoadScene(sceneName, beforeLoadCallback);
			}
		}

		// Token: 0x06000052 RID: 82 RVA: 0x00002FB8 File Offset: 0x000011B8
		public bool IsNormalGameSceneLoaded(out bool isTutorialScene, bool allowTutorial = false)
		{
			if (!this.IsSpecialSceneLoaded())
			{
				IGameLocationProvider gameLocationProvider = base.CurrentGame.Services.Get<IGameLocationProvider>();
				GameLocation gameLocation = (gameLocationProvider != null) ? gameLocationProvider.CurrentLocation : null;
				if (gameLocation != null)
				{
					isTutorialScene = gameLocation.IsTutorialLocation;
					return allowTutorial || !isTutorialScene;
				}
			}
			isTutorialScene = false;
			return false;
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00003004 File Offset: 0x00001204
		public void PostponeSceneLoading(float delay = 0f)
		{
			float num = Time.realtimeSinceStartup + Mathf.Max(delay, 0.1f);
			if (num > this.sceneLoadingStartTime)
			{
				this.sceneLoadingStartTime = num;
			}
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00003033 File Offset: 0x00001233
		public void LoadMainMenu()
		{
			if (this.TryExitGameSession())
			{
				return;
			}
			if (this.mainmenuScene.IsValid())
			{
				this.LoadScene(this.mainmenuScene, null);
			}
		}

		// Token: 0x06000055 RID: 85 RVA: 0x0000305D File Offset: 0x0000125D
		public void LoadHomespace()
		{
			if (this.homespaceScene.IsValid())
			{
				this.LoadScene(this.homespaceScene, null);
			}
		}

		// Token: 0x06000056 RID: 86 RVA: 0x0000307E File Offset: 0x0000127E
		public void StartNewGame(bool forceProperGameRun = false)
		{
			if (this.startScene.IsValid())
			{
				this.isNewGameRun = true;
				if (forceProperGameRun)
				{
					this.isProperGameRun = true;
				}
				this.LoadScene(this.startScene, new Action(this.ResetLocalPlayerProgress));
			}
		}

		// Token: 0x06000057 RID: 87 RVA: 0x000030BC File Offset: 0x000012BC
		public void ContinueOrStartNewGame()
		{
			PlayerProfileManager playerProfileManager = this.playerProfileManager;
			PlayerProfile playerProfile = (playerProfileManager != null) ? playerProfileManager.CurrentPlayerProfile : null;
			if (playerProfile == null)
			{
				return;
			}
			GameSceneManager.RestorableState restorableState;
			if (playerProfile.CanContinueGame(out restorableState))
			{
				this.isNewGameRun = false;
				this.isProperGameRun = true;
				this.playerProfileManager.LoadLastGamerunProfileState();
				this.StoreSceneState(restorableState);
				this.StartSceneLoading(restorableState.GetActualSceneName(this));
				return;
			}
			if (playerProfile.LaunchTutorial)
			{
				this.StartNewGame(true);
				return;
			}
			this.LoadHomespace();
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00003130 File Offset: 0x00001330
		public void UpdateLocalPlayerProgress()
		{
			IGameSessionManager gameSessionManager = this.gameSessionManager;
			PlayerBehaviour playerBehaviour = (gameSessionManager != null) ? gameSessionManager.CurrentPlayer : null;
			if (playerBehaviour != null)
			{
				PlayerProfileManager playerProfileManager = this.playerProfileManager;
				if (playerProfileManager == null)
				{
					return;
				}
				playerProfileManager.UpdateLocalPlayerProgress(playerBehaviour);
			}
		}

		// Token: 0x06000059 RID: 89 RVA: 0x0000316A File Offset: 0x0000136A
		public void KeepLastPlayerLocalProgressAndSaveProfile()
		{
			if (this.playerProfileManager != null && this.IsProperGameRun)
			{
				this.playerProfileManager.SavePlayerProfile(true);
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00003190 File Offset: 0x00001390
		public void SavePlayerProfile(bool saveLocalPlayerProgress)
		{
			if (this.playerProfileManager != null && this.IsProperGameRun)
			{
				PlayerBehaviour playerBehaviour;
				if (!saveLocalPlayerProgress)
				{
					playerBehaviour = null;
				}
				else
				{
					IGameSessionManager gameSessionManager = this.gameSessionManager;
					playerBehaviour = ((gameSessionManager != null) ? gameSessionManager.CurrentPlayer : null);
				}
				PlayerBehaviour playerToUpdateLocalProgress = playerBehaviour;
				this.playerProfileManager.SavePlayerProfile(playerToUpdateLocalProgress);
			}
		}

		// Token: 0x0600005B RID: 91 RVA: 0x000031D8 File Offset: 0x000013D8
		public void DeleteCurrentPlayerProfile()
		{
			PlayerProfileManager playerProfileManager = this.playerProfileManager;
			if (playerProfileManager == null)
			{
				return;
			}
			playerProfileManager.DeleteCurrentPlayerProfile();
		}

		// Token: 0x0600005C RID: 92 RVA: 0x000031EA File Offset: 0x000013EA
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.isProperGameRun = false;
			this.ExpectedSceneName = null;
			currentGame.Services.TryGet<PlayerProfileManager>(out this.playerProfileManager);
			GameApplication.QuittingGame += this.OnQuittingGame;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x00003224 File Offset: 0x00001424
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			this.IsSceneLoadingInProgress = false;
			GameApplication.OnSceneLoaded();
			this.RemoveStoredSceneState();
			if (this.isNewGameRun && this.IsSpecialSceneLoaded())
			{
				this.isNewGameRun = false;
			}
			if (!this.isProperGameRun)
			{
				this.isProperGameRun = this.IsHomespaceLoaded;
			}
			if (this.IsStartSceneLoaded)
			{
				Action<IGameManager> gameStarted = this.GameStarted;
				if (gameStarted != null)
				{
					gameStarted(this);
				}
			}
			Resources.UnloadUnusedAssets();
		}

		// Token: 0x0600005E RID: 94 RVA: 0x0000328E File Offset: 0x0000148E
		private void OnQuittingGame()
		{
			this.TryExitGameSession();
			GameApplication.QuittingGame -= this.OnQuittingGame;
		}

		// Token: 0x0400001B RID: 27
		private static float? defaultTimeScale;

		// Token: 0x04000022 RID: 34
		[SerializeField]
		private SceneAssetReference initializationScene;

		// Token: 0x04000023 RID: 35
		[SerializeField]
		private SceneAssetReference mainmenuScene;

		// Token: 0x04000024 RID: 36
		[SerializeField]
		[FormerlySerializedAs("_homespaceScene")]
		private SceneAssetReference homespaceScene;

		// Token: 0x04000025 RID: 37
		[SerializeField]
		[FormerlySerializedAs("_startScene")]
		private SceneAssetReference startScene;

		// Token: 0x04000026 RID: 38
		[SerializeField]
		private GameManager.LocationSceneInfo[] gameLocationScenes;

		// Token: 0x04000027 RID: 39
		private PlayerProfileManager playerProfileManager;

		// Token: 0x04000028 RID: 40
		private IGameSessionManager gameSessionManager;

		// Token: 0x04000029 RID: 41
		private bool isNewGameRun;

		// Token: 0x0400002A RID: 42
		private bool isProperGameRun;

		// Token: 0x0400002B RID: 43
		private float sceneLoadingStartTime;

		// Token: 0x020003F8 RID: 1016
		[Serializable]
		private struct LocationSceneInfo
		{
			// Token: 0x0400157A RID: 5498
			public GameLocation.TypeID locationID;

			// Token: 0x0400157B RID: 5499
			public SceneAssetReference scene;
		}
	}
}
