using System;
using Common.Factories;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A2 RID: 418
	[Service(typeof(PassiveAbilityFactory), new Type[]
	{
		typeof(IObjectFactory<PassiveAbilityBase>),
		typeof(IFactory<int, PassiveAbilityBase>)
	})]
	public sealed class PassiveAbilityFactory : UnityObjectPrototypeBasedFactory<PassiveAbilityFactoryPrototype, PassiveAbilityBase>
	{
		// Token: 0x06000BF0 RID: 3056 RVA: 0x00025C40 File Offset: 0x00023E40
		private void OnMultiRepresentationObjectCreated(UnityEngine.Object createdObject, MultiRepresentationObjectInstantiator.IObjectData objectData, MultiRepresentationObjectInstantiator.IArgs args)
		{
			IPurchasableObject purchasableObject = createdObject.CastOrGetComponent<IPurchasableObject>();
			if (purchasableObject != null)
			{
				purchasableObject.CurrentPickingContext = args.Type;
				purchasableObject.InitializeData(args, objectData);
			}
		}

		// Token: 0x06000BF1 RID: 3057 RVA: 0x00025C6C File Offset: 0x00023E6C
		public override object Create(object query)
		{
			if (this.representationsInstantiator == null)
			{
				this.representationsInstantiator = new MultiRepresentationObjectInstantiator(new Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData>(base.GetObjectPrototype), this.defaultAbilitiesData, new Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs>(this.OnMultiRepresentationObjectCreated));
			}
			return this.representationsInstantiator.CreateObject<PassiveAbilityID>(query, new Func<object, object>(base.Create));
		}

		// Token: 0x06000BF2 RID: 3058 RVA: 0x00025CC2 File Offset: 0x00023EC2
		protected override void OnObjectCreated(PassiveAbilityFactoryPrototype objectPrototype, PassiveAbilityBase objectInstance, IBaseObjectDescription query)
		{
			objectInstance.ID = objectPrototype.abilityID;
		}

		// Token: 0x040006A3 RID: 1699
		public const int NullAbilityID = -1;

		// Token: 0x040006A4 RID: 1700
		public MultiRepresentationObjectInstantiator.DefaultData defaultAbilitiesData;

		// Token: 0x040006A5 RID: 1701
		public GameObject commonHomespacePrefab;

		// Token: 0x040006A6 RID: 1702
		public GameObject commonWorldObjectPrefab;

		// Token: 0x040006A7 RID: 1703
		public GameObject commonStorePrefab;

		// Token: 0x040006A8 RID: 1704
		private MultiRepresentationObjectInstantiator representationsInstantiator;
	}
}
