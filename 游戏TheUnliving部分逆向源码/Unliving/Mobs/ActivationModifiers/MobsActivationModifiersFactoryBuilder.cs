using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Mobs.ActivationModifiers
{
	// Token: 0x02000224 RID: 548
	[CreateAssetMenu(fileName = "MobsActivationModifiersFactoryBuilder", menuName = "Game/Factories/Mobs Activation Modifiers Factory Builder")]
	public sealed class MobsActivationModifiersFactoryBuilder : PrototypeBasedFactoryBuilder<MobsActivationModifiersFactory.PrototypeInfo, MobActivationAbilityModifier>
	{
		// Token: 0x170003F4 RID: 1012
		// (get) Token: 0x060012D1 RID: 4817 RVA: 0x0003BDB3 File Offset: 0x00039FB3
		// (set) Token: 0x060012D2 RID: 4818 RVA: 0x0003BDC0 File Offset: 0x00039FC0
		public override MobsActivationModifiersFactory.PrototypeInfo[] FactoryData
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

		// Token: 0x060012D3 RID: 4819 RVA: 0x0003BDCE File Offset: 0x00039FCE
		protected override PrototypeBasedFactory<MobsActivationModifiersFactory.PrototypeInfo, MobActivationAbilityModifier> CreateFactoryInternal()
		{
			return new MobsActivationModifiersFactory
			{
				defaultModifiersData = this.defaultModifiersData
			};
		}

		// Token: 0x04000B0E RID: 2830
		public MultiRepresentationObjectInstantiator.DefaultData defaultModifiersData;

		// Token: 0x04000B0F RID: 2831
		[SerializeField]
		[CustomReorderableList(null, "modifierPrototype", true, true)]
		private ReorderableListAdapter<MobsActivationModifiersFactory.PrototypeInfo[]> factoryData;
	}
}
