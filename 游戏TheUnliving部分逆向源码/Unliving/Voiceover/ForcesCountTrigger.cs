using System;
using Common.UnityExtensions;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Voiceover
{
	// Token: 0x02000025 RID: 37
	[CreateAssetMenu(fileName = "ForcesCountTrigger", menuName = "Game/VoiceoverTriggers/ForcesCountTrigger")]
	public class ForcesCountTrigger : BaseVoiceoverTrigger
	{
		// Token: 0x0600015D RID: 349 RVA: 0x00005C15 File Offset: 0x00003E15
		public override void InitializeTriggerLogic()
		{
			this.currentPlayer.LocationChunkFullyReached += this.OnLocationChunkEntered;
		}

		// Token: 0x0600015E RID: 350 RVA: 0x00005C30 File Offset: 0x00003E30
		private void OnLocationChunkEntered(ILocationChunk newChunk)
		{
			int enemyMobsCount = newChunk.GetEnemyMobsCount(this.gameSessionManager, true, false);
			if (enemyMobsCount == 0)
			{
				return;
			}
			GameMobsGroupControllerBase group = this.currentPlayer.Group;
			float num = (float)((group != null) ? group.Mobs.Count : 0) / (float)enemyMobsCount;
			if (num > 1f + this.ForcesDelta)
			{
				base.QueueTrigger(this.PlayerStrongerMessages);
				return;
			}
			if (num < 1f - this.ForcesDelta)
			{
				base.QueueTrigger(this.EnemyStrongerMessages);
				return;
			}
			base.QueueTrigger(this.ForcesEqualsMessages);
		}

		// Token: 0x0600015F RID: 351 RVA: 0x00005CB5 File Offset: 0x00003EB5
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkFullyReached -= this.OnLocationChunkEntered;
			}
		}

		// Token: 0x040000A3 RID: 163
		[Header("Разница в кол-ве мобов в армии, в процентах(0,1(10%) => 9 и 10 мобов будут считаться одинаковыми силами)")]
		public float ForcesDelta = 0.1f;

		// Token: 0x040000A4 RID: 164
		[Header("Пул сообщений: армия игрока сильнее, чем мобы чанка")]
		public VoiceoverMessage[] PlayerStrongerMessages;

		// Token: 0x040000A5 RID: 165
		[Header("Пул сообщений: армия игрока слабее, чем мобы чанка")]
		public VoiceoverMessage[] EnemyStrongerMessages;

		// Token: 0x040000A6 RID: 166
		[Header("Пул сообщений: силы примерно равны")]
		public VoiceoverMessage[] ForcesEqualsMessages;
	}
}
