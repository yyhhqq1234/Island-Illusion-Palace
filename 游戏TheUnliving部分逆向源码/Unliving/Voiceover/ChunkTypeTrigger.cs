using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.Voiceover
{
	// Token: 0x02000021 RID: 33
	[CreateAssetMenu(fileName = "ChunkTypeTrigger", menuName = "Game/VoiceoverTriggers/ChunkTypeTrigger")]
	public class ChunkTypeTrigger : BasyFewTypeTrigger<LocationChunk.TypeID>
	{
		// Token: 0x0600014D RID: 333 RVA: 0x000058D0 File Offset: 0x00003AD0
		public override void InitializeTriggerLogic()
		{
			BasyFewTypeTrigger<LocationChunk.TypeID>.MessagePool<LocationChunk.TypeID>[] chunkMessages = this.ChunkMessages;
			this.TypedMessagesPool = chunkMessages;
			base.InitializeTriggerLogic();
			this.currentPlayer.LocationChunkFullyReached += this.OnLocationChunkEntered;
		}

		// Token: 0x0600014E RID: 334 RVA: 0x00005908 File Offset: 0x00003B08
		private void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			if (newChunk.LocationObjectType == null)
			{
				return;
			}
			VoiceoverMessage[] messages = base.GetMessages((LocationChunk.TypeID)newChunk.LocationObjectType.Value);
			if (!messages.IsNull() && messages.Length != 0)
			{
				base.QueueTrigger(messages);
			}
		}

		// Token: 0x0600014F RID: 335 RVA: 0x0000594E File Offset: 0x00003B4E
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkFullyReached -= this.OnLocationChunkEntered;
			}
		}

		// Token: 0x0400009B RID: 155
		[Header("Специальные сообщения")]
		public ChunkTypeTrigger.ChunkMessagePool[] ChunkMessages;

		// Token: 0x02000404 RID: 1028
		[Serializable]
		public class ChunkMessagePool : BasyFewTypeTrigger<LocationChunk.TypeID>.MessagePool<LocationChunk.TypeID>
		{
		}
	}
}
