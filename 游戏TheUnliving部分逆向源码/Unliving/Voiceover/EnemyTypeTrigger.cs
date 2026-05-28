using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Voiceover
{
	// Token: 0x02000024 RID: 36
	[CreateAssetMenu(fileName = "EnemyTypeTrigger", menuName = "Game/VoiceoverTriggers/EnemyTypeTrigger")]
	public class EnemyTypeTrigger : BasyFewTypeTrigger<MobBehaviour.ID>
	{
		// Token: 0x06000159 RID: 345 RVA: 0x00005B5C File Offset: 0x00003D5C
		public override void InitializeTriggerLogic()
		{
			BasyFewTypeTrigger<MobBehaviour.ID>.MessagePool<MobBehaviour.ID>[] enemyTypeMessages = this.EnemyTypeMessages;
			this.TypedMessagesPool = enemyTypeMessages;
			base.InitializeTriggerLogic();
			this.currentPlayer.LocationChunkFullyReached += this.OnLocationChunkEntered;
		}

		// Token: 0x0600015A RID: 346 RVA: 0x00005B94 File Offset: 0x00003D94
		private void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			foreach (BasyFewTypeTrigger<MobBehaviour.ID>.MessagePool<MobBehaviour.ID> messagePool in this.TypedMessagesPool)
			{
				if (newChunk.HasEnemyMobWithID(this.gameSessionManager, messagePool.TypeID))
				{
					base.QueueTrigger(base.GetMessages(messagePool.TypeID));
					return;
				}
			}
		}

		// Token: 0x0600015B RID: 347 RVA: 0x00005BE1 File Offset: 0x00003DE1
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkFullyReached -= this.OnLocationChunkEntered;
			}
		}

		// Token: 0x040000A2 RID: 162
		[Header("Специальные сообщения")]
		public EnemyTypeTrigger.EnemyTypeMessagePool[] EnemyTypeMessages;

		// Token: 0x02000406 RID: 1030
		[Serializable]
		public class EnemyTypeMessagePool : BasyFewTypeTrigger<MobBehaviour.ID>.MessagePool<MobBehaviour.ID>
		{
		}
	}
}
