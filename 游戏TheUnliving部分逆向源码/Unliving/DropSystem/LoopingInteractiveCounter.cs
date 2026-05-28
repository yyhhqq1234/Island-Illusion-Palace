using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.DropSystem
{
	// Token: 0x02000295 RID: 661
	public class LoopingInteractiveCounter : MonoBehaviour
	{
		// Token: 0x060016CD RID: 5837 RVA: 0x0004906C File Offset: 0x0004726C
		public void IncrementCounter()
		{
			this.counter++;
			if (this.events == null || this.events.Length == 0)
			{
				return;
			}
			if (this.counter >= this.events.Length)
			{
				this.counter = 0;
			}
			UnityEvent unityEvent = this.events[this.counter];
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x04000D2D RID: 3373
		private int counter;

		// Token: 0x04000D2E RID: 3374
		public UnityEvent[] events;
	}
}
