using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Editor;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.GameSession.PlayerLeveling;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving
{
	// Token: 0x0200000F RID: 15
	[CreateAssetMenu(fileName = "GameSessionRules", menuName = "Game/Data/Game Session Rules")]
	public sealed class GameSessionRules : ScriptableObject, IInitializable<GameSessionManager>
	{
		// Token: 0x17000033 RID: 51
		// (get) Token: 0x060000B9 RID: 185 RVA: 0x000046FA File Offset: 0x000028FA
		public IGameSessionRewardRules RewardRules
		{
			get
			{
				return this.rewardRules;
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x060000BA RID: 186 RVA: 0x00004702 File Offset: 0x00002902
		public int MaxPlayerLevel
		{
			get
			{
				return this.playerLevelsInfo.Length + 1;
			}
		}

		// Token: 0x060000BB RID: 187 RVA: 0x00004710 File Offset: 0x00002910
		internal bool IsVictoryReached(GameSessionManager sessionManager)
		{
			if (this.victoryConditions.Count == 0)
			{
				return false;
			}
			foreach (Predicate<GameSessionManager> predicate in this.victoryConditions)
			{
				if (predicate != null && !predicate(sessionManager))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x060000BC RID: 188 RVA: 0x00004780 File Offset: 0x00002980
		internal bool TryGetNextPlayerLevelEXP(int currentPlayerLevel, out int nextLevelEXP)
		{
			if (currentPlayerLevel > 0 && currentPlayerLevel < this.MaxPlayerLevel)
			{
				nextLevelEXP = this.playerLevelsInfo[Mathf.Max(currentPlayerLevel, 1) - 1].requiredEXP;
				return true;
			}
			nextLevelEXP = -1;
			return false;
		}

		// Token: 0x060000BD RID: 189 RVA: 0x000047B0 File Offset: 0x000029B0
		public void ReceiveReward(IGame game)
		{
			if (this.rewardRules != null)
			{
				this.rewardRules.ReceiveReward(game);
			}
		}

		// Token: 0x060000BE RID: 190 RVA: 0x000047C6 File Offset: 0x000029C6
		public bool IsPlayerEnemyFaction(GameMobFactions faction)
		{
			return this.playerEnemyFactionsSet.Contains((int)faction);
		}

		// Token: 0x060000BF RID: 191 RVA: 0x000047D4 File Offset: 0x000029D4
		public bool IsPlayerEnemyMob(BaseGameMob mob)
		{
			return !mob.IsNull() && this.IsPlayerEnemyFaction(mob.Faction);
		}

		// Token: 0x060000C0 RID: 192 RVA: 0x000047EC File Offset: 0x000029EC
		public void AddVictoryCondition(Predicate<GameSessionManager> victoryCondition)
		{
			if (victoryCondition != null)
			{
				this.victoryConditions.Add(victoryCondition);
			}
		}

		// Token: 0x060000C1 RID: 193 RVA: 0x000047FD File Offset: 0x000029FD
		public void RemoveVictoryCondition(Predicate<GameSessionManager> victoryCondition)
		{
			if (victoryCondition != null)
			{
				this.victoryConditions.Remove(victoryCondition);
			}
		}

		// Token: 0x060000C2 RID: 194 RVA: 0x00004810 File Offset: 0x00002A10
		public void Initialize(GameSessionManager sessionManager)
		{
			GameMobsFactory gameMobsFactory = sessionManager.CurrentGame.Services.Get<GameMobsFactory>();
			if (gameMobsFactory != null)
			{
				this.playerEnemyFactionsSet = new HashSet<int>(from factionInfo in gameMobsFactory.GetEnemyFactionsInfo(PlayerFactory.PlayerLayer)
				select (int)factionInfo.faction);
			}
			else
			{
				this.playerEnemyFactionsSet = new HashSet<int>(from factionID in this.defaultPlayerEnemyFactions
				select (int)factionID);
			}
			int count = this.playerEnemyFactionsSet.Count;
		}

		// Token: 0x04000056 RID: 86
		[Tooltip("Враждебные относительно игрока фракции мобов. Будут использованы, если не удалось получить данные фракций из фабрики мобов.")]
		public GameMobFactions[] defaultPlayerEnemyFactions = new GameMobFactions[]
		{
			GameMobFactions.ENEMY,
			GameMobFactions.PROTOSS,
			GameMobFactions.TERRANS,
			GameMobFactions.ZERG
		};

		// Token: 0x04000057 RID: 87
		[SerializeReference]
		[ManagedObjectField(typeof(IGameSessionRewardRules))]
		private IGameSessionRewardRules rewardRules;

		// Token: 0x04000058 RID: 88
		[SerializeField]
		private PlayerLevelInfo[] playerLevelsInfo;

		// Token: 0x04000059 RID: 89
		public int newPlayerLevelMaxRewardCount = 3;

		// Token: 0x0400005A RID: 90
		public float playerLevelRewardsGenerationDelay = 3f;

		// Token: 0x0400005B RID: 91
		public PlayerLevelRewardsPoolAssetBase[] playerLevelingRewardPools;

		// Token: 0x0400005C RID: 92
		private readonly List<Predicate<GameSessionManager>> victoryConditions = new List<Predicate<GameSessionManager>>();

		// Token: 0x0400005D RID: 93
		private HashSet<int> playerEnemyFactionsSet;
	}
}
