using System;
using Common.Editor.Reorderable;
using Common.Factories;
using Game.Abilities;
using Game.Localization;
using UnityEngine;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Factories;

namespace Unliving.Abilities
{
	// Token: 0x0200039E RID: 926
	[CreateAssetMenu(fileName = "AbilityFactoryBuilder", menuName = "Game/Factories/Ability Factory Builder")]
	public sealed class AbilityFactoryBuilder : PrototypeBasedFactoryBuilder<AbilityFactoryPrototype, BaseAbility>, ILocalizable
	{
		// Token: 0x17000629 RID: 1577
		// (get) Token: 0x06001E88 RID: 7816 RVA: 0x00060B74 File Offset: 0x0005ED74
		// (set) Token: 0x06001E89 RID: 7817 RVA: 0x00060B81 File Offset: 0x0005ED81
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

		// Token: 0x06001E8A RID: 7818 RVA: 0x00060B8F File Offset: 0x0005ED8F
		protected override PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility> CreateFactoryInternal()
		{
			AbilitiesFactory abilitiesFactory = new AbilitiesFactory();
			abilitiesFactory.defaultAbilitiesData = this.defaultAbilitiesData;
			AbilitySpecialBehaviourGeneratorDataAsset abilitySpecialBehaviourGeneratorDataAsset = this.abilitiesSpecialBehavioursDataAsset;
			abilitiesFactory.specialBehavioursGenerationData = ((abilitySpecialBehaviourGeneratorDataAsset != null) ? abilitySpecialBehaviourGeneratorDataAsset.Data : null);
			return abilitiesFactory;
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x00060BBC File Offset: 0x0005EDBC
		object[] ILocalizable.GetLocalizableObjects()
		{
			return this.factoryData.list;
		}

		// Token: 0x0400113F RID: 4415
		public MultiRepresentationObjectInstantiator.DefaultData defaultAbilitiesData;

		// Token: 0x04001140 RID: 4416
		public AbilitySpecialBehaviourGeneratorDataAsset abilitiesSpecialBehavioursDataAsset;

		// Token: 0x04001141 RID: 4417
		[SerializeField]
		[CustomReorderableList(null, "abilityPrototype", true, true)]
		private ReorderableListAdapter<AbilityFactoryPrototype[]> factoryData;
	}
}
