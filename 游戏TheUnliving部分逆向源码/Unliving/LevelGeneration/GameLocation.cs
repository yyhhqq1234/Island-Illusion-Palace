using System;
using System.Collections.Generic;
using Game.Abilities.TargetsCollection;
using Game.LevelGeneration;
using Game.Utility;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000268 RID: 616
	public sealed class GameLocation : BaseLocation
	{
		// Token: 0x17000457 RID: 1111
		// (get) Token: 0x06001498 RID: 5272 RVA: 0x000411FD File Offset: 0x0003F3FD
		// (set) Token: 0x06001499 RID: 5273 RVA: 0x00041205 File Offset: 0x0003F405
		public new GameLocation.TypeID Type
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
				base.Type = (int)value;
			}
		}

		// Token: 0x17000458 RID: 1112
		// (get) Token: 0x0600149A RID: 5274 RVA: 0x00041215 File Offset: 0x0003F415
		// (set) Token: 0x0600149B RID: 5275 RVA: 0x0004121D File Offset: 0x0003F41D
		public bool IsTutorialLocation { get; internal set; }

		// Token: 0x0600149C RID: 5276 RVA: 0x00041228 File Offset: 0x0003F428
		public int GetMobsWithColliderInRange(GameLocation.MobsGatheringArgs args, out BaseGameMob[] mobsInRange)
		{
			float range = args.range;
			int layers = args.layers;
			if (layers != 0 && range > 0f)
			{
				Vector2 position = args.position;
				int maxCount = args.maxCount;
				bool flag = maxCount > 0;
				bool flag2 = args.filter == null;
				int num = (flag && args.sortingComparer == null) ? Mathf.Min(GameLocation.MobsQueriesBuffer.Length, maxCount) : GameLocation.MobsQueriesBuffer.Length;
				int num2 = 0;
				int num3 = Physics2D.OverlapCircleNonAlloc(position, range, GameLocation.MobCollidersBuffer, layers);
				mobsInRange = GameLocation.MobsQueriesBuffer;
				for (int i = 0; i < num3; i++)
				{
					Collider2D collider2D = GameLocation.MobCollidersBuffer[i];
					BaseGameMob baseGameMob;
					if (collider2D.TryGetComponent<BaseGameMob>(out baseGameMob) && !(baseGameMob.HitCollider != collider2D) && (flag2 || args.filter(baseGameMob)))
					{
						mobsInRange[num2++] = baseGameMob;
						if (num2 == num)
						{
							break;
						}
					}
				}
				if (args.sortingComparer != null)
				{
					IDistanceBasedComparer distanceBasedComparer = args.sortingComparer as IDistanceBasedComparer;
					if (distanceBasedComparer != null)
					{
						distanceBasedComparer.SortingPoint = position;
					}
					Array.Sort<BaseGameMob>(mobsInRange, 0, num2, args.sortingComparer);
					if (flag && num2 > maxCount)
					{
						num2 = maxCount;
					}
				}
				return num2;
			}
			mobsInRange = Array.Empty<BaseGameMob>();
			return 0;
		}

		// Token: 0x0600149D RID: 5277 RVA: 0x00041358 File Offset: 0x0003F558
		public int GetMobsInRange(GameLocation.MobsGatheringArgs args, out BaseGameMob[] mobsInRange)
		{
			float range = args.range;
			int layers = args.layers;
			if (layers != 0 && range > 0f)
			{
				Vector2 position = args.position;
				int[] array;
				int intersectedLocationChunks = base.GetIntersectedLocationChunks(position, range, false, out array);
				if (intersectedLocationChunks != 0)
				{
					int maxCount = args.maxCount;
					bool flag = maxCount > 0;
					bool flag2 = args.filter == null;
					int num = (flag && args.sortingComparer == null) ? Mathf.Min(GameLocation.MobsQueriesBuffer.Length, maxCount) : GameLocation.MobsQueriesBuffer.Length;
					int num2 = 0;
					mobsInRange = GameLocation.MobsQueriesBuffer;
					int num3 = 0;
					while (num3 < intersectedLocationChunks && num2 < num)
					{
						ILocationChunk locationChunk = this.chunks[array[num3]];
						if (locationChunk.IsVisible)
						{
							LocationChunk locationChunk2 = locationChunk as LocationChunk;
							if (locationChunk2 != null)
							{
								LocationChunkMobsGridController mobsGrid = locationChunk2.MobsGrid;
								UniformGrid2D uniformGrid2D = (mobsGrid != null) ? mobsGrid.Grid : null;
								if (uniformGrid2D != null)
								{
									int[] array2;
									int agentsInRange = uniformGrid2D.GetAgentsInRange(position, range, out array2, -1);
									for (int i = 0; i < agentsInRange; i++)
									{
										BaseGameMob linkedMob = ((LocationChunkMobsGridController.GridAgent)uniformGrid2D.GetAgent(array2[i])).LinkedMob;
										if (linkedMob.IsLayerInMask(layers) && (flag2 || args.filter(linkedMob)))
										{
											mobsInRange[num2++] = linkedMob;
											if (num2 == num)
											{
												break;
											}
										}
									}
								}
							}
						}
						num3++;
					}
					if (args.sortingComparer != null)
					{
						IDistanceBasedComparer distanceBasedComparer = args.sortingComparer as IDistanceBasedComparer;
						if (distanceBasedComparer != null)
						{
							distanceBasedComparer.SortingPoint = position;
						}
						Array.Sort<BaseGameMob>(mobsInRange, 0, num2, args.sortingComparer);
						if (flag && num2 > maxCount)
						{
							num2 = maxCount;
						}
					}
					return num2;
				}
			}
			mobsInRange = Array.Empty<BaseGameMob>();
			return 0;
		}

		// Token: 0x04000BEF RID: 3055
		public static readonly BaseGameMob[] MobsQueriesBuffer = new BaseGameMob[512];

		// Token: 0x04000BF0 RID: 3056
		private static readonly Collider2D[] MobCollidersBuffer = new Collider2D[512];

		// Token: 0x04000BF2 RID: 3058
		private GameLocation.TypeID type;

		// Token: 0x020004E8 RID: 1256
		public enum TypeID
		{
			// Token: 0x04001A3B RID: 6715
			Undefined,
			// Token: 0x04001A3C RID: 6716
			Cemetery,
			// Token: 0x04001A3D RID: 6717
			Swamps,
			// Token: 0x04001A3E RID: 6718
			TheCapital,
			// Token: 0x04001A3F RID: 6719
			Heavens,
			// Token: 0x04001A40 RID: 6720
			Homespace
		}

		// Token: 0x020004E9 RID: 1257
		public sealed class MobsGatheringArgs
		{
			// Token: 0x06002599 RID: 9625 RVA: 0x00074FD2 File Offset: 0x000731D2
			public void Reset()
			{
				this.position = default(Vector2);
				this.range = 0f;
				this.layers = 0;
				this.maxCount = -1;
				this.filter = null;
				this.sortingComparer = null;
			}

			// Token: 0x04001A41 RID: 6721
			public Vector2 position;

			// Token: 0x04001A42 RID: 6722
			public float range;

			// Token: 0x04001A43 RID: 6723
			public int layers;

			// Token: 0x04001A44 RID: 6724
			public int maxCount;

			// Token: 0x04001A45 RID: 6725
			public Predicate<BaseGameMob> filter;

			// Token: 0x04001A46 RID: 6726
			public IComparer<BaseGameMob> sortingComparer;
		}
	}
}
