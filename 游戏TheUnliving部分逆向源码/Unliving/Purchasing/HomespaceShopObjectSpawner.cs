using System;
using Common.Factories;
using Common.UnityExtensions;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.Purchasing
{
	// Token: 0x020000FC RID: 252
	public abstract class HomespaceShopObjectSpawner<TObject> : HomespaceShopObjectSpawnerBase where TObject : class
	{
		// Token: 0x14000037 RID: 55
		// (add) Token: 0x06000618 RID: 1560 RVA: 0x00014CFC File Offset: 0x00012EFC
		// (remove) Token: 0x06000619 RID: 1561 RVA: 0x00014D34 File Offset: 0x00012F34
		public override event Action PickableObjectSpawned;

		// Token: 0x170000F4 RID: 244
		// (get) Token: 0x0600061A RID: 1562 RVA: 0x00014D69 File Offset: 0x00012F69
		public override PickableObjectBase SpawnedPickable
		{
			get
			{
				return this.spawnedPickable;
			}
		}

		// Token: 0x0600061B RID: 1563 RVA: 0x00014D71 File Offset: 0x00012F71
		private void Start()
		{
			this.Spawn();
		}

		// Token: 0x0600061C RID: 1564 RVA: 0x00014D7C File Offset: 0x00012F7C
		public override void Spawn()
		{
			IFactory factory = (IFactory)((object)base.CurrentGame.Services.Get<TObject>());
			if (factory == null)
			{
				return;
			}
			MultiRepresentationObjectInstantiator.IArgs query = this.CreateFactoryQueryArgs();
			object obj = factory.Create(query);
			this.spawnedPickable = obj.CastOrGetComponent<PickableObjectBase>();
			if (this.spawnedPickable == null || this.spawnedPickable.PurchasableData == null)
			{
				return;
			}
			this.spawnedPickable.PurchasableData.SetGameData(base.CurrentGame);
			this.spawnedPickable.transform.SetParent(base.transform);
			Action pickableObjectSpawned = this.PickableObjectSpawned;
			if (pickableObjectSpawned == null)
			{
				return;
			}
			pickableObjectSpawned();
		}

		// Token: 0x040003F4 RID: 1012
		private PickableObjectBase spawnedPickable;
	}
}
