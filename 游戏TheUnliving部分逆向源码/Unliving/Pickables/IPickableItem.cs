using System;
using UnityEngine;

namespace Unliving.Pickables
{
	// Token: 0x0200018E RID: 398
	public interface IPickableItem
	{
		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06000B37 RID: 2871
		// (set) Token: 0x06000B38 RID: 2872
		Sprite PickableSprite { get; set; }

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06000B39 RID: 2873
		// (set) Token: 0x06000B3A RID: 2874
		Sprite InventorySprite { get; set; }
	}
}
