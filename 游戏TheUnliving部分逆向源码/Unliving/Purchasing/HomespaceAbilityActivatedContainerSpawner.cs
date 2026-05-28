using System;
using Game.Factories;
using Unliving.Abilities;
using Unliving.Factories;
using Unliving.Mobs;

namespace Unliving.Purchasing
{
	// Token: 0x020000F8 RID: 248
	public class HomespaceAbilityActivatedContainerSpawner : HomespaceShopObjectSpawner<IAbilityActivatedContainersFactory>
	{
		// Token: 0x170000E9 RID: 233
		// (get) Token: 0x060005F9 RID: 1529 RVA: 0x00014B7A File Offset: 0x00012D7A
		// (set) Token: 0x060005FA RID: 1530 RVA: 0x00014B87 File Offset: 0x00012D87
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

		// Token: 0x170000EA RID: 234
		// (get) Token: 0x060005FB RID: 1531 RVA: 0x00014B9B File Offset: 0x00012D9B
		public override Type PurchasableItemNodeType
		{
			get
			{
				return typeof(PurchasableAbilityActivatedContainerNode);
			}
		}

		// Token: 0x060005FC RID: 1532 RVA: 0x00014BA7 File Offset: 0x00012DA7
		public override bool HasDefaultID()
		{
			return this.abilityID == AbilityID.None;
		}

		// Token: 0x060005FD RID: 1533 RVA: 0x00014BB2 File Offset: 0x00012DB2
		public override MultiRepresentationObjectInstantiator.IArgs CreateFactoryQueryArgs()
		{
			return new AbilityFactoryArgs
			{
				abilityID = this.abilityID,
				objectType = MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject,
				spawnPosition = base.transform.position
			};
		}

		// Token: 0x040003F0 RID: 1008
		[ObjectFactoryIDPopup(typeof(AbilityActivatedContainer))]
		public AbilityID abilityID;
	}
}
