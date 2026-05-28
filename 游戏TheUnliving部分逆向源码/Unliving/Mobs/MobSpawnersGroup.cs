using System;
using System.Collections.Generic;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameScene;
using Unliving.LevelGeneration;

namespace Unliving.Mobs
{
	// Token: 0x020001C8 RID: 456
	[DefaultExecutionOrder(10)]
	public sealed class MobSpawnersGroup : GameBehaviourBase
	{
		// Token: 0x06000E56 RID: 3670 RVA: 0x0002D870 File Offset: 0x0002BA70
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			MobSpawnersGroup.SpawnersBuffer.Clear();
			base.GetComponentsInChildren<MobBehaviourSpawner>(true, MobSpawnersGroup.SpawnersBuffer);
			GameLocation generatedLocation = sceneManager.GeneratedLocation;
			ILocationChunk locationChunk;
			base.transform.root.TryGetComponent<ILocationChunk>(out locationChunk);
			for (int i = 0; i < MobSpawnersGroup.SpawnersBuffer.Count; i++)
			{
				MobBehaviourSpawner mobBehaviourSpawner = MobSpawnersGroup.SpawnersBuffer[i];
				if (mobBehaviourSpawner.InitialLocationChunk == null && ((ILocationObject)mobBehaviourSpawner).CurrentLocationChunk == null && !mobBehaviourSpawner.IsPlayerGroupSpawner())
				{
					ILocationChunk locationChunk2 = locationChunk ?? generatedLocation.GetLocationChunkAtPoint(mobBehaviourSpawner.transform.position, false);
					if (locationChunk2 != null)
					{
						locationChunk2.AddEnvironmentObject(mobBehaviourSpawner);
					}
				}
			}
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x0002D910 File Offset: 0x0002BB10
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			GameSceneManager gameSceneManager;
			if (currentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
		}

		// Token: 0x0400087D RID: 2173
		private static readonly List<MobBehaviourSpawner> SpawnersBuffer = new List<MobBehaviourSpawner>(8);
	}
}
