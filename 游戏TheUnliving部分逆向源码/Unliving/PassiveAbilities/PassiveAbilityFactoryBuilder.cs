using System;
using Common.Editor.Reorderable;
using Common.Factories;
using Game.Localization;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A3 RID: 419
	[CreateAssetMenu(fileName = "PassiveAbilityFactoryBuilder", menuName = "Game/Factories/Passive Ability Factory Builder")]
	public sealed class PassiveAbilityFactoryBuilder : PrototypeBasedFactoryBuilder<PassiveAbilityFactoryPrototype, PassiveAbilityBase>, ILocalizable
	{
		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06000BF4 RID: 3060 RVA: 0x00025CD8 File Offset: 0x00023ED8
		// (set) Token: 0x06000BF5 RID: 3061 RVA: 0x00025CE5 File Offset: 0x00023EE5
		public override PassiveAbilityFactoryPrototype[] FactoryData
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

		// Token: 0x06000BF6 RID: 3062 RVA: 0x00025CF3 File Offset: 0x00023EF3
		protected override PrototypeBasedFactory<PassiveAbilityFactoryPrototype, PassiveAbilityBase> CreateFactoryInternal()
		{
			return new PassiveAbilityFactory
			{
				defaultAbilitiesData = this.defaultAbilitiesData
			};
		}

		// Token: 0x06000BF7 RID: 3063 RVA: 0x00025D08 File Offset: 0x00023F08
		object[] ILocalizable.GetLocalizableObjects()
		{
			return this.factoryData.list;
		}

		// Token: 0x040006A9 RID: 1705
		public MultiRepresentationObjectInstantiator.DefaultData defaultAbilitiesData;

		// Token: 0x040006AA RID: 1706
		[SerializeField]
		[CustomReorderableList(null, "abilityPrototype", true, true)]
		private ReorderableListAdapter<PassiveAbilityFactoryPrototype[]> factoryData;
	}
}
