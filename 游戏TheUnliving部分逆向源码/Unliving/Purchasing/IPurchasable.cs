using System;
using System.Collections.Generic;
using Common;
using Game.Core;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.LeveledItems;

namespace Unliving.Purchasing
{
	// Token: 0x020000DD RID: 221
	public interface IPurchasable : ICloneable<IPurchasable>, ILeveledItem, IItemLevelProvider, IEquatable<IPurchasable>
	{
		// Token: 0x170000C6 RID: 198
		// (get) Token: 0x06000579 RID: 1401
		object ObjectID { get; }

		// Token: 0x170000C7 RID: 199
		// (get) Token: 0x0600057A RID: 1402
		bool Locked { get; }

		// Token: 0x170000C8 RID: 200
		// (get) Token: 0x0600057B RID: 1403
		bool Purchased { get; }

		// Token: 0x170000C9 RID: 201
		// (get) Token: 0x0600057C RID: 1404
		// (set) Token: 0x0600057D RID: 1405
		CurrencyOperationArgs UnlockArgs { get; set; }

		// Token: 0x170000CA RID: 202
		// (get) Token: 0x0600057E RID: 1406
		// (set) Token: 0x0600057F RID: 1407
		CurrencyOperationArgs BuyArgs { get; set; }

		// Token: 0x170000CB RID: 203
		// (get) Token: 0x06000580 RID: 1408
		// (set) Token: 0x06000581 RID: 1409
		List<IPurchasable> Parents { get; set; }

		// Token: 0x170000CC RID: 204
		// (get) Token: 0x06000582 RID: 1410
		IPurchasableUnlockTrigger Trigger { get; }

		// Token: 0x06000583 RID: 1411
		void SetGameData(IGame currentGame);

		// Token: 0x06000584 RID: 1412
		bool TryPurchase(MultiRepresentationObjectInstantiator.ObjectType pickingContext);

		// Token: 0x06000585 RID: 1413
		bool CanBePurchased(MultiRepresentationObjectInstantiator.ObjectType pickingContext);

		// Token: 0x06000586 RID: 1414
		void ChangePurchaseState(bool state);

		// Token: 0x06000587 RID: 1415
		IPurchasableData GetPurchasableData();

		// Token: 0x06000588 RID: 1416
		void SetPurchasableData(IPurchasableData data);
	}
}
