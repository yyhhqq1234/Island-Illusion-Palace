using System;
using Common.Editor;
using Common.Math;
using Game.Core;
using Game.LevelGeneration;
using Game.Utility;
using UnityEngine;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Test
{
	// Token: 0x0200003E RID: 62
	public sealed class MobsSearchQueriesBenchmark : GameBehaviourBase
	{
		// Token: 0x06000215 RID: 533 RVA: 0x000084AC File Offset: 0x000066AC
		private BaseGameMob CreateTestMob()
		{
			GameObject gameObject = new GameObject("TestMob")
			{
				layer = this.mobLayer
			};
			if (this.usePhysicsQueries)
			{
				CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
				Rigidbody2D rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
				circleCollider2D.radius = this.mobRadius;
				rigidbody2D.isKinematic = true;
			}
			PushableGameMob pushableGameMob = gameObject.AddComponent<PushableGameMob>();
			pushableGameMob.Radius = this.mobRadius;
			return pushableGameMob;
		}

		// Token: 0x06000216 RID: 534 RVA: 0x0000850C File Offset: 0x0000670C
		private void SearchTargets(BaseGameMob mob, int mobIndex)
		{
			float num = this.searchRanges[mobIndex];
			if (this.usePhysicsQueries)
			{
				int num2 = Physics2D.OverlapCircleNonAlloc(mob.Position, num, MobsSearchQueriesBenchmark.CollidersBuffer, this.mobsLayers);
				for (int i = 0; i < num2; i++)
				{
					BaseGameMob baseGameMob;
					if (MobsSearchQueriesBenchmark.CollidersBuffer[i].TryGetComponent<BaseGameMob>(out baseGameMob))
					{
						Debug.DrawLine(mob.Position, baseGameMob.Position);
					}
				}
				return;
			}
			MobsSearchQueriesBenchmark.MobsGatheringArgs.position = mob.Position;
			MobsSearchQueriesBenchmark.MobsGatheringArgs.range = num;
			MobsSearchQueriesBenchmark.MobsGatheringArgs.layers = this.mobsLayers;
			BaseGameMob[] array;
			int mobsInRange = this.currentLocation.GetMobsInRange(MobsSearchQueriesBenchmark.MobsGatheringArgs, out array);
			for (int j = 0; j < mobsInRange; j++)
			{
				Debug.DrawLine(mob.Position, array[j].Position);
			}
		}

		// Token: 0x06000217 RID: 535 RVA: 0x000085EC File Offset: 0x000067EC
		private void Initialize(GameSceneManager sceneManager)
		{
			this.currentLocation = sceneManager.GeneratedLocation;
			LocationChunk locationChunk = (LocationChunk)this.currentLocation.Chunks[0];
			this.chunkGridController = locationChunk.MobsGrid;
			Bounds bounds = locationChunk.GetBounds();
			Vector2 a = bounds.min;
			Vector2 vector = bounds.size;
			int num = UnityEngine.Random.Range(0, 1000);
			this.testMobs = new BaseGameMob[this.mobsCount];
			this.searchRanges = new float[this.mobsCount];
			for (int i = 0; i < this.mobsCount; i++)
			{
				Vector2 r2Value = PhiSequence.GetR2Value(i + num, false);
				r2Value.x *= vector.x;
				r2Value.y *= vector.y;
				BaseGameMob baseGameMob = this.CreateTestMob();
				baseGameMob.Position = a + r2Value;
				this.testMobs[i] = baseGameMob;
				this.searchRanges[i] = UnityEngine.Random.Range(this.minTargetsSearchRange, this.maxTargetsSearchRange);
			}
		}

		// Token: 0x06000218 RID: 536 RVA: 0x000086F8 File Offset: 0x000068F8
		private void Start()
		{
			this.mobsLayers = 1 << this.mobLayer;
			this.timeOffsets = new Vector2(UnityEngine.Random.value, UnityEngine.Random.value) * 100f;
			GameSceneManager gameSceneManager;
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.Initialize));
			}
		}

		// Token: 0x06000219 RID: 537 RVA: 0x0000875C File Offset: 0x0000695C
		private void Update()
		{
			UniformGrid2D grid = this.chunkGridController.Grid;
			if (grid == null)
			{
				return;
			}
			Vector2 size = grid.Size;
			Vector2 origin = grid.Origin;
			Vector2 position = origin + size * 0.5f;
			float num = Time.time + this.timeOffsets.x;
			float num2 = Time.time + this.timeOffsets.y;
			float deltaTime = Time.deltaTime;
			for (int i = 0; i < this.testMobs.Length; i++)
			{
				BaseGameMob baseGameMob = this.testMobs[i];
				Vector2 position2 = baseGameMob.Position;
				float num3 = (position2.x - origin.x) / size.x;
				float num4 = (position2.y - origin.y) / size.y;
				Vector2 a = new Vector2
				{
					x = Mathf.PerlinNoise((num + (float)i) * this.velocityNoiseFrequency, (num2 + (float)i) * this.velocityNoiseFrequency) * 2f - 1f,
					y = Mathf.PerlinNoise((num2 + (float)i) * this.velocityNoiseFrequency, (num + (float)i) * this.velocityNoiseFrequency) * 2f - 1f
				};
				a.Normalize();
				a *= this.mobsSpeed;
				baseGameMob.Position += a * deltaTime;
				if (num3 < 0f || num3 > 1f || num4 < 0f || num4 > 1f)
				{
					baseGameMob.Position = position;
				}
				this.SearchTargets(baseGameMob, i);
			}
		}

		// Token: 0x0600021A RID: 538 RVA: 0x000088F7 File Offset: 0x00006AF7
		private void OnDrawGizmos()
		{
			LocationChunkMobsGridController locationChunkMobsGridController = this.chunkGridController;
			if (locationChunkMobsGridController == null)
			{
				return;
			}
			UniformGrid2D grid = locationChunkMobsGridController.Grid;
			if (grid == null)
			{
				return;
			}
			grid.DrawGizmos();
		}

		// Token: 0x0400012B RID: 299
		private static readonly Collider2D[] CollidersBuffer = new Collider2D[512];

		// Token: 0x0400012C RID: 300
		private static readonly GameLocation.MobsGatheringArgs MobsGatheringArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x0400012D RID: 301
		public int mobsCount = 300;

		// Token: 0x0400012E RID: 302
		[Layer]
		public int mobLayer = 1;

		// Token: 0x0400012F RID: 303
		public float mobRadius = 0.5f;

		// Token: 0x04000130 RID: 304
		public float mobsSpeed = 10f;

		// Token: 0x04000131 RID: 305
		public float velocityNoiseFrequency = 0.5f;

		// Token: 0x04000132 RID: 306
		public float minTargetsSearchRange = 5f;

		// Token: 0x04000133 RID: 307
		public float maxTargetsSearchRange = 15f;

		// Token: 0x04000134 RID: 308
		public bool usePhysicsQueries;

		// Token: 0x04000135 RID: 309
		private GameLocation currentLocation;

		// Token: 0x04000136 RID: 310
		private LocationChunkMobsGridController chunkGridController;

		// Token: 0x04000137 RID: 311
		private BaseGameMob[] testMobs;

		// Token: 0x04000138 RID: 312
		private float[] searchRanges;

		// Token: 0x04000139 RID: 313
		private int mobsLayers;

		// Token: 0x0400013A RID: 314
		private Vector2 timeOffsets;
	}
}
