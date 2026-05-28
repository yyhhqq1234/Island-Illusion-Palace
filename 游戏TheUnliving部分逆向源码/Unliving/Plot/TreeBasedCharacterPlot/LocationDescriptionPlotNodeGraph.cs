using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using Unliving.LevelGeneration;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000301 RID: 769
	[CreateAssetMenu(fileName = "LocationDescriptionPlotNodeGraph", menuName = "Game/Plot/Location Description Plot Node Graph")]
	public class LocationDescriptionPlotNodeGraph : BaseGraph
	{
		// Token: 0x06001A1C RID: 6684 RVA: 0x00051CF4 File Offset: 0x0004FEF4
		public bool TryGetNodeWithID(string descriptionID, out LocationDescriptionPlotNode node)
		{
			node = null;
			for (int i = 0; i < this.nodes.Count; i++)
			{
				LocationDescriptionPlotNode locationDescriptionPlotNode = this.nodes[i] as LocationDescriptionPlotNode;
				if (locationDescriptionPlotNode != null && string.Equals(locationDescriptionPlotNode.descriptionID, descriptionID))
				{
					node = locationDescriptionPlotNode;
					break;
				}
			}
			return node != null;
		}

		// Token: 0x06001A1D RID: 6685 RVA: 0x00051D48 File Offset: 0x0004FF48
		public List<LocationDescriptionPlotNode> GetNodesOfType(GameLocation.TypeID locationID, LocationDescriptionPlotNode.NodeType nodeType, CharacterPlotContext context, Predicate<LocationDescriptionPlotNode> nodeFilter = null)
		{
			this.nodesBuffer.Clear();
			for (int i = 0; i < this.nodes.Count; i++)
			{
				LocationDescriptionPlotNode locationDescriptionPlotNode = this.nodes[i] as LocationDescriptionPlotNode;
				if (locationDescriptionPlotNode != null && locationDescriptionPlotNode.locationID == locationID && locationDescriptionPlotNode.nodeType == nodeType && locationDescriptionPlotNode.IsActivePlotItem(context) && (nodeFilter == null || nodeFilter(locationDescriptionPlotNode)))
				{
					this.nodesBuffer.Add(locationDescriptionPlotNode);
				}
			}
			return this.nodesBuffer;
		}

		// Token: 0x06001A1E RID: 6686 RVA: 0x00051DC8 File Offset: 0x0004FFC8
		public List<LocationDescriptionPlotNode> GetNodes()
		{
			this.nodesBuffer.Clear();
			for (int i = 0; i < this.nodes.Count; i++)
			{
				LocationDescriptionPlotNode locationDescriptionPlotNode = this.nodes[i] as LocationDescriptionPlotNode;
				if (locationDescriptionPlotNode != null)
				{
					this.nodesBuffer.Add(locationDescriptionPlotNode);
				}
			}
			return this.nodesBuffer;
		}

		// Token: 0x04000E82 RID: 3714
		private readonly List<LocationDescriptionPlotNode> nodesBuffer = new List<LocationDescriptionPlotNode>();
	}
}
