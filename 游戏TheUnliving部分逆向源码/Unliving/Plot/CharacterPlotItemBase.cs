using System;
using Common;
using UnityEngine;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D5 RID: 725
	public abstract class CharacterPlotItemBase : ICharacterPlotItem, IWeighted
	{
		// Token: 0x1700054F RID: 1359
		// (get) Token: 0x06001937 RID: 6455 RVA: 0x0004F9E6 File Offset: 0x0004DBE6
		// (set) Token: 0x06001938 RID: 6456 RVA: 0x0004F9EF File Offset: 0x0004DBEF
		float IWeighted.Weight
		{
			get
			{
				return (float)this.priority;
			}
			set
			{
				this.priority = (int)value;
			}
		}

		// Token: 0x17000550 RID: 1360
		// (get) Token: 0x06001939 RID: 6457
		// (set) Token: 0x0600193A RID: 6458
		public abstract string ID { get; set; }

		// Token: 0x17000551 RID: 1361
		// (get) Token: 0x0600193B RID: 6459 RVA: 0x0004F9F9 File Offset: 0x0004DBF9
		public ConversationBranch ConversationBranch
		{
			get
			{
				return this.conversationBranch;
			}
		}

		// Token: 0x17000552 RID: 1362
		// (get) Token: 0x0600193C RID: 6460
		// (set) Token: 0x0600193D RID: 6461
		public abstract CharacterPlotItemTriggerBase Trigger { get; set; }

		// Token: 0x17000553 RID: 1363
		// (get) Token: 0x0600193E RID: 6462
		// (set) Token: 0x0600193F RID: 6463
		public abstract CharacterPlotItemTriggerBase DeactivationTrigger { get; set; }

		// Token: 0x17000554 RID: 1364
		// (get) Token: 0x06001940 RID: 6464 RVA: 0x0004FA01 File Offset: 0x0004DC01
		// (set) Token: 0x06001941 RID: 6465 RVA: 0x0004FA09 File Offset: 0x0004DC09
		public int Priority
		{
			get
			{
				return this.priority;
			}
			set
			{
				this.priority = value;
			}
		}

		// Token: 0x17000555 RID: 1365
		// (get) Token: 0x06001942 RID: 6466 RVA: 0x0004FA12 File Offset: 0x0004DC12
		// (set) Token: 0x06001943 RID: 6467 RVA: 0x0004FA1A File Offset: 0x0004DC1A
		public CharacterPlotItemRuntimeData RuntimeData { get; set; }

		// Token: 0x04000E36 RID: 3638
		[HideInInspector]
		public ConversationBranch conversationBranch;

		// Token: 0x04000E37 RID: 3639
		[SerializeField]
		private int priority;
	}
}
