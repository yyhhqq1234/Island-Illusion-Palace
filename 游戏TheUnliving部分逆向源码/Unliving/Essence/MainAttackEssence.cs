using System;
using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.DropSystem;
using Unliving.LeveledItems;
using Unliving.Pickables;
using Unliving.Player;
using Unliving.Purchasing;

namespace Unliving.Essence
{
	// Token: 0x020002CE RID: 718
	public class MainAttackEssence : EssenceBase
	{
		// Token: 0x140000F5 RID: 245
		// (add) Token: 0x060018FB RID: 6395 RVA: 0x0004ECAC File Offset: 0x0004CEAC
		// (remove) Token: 0x060018FC RID: 6396 RVA: 0x0004ECE4 File Offset: 0x0004CEE4
		public override event Action EmptyEssenceSpawned;

		// Token: 0x060018FD RID: 6397 RVA: 0x0004ED1C File Offset: 0x0004CF1C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<AbilitiesFactory>(out this.abilitiesFactory);
			this.playerAbilitiesController = (base.CurrentPlayer.AbilitiesController as PlayerAbilitiesController);
			bool flag;
			this.UpdateBattleAbilitiesInfo(out flag, out this.lockedAbilities);
			if (!flag)
			{
				Action emptyEssenceSpawned = this.EmptyEssenceSpawned;
				if (emptyEssenceSpawned != null)
				{
					emptyEssenceSpawned();
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x060018FE RID: 6398 RVA: 0x0004ED88 File Offset: 0x0004CF88
		public void UpdateBattleAbilitiesInfo(out bool hasPurchasedBattleAbilities, out List<AbilityID> lockedAbilities)
		{
			lockedAbilities = new List<AbilityID>();
			hasPurchasedBattleAbilities = false;
			if (this.playerAbilitiesController == null)
			{
				return;
			}
			AbilityID id = (AbilityID)this.playerAbilitiesController.GetMainBattleAbility().ID;
			List<PurchaseItemAbility> purchasablesOfType = this.purchaseManager.GetPurchasablesOfType<PurchaseItemAbility>(true);
			if (purchasablesOfType.Count > 0)
			{
				for (int i = 0; i < purchasablesOfType.Count; i++)
				{
					PurchaseItemAbility purchaseItemAbility = purchasablesOfType[i];
					AbilityID abilityID = (AbilityID)purchaseItemAbility.ObjectID;
					if (purchaseItemAbility.Locked)
					{
						lockedAbilities.Add(abilityID);
					}
					else if (abilityID != id)
					{
						AbilityFactoryPrototype objectPrototype = this.abilitiesFactory.GetObjectPrototype<AbilityID>(abilityID);
						BaseAbility baseAbility = (objectPrototype != null) ? objectPrototype.abilityPrototype : null;
						if (baseAbility != null && baseAbility.IsPlayerMainBattleAbilityPrototype())
						{
							hasPurchasedBattleAbilities = true;
						}
					}
				}
			}
		}

		// Token: 0x060018FF RID: 6399 RVA: 0x0004EE38 File Offset: 0x0004D038
		private void CreateFactoryArgs()
		{
			this.factoryArgs = new AbilityFactoryArgs
			{
				specialBehaviourDescription = null,
				preventRandomSpecialBehaviourGeneration = true
			};
			IItemLevelProvider itemLevelProvider = this.playerAbilitiesController.GetMainBattleAbility() as IItemLevelProvider;
			if (itemLevelProvider != null)
			{
				this.factoryArgs.abilityLevel = itemLevelProvider.ItemLevel;
			}
		}

		// Token: 0x06001900 RID: 6400 RVA: 0x0004EE84 File Offset: 0x0004D084
		private IPickableObject SpawnPickable(AbilityID abilityID, DropSpawner spawner, AbilityFactoryArgs args)
		{
			List<object> list = this.lockedAbilities.Cast<object>().ToList<object>();
			if (!list.Contains(abilityID))
			{
				list.Add(abilityID);
			}
			IPickableObject pickableObject;
			IDropable dropable;
			spawner.SpawnPickable(list, out pickableObject, out dropable, args);
			pickableObject.SetPickingSettings(new OnClickPickingSettings());
			return pickableObject;
		}

		// Token: 0x06001901 RID: 6401 RVA: 0x0004EED4 File Offset: 0x0004D0D4
		protected override void SpawnItems(List<object> itemsExceptionListOverride = null)
		{
			this.items.Clear();
			if (this.playerAbilitiesController == null)
			{
				return;
			}
			this.CreateFactoryArgs();
			AbilityID id = (AbilityID)this.playerAbilitiesController.GetMainBattleAbility().ID;
			IPickableObject item = this.SpawnPickable(id, this.dropSpawners[0], this.factoryArgs);
			this.items.Add(item);
		}

		// Token: 0x04000E18 RID: 3608
		private AbilitiesFactory abilitiesFactory;

		// Token: 0x04000E19 RID: 3609
		private AbilityFactoryArgs factoryArgs;

		// Token: 0x04000E1A RID: 3610
		private PlayerAbilitiesController playerAbilitiesController;

		// Token: 0x04000E1B RID: 3611
		private List<AbilityID> lockedAbilities;
	}
}
