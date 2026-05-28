using System;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Gameplay
{
	// Token: 0x020002A9 RID: 681
	public abstract class EnemyLocationChunkClearingTrigger : GameBehaviourBase
	{
		// Token: 0x17000517 RID: 1303
		// (get) Token: 0x060017D8 RID: 6104 RVA: 0x0004B8C5 File Offset: 0x00049AC5
		// (set) Token: 0x060017D9 RID: 6105 RVA: 0x0004B8CD File Offset: 0x00049ACD
		public LocationChunk TargetLocationChunk
		{
			get
			{
				return this._targetLocationChunk;
			}
			set
			{
				this._targetLocationChunk = value;
			}
		}

		// Token: 0x17000518 RID: 1304
		// (get) Token: 0x060017DA RID: 6106 RVA: 0x0004B8D6 File Offset: 0x00049AD6
		public int CurrentEnemyMobsCount
		{
			get
			{
				return this._currentEnemyMobsCount;
			}
		}

		// Token: 0x17000519 RID: 1305
		// (get) Token: 0x060017DB RID: 6107 RVA: 0x0004B8DE File Offset: 0x00049ADE
		public bool IsChunkCleared
		{
			get
			{
				return this._isChunkCleared;
			}
		}

		// Token: 0x1700051A RID: 1306
		// (get) Token: 0x060017DC RID: 6108 RVA: 0x0004B8E6 File Offset: 0x00049AE6
		protected GameSessionManager GameSessionManager
		{
			get
			{
				return this._gameSessionManager;
			}
		}

		// Token: 0x1700051B RID: 1307
		// (get) Token: 0x060017DD RID: 6109 RVA: 0x0004B8EE File Offset: 0x00049AEE
		protected bool IsPlayerInsideChunk
		{
			get
			{
				return this._isPlayerInsideChunk;
			}
		}

		// Token: 0x140000E6 RID: 230
		// (add) Token: 0x060017DE RID: 6110 RVA: 0x0004B8F8 File Offset: 0x00049AF8
		// (remove) Token: 0x060017DF RID: 6111 RVA: 0x0004B930 File Offset: 0x00049B30
		public event Action<ILocationChunk, bool> ClearingStateChanged;

		// Token: 0x060017E0 RID: 6112 RVA: 0x0004B968 File Offset: 0x00049B68
		private GameSessionManager GetGameSessionManager()
		{
			GameSessionManager result;
			if ((result = this._gameSessionManager) == null)
			{
				IGame currentGame = base.CurrentGame;
				result = (this._gameSessionManager = ((currentGame != null) ? currentGame.Services.Get<GameSessionManager>() : null));
			}
			return result;
		}

		// Token: 0x060017E1 RID: 6113 RVA: 0x0004B9A0 File Offset: 0x00049BA0
		private void TryToInititalize()
		{
			if (this.isInitialized)
			{
				return;
			}
			this.GetGameSessionManager();
			if (this._gameSessionManager == null)
			{
				return;
			}
			if (this._targetLocationChunk != null || base.TryGetComponent<LocationChunk>(out this._targetLocationChunk))
			{
				this._targetLocationChunk.VisitorAdded += this.OnVisitorEnteredLocationChunk;
				this._targetLocationChunk.VisitorRemoved += this.OnVisitorExitedLocationChunk;
			}
			this.OnFullyInitialized();
			this.isInitialized = true;
		}

		// Token: 0x060017E2 RID: 6114 RVA: 0x0004BA24 File Offset: 0x00049C24
		private bool CountEnemyMob(ILocationChunkVisitor chunkVisitor)
		{
			if (!(this._gameSessionManager == null))
			{
				BaseGameMob baseGameMob = chunkVisitor as BaseGameMob;
				if (baseGameMob != null && !baseGameMob.isEnvironmentMob)
				{
					return this._gameSessionManager.IsEnemyMob(baseGameMob);
				}
			}
			return false;
		}

		// Token: 0x060017E3 RID: 6115 RVA: 0x0004BA5F File Offset: 0x00049C5F
		protected void UpdateClearingState(bool newState)
		{
			if (this._isChunkCleared == newState)
			{
				return;
			}
			this._isChunkCleared = newState;
			this.OnClearingStateChanged(newState);
			Action<ILocationChunk, bool> clearingStateChanged = this.ClearingStateChanged;
			if (clearingStateChanged == null)
			{
				return;
			}
			clearingStateChanged(this._targetLocationChunk, newState);
		}

		// Token: 0x060017E4 RID: 6116 RVA: 0x0004BA90 File Offset: 0x00049C90
		public int ForceGetEnemyMobsCount(ILocationChunk arbitraryChunk)
		{
			return arbitraryChunk.GetEnemyMobsCount(this.GetGameSessionManager(), true, false);
		}

		// Token: 0x060017E5 RID: 6117 RVA: 0x0004BAA0 File Offset: 0x00049CA0
		protected virtual void OnFullyInitialized()
		{
		}

		// Token: 0x060017E6 RID: 6118 RVA: 0x0004BAA2 File Offset: 0x00049CA2
		protected virtual void OnPlayerEnteredChunk(ILocationChunkVisitor playerVisitor, int remainingEnemyCount)
		{
		}

		// Token: 0x060017E7 RID: 6119 RVA: 0x0004BAA4 File Offset: 0x00049CA4
		protected virtual void OnPlayerMobEnteredChunk(ILocationChunkVisitor playerMobVisitor)
		{
		}

		// Token: 0x060017E8 RID: 6120 RVA: 0x0004BAA6 File Offset: 0x00049CA6
		protected virtual void OnEnemyMobExitedChunk(ILocationChunkVisitor mobVisitor, int remainingEnemyCount)
		{
		}

		// Token: 0x060017E9 RID: 6121 RVA: 0x0004BAA8 File Offset: 0x00049CA8
		protected virtual void OnClearingStateChanged(bool isCleared)
		{
		}

		// Token: 0x060017EA RID: 6122 RVA: 0x0004BAAC File Offset: 0x00049CAC
		private void OnVisitorEnteredLocationChunk(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (visitor.IsPlayerVisitor())
			{
				this._isPlayerInsideChunk = true;
				this.OnPlayerEnteredChunk(visitor, this._currentEnemyMobsCount);
				return;
			}
			if (visitor.IsPlayerMobVisitor())
			{
				this.OnPlayerMobEnteredChunk(visitor);
				return;
			}
			if (this.CountEnemyMob(visitor))
			{
				this._currentEnemyMobsCount++;
				this.UpdateClearingState(false);
			}
		}

		// Token: 0x060017EB RID: 6123 RVA: 0x0004BB04 File Offset: 0x00049D04
		private void OnVisitorExitedLocationChunk(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (this._isPlayerInsideChunk && visitor.IsPlayerVisitor())
			{
				this._isPlayerInsideChunk = false;
			}
			if (this._currentEnemyMobsCount > 0 && this.CountEnemyMob(visitor))
			{
				this._currentEnemyMobsCount--;
				this.OnEnemyMobExitedChunk(visitor, this._currentEnemyMobsCount);
				this.UpdateClearingState(this._currentEnemyMobsCount == 0);
			}
		}

		// Token: 0x060017EC RID: 6124 RVA: 0x0004BB64 File Offset: 0x00049D64
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this._currentEnemyMobsCount = 0;
			this.isInitialized = false;
		}

		// Token: 0x060017ED RID: 6125 RVA: 0x0004BB7B File Offset: 0x00049D7B
		private void Start()
		{
			this.TryToInititalize();
		}

		// Token: 0x060017EE RID: 6126 RVA: 0x0004BB83 File Offset: 0x00049D83
		private void OnEnable()
		{
			this.TryToInititalize();
		}

		// Token: 0x060017EF RID: 6127 RVA: 0x0004BB8C File Offset: 0x00049D8C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this._targetLocationChunk != null)
			{
				this._targetLocationChunk.VisitorAdded -= this.OnVisitorEnteredLocationChunk;
				this._targetLocationChunk.VisitorRemoved -= this.OnVisitorExitedLocationChunk;
			}
		}

		// Token: 0x04000D99 RID: 3481
		[SerializeField]
		private LocationChunk _targetLocationChunk;

		// Token: 0x04000D9A RID: 3482
		private GameSessionManager _gameSessionManager;

		// Token: 0x04000D9B RID: 3483
		private bool _isPlayerInsideChunk;

		// Token: 0x04000D9C RID: 3484
		private int _currentEnemyMobsCount;

		// Token: 0x04000D9D RID: 3485
		private bool _isChunkCleared;

		// Token: 0x04000D9E RID: 3486
		private bool isInitialized;
	}
}
