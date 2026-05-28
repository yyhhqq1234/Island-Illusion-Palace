using System;
using System.Collections.Generic;
using Common;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving
{
	// Token: 0x02000010 RID: 16
	public interface IGameSessionManager : IPlayerProvider, IDestroyable
	{
		// Token: 0x17000035 RID: 53
		// (get) Token: 0x060000C4 RID: 196
		string NextSceneName { get; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x060000C5 RID: 197
		SessionState CurrentSessionState { get; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x060000C6 RID: 198
		bool IsGameSessionInProgress { get; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x060000C7 RID: 199
		bool IsGameSessionFinalized { get; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x060000C8 RID: 200
		bool IsWaitingForPlayerTransition { get; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x060000C9 RID: 201
		IReadOnlyList<BaseGameMob> RegisteredMobs { get; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x060000CA RID: 202
		object CurrentCutsceneContext { get; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x060000CB RID: 203
		bool IsCutsceneActive { get; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x060000CC RID: 204
		bool IsVictoryStateReached { get; }

		// Token: 0x1400000D RID: 13
		// (add) Token: 0x060000CD RID: 205
		// (remove) Token: 0x060000CE RID: 206
		event Action<BaseGameMob> MobRegistered;

		// Token: 0x1400000E RID: 14
		// (add) Token: 0x060000CF RID: 207
		// (remove) Token: 0x060000D0 RID: 208
		event Action<BaseGameMob> MobUnregistered;

		// Token: 0x1400000F RID: 15
		// (add) Token: 0x060000D1 RID: 209
		// (remove) Token: 0x060000D2 RID: 210
		event Action<IGameSessionManager, SessionState> SessionStateChanged;

		// Token: 0x060000D3 RID: 211
		bool SetSessionState(SessionState state);

		// Token: 0x060000D4 RID: 212
		void TryRestartCurrentGame(bool force = false);

		// Token: 0x060000D5 RID: 213
		void RegisterMob(BaseGameMob mob);

		// Token: 0x060000D6 RID: 214
		void UnregisterMob(BaseGameMob mob);

		// Token: 0x060000D7 RID: 215
		bool IsEnemyMob(BaseGameMob mob);

		// Token: 0x060000D8 RID: 216
		void SetCutsceneActive(bool isActive, object context);
	}
}
