using System;
using Common.ServiceRegistry;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Currencies;
using Unliving.Purchasing;

namespace Unliving.Stores
{
	// Token: 0x02000049 RID: 73
	[Service(typeof(AbilitySpecialBehaviourStoreManager), new Type[]
	{

	})]
	[CreateAssetMenu(fileName = "AbilitySpecialBehaviourStoreManager", menuName = "Game/Global/Ability Special Behaviour Store Manager")]
	public sealed class AbilitySpecialBehaviourStoreManager : StoreManagerBase
	{
		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600024B RID: 587 RVA: 0x00009917 File Offset: 0x00007B17
		public override CurrencyID CurrencyID
		{
			get
			{
				return CurrencyID.Prima;
			}
		}

		// Token: 0x0600024C RID: 588 RVA: 0x0000991C File Offset: 0x00007B1C
		private bool IsAvailableSpecialBehaviour(AbilitySpecialBehaviourGenerationOption option)
		{
			AbilitySpecialBehaviourID behaviourID = option.behaviourID;
			PurchaseManager purchaseManager;
			IPurchasable purchasable;
			return base.CurrentGame.Services.TryGet<PurchaseManager>(out purchaseManager) && purchaseManager.TryGetPurchasable(behaviourID, out purchasable) && purchasable.Purchased;
		}

		// Token: 0x0600024D RID: 589 RVA: 0x0000995C File Offset: 0x00007B5C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			IGameAbilitiesFactory gameAbilitiesFactory;
			if (currentGame.Services.TryGet<IGameAbilitiesFactory>(out gameAbilitiesFactory))
			{
				gameAbilitiesFactory.SetAbilitySpecialBehaviourFilterPredicate(new Predicate<AbilitySpecialBehaviourGenerationOption>(this.IsAvailableSpecialBehaviour));
			}
		}
	}
}
