using System;
using Game.LevelGeneration;
using UnityEngine.Serialization;

namespace Unliving.Gameplay
{
	// Token: 0x020002AA RID: 682
	public sealed class EnemyLocationChunkLocker : EnemyLocationChunkClearingTrigger
	{
		// Token: 0x060017F1 RID: 6129 RVA: 0x0004BBE3 File Offset: 0x00049DE3
		private void UpdateChunkLock(bool isLocked)
		{
			base.TargetLocationChunk.SetLocked(isLocked, this.lockNeighbourChunks, false);
		}

		// Token: 0x060017F2 RID: 6130 RVA: 0x0004BBF8 File Offset: 0x00049DF8
		protected override void OnPlayerEnteredChunk(ILocationChunkVisitor playerVisitor, int remainingEnemyCount)
		{
			if (!base.IsChunkCleared)
			{
				this.UpdateChunkLock(true);
			}
		}

		// Token: 0x060017F3 RID: 6131 RVA: 0x0004BC09 File Offset: 0x00049E09
		protected override void OnClearingStateChanged(bool isCleared)
		{
			if (isCleared)
			{
				this.UpdateChunkLock(false);
				return;
			}
			if (base.IsPlayerInsideChunk)
			{
				this.UpdateChunkLock(true);
			}
		}

		// Token: 0x04000D9F RID: 3487
		[FormerlySerializedAs("lockCorridorsOnly")]
		public bool lockNeighbourChunks;
	}
}
