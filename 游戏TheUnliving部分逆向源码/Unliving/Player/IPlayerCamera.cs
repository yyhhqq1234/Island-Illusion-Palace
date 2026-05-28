using System;
using Cinemachine;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x02000146 RID: 326
	public interface IPlayerCamera
	{
		// Token: 0x17000165 RID: 357
		// (get) Token: 0x0600087E RID: 2174
		PlayerBehaviour Player { get; }

		// Token: 0x17000166 RID: 358
		// (get) Token: 0x0600087F RID: 2175
		Camera CameraComponent { get; }

		// Token: 0x17000167 RID: 359
		// (get) Token: 0x06000880 RID: 2176
		CinemachineBrain CinemachineBrain { get; }

		// Token: 0x06000881 RID: 2177
		void SetCameraTargetOverride(Transform target);

		// Token: 0x06000882 RID: 2178
		void ResetCameraTargetOverride();
	}
}
