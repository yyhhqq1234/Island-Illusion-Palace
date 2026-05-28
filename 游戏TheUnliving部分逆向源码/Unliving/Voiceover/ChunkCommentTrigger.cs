using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.Voiceover
{
	// Token: 0x02000020 RID: 32
	[CreateAssetMenu(fileName = "ChunkCommentTrigger", menuName = "Game/VoiceoverTriggers/ChunkCommentTrigger")]
	public class ChunkCommentTrigger : BasyFewTypeTrigger<LocationChunk>
	{
		// Token: 0x06000149 RID: 329 RVA: 0x00005834 File Offset: 0x00003A34
		public override void InitializeTriggerLogic()
		{
			BasyFewTypeTrigger<LocationChunk>.MessagePool<LocationChunk>[] chunkMessages = this.ChunkMessages;
			this.TypedMessagesPool = chunkMessages;
			base.InitializeTriggerLogic();
			this.currentPlayer.LocationChunkFullyReached += this.OnLocationChunkEntered;
		}

		// Token: 0x0600014A RID: 330 RVA: 0x0000586C File Offset: 0x00003A6C
		private void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			VoiceoverMessage[] messages = base.GetMessages(newChunk.ChunkPrototype as LocationChunk);
			if (!messages.IsNull())
			{
				base.QueueTrigger(messages);
			}
		}

		// Token: 0x0600014B RID: 331 RVA: 0x0000589A File Offset: 0x00003A9A
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkFullyReached -= this.OnLocationChunkEntered;
			}
		}

		// Token: 0x0400009A RID: 154
		public ChunkCommentTrigger.ChunkMessagePool[] ChunkMessages;

		// Token: 0x02000403 RID: 1027
		[Serializable]
		public class ChunkMessagePool : BasyFewTypeTrigger<LocationChunk>.MessagePool<LocationChunk>
		{
		}
	}
}
