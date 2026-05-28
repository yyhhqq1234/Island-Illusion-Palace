using System;
using Common.Editor;
using GraphProcessor;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000306 RID: 774
	[Serializable]
	public class CharacterPlotThreadNodeAsset : ScriptableObject
	{
		// Token: 0x17000581 RID: 1409
		// (get) Token: 0x06001A3E RID: 6718 RVA: 0x000520D8 File Offset: 0x000502D8
		public CharacterPlotNodeBase Node
		{
			get
			{
				BaseNode baseNode;
				if (this.node == null && this.graph.nodesPerGUID.TryGetValue(this.nodeGuid, out baseNode))
				{
					this.node = (baseNode as CharacterPlotNodeBase);
				}
				return this.node;
			}
		}

		// Token: 0x06001A3F RID: 6719 RVA: 0x00052119 File Offset: 0x00050319
		public void SetNodeData(CharacterPlotNodeGraph graph, string nodeGuid)
		{
			this.graph = graph;
			this.nodeGuid = nodeGuid;
		}

		// Token: 0x04000E89 RID: 3721
		[ReadOnly]
		public CharacterPlotNodeGraph graph;

		// Token: 0x04000E8A RID: 3722
		[ReadOnly]
		public string nodeGuid;

		// Token: 0x04000E8B RID: 3723
		public CharacterPlotThread thread = new CharacterPlotThread(new CharactersConversation[0], null);

		// Token: 0x04000E8C RID: 3724
		private CharacterPlotNodeBase node;
	}
}
