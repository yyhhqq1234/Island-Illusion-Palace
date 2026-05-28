using System;
using Game.Damage;
using Game.Localization;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.Mobs;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x02000180 RID: 384
	public sealed class PickableAbilityActivatedContainer : PickableObjectBase<AbilityID>, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06000AAB RID: 2731 RVA: 0x0002334C File Offset: 0x0002154C
		public override AbilityID ID
		{
			get
			{
				return this.abilityDescription.abilityID;
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06000AAC RID: 2732 RVA: 0x00023359 File Offset: 0x00021559
		// (set) Token: 0x06000AAD RID: 2733 RVA: 0x00023366 File Offset: 0x00021566
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

		// Token: 0x06000AAE RID: 2734 RVA: 0x00023380 File Offset: 0x00021580
		public override void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			this.abilityDescription = AbilityInfo.Undefined;
			AbilityFactoryPrototype abilityFactoryPrototype = data as AbilityFactoryPrototype;
			if (abilityFactoryPrototype != null)
			{
				this.abilityDescription.abilityID = abilityFactoryPrototype.abilityID;
			}
			base.InitializeData(args, data);
			Metadata metadata = this.PickableObjectData.metadata;
		}

		// Token: 0x06000AAF RID: 2735 RVA: 0x000233C7 File Offset: 0x000215C7
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000AB0 RID: 2736 RVA: 0x000233CC File Offset: 0x000215CC
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return;
			}
			if (this.currentCollector == this.PointerEventsSender)
			{
				IContainerBasedHPController component = this.currentCollector.Component.GetComponent<IContainerBasedHPController>();
				if (component.CanAddContainer())
				{
					HealthContainer container = new HealthContainer(component.InitialHitPoints);
					component.AddContainer(container);
				}
				else
				{
					HitPointsController.HPChangingArgs args = new HitPointsController.HPChangingArgs(false)
					{
						amount = component.InitialHitPoints
					};
					component.ModifyHitPoints(this, args);
					VitalEnergyHitPointsController.RestoreVitalEnergyArgs args2 = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs
					{
						amount = component.InitialHitPoints
					};
					component.ModifyHitPoints(this, args2);
				}
				IAbilityActivatedContainersController abilityActivatedContainersController = component as IAbilityActivatedContainersController;
				if (abilityActivatedContainersController != null)
				{
					AbilityInfo abilityInfo = new AbilityInfo
					{
						abilityID = this.ID,
						abilityLevel = this.ItemLevel
					};
					CurrencyOperationArgs destructionRewardArgs = (base.PurchasableData as PurchasableItemAbilityActivatedContainer).destructionRewardArgs;
					abilityActivatedContainersController.AddContainer(abilityInfo, destructionRewardArgs);
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04000631 RID: 1585
		private AbilityInfo abilityDescription;
	}
}
