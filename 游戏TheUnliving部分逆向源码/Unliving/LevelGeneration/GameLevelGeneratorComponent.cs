using System;
using Game.LevelGeneration;
using UnityEngine;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000267 RID: 615
	[DefaultExecutionOrder(-120)]
	public sealed class GameLevelGeneratorComponent : MonoBehaviour
	{
		// Token: 0x17000456 RID: 1110
		// (get) Token: 0x06001491 RID: 5265 RVA: 0x00041074 File Offset: 0x0003F274
		public GameLocationGenerator Generator
		{
			get
			{
				return this._generator;
			}
		}

		// Token: 0x06001492 RID: 5266 RVA: 0x0004107C File Offset: 0x0003F27C
		private async void Awake()
		{
			this._generator.StartPosition = base.transform.position;
			await this._generator.GenerateLocation(this._generationArgs, delegate(IGameLocation loc)
			{
				this.generatedLocation = loc;
			});
		}

		// Token: 0x06001493 RID: 5267 RVA: 0x000410B5 File Offset: 0x0003F2B5
		private void OnDestroy()
		{
			this._generator.Cleanup();
		}

		// Token: 0x06001494 RID: 5268 RVA: 0x000410C4 File Offset: 0x0003F2C4
		private void OnGUI()
		{
			if (this.generatedLocation != null)
			{
				GUILayout.Label(string.Format("used_seed: {0}", this._generator.UsedSeed), Array.Empty<GUILayoutOption>());
				GUILayout.Label(string.Format("chunk_count: {0}", this.generatedLocation.Chunks.Count), Array.Empty<GUILayoutOption>());
			}
		}

		// Token: 0x06001495 RID: 5269 RVA: 0x00041128 File Offset: 0x0003F328
		private void OnDrawGizmos()
		{
			if (this.generatedLocation != null)
			{
				Gizmos.color = Color.white;
				Gizmos.DrawWireCube(this.generatedLocation.Bounds.center, this.generatedLocation.Bounds.size);
				Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
				foreach (ILocationChunk locationChunk in this.generatedLocation.Chunks)
				{
					Gizmos.DrawWireCube(locationChunk.Position, locationChunk.WorldSize);
				}
			}
		}

		// Token: 0x04000BEC RID: 3052
		[SerializeField]
		private GameLocationGenerator _generator;

		// Token: 0x04000BED RID: 3053
		[SerializeField]
		private GameLocationGenerator.GenerationArgs _generationArgs;

		// Token: 0x04000BEE RID: 3054
		private IGameLocation generatedLocation;
	}
}
