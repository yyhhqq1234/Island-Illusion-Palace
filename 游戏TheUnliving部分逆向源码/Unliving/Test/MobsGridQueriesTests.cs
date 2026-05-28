using System;
using Game.Core;
using UnityEngine;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Test
{
	// Token: 0x0200003D RID: 61
	public sealed class MobsGridQueriesTests : GameBehaviourBase
	{
		// Token: 0x06000211 RID: 529 RVA: 0x000083C4 File Offset: 0x000065C4
		private void Start()
		{
			base.CurrentGame.Services.TryGet<GameSceneManager>(out this.sceneManager);
		}

		// Token: 0x06000212 RID: 530 RVA: 0x000083E0 File Offset: 0x000065E0
		private void LateUpdate()
		{
			GameSceneManager gameSceneManager = this.sceneManager;
			GameLocation gameLocation = (gameSceneManager != null) ? gameSceneManager.GeneratedLocation : null;
			if (gameLocation == null)
			{
				return;
			}
			MobsGridQueriesTests.MobsGatheringArgs.position = base.transform.position;
			MobsGridQueriesTests.MobsGatheringArgs.range = this.queryRange;
			MobsGridQueriesTests.MobsGatheringArgs.layers = this.mobsLayers;
			BaseGameMob[] array;
			int mobsInRange = gameLocation.GetMobsInRange(MobsGridQueriesTests.MobsGatheringArgs, out array);
			for (int i = 0; i < mobsInRange; i++)
			{
				Debug.DrawLine(array[i].Position, base.transform.position, Color.yellow);
			}
		}

		// Token: 0x04000127 RID: 295
		private static readonly GameLocation.MobsGatheringArgs MobsGatheringArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x04000128 RID: 296
		public float queryRange = 5f;

		// Token: 0x04000129 RID: 297
		public LayerMask mobsLayers = -1;

		// Token: 0x0400012A RID: 298
		private GameSceneManager sceneManager;
	}
}
