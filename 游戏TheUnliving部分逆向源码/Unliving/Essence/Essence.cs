using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.DropSystem;
using Unliving.GameScene;
using Unliving.LeveledItems;
using Unliving.LevelGeneration;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Pickables;
using Unliving.Player;

namespace Unliving.Essence
{
	// Token: 0x020002C7 RID: 711
	public class Essence : EssenceBase
	{
		// Token: 0x060018CD RID: 6349 RVA: 0x0004E2C1 File Offset: 0x0004C4C1
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<IGameLocationProvider>(out this.locationProvider);
		}

		// Token: 0x060018CE RID: 6350 RVA: 0x0004E2DC File Offset: 0x0004C4DC
		public void Reroll()
		{
			this.exceptionListAlreadyCleared = false;
			List<object> itemsExceptionListOverride = (from i in this.items
			select (i as PickableObjectBase).PurchasableData.ObjectID).ToList<object>();
			base.DestroySpawnedItems();
			this.SpawnItems(itemsExceptionListOverride);
		}

		// Token: 0x060018CF RID: 6351 RVA: 0x0004E330 File Offset: 0x0004C530
		protected override void SpawnItems(List<object> itemsExceptionListOverride = null)
		{
			this.ClearExceptionList();
			if (itemsExceptionListOverride != null)
			{
				this.itemsExceptionList.AddRange(itemsExceptionListOverride);
			}
			int currentLocationItemsLevel = this.GetCurrentLocationItemsLevel();
			List<Essence.EssenceSlotData> list = new List<Essence.EssenceSlotData>();
			for (int i = 0; i < this.dropSpawners.Length; i++)
			{
				IPickableObject pickableObject;
				IDropable dropable;
				this.dropSpawners[i].SpawnPickable(this.itemsExceptionList, out pickableObject, out dropable, null);
				if (!pickableObject.IsNull())
				{
					PickableObjectBase pickableObjectBase = pickableObject as PickableObjectBase;
					if (pickableObjectBase != null)
					{
						bool slotBusy;
						bool hasSameItem;
						pickableObjectBase.CalculateItemLevel(out slotBusy, out hasSameItem);
						Essence.EssenceSlotData item = new Essence.EssenceSlotData
						{
							pickable = pickableObjectBase,
							hasSameItem = hasSameItem,
							slotBusy = slotBusy
						};
						list.Add(item);
						pickableObjectBase.gameObject.SetActive(false);
						this.itemsExceptionList.Add(dropable.ObjectID);
					}
				}
			}
			for (int j = 0; j < list.Count; j++)
			{
				Essence.EssenceSlotData essenceSlotData = list[j];
				int num = essenceSlotData.ItemLevel;
				if (essenceSlotData.slotBusy && essenceSlotData.ItemLevel >= currentLocationItemsLevel)
				{
					if (essenceSlotData.hasSameItem)
					{
						int num2 = (UnityEngine.Random.Range(0f, 1f) <= this.levelGenerationSettings.sameRandomChance) ? this.levelGenerationSettings.sameRandomBonus : 0;
						num += this.levelGenerationSettings.sameBase + num2;
					}
					else
					{
						num += ((UnityEngine.Random.Range(0f, 1f) <= this.levelGenerationSettings.equalLevelRandomChance) ? this.levelGenerationSettings.equalLevelRandomBonus : 0);
					}
				}
				else
				{
					num = currentLocationItemsLevel;
				}
				essenceSlotData.SetLevel(num);
			}
			for (int k = 0; k < list.Count; k++)
			{
				Essence.EssenceSlotData essenceSlotData2 = list[k];
				if (this.extraLevelBonus > 0)
				{
					essenceSlotData2.SetLevel(essenceSlotData2.ItemLevel + this.extraLevelBonus);
				}
				if (essenceSlotData2.levelBonus > 0 && essenceSlotData2.slotBusy && !essenceSlotData2.hasSameItem)
				{
					essenceSlotData2.SetLevel(essenceSlotData2.ItemLevel + this.levelGenerationSettings.noNoveltyBonus);
				}
				int maxItemLevel = this.levelGenerationSettings.maxItemLevel;
				if (essenceSlotData2.ItemLevel > maxItemLevel)
				{
					essenceSlotData2.SetLevel(maxItemLevel);
				}
			}
			bool flag = this.itemsExceptionList.Count == 0;
			if (list.Count == 0 && flag)
			{
				base.Disable();
				return;
			}
			if (list.Count != this.dropSpawners.Length && !flag && !this.exceptionListAlreadyCleared)
			{
				this.exceptionListAlreadyCleared = true;
				base.DestroySpawnedItems();
				this.SpawnItems(null);
				return;
			}
			this.items = (from s in list
			select s.pickable).ToList<IPickableObject>();
		}

