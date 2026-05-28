using System;
using Common.Editor;
using Common.Factories;
using Unliving.Factories;
using Unliving.PassiveAbilities;

namespace Unliving.DropSystem
{
	// Token: 0x02000292 RID: 658
	[Serializable]
	public class PassiveAbilityDropable : DropableBase<PassiveAbilityID>
	{
		// Token: 0x170004E8 RID: 1256
		// (get) Token: 0x060016BA RID: 5818 RVA: 0x00048A66 File Offset: 0x00046C66
		public override Type FactoryType
		{
			get
			{
				return typeof(IObjectFactory<PassiveAbilityBase>);
			}
		}

		// Token: 0x170004E9 RID: 1257
		// (get) Token: 0x060016BB RID: 5819 RVA: 0x00048A72 File Offset: 0x00046C72
		public override PassiveAbilityID ID
		{
			get
			{
				return this.abilityID;
			}
		}

		// Token: 0x060016BC RID: 5820 RVA: 0x00048A7A File Offset: 0x00046C7A
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			return new PassiveAbilityFactoryQuery
			{
				abilityID = this.ID
			};
		}

		// Token: 0x04000D27 RID: 3367
		[EnumPopup]
		public PassiveAbilityID abilityID;
	}
}
