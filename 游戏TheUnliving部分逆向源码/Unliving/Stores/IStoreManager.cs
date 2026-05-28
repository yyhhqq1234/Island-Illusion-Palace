using System;

namespace Unliving.Stores
{
	// Token: 0x0200004C RID: 76
	public interface IStoreManager
	{
		// Token: 0x14000025 RID: 37
		// (add) Token: 0x06000259 RID: 601
		// (remove) Token: 0x0600025A RID: 602
		event Action<IStoreManager, bool> StoreStateChanged;

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x0600025B RID: 603
		IStoreInteractionSpot CurrentInteractionSpot { get; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x0600025C RID: 604
		bool IsStoreOpened { get; }

		// Token: 0x0600025D RID: 605
		void OpenStore(IStoreInteractionSpot interactionSpot);

		// Token: 0x0600025E RID: 606
		void CloseStore();
	}
}
