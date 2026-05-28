using System;
using System.Collections;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.DropSystem
{
	// Token: 0x0200028A RID: 650
	public class DropSpawner : GameBehaviourBase
	{
		// Token: 0x170004D4 RID: 1236
		// (get) Token: 0x06001676 RID: 5750 RVA: 0x000482AE File Offset: 0x000464AE
		// (set) Token: 0x06001677 RID: 5751 RVA: 0x000482B6 File Offset: 0x000464B6
		public IPickableObject SpawnedPickable { get; private set; }

		// Token: 0x140000D3 RID: 211
		// (add) Token: 0x06001678 RID: 5752 RVA: 0x000482C0 File Offset: 0x000464C0
		// (remove) Token: 0x06001679 RID: 5753 RVA: 0x000482F8 File Offset: 0x000464F8
		public event Action<DropSpawner, IPickableObject> PickableSpawned;

		// Token: 0x140000D4 RID: 212
		// (add) Token: 0x0600167A RID: 5754 RVA: 0x00048330 File Offset: 0x00046530
		// (remove) Token: 0x0600167B RID: 5755 RVA: 0x00048368 File Offset: 0x00046568
		public event Action<DropSpawner, IPickableObject> PickablePickedUp;

		// Token: 0x140000D5 RID: 213
		// (add) Token: 0x0600167C RID: 5756 RVA: 0x000483A0 File Offset: 0x000465A0
		// (remove) Token: 0x0600167D RID: 5757 RVA: 0x000483D8 File Offset: 0x000465D8
		public event Action<DropSpawner> PickableDestroyed;

		// Token: 0x0600167E RID: 5758 RVA: 0x00048410 File Offset: 0x00046610
		public override async void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.dropItemsPool.Initialize(currentGame);
			await new WaitUntil(delegate()
			{
				this.dropGenerator = currentGame.Services.Get<DropGenerator>();
				return this.dropGenerator != null;
			});
			if (this.dropOnChunkCleared)
			{
				this.currentLocationChunk = base.GetComponentInParent<ILocationChunk>();
			}
			if (this.dropOnStart)
			{
				this.SpawnPickable(null);
			}
			else if (this.dropOnChunkCleared && !this.currentLocationChunk.IsNull())
			{
				this.dropGenerator.LocationChunkCleared += this.OnLocationChunkCleared;
			}
		}

		// Token: 0x0600167F RID: 5759 RVA: 0x00048454 File Offset: 0x00046654
		public void DestroySpawnedPickable()
		{
			if (this.SpawnedPickable == null)
			{
				return;
			}
			NavMeshObstacle navMeshObstacle;
			if (this.SpawnedPickable.Component.TryGetComponent<NavMeshObstacle>(out navMeshObstacle))
			{
				navMeshObstacle.carving = false;
			}
			UnityEngine.Object.Destroy(this.SpawnedPickable.Component.gameObject);
			this.SpawnedPickable = null;
			Action<DropSpawner> pickableDestroyed = this.PickableDestroyed;
			if (pickableDestroyed != null)
			{
				pickableDestroyed(this);
			}
			UnityEvent pickableDestroyedEvents = this.PickableDestroyedEvents;
			if (pickableDestroyedEvents == null)
			{
				return;
			}
			pickableDestroyedEvents.Invoke();
		}

		// Token: 0x06001680 RID: 5760 RVA: 0x000484C3 File Offset: 0x000466C3
		public void RespawnPickable()
		{
			base.StartCoroutine(this.RespawnRoutine());
		}

		// Token: 0x06001681 RID: 5761 RVA: 0x000484D2 File Offset: 0x000466D2
		private IEnumerator RespawnRoutine()
		{
			if (this.SpawnedPickable.IsNull())
			{
				yield break;
			}
			this.DestroySpawnedPickable();
			yield return null;
			yield return null;
			this.SpawnPickable(null);
			yield break;
		}

		// Token: 0x06001682 RID: 5762 RVA: 0x000484E1 File Offset: 0x000466E1
		private void OnPickablePickedUp(IPickableObject obj, IPickableObjectCollector collector)
		{
			Action<DropSpawner, IPickableObject> pickablePickedUp = this.PickablePickedUp;
			if (pickablePickedUp == null)
			{
				return;
			}
			pickablePickedUp(this, this.SpawnedPickable);
		}

		// Token: 0x06001683 RID: 5763 RVA: 0x000484FA File Offset: 0x000466FA
		private void OnLocationChunkCleared(ILocationChunk locationChunk)
		{
			if (locationChunk.IsNull() || !locationChunk.Equals(this.currentLocationChunk))
			{
				return;
			}
			this.SpawnPickable(null);
			this.dropGenerator.LocationChunkCleared -= this.OnLocationChunkCleared;
		}

		// Token: 0x06001684 RID: 5764 RVA: 0x00048531 File Offset: 0x00046731
		protected virtual bool SpawnCurrentContextObject(ref IDropable dropable, Transform parentTransform)
		{
			return this.dropGenerator.SpawnWorldObjectItem(ref dropable, base.transform);
		}

		// Token: 0x06001685 RID: 5765 RVA: 0x00048545 File Offset: 0x00046745
		private bool TryCreatePickableObject(IList<object> exceptItems, out IDropable dropable, MultiRepresentationObjectInstantiator.IArgs argsOverride = null)
		{
			dropable = this.dropItemsPool.GetRandomItem(exceptItems);
			if (dropable == null)
			{
				return false;
			}
			if (argsOverride != null)
			{
				dropable.SetQueryOverride(argsOverride);
			}
			return this.SpawnCurrentContextObject(ref dropable, base.transform);
		}

		// Token: 0x06001686 RID: 5766 RVA: 0x00048578 File Offset: 0x00046778
		private bool TryCreatePickableObject(object objectID, out IDropable dropable, MultiRepresentationObjectInstantiator.IArgs argsOverride = null)
		{
			if (this.dropItemsPool.TryGetItem(objectID, out dropable))
			{
				if (argsOverride != null)
				{
					dropable.SetQueryOverride(argsOverride);
				}
				return this.SpawnCurrentContextObject(ref dropable, base.transform);
			}
			return false;
		}

		// Token: 0x06001687 RID: 5767 RVA: 0x000485A3 File Offset: 0x000467A3
		public void SpawnPickable(object objectID, out IPickableObject pickable, out IDropable dropable, MultiRepresentationObjectInstantiator.IArgs argsOverride = null)
		{
			if (this.TryCreatePickableObject(objectID, out dropable, argsOverride))
			{
				this.RegisterSpawnedObject(dropable.ObjectInstance, dropable, out pickable);
				return;
			}
			pickable = null;
		}

		// Token: 0x06001688 RID: 5768 RVA: 0x000485C5 File Offset: 0x000467C5
		public void SpawnPickable()
		{
			this.SpawnPickable(null);
		}

		// Token: 0x06001689 RID: 5769 RVA: 0x000485D0 File Offset: 0x000467D0
		public void SpawnPickable(IList<object> exceptItems)
		{
			IPickableObject pickableObject;
			IDropable dropable;
			this.SpawnPickable(exceptItems, out pickableObject, out dropable, null);
		}

		// Token: 0x0600168A RID: 5770 RVA: 0x000485E9 File Offset: 0x000467E9
		public void SpawnPickable(IList<object> exceptItems, out IPickableObject pickable, out IDropable dropable, MultiRepresentationObjectInstantiator.IArgs argsOverride = null)
		{
			pickable = null;
			dropable = null;
			if (!this.SpawnedPickable.IsNull())
			{
				pickable = this.SpawnedPickable;
				dropable = this.dropableData;
				return;
			}
			this.SpawnPickableRecursive(exceptItems, out pickable, out dropable, argsOverride);
		}

		// Token: 0x0600168B RID: 5771 RVA: 0x0004861C File Offset: 0x0004681C
		private void SpawnPickableRecursive(IList<object> exceptItems, out IPickableObject pickable, out IDropable dropable, MultiRepresentationObjectInstantiator.IArgs argsOverride = null)
		{
			pickable = null;
			this.dropItemsPool.ResetPoolItems();
			if (this.TryCreatePickableObject(exceptItems, out dropable, argsOverride))
			{
				GameObject objectInstance = dropable.ObjectInstance;
				if (objectInstance.IsNull())
				{
					return;
				}
				DropSpawner dropSpawner = null;
				this.dropItemsPool.RemovePoolItem(dropable);
				while (objectInstance.IsNull() || objectInstance.TryGetComponent<DropSpawner>(out dropSpawner))
				{
					if (dropSpawner.TryCreatePickableObject(exceptItems, out dropable, argsOverride))
					{
						dropSpawner.dropItemsPool.RemovePoolItem(dropable);
						objectInstance = dropable.ObjectInstance;
					}
					else
					{
						if (!this.TryCreatePickableObject(exceptItems, out dropable, null))
						{
							return;
						}
						objectInstance = dropable.ObjectInstance;
						this.dropItemsPool.RemovePoolItem(dropable);
					}
				}
				this.dropableData = dropable;
				this.RegisterSpawnedObject(objectInstance, dropable, out pickable);
			}
		}

		// Token: 0x0600168C RID: 5772 RVA: 0x000486D4 File Offset: 0x000468D4
		private void RegisterSpawnedObject(GameObject spawnedObject, IDropable dropable, out IPickableObject pickable)
		{
			if (spawnedObject.IsNull())
			{
				pickable = null;
				return;
			}
			pickable = spawnedObject.GetComponentInChildren<IPickableObject>();
			if (pickable != null)
			{
				if (this.useDebugName)
				{
					string name = "prefab";
					if (dropable is AbilityDropable)
					{
						name = ((AbilityDropable)dropable).abilityID.ToString();
					}
					else if (dropable is AbilityActivatedContainerDropable)
					{
						name = ((AbilityActivatedContainerDropable)dropable).abilityID.ToString();
					}
					else if (dropable is ActivationModifierDropable)
					{
						name = ((ActivationModifierDropable)dropable).modifierID.ToString();
					}
					PickableObjectDebugName componentInChildren = spawnedObject.GetComponentInChildren<PickableObjectDebugName>();
					if (componentInChildren != null)
					{
						componentInChildren.SetText(name, true);
					}
				}
				this.SpawnedPickable = pickable;
				this.SpawnedPickable.PickedUp += this.OnPickablePickedUp;
				Action<DropSpawner, IPickableObject> pickableSpawned = this.PickableSpawned;
				if (pickableSpawned == null)
				{
					return;
				}
				pickableSpawned(this, this.SpawnedPickable);
			}
		}

		// Token: 0x0600168D RID: 5773 RVA: 0x000487B9 File Offset: 0x000469B9
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.SpawnedPickable.IsNull())
			{
				this.SpawnedPickable.PickedUp -= this.OnPickablePickedUp;
			}
		}

		// Token: 0x04000D0F RID: 3343
		public UnityEvent PickableDestroyedEvents;

		// Token: 0x04000D13 RID: 3347
		public bool useDebugName = true;

		// Token: 0x04000D14 RID: 3348
		public bool dropOnStart;

		// Token: 0x04000D15 RID: 3349
		public bool dropOnChunkCleared = true;

		// Token: 0x04000D16 RID: 3350
		[FormerlySerializedAs("abilitiesPool")]
		public DropItemsPool dropItemsPool;

		// Token: 0x04000D17 RID: 3351
		protected DropGenerator dropGenerator;

		// Token: 0x04000D18 RID: 3352
		private ILocationChunk currentLocationChunk;

		// Token: 0x04000D19 RID: 3353
		private IDropable dropableData;
	}
}
