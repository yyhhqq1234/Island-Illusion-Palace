using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Game.Localization;
using GraphProcessor;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Plot.Triggers;

namespace Unliving.Plot.TreeBasedCharacterPlot
{
	// Token: 0x02000307 RID: 775
	[NodeMenuItem("Plot/Location Description Plot Node", null)]
	[Serializable]
	public sealed class LocationDescriptionPlotNode : BaseNode, ILocalizableDataHolder
	{
		// Token: 0x17000582 RID: 1410
		// (get) Token: 0x06001A41 RID: 6721 RVA: 0x00052143 File Offset: 0x00050343
		public override string name
		{
			get
			{
				if (!string.IsNullOrEmpty(this.descriptionID))
				{
					return this.descriptionID;
				}
				return "Location Description";
			}
		}

		// Token: 0x17000583 RID: 1411
		// (get) Token: 0x06001A42 RID: 6722 RVA: 0x0005215E File Offset: 0x0005035E
		public LocationDescriptionPlotNode ParentNode
		{
			get
			{
				return this.input;
			}
		}

		// Token: 0x17000584 RID: 1412
		// (get) Token: 0x06001A43 RID: 6723 RVA: 0x00052168 File Offset: 0x00050368
		public float UsingProgress
		{
			get
			{
				if (this.loopGeneration)
				{
					return 0f;
				}
				if (this.nodeMetadata != null)
				{
					return (float)this.usedDescriptionStrings.Count / (float)this.nodeMetadata.AdditionalText.Length;
				}
				return (float)((this.usedDescriptionStrings.Count > 0) ? 1 : 0);
			}
		}

		// Token: 0x17000585 RID: 1413
		// (get) Token: 0x06001A44 RID: 6724 RVA: 0x000521BA File Offset: 0x000503BA
		string ILocalizableDataHolder.DataID
		{
			get
			{
				return this.descriptionID;
			}
		}

		// Token: 0x06001A45 RID: 6725 RVA: 0x000521C2 File Offset: 0x000503C2
		public bool IsActivePlotItem(CharacterPlotContext context)
		{
			return (this.ParentNode == null || this.ParentNode.IsActivePlotItem(context)) && this.UsingProgress != 1f && (this.trigger == null || this.trigger.IsFired(context));
		}

		// Token: 0x06001A46 RID: 6726 RVA: 0x00052201 File Offset: 0x00050401
		public void SetUsedDescriptions(List<int> indexes)
		{
			this.usedDescriptionStrings = new List<int>(indexes);
		}

		// Token: 0x06001A47 RID: 6727 RVA: 0x0005220F File Offset: 0x0005040F
		public List<int> GetUsedDescriptions()
		{
			return this.usedDescriptionStrings;
		}

		// Token: 0x06001A48 RID: 6728 RVA: 0x00052217 File Offset: 0x00050417
		public void ResetUsedDescriptions()
		{
			this.usedDescriptionStrings.Clear();
		}

		// Token: 0x06001A49 RID: 6729 RVA: 0x00052224 File Offset: 0x00050424
		public Metadata GetLocationDescriptionMetadata()
		{
			string[] additionalText = this.nodeMetadata.AdditionalText;
			if (additionalText == null || additionalText.Length == 0)
			{
				return this.nodeMetadata;
			}
			if (this.usedDescriptionStrings.Count == additionalText.Length)
			{
				if (!this.loopGeneration)
				{
					return null;
				}
				this.ResetUsedDescriptions();
			}
			string randomItem = additionalText.GetRandomItem(this.usedDescriptionStrings);
			int item = Array.IndexOf<string>(additionalText, randomItem);
			this.usedDescriptionStrings.Add(item);
			this.nodeMetadata.Description = randomItem;
			return this.nodeMetadata;
		}

		// Token: 0x06001A4A RID: 6730 RVA: 0x000522A0 File Offset: 0x000504A0
		void ILocalizableDataHolder.SetLocalizedData(LocalizationManager localizationManager, object data)
		{
			Metadata metadata = data as Metadata;
			if (metadata != null)
			{
				this.nodeMetadata = metadata;
			}
		}

		// Token: 0x04000E8D RID: 3725
		public string descriptionID;

		// Token: 0x04000E8E RID: 3726
		public GameLocation.TypeID locationID;

		// Token: 0x04000E8F RID: 3727
		public LocationDescriptionPlotNode.NodeType nodeType;

		// Token: 0x04000E90 RID: 3728
		public int priority;

		// Token: 0x04000E91 RID: 3729
		[Tooltip("После использования всех описаний список обнуляется и они показываются по новой")]
		public bool loopGeneration;

		// Token: 0x04000E92 RID: 3730
		public bool tutorialNode;

		// Token: 0x04000E93 RID: 3731
		[SerializeReference]
		[CharacterPlotItemTrigger]
		private CharacterPlotItemTriggerBase trigger;

		// Token: 0x04000E94 RID: 3732
		[Output(null, true, name = "Childs", allowMultiple = true)]
		public LocationDescriptionPlotNode output;

		// Token: 0x04000E95 RID: 3733
		[Input(null, false, name = "Parent", allowMultiple = false)]
		public LocationDescriptionPlotNode input;

		// Token: 0x04000E96 RID: 3734
		private List<int> usedDescriptionStrings = new List<int>();

		// Token: 0x04000E97 RID: 3735
		private Metadata nodeMetadata;

		// Token: 0x02000544 RID: 1348
		public enum NodeType
		{
			// Token: 0x04001B9A RID: 7066
			Main,
			// Token: 0x04001B9B RID: 7067
			Exposition
		}
	}
}
