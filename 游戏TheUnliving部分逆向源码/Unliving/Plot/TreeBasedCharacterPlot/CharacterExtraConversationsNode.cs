using System;
using GraphProcessor;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000302 RID: 770
	[NodeMenuItem("Plot/Additional Conversations Node", null)]
	[Serializable]
	public sealed class CharacterExtraConversationsNode : CharacterPlotNodeBase
	{
		// Token: 0x06001A20 RID: 6688 RVA: 0x00051E30 File Offset: 0x00050030
		public override CharacterPlotItemBase GetPlotItem()
		{
			CharacterPlotThreadNodeAsset nodeAsset = this.nodeAsset;
			if (nodeAsset == null)
			{
				return null;
			}
			return nodeAsset.thread;
		}

		// Token: 0x17000572 RID: 1394
		// (get) Token: 0x06001A21 RID: 6689 RVA: 0x00051E43 File Offset: 0x00050043
		public override string layoutStyle
		{
			get
			{
				return "CharacterPlotExtraNodeStyle";
			}
		}

		// Token: 0x06001A22 RID: 6690 RVA: 0x00051E4C File Offset: 0x0005004C
		public override void UpdateNodeAssetName()
		{
			string str = string.IsNullOrEmpty(base.ID) ? "Empty" : base.ID;
			this.nodeAsset.name = str + "_extraConversationNode";
		}

		// Token: 0x04000E83 RID: 3715
		public CharacterExtraConversationsNode.Type type;

		// Token: 0x02000543 RID: 1347
		public enum Type
		{
			// Token: 0x04001B97 RID: 7063
			ExpositionConversations,
			// Token: 0x04001B98 RID: 7064
			FillingConversations
		}
	}
}
