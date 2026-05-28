using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common;
using Common.CollectionsExtensions;
using Game.BundlesCache;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000275 RID: 629
	[Serializable]
	public sealed class RandomChunksPool
	{
		// Token: 0x17000489 RID: 1161
		// (get) Token: 0x06001575 RID: 5493 RVA: 0x00044A51 File Offset: 0x00042C51
		public bool IsChunksLoadPathDefined
		{
			get
			{
				return !string.IsNullOrEmpty(this.chunksLabel.labelString);
			}
		}

		// Token: 0x1700048A RID: 1162
		// (get) Token: 0x06001576 RID: 5494 RVA: 0x00044A66 File Offset: 0x00042C66
		public LocationChunksCollection LoadedChunks
		{
			get
			{
				return this.loadedChunks;
			}
		}

		// Token: 0x1700048B RID: 1163
		// (get) Token: 0x06001577 RID: 5495 RVA: 0x00044A6E File Offset: 0x00042C6E
		public int CurrentSize
		{
			get
			{
				return this.items.Count;
			}
		}

		// Token: 0x06001578 RID: 5496 RVA: 0x00044A7C File Offset: 0x00042C7C
		private int TrySelectChunksFromPool(LocationChunk.TypeID desiredChunkType, RandomChunksPool.ChunksSelector chunksSelector, out ILocationChunk[] selectedChunksPrototypes, bool hasFallbackIteration = true)
		{
			selectedChunksPrototypes = RandomChunksPool.ChunksBuffer;
			int i = 0;
			while (i < this.items.Count)
			{
				LocationChunk.TypeID typeID = this.items[i];
				bool flag = typeID == this.fallbackChunkType;
				if (flag || typeID == desiredChunkType)
				{
					int num = this.<TrySelectChunksFromPool>g__SelectLoadedChunks|16_0(typeID, chunksSelector, flag);
					if (num != 0 || flag)
					{
						this.lastSelectedChunkType = typeID;
						this.items.RemoveAt(i);
						return num;
					}
					break;
				}
				else
				{
					i++;
				}
			}
			if (hasFallbackIteration)
			{
				this.lastSelectedChunkType = this.fallbackChunkType;
				this.items.Remove(this.fallbackChunkType);
				return this.<TrySelectChunksFromPool>g__SelectLoadedChunks|16_0(this.fallbackChunkType, chunksSelector, true);
			}
			return 0;
		}

		// Token: 0x06001579 RID: 5497 RVA: 0x00044B1D File Offset: 0x00042D1D
		private bool IsAllowedChunkType(LocationChunk.TypeID chunkType)
		{
			return !this.disallowLastSelectedChunkTypeRepetition || chunkType != this.lastSelectedChunkType;
		}

		// Token: 0x0600157A RID: 5498 RVA: 0x00044B38 File Offset: 0x00042D38
		public async Task Initialize(int poolSize, IBundlesCacheManager cacheManager)
		{
			if (this.loadedChunks == null)
			{
				this.loadedChunks = new LocationChunksCollection(10);
				await this.loadedChunks.LoadChunksPrototypes(this.chunksLabel, cacheManager, null);
			}
			if (this.items == null)
			{
				this.items = new List<LocationChunk.TypeID>(32);
			}
			this.lastSelectedChunkType = LocationChunk.TypeID.Undefined;
			this.items.Clear();
			if (poolSize > 0)
			{
				for (int i = 0; i < this.generationOptions.Length; i++)
				{
					RandomChunksPool.ChunkTypeGenerationOption chunkTypeGenerationOption = this.generationOptions[i];
					if (chunkTypeGenerationOption.Weight > 0f)
					{
						int minChunksCount = chunkTypeGenerationOption.minChunksCount;
						int num = 0;
						while (num < minChunksCount && this.items.Count < poolSize)
						{
							this.items.Add(chunkTypeGenerationOption.chunkType);
							num++;
						}
					}
				}
				this.items.RandomFill(this.generationOptions, poolSize, (IWeightedWithMaxCount o) => ((RandomChunksPool.ChunkTypeGenerationOption)o).chunkType, false);
				this.items.Shuffle(-1);
			}
		}

		// Token: 0x0600157B RID: 5499 RVA: 0x00044B8D File Offset: 0x00042D8D
		public int SelectChunks(LocationChunk.TypeID desiredChunkType, RandomChunksPool.ChunksSelector chunksSelector, out ILocationChunk[] selectedChunksPrototypes)
		{
			if (!this.IsAllowedChunkType(desiredChunkType))
			{
				desiredChunkType = this.fallbackChunkType;
			}
			return this.TrySelectChunksFromPool(desiredChunkType, chunksSelector, out selectedChunksPrototypes, true);
		}

		// Token: 0x0600157C RID: 5500 RVA: 0x00044BAC File Offset: 0x00042DAC
		public int SelectChunks(LocationChunk.TypeID[] allowedChunkTypes, RandomChunksPool.ChunksSelector chunksSelector, out ILocationChunk[] selectedChunksPrototypes)
		{
			foreach (LocationChunk.TypeID typeID in allowedChunkTypes)
			{
				if (this.IsAllowedChunkType(typeID))
				{
					int num = this.TrySelectChunksFromPool(typeID, chunksSelector, out selectedChunksPrototypes, false);
					if (num != 0)
					{
						return num;
					}
				}
			}
			return this.TrySelectChunksFromPool(this.fallbackChunkType, chunksSelector, out selectedChunksPrototypes, true);
		}

		// Token: 0x0600157D RID: 5501 RVA: 0x00044BF3 File Offset: 0x00042DF3
		public IEnumerable<LocationChunk.TypeID> GetPoolItems()
		{
			int num;
			for (int i = 0; i < this.items.Count; i = num + 1)
			{
				yield return this.items[i];
				num = i;
			}
			yield break;
		}

		// Token: 0x06001580 RID: 5504 RVA: 0x00044C24 File Offset: 0x00042E24
		[CompilerGenerated]
		private int <TrySelectChunksFromPool>g__SelectLoadedChunks|16_0(LocationChunk.TypeID chunksType, RandomChunksPool.ChunksSelector chunksSelector, bool isFallbackIteration)
		{
			int result = 0;
			IReadOnlyList<ILocationChunk> readOnlyList;
			int chunksPrototypes = this.loadedChunks.GetChunksPrototypes((int)chunksType, out readOnlyList);
			for (int i = 0; i < chunksPrototypes; i++)
			{
				ILocationChunk locationChunk = readOnlyList[i];
				if (chunksSelector == null || chunksSelector.CanBeSelected(isFallbackIteration, locationChunk))
				{
					RandomChunksPool.ChunksBuffer[result++] = locationChunk;
				}
			}
			return result;
		}

		// Token: 0x04000C71 RID: 3185
		private static readonly ILocationChunk[] ChunksBuffer = new ILocationChunk[32];

		// Token: 0x04000C72 RID: 3186
		public AssetLabelReference chunksLabel;

		// Token: 0x04000C73 RID: 3187
		public LocationChunk.TypeID fallbackChunkType = LocationChunk.TypeID.DeadEnd;

		// Token: 0x04000C74 RID: 3188
		public bool disallowLastSelectedChunkTypeRepetition;

		// Token: 0x04000C75 RID: 3189
		public RandomChunksPool.ChunkTypeGenerationOption[] generationOptions;

		// Token: 0x04000C76 RID: 3190
		private LocationChunksCollection loadedChunks;

		// Token: 0x04000C77 RID: 3191
		private List<LocationChunk.TypeID> items;

		// Token: 0x04000C78 RID: 3192
		private LocationChunk.TypeID lastSelectedChunkType;

		// Token: 0x020004F8 RID: 1272
		[Serializable]
		public sealed class ChunkTypeGenerationOption : IWeightedWithMaxCount, IWeighted
		{
			// Token: 0x170007AE RID: 1966
			// (get) Token: 0x060025C2 RID: 9666 RVA: 0x00075F6F File Offset: 0x0007416F
			// (set) Token: 0x060025C3 RID: 9667 RVA: 0x00075F77 File Offset: 0x00074177
			int IWeightedWithMaxCount.MaxCount
			{
				get
				{
					return this.maxChunksCount;
				}
				set
				{
					this.maxChunksCount = value;
				}
			}

			// Token: 0x170007AF RID: 1967
			// (get) Token: 0x060025C4 RID: 9668 RVA: 0x00075F80 File Offset: 0x00074180
			// (set) Token: 0x060025C5 RID: 9669 RVA: 0x00075F88 File Offset: 0x00074188
			public float Weight
			{
				get
				{
					return this.weight;
				}
				set
				{
					this.weight = Mathf.Clamp01(value);
				}
			}

			// Token: 0x060025C6 RID: 9670 RVA: 0x00075F96 File Offset: 0x00074196
			public int GetChunksCount()
			{
				if (this.maxChunksCount <= 0)
				{
					return this.minChunksCount;
				}
				return UnityEngine.Random.Range(this.minChunksCount, this.maxChunksCount);
			}

			// Token: 0x04001A94 RID: 6804
			public LocationChunk.TypeID chunkType;

			// Token: 0x04001A95 RID: 6805
			public int minChunksCount = 1;

			// Token: 0x04001A96 RID: 6806
			public int maxChunksCount;

			// Token: 0x04001A97 RID: 6807
			[SerializeField]
			[Range(0f, 1f)]
			private float weight;
		}

		// Token: 0x020004F9 RID: 1273
		public abstract class ChunksSelector
		{
			// Token: 0x060025C8 RID: 9672
			public abstract bool CanBeSelected(bool isFallbackIteration, ILocationChunk chunk);
		}
	}
}
