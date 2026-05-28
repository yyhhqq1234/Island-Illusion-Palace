using System;
using System.Collections;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Currencies;
using Unliving.LeveledItems;
using Unliving.Mobs;
using Unliving.Purchasing;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000169 RID: 361
	[CreateAssetMenu(fileName = "AdditionalHPContainersPlayerUpgrade", menuName = "Game/Player/Player Upgrade/Additional HP Containers Upgrade")]
	public class AdditionalHPContainersPlayerUpgrade : ScriptableObject, IPlayerUpgrade, ILeveledItem, IItemLevelProvider
	{
		// Token: 0x17000194 RID: 404
		// (get) Token: 0x060009F8 RID: 2552 RVA: 0x00021DF7 File Offset: 0x0001FFF7
		// (set) Token: 0x060009F9 RID: 2553 RVA: 0x00021DFF File Offset: 0x0001FFFF
		public IPlayerUpgrade UpgradePrototype { get; private set; }

		// Token: 0x17000195 RID: 405
		// (get) Token: 0x060009FA RID: 2554 RVA: 0x00021E08 File Offset: 0x00020008
		// (set) Token: 0x060009FB RID: 2555 RVA: 0x00021E10 File Offset: 0x00020010
		public int ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
			set
			{
			}
		}

		// Token: 0x17000196 RID: 406
		// (get) Token: 0x060009FC RID: 2556 RVA: 0x00021E12 File Offset: 0x00020012
		// (set) Token: 0x060009FD RID: 2557 RVA: 0x00021E1A File Offset: 0x0002001A
		public bool IsActivated { get; private set; }

		// Token: 0x17000197 RID: 407
		// (get) Token: 0x060009FE RID: 2558 RVA: 0x00021E23 File Offset: 0x00020023
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.itemLevel;
			}
		}

		// Token: 0x060009FF RID: 2559 RVA: 0x00021E2B File Offset: 0x0002002B
		public IEnumerator Activate(IPlayerUpgradesRegistry upgradesRegistry, object activationContext)
		{
			if (this.IsActivated)
			{
				yield break;
			}
			this.IsActivated = true;
			yield return null;
			this.currentGame = (activationContext as IGame);
			GameManager gameManager;
			if (this.currentGame.Services.TryGet<GameManager>(out gameManager) && !gameManager.IsStartSceneLoaded)
			{
				yield break;
			}
			IPlayerProvider playerProvider;
			if (this.currentGame != null && this.currentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				IDamageable damageable = (currentPlayer != null) ? currentPlayer.HitPointsController : null;
				IContainerBasedHPController containerBasedHPController = damageable as IContainerBasedHPController;
				if (containerBasedHPController != null)
				{
					IAbilityActivatedContainersController abilityActivatedContainersController = damageable as IAbilityActivatedContainersController;
					if (abilityActivatedContainersController != null)
					{
						PurchaseManager purchaseManager;
						this.currentGame.Services.TryGet<PurchaseManager>(out purchaseManager);
						for (int i = 0; i < this.additionalContainersAbilities.Length; i++)
						{
							containerBasedHPController.AddContainerSlot();
							HealthContainer container = new HealthContainer(containerBasedHPController.InitialHitPoints);
							containerBasedHPController.AddContainer(container);
							AbilityID abilityID = this.additionalContainersAbilities[i];
							IPurchasable purchasable;
							if (purchaseManager.TryGetPurchasable(abilityID, out purchasable))
							{
								this.itemLevel = purchasable.ItemLevel;
								AbilityInfo abilityInfo = new AbilityInfo
								{
									abilityID = abilityID,
									abilityLevel = this.itemLevel
								};
								CurrencyOperationArgs destructionRewardArgs = (purchasable as PurchasableItemAbilityActivatedContainer).destructionRewardArgs;
								abilityActivatedContainersController.AddContainer(abilityInfo, destructionRewardArgs);
							}
						}
					}
				}
			}
			yield break;
		}

		// Token: 0x06000A00 RID: 2560 RVA: 0x00021E41 File Offset: 0x00020041
		public IPlayerUpgrade Clone()
		{
			AdditionalHPContainersPlayerUpgrade additionalHPContainersPlayerUpgrade = (AdditionalHPContainersPlayerUpgrade)base.MemberwiseClone();
			additionalHPContainersPlayerUpgrade.UpgradePrototype = (this.UpgradePrototype ?? this);
			return additionalHPContainersPlayerUpgrade;
		}

		// Token: 0x040005E9 RID: 1513
		public AbilityID[] additionalContainersAbilities;

		// Token: 0x040005EA RID: 1514
		private IGame currentGame;

		// Token: 0x040005EB RID: 1515
		private int itemLevel;
	}
}
