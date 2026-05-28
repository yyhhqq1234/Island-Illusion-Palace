using System;
using System.Collections;
using System.IO;
using Common;
using Common.RestorableState;
using Common.ServiceRegistry;
using Game.BundlesCache;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Unliving.LevelGeneration;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.GameScene
{
	// Token: 0x0200025D RID: 605
	[Service(typeof(GameSceneManager), new Type[]
	{
		typeof(IGameLocationProvider)
	})]
	[DefaultExecutionOrder(-120)]
	[DisallowMultipleComponent]
	public sealed class GameSceneManager : GlobalSceneManagerBase, IGameLocationProvider
	{
		// Token: 0x17000441 RID: 1089
		// (get) Token: 0x06001412 RID: 5138 RVA: 0x0003F3C6 File Offset: 0x0003D5C6
		// (set) Token: 0x06001413 RID: 5139 RVA: 0x0003F3CE File Offset: 0x0003D5CE
		public ILocationChunkVisitor LocationExplorer
		{
			get
			{
				return this.locationExplorer;
			}
			set
			{
				this.locationExplorer = value;
				if (this.generatedLocation != null)
				{
					this.generatedLocation.LocationExplorer = this.locationExplorer;
				}
			}
		}

		// Token: 0x17000442 RID: 1090
		// (get) Token: 0x06001414 RID: 5140 RVA: 0x0003F3F0 File Offset: 0x0003D5F0
		public float LocationGenerationProgress
		{
			get
			{
				return this.currentLocationGenerator.GenerationProgress;
			}
		}

		// Token: 0x17000443 RID: 1091
		// (get) Token: 0x06001415 RID: 5141 RVA: 0x0003F3FD File Offset: 0x0003D5FD
		public GameLocationGenerator CurrentLocationGenerator
		{
			get
			{
				return this.currentLocationGenerator;
			}
		}

		// Token: 0x17000444 RID: 1092
		// (get) Token: 0x06001416 RID: 5142 RVA: 0x0003F405 File Offset: 0x0003D605
		public GameLocation GeneratedLocation
		{
			get
			{
				return this.generatedLocation;
			}
		}

		// Token: 0x17000445 RID: 1093
		// (get) Token: 0x06001417 RID: 5143 RVA: 0x0003F40D File Offset: 0x0003D60D
		GameLocation IGameLocationProvider.CurrentLocation
		{
			get
			{
				return this.generatedLocation;
			}
		}

		// Token: 0x17000446 RID: 1094
		// (get) Token: 0x06001418 RID: 5144 RVA: 0x0003F415 File Offset: 0x0003D615
		GameLocation.TypeID IGameLocationProvider.LocationType
		{
			get
			{
				return this.currentLocationGenerator.locationType;
			}
		}

		// Token: 0x17000447 RID: 1095
		// (get) Token: 0x06001419 RID: 5145 RVA: 0x0003F422 File Offset: 0x0003D622
		string IGameLocationProvider.LevelID
		{
			get
			{
				return this.currentLocationGenerator.levelID;
			}
		}

		// Token: 0x140000C6 RID: 198
		// (add) Token: 0x0600141A RID: 5146 RVA: 0x0003F430 File Offset: 0x0003D630
		// (remove) Token: 0x0600141B RID: 5147 RVA: 0x0003F468 File Offset: 0x0003D668
		public event Action<GameSceneManager> LocationGenerated;

		// Token: 0x0600141C RID: 5148 RVA: 0x0003F49D File Offset: 0x0003D69D
		private IEnumerator GenerationCallbackRoutine(Action<GameSceneManager> locationGeneratedCallback)
		{
			while (this.generatedLocation == null)
			{
				yield return null;
			}
			locationGeneratedCallback(this);
			yield break;
		}

		// Token: 0x0600141D RID: 5149 RVA: 0x0003F4B3 File Offset: 0x0003D6B3
		public void InvokeAfterLocationGenerated(Action<GameSceneManager> locationGeneratedCallback)
		{
			if (locationGeneratedCallback == null)
			{
				return;
			}
			if (this.generatedLocation != null)
			{
				if (locationGeneratedCallback != null)
				{
					locationGeneratedCallback(this);
				}
				return;
			}
			base.StartCoroutine(this.GenerationCallbackRoutine(locationGeneratedCallback));
		}

		// Token: 0x0600141E RID: 5150 RVA: 0x0003F4DC File Offset: 0x0003D6DC
		public async void GenerateLocation()
		{
			if (this.generatedLocation == null)
			{
				IBundlesCacheManager cacheManager;
				if (base.CurrentGame.Services.TryGet<IBundlesCacheManager>(out cacheManager))
				{
					this.locationGenerationSettings.cacheManager = cacheManager;
				}
				await this.currentLocationGenerator.GenerateLocation(this.locationGenerationSettings, delegate(IGameLocation loc)
				{
					this.generatedLocation = (GameLocation)loc;
				});
				if (this.generatedLocation != null)
				{
					this.generatedLocation.IsTutorialLocation = this.isTutorialLocation;
					this.generatedLocation.LocationExplorer = this.locationExplorer;
					Action<GameSceneManager> locationGenerated = this.LocationGenerated;
					if (locationGenerated != null)
					{
						locationGenerated(this);
					}
				}
				else
				{
					base.StopAllCoroutines();
				}
				LocationChunksVisibilityController locationChunksVisibilityController;
				if (!base.TryGetComponent<LocationChunksVisibilityController>(out locationChunksVisibilityController))
				{
					locationChunksVisibilityController = base.gameObject.AddComponent<LocationChunksVisibilityController>();
				}
				locationChunksVisibilityController.returnStrayedMobs = this.locationGenerationSettings.returnStrayedMobs;
				locationChunksVisibilityController.keepAllChunksVisible = this.locationGenerationSettings.generateAlwaysVisibleChunks;
			}
		}

		// Token: 0x0600141F RID: 5151 RVA: 0x0003F518 File Offset: 0x0003D718
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.currentLocationGenerator = this.locationGenerator;
			this.isTutorialLocation = false;
			PlayerProfileManager playerProfileManager;
			if (!this.forceIgnoreTutorialLocationGenerator && this.tutorialLocationGenerator != null && this.tutorialLocationGenerator.IsActive && currentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager) && playerProfileManager.CurrentPlayerProfile != null && playerProfileManager.CurrentPlayerProfile.LaunchTutorial)
			{
				this.currentLocationGenerator = this.tutorialLocationGenerator;
				this.isTutorialLocation = true;
			}
			GameManager gameManager;
			if (currentGame.Services.TryGet<GameManager>(out gameManager))
			{
				GameLocation.TypeID locationID = gameManager.GetLocationID(GameManager.CurrentSceneName);
				if (locationID != GameLocation.TypeID.Undefined)
				{
					this.currentLocationGenerator.locationType = locationID;
				}
			}
			this.currentLocationGenerator.StartPosition = base.transform.position;
			this.currentLocationGenerator.GeneratedChunksRoot = this.customGeneratedChunksRoot;
			GameSceneManager.RestorableState globalData = currentGame.GetGlobalData<GameSceneManager.RestorableState>();
			if (globalData != null)
			{
				globalData.Restore(this, null);
			}
			this.GenerateLocation();
		}

		// Token: 0x06001420 RID: 5152 RVA: 0x0003F602 File Offset: 0x0003D802
		protected override void OnDestroy()
		{
			base.OnDestroy();
			GameLocationGenerator gameLocationGenerator = this.currentLocationGenerator;
			if (gameLocationGenerator == null)
			{
				return;
			}
			gameLocationGenerator.Cleanup();
		}

		// Token: 0x06001421 RID: 5153 RVA: 0x0003F61C File Offset: 0x0003D81C
		private void OnGUI()
		{
			if (this.locationGenerationSettings.generateAlwaysVisibleChunks)
			{
				Rect position = new Rect(5f, 5f, (float)Screen.width * 0.4f, (float)(Screen.height / 50 + 10));
				GUIStyle guistyle = new GUIStyle("box")
				{
					fontSize = Screen.height / 50,
					alignment = TextAnchor.MiddleLeft
				};
				guistyle.normal.textColor = Color.red;
				GUI.Label(position, "WARNING: GenerateAlwaysVisibleChunks option is enabled.", guistyle);
			}
		}

		// Token: 0x04000BB7 RID: 2999
		public bool forceIgnoreTutorialLocationGenerator;

		// Token: 0x04000BB8 RID: 3000
		[Space]
		[SerializeField]
		private GameLocationGenerator tutorialLocationGenerator;

		// Token: 0x04000BB9 RID: 3001
		[Space]
		[SerializeField]
		[FormerlySerializedAs("_locationGenerator")]
		private GameLocationGenerator locationGenerator;

		// Token: 0x04000BBA RID: 3002
		public GameLocationGenerator.GenerationArgs locationGenerationSettings;

		// Token: 0x04000BBB RID: 3003
		public Transform customGeneratedChunksRoot;

		// Token: 0x04000BBC RID: 3004
		private GameLocationGenerator currentLocationGenerator;

		// Token: 0x04000BBD RID: 3005
		private ILocationChunkVisitor locationExplorer;

		// Token: 0x04000BBE RID: 3006
		private GameLocation generatedLocation;

		// Token: 0x04000BBF RID: 3007
		private bool isTutorialLocation;

		// Token: 0x020004D6 RID: 1238
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<GameSceneManager>, ICloneable<GameSceneManager.RestorableState>
		{
			// Token: 0x06002564 RID: 9572 RVA: 0x00073DB5 File Offset: 0x00071FB5
			public RestorableState(GameSceneManager sceneManager) : base(sceneManager)
			{
			}

			// Token: 0x06002565 RID: 9573 RVA: 0x00073DC0 File Offset: 0x00071FC0
			public override void Store(GameSceneManager sceneManager)
			{
				if (sceneManager != null)
				{
					Scene sceneByName = SceneManager.GetSceneByName(GameManager.CurrentSceneName);
					this.sceneName = sceneByName.name;
					this.sceneIndex = (sceneByName.IsValid() ? new int?(sceneByName.buildIndex) : null);
					this.sceneSeed = sceneManager.CurrentLocationGenerator.UsedSeed;
					this.useSceneSeed = true;
					GameManager gameManager;
					if (sceneManager.CurrentGame.Services.TryGet<GameManager>(out gameManager))
					{
						GameLocation.TypeID typeID = gameManager.GetLocationID(this.sceneName);
						this.locationID = ((typeID != GameLocation.TypeID.Undefined) ? new GameLocation.TypeID?(typeID) : null);
						return;
					}
				}
				else
				{
					this.Reset();
				}
			}

			// Token: 0x06002566 RID: 9574 RVA: 0x00073E74 File Offset: 0x00072074
			public void Store(PlayerBehaviour player)
			{
				GameSceneManager targetObject;
				if (player.CurrentGame.Services.TryGet<GameSceneManager>(out targetObject))
				{
					this.Store(targetObject);
				}
			}

			// Token: 0x06002567 RID: 9575 RVA: 0x00073E9C File Offset: 0x0007209C
			public override void Restore(GameSceneManager sceneManager, object args = null)
			{
				if (this.useSceneSeed)
				{
					sceneManager.locationGenerationSettings.customSeed = this.sceneSeed;
					sceneManager.locationGenerationSettings.useCustomSeed = true;
				}
			}

			// Token: 0x06002568 RID: 9576 RVA: 0x00073EC4 File Offset: 0x000720C4
			public string GetActualSceneName(GameManager gameManager)
			{
				if (this.locationID != null && gameManager != null)
				{
					string text = gameManager.GetSceneName(this.locationID.Value);
					if (!string.IsNullOrEmpty(text))
					{
						return text;
					}
				}
				if (SceneUtility.GetBuildIndexByScenePath(this.sceneName) >= 0)
				{
					return this.sceneName;
				}
				if (this.sceneIndex != null)
				{
					string scenePathByBuildIndex = SceneUtility.GetScenePathByBuildIndex(this.sceneIndex.Value);
					if (!string.IsNullOrEmpty(scenePathByBuildIndex))
					{
						return Path.GetFileNameWithoutExtension(scenePathByBuildIndex);
					}
				}
				string text2 = this.sceneName;
				if (text2 != null)
				{
					if (text2 == "CemeteryPlayTestLessPlayTime")
					{
						return "Master 1. Cemetery";
					}
					if (text2 == "SwampLessPlayTimePlaytest")
					{
						return "Master 2. Swamp";
					}
					if (text2 == "CityPlayTestLessPlayTime")
					{
						return "Master 3. City";
					}
				}
				return this.sceneName;
			}

			// Token: 0x06002569 RID: 9577 RVA: 0x00073F98 File Offset: 0x00072198
			public bool IsValid()
			{
				int? num = this.sceneIndex;
				int num2 = 0;
				return (num.GetValueOrDefault() >= num2 & num != null) || !string.IsNullOrWhiteSpace(this.sceneName);
			}

			// Token: 0x0600256A RID: 9578 RVA: 0x00073FD5 File Offset: 0x000721D5
			public GameSceneManager.RestorableState Clone()
			{
				return (GameSceneManager.RestorableState)base.MemberwiseClone();
			}

			// Token: 0x0600256B RID: 9579 RVA: 0x00073FE2 File Offset: 0x000721E2
			public void Reset()
			{
				this.locationID = null;
				this.sceneName = string.Empty;
				this.sceneIndex = null;
				this.sceneSeed = 0;
				this.useSceneSeed = false;
			}

			// Token: 0x0600256C RID: 9580 RVA: 0x00074015 File Offset: 0x00072215
			public override string ToString()
			{
				if (!this.IsValid())
				{
					return "NULL";
				}
				return string.Format("{0}_{1} {2}", this.sceneName, this.sceneIndex, this.sceneSeed);
			}

			// Token: 0x040019E3 RID: 6627
			public GameLocation.TypeID? locationID;

			// Token: 0x040019E4 RID: 6628
			public string sceneName;

			// Token: 0x040019E5 RID: 6629
			public int? sceneIndex;

			// Token: 0x040019E6 RID: 6630
			public int sceneSeed;

			// Token: 0x040019E7 RID: 6631
			public bool useSceneSeed;
		}
	}
}
