using System;
using Unliving.Factories;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x02000192 RID: 402
	public interface IPurchasableObject : IPickableObject
	{
		// Token: 0x14000075 RID: 117
		// (add) Token: 0x06000B60 RID: 2912
		// (remove) Token: 0x06000B61 RID: 2913
		event Action<IPurchasableObject, IPickableObjectCollector> PurchaseFailed;

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06000B62 RID: 2914
		bool IsPurchasable { get; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06000B63 RID: 2915
		bool CanBePickedInHomespace { get; }

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06000B64 RID: 2916
		// (set) Token: 0x06000B65 RID: 2917
		IPurchasable PurchasableData { get; set; }

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06000B66 RID: 2918
		// (set) Token: 0x06000B67 RID: 2919
		MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext { get; set; }

		// Token: 0x06000B68 RID: 2920
		void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data);
	}
}
