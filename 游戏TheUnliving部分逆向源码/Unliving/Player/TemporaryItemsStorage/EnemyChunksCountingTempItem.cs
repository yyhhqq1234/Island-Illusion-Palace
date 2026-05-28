using System;
using System.Runtime.CompilerServices;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.Gameplay;
using Unliving.LevelGeneration;

namespace Unliving.Player.TemporaryItemsStorage
{
	// Token: 0x02000174 RID: 372
	public sealed class EnemyChunksCountingTempItem : TemporaryItemBase
	{
		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06000A4B RID: 2635 RVA: 0x000223F2 File Offset: 0x000205F2
		public override bool IsExpired
		{
			get
			{
				return this.currentChunksCount >= this.targetChunksCount;
			}
		}

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06000A4C RID: 2636 RVA: 0x00022405 File Offset: 0x00020605
		public override float ExpirationProgress
		{
			get
			{
				return (float)this.currentChunksCount / (float)this.targetChunksCount;
			}
		}

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06000A4D RID: 2637 RVA: 0x00022416 File Offset: 0x00020616
		public override object CurrentProgressValue
		{
			get
			{
				return this.currentChunksCount;
			}
		}

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06000A4E RID: 2638 RVA: 0x00022423 File Offset: 0x00020623
		public override object TargetProgressValue
		{
			get
			{
				return this.targetChunksCount;
			}
		}

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06000A4F RID: 2639 RVA: 0x00022430 File Offset: 0x00020630
		public override string ID
		{
			get
			{
				return "enemy_chunks_counting_item";
			}
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x00022437 File Offset: 0x00020637
		private bool IsValidChunkType(LocationChunk.TypeID chunkType)
		{
			return this.expectedChunksTypes == null || this.expectedChunksTypes.Length == 0 || this.expectedChunksTypes.Contains(chunkType);
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x00022458 File Offset: 0x00020658
		private void SetCurrentChunk(ILocationChunk newChunk)
		{
			if (this.currentChunk == (Component)newChunk)
			{
				return;
			}
			this.<SetCurrentChunk>g__SetClearingEventSubscription|15_0(this.currentChunk, false);
			this.currentChunk = null;
			LocationChunk locationChunk = newChunk as LocationChunk;
			if (locationChunk != null && this.IsValidChunkType(locationChunk.Type))
			{
				this.<SetCurrentChunk>g__SetClearingEventSubscription|15_0(locationChunk, true);
				this.currentChunk = locationChunk;
			}
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x000224B4 File Offset: 0x000206B4
		private void OnEnemyChunkClearingStateChanged(ILocationChunk enemyChunk, bool isCleared)
		{
			if (isCleared)
			{
				this.currentChunksCount++;
			}
		}

		// Token: 0x06000A53 RID: 2643 RVA: 0x000224C7 File Offset: 0x000206C7
		public EnemyChunksCountingTempItem(object content, LocationChunk.TypeID[] expectedChunksTypes, int targetChunksCount) : base(content)
		{
			this.expectedChunksTypes = expectedChunksTypes;
			this.targetChunksCount = targetChunksCount;
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x000224DE File Offset: 0x000206DE
		private void OnLocationChunkEntered(ILocationChunk lastChunk, ILocationChunk newChunk)
		{
			this.SetCurrentChunk(newChunk);
		}

		// Token: 0x06000A55 RID: 2645 RVA: 0x000224E7 File Offset: 0x000206E7
		public override void OnStored(TemporaryItemsStorageController storage)
		{
			storage.ControllerOwner.LocationChunkEntered += this.OnLocationChunkEntered;
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x00022500 File Offset: 0x00020700
		public override void OnDiscarded(TemporaryItemsStorageController storage)
		{
			this.SetCurrentChunk(null);
			storage.ControllerOwner.LocationChunkEntered -= this.OnLocationChunkEntered;
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x00022520 File Offset: 0x00020720
		[CompilerGenerated]
		private void <SetCurrentChunk>g__SetClearingEventSubscription|15_0(LocationChunk targetChunk, bool subscribe)
		{
			EnemyLocationChunkClearingTrigger enemyLocationChunkClearingTrigger;
			if (targetChunk == null || !targetChunk.TryGetComponent<EnemyLocationChunkClearingTrigger>(out enemyLocationChunkClearingTrigger))
			{
				return;
			}
			if (subscribe)
			{
				enemyLocationChunkClearingTrigger.ClearingStateChanged += this.OnEnemyChunkClearingStateChanged;
				return;
			}
			enemyLocationChunkClearingTrigger.ClearingStateChanged -= this.OnEnemyChunkClearingStateChanged;
		}

		// Token: 0x0400060C RID: 1548
		private readonly LocationChunk.TypeID[] expectedChunksTypes;

		// Token: 0x0400060D RID: 1549
		private readonly int targetChunksCount;

		// Token: 0x0400060E RID: 1550
		private int currentChunksCount;

		// Token: 0x0400060F RID: 1551
		private LocationChunk currentChunk;
	}
}
