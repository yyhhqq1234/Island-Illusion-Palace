using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.Voiceover
{
	// Token: 0x02000023 RID: 35
	[CreateAssetMenu(fileName = "EmptyChunkTrigger", menuName = "Game/VoiceoverTriggers/EmptyChunkTrigger")]
	public class EmptyChunkTrigger : BaseVoiceoverTrigger
	{
		// Token: 0x06000155 RID: 341 RVA: 0x00005A56 File Offset: 0x00003C56
		public override void InitializeTriggerLogic()
		{
			this.currentPlayer.LocationChunkFullyReached += this.OnLocationChunkEntered;
			this.lastChunksQueue.Clear();
			this.currentEmptyChunkIndex = 0;
		}

		// Token: 0x06000156 RID: 342 RVA: 0x00005A84 File Offset: 0x00003C84
		private void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			if (this.lastChunksQueue.Contains(newChunk))
			{
				this.currentEmptyChunkIndex = 0;
				return;
			}
			this.lastChunksQueue.Enqueue(newChunk);
			if (this.lastChunksQueue.Count > 10)
			{
				this.lastChunksQueue.Dequeue();
			}
			if (newChunk.GetEnemyMobsCount(this.gameSessionManager, true, false) == 0)
			{
				this.currentEmptyChunkIndex++;
				if (this.currentEmptyChunkIndex >= this.EmptyChunkCount)
				{
					base.QueueTrigger(this.Messages);
					this.currentEmptyChunkIndex = 0;
					return;
				}
			}
			else
			{
				this.currentEmptyChunkIndex = 0;
			}
		}

		// Token: 0x06000157 RID: 343 RVA: 0x00005B15 File Offset: 0x00003D15
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkFullyReached -= this.OnLocationChunkEntered;
			}
		}

		// Token: 0x0400009E RID: 158
		[Header("Пул сообщений")]
		public VoiceoverMessage[] Messages;

		// Token: 0x0400009F RID: 159
		public int EmptyChunkCount = 2;

		// Token: 0x040000A0 RID: 160
		[NonSerialized]
		private int currentEmptyChunkIndex;

		// Token: 0x040000A1 RID: 161
		[NonSerialized]
		private Queue<ILocationChunk> lastChunksQueue = new Queue<ILocationChunk>();
	}
}
