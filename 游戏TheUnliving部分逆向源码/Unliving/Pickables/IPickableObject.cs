using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Pickables
{
	// Token: 0x0200018F RID: 399
	public interface IPickableObject
	{
		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06000B3B RID: 2875
		PickableObjectData PickableObjectData { get; }

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06000B3C RID: 2876
		Component Component { get; }

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06000B3D RID: 2877
		Vector2 WorldPosition { get; }

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06000B3E RID: 2878
		Renderer Renderer { get; }

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06000B3F RID: 2879
		IList<IPickableObjectCollector> CollectorsInRange { get; }

		// Token: 0x1400006D RID: 109
		// (add) Token: 0x06000B40 RID: 2880
		// (remove) Token: 0x06000B41 RID: 2881
		event Action<IPickableObjectCollector> CollectorEnteredPickingRange;

		// Token: 0x1400006E RID: 110
		// (add) Token: 0x06000B42 RID: 2882
		// (remove) Token: 0x06000B43 RID: 2883
		event Action<IPickableObjectCollector> CollectorExitedPickingRange;

		// Token: 0x1400006F RID: 111
		// (add) Token: 0x06000B44 RID: 2884
		// (remove) Token: 0x06000B45 RID: 2885
		event Action<IPickableObject, IPickableObjectCollector> PickedUp;

		// Token: 0x14000070 RID: 112
		// (add) Token: 0x06000B46 RID: 2886
		// (remove) Token: 0x06000B47 RID: 2887
		event Action Dropped;

		// Token: 0x06000B48 RID: 2888
		bool CanBeUsedByCollector(IPickableObjectCollector targetCollector);

		// Token: 0x06000B49 RID: 2889
		bool CanBePickedUp(IPickableObjectCollector targetCollector, PickingArgs args, out PickingUpErrorType error);

		// Token: 0x06000B4A RID: 2890
		void OnPickedUp(IPickableObjectCollector collector);

		// Token: 0x06000B4B RID: 2891
		void OnPickUpFailed(IPickableObjectCollector collector, PickingUpErrorType error);

		// Token: 0x06000B4C RID: 2892
		void OnDropped();

		// Token: 0x06000B4D RID: 2893
		void SetPickingSettings(IPickingSettings pickingSettings);

		// Token: 0x06000B4E RID: 2894
		float UpdatePickingProgress(PickingArgs args);

		// Token: 0x06000B4F RID: 2895
		void PickupByPointerEventsSender(bool force, bool lockInput);
	}
}
