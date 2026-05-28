using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.DropSystem
{
	// Token: 0x02000285 RID: 645
	public class CustomInteractiveWatcher : MonoBehaviour
	{
		// Token: 0x06001647 RID: 5703 RVA: 0x0004765A File Offset: 0x0004585A
		public void IncrementCounter()
		{
			this.counter++;
			if (this.counter >= this.interactionLimit)
			{
				this.counter = 0;
				UnityEvent unityEvent = this.interactionLimitReached;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x04000CE2 RID: 3298
		[SerializeField]
		private int interactionLimit = 3;

		// Token: 0x04000CE3 RID: 3299
		public UnityEvent interactionLimitReached;

		// Token: 0x04000CE4 RID: 3300
		private int counter;
	}
}
