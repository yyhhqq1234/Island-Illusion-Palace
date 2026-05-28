using System;
using System.Linq;
using Common;
using Common.CollectionsExtensions;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.Pickables;

namespace Unliving.Mobs
{
	// Token: 0x020001EF RID: 495
	public class MobDropSpawner : GameBehaviourBase
	{
		// Token: 0x06001069 RID: 4201 RVA: 0x000334AC File Offset: 0x000316AC
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (this.dropObjectsCount < 0)
			{
				this.dropObjectsCount = 1;
			}
			else if (this.dropObjectsCount > 1)
			{
				this.DropObjects = (from d in this.DropObjects
				where d.ObjectPrefab != null
				select d).ToArray<DropObject>();
			}
			this.hitPointsController = base.GetComponent<IDamageable>();
			this.hitPointsController.TotallyDestroyed += this.OnMobTotallyDestroyed;
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x00033534 File Offset: 0x00031734
		private void OnMobTotallyDestroyed(IDamageable mob)
		{
			for (int i = 0; i < this.dropObjectsCount; i++)
			{
				Vector3 b = UnityEngine.Random.insideUnitCircle * 2f;
				DropObject dropObject;
				this.DropObjects.GetRandomWeightedItem(out dropObject, 0, int.MaxValue, null);
				if (dropObject.ObjectPrefab != null)
				{
					PickableBase componentInChildren = UnityEngine.Object.Instantiate<GameObject>(dropObject.ObjectPrefab, mob.Transform.position + b, mob.Transform.rotation).GetComponentInChildren<PickableBase>();
					IAmountBased amountBased = componentInChildren as IAmountBased;
					if (amountBased != null)
					{
						amountBased.Amount = UnityEngine.Random.Range(dropObject.minAmount, dropObject.maxAmount);
					}
					if (componentInChildren != null)
					{
						((IPickableObject)componentInChildren).OnDropped();
					}
				}
			}
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x000335F3 File Offset: 0x000317F3
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.hitPointsController != null)
			{
				this.hitPointsController.TotallyDestroyed -= this.OnMobTotallyDestroyed;
			}
		}

		// Token: 0x04000956 RID: 2390
		[Tooltip("Количество объектов, которое может выпасть с моба. Если -1 - может выпасть 1 объект или 0.")]
		public int dropObjectsCount = -1;

		// Token: 0x04000957 RID: 2391
		public DropObject[] DropObjects;

		// Token: 0x04000958 RID: 2392
		private IDamageable hitPointsController;
	}
}
