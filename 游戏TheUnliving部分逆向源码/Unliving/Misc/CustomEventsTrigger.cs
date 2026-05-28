using System;
using Common.UnityExtensions;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.Misc
{
	// Token: 0x0200023D RID: 573
	public class CustomEventsTrigger : MonoBehaviour
	{
		// Token: 0x06001383 RID: 4995 RVA: 0x0003CEEB File Offset: 0x0003B0EB
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!collider.InLayerMask(this.triggeredObjectsLayers))
			{
				return;
			}
			UnityEvent unityEvent = this.onTriggerEnterEvents;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06001384 RID: 4996 RVA: 0x0003CF11 File Offset: 0x0003B111
		private void OnTriggerExit2D(Collider2D collider)
		{
			if (!collider.InLayerMask(this.triggeredObjectsLayers))
			{
				return;
			}
			UnityEvent unityEvent = this.onTriggerExitEvents;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x04000B50 RID: 2896
		public LayerMask triggeredObjectsLayers;

		// Token: 0x04000B51 RID: 2897
		public UnityEvent onTriggerEnterEvents;

		// Token: 0x04000B52 RID: 2898
		public UnityEvent onTriggerExitEvents;
	}
}
