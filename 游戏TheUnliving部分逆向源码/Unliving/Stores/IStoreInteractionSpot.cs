using System;
using UnityEngine;
using Unliving.Pickables;

namespace Unliving.Stores
{
	// Token: 0x0200004B RID: 75
	public interface IStoreInteractionSpot : IPickableObject
	{
		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000256 RID: 598
		IStoreManager StoreManager { get; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000257 RID: 599
		Transform PivotTransform { get; }

		// Token: 0x06000258 RID: 600
		void OpenStore();
	}
}
