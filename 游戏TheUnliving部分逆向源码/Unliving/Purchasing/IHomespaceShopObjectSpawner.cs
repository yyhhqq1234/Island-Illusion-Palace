using System;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x020000FE RID: 254
	public interface IHomespaceShopObjectSpawner
	{
		// Token: 0x14000038 RID: 56
		// (add) Token: 0x06000625 RID: 1573
		// (remove) Token: 0x06000626 RID: 1574
		event Action PickableObjectSpawned;

		// Token: 0x170000F5 RID: 245
		// (get) Token: 0x06000627 RID: 1575
		PickableObjectBase SpawnedPickable { get; }

		// Token: 0x170000F6 RID: 246
		// (get) Token: 0x06000628 RID: 1576
		GameObject GameObject { get; }

		// Token: 0x170000F7 RID: 247
		// (get) Token: 0x06000629 RID: 1577
		Transform Transform { get; }

		// Token: 0x170000F8 RID: 248
		// (get) Token: 0x0600062A RID: 1578
		Type PurchasableItemNodeType { get; }

		// Token: 0x170000F9 RID: 249
		// (get) Token: 0x0600062B RID: 1579
		// (set) Token: 0x0600062C RID: 1580
		object ID { get; set; }

		// Token: 0x0600062D RID: 1581
		bool HasDefaultID();

		// Token: 0x0600062E RID: 1582
		void Spawn();

		// Token: 0x0600062F RID: 1583
		void UpdateSpawnerName();

		// Token: 0x06000630 RID: 1584
		MultiRepresentationObjectInstantiator.IArgs CreateFactoryQueryArgs();
	}
}
