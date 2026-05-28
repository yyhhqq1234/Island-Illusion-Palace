using System;

namespace Unliving
{
	// Token: 0x02000013 RID: 19
	public enum SessionState
	{
		// Token: 0x0400006E RID: 110
		Undefined = -1,
		// Token: 0x0400006F RID: 111
		InProgress,
		// Token: 0x04000070 RID: 112
		Paused,
		// Token: 0x04000071 RID: 113
		Interrupted,
		// Token: 0x04000072 RID: 114
		Victory,
		// Token: 0x04000073 RID: 115
		Defeat,
		// Token: 0x04000074 RID: 116
		Freezed,
		// Token: 0x04000075 RID: 117
		Cutscene,
		// Token: 0x04000076 RID: 118
		VictoryCutscene,
		// Token: 0x04000077 RID: 119
		Exited
	}
}
