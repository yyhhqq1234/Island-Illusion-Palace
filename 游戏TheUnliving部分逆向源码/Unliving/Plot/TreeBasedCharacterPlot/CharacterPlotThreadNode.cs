using System;
using System.Collections.Generic;
using System.Linq;
using GraphProcessor;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000305 RID: 773
	[NodeMenuItem("Plot/Plot Thread Node", null)]
	[Serializable]
	public sealed class CharacterPlotThreadNode : CharacterPlotNodeBase
	{
		// Token: 0x1700057F RID: 1407
		// (get) Token: 0x06001A39 RID: 6713 RVA: 0x00052038 File Offset: 0x00050238
		public CharacterPlotThread ParentPlotThread
		{
			get
			{
				IEnumerable<BaseNode> inputNodes = base.GetInputNodes();
				if (inputNodes == null || !inputNodes.Any<BaseNode>())
				{
					return null;
				}
				CharacterPlotThreadNode characterPlotThreadNode = (CharacterPlotThreadNode)inputNodes.First<BaseNode>();
				if (characterPlotThreadNode == null)
				{
					return null;
				}
				return characterPlotThreadNode.nodeAsset.thread;
			}
		}

		// Token: 0x06001A3A RID: 6714 RVA: 0x00052074 File Offset: 0x00050274
		public override CharacterPlotItemBase GetPlotItem()
		{
			CharacterPlotThreadNodeAsset nodeAsset = this.nodeAsset;
			if (nodeAsset == null)
			{
				return null;
			}
			return nodeAsset.thread;
		}

		// Token: 0x17000580 RID: 1408
		// (get) Token: 0x06001A3B RID: 6715 RVA: 0x00052087 File Offset: 0x00050287
		public override string layoutStyle
		{
			get
			{
				return "CharacterPlotThreadNodeStyle";
			}
		}

		// Token: 0x06001A3C RID: 6716 RVA: 0x00052090 File Offset: 0x00050290
		public override void UpdateNodeAssetName()
		{
			string str = string.IsNullOrEmpty(base.ID) ? "Empty" : base.ID;
			this.nodeAsset.name = str + "_threadNode";
		}
	}
}
