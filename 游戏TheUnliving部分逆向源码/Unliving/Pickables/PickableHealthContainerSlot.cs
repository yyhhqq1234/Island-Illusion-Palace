using System;
using Game.Damage;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Pickables
{
	// Token: 0x02000184 RID: 388
	public class PickableHealthContainerSlot : NonFactoryPickableBase
	{
		// Token: 0x170001D2 RID: 466
		// (get) Token: 0x06000AC1 RID: 2753 RVA: 0x0002364D File Offset: 0x0002184D
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.HealthContainerSlot;
			}
		}

		// Token: 0x06000AC2 RID: 2754 RVA: 0x00023650 File Offset: 0x00021850
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			IContainerBasedHPController component = targetCollector.Component.GetComponent<IContainerBasedHPController>();
			return component != null && component.CanAddContainerSlot();
		}

		// Token: 0x06000AC3 RID: 2755 RVA: 0x00023674 File Offset: 0x00021874
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return;
			}
			if (this.currentCollector == this.PointerEventsSender && this.currentCollector.Component.GetComponent<IContainerBasedHPController>().AddContainerSlot())
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
	}
}
