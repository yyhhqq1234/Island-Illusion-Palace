using System;
using System.Runtime.CompilerServices;
using Game.LevelGeneration;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000274 RID: 628
	public sealed class ParentChunksVisibilityInheritor : LocationChunkControllerBase
	{
		// Token: 0x17000488 RID: 1160
		// (get) Token: 0x0600156D RID: 5485 RVA: 0x0004493E File Offset: 0x00042B3E
		public override bool NeedsEntrancePointsReachingNotifications
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600156E RID: 5486 RVA: 0x00044941 File Offset: 0x00042B41
		private void OnParentChunkVisibilityChanged(ILocationChunk parentChunk, bool isVisible)
		{
			if (isVisible)
			{
				this.visibleParentChunksCount++;
			}
			else if (this.visibleParentChunksCount != 0)
			{
				this.visibleParentChunksCount--;
			}
			this.UpdateChunkVisibility(false);
		}

		// Token: 0x0600156F RID: 5487 RVA: 0x00044973 File Offset: 0x00042B73
		protected override void UpdateChunkVisibility(bool force = false)
		{
			this.currentChunk.IsVisible = (!base.ControlVisibility || this.visibleParentChunksCount > 0);
		}

		// Token: 0x06001570 RID: 5488 RVA: 0x00044994 File Offset: 0x00042B94
		protected override void OnChunkAssigned()
		{
			base.OnChunkAssigned();
			this.visibleParentChunksCount = 0;
			this.currentChunk.ProcessGateways(new Action<ILocationChunkGateway>(this.<OnChunkAssigned>g__RegisterParentChunk|5_0));
			this.UpdateChunkVisibility(false);
		}

		// Token: 0x06001571 RID: 5489 RVA: 0x000449C1 File Offset: 0x00042BC1
		protected override void OnDestroy()
		{
			this.currentChunk.ProcessGateways(new Action<ILocationChunkGateway>(this.<OnDestroy>g__UnregisterParentChunk|6_0));
			base.OnDestroy();
		}

		// Token: 0x06001573 RID: 5491 RVA: 0x000449E8 File Offset: 0x00042BE8
		[CompilerGenerated]
		private void <OnChunkAssigned>g__RegisterParentChunk|5_0(ILocationChunkGateway gateway)
		{
			ILocationChunk nextChunk = gateway.GetNextChunk();
			if (nextChunk == null)
			{
				return;
			}
			if (nextChunk.IsVisible)
			{
				this.visibleParentChunksCount++;
			}
			nextChunk.VisibilityChanged += this.OnParentChunkVisibilityChanged;
		}

		// Token: 0x06001574 RID: 5492 RVA: 0x00044A28 File Offset: 0x00042C28
		[CompilerGenerated]
		private void <OnDestroy>g__UnregisterParentChunk|6_0(ILocationChunkGateway gateway)
		{
			ILocationChunk nextChunk = gateway.GetNextChunk();
			if (nextChunk != null)
			{
				nextChunk.VisibilityChanged -= this.OnParentChunkVisibilityChanged;
			}
		}

		// Token: 0x04000C70 RID: 3184
		private int visibleParentChunksCount;
	}
}
