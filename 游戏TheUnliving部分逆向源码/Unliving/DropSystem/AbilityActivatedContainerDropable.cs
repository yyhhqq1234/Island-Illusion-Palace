using System;
using Common.Editor;
using Unliving.Abilities;
using Unliving.Factories;

namespace Unliving.DropSystem
{
	// Token: 0x0200028C RID: 652
	[Serializable]
	public class AbilityActivatedContainerDropable : DropableBase<AbilityID>
	{
		// Token: 0x170004D5 RID: 1237
		// (get) Token: 0x06001693 RID: 5779 RVA: 0x00048893 File Offset: 0x00046A93
		public override Type FactoryType
		{
			get
			{
				return typeof(IAbilityActivatedContainersFactory);
			}
		}

		// Token: 0x170004D6 RID: 1238
		// (get) Token: 0x06001694 RID: 5780 RVA: 0x0004889F File Offset: 0x00046A9F
		public override AbilityID ID
		{
			get
			{
				return this.abilityID;
			}
		}

		// Token: 0x06001695 RID: 5781 RVA: 0x000488A7 File Offset: 0x00046AA7
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			return new AbilityFactoryArgs
			{
				abilityID = this.ID
			};
		}

		// Token: 0x04000D1B RID: 3355
		[EnumPopup]
		public AbilityID abilityID;
	}
}
