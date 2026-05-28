using System;
using Game.Core;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x020000FB RID: 251
	public abstract class HomespaceShopObjectSpawnerBase : GameBehaviourBase, IHomespaceShopObjectSpawner
	{
		// Token: 0x14000036 RID: 54
		// (add) Token: 0x0600060B RID: 1547
		// (remove) Token: 0x0600060C RID: 1548
		public abstract event Action PickableObjectSpawned;

		// Token: 0x170000EF RID: 239
		// (get) Token: 0x0600060D RID: 1549
		public abstract PickableObjectBase SpawnedPickable { get; }

		// Token: 0x170000F0 RID: 240
		// (get) Token: 0x0600060E RID: 1550
		public abstract Type PurchasableItemNodeType { get; }

		// Token: 0x170000F1 RID: 241
		// (get) Token: 0x0600060F RID: 1551
		// (set) Token: 0x06000610 RID: 1552
		public abstract object ID { get; set; }

		// Token: 0x170000F2 RID: 242
		// (get) Token: 0x06000611 RID: 1553 RVA: 0x00014CCA File Offset: 0x00012ECA
		public GameObject GameObject
		{
			get
			{
				return base.gameObject;
			}
		}

		// Token: 0x170000F3 RID: 243
		// (get) Token: 0x06000612 RID: 1554 RVA: 0x00014CD2 File Offset: 0x00012ED2
		public Transform Transform
		{
			get
			{
				return base.transform;
			}
		}

		// Token: 0x06000613 RID: 1555
		public abstract bool HasDefaultID();

		// Token: 0x06000614 RID: 1556
		public abstract MultiRepresentationObjectInstantiator.IArgs CreateFactoryQueryArgs();

		// Token: 0x06000615 RID: 1557
		public abstract void Spawn();

		// Token: 0x06000616 RID: 1558 RVA: 0x00014CDA File Offset: 0x00012EDA
		public void UpdateSpawnerName()
		{
			base.name = string.Format("{0}_spawner", this.ID);
		}
	}
}
