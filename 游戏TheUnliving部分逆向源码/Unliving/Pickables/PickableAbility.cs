using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Localization;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.Localization;
using Unliving.Mobs;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.Pickables
{
	// Token: 0x0200017F RID: 383
	public sealed class PickableAbility : PickableObjectBase<AbilityID>, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x06000A9C RID: 2716 RVA: 0x00022F14 File Offset: 0x00021114
		private static bool TryGetAbilitiesController(IPickableObjectCollector abilityCollector, out BaseAbilitiesController abilitiesController)
		{
			BaseGameMob baseGameMob = abilityCollector.Component.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob != null)
			{
				abilitiesController = baseGameMob.AbilitiesController;
				return abilitiesController != null;
			}
			abilitiesController = null;
			return false;
		}

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06000A9D RID: 2717 RVA: 0x00022F48 File Offset: 0x00021148
		public BaseAbility AbilityPrototype
		{
			get
			{
				return this.abilityPrototype;
			}
		}

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06000A9E RID: 2718 RVA: 0x00022F50 File Offset: 0x00021150
		public AbilityInfo AbilityDescription
		{
			get
			{
				return this.abilityDescription;
			}
		}

		// Token: 0x170001C7 RID: 455
		// (get) Token: 0x06000A9F RID: 2719 RVA: 0x00022F58 File Offset: 0x00021158
		public override AbilityID ID
		{
			get
			{
				return this.abilityDescription.abilityID;
			}
		}

		// Token: 0x170001C8 RID: 456
		// (get) Token: 0x06000AA0 RID: 2720 RVA: 0x00022F65 File Offset: 0x00021165
		public int AbilityType
		{
			get
			{
				return this.abilityPrototype.Type;
			}
		}

		// Token: 0x170001C9 RID: 457
		// (get) Token: 0x06000AA1 RID: 2721 RVA: 0x00022F72 File Offset: 0x00021172
		// (set) Token: 0x06000AA2 RID: 2722 RVA: 0x00022F7F File Offset: 0x0002117F
		public int ItemLevel
		{
			get
			{
				return this.abilityDescription.abilityLevel;
			}
			set
			{
				this.abilityDescription.abilityLevel = value;
				this.pickableObjectData.metadata = null;
			}
		}

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06000AA3 RID: 2723 RVA: 0x00022F99 File Offset: 0x00021199
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.ItemLevel;
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06000AA4 RID: 2724 RVA: 0x00022FA4 File Offset: 0x000211A4
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (base.PickableObjectData.metadata.AdditionalText == null)
				{
					this.pickableObjectData.metadata.AdditionalText = new string[2];
					bool flag = this.IsPlayerMainBattleAbility();
					string id = flag ? "main_battle_ability_ui_label" : "ability_ui_label";
					this.pickableObjectData.metadata.AdditionalText[0] = this.localizationManager.GetTitle(id);
					this.pickableObjectData.metadata.AdditionalText[1] = this.localizationManager.GetMetadata<AbilityTypes>((AbilityTypes)this.abilityPrototype.Type, Array.Empty<string>()).Title;
					if (flag && this.abilityDescription.specialBehaviourDescription != null)
					{
						string specialBehaviourDescriptionLocalization = this.abilityDescription.specialBehaviourDescription.GetSpecialBehaviourDescriptionLocalization(this.localizationManager, false);
						Metadata metadata = this.pickableObjectData.metadata;
						metadata.Description += specialBehaviourDescriptionLocalization;
					}
				}
				this.pickableObjectData.metadata.AdditionalDescription = string.Empty;
				return this.pickableObjectData;
			}
		}

		// Token: 0x06000AA5 RID: 2725 RVA: 0x000230A6 File Offset: 0x000212A6
		public bool IsPlayerMainBattleAbility()
		{
			return this.abilityPrototype.IsPlayerMainBattleAbilityPrototype();
		}

		// Token: 0x06000AA6 RID: 2726 RVA: 0x000230B4 File Offset: 0x000212B4
		public override void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			this.abilityDescription = AbilityInfo.Undefined;
			this.storedReloadingProgress = null;
			AbilityFactoryPrototype abilityFactoryPrototype = data as AbilityFactoryPrototype;
			if (abilityFactoryPrototype != null)
			{
				this.abilityDescription.abilityID = abilityFactoryPrototype.abilityID;
				this.abilityPrototype = abilityFactoryPrototype.abilityPrototype;
			}
			IItemLevelProvider itemLevelProvider = args as IItemLevelProvider;
			if (itemLevelProvider != null)
			{
				this.abilityDescription.abilityLevel = itemLevelProvider.ItemLevel;
			}
			else
			{
				this.abilityDescription.abilityLevel = 1;
			}
			AbilityFactoryArgs abilityFactoryArgs = args as AbilityFactoryArgs;
			if (abilityFactoryArgs != null && abilityFactoryArgs.specialBehaviourDescription != null)
			{
				this.abilityDescription.specialBehaviourDescription = abilityFactoryArgs.specialBehaviourDescription;
				this.storedReloadingProgress = abilityFactoryArgs.reloadingProgressOverride;
			}
			base.InitializeData(args, data);
			base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.playerProfileManager);
		}

		// Token: 0x06000AA7 RID: 2727 RVA: 0x0002317C File Offset: 0x0002137C
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject && !base.CanBePickedInHomespace)
			{
				return true;
			}
			BaseAbilitiesController baseAbilitiesController;
			if (PickableAbility.TryGetAbilitiesController(targetCollector, out baseAbilitiesController))
			{
				PlayerAbilitiesController playerAbilitiesController = baseAbilitiesController as PlayerAbilitiesController;
				if (playerAbilitiesController != null && playerAbilitiesController.CanBeAddedAsMainBattleAbility(this.abilityPrototype))
				{
					return true;
				}
				ISlotsBasedAbilitiesController slotsBasedAbilitiesController = baseAbilitiesController as ISlotsBasedAbilitiesController;
				if (slotsBasedAbilitiesController != null)
				{
					return slotsBasedAbilitiesController.GetCompatibleAbilitySlot(this.abilityPrototype.Type) >= 0;
				}
			}
			return false;
		}

		// Token: 0x06000AA8 RID: 2728 RVA: 0x000231E4 File Offset: 0x000213E4
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject && !base.CanBePickedInHomespace)
			{
				return;
			}
			BaseAbilitiesController baseAbilitiesController;
			if (PickableAbility.TryGetAbilitiesController(this.currentCollector, out baseAbilitiesController))
			{
				BaseAbility baseAbility = null;
				PlayerAbilitiesController playerAbilitiesController = baseAbilitiesController as PlayerAbilitiesController;
				if (playerAbilitiesController != null)
				{
					if (this.IsPlayerMainBattleAbility())
					{
						baseAbility = playerAbilitiesController.AddMainBattleAbility(this.abilityDescription, false);
					}
					else
					{
						int compatibleAbilitySlot = playerAbilitiesController.GetCompatibleAbilitySlot(this.abilityPrototype.Type);
						if (compatibleAbilitySlot >= 0)
						{
							playerAbilitiesController.ClearAbilitySlot(compatibleAbilitySlot);
							baseAbility = playerAbilitiesController.AddAbilityToSlot(this.abilityDescription, compatibleAbilitySlot, false);
						}
					}
				}
				if (baseAbility != null && this.storedReloadingProgress != null)
				{
					baseAbility.SetReloadingProgress(this.storedReloadingProgress.Value);
				}
			}
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				this.playerProfileManager.SavePlayerProfile(base.CurrentPlayer);
				return;
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06000AA9 RID: 2729 RVA: 0x000232B8 File Offset: 0x000214B8
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameItem)
		{
			int num = 1;
			int id = (int)this.ID;
			hasSameItem = false;
			slotBusy = false;
			PlayerAbilitiesController playerAbilitiesController = base.CurrentPlayer.AbilitiesController as PlayerAbilitiesController;
			if (playerAbilitiesController != null)
			{
				int num2;
				IAbility abilityByID = playerAbilitiesController.GetAbilityByID(id, out num2);
				if (abilityByID != null)
				{
					slotBusy = true;
					hasSameItem = true;
					num = Mathf.Max((abilityByID as IItemLevelProvider).ItemLevel, num);
				}
				else
				{
					IAbility lowerLevelAbilityOfType = playerAbilitiesController.GetLowerLevelAbilityOfType(this.AbilityType);
					if (lowerLevelAbilityOfType != null)
					{
						slotBusy = true;
						int itemLevel = (lowerLevelAbilityOfType as IItemLevelProvider).ItemLevel;
						if (itemLevel >= num)
						{
							num = itemLevel;
						}
					}
				}
				this.ItemLevel = num;
				return num;
			}
			return 0;
		}

		// Token: 0x0400062B RID: 1579
		private const string PICKABLE_ABILITY_LOCALIZATION_KEY = "ability_ui_label";

		// Token: 0x0400062C RID: 1580
		private const string PICKABLE_MAIN_BATTLE_ABILITY_LOCALIZATION_KEY = "main_battle_ability_ui_label";

		// Token: 0x0400062D RID: 1581
		private AbilityInfo abilityDescription;

		// Token: 0x0400062E RID: 1582
		private BaseAbility abilityPrototype;

		// Token: 0x0400062F RID: 1583
		private float? storedReloadingProgress;

		// Token: 0x04000630 RID: 1584
		private PlayerProfileManager playerProfileManager;
	}
}
