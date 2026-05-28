using System;
using Common.Factories;
using Game.Abilities;
using Game.Factories;
using Unliving.Abilities;
using Unliving.Factories;

namespace Unliving.Purchasing
{
	// Token: 0x020000F9 RID: 249
	public class HomespaceAbilitySpawner : HomespaceShopObjectSpawner<IObjectFactory<BaseAbility>>
	{
		// Token: 0x170000EB RID: 235
		// (get) Token: 0x060005FF RID: 1535 RVA: 0x00014BEA File Offset: 0x00012DEA
		// (set) Token: 0x06000600 RID: 1536 RVA: 0x00014BF7 File Offset: 0x00012DF7
		public override object ID
		{
			get
			{
				return this.abilityID;
			}
			set
			{
				this.abilityID = (AbilityID)value;
				base.UpdateSpawnerName();
			}
		}

		// Token: 0x170000EC RID: 236
		// (get) Token: 0x06000601 RID: 1537 RVA: 0x00014C0B File Offset: 0x00012E0B
		public override Type PurchasableItemNodeType
		{
			get
			{
				return typeof(PurchasableAbilityNode);
			}
		}

		// Token: 0x06000602 RID: 1538 RVA: 0x00014C17 File Offset: 0x00012E17
		public override bool HasDefaultID()
		{
			return this.abilityID == AbilityID.None;
		}

		// Token: 0x06000603 RID: 1539 RVA: 0x00014C22 File Offset: 0x00012E22
		public override MultiRepresentationObjectInstantiator.IArgs CreateFactoryQueryArgs()
		{
			return new AbilityFactoryArgs
			{
				abilityID = this.abilityID,
				objectType = MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject,
				spawnPosition = base.transform.position
			};
		}

		// Token: 0x040003F1 RID: 1009
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID abilityID;
	}
}
