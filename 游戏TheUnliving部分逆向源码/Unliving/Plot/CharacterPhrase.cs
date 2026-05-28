using System;
using UnityEngine;
using Unliving.Plot.Triggers;

namespace Unliving.Plot
{
	// Token: 0x020002D3 RID: 723
	[Serializable]
	public sealed class CharacterPhrase : CharacterPlotItemBase
	{
		// Token: 0x1700054C RID: 1356
		// (get) Token: 0x0600192F RID: 6447 RVA: 0x0004F9BB File Offset: 0x0004DBBB
		// (set) Token: 0x06001930 RID: 6448 RVA: 0x0004F9C3 File Offset: 0x0004DBC3
		public override string ID
		{
			get
			{
				return this.speakerID;
			}
			set
			{
				this.speakerID = value;
			}
		}

		// Token: 0x1700054D RID: 1357
		// (get) Token: 0x06001931 RID: 6449 RVA: 0x0004F9CC File Offset: 0x0004DBCC
		// (set) Token: 0x06001932 RID: 6450 RVA: 0x0004F9CF File Offset: 0x0004DBCF
		public override CharacterPlotItemTriggerBase Trigger
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x1700054E RID: 1358
		// (get) Token: 0x06001933 RID: 6451 RVA: 0x0004F9D1 File Offset: 0x0004DBD1
		// (set) Token: 0x06001934 RID: 6452 RVA: 0x0004F9D4 File Offset: 0x0004DBD4
		public override CharacterPlotItemTriggerBase DeactivationTrigger
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		// Token: 0x04000E2C RID: 3628
		[SerializeField]
		private string speakerID;

		// Token: 0x04000E2D RID: 3629
		public string speakerName;

		// Token: 0x04000E2E RID: 3630
		public string voiceoverEvent;

		// Token: 0x04000E2F RID: 3631
		[TextArea(5, 10)]
		public string text;
	}
}
