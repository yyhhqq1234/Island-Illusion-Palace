using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

namespace Unliving.GameScene
{
	// Token: 0x0200025F RID: 607
	public sealed class MainSceneLight : MonoBehaviour
	{
		// Token: 0x1700044C RID: 1100
		// (get) Token: 0x06001428 RID: 5160 RVA: 0x0003F6B3 File Offset: 0x0003D8B3
		// (set) Token: 0x06001429 RID: 5161 RVA: 0x0003F6BA File Offset: 0x0003D8BA
		public static Light2D Instance { get; private set; }

		// Token: 0x0600142A RID: 5162 RVA: 0x0003F6C2 File Offset: 0x0003D8C2
		private void Awake()
		{
			MainSceneLight.Instance = base.GetComponent<Light2D>();
		}
	}
}
