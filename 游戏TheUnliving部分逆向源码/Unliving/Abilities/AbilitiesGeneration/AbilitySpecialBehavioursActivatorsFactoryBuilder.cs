using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003ED RID: 1005
	[CreateAssetMenu(fileName = "AbilitySpecialBehavioursActivatorsFactoryBuilder", menuName = "Game/Abilities Generation/Ability Special Behaviours Activators Factory Builder")]
	public sealed class AbilitySpecialBehavioursActivatorsFactoryBuilder : PrototypeBasedFactoryBuilder<AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData, AbilityModifiersActivatorBase>
	{
		// Token: 0x170006DA RID: 1754
		// (get) Token: 0x060021E3 RID: 8675 RVA: 0x00069B73 File Offset: 0x00067D73
		// (set) Token: 0x060021E4 RID: 8676 RVA: 0x00069B80 File Offset: 0x00067D80
		public override AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData[] FactoryData
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

		// Token: 0x060021E5 RID: 8677 RVA: 0x00069B8E File Offset: 0x00067D8E
		protected override PrototypeBasedFactory<AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData, AbilityModifiersActivatorBase> CreateFactoryInternal()
		{
			return new AbilitySpecialBehavioursActivatorsFactory();
		}

		// Token: 0x04001568 RID: 5480
		[SerializeField]
		[CustomReorderableList]
		private ReorderableListAdapter<AbilitySpecialBehavioursActivatorsFactory.ActivatorPrototypeData[]> factoryData;
	}
}
