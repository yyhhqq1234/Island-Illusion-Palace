using System;
using System.Collections;
using Common.Editor;
using Game.Localization;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.PlayerProfileManagement;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x0200018B RID: 395
	public class CurrencyExchangeObject : PickableBase
	{
		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06000B16 RID: 2838 RVA: 0x00024458 File Offset: 0x00022658
		public override PickableObjectData PickableObjectData
		{
			get
			{
				CurrencyExchangeObject.Tier currentExchangeTier = this.GetCurrentExchangeTier();
				PickableObjectData pickableObjectData = new PickableObjectData
				{
					metadata = this.metadata.Clone()
				};
				pickableObjectData.metadata.Description = string.Format(pickableObjectData.metadata.Description, currentExchangeTier.fromArgs.Amount, currentExchangeTier.toArgs.Amount);
				return pickableObjectData;
			}
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06000B17 RID: 2839 RVA: 0x000244BF File Offset: 0x000226BF
		protected override string LocalizationID
		{
			get
			{
				return string.Empty;
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06000B18 RID: 2840 RVA: 0x000244C6 File Offset: 0x000226C6
		protected override IPickingSettings PickupSettings
		{
			get
			{
				return this.pickingSettings;
			}
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06000B19 RID: 2841 RVA: 0x000244CE File Offset: 0x000226CE
		public bool IsPurchasable
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170001EB RID: 491
		// (get) Token: 0x06000B1A RID: 2842 RVA: 0x000244D1 File Offset: 0x000226D1
		public bool CanBePickedInHomespace
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06000B1B RID: 2843 RVA: 0x000244D4 File Offset: 0x000226D4
		public IPurchasable PurchasableData
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06000B1C RID: 2844 RVA: 0x000244D7 File Offset: 0x000226D7
		public MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext
		{
			get
			{
				return this.pickingContext;
			}
		}

		// Token: 0x1400006C RID: 108
		// (add) Token: 0x06000B1D RID: 2845 RVA: 0x000244E0 File Offset: 0x000226E0
		// (remove) Token: 0x06000B1E RID: 2846 RVA: 0x00024518 File Offset: 0x00022718
		public event Action DataChanged;

		// Token: 0x06000B1F RID: 2847 RVA: 0x00024550 File Offset: 0x00022750
		private void Start()
		{
			if (base.CurrentGame.Services.TryGet<LocalizationManager>(out this.localizationManager))
			{
				this.metadata = this.localizationManager.GetMetadata(this.metadataKey, Array.Empty<string>());
			}
			PlayerProfileManager playerProfileManager;
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				this.playerProfile = playerProfileManager.CurrentPlayerProfile;
				this.playerProfile.CurrencyOperationSucceed += this.OnCurrencyOperationSucceed;
			}
		}

		// Token: 0x06000B20 RID: 2848 RVA: 0x000245C8 File Offset: 0x000227C8
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000B21 RID: 2849 RVA: 0x000245CC File Offset: 0x000227CC
		public override bool CanBePickedUp(IPickableObjectCollector targetCollector, PickingArgs args, out PickingUpErrorType error)
		{
			if (base.CanBePickedUp(targetCollector, args, out error))
			{
				if (this.IsEnoughCurrencyForCurrentTier())
				{
					return true;
				}
				CurrencyID currencyID = this.GetCurrentExchangeTier().fromArgs.CurrencyID;
				if (currencyID == CurrencyID.Ash)
				{
					error = PickingUpErrorType.NotEnoughAsh;
				}
				else if (currencyID == CurrencyID.Gold)
				{
					error = PickingUpErrorType.NotEnoughGold;
				}
				else if (currencyID == CurrencyID.Meta)
				{
					error = PickingUpErrorType.NotEnoughMetaCurrency;
				}
				else if (currencyID == CurrencyID.Prima)
				{
					error = PickingUpErrorType.NotEnoughPrima;
				}
			}
			return false;
		}

		// Token: 0x06000B22 RID: 2850 RVA: 0x00024624 File Offset: 0x00022824
		public bool IsEnoughCurrencyForCurrentTier()
		{
			CurrencyExchangeObject.Tier currentExchangeTier = this.GetCurrentExchangeTier();
			return (float)this.playerProfile.GetCurrencyAmount(currentExchangeTier.fromArgs.CurrencyID) >= currentExchangeTier.fromArgs.Amount;
		}

		// Token: 0x06000B23 RID: 2851 RVA: 0x00024660 File Offset: 0x00022860
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			CurrencyExchangeObject.Tier currentExchangeTier = this.GetCurrentExchangeTier();
			if (this.playerProfile.TryExecuteCurrencyOperation(currentExchangeTier.fromArgs))
			{
				this.playerProfile.TryExecuteCurrencyOperation(currentExchangeTier.toArgs);
				this.currentTierIndex++;
				Action dataChanged = this.DataChanged;
				if (dataChanged != null)
				{
					dataChanged();
				}
			}
			base.StartCoroutine(this.EnableColliderRoutine());
		}

		// Token: 0x06000B24 RID: 2852 RVA: 0x000246CF File Offset: 0x000228CF
		private void OnCurrencyOperationSucceed(ICurrencyOperationArgs args)
		{
			Action dataChanged = this.DataChanged;
			if (dataChanged == null)
			{
				return;
			}
			dataChanged();
		}

		// Token: 0x06000B25 RID: 2853 RVA: 0x000246E1 File Offset: 0x000228E1
		private CurrencyExchangeObject.Tier GetCurrentExchangeTier()
		{
			this.currentTierIndex = Mathf.Clamp(this.currentTierIndex, 0, this.exchangeTiers.Length - 1);
			return this.exchangeTiers[this.currentTierIndex];
		}

		// Token: 0x06000B26 RID: 2854 RVA: 0x0002470C File Offset: 0x0002290C
		private IEnumerator EnableColliderRoutine()
		{
			yield return new WaitForSeconds(0.5f);
			this.collider2D.enabled = true;
			yield break;
		}

		// Token: 0x06000B27 RID: 2855 RVA: 0x0002471B File Offset: 0x0002291B
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.playerProfile != null)
			{
				this.playerProfile.CurrencyOperationSucceed -= this.OnCurrencyOperationSucceed;
			}
		}

		// Token: 0x04000651 RID: 1617
		[SerializeField]
		private string metadataKey;

		// Token: 0x04000652 RID: 1618
		[SerializeReference]
		[ManagedObjectField(typeof(IPickingSettings))]
		private IPickingSettings pickingSettings;

		// Token: 0x04000653 RID: 1619
		[SerializeField]
		private CurrencyExchangeObject.Tier[] exchangeTiers;

		// Token: 0x04000654 RID: 1620
		[SerializeField]
		private MultiRepresentationObjectInstantiator.ObjectType pickingContext;

		// Token: 0x04000655 RID: 1621
		private int currentTierIndex = -1;

		// Token: 0x04000656 RID: 1622
		private PlayerProfile playerProfile;

		// Token: 0x04000657 RID: 1623
		private LocalizationManager localizationManager;

		// Token: 0x04000658 RID: 1624
		private Metadata metadata;

		// Token: 0x02000476 RID: 1142
		[Serializable]
		public class Tier
		{
			// Token: 0x0400176B RID: 5995
			public CurrencyOperationArgs fromArgs;

			// Token: 0x0400176C RID: 5996
			public CurrencyOperationArgs toArgs;
		}
	}
}
