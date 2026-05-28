using System;
using Common.Factories;
using Common.UnityExtensions;
using Game.Core;
using Game.PassiveAbilities;
using UnityEngine;
using Unliving.Factories;
using Unliving.Mobs;
using Unliving.PassiveAbilities;

namespace Unliving.Pickables
{
	// Token: 0x02000189 RID: 393
	public sealed class PickablePassiveAbility : PickableObjectBase<PassiveAbilityID>
	{
		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06000B03 RID: 2819 RVA: 0x000240F3 File Offset: 0x000222F3
		public override PassiveAbilityID ID
		{
			get
			{
				return this.AbilityID;
			}
		}

		// Token: 0x06000B04 RID: 2820 RVA: 0x000240FC File Offset: 0x000222FC
		public override void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			PassiveAbilityFactoryPrototype passiveAbilityFactoryPrototype = data as PassiveAbilityFactoryPrototype;
			if (passiveAbilityFactoryPrototype != null)
			{
				this.abilityData = passiveAbilityFactoryPrototype;
				this.AbilityID = this.abilityData.abilityID;
			}
			base.InitializeData(args, data);
		}

		// Token: 0x06000B05 RID: 2821 RVA: 0x00024133 File Offset: 0x00022333
		private IPassiveAbilitiesController GetPassiveAbilitiesController(IPickableObjectCollector objectCollector)
		{
			BaseGameMob baseGameMob = objectCollector.Component.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob == null)
			{
				return null;
			}
			return baseGameMob.PassiveAbilitiesController;
		}

		// Token: 0x06000B06 RID: 2822 RVA: 0x0002414C File Offset: 0x0002234C
		private void Start()
		{
			if (this.abilityData == null)
			{
				IGame currentGame = base.CurrentGame;
				PrototypeBasedFactory<PassiveAbilityFactoryPrototype, PassiveAbilityBase> prototypeBasedFactory = ((currentGame != null) ? currentGame.Services.Get<IObjectFactory<PassiveAbilityBase>>() : null) as PrototypeBasedFactory<PassiveAbilityFactoryPrototype, PassiveAbilityBase>;
				if (prototypeBasedFactory != null)
				{
					this.abilityData = prototypeBasedFactory.GetObjectPrototype((int)this.AbilityID);
				}
			}
		}

		// Token: 0x06000B07 RID: 2823 RVA: 0x00024193 File Offset: 0x00022393
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject || this.GetPassiveAbilitiesController(targetCollector) != null;
		}

		// Token: 0x06000B08 RID: 2824 RVA: 0x000241AC File Offset: 0x000223AC
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return;
			}
			IPassiveAbilitiesController passiveAbilitiesController = this.GetPassiveAbilitiesController(this.currentCollector);
			if (passiveAbilitiesController == null || this.abilityData == null)
			{
				return;
			}
			passiveAbilitiesController.AddAbility((int)this.abilityData.abilityID);
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x04000649 RID: 1609
		public PassiveAbilityID AbilityID;

		// Token: 0x0400064A RID: 1610
		private PassiveAbilityFactoryPrototype abilityData;
	}
}
