using System;
using UnityEngine;

namespace Unliving.Plot.Milestones
{
	// Token: 0x0200030F RID: 783
	public abstract class MilestoneDataBase
	{
		// Token: 0x04000EAF RID: 3759
		public string milestoneID;

		// Token: 0x04000EB0 RID: 3760
		[Tooltip("One-Time событие вызывается только в момент достижения майлстоуна")]
		public bool oneTimeActivation;

		// Token: 0x04000EB1 RID: 3761
		[HideInInspector]
		public bool alreadyActivated;

		// Token: 0x04000EB2 RID: 3762
		public Action activationEvent;
	}
}
