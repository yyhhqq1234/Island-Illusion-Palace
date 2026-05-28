using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003EF RID: 1007
	[CreateAssetMenu(fileName = "AbilitySpecialBehavioursFactoryBuilder", menuName = "Game/Abilities Generation/Ability Special Behaviours Factory Builder")]
	public sealed class AbilitySpecialBehavioursFactoryBuilder : PrototypeBasedFactoryBuilder<AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData, AbilityModifiersController>
	{
		// Token: 0x170006DB RID: 1755
		// (get) Token: 0x060021EC RID: 8684 RVA: 0x00069C1D File Offset: 0x00067E1D
		// (set) Token: 0x060021ED RID: 8685 RVA: 0x00069C2A File Offset: 0x00067E2A
		public override AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData[] FactoryData
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

		// Token: 0x060021EE RID: 8686 RVA: 0x00069C38 File Offset: 0x00067E38
		protected override PrototypeBasedFactory<AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData, AbilityModifiersController> CreateFactoryInternal()
		{
			return new AbilitySpecialBehavioursFactory();
		}

		// Token: 0x04001569 RID: 5481
		[SerializeField]
		[CustomReorderableList]
		private ReorderableListAdapter<AbilitySpecialBehavioursFactory.SpecialBehaviourPrototypeData[]> factoryData;
	}
}
