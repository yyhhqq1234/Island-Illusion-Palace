using System;
using Game.Damage;
using UnityEngine;
using Unliving.Factories;
using Unliving.Mobs;

namespace Unliving.Pickables
{
	// Token: 0x02000183 RID: 387
	public class PickableHealthContainer : NonFactoryPickableBase
	{
		// Token: 0x170001D1 RID: 465
		// (get) Token: 0x06000ABD RID: 2749 RVA: 0x000235BB File Offset: 0x000217BB
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.HealthContainer;
			}
		}

		// Token: 0x06000ABE RID: 2750 RVA: 0x000235BE File Offset: 0x000217BE
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}

		// Token: 0x06000ABF RID: 2751 RVA: 0x000235C4 File Offset: 0x000217C4
		protected override void OnObjectCollected()
		{
			base.OnObjectCollected();
			if (this.CurrentPickingContext == MultiRepresentationObjectInstantiator.ObjectType.HomespaceObject)
			{
				return;
			}
			if (this.currentCollector == this.PointerEventsSender)
			{
				IContainerBasedHPController component = this.currentCollector.Component.GetComponent<IContainerBasedHPController>();
				if (component.CanAddContainer())
				{
					HealthContainer container = new HealthContainer(component.InitialHitPoints);
					component.AddContainer(container);
				}
				else
				{
					HitPointsController.HPChangingArgs args = new HitPointsController.HPChangingArgs(false)
					{
						amount = component.InitialHitPoints
					};
					component.ModifyHitPoints(this, args);
				}
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
