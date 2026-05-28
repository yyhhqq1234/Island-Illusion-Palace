using System;
using Common;
using UltEvents;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x0200023C RID: 572
	public sealed class AmountBasedObjectValueSetter : MonoBehaviour, IAmountBased
	{
		// Token: 0x17000434 RID: 1076
		// (get) Token: 0x0600137D RID: 4989 RVA: 0x0003CE39 File Offset: 0x0003B039
		// (set) Token: 0x0600137E RID: 4990 RVA: 0x0003CE44 File Offset: 0x0003B044
		public float Amount
		{
			get
			{
				return this.amount;
			}
			set
			{
				float num = this.amount;
				this.amount = value;
				if (this.disallowInactiveObjectEventFiring && !base.gameObject.activeInHierarchy)
				{
					this.isValueDirty |= (num != this.amount);
					return;
				}
				this.FireEvent();
			}
		}

		// Token: 0x0600137F RID: 4991 RVA: 0x0003CE94 File Offset: 0x0003B094
		private void FireEvent()
		{
			UltEvent<float> ultEvent = this.onAmountSettedEvent;
			if (ultEvent != null)
			{
				ultEvent.Invoke(this.amount);
			}
			this.isValueDirty = false;
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x0003CEB4 File Offset: 0x0003B0B4
		private void OnEnable()
		{
			if (this.isValueDirty && this.isInitialized)
			{
				this.FireEvent();
			}
		}

		// Token: 0x06001381 RID: 4993 RVA: 0x0003CECC File Offset: 0x0003B0CC
		private void Start()
		{
			if (this.isValueDirty)
			{
				this.FireEvent();
			}
			this.isInitialized = true;
		}

		// Token: 0x04000B4B RID: 2891
		public bool disallowInactiveObjectEventFiring;

		// Token: 0x04000B4C RID: 2892
		public UltEvent<float> onAmountSettedEvent;

		// Token: 0x04000B4D RID: 2893
		private float amount;

		// Token: 0x04000B4E RID: 2894
		private bool isValueDirty;

		// Token: 0x04000B4F RID: 2895
		private bool isInitialized;
	}
}