		// Token: 0x060018D0 RID: 6352 RVA: 0x0004E5E0 File Offset: 0x0004C7E0
		protected int GetCurrentLocationItemsLevel()
		{
			if (this.locationProvider != null)
			{
				GameLocation.TypeID type = this.locationProvider.CurrentLocation.Type;
				for (int i = 0; i < this.locationsLevelsData.Length; i++)
				{
					EssenceFactoryBuilder.LocationLevelData locationLevelData = this.locationsLevelsData[i];
					if (locationLevelData.location == type)
					{
						return locationLevelData.minLevel;
					}
				}
			}
			return 1;
		}

		// Token: 0x060018D1 RID: 6353 RVA: 0x0004E638 File Offset: 0x0004C838
		private void ClearExceptionList()
		{
			this.itemsExceptionList.Clear();
			int maxItemLevel = this.levelGenerationSettings.maxItemLevel;
			PlayerBehaviour playerBehaviour;
			if (base.CurrentGame.TryGetPlayer(out playerBehaviour))
			{
				PlayerAbilitiesController playerAbilitiesController = playerBehaviour.AbilitiesController as PlayerAbilitiesController;
				if (playerAbilitiesController != null)
				{
					int nonNativeAbilitiesStartSlotIndex = playerAbilitiesController.NonNativeAbilitiesStartSlotIndex;
					int nonNativeAbilitiesCount = playerAbilitiesController.NonNativeAbilitiesCount;
					for (int i = 0; i < nonNativeAbilitiesCount; i++)
					{
						BaseAbility ability = playerAbilitiesController.GetAbility(nonNativeAbilitiesStartSlotIndex + i);
						ILeveledItem leveledItem = ability as ILeveledItem;
						if (leveledItem != null && leveledItem.ItemLevel >= maxItemLevel)
						{
							this.itemsExceptionList.Add((AbilityID)ability.ID);
						}
					}
				}
				foreach (MobsActivationModifiersController.Slot slot in playerBehaviour.ActivationModifiersController.Slots)
				{
					if (slot.CurrentModifierID != MobActivationModifierID.None && slot.CurrentModifierLevel >= maxItemLevel)
					{
						this.itemsExceptionList.Add(slot.CurrentModifierID);
					}
				}
			}
		}

		// Token: 0x04000DED RID: 3565
		public int extraLevelBonus;

		// Token: 0x04000DEE RID: 3566
		private IGameLocationProvider locationProvider;

		// Token: 0x04000DEF RID: 3567
		private readonly List<object> itemsExceptionList = new List<object>();

		// Token: 0x04000DF0 RID: 3568
		private bool exceptionListAlreadyCleared;

		// Token: 0x02000531 RID: 1329
		public class EssenceSlotData
		{
			// Token: 0x170007CE RID: 1998
			// (get) Token: 0x06002674 RID: 9844 RVA: 0x0007824C File Offset: 0x0007644C
			public int ItemLevel
			{
				get
				{
					return this.LeveledItem.ItemLevel;
				}
			}

			// Token: 0x170007CF RID: 1999
			// (get) Token: 0x06002675 RID: 9845 RVA: 0x00078259 File Offset: 0x00076459
			private ILeveledItem LeveledItem
			{
				get
				{
					if (this.leveledItem == null)
					{
						this.leveledItem = (this.pickable as ILeveledItem);
					}
					return this.leveledItem;
				}
			}

			// Token: 0x06002676 RID: 9846 RVA: 0x0007827A File Offset: 0x0007647A
			public void SetLevel(int level)
			{
				if (level == this.ItemLevel)
				{
					return;
				}
				this.levelBonus += level - this.ItemLevel;
				this.LeveledItem.ItemLevel = level;
			}

			// Token: 0x04001B63 RID: 7011
			public PickableObjectBase pickable;

			// Token: 0x04001B64 RID: 7012
			public bool hasSameItem;

			// Token: 0x04001B65 RID: 7013
			public bool slotBusy;

			// Token: 0x04001B66 RID: 7014
			public int levelBonus;

			// Token: 0x04001B67 RID: 7015
			private ILeveledItem leveledItem;
		}
	}
}
