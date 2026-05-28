using System;
using Common;
using Game.Damage;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Pickables
{
	// Token: 0x0200018D RID: 397
	public class HealthPack : NonFactoryPickableBase, IAmountBased
	{
		// Token: 0x170001F0 RID: 496
		// (get) Token: 0x06000B2F RID: 2863 RVA: 0x000247B5 File Offset: 0x000229B5
		// (set) Token: 0x06000B30 RID: 2864 RVA: 0x000247C2 File Offset: 0x000229C2
		public float Amount
		{
			get
			{
				return this.HealthRestoringArgs.amount;
			}
			set
			{
				this.HealthRestoringArgs.amount = value;
			}
		}

		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06000B31 RID: 2865 RVA: 0x000247D0 File Offset: 0x000229D0
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.HealthPack;
			}
		}

		// Token: 0x06000B32 RID: 2866 RVA: 0x000247D3 File Offset: 0x000229D3
		protected override void Start()
		{
			base.Start();
			this.HealthRestoringArgs.isForcedChanging = true;
		}

		// Token: 0x06000B33 RID: 2867 RVA: 0x000247E7 File Offset: 0x000229E7
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000B34 RID: 2868 RVA: 0x000247EA File Offset: 0x000229EA
		protected override void OnCollectorEnteredRange(IPickableObjectCollector collector)
		{
			base.OnCollectorEnteredRange(collector);
			if (collector == this.PointerEventsSender && this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.PickableObject)
			{
				base.PickupByPointerEventsSender(true, false);
			}
		}

		// Token: 0x06000B35 RID: 2869 RVA: 0x00024810 File Offset: 0x00022A10
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.currentCollector == this.PointerEventsSender)
			{
				IDamageable component = this.currentCollector.Component.GetComponent<IDamageable>();
				if (component.HitPointsLack > 0f)
				{
					component.ModifyHitPoints(component.Behaviour, this.HealthRestoringArgs);
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x0400065C RID: 1628
		public HitPointsController.HPChangingArgs HealthRestoringArgs;
	}
}
