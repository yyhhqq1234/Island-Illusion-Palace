using System;
using System.Collections;
using Common;
using Game.Core;
using Game.Localization;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Currencies;
using Unliving.Factories;
using Unliving.Purchasing;

namespace Unliving.Pickables
{
	// Token: 0x02000188 RID: 392
	public abstract class PickableObjectBase<TObjectID> : PickableObjectBase where TObjectID : Enum
	{
		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06000AF6 RID: 2806 RVA: 0x00023DB8 File Offset: 0x00021FB8
		public override PickableObjectData PickableObjectData
		{
			get
			{
				if (this.pickableObjectData == null)
				{
					this.pickableObjectData = new PickableObjectData();
				}
				if (this.pickableObjectData.metadata == null)
				{
					IAmountBased amountBased = this as IAmountBased;
					Metadata metadata;
					if (amountBased != null)
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, new string[]
						{
							amountBased.Amount.ToString()
						});
					}
					else
					{
						metadata = this.localizationManager.GetMetadata(this.LocalizationID, Array.Empty<string>());
					}
					this.pickableObjectData.metadata = metadata;
				}
				return this.pickableObjectData;
			}
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06000AF7 RID: 2807
		public abstract TObjectID ID { get; }

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06000AF8 RID: 2808 RVA: 0x00023E44 File Offset: 0x00022044
		protected override string LocalizationID
		{
			get
			{
				string str = LocalizationManager.GetLocalizationPrefix(typeof(TObjectID)).ToString();
				TObjectID id = this.ID;
				return str + id.ToString();
			}
		}

		// Token: 0x06000AF9 RID: 2809 RVA: 0x00023E87 File Offset: 0x00022087
		protected virtual void TryCollectObject(IPickableObjectCollector targetCollector)
		{
			this.currentCollector = targetCollector;
			if (this.defferedObjectCollect)
			{
				this.OnObjectCollectionStarted();
				return;
			}
			this.OnObjectCollected();
		}

		// Token: 0x06000AFA RID: 2810 RVA: 0x00023EA8 File Offset: 0x000220A8
		protected virtual IPurchasable GetPurchasableData(TObjectID id)
		{
			if (this.purchaseManager == null)
			{
				Debug.LogError(string.Format("Purchasable with id: {0} is missing!", id));
				return null;
			}
			IPurchasable result;
			if (this.purchaseManager.TryGetPurchasable(id, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000AFB RID: 2811 RVA: 0x00023EF4 File Offset: 0x000220F4
		public override bool CanBePickedUp(IPickableObjectCollector targetCollector, PickingArgs args, out PickingUpErrorType error)
		{
			if (!base.CanBePickedUp(targetCollector, args, out error))
			{
				return false;
			}
			if (this.IsPurchasable && (base.PurchasableData == null || this.purchaseManager == null || !base.PurchasableData.CanBePurchased(this.CurrentPickingContext)))
			{
				CurrencyID currencyID;
				if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject)
				{
					currencyID = base.PurchasableData.BuyArgs.currencyID;
				}
				else
				{
					currencyID = base.PurchasableData.UnlockArgs.currencyID;
				}
				switch (currencyID)
				{
				case CurrencyID.Gold:
					error = PickingUpErrorType.NotEnoughGold;
					break;
				case CurrencyID.Meta:
					error = PickingUpErrorType.NotEnoughMetaCurrency;
					break;
				case CurrencyID.Prima:
					error = PickingUpErrorType.NotEnoughPrima;
					break;
				}
				return false;
			}
			return this.CurrentPickingContext != MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject || !base.PurchasableData.Purchased;
		}

		// Token: 0x06000AFC RID: 2812 RVA: 0x00023FB4 File Offset: 0x000221B4
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (this.IsPurchasable)
			{
				MultiRepresentationObjectInstantiator.ObjectType currentPickingContext = this.CurrentPickingContext;
				if (currentPickingContext != MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
				{
					if (currentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.StoreObject)
					{
						if (base.PurchasableData.Locked)
						{
							return;
						}
						if (!base.PurchasableData.TryPurchase(MultiRepresentationObjectInstantiator.ObjectType.StoreObject))
						{
							this.OnPurchaseFailed(targetCollector);
							return;
						}
						this.OnPurchased(targetCollector);
					}
				}
				else
				{
					if (base.PurchasableData.TryPurchase(MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject))
					{
						this.OnPurchased(targetCollector);
					}
					if (!base.CanBePickedInHomespace)
					{
						return;
					}
				}
			}
			this.TryCollectObject(targetCollector);
		}

		// Token: 0x06000AFD RID: 2813 RVA: 0x0002402C File Offset: 0x0002222C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<PurchaseManager>(out this.purchaseManager);
			if (this.autoCollectTimeout > 0f)
			{
				base.StartCoroutine(this.AutoCollectRoutine());
			}
		}

		// Token: 0x06000AFE RID: 2814 RVA: 0x00024061 File Offset: 0x00022261
		public override int CalculateItemLevel(out bool slotBusy, out bool hasSameLevel)
		{
			hasSameLevel = false;
			slotBusy = false;
			return 1;
		}

		// Token: 0x06000AFF RID: 2815 RVA: 0x0002406A File Offset: 0x0002226A
		private IEnumerator AutoCollectRoutine()
		{
			yield return new WaitForSeconds(this.autoCollectTimeout);
			base.PickupByPointerEventsSender(true, false);
			yield break;
		}

		// Token: 0x06000B00 RID: 2816 RVA: 0x0002407C File Offset: 0x0002227C
		public override void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			base.InitializeData(args, data);
			base.CanBePickedInHomespace = data.CanBePickedInHomespace;
			base.PurchasableData = this.GetPurchasableData((TObjectID)((object)data.ObjectID));
			IPurchasable purchasableData = base.PurchasableData;
			if (purchasableData == null)
			{
				return;
			}
			purchasableData.SetGameData(base.CurrentGame);
		}

		// Token: 0x06000B01 RID: 2817 RVA: 0x000240CF File Offset: 0x000222CF
		public override void OnPickUpFailed(IPickableObjectCollector collector, PickingUpErrorType error)
		{
			base.OnPickUpFailed(collector, error);
			this.OnPurchaseFailed(collector);
		}

		// Token: 0x04000646 RID: 1606
		[Tooltip("Время уничтожения")]
		[FormerlySerializedAs("DestroyTimeout")]
		public float autoCollectTimeout = -1f;

		// Token: 0x04000647 RID: 1607
		[Tooltip("Отсроченный подъем предмета, например можно вызвать ключом анимации")]
		public bool defferedObjectCollect;

		// Token: 0x04000648 RID: 1608
		protected PurchaseManager purchaseManager;
	}
}
