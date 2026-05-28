using System;
using Common.Editor;
using Common.Factories;
using Unliving.Factories;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.DropSystem
{
	// Token: 0x0200028E RID: 654
	[Serializable]
	public class ActivationModifierDropable : DropableBase<MobActivationModifierID>
	{
		// Token: 0x170004D9 RID: 1241
		// (get) Token: 0x0600169B RID: 5787 RVA: 0x00048920 File Offset: 0x00046B20
		public override Type FactoryType
		{
			get
			{
				return typeof(IObjectFactory<MobActivationAbilityModifier>);
			}
		}

		// Token: 0x170004DA RID: 1242
		// (get) Token: 0x0600169C RID: 5788 RVA: 0x0004892C File Offset: 0x00046B2C
		public override MobActivationModifierID ID
		{
			get
			{
				return this.modifierID;
			}
		}

		// Token: 0x0600169D RID: 5789 RVA: 0x00048934 File Offset: 0x00046B34
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			return new MobsActivationModifiersFactory.Args
			{
				modifierID = this.modifierID
			};
		}

		// Token: 0x04000D1D RID: 3357
		[EnumPopup]
		public MobActivationModifierID modifierID;
	}
}
