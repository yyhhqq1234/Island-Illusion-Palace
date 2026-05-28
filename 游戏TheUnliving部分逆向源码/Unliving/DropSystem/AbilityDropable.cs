using System;
using Common.Factories;
using Game.Abilities;
using Game.Factories;
using Unliving.Abilities;
using Unliving.Factories;

namespace Unliving.DropSystem
{
	// Token: 0x0200028D RID: 653
	[Serializable]
	public class AbilityDropable : DropableBase<AbilityID>
	{
		// Token: 0x170004D7 RID: 1239
		// (get) Token: 0x06001697 RID: 5783 RVA: 0x000488C2 File Offset: 0x00046AC2
		public override Type FactoryType
		{
			get
			{
				return typeof(IObjectFactory<BaseAbility>);
			}
		}

		// Token: 0x170004D8 RID: 1240
		// (get) Token: 0x06001698 RID: 5784 RVA: 0x000488CE File Offset: 0x00046ACE
		public override AbilityID ID
		{
			get
			{
				return this.abilityID;
			}
		}

		// Token: 0x06001699 RID: 5785 RVA: 0x000488D8 File Offset: 0x00046AD8
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			AbilityFactoryArgs abilityFactoryArgs = this.queryOverride as AbilityFactoryArgs;
			if (abilityFactoryArgs != null)
			{
				abilityFactoryArgs.abilityID = this.ID;
				return this.queryOverride;
			}
			return new AbilityFactoryArgs
			{
				abilityID = this.ID
			};
		}

		// Token: 0x04000D1C RID: 3356
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID abilityID;
	}
}
