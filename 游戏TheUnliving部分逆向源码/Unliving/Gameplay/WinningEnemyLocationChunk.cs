using System;
using Game.LevelGeneration;

namespace Unliving.Gameplay
{
	// Token: 0x020002AC RID: 684
	public sealed class WinningEnemyLocationChunk : EnemyLocationChunkClearingTrigger
	{
		// Token: 0x060017F8 RID: 6136 RVA: 0x0004BC2D File Offset: 0x00049E2D
		private bool VictoryCondition(GameSessionManager gameSessionManager)
		{
			return base.IsChunkCleared;
		}

		// Token: 0x060017F9 RID: 6137 RVA: 0x0004BC35 File Offset: 0x00049E35
		protected override void OnFullyInitialized()
		{
			GameSessionManager gameSessionManager = base.GameSessionManager;
			this.gameSessionRules = ((gameSessionManager != null) ? gameSessionManager.CurrentGameRules : null);
			if (this.gameSessionRules != null)
			{
				this.gameSessionRules.AddVictoryCondition(new Predicate<GameSessionManager>(this.VictoryCondition));
			}
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x0004BC74 File Offset: 0x00049E74
		protected override void OnEnemyMobExitedChunk(ILocationChunkVisitor mobVisitor, int remainingEnemyCount)
		{
			GameSessionManager gameSessionManager = base.GameSessionManager;
			if (gameSessionManager == null)
			{
				return;
			}
			gameSessionManager.UpdateSessionState(false);
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x0004BC87 File Offset: 0x00049E87
		protected override void OnDestroy()
		{
			GameSessionRules gameSessionRules = this.gameSessionRules;
			if (gameSessionRules != null)
			{
				gameSessionRules.RemoveVictoryCondition(new Predicate<GameSessionManager>(this.VictoryCondition));
			}
			base.OnDestroy();
		}

		// Token: 0x04000DA0 RID: 3488
		private GameSessionRules gameSessionRules;
	}
}
