using System;
using Common.Editor;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Abilities;
using UnityEngine;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003EE RID: 1006
	[Service(typeof(AbilitySpecialBehavioursFactory), new Type[]
	{
		typeof(IAbilitySpecialBehavioursFactory)
	})]
	public sealed class AbilitySpecialBehavioursFactory : AbilitySpecialBehaviourFactoryBase<AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData, AbilityModifiersController>, IAbilitySpecialBehavioursFactory, IFactory<AbilitySpecialBehaviourID, AbilityModifiersController>, IFactory
	{
		// Token: 0x060021E7 RID: 8679 RVA: 0x00069BA0 File Offset: 0x00067DA0
		protected override bool TryGetSpecialBehaviourItemInstanceID(IAbilityExtension specialBehaviourItem, out int instanceID)
		{
			AbilityModifiersController abilityModifiersController = specialBehaviourItem as AbilityModifiersController;
			if (abilityModifiersController != null && abilityModifiersController.Prototype != null)
			{
				instanceID = abilityModifiersController.Prototype.GetInstanceID();
				return true;
			}
			instanceID = 0;
			return false;
		}

		// Token: 0x060021E8 RID: 8680 RVA: 0x00069BD8 File Offset: 0x00067DD8
		protected override AbilityModifiersController Create(AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData data, IBaseObjectDescription args)
		{
			if (data == null)
			{
				return null;
			}
			AbilityModifiersController specialBehaviour = data.specialBehaviour;
			AbilityModifiersController abilityModifiersController = (AbilityModifiersController)specialBehaviour.ForceInstantiate();
			abilityModifiersController.Prototype = specialBehaviour;
			return abilityModifiersController;
		}

		// Token: 0x060021E9 RID: 8681 RVA: 0x00069C03 File Offset: 0x00067E03
		public AbilityModifiersController Create(AbilitySpecialBehaviourID behaviourID)
		{
			return base.Create((int)behaviourID);
		}

		// Token: 0x060021EA RID: 8682 RVA: 0x00069C0C File Offset: 0x00067E0C
		public AbilitySpecialBehaviourID GetSpecialBehaviourID(BaseAbility ability)
		{
			return (AbilitySpecialBehaviourID)base.GetSpecialBehaviourItemID(ability);
		}

		// Token: 0x0200059C RID: 1436
		[Serializable]
		public sealed class SpecialBehaviourPrototypeData : IUnityObjectDescription, IBaseObjectDescription
		{
			// Token: 0x1700081A RID: 2074
			// (get) Token: 0x060027C0 RID: 10176 RVA: 0x0007C5EE File Offset: 0x0007A7EE
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.specialBehaviour;
				}
			}

			// Token: 0x1700081B RID: 2075
			// (get) Token: 0x060027C1 RID: 10177 RVA: 0x0007C5F6 File Offset: 0x0007A7F6
			// (set) Token: 0x060027C2 RID: 10178 RVA: 0x0007C5FE File Offset: 0x0007A7FE
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.specialBehaviourID;
				}
				set
				{
					this.specialBehaviourID = (AbilitySpecialBehaviourID)value;
				}
			}

			// Token: 0x04001D0B RID: 7435
			[EnumPopup]
			public AbilitySpecialBehaviourID specialBehaviourID;

			// Token: 0x04001D0C RID: 7436
			public AbilityModifiersController specialBehaviour;
		}
	}
}
