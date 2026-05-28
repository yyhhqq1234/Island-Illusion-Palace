using System;
using Common;
using Game.Localization;

namespace Unliving.Player
{
	// Token: 0x02000139 RID: 313
	[LocalizationObject(LocalizationPrefix.aimAssistType_)]
	public enum AimAssistType
	{
		// Token: 0x0400048C RID: 1164
		Disabled,
		// Token: 0x0400048D RID: 1165
		[EnumClass(typeof(AutoTargetMode))]
		AutoTarget,
		// Token: 0x0400048E RID: 1166
		[EnumClass(typeof(AimAssistMode))]
		AimAssist
	}
}
