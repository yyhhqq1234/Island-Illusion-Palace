using System;
using Common;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.Factories;
using Unliving.Mobs;

namespace Unliving.Pickables
{
	// Token: 0x0200019F RID: 415
	public class VitalEnergyPack : NonFactoryPickableBase, IAmountBased
	{
		// Token: 0x17000214 RID: 532
		// (get) Token: 0x06000BDB RID: 3035 RVA: 0x000258DF File Offset: 0x00023ADF
		// (set) Token: 0x06000BDC RID: 3036 RVA: 0x000258EC File Offset: 0x00023AEC
		public float Amount
		{
			get
			{
				return this.restoringArgs.amount;
			}
			set
			{
				this.restoringArgs.amount = value;
			}
		}

		// Token: 0x17000215 RID: 533
		// (get) Token: 0x06000BDD RID: 3037 RVA: 0x000258FA File Offset: 0x00023AFA
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.VitalEnergyPack;
			}
		}

		// Token: 0x06000BDE RID: 3038 RVA: 0x000258FD File Offset: 0x00023AFD
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000BDF RID: 3039 RVA: 0x00025900 File Offset: 0x00023B00
		protected override void OnCollectorEnteredRange(IPickableObjectCollector collector)
		{
			base.OnCollectorEnteredRange(collector);
			if (collector == this.PointerEventsSender && this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.PickableObject)
			{
				base.PickupByPointerEventsSender(true, false);
			}
		}

		// Token: 0x06000BE0 RID: 3040 RVA: 0x00025924 File Offset: 0x00023B24
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			VitalEnergyHitPointsController vitalEnergyHitPointsController;
			if (this.currentCollector.Component.TryGetComponent<VitalEnergyHitPointsController>(out vitalEnergyHitPointsController))
			{
				vitalEnergyHitPointsController.ModifyHitPoints(vitalEnergyHitPointsController.Behaviour, this.restoringArgs);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}

		// Token: 0x06000BE1 RID: 3041 RVA: 0x0002596C File Offset: 0x00023B6C
		protected override void Start()
		{
			base.Start();
			this.restoringArgs.isForcedChanging = true;
			GatheringMotionComponent gatheringMotionComponent;
			if (base.TryGetComponent<GatheringMotionComponent>(out gatheringMotionComponent))
			{
				gatheringMotionComponent.TryStartGatheringMotion(this.PointerEventsSender.Component.transform, 0f, null);
			}
		}

		// Token: 0x0400069A RID: 1690
		[SerializeField]
		private VitalEnergyHitPointsController.RestoreVitalEnergyArgs restoringArgs = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs();
	}
}
