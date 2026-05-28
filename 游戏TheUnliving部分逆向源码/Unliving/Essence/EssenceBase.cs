using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using Unliving.Currencies;
using Unliving.DropSystem;
using Unliving.Factories;
using Unliving.Interactables;
using Unliving.Pickables;
using Unliving.Plot;
using Unliving.Purchasing;

namespace Unliving.Essence
{
	// Token: 0x020002C8 RID: 712
	public abstract class EssenceBase : NPCControllerBase<EssenceType>
	{
		// Token: 0x17000544 RID: 1348
		// (get) Token: 0x060018D3 RID: 6355 RVA: 0x0004E73F File Offset: 0x0004C93F
		public IReadOnlyList<IPickableObject> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x17000545 RID: 1349
		// (get) Token: 0x060018D4 RID: 6356 RVA: 0x0004E747 File Offset: 0x0004C947
		public override EssenceType ID
		{
			get
			{
				return this.essenceType;
			}
		}

		// Token: 0x17000546 RID: 1350
		// (get) Token: 0x060018D5 RID: 6357 RVA: 0x0004E74F File Offset: 0x0004C94F
		protected override string LocalizationID
		{
			get
			{
				return "essenceID_" + this.essenceType.ToString();
			}
		}

		// Token: 0x140000F4 RID: 244
		// (add) Token: 0x060018D6 RID: 6358 RVA: 0x0004E76C File Offset: 0x0004C96C
		// (remove) Token: 0x060018D7 RID: 6359 RVA: 0x0004E7A4 File Offset: 0x0004C9A4
		public virtual event Action EmptyEssenceSpawned;

		// Token: 0x060018D8 RID: 6360 RVA: 0x0004E7D9 File Offset: 0x0004C9D9
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager);
		}

		// Token: 0x060018D9 RID: 6361 RVA: 0x0004E7F4 File Offset: 0x0004C9F4
		public void OnItemSelected(IPickableObject selectedItem)
		{
			this.Disable();
		}

		// Token: 0x060018DA RID: 6362 RVA: 0x0004E7FC File Offset: 0x0004C9FC
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject && !base.PurchasableData.TryPurchase(this.CurrentPickingContext))
			{
				return;
			}
			if (base.StartConversation())
			{
				this.gameSessionManager.SetSessionState(SessionState.Freezed);
			}
		}

		// Token: 0x060018DB RID: 6363 RVA: 0x0004E830 File Offset: 0x0004CA30
		protected override void OnConversationCompleted(IPlotCharacter character, ICharactersConversation conversation)
		{
			if (base.CurrentConversation != conversation)
			{
				return;
			}
			this.SpawnItems(null);
			if (!this.IsSpawnedItemsValid())
			{
				Action emptyEssenceSpawned = this.EmptyEssenceSpawned;
				if (emptyEssenceSpawned != null)
				{
					emptyEssenceSpawned();
				}
				if (this.gameSessionManager.CurrentSessionState == SessionState.Freezed)
				{
					this.Disable();
				}
				return;
			}
			if (this.gameSessionManager.CurrentSessionState == SessionState.InProgress)
			{
				this.gameSessionManager.SetSessionState(SessionState.Freezed);
			}
			base.OnConversationCompleted(character, conversation);
		}

		// Token: 0x060018DC RID: 6364
		protected abstract void SpawnItems(List<object> itemsExceptionListOverride = null);

		// Token: 0x060018DD RID: 6365 RVA: 0x0004E89E File Offset: 0x0004CA9E
		protected bool IsSpawnedItemsValid()
		{
			return this.dropSpawners != null && this.dropSpawners.Length != 0 && this.items.Count > 0;
		}

		// Token: 0x060018DE RID: 6366 RVA: 0x0004E8C4 File Offset: 0x0004CAC4
		public virtual CurrencyOperationArgs GetCloseRewardArgs()
		{
			PurchasableEssence purchasableEssence = base.PurchasableData as PurchasableEssence;
			if (purchasableEssence != null)
			{
				CurrencyOperationArgs closeRewardArgs = purchasableEssence.closeRewardArgs;
				closeRewardArgs.sender = base.gameObject;
				return closeRewardArgs;
			}
			return default(CurrencyOperationArgs);
		}

		// Token: 0x060018DF RID: 6367 RVA: 0x0004E900 File Offset: 0x0004CB00
		protected void DestroySpawnedItems()
		{
			for (int i = 0; i < this.dropSpawners.Length; i++)
			{
				this.dropSpawners[i].DestroySpawnedPickable();
			}
			this.items.Clear();
		}

		// Token: 0x060018E0 RID: 6368 RVA: 0x0004E938 File Offset: 0x0004CB38
		protected void Disable()
		{
			this.DestroySpawnedItems();
			UnityEngine.Object.Destroy(base.gameObject);
			this.gameSessionManager.SetSessionState(SessionState.InProgress);
		}

		// Token: 0x04000DF2 RID: 3570
		public DropSpawner[] dropSpawners;

		// Token: 0x04000DF3 RID: 3571
		[HideInInspector]
		public bool hideCloseButton;

		// Token: 0x04000DF4 RID: 3572
		[HideInInspector]
		public EssenceType essenceType;

		// Token: 0x04000DF5 RID: 3573
		[HideInInspector]
		public EssenceFactoryBuilder.LocationLevelData[] locationsLevelsData;

		// Token: 0x04000DF6 RID: 3574
		[HideInInspector]
		public EssenceFactoryBuilder.LevelGenerationSettings levelGenerationSettings;

		// Token: 0x04000DF7 RID: 3575
		protected List<IPickableObject> items = new List<IPickableObject>();

		// Token: 0x04000DF8 RID: 3576
		private GameSessionManager gameSessionManager;
	}
}
