using System;
using Common.Factories;
using Game.Factories;
using Unliving.Factories;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Purchasing
{
	// Token: 0x020000FA RID: 250
	public class HomespaceActivationModifiersSpawner : HomespaceShopObjectSpawner<IObjectFactory<MobActivationAbilityModifier>>
	{
		// Token: 0x170000ED RID: 237
		// (get) Token: 0x06000605 RID: 1541 RVA: 0x00014C5A File Offset: 0x00012E5A
		// (set) Token: 0x06000606 RID: 1542 RVA: 0x00014C67 File Offset: 0x00012E67
		public override object ID
		{
			get
			{
				return this.modifierID;
			}
			set
			{
				this.modifierID = (MobActivationModifierID)value;
				base.UpdateSpawnerName();
			}
		}

		// Token: 0x170000EE RID: 238
		// (get) Token: 0x06000607 RID: 1543 RVA: 0x00014C7B File Offset: 0x00012E7B
		public override Type PurchasableItemNodeType
		{
			get
			{
				return typeof(PurchasableModifierNode);
			}
		}

		// Token: 0x06000608 RID: 1544 RVA: 0x00014C87 File Offset: 0x00012E87
		public override bool HasDefaultID()
		{
			return this.modifierID == MobActivationModifierID.None;
		}

		// Token: 0x06000609 RID: 1545 RVA: 0x00014C92 File Offset: 0x00012E92
		public override MultiRepresentationObjectInstantiator.IArgs CreateFactoryQueryArgs()
		{
			return new MobsActivationModifiersFactory.Args
			{
				modifierID = this.modifierID,
				objectType = MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject,
				spawnPosition = base.transform.position
			};
		}

		// Token: 0x040003F2 RID: 1010
		[ObjectFactoryIDPopup(typeof(MobActivationAbilityModifier))]
		public MobActivationModifierID modifierID;
	}
}
