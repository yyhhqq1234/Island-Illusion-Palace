using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Scene;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.GameSession.PlayerLeveling;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Pickables;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving
{
	// Token: 0x0200000E RID: 14
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	[Service(typeof(IGameSessionManager), new Type[]
	{
		typeof(GameSessionManager),
		typeof(IPlayerProvider),
		typeof(IPlayerLevelingManager)
	})]
	public sealed class GameSessionManager : GlobalSceneManagerBase, IPlayerProvider, IGameSessionManager, IDestroyable, IPlayerLevelingManager
	{
		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000065 RID: 101 RVA: 0x00003402 File Offset: 0x00001602
		public IReadOnlyList<BaseGameMob> RegisteredMobs
		{
			get
			{
				if (this.registeredMobs == null)
				{
					this.registeredMobs = this.registeredMobsList.AsReadOnly();
				}
				return this.registeredMobs;
			}
		}

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000066 RID: 102 RVA: 0x00003423 File Offset: 0x00001623
		public GameSessionRules CurrentGameRules
		{
			get
			{
				return this._currentGameRules;
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000067 RID: 103 RVA: 0x0000342B File Offset: 0x0000162B
		public PlayerBehaviour CurrentPlayer
		{
			get
			{
				return this.currentPlayer;
			}
		}

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x06000068 RID: 104 RVA: 0x00003433 File Offset: 0x00001633
		public bool IsPlayerDestroyed
		{
			get
			{
				return this.isPlayerDestroyed;
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x06000069 RID: 105 RVA: 0x0000343B File Offset: 0x0000163B
		public int CurrentEnemyMobCount
		{
			get
			{
				return this.currentEnemyMobCount;
			}
		}

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600006A RID: 106 RVA: 0x00003443 File Offset: 0x00001643
		public int TotalEnemyMobCount
		{
			get
			{
				return this.totalEnemyMobCount;
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x0600006B RID: 107 RVA: 0x0000344B File Offset: 0x0000164B
		public int DestroyedEnemyMobCount
		{
			get
			{
				return this.destroyedEnemyMobCount;
			}
		}

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x0600006C RID: 108 RVA: 0x00003453 File Offset: 0x00001653
		public float CurrentEnemyMobsPopulation
		{
			get
			{
				if (this.totalEnemyMobCount <= 0)
				{
					return 0f;
				}
				return (float)this.currentEnemyMobCount / (float)this.totalEnemyMobCount;
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00003473 File Offset: 0x00001673
		public MobBehaviour.ID LastDestroyedMobID
		{
			get
			{
				return this.lastDestroyedMobID;
			}
		}

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x0600006E RID: 110 RVA: 0x0000347B File Offset: 0x0000167B
		public SessionState CurrentSessionState
		{
			get
			{
				return this.currentSessionState;
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00003483 File Offset: 0x00001683
		public bool IsGameSessionInProgress
		{
			get
			{
				return this.currentSessionState == SessionState.InProgress || this.currentSessionState == SessionState.Paused;
			}
		}

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x06000070 RID: 112 RVA: 0x00003498 File Offset: 0x00001698
		public bool IsGameSessionFinalized
		{
			get
			{
				return this.currentSessionState == SessionState.Victory || this.currentSessionState == SessionState.Defeat;
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x06000071 RID: 113 RVA: 0x000034AE File Offset: 0x000016AE
		public bool IsWaitingForPlayerTransition
		{
			get
			{
				return this.isWaitingForPlayerTransition;
			}
		}

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x06000072 RID: 114 RVA: 0x000034B6 File Offset: 0x000016B6
		public bool IsWaitingForSceneRestart
		{
			get
			{
				return this.isWaitingForSceneRestart;
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000073 RID: 115 RVA: 0x000034BE File Offset: 0x000016BE
		public bool IsCutsceneActive
		{
			get
			{
				return this.isCutsceneActive;
			}
		}

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000074 RID: 116 RVA: 0x000034C6 File Offset: 0x000016C6
		public bool IsVictoryStateReached
		{
			get
			{
				return this.isVictoryStateReached;
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000075 RID: 117 RVA: 0x000034CE File Offset: 0x000016CE
		public object CurrentCutsceneContext
		{
			get
			{
				return this.currentCutsceneContext;
			}
		}

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000076 RID: 118 RVA: 0x000034D6 File Offset: 0x000016D6
		public string NextSceneName
		{
			get
			{
				if (this.nextLocation != GameLocation.TypeID.Undefined && this.gameManager != null)
				{
					return this.gameManager.GetSceneName(this.nextLocation);
				}
				return this._nextScene;
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000077 RID: 119 RVA: 0x0000350B File Offset: 0x0000170B
		public int MaxPlayerLevel
		{
			get
			{
				return this._currentGameRules.MaxPlayerLevel;
			}
		}

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x06000078 RID: 120 RVA: 0x00003518 File Offset: 0x00001718
		public int LastPlayerLevelEXP
		{
			get
			{
				return this.lastPlayerLevelEXP;
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x06000079 RID: 121 RVA: 0x00003520 File Offset: 0x00001720
		public int NextPlayerLevelEXP
		{
			get
			{
				return this.nextPlayerLevelEXP;
			}
		}

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600007A RID: 122 RVA: 0x00003528 File Offset: 0x00001728
		public int DesiredPlayerLevel
		{
			get
			{
				return this.desiredPlayerLevel;
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00003530 File Offset: 0x00001730
		// (set) Token: 0x0600007C RID: 124 RVA: 0x0000353D File Offset: 0x0000173D
		public int CurrentPlayerLevel
		{
			get
			{
				return this.playerLevelState.currentLevel;
			}
			private set
			{
				this.playerLevelState.currentLevel = value;
			}
		}

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x0600007D RID: 125 RVA: 0x0000354B File Offset: 0x0000174B
		// (set) Token: 0x0600007E RID: 126 RVA: 0x00003558 File Offset: 0x00001758
		public int CurrentPlayerEXP
		{
			get
			{
				return this.playerLevelState.currentEXP;
			}
			private set
			{
				int currentEXP = this.playerLevelState.currentEXP;
				if (currentEXP == value)
				{
					return;
				}
				this.playerLevelState.currentEXP = value;
				Action<IPlayerLevelingManager, int, int> playerEXPChanged = this.PlayerEXPChanged;
				if (playerEXPChanged == null)
				{
					return;
				}
				playerEXPChanged(this, currentEXP, value);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x0600007F RID: 127 RVA: 0x00003598 File Offset: 0x00001798
		bool IPlayerLevelingManager.IsActive
		{
			get
			{
				bool flag;
				return !this.gameManager.IsNull() && this.gameManager.IsNormalGameSceneLoaded(out flag, true) && !flag;
			}
		}

		// Token: 0x14000005 RID: 5
		// (add) Token: 0x06000080 RID: 128 RVA: 0x000035C8 File Offset: 0x000017C8
		// (remove) Token: 0x06000081 RID: 129 RVA: 0x00003600 File Offset: 0x00001800
		public event Action<PlayerBehaviour> PlayerRegistered;

		// Token: 0x14000006 RID: 6
		// (add) Token: 0x06000082 RID: 130 RVA: 0x00003638 File Offset: 0x00001838
		// (remove) Token: 0x06000083 RID: 131 RVA: 0x00003670 File Offset: 0x00001870
		public event Action<BaseGameMob> MobRegistered;

		// Token: 0x14000007 RID: 7
		// (add) Token: 0x06000084 RID: 132 RVA: 0x000036A8 File Offset: 0x000018A8
		// (remove) Token: 0x06000085 RID: 133 RVA: 0x000036E0 File Offset: 0x000018E0
		public event Action<BaseGameMob> MobUnregistered;

		// Token: 0x14000008 RID: 8
		// (add) Token: 0x06000086 RID: 134 RVA: 0x00003718 File Offset: 0x00001918
		// (remove) Token: 0x06000087 RID: 135 RVA: 0x00003750 File Offset: 0x00001950
		public event Action<IGameSessionManager, SessionState> SessionStateChanged;

		// Token: 0x14000009 RID: 9
		// (add) Token: 0x06000088 RID: 136 RVA: 0x00003788 File Offset: 0x00001988
		// (remove) Token: 0x06000089 RID: 137 RVA: 0x000037C0 File Offset: 0x000019C0
		public event Action<IPlayerLevelingManager, int, int> PlayerEXPChanged;

		// Token: 0x1400000A RID: 10
		// (add) Token: 0x0600008A RID: 138 RVA: 0x000037F8 File Offset: 0x000019F8
		// (remove) Token: 0x0600008B RID: 139 RVA: 0x00003830 File Offset: 0x00001A30
		public event Action<IPlayerLevelingManager, int> NewPlayerLevelEXPReached;

		// Token: 0x1400000B RID: 11
		// (add) Token: 0x0600008C RID: 140 RVA: 0x00003868 File Offset: 0x00001A68
		// (remove) Token: 0x0600008D RID: 141 RVA: 0x000038A0 File Offset: 0x00001AA0
		public event Action<IPlayerLevelingManager, int, IReadOnlyList<IPlayerLevelReward>> NewPlayerLevelRewardsGenerated;

		// Token: 0x1400000C RID: 12
		// (add) Token: 0x0600008E RID: 142 RVA: 0x000038D8 File Offset: 0x00001AD8
		// (remove) Token: 0x0600008F RID: 143 RVA: 0x00003910 File Offset: 0x00001B10
		public event Action<IPlayerLevelingManager> PlayerLevelRewardsReset;

		// Token: 0x06000090 RID: 144 RVA: 0x00003948 File Offset: 0x00001B48
		private int GetMobIndex(BaseGameMob mob)
		{
			if (this.registeredMobsList.Count != 0)
			{
				for (int i = 0; i < this.registeredMobsList.Count; i++)
				{
					if (this.registeredMobsList[i].GetInstanceID() == mob.GetInstanceID())
					{
						return i;
					}
				}
			}
			return -1;
		}

		// Token: 0x06000091 RID: 145 RVA: 0x00003994 File Offset: 0x00001B94
		private void CountMob(BaseGameMob mob, bool isDestroyed)
		{
			if (mob.IsNull())
			{
				return;
			}
			int faction = (int)mob.Faction;
			int num;
			if (!this.mobsPerFactionCounter.TryGetValue(faction, out num))
			{
				num = -1;
			}
			if (isDestroyed)
			{
				if (this.IsEnemyMob(mob))
				{
					this.currentEnemyMobCount--;
				}
				if (--num <= 0)
				{
					this.mobsPerFactionCounter.Remove(faction);
					return;
				}
				this.mobsPerFactionCounter[faction] = num;
				return;
			}
			else
			{
				if (this.IsEnemyMob(mob))
				{
					this.totalEnemyMobCount++;
					this.currentEnemyMobCount++;
				}
				if (num < 0)
				{
					this.mobsPerFactionCounter.Add(faction, 1);
					return;
				}
				this.mobsPerFactionCounter[faction] = num + 1;
				return;
			}
		}

		// Token: 0x06000092 RID: 146 RVA: 0x00003A47 File Offset: 0x00001C47
		private void ReceiveReward()
		{
			if (this._currentGameRules != null)
			{
				this._currentGameRules.ReceiveReward(base.CurrentGame);
			}
		}

		// Token: 0x06000093 RID: 147 RVA: 0x00003A68 File Offset: 0x00001C68
		private SessionState GetActualGameSessionState()
		{
			if (this.CurrentSessionState != SessionState.Undefined)
			{
				if (this.IsPlayerDestroyed)
				{
					return SessionState.Defeat;
				}
				if (this._currentGameRules != null && this._currentGameRules.IsVictoryReached(this))
				{
					return SessionState.Victory;
				}
			}
			return SessionState.InProgress;
		}

		// Token: 0x06000094 RID: 148 RVA: 0x00003A9C File Offset: 0x00001C9C
		private void ResetPlayerDummy()
		{
			if (!this.isPlayerDummyInUse)
			{
				return;
			}
			this.RegisterPlayer(this.mainPlayer);
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00003AB4 File Offset: 0x00001CB4
		private void InitializePlayerLeveling(GameSessionRules rules, PlayerBehaviour player)
		{
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				this.playerLevelState = playerProfileManager.GetCurrentPlayerLevelRuntimeState();
				this.playerLevelState.RestoreRewards(player);
			}
			if (this.playerLevelState.currentLevel < 1)
			{
				this.playerLevelState.currentLevel = 1;
			}
			this.playerLevelRewardsGenerator = new PlayerLevelRewardsGenerator(rules.playerLevelingRewardPools);
			this.levelRewardCollectionContext = new PlayerLevelRewardCollectionContext
			{
				player = player
			};
			rules.TryGetNextPlayerLevelEXP(this.CurrentPlayerLevel, out this.nextPlayerLevelEXP);
			rules.TryGetNextPlayerLevelEXP(this.CurrentPlayerLevel - 1, out this.lastPlayerLevelEXP);
			if (this.lastPlayerLevelEXP < 0)
			{
				this.lastPlayerLevelEXP = 0;
			}
			this.desiredPlayerLevel = this.CurrentPlayerLevel;
			this.ResetPlayerLevelRewardsGenerationTime();
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00003B74 File Offset: 0x00001D74
		private bool IsPlayerLevelRewardsGenerationAvailable()
		{
			return !this.IsVictoryStateReached && !this.currentPlayer.IsKilled && !this.currentPlayer.Group.InBattle && this.currentPlayer.CurrentAttackers.Count + this.currentPlayer.CurrentThreateners.Count == 0 && !this.isCutsceneActive && (this.currentSessionState == SessionState.InProgress || this.currentSessionState == SessionState.Freezed);
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00003BEC File Offset: 0x00001DEC
		private void GeneratePlayerLevelRewards(bool isNextLevelRewards)
		{
			this.currentPlayerLevelRewards = this.playerLevelRewardsGenerator.GenerateRewards(this._currentGameRules.newPlayerLevelMaxRewardCount);
			if (isNextLevelRewards)
			{
				int arg = isNextLevelRewards ? (this.CurrentPlayerLevel + 1) : this.CurrentPlayerLevel;
				Action<IPlayerLevelingManager, int, IReadOnlyList<IPlayerLevelReward>> newPlayerLevelRewardsGenerated = this.NewPlayerLevelRewardsGenerated;
				if (newPlayerLevelRewardsGenerated == null)
				{
					return;
				}
				newPlayerLevelRewardsGenerated(this, arg, this.currentPlayerLevelRewards);
			}
		}

		// Token: 0x06000098 RID: 152 RVA: 0x00003C44 File Offset: 0x00001E44
		private bool PlayerHasNewLevels()
		{
			return this.CurrentPlayerLevel != this.desiredPlayerLevel;
		}

		// Token: 0x06000099 RID: 153 RVA: 0x00003C58 File Offset: 0x00001E58
		private void TrackNewPlayerLevels()
		{
			if (this.CurrentPlayerLevel >= this.MaxPlayerLevel || !((IPlayerLevelingManager)this).IsActive)
			{
				return;
			}
			if (this.nextPlayerLevelEXP > 0 && this.CurrentPlayerEXP >= this.nextPlayerLevelEXP)
			{
				while (this.CurrentPlayerEXP >= this.nextPlayerLevelEXP)
				{
					this.desiredPlayerLevel++;
					int num;
					if (!this._currentGameRules.TryGetNextPlayerLevelEXP(this.desiredPlayerLevel, out num))
					{
						break;
					}
					this.lastPlayerLevelEXP = this.nextPlayerLevelEXP;
					this.nextPlayerLevelEXP = num;
					Action<IPlayerLevelingManager, int> newPlayerLevelEXPReached = this.NewPlayerLevelEXPReached;
					if (newPlayerLevelEXPReached != null)
					{
						newPlayerLevelEXPReached(this, this.desiredPlayerLevel);
					}
				}
			}
			bool flag = this.PlayerHasNewLevels();
			if (!this.IsPlayerLevelRewardsGenerationAvailable())
			{
				this.ResetPlayerLevelRewardsGenerationTime();
			}
			if (this.currentPlayerLevelRewards == null && flag && Time.time > this.playerLevelRewardsGenerationTime)
			{
				this.GeneratePlayerLevelRewards(true);
			}
		}

		// Token: 0x0600009A RID: 154 RVA: 0x00003D27 File Offset: 0x00001F27
		private void ResetPlayerLevelState()
		{
			this.playerLevelState.Reset();
			this.desiredPlayerLevel = this.playerLevelState.currentLevel;
		}

		// Token: 0x0600009B RID: 155 RVA: 0x00003D45 File Offset: 0x00001F45
		public void InterruptSession()
		{
			if (this.currentPlayer.IsNull())
			{
				return;
			}
			this.currentPlayer.KillMob(this.currentPlayer);
			GameManager.SetGameFreezed(false);
		}

		// Token: 0x0600009C RID: 156 RVA: 0x00003D6C File Offset: 0x00001F6C
		public bool SetSessionState(SessionState newSessionState)
		{
			if (this.blockVictoryState && newSessionState == SessionState.Victory)
			{
				return false;
			}
			if (this.currentSessionState == newSessionState)
			{
				return false;
			}
			bool gameFreezed = false;
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			bool flag4 = false;
			bool flag5 = false;
			bool flag6 = false;
			string text = null;
			switch (newSessionState)
			{
			case SessionState.Paused:
			case SessionState.Freezed:
				gameFreezed = true;
				flag2 = true;
				break;
			case SessionState.Interrupted:
				flag6 = true;
				flag4 = true;
				flag5 = true;
				text = this.gameManager.HomespaceSceneName;
				break;
			case SessionState.Victory:
				this.isVictoryStateReached = true;
				flag3 = true;
				flag2 = true;
				this.ReceiveReward();
				break;
			case SessionState.Defeat:
				gameFreezed = false;
				flag6 = true;
				flag3 = true;
				flag2 = true;
				flag4 = true;
				flag5 = true;
				this.ReceiveReward();
				break;
			case SessionState.VictoryCutscene:
				this.isVictoryStateReached = true;
				this.SetPlayerTransitionAreasActive(true);
				break;
			case SessionState.Exited:
			{
				flag5 = true;
				bool flag7;
				if (this.gameManager != null && this.gameManager.IsNormalGameSceneLoaded(out flag7, true))
				{
					if (flag7)
					{
						this.gameManager.DeleteCurrentPlayerProfile();
					}
					else if (!this.isVictoryStateReached)
					{
						this.ResetPlayerDummy();
						this.gameManager.KeepLastPlayerLocalProgressAndSaveProfile();
					}
					flag6 = false;
				}
				text = this.gameManager.MainMenuSceneName;
				break;
			}
			}
			if (flag3)
			{
				this.ResetPlayerDummy();
			}
			if (flag6)
			{
				GameManager gameManager = this.gameManager;
				if (gameManager != null)
				{
					gameManager.SavePlayerProfile(!flag4);
				}
			}
			GameManager.SetGameFreezed(gameFreezed);
			if (this.currentPlayer != null)
			{
				this.currentPlayer.PlayerInputController.IsActive = !flag2;
			}
			if (flag)
			{
				this.TryRestartCurrentGame(false);
			}
			else
			{
				this.CancelGameRestarting();
			}
			this.currentSessionState = newSessionState;
			Action<IGameSessionManager, SessionState> sessionStateChanged = this.SessionStateChanged;
			if (sessionStateChanged != null)
			{
				sessionStateChanged(this, newSessionState);
			}
			if (flag5)
			{
				this.ResetPlayerLevelState();
				this.ResetPlayerLevelRewards();
			}
			if (text != null)
			{
				this.gameManager.LoadScene(text, null);
			}
			return true;
		}

		// Token: 0x0600009D RID: 157 RVA: 0x00003F2C File Offset: 0x0000212C
		private bool SetPlayerTransitionAreasActive(bool isActive)
		{
			if (isActive && this.playerTransitionAreas.Count == 0)
			{
				return false;
			}
			bool result = false;
			this.isWaitingForPlayerTransition = false;
			foreach (PlayerAreaTrigger playerAreaTrigger in this.playerTransitionAreas)
			{
				if (!playerAreaTrigger.IsNull())
				{
					this.isWaitingForPlayerTransition |= (isActive && playerAreaTrigger.isSceneTransitionArea);
					playerAreaTrigger.SetActive(isActive);
					result = true;
				}
			}
			return result;
		}

		// Token: 0x0600009E RID: 158 RVA: 0x00003FC0 File Offset: 0x000021C0
		public void UpdateSessionState(bool force = false)
		{
			if (this._currentGameRules == null || (!force && this.currentSessionState != SessionState.InProgress))
			{
				return;
			}
			this.SetSessionState(this.GetActualGameSessionState());
		}

		// Token: 0x0600009F RID: 159 RVA: 0x00003FE9 File Offset: 0x000021E9
		private IEnumerator GameRestartingRoutine()
		{
			yield return new WaitForSecondsRealtime(0.3f);
			this.isWaitingForSceneRestart = false;
			this.TryRestartCurrentGame(true);
			yield break;
		}

		// Token: 0x060000A0 RID: 160 RVA: 0x00003FF8 File Offset: 0x000021F8
		public void RegisterPlayer(PlayerBehaviour player)
		{
			if (player.IsNull() || this.currentPlayer == player || this.IsGameSessionFinalized)
			{
				return;
			}
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.RevivedMob -= this.OnMobRevivedByPlayer;
				this.currentPlayer.HitPointsController.HitPointsChanged -= this.OnPlayerHitPointsChanged;
				this.currentPlayer.Killed -= this.OnPlayerKilled;
			}
			else if (this.mainPlayer == null)
			{
				this.mainPlayer = player;
			}
			if (this.isPlayerDummyInUse = (player != this.mainPlayer))
			{
				IPickableObjectCollector pickableObjectCollector;
				if (player.TryGetComponent<IPickableObjectCollector>(out pickableObjectCollector))
				{
					UnityEngine.Object.Destroy((Component)pickableObjectCollector);
				}
			}
			else
			{
				player.gameObject.SetActive(true);
			}
			this.currentPlayer = player;
			this.isPlayerDestroyed = false;
			bool flag;
			if (!this.isPlayerDummyInUse && this.gameManager != null && this.gameManager.IsNormalGameSceneLoaded(out flag, true))
			{
				this.gameManager.SavePlayerProfile(!flag);
			}
			this.InitializePlayerLeveling(this._currentGameRules, player);
			this.currentPlayer.RevivedMob += this.OnMobRevivedByPlayer;
			this.currentPlayer.HitPointsController.HitPointsChanged += this.OnPlayerHitPointsChanged;
			this.currentPlayer.Killed += this.OnPlayerKilled;
			Action<PlayerBehaviour> playerRegistered = this.PlayerRegistered;
			if (playerRegistered == null)
			{
				return;
			}
			playerRegistered(this.currentPlayer);
		}

		// Token: 0x060000A1 RID: 161 RVA: 0x0000417C File Offset: 0x0000237C
		public void AddPlayerTransitionArea(PlayerAreaTrigger transitionArea)
		{
			if (!transitionArea.IsNull())
			{
				transitionArea.nextScene = new SceneAssetReference(this.NextSceneName);
				transitionArea.SetActive(this.currentSessionState == SessionState.Victory);
				this.playerTransitionAreas.Add(transitionArea);
			}
		}

		// Token: 0x060000A2 RID: 162 RVA: 0x000041B4 File Offset: 0x000023B4
		public void RegisterMob(BaseGameMob mob)
		{
			if (mob.IsNull() || this.IsGameSessionFinalized)
			{
				return;
			}
			this.registeredMobsList.Add(mob);
			this.CountMob(mob, false);
			mob.Killed += this.OnMobKilled;
			Action<BaseGameMob> mobRegistered = this.MobRegistered;
			if (mobRegistered == null)
			{
				return;
			}
			mobRegistered(mob);
		}

		// Token: 0x060000A3 RID: 163 RVA: 0x0000420C File Offset: 0x0000240C
		public void UnregisterMob(BaseGameMob mob)
		{
			if (mob.IsNull())
			{
				return;
			}
			int mobIndex = this.GetMobIndex(mob);
			if (mobIndex != -1)
			{
				int index = this.registeredMobsList.Count - 1;
				this.registeredMobsList[mobIndex] = this.registeredMobsList[index];
				this.registeredMobsList.RemoveAt(index);
				this.CountMob(mob, true);
				mob.Killed -= this.OnMobKilled;
				Action<BaseGameMob> mobUnregistered = this.MobUnregistered;
				if (mobUnregistered == null)
				{
					return;
				}
				mobUnregistered(mob);
			}
		}

		// Token: 0x060000A4 RID: 164 RVA: 0x0000428B File Offset: 0x0000248B
		public bool IsEnemyMob(BaseGameMob mob)
		{
			return this._currentGameRules != null && this._currentGameRules.IsPlayerEnemyMob(mob);
		}

		// Token: 0x060000A5 RID: 165 RVA: 0x000042AC File Offset: 0x000024AC
		public int GetMobsCount(GameMobFactions factionID)
		{
			int result;
			this.mobsPerFactionCounter.TryGetValue((int)factionID, out result);
			return result;
		}

		// Token: 0x060000A6 RID: 166 RVA: 0x000042C9 File Offset: 0x000024C9
		public void CancelGameRestarting()
		{
			if (this.currentGameRestartingCoroutine != null)
			{
				base.StopCoroutine(this.currentGameRestartingCoroutine);
				this.currentGameRestartingCoroutine = null;
				this.isWaitingForSceneRestart = false;
			}
		}

		// Token: 0x060000A7 RID: 167 RVA: 0x000042ED File Offset: 0x000024ED
		public void TryRestartCurrentGame(bool force = false)
		{
			this.CancelGameRestarting();
			if (force)
			{
				this.LoadScene(GameManager.CurrentSceneName);
				return;
			}
			this.isWaitingForSceneRestart = true;
			this.currentGameRestartingCoroutine = base.StartCoroutine(this.GameRestartingRoutine());
		}

		// Token: 0x060000A8 RID: 168 RVA: 0x0000431D File Offset: 0x0000251D
		public void LoadScene(string newSceneName)
		{
			if (this.PlayerHasNewLevels())
			{
				this.gameManager.PostponeSceneLoading(0f);
			}
			this.gameManager.LoadScene(newSceneName, null);
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00004344 File Offset: 0x00002544
		public void LoadNextScene()
		{
			this.gameManager.UpdateLocalPlayerProgress();
			this.LoadScene(this.NextSceneName);
		}

		// Token: 0x060000AA RID: 170 RVA: 0x00004360 File Offset: 0x00002560
		public void SetCutsceneActive(bool isActive, object context)
		{
			if (this.isCutsceneActive == isActive)
			{
				return;
			}
			this.isCutsceneActive = isActive;
			this.currentCutsceneContext = (isActive ? context : null);
			if (this.currentSessionState != SessionState.VictoryCutscene)
			{
				SessionState sessionState = isActive ? SessionState.Cutscene : SessionState.InProgress;
				this.SetSessionState(sessionState);
			}
			for (int i = 0; i < this.registeredMobsList.Count; i++)
			{
				BaseGameMob baseGameMob = this.registeredMobsList[i];
				if (baseGameMob != null)
				{
					baseGameMob.SetCutsceneActive(isActive, context);
				}
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000043D8 File Offset: 0x000025D8
		public bool AddPlayerLevelEXP(int amount, out int addedAmount)
		{
			if (amount > 0 && this.nextPlayerLevelEXP > 0)
			{
				int maxPlayerLevel = this.MaxPlayerLevel;
				int currentPlayerLevel = this.CurrentPlayerLevel;
				if (currentPlayerLevel < maxPlayerLevel)
				{
					addedAmount = amount;
					if (currentPlayerLevel + 1 == maxPlayerLevel)
					{
						int num = this.nextPlayerLevelEXP - this.CurrentPlayerEXP;
						if (amount > num)
						{
							amount = num;
							addedAmount = amount;
						}
					}
					this.CurrentPlayerEXP += amount;
					return true;
				}
			}
			addedAmount = 0;
			return false;
		}

		// Token: 0x060000AC RID: 172 RVA: 0x0000443C File Offset: 0x0000263C
		public bool TakeNewPlayerLevelReward(IPlayerLevelReward playerLevelReward)
		{
			if (this.currentPlayerLevelRewards != null)
			{
				for (int i = 0; i < this.currentPlayerLevelRewards.Count; i++)
				{
					if (this.currentPlayerLevelRewards[i] == playerLevelReward)
					{
						playerLevelReward.Take(this.levelRewardCollectionContext);
						this.playerLevelState.StoreReward(playerLevelReward);
						this.ResetPlayerLevelRewards();
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060000AD RID: 173 RVA: 0x00004497 File Offset: 0x00002697
		public bool RerollCurrentRewards(out IReadOnlyList<IPlayerLevelReward> updatedRewards)
		{
			if (this.currentPlayerLevelRewards != null)
			{
				this.GeneratePlayerLevelRewards(false);
				updatedRewards = this.currentPlayerLevelRewards;
				return updatedRewards.Count != 0;
			}
			updatedRewards = Array.Empty<IPlayerLevelReward>();
			return false;
		}

		// Token: 0x060000AE RID: 174 RVA: 0x000044C4 File Offset: 0x000026C4
		public void ResetPlayerLevelRewards()
		{
			if (this.desiredPlayerLevel != this.CurrentPlayerLevel)
			{
				int currentPlayerLevel = this.CurrentPlayerLevel;
				this.CurrentPlayerLevel = currentPlayerLevel + 1;
			}
			this.currentPlayerLevelRewards = null;
			Action<IPlayerLevelingManager> playerLevelRewardsReset = this.PlayerLevelRewardsReset;
			if (playerLevelRewardsReset == null)
			{
				return;
			}
			playerLevelRewardsReset(this);
		}

		// Token: 0x060000AF RID: 175 RVA: 0x00004507 File Offset: 0x00002707
		public void ResetPlayerLevelRewardsGenerationTime()
		{
			this.playerLevelRewardsGenerationTime = Time.time + this._currentGameRules.playerLevelRewardsGenerationDelay;
		}

		// Token: 0x060000B0 RID: 176 RVA: 0x00004520 File Offset: 0x00002720
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<GameManager>(out this.gameManager);
			if (this._currentGameRules == null)
			{
				currentGame.BindDataDirectly(ref this._currentGameRules);
			}
			this.totalEnemyMobCount = 0;
			this.currentEnemyMobCount = 0;
			this.lastDestroyedMobID = MobBehaviour.ID.None;
			this.currentSessionState = SessionState.Undefined;
			this.isWaitingForSceneRestart = false;
			GameSessionRules currentGameRules = this._currentGameRules;
			if (currentGameRules == null)
			{
				return;
			}
			currentGameRules.Initialize(this);
		}

		// Token: 0x060000B1 RID: 177 RVA: 0x00004598 File Offset: 0x00002798
		private void OnMobRevivedByPlayer(BaseGameMob deadMob, IRevivableGameMob revivedMob)
		{
			IPlayerLevelingEXPSource playerLevelingEXPSource = revivedMob as IPlayerLevelingEXPSource;
			if (playerLevelingEXPSource != null)
			{
				int num;
				this.AddPlayerLevelEXP(playerLevelingEXPSource.EXPAmount, out num);
			}
		}

		// Token: 0x060000B2 RID: 178 RVA: 0x000045BE File Offset: 0x000027BE
		private void OnPlayerHitPointsChanged(IHitPointsSource hpController, object sender, IHitPointsChangingArgs args)
		{
			if (args.IsDamage)
			{
				this.ResetPlayerLevelRewardsGenerationTime();
			}
		}

		// Token: 0x060000B3 RID: 179 RVA: 0x000045D0 File Offset: 0x000027D0
		private void OnPlayerKilled(IGameMob killedPlayer)
		{
			if (this.isPlayerDummyInUse && this.mainPlayer != null && !this.mainPlayer.IsKilled)
			{
				this.ResetPlayerDummy();
				return;
			}
			this.RegisterPlayer(null);
			this.isPlayerDestroyed = true;
			this.UpdateSessionState(true);
		}

		// Token: 0x060000B4 RID: 180 RVA: 0x0000461C File Offset: 0x0000281C
		private void OnMobKilled(IGameMob killedMob)
		{
			BaseGameMob baseGameMob = killedMob as BaseGameMob;
			if (baseGameMob == null)
			{
				return;
			}
			if (this.IsEnemyMob(baseGameMob))
			{
				this.destroyedEnemyMobCount++;
			}
			MobBehaviour mobBehaviour = killedMob as MobBehaviour;
			MobBehaviour.ID id = (mobBehaviour != null) ? mobBehaviour.ObjectID : MobBehaviour.ID.None;
			if (id != MobBehaviour.ID.None)
			{
				this.lastDestroyedMobID = id;
			}
			this.UnregisterMob(baseGameMob);
			this.UpdateSessionState(false);
		}

		// Token: 0x060000B5 RID: 181 RVA: 0x00004677 File Offset: 0x00002877
		private void Start()
		{
			if (this.currentSessionState == SessionState.Undefined)
			{
				this.UpdateSessionState(true);
			}
		}

		// Token: 0x060000B6 RID: 182 RVA: 0x00004689 File Offset: 0x00002889
		private void Update()
		{
			this.TrackNewPlayerLevels();
		}

		// Token: 0x060000B7 RID: 183 RVA: 0x00004691 File Offset: 0x00002891
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.Killed -= this.OnPlayerKilled;
			}
		}

		// Token: 0x04000036 RID: 54
		[SerializeField]
		[Tooltip("Правила игры. Если не заданы, то будут использованы правила по умолчанию.")]
		private GameSessionRules _currentGameRules;

		// Token: 0x04000037 RID: 55
		[SerializeField]
		private GameLocation.TypeID nextLocation;

		// Token: 0x04000038 RID: 56
		[SerializeField]
		[Obsolete]
		private SceneAssetReference _nextScene;

		// Token: 0x04000039 RID: 57
		public bool blockVictoryState;

		// Token: 0x0400003A RID: 58
		private readonly List<BaseGameMob> registeredMobsList = new List<BaseGameMob>(50);

		// Token: 0x0400003B RID: 59
		private readonly Dictionary<int, int> mobsPerFactionCounter = new Dictionary<int, int>();

		// Token: 0x0400003C RID: 60
		private readonly List<PlayerAreaTrigger> playerTransitionAreas = new List<PlayerAreaTrigger>();

		// Token: 0x0400003D RID: 61
		private GameManager gameManager;

		// Token: 0x0400003E RID: 62
		private PlayerBehaviour mainPlayer;

		// Token: 0x0400003F RID: 63
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000040 RID: 64
		private bool isPlayerDummyInUse;

		// Token: 0x04000041 RID: 65
		private bool isPlayerDestroyed;

		// Token: 0x04000042 RID: 66
		private IReadOnlyList<BaseGameMob> registeredMobs;

		// Token: 0x04000043 RID: 67
		private int currentEnemyMobCount;

		// Token: 0x04000044 RID: 68
		private int totalEnemyMobCount;

		// Token: 0x04000045 RID: 69
		private int destroyedEnemyMobCount;

		// Token: 0x04000046 RID: 70
		private SessionState currentSessionState = SessionState.Undefined;

		// Token: 0x04000047 RID: 71
		private MobBehaviour.ID lastDestroyedMobID;

		// Token: 0x04000048 RID: 72
		private PlayerLevelRuntimeState playerLevelState;

		// Token: 0x04000049 RID: 73
		private PlayerLevelRewardCollectionContext levelRewardCollectionContext;

		// Token: 0x0400004A RID: 74
		private int lastPlayerLevelEXP;

		// Token: 0x0400004B RID: 75
		private int nextPlayerLevelEXP;

		// Token: 0x0400004C RID: 76
		private int desiredPlayerLevel;

		// Token: 0x0400004D RID: 77
		private PlayerLevelRewardsGenerator playerLevelRewardsGenerator;

		// Token: 0x0400004E RID: 78
		private float playerLevelRewardsGenerationTime = -1f;

		// Token: 0x0400004F RID: 79
		private IReadOnlyList<IPlayerLevelReward> currentPlayerLevelRewards;

		// Token: 0x04000050 RID: 80
		private Coroutine currentGameRestartingCoroutine;

		// Token: 0x04000051 RID: 81
		private bool isWaitingForPlayerTransition;

		// Token: 0x04000052 RID: 82
		private bool isWaitingForSceneRestart;

		// Token: 0x04000053 RID: 83
		private bool isCutsceneActive;

		// Token: 0x04000054 RID: 84
		private object currentCutsceneContext;

		// Token: 0x04000055 RID: 85
		private bool isVictoryStateReached;
	}
}
