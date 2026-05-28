using System;
using Common;
using Common.Factories;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Factories;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Purchasing;

namespace Unliving.DropSystem
{
	// Token: 0x02000287 RID: 647
	[CreateAssetMenu(fileName = "DropGenerator", menuName = "Game/Global/Drop Generator")]
	public class DropGenerator : GlobalManagerBase
	{
		// Token: 0x170004CE RID: 1230
		// (get) Token: 0x06001651 RID: 5713 RVA: 0x00047819 File Offset: 0x00045A19
		public bool DebugLogEnabled
		{
			get
			{
				return GameApplicationSettings.IsDebugBuild && this.debugLogEnabled;
			}
		}

		// Token: 0x140000D2 RID: 210
		// (add) Token: 0x06001652 RID: 5714 RVA: 0x0004782C File Offset: 0x00045A2C
		// (remove) Token: 0x06001653 RID: 5715 RVA: 0x00047864 File Offset: 0x00045A64
		public event Action<ILocationChunk> LocationChunkCleared;

		// Token: 0x06001654 RID: 5716 RVA: 0x0004789C File Offset: 0x00045A9C
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			base.OnSceneLoaded(loadedScene);
			base.CurrentGame.Services.TryGet<PurchaseManager>(out this.purchaseManager);
			GameSceneManager gameSceneManager;
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
		}

		// Token: 0x06001655 RID: 5717 RVA: 0x000478F0 File Offset: 0x00045AF0
		private void OnLocationGenerated(GameSceneManager gameSceneManager)
		{
			this.currentLocation = gameSceneManager.GeneratedLocation;
			for (int i = 0; i < this.locationsDropData.Length; i++)
			{
				LocationDropData locationDropData = this.locationsDropData[i];
				if (locationDropData.locationType == this.currentLocation.Type)
				{
					this.currentLocationDropData = locationDropData;
					break;
				}
			}
			LocationDropData locationDropData2 = this.currentLocationDropData;
			if (locationDropData2 == null)
			{
				return;
			}
			locationDropData2.Initialize(this.DebugLogEnabled);
		}

		// Token: 0x06001656 RID: 5718 RVA: 0x00047958 File Offset: 0x00045B58
		public bool SpawnRandomItem(RandomDropItemType allowedTypes, Transform spawnerTransform)
		{
			IDropable dropable;
			return this.currentLocationDropData != null && this.currentLocationDropData.TryGetNextDropable(allowedTypes, out dropable) && this.SpawnWorldObjectItem(ref dropable, spawnerTransform);
		}

		// Token: 0x06001657 RID: 5719 RVA: 0x00047988 File Offset: 0x00045B88
		public bool SpawnIngameStoreItem(IDropable dropable, Transform parentTransform)
		{
			if (dropable.IsNull())
			{
				return false;
			}
			AnotherItemDropable anotherItemDropable = dropable as AnotherItemDropable;
			if (anotherItemDropable != null)
			{
				if (anotherItemDropable.prefab.IsNull())
				{
					return false;
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(anotherItemDropable.prefab, parentTransform);
				gameObject.transform.localPosition = Vector3.zero;
				gameObject = gameObject.gameObject;
				IPickingContextProvider pickingContextProvider;
				if (gameObject.TryGetComponent<IPickingContextProvider>(out pickingContextProvider))
				{
					pickingContextProvider.CurrentPickingContext = MultiRepresentationObjectInstantiator.ObjectType.StoreObject;
				}
				IAmountBased amountBased;
				if (anotherItemDropable.minAmount > 0 && anotherItemDropable.maxAmount > 0 && gameObject.TryGetComponent<IAmountBased>(out amountBased))
				{
					amountBased.Amount = (float)UnityEngine.Random.Range(anotherItemDropable.minAmount, anotherItemDropable.maxAmount);
				}
				dropable.OnObjectSpawned(gameObject);
				return true;
			}
			else
			{
				MultiRepresentationObjectInstantiator.IArgs args = dropable.CreateQuery();
				args.Type = MultiRepresentationObjectInstantiator.ObjectType.StoreObject;
				GameObject spawnedObject;
				if (this.SpawnItem(dropable, parentTransform, args, out spawnedObject))
				{
					dropable.OnObjectSpawned(spawnedObject);
					return true;
				}
				return false;
			}
		}

		// Token: 0x06001658 RID: 5720 RVA: 0x00047A54 File Offset: 0x00045C54
		public bool SpawnWorldObjectItem(ref IDropable dropable, Transform parentTransform)
		{
			if (dropable.IsNull())
			{
				return false;
			}
			MultiRepresentationObjectInstantiator.IArgs args = dropable.CreateQuery();
			args.Type = MultiRepresentationObjectInstantiator.ObjectType.PickableObject;
			GameObject spawnedObject;
			if (this.SpawnItem(dropable, parentTransform, args, out spawnedObject))
			{
				dropable.OnObjectSpawned(spawnedObject);
				return true;
			}
			return false;
		}

		// Token: 0x06001659 RID: 5721 RVA: 0x00047A94 File Offset: 0x00045C94
		private bool SpawnItem(IDropable dropable, Transform parentTransform, IBaseObjectDescription query, out GameObject objectInstance)
		{
			objectInstance = null;
			return !dropable.IsNull() && this.Spawn(dropable, parentTransform, query, out objectInstance);
		}

		// Token: 0x0600165A RID: 5722 RVA: 0x00047AB4 File Offset: 0x00045CB4
		private bool Spawn(IDropable dropable, Transform parentTransform, IBaseObjectDescription query, out GameObject spawnedObject)
		{
			spawnedObject = null;
			AnotherItemDropable anotherItemDropable = dropable as AnotherItemDropable;
			if (anotherItemDropable != null)
			{
				if (!anotherItemDropable.prefab.IsNull())
				{
					spawnedObject = UnityEngine.Object.Instantiate<GameObject>(anotherItemDropable.prefab);
					IAmountBased component = spawnedObject.GetComponent<IAmountBased>();
					if (anotherItemDropable.minAmount > 0 && anotherItemDropable.maxAmount > 0 && component != null)
					{
						component.Amount = (float)UnityEngine.Random.Range(anotherItemDropable.minAmount, anotherItemDropable.maxAmount);
					}
				}
			}
			else if (query != null)
			{
				object obj = ((IFactory)base.CurrentGame.Services.Get(dropable.FactoryType)).Create(query);
				Component component2 = obj as Component;
				if (component2 != null)
				{
					spawnedObject = component2.gameObject;
				}
				else
				{
					spawnedObject = (obj as GameObject);
				}
			}
			if (!spawnedObject.IsNull())
			{
				spawnedObject.transform.SetParent(parentTransform);
				spawnedObject.transform.localPosition = Vector3.zero;
				spawnedObject.transform.localRotation = Quaternion.identity;
				return true;
			}
			return false;
		}

		// Token: 0x04000CEF RID: 3311
		public bool debugLogEnabled;

		// Token: 0x04000CF0 RID: 3312
		public LocationDropData[] locationsDropData;

		// Token: 0x04000CF1 RID: 3313
		private GameLocation currentLocation;

		// Token: 0x04000CF2 RID: 3314
		private LocationDropData currentLocationDropData;

		// Token: 0x04000CF3 RID: 3315
		private PurchaseManager purchaseManager;
	}
}
