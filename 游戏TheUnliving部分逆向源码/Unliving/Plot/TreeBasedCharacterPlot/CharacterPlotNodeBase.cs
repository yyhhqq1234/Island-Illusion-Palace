using System;
using Common;
using GraphProcessor;
using UnityEngine;
using Unliving.Plot.Triggers;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000304 RID: 772
	public abstract class CharacterPlotNodeBase : BaseNode, ICharacterPlotItem, IWeighted
	{
		// Token: 0x17000576 RID: 1398
		// (get) Token: 0x06001A29 RID: 6697 RVA: 0x00051F35 File Offset: 0x00050135
		public CharacterPlotNodeGraph Graph
		{
			get
			{
				return this.graph as CharacterPlotNodeGraph;
			}
		}

		// Token: 0x17000577 RID: 1399
		// (get) Token: 0x06001A2A RID: 6698 RVA: 0x00051F42 File Offset: 0x00050142
		public override string name
		{
			get
			{
				return this.ID;
			}
		}

		// Token: 0x17000578 RID: 1400
		// (get) Token: 0x06001A2B RID: 6699 RVA: 0x00051F4A File Offset: 0x0005014A
		public string ID
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return null;
				}
				return plotItem.ID;
			}
		}

		// Token: 0x17000579 RID: 1401
		// (get) Token: 0x06001A2C RID: 6700 RVA: 0x00051F5D File Offset: 0x0005015D
		public int Priority
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return int.MinValue;
				}
				return plotItem.Priority;
			}
		}

		// Token: 0x1700057A RID: 1402
		// (get) Token: 0x06001A2D RID: 6701 RVA: 0x00051F74 File Offset: 0x00050174
		public CharacterPlotItemTriggerBase Trigger
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return null;
				}
				return plotItem.Trigger;
			}
		}

		// Token: 0x1700057B RID: 1403
		// (get) Token: 0x06001A2E RID: 6702 RVA: 0x00051F87 File Offset: 0x00050187
		public CharacterPlotItemTriggerBase DeactivationTrigger
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return null;
				}
				return plotItem.DeactivationTrigger;
			}
		}

		// Token: 0x1700057C RID: 1404
		// (get) Token: 0x06001A2F RID: 6703 RVA: 0x00051F9A File Offset: 0x0005019A
		// (set) Token: 0x06001A30 RID: 6704 RVA: 0x00051FAD File Offset: 0x000501AD
		CharacterPlotItemRuntimeData ICharacterPlotItem.RuntimeData
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return null;
				}
				return plotItem.RuntimeData;
			}
			set
			{
				if (this.GetPlotItem() != null)
				{
					this.GetPlotItem().RuntimeData = value;
				}
			}
		}

		// Token: 0x1700057D RID: 1405
		// (get) Token: 0x06001A31 RID: 6705 RVA: 0x00051FC3 File Offset: 0x000501C3
		// (set) Token: 0x06001A32 RID: 6706 RVA: 0x00051FDA File Offset: 0x000501DA
		float IWeighted.Weight
		{
			get
			{
				CharacterPlotItemBase plotItem = this.GetPlotItem();
				if (plotItem == null)
				{
					return 0f;
				}
				return ((IWeighted)plotItem).Weight;
			}
			set
			{
				if (this.GetPlotItem() != null)
				{
					((IWeighted)this.GetPlotItem()).Weight = value;
				}
			}
		}

		// Token: 0x1700057E RID: 1406
		// (get) Token: 0x06001A33 RID: 6707 RVA: 0x00051FF0 File Offset: 0x000501F0
		public ConversationBranch ConversationBranch
		{
			get
			{
				return this.nodeAsset.thread.ConversationBranch;
			}
		}

		// Token: 0x06001A34 RID: 6708
		public abstract CharacterPlotItemBase GetPlotItem();

		// Token: 0x06001A35 RID: 6709
		public abstract void UpdateNodeAssetName();

		// Token: 0x06001A36 RID: 6710 RVA: 0x00052002 File Offset: 0x00050202
		public void CreateNodeAsset()
		{
			this.nodeAsset = ScriptableObject.CreateInstance<CharacterPlotThreadNodeAsset>();
			this.UpdateNodeAssetName();
			this.nodeAsset.SetNodeData(this.Graph, this.GUID);
		}

		// Token: 0x06001A37 RID: 6711 RVA: 0x0005202C File Offset: 0x0005022C
		public void RemoveNodeAsset()
		{
		}

		// Token: 0x04000E86 RID: 3718
		[Output(null, true, name = "Childs", allowMultiple = true)]
		public CharacterPlotNodeBase output;

		// Token: 0x04000E87 RID: 3719
		[Input(null, false, name = "Parent", allowMultiple = false)]
		public CharacterPlotNodeBase input;

		// Token: 0x04000E88 RID: 3720
		public CharacterPlotThreadNodeAsset nodeAsset;
	}
}
