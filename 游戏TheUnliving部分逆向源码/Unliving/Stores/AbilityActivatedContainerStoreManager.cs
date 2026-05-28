using System;
using Common.ServiceRegistry;
using UnityEngine;
using Unliving.Currencies;

namespace Unliving.Stores
{
	// Token: 0x02000048 RID: 72
	[Service(typeof(AbilityActivatedContainerStoreManager), new Type[]
	{

	})]
	[CreateAssetMenu(fileName = "AbilityActivatedContainerStoreManager", menuName = "Game/Global/Ability Activated Container Store Manager")]
	public class AbilityActivatedContainerStoreManager : StoreManagerBase
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x06000249 RID: 585 RVA: 0x0000990C File Offset: 0x00007B0C
		public override CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Prima;
			}
		}
	}
}
