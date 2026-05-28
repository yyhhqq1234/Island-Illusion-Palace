using System;
using System.Collections.Generic;
using Game.Abilities;
using Unliving.Abilities;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Currencies;
using Unliving.DropSystem;
using Unliving.LeveledItems;
using Unliving.Pickables;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Essence
{
	// Token: 0x020002C6 RID: 710
	public class AshSmithy : EssenceBase
	{
		// Token: 0x17000542 RID: 1346
		// (get) Token: 0x060018C2 RID: 6338 RVA: 0x0004DFD1 File Offset: 0x0004C1D1
		// (set) Token: 0x060018C3 RID: 6339 RVA: 0x0004DFD9 File Offset: 0x0004C1D9
		public BaseAbility CurrentAbility { get; private set; }

		// Token: 0x17000543 RID: 1347
		// (get) Token: 0x060018C4 RID: 6340 RVA: 0x0004DFE2 File Offset: 0x0004C1E2
		// (set) Token: 0x060018C5 RID: 6341 RVA: 0x0004DFEA File Offset: 0x0004C1EA
		public AbilitySpecialBehaviourDescription CurrentAbilitySpecialBehaviour { get; private set; }

		// Token: 0x060018C6 RID: 6342 RVA: 0x0004DFF4 File Offset: 0x0004C1F4
		protected override void SpawnItems(List<object> itemsExceptionListOverride = null)
		{
			this.items.Clear();
			PlayerAbilitiesController playerAbilitiesController = base.CurrentPlayer.AbilitiesController as PlayerAbilitiesController;
			if (playerAbilitiesController != null)
			{
				this.CurrentAbility = playerAbilitiesController.GetMainBattleAbility();
				AbilityID id = (AbilityID)this.CurrentAbility.ID;
				int num = 1;
				IItemLevelProvider itemLevelProvider = this.CurrentAbility as IItemLevelProvider;
				if (itemLevelProvider != null)
				{
					num = itemLevelProvider.ItemLevel;
				}
				int level = num + 1;
				PurchasableAshSmithy purchasableAshSmithy = base.PurchasableData as PurchasableAshSmithy;
				if (purchasableAshSmithy != null && base.CurrentGame.Services.TryGet<IGameAbilitiesFactory>(out this.gameAbilitiesFactory))
				{
					this.CreateUpgradeSlot(this.CurrentAbility, level, purchasableAshSmithy, this.gameAbilitiesFactory);
					this.CreateEnchantSlot(id, num, purchasableAshSmithy, this.gameAbilitiesFactory);
					this.CreateReforgeSlot(id, level, purchasableAshSmithy, this.gameAbilitiesFactory);
				}
			}
		}

		// Token: 0x060018C7 RID: 6343 RVA: 0x0004E0B8 File Offset: 0x0004C2B8
		private void CreateUpgradeSlot(BaseAbility ability, int level, PurchasableAshSmithy purchasable, IGameAbilitiesFactory gameAbilitiesFactory)
		{
			this.CurrentAbilitySpecialBehaviour = gameAbilitiesFactory.GetAbilitySpecialBehaviourDescription(ability);
			this.factoryArgs = new AbilityFactoryArgs
			{
				abilityLevel = level,
				preventRandomSpecialBehaviourGeneration = true
			};
			if (this.CurrentAbilitySpecialBehaviour != null && !this.CurrentAbilitySpecialBehaviour.IsBlank())
			{
				this.factoryArgs.specialBehaviourDescription = this.CurrentAbilitySpecialBehaviour;
			}
			IPickableObject pickableObject = this.SpawnPickable((AbilityID)ability.ID, this.dropSpawners[0], this.factoryArgs);
			IPurchasableObject purchasableObject = pickableObject as IPurchasableObject;
			if (purchasableObject != null)
			{
				purchasable.UpdateUpgradePrice(level);
				this.CreateTemporaryBuyArgs(purchasableObject, purchasable.BuyArgs);
			}
			this.items.Add(pickableObject);
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x0004E160 File Offset: 0x0004C360
		private void CreateReforgeSlot(AbilityID abilityID, int level, PurchasableAshSmithy purchasable, IGameAbilitiesFactory gameAbilitiesFactory)
		{
			AbilitySpecialBehaviourDescription randomAbilitySpecialBehaviourDescription = gameAbilitiesFactory.GetRandomAbilitySpecialBehaviourDescription((int)abilityID);
			if (randomAbilitySpecialBehaviourDescription == null || randomAbilitySpecialBehaviourDescription.IsBlank())
			{
				return;
			}
			this.factoryArgs = new AbilityFactoryArgs
			{
				abilityLevel = level,
				specialBehaviourDescription = randomAbilitySpecialBehaviourDescription
			};
			IPickableObject pickableObject = this.SpawnPickable(abilityID, this.dropSpawners[1], this.factoryArgs);
			IPurchasableObject purchasableObject = pickableObject as IPurchasableObject;
			if (purchasableObject != null)
			{
				purchasable.UpdateReforgePrice(level);
				this.CreateTemporaryBuyArgs(purchasableObject, purchasable.BuyArgs);
			}
			this.items.Add(pickableObject);
		}

		// Token: 0x060018C9 RID: 6345 RVA: 0x0004E1E4 File Offset: 0x0004C3E4
		private void CreateEnchantSlot(AbilityID abilityID, int level, PurchasableAshSmithy purchasable, IGameAbilitiesFactory gameAbilitiesFactory)
		{
			AbilitySpecialBehaviourDescription randomAbilitySpecialBehaviourDescription = gameAbilitiesFactory.GetRandomAbilitySpecialBehaviourDescription((int)abilityID);
			if (randomAbilitySpecialBehaviourDescription == null || randomAbilitySpecialBehaviourDescription.IsBlank())
			{
				return;
			}
			this.factoryArgs = new AbilityFactoryArgs
			{
				abilityLevel = level,
				specialBehaviourDescription = randomAbilitySpecialBehaviourDescription
			};
			IPickableObject pickableObject = this.SpawnPickable(abilityID, this.dropSpawners[2], this.factoryArgs);
			IPurchasableObject purchasableObject = pickableObject as IPurchasableObject;
			if (purchasableObject != null)
			{
				purchasable.UpdateEnchantPrice();
				this.CreateTemporaryBuyArgs(purchasableObject, purchasable.BuyArgs);
			}
			this.items.Add(pickableObject);
		}

		// Token: 0x060018CA RID: 6346 RVA: 0x0004E264 File Offset: 0x0004C464
		private void CreateTemporaryBuyArgs(IPurchasableObject purchasableObject, CurrencyOperationArgs buyArgs)
		{
			purchasableObject.PurchasableData = purchasableObject.PurchasableData.Clone();
			purchasableObject.PurchasableData.BuyArgs = buyArgs;
		}

		// Token: 0x060018CB RID: 6347 RVA: 0x0004E284 File Offset: 0x0004C484
		private IPickableObject SpawnPickable(AbilityID abilityID, DropSpawner spawner, AbilityFactoryArgs args)
		{
			IPickableObject pickableObject;
			IDropable dropable;
			spawner.SpawnPickable(abilityID, out pickableObject, out dropable, args);
			pickableObject.SetPickingSettings(new OnClickPickingSettings());
			return pickableObject;
		}

		// Token: 0x04000DEB RID: 3563
		private AbilityFactoryArgs factoryArgs = new AbilityFactoryArgs();

		// Token: 0x04000DEC RID: 3564
		private IGameAbilitiesFactory gameAbilitiesFactory;
	}
}
