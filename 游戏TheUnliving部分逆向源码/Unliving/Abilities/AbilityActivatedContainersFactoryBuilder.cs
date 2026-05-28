using System;
using Common.Editor.Reorderable;
using Common.Factories;
using Game.Abilities;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Abilities
{
	// Token: 0x0200039C RID: 924
	[CreateAssetMenu(fileName = "AbilityActivatedContainersFactoryBuilder", menuName = "Game/Factories/Ability Activated Containers Factory Builder")]
	public sealed class AbilityActivatedContainersFactoryBuilder : PrototypeBasedFactoryBuilder<AbilityFactoryPrototype, BaseAbility>
	{
		// Token: 0x17000624 RID: 1572
		// (get) Token: 0x06001E7B RID: 7803 RVA: 0x00060AAB File Offset: 0x0005ECAB
		// (set) Token: 0x06001E7C RID: 7804 RVA: 0x00060AB8 File Offset: 0x0005ECB8
		public override AbilityFactoryPrototype[] FactoryData
		{
			get
			{
				return this.factoryData;
			}
			set
			{
				this.factoryData.list = value;
			}
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x00060AC6 File Offset: 0x0005ECC6
		protected override PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility> CreateFactoryInternal()
		{
			return new AbilityActivatedContainersFactory
			{
				defaultAbilitiesData = this.defaultAbilitiesData
			};
		}

		// Token: 0x0400112E RID: 4398
		public MultiRepresentationObjectInstantiator.DefaultData defaultAbilitiesData;

		// Token: 0x0400112F RID: 4399
		[SerializeField]
		[CustomReorderableList(null, "abilityPrototype", true, true)]
		private ReorderableListAdapter<AbilityFactoryPrototype[]> factoryData;
	}
}
