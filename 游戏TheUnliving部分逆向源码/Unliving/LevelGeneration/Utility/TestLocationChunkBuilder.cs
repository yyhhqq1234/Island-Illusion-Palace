using System;
using Common.ServiceRegistry;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameScene;
using Unliving.Mobs;

namespace Unliving.LevelGeneration.Utility
{
	// Token: 0x02000279 RID: 633
	[Service(typeof(IGameLocationProvider), new Type[]
	{

	})]
	[DefaultExecutionOrder(-110)]
	public sealed class TestLocationChunkBuilder : GlobalSceneManagerBase, IGameLocationProvider
	{
		// Token: 0x1700048F RID: 1167
		// (get) Token: 0x06001599 RID: 5529 RVA: 0x0004535D File Offset: 0x0004355D
		// (set) Token: 0x0600159A RID: 5530 RVA: 0x00045364 File Offset: 0x00043564
		public static GameLocation TestLocation { get; private set; }

		// Token: 0x17000490 RID: 1168
		// (get) Token: 0x0600159B RID: 5531 RVA: 0x0004536C File Offset: 0x0004356C
		public float LocationGenerationProgress
		{
			get
			{
				return 1f;
			}
		}

		// Token: 0x17000491 RID: 1169
		// (get) Token: 0x0600159C RID: 5532 RVA: 0x00045373 File Offset: 0x00043573
		public GameLocation CurrentLocation
		{
			get
			{
				return TestLocationChunkBuilder.TestLocation;
			}
		}

		// Token: 0x17000492 RID: 1170
		// (get) Token: 0x0600159D RID: 5533 RVA: 0x0004537A File Offset: 0x0004357A
		GameLocation.TypeID IGameLocationProvider.LocationType
		{
			get
			{
				return GameLocation.TypeID.Undefined;
			}
		}

		// Token: 0x17000493 RID: 1171
		// (get) Token: 0x0600159E RID: 5534 RVA: 0x0004537D File Offset: 0x0004357D
		public string LevelID
		{
			get
			{
				return "test_level";
			}
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x00045384 File Offset: 0x00043584
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			SceneRelatedObjectsManager sceneRelatedObjectsManager;
			if (currentGame.Services.TryGet<SceneRelatedObjectsManager>(out sceneRelatedObjectsManager))
			{
				sceneRelatedObjectsManager.isActive = false;
			}
			if (TestLocationChunkBuilder.TestLocation == null)
			{
				TestLocationChunkBuilder.TestLocation = new GameLocation
				{
					Type = this.testLocationType
				};
			}
			ILocationChunk locationChunk;
			if (!base.TryGetComponent<ILocationChunk>(out locationChunk))
			{
				return;
			}
			ILocationChunk locationChunk2 = locationChunk;
			GameObject gameObject = new GameObject(((locationChunk2 != null) ? locationChunk2.ToString() : null) + "_visitorsWatcher");
			gameObject.transform.position = locationChunk.Position;
			BoxCollider2D boxCollider2D = gameObject.AddComponent<BoxCollider2D>();
			boxCollider2D.isTrigger = true;
			boxCollider2D.size = locationChunk.WorldSize;
			LocationChunkVisitorsWatcher locationChunkVisitorsWatcher = gameObject.AddComponent<LocationChunkVisitorsWatcher>();
			locationChunkVisitorsWatcher.observableLayers = this.chunkVisitorLayers;
			locationChunkVisitorsWatcher.AreaCollider = boxCollider2D;
			locationChunk.VisitorsWatcher = locationChunkVisitorsWatcher;
			LocationChunkMobsGridController locationChunkMobsGridController;
			if (!base.TryGetComponent<LocationChunkMobsGridController>(out locationChunkMobsGridController))
			{
				base.gameObject.AddComponent<LocationChunkMobsGridController>();
			}
			TestLocationChunkBuilder.TestLocation.SetData(new ILocationChunk[]
			{
				locationChunk
			}, locationChunk.GetBounds());
		}

		// Token: 0x04000C8B RID: 3211
		public GameLocation.TypeID testLocationType;

		// Token: 0x04000C8C RID: 3212
		public LayerMask chunkVisitorLayers = -1;
	}
}
