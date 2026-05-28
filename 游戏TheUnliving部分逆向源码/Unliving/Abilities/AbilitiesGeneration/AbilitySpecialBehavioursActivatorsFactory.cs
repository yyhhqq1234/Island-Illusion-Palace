using System;
using Common.Editor;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003EC RID: 1004
	[Service(typeof(AbilitySpecialBehavioursActivatorsFactory), new Type[]
	{
		typeof(IAbilitySpecialBehavioursActivatorsFactory)
	})]
	public sealed class AbilitySpecialBehavioursActivatorsFactory : AbilitySpecialBehaviourFactoryBase<AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData, AbilityModifiersActivatorBase>, IAbilitySpecialBehavioursActivatorsFactory, IFactory<AbilitySpecialBehaviourActivatorID, AbilityModifiersActivatorBase>, IFactory
	{
		// Token: 0x060021DE RID: 8670 RVA: 0x00069B14 File Offset: 0x00067D14
		protected override bool TryGetSpecialBehaviourItemInstanceID(IAbilityExtension specialBehaviourItem, out int instanceID)
		{
			AbilityModifiersController abilityModifiersController = specialBehaviourItem as AbilityModifiersController;
			if (abilityModifiersController != null && abilityModifiersController.Activator != null)
			{
				instanceID = abilityModifiersController.Activator.GetInstanceID();
				return true;
			}
			instanceID = 0;
			return false;
		}

		// Token: 0x060021DF RID: 8671 RVA: 0x00069B4C File Offset: 0x00067D4C
		protected override AbilityModifiersActivatorBase Create(AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData data, IBaseObjectDescription query)
		{
			if (data == null)
			{
				return null;
			}
			return data.activator;
		}

		// Token: 0x060021E0 RID: 8672 RVA: 0x00069B59 File Offset: 0x00067D59
		public AbilityModifiersActivatorBase Create(AbilitySpecialBehaviourActivatorID activatorID)
		{
			return base.Create((int)activatorID);
		}

		// Token: 0x060021E1 RID: 8673 RVA: 0x00069B62 File Offset: 0x00067D62
		public AbilitySpecialBehaviourActivatorID GetSpecialBehaviourActivatorID(BaseAbility ability)
		{
			return (AbilitySpecialBehaviourActivatorID)base.GetSpecialBehaviourItemID(ability);
		}

		// Token: 0x0200059B RID: 1435
		[Serializable]
		public sealed class ActivatorPrototypeData : IUnityObjectDescription, IBaseObjectDescription
		{
			// Token: 0x17000818 RID: 2072
			// (get) Token: 0x060027BC RID: 10172 RVA: 0x0007C5CD File Offset: 0x0007A7CD
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.activator;
				}
			}

			// Token: 0x17000819 RID: 2073
			// (get) Token: 0x060027BD RID: 10173 RVA: 0x0007C5D5 File Offset: 0x0007A7D5
			// (set) Token: 0x060027BE RID: 10174 RVA: 0x0007C5DD File Offset: 0x0007A7DD
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.activatorID;
				}
				set
				{
					this.activatorID = (AbilitySpecialBehaviourActivatorID)value;
				}
			}

			// Token: 0x04001D09 RID: 7433
			[EnumPopup]
			public AbilitySpecialBehaviourActivatorID activatorID;

			// Token: 0x04001D0A RID: 7434
			public AbilityModifiersActivatorBase activator;
		}
	}
}
