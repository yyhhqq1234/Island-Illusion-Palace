using System;
using System.Collections.Generic;
using Common;
using Common.Scene;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Abilities;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving
{
	// Token: 0x02000012 RID: 18
	[Service(typeof(PlayerHomespaceManager), new Type[]
	{
		typeof(IGameSessionManager),
		typeof(IPlayerProvider),
		typeof(IGameLocationProvider)
	})]
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	public sealed class PlayerHomespaceManager : GlobalSceneManagerBase, IPlayerProvider, IGameSessionManager, IDestroyable, IGameLocationProvider
	{
		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000DC RID: 220 RVA: 0x000048EA File Offset: 0x00002AEA
		public PlayerBehaviour CurrentPlayer
		{
			get
			{
				return this.currentPlayer;
			}
		}

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000DD RID: 221 RVA: 0x000048F2 File Offset: 0x00002AF2
		public SessionState CurrentSessionState
		{
			get
			{
				return this.currentSessionState;
			}
		}

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000DE RID: 222 RVA: 0x000048FA File Offset: 0x00002AFA
		public bool IsGameSessionInProgress
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000DF RID: 223 RVA: 0x000048FD File Offset: 0x00002AFD
		public bool IsGameSessionFinalized
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000E0 RID: 224 RVA: 0x00004900 File Offset: 0x00002B00
		public bool IsWaitingForPlayerTransition
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000E1 RID: 225 RVA: 0x00004903 File Offset: 0x00002B03
		public string NextSceneName
		{
			get
			{
				return this.newGameTrigger ? this.newGameTrigger.nextScene : null;
			}
		}

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000E2 RID: 226 RVA: 0x00004925 File Offset: 0x00002B25
		public IReadOnlyList<BaseGameMob> RegisteredMobs
		{
			get
			{
				return this.homespaceMobs ?? Array.Empty<BaseGameMob>();
			}
		}

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000E3 RID: 227 RVA: 0x00004936 File Offset: 0x00002B36
		// (set) Token: 0x060000E4 RID: 228 RVA: 0x0000493E File Offset: 0x00002B3E
		public bool IsCutsceneActive { get; private set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000E5 RID: 229 RVA: 0x00004947 File Offset: 0x00002B47
		// (set) Token: 0x060000E6 RID: 230 RVA: 0x0000494F File Offset: 0x00002B4F
		public object CurrentCutsceneContext { get; private set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000E7 RID: 231 RVA: 0x00004958 File Offset: 0x00002B58
		public bool IsVictoryStateReached
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000E8 RID: 232 RVA: 0x0000495B File Offset: 0x00002B5B
		GameLocation IGameLocationProvider.CurrentLocation
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000E9 RID: 233 RVA: 0x0000495E File Offset: 0x00002B5E
		GameLocation.TypeID IGameLocationProvider.LocationType
		{
			get
			{
				return GameLocation.TypeID.Homespace;
			}
		}

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000EA RID: 234 RVA: 0x00004961 File Offset: 0x00002B61
		string IGameLocationProvider.LevelID
		{
			get
			{
				return this.homespaceLevelID;
			}
		}

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000EB RID: 235 RVA: 0x00004969 File Offset: 0x00002B69
		public float LocationGenerationProgress
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x14000010 RID: 16
		// (add) Token: 0x060000EC RID: 236 RVA: 0x00004970 File Offset: 0x00002B70
		// (remove) Token: 0x060000ED RID: 237 RVA: 0x000049A8 File Offset: 0x00002BA8
		public event Action<Action> BeforeGameStarted;

		// Token: 0x14000011 RID: 17
		// (add) Token: 0x060000EE RID: 238 RVA: 0x000049E0 File Offset: 0x00002BE0
		// (remove) Token: 0x060000EF RID: 239 RVA: 0x00004A18 File Offset: 0x00002C18
		public event Action<PlayerBehaviour> PlayerRegistered;

		// Token: 0x14000012 RID: 18
		// (add) Token: 0x060000F0 RID: 240 RVA: 0x00004A50 File Offset: 0x00002C50
		// (remove) Token: 0x060000F1 RID: 241 RVA: 0x00004A88 File Offset: 0x00002C88
		public event Action<IGameSessionManager, SessionState> SessionStateChanged;

		// Token: 0x14000013 RID: 19
		// (add) Token: 0x060000F2 RID: 242 RVA: 0x00004AC0 File Offset: 0x00002CC0
		// (remove) Token: 0x060000F3 RID: 243 RVA: 0x00004AF8 File Offset: 0x00002CF8
		public event Action<BaseGameMob> MobRegistered;

		// Token: 0x14000014 RID: 20
		// (add) Token: 0x060000F4 RID: 244 RVA: 0x00004B30 File Offset: 0x00002D30
		// (remove) Token: 0x060000F5 RID: 245 RVA: 0x00004B68 File Offset: 0x00002D68
		public event Action<BaseGameMob> MobUnregistered;

		// Token: 0x060000F6 RID: 246 RVA: 0x00004B9D File Offset: 0x00002D9D
		private void ResetPlayerTrigger(PlayerAreaTrigger trigger)
		{
			trigger.nextScene = new SceneAssetReference(null);
			trigger.isSceneTransitionArea = false;
			trigger.mobToSpawn = MobBehaviour.ID.None;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00004BBC File Offset: 0x00002DBC
		public void RegisterPlayer(PlayerBehaviour player)
		{
			if (player.IsNull() || this.currentPlayer == player)
			{
				return;
			}
			if (this.homespaceMobs == null)
			{
				this.homespaceMobs = new BaseGameMob[1];
			}
			this.currentPlayer = player;
			Action<PlayerBehaviour> playerRegistered = this.PlayerRegistered;
			if (playerRegistered != null)
			{
				playerRegistered(player);
			}
			this.currentPlayer.AbilitiesController.SetAllowedAbilityDescription(this.allowedAbilitiesDescription, true);
			this.currentPlayer.PlayerInputController.PlayerActionPerformed += this.OnPlayerActionPerformed;
			this.homespaceMobs[0] = player;
			this.SetSessionState(SessionState.InProgress);
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00004C51 File Offset: 0x00002E51
		private void OnPlayerActionPerformed(PlayerInputController.ActionArgs args)
		{
			if (!args.HasActionFlag(PlayerAction.UI_CANCEL))
			{
				return;
			}
			if (this.currentSessionState == SessionState.InProgress)
			{
				this.SetSessionState(SessionState.Paused);
				return;
			}
			if (this.currentSessionState == SessionState.Paused)
			{
				this.SetSessionState(SessionState.InProgress);
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00004C80 File Offset: 0x00002E80
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.gameManager = currentGame.Services.Get<GameManager>();
			PlayerProfileManager playerProfileManager;
			if (currentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				playerProfileManager.SavePlayerProfile(null);
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00004CBC File Offset: 0x00002EBC
		private void Start()
		{
			if (this.gameManager != null && this.newGameTrigger != null)
			{
				this.ResetPlayerTrigger(this.newGameTrigger);
				this.newGameTrigger.Activated.AddListener(delegate()
				{
					if (this.BeforeGameStarted == null)
					{
						this.StartNewGame();
						return;
					}
					Action<Action> beforeGameStarted = this.BeforeGameStarted;
					if (beforeGameStarted == null)
					{
						return;
					}
					beforeGameStarted(new Action(this.StartNewGame));
				});
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x00004D0D File Offset: 0x00002F0D
		public void StartNewGame()
		{
			this.gameManager.StartNewGame(false);
		}

		// Token: 0x060000FC RID: 252 RVA: 0x00004D1B File Offset: 0x00002F1B
		public void TryRestartCurrentGame(bool force = false)
		{
			SceneManager.LoadScene(GameManager.CurrentSceneName);
		}

		// Token: 0x060000FD RID: 253 RVA: 0x00004D28 File Offset: 0x00002F28
		public bool SetSessionState(SessionState state)
		{
			if (this.currentSessionState == state)
			{
				return false;
			}
			if (state == SessionState.Exited)
			{
				return false;
			}
			this.currentSessionState = state;
			bool flag = state == SessionState.Paused || state == SessionState.Freezed;
			GameManager.SetGameFreezed(flag);
			if (state != SessionState.Cutscene && !this.currentPlayer.IsNull())
			{
				this.currentPlayer.PlayerInputController.IsActive = !flag;
			}
			Action<IGameSessionManager, SessionState> sessionStateChanged = this.SessionStateChanged;
			if (sessionStateChanged != null)
			{
				sessionStateChanged(this, this.currentSessionState);
			}
			return true;
		}

		// Token: 0x060000FE RID: 254 RVA: 0x00004D9D File Offset: 0x00002F9D
		public void RegisterMob(BaseGameMob mob)
		{
		}

		// Token: 0x060000FF RID: 255 RVA: 0x00004D9F File Offset: 0x00002F9F
		public void UnregisterMob(BaseGameMob mob)
		{
		}

		// Token: 0x06000100 RID: 256 RVA: 0x00004DA1 File Offset: 0x00002FA1
		public bool IsEnemyMob(BaseGameMob mob)
		{
			return false;
		}

		// Token: 0x06000101 RID: 257 RVA: 0x00004DA4 File Offset: 0x00002FA4
		public void SetCutsceneActive(bool isActive, object context)
		{
			this.IsCutsceneActive = isActive;
			this.CurrentCutsceneContext = (isActive ? context : null);
			if (isActive)
			{
				this.SetSessionState(SessionState.Cutscene);
			}
			else
			{
				this.SetSessionState(SessionState.InProgress);
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.SetCutsceneActive(isActive, context);
			}
		}

		// Token: 0x06000102 RID: 258 RVA: 0x00004DF4 File Offset: 0x00002FF4
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.PlayerInputController.PlayerActionPerformed -= this.OnPlayerActionPerformed;
			}
		}

		// Token: 0x04000065 RID: 101
		public PlayerAreaTrigger newGameTrigger;

		// Token: 0x04000066 RID: 102
		[HideInInspector]
		public PlayerAreaTrigger continueGameTrigger;

		// Token: 0x04000067 RID: 103
		public string homespaceLevelID = "level_homespace_01";

		// Token: 0x04000068 RID: 104
		public AbilityDescription allowedAbilitiesDescription = new AbilityDescription
		{
			abilityID = AbilityID.Dash
		};

		// Token: 0x04000069 RID: 105
		private BaseGameMob[] homespaceMobs;

		// Token: 0x0400006A RID: 106
		private GameManager gameManager;

		// Token: 0x0400006B RID: 107
		private PlayerBehaviour currentPlayer;

		// Token: 0x0400006C RID: 108
		private SessionState currentSessionState = SessionState.Undefined;
	}
}
