using System;
using Common;
using UnityEngine;

namespace Unliving.Scripting
{
	// Token: 0x02000075 RID: 117
	public sealed class DestructionEventComponent : MonoBehaviour, IDestroyable
	{
		// Token: 0x1400002C RID: 44
		// (add) Token: 0x06000340 RID: 832 RVA: 0x0000BD2C File Offset: 0x00009F2C
		// (remove) Token: 0x06000341 RID: 833 RVA: 0x0000BD64 File Offset: 0x00009F64
		public event Action<object> Destroyed;

		// Token: 0x1700009A RID: 154
		// (get) Token: 0x06000342 RID: 834 RVA: 0x0000BD99 File Offset: 0x00009F99
		// (set) Token: 0x06000343 RID: 835 RVA: 0x0000BDA1 File Offset: 0x00009FA1
		public bool IsDestroyed { get; private set; }

		// Token: 0x06000344 RID: 836 RVA: 0x0000BDAA File Offset: 0x00009FAA
		public void Destroy()
		{
			this.IsDestroyed = true;
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06000345 RID: 837 RVA: 0x0000BDBE File Offset: 0x00009FBE
		private void OnDestroy()
		{
			this.IsDestroyed = true;
			Action<object> destroyed = this.Destroyed;
			if (destroyed == null)
			{
				return;
			}
			destroyed(this);
		}
	}
}
