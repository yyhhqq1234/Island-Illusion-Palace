using System;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x0200013C RID: 316
	public interface IAimAssistMode
	{
		// Token: 0x17000140 RID: 320
		// (get) Token: 0x060007F7 RID: 2039
		Vector2 CurrentPointerPosition { get; }

		// Token: 0x17000141 RID: 321
		// (get) Token: 0x060007F8 RID: 2040
		AimAssistType AimAssistType { get; }

		// Token: 0x1400004A RID: 74
		// (add) Token: 0x060007F9 RID: 2041
		// (remove) Token: 0x060007FA RID: 2042
		event Action ModeStateUpdated;

		// Token: 0x060007FB RID: 2043
		void SetData(object data);

		// Token: 0x060007FC RID: 2044
		void OnUpdate();

		// Token: 0x060007FD RID: 2045
		void Destroy();
	}
}
