using System;
using Common.Editor;
using UnityEngine;
using Unliving.DropSystem;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000311 RID: 785
	[Serializable]
	public class MilestoneDropData : MilestoneDataBase
	{
		// Token: 0x04000EBB RID: 3771
		public MilestoneDropData.DropItemsSettings settings;

		// Token: 0x04000EBC RID: 3772
		[SerializeReference]
		[ManagedObjectField(typeof(IDropable))]
		public IDropable[] dropables;

		// Token: 0x02000546 RID: 1350
		public enum DropItemsSettings
		{
			// Token: 0x04001B9F RID: 7071
			AddItems,
			// Token: 0x04001BA0 RID: 7072
			OverrideItems
		}
	}
}
