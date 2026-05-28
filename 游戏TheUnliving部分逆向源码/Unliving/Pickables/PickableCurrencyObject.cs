using System;
using Common;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Factories;

namespace Unliving.Pickables
{
	// Token: 0x02000181 RID: 385
	public class PickableCurrencyObject : NonFactoryPickableBase, IAmountBased
	{
		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06000AB2 RID: 2738 RVA: 0x000234C6 File Offset: 0x000216C6
		// (set) Token: 0x06000AB3 RID: 2739 RVA: 0x000234D3 File Offset: 0x000216D3
		public float Amount
		{
			get
			{
				return this.reward.amount;
			}
			set
			{
				this.reward.amount = (float)((int)value);
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06000AB4 RID: 2740 RVA: 0x000234E3 File Offset: 0x000216E3
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.Gold;
			}
		}

		// Token: 0x06000AB5 RID: 2741 RVA: 0x000234E6 File Offset: 0x000216E6
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000AB6 RID: 2742 RVA: 0x000234E9 File Offset: 0x000216E9
		protected override void OnCollectorEnteredRange(IPickableObjectCollector collector)
		{
			base.OnCollectorEnteredRange(collector);
			if (collector == this.PointerEventsSender && this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.PickableObject)
			{
				base.PickupByPointerEventsSender(true, false);
			}
		}

		// Token: 0x06000AB7 RID: 2743 RVA: 0x0002350C File Offset: 0x0002170C
		public override void OnAnimationEventFired(string eventArg)
		{
			base.OnAnimationEventFired(eventArg);
			if (eventArg == "apply_collect")
			{
				this.OnObjectCollected();
				return;
			}
			if (eventArg == "destroy")
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x06000AB8 RID: 2744 RVA: 0x00023544 File Offset: 0x00021744
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.currentCollector == this.PointerEventsSender)
			{
				ICurrencyOperationPerformer currencyOperationPerformer = this.currentCollector.Component.CastOrGetComponent<ICurrencyOperationPerformer>();
				if (!currencyOperationPerformer.IsNull())
				{
					this.reward.sender = base.gameObject;
					currencyOperationPerformer.PerformCurrencyOperation(this.reward);
				}
			}
		}

		// Token: 0x04000632 RID: 1586
		private const string APPLY_COLLECT_ANIM_ARG = "apply_collect";

		// Token: 0x04000633 RID: 1587
		private const string DESTROY_ANIM_ARG = "destroy";

		// Token: 0x04000634 RID: 1588
		public CurrencyOperationArgs reward;
	}
}
