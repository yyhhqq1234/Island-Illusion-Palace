using System;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x02000185 RID: 389
	public sealed class PickableMobActivationModifier : PickableObjectBase<MobActivationModifierID>, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06000AC5 RID: 2757 RVA: 0x000236CC File Offset: 0x000218CC
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (base.PickableObjectData.metadata.AdditionalText == null)
				{
					this.pickableObjectData.metadata.AdditionalText = new string[1];
					this.pickableObjectData.metadata.AdditionalText[0] = this.localizationManager.GetMetadata<MobActivationAbilityType>(this.modifierPrototype.allowedActivationAbilityTypes, Array.Empty<string>()).Title;
				}
				this.pickableObjectData.metadata.AdditionalDescription = string.Empty;
				return this.pickableObjectData;
			}
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06000AC6 RID: 2758 RVA: 0x0002374E File Offset: 0x0002194E
		public override MobActivationModifierID ID
		{
			get
			{
				return this.modifierPrototype.ModifierID;
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06000AC7 RID: 2759 RVA: 0x0002375B File Offset: 0x0002195B
		public MobActivationAbilityType AllowedActivationAbilityTypes
		{
			get
			{
				return this.modifierPrototype.allowedActivationAbilityTypes;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06000AC8 RID: 2760 RVA: 0x00023768 File Offset: 0x00021968
		public int ModifierLevel
		{
			get
			{
				return this.modifierLevel;
			}
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06000AC9 RID: 2761 RVA: 0x00023770 File Offset: 0x00021970
		public MobActivationAbilityModifier ModifierPrototype
		{
			get
			{
				return this.modifierPrototype;
			}
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06000ACA RID: 2762 RVA: 0x00023778 File Offset: 0x00021978
		// (set) Token: 0x06000ACB RID: 2763 RVA: 0x00023780 File Offset: 0x00021980
		public int ItemLevel
		{
			get
			{
				return this.modifierLevel;
			}
			set
			{
				this.modifierLevel = value;
				this.pickableObjectData.metadata = null;
			}
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06000ACC RID: 2764 RVA: 0x00023795 File Offset: 0x00021995
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.ItemLevel;
			}
		}

		// Token: 0x06000ACD RID: 2765 RVA: 0x000237A0 File Offset: 0x000219A0
		private MobsActivationModifiersController GetModifiersController(IPickableObjectCollector targetCollector)
		{
			if (targetCollector == null || targetCollector.Component == null)
			{
				return null;
			}
			PlayerBehaviour playerBehaviour = targetCollector.Component.CastOrGetComponent<PlayerBehaviour>();
			if (playerBehaviour == null)
			{
				return null;
			}
			return playerBehaviour.ActivationModifiersController;
		}

		// Token: 0x06000ACE RID: 2766 RVA: 0x000237E0 File Offset: 0x000219E0
		public override void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			MobsActivationModifiersFactory.PrototypeInfo prototypeInfo = data as MobsActivationModifiersFactory.PrototypeInfo;
			if (prototypeInfo != null)
			{
				this.modifierPrototype = prototypeInfo.modifierPrototype;
				this.modifierPrototype.ModifierID = prototypeInfo.modifierID;
			}
			IItemLevelProvider itemLevelProvider = args as IItemLevelProvider;
			this.modifierLevel = ((itemLevelProvider != null) ? itemLevelProvider.ItemLevel : 1);
			base.InitializeData(args, data);
		}

		// Token: 0x06000ACF RID: 2767 RVA: 0x00023838 File Offset: 0x00021A38
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return true;
			}
			MobsActivationModifiersController modifiersController = this.GetModifiersController(targetCollector);
			return modifiersController != null && modifiersController.GetCompatibleSlot(this.modifierPrototype.allowedActivationAbilityTypes) >= 0;
		}

		// Token: 0x06000AD0 RID: 2768 RVA: 0x00023874 File Offset: 0x00021A74
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return;
			}
			MobsActivationModifiersController modifiersController = this.GetModifiersController(this.currentCollector);
			if (modifiersController != null)
			{
				int compatibleSlot = modifiersController.GetCompatibleSlot(this.modifierPrototype.allowedActivationAbilityTypes);
				if (compatibleSlot >= 0)
				{
					modifiersController.RemoveModifier(compatibleSlot);
					modifiersController.AddModifier(compatibleSlot, this.modifierPrototype, this.modifierLevel);
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06000AD1 RID: 2769 RVA: 0x000238E0 File Offset: 0x00021AE0
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameItem)
		{
			hasSameItem = false;
			slotBusy = false;
			int num = 1;
			MobsActivationModifiersController activationModifiersController = base.CurrentPlayer.ActivationModifiersController;
			MobsActivationModifiersController.Slot slot;
			if (activationModifiersController.TryGetSlotWithModifier(this.ID, out slot))
			{
				slotBusy = true;
				hasSameItem = true;
				num = Mathf.Max(slot.CurrentModifierLevel, num);
			}
			else
			{
				MobsActivationModifiersController.Slot lowerLevelCompatibleSlot = activationModifiersController.GetLowerLevelCompatibleSlot(this.AllowedActivationAbilityTypes);
				slotBusy = !lowerLevelCompatibleSlot.IsFree();
				if (lowerLevelCompatibleSlot.CurrentModifierLevel > 0)
				{
					num = Mathf.Max(lowerLevelCompatibleSlot.CurrentModifierLevel, num);
				}
			}
			this.ItemLevel = num;
			return num;
		}

		// Token: 0x04000636 RID: 1590
		private MobActivationAbilityModifier modifierPrototype;

		// Token: 0x04000637 RID: 1591
		private int modifierLevel;
	}
}
