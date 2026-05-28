using System;
using Common;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001EE RID: 494
	[Serializable]
	public class DropObject : IWeighted
	{
		// Token: 0x17000356 RID: 854
		// (get) Token: 0x06001066 RID: 4198 RVA: 0x0003348D File Offset: 0x0003168D
		// (set) Token: 0x06001067 RID: 4199 RVA: 0x00033495 File Offset: 0x00031695
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = Mathf.Clamp01(value);
			}
		}

		// Token: 0x04000952 RID: 2386
		public GameObject ObjectPrefab;

		// Token: 0x04000953 RID: 2387
		public float minAmount;

		// Token: 0x04000954 RID: 2388
		public float maxAmount;

		// Token: 0x04000955 RID: 2389
		[Range(0f, 1f)]
		public float weight;
	}
}
