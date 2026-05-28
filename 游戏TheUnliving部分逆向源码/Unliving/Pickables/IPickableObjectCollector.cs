using System;
using UnityEngine;

namespace Unliving.Pickables
{
	// Token: 0x02000190 RID: 400
	public interface IPickableObjectCollector
	{
		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06000B50 RID: 2896
		Component Component { get; }

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06000B51 RID: 2897
		IPickableObject CurrentBestPickingCandidate { get; }

		// Token: 0x14000071 RID: 113
		// (add) Token: 0x06000B52 RID: 2898
		// (remove) Token: 0x06000B53 RID: 2899
		event Action<IPickableObjectCollector, IPickableObject> ObjectPickedUp;

		// Token: 0x14000072 RID: 114
		// (add) Token: 0x06000B54 RID: 2900
		// (remove) Token: 0x06000B55 RID: 2901
		event Action<IPickableObjectCollector, IPickableObject, PickingUpErrorType> ObjectPickingUpFailed;

		// Token: 0x14000073 RID: 115
		// (add) Token: 0x06000B56 RID: 2902
		// (remove) Token: 0x06000B57 RID: 2903
		event Action<IPickableObjectCollector, IPickableObject> BestPickingCandidateChanged;

		// Token: 0x14000074 RID: 116
		// (add) Token: 0x06000B58 RID: 2904
		// (remove) Token: 0x06000B59 RID: 2905
		event Action<IPickableObjectCollector, IPickableObject> PickableObjectCollected;

		// Token: 0x06000B5A RID: 2906
		void AddPickingCandidate(IPickableObject obj);

		// Token: 0x06000B5B RID: 2907
		void RemovePickingCandidate(IPickableObject obj);

		// Token: 0x06000B5C RID: 2908
		bool PickUp(IPickableObject obj, PickingArgs args);

		// Token: 0x06000B5D RID: 2909
		bool PickUpBestObject(PickingArgs args);

		// Token: 0x06000B5E RID: 2910
		void OnPickableObjectCollected(IPickableObject obj);
	}
}
