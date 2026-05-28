using System;
using Game.Abilities;
using Game.Core;
using Unliving.LevelGeneration;
using Unliving.Player;
using Unliving.Statistics;

namespace Unliving.Tutorial
{
	// Token: 0x02000032 RID: 50
	[Serializable]
	public class ZeroSacrificedMobsOnChunkHint : TutorialHintBase
	{
		// Token: 0x060001AB RID: 427 RVA: 0x00006DD0 File Offset: 0x00004FD0
		public override void OnSceneLoaded(IGame game)
		{
			base.OnSceneLoaded(game);
			if (this.isCompleted)
			{
				return;
			}
			this.Deinitialize();
			if (this.statisticsManager == null && game.Services.TryGet<PlayerStatisticsManager>(out this.statisticsManager))
			{
				this.ResetVisitedChunksCount();
			}
			if (game.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
			}
		}

		// Token: 0x060001AC RID: 428 RVA: 0x00006E55 File Offset: 0x00005055
		private void Deinitialize()
		{
			if (this.playerProvider != null)
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			this.OnPlayerRegistered(null);
		}

		// Token: 0x060001AD RID: 429 RVA: 0x00006E80 File Offset: 0x00005080
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (this.currentPlayer != null)
			{
				this.currentPlayer.AbilitiesController.AbilityActivated -= this.OnAbilityActivated;
			}
			this.currentPlayer = player;
			if (this.currentPlayer != null)
			{
				this.currentPlayer.AbilitiesController.AbilityActivated += this.OnAbilityActivated;
			}
		}

		// Token: 0x060001AE RID: 430 RVA: 0x00006EE8 File Offset: 0x000050E8
		private void OnAbilityActivated(IAbility ability, object abilityArgs)
		{
			if (ability.ID == 1037)
			{
				this.ResetVisitedChunksCount();
			}
		}

		// Token: 0x060001AF RID: 431 RVA: 0x00006EFD File Offset: 0x000050FD
		private void ResetVisitedChunksCount()
		{
			this.lastVisitedChunksCount = this.statisticsManager.GameSessionStatsData.GetZeroSacrificedMobsChunksCount(this.chunkTypes);
		}

		// Token: 0x060001B0 RID: 432 RVA: 0x00006F20 File Offset: 0x00005120
		protected override bool IsConditionReached()
		{
			return this.statisticsManager.GameSessionStatsData.GetZeroSacrificedMobsChunksCount(this.chunkTypes) - this.lastVisitedChunksCount >= this.targetChunksCount;
		}

		// Token: 0x060001B1 RID: 433 RVA: 0x00006F4F File Offset: 0x0000514F
		protected override void OnHintConditionReached()
		{
			this.ResetVisitedChunksCount();
		}

		// Token: 0x060001B2 RID: 434 RVA: 0x00006F57 File Offset: 0x00005157
		protected override void OnHintCompleted()
		{
			this.Deinitialize();
		}

		// Token: 0x040000DE RID: 222
		private const int SacrificeAbilityID = 1037;

		// Token: 0x040000DF RID: 223
		public LocationChunk.TypeID[] chunkTypes;

		// Token: 0x040000E0 RID: 224
		public int targetChunksCount;

		// Token: 0x040000E1 RID: 225
		private PlayerBehaviour currentPlayer;

		// Token: 0x040000E2 RID: 226
		private IPlayerProvider playerProvider;

		// Token: 0x040000E3 RID: 227
		private PlayerStatisticsManager statisticsManager;

		// Token: 0x040000E4 RID: 228
		private int lastVisitedChunksCount;
	}
}
