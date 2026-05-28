using System;
using System.Collections;
using UnityEngine;

namespace Unliving.Mobs.Animation
{
	// Token: 0x0200021C RID: 540
	public struct AnimatorDisabler
	{
		// Token: 0x170003EB RID: 1003
		// (get) Token: 0x06001265 RID: 4709 RVA: 0x00039FFF File Offset: 0x000381FF
		public bool InProgress
		{
			get
			{
				return this.inProgress;
			}
		}

		// Token: 0x06001266 RID: 4710 RVA: 0x0003A007 File Offset: 0x00038207
		public AnimatorDisabler(Animator targetAnimator, int stateToWaitTagHash)
		{
			this = default(AnimatorDisabler);
			this.targetAnimator = targetAnimator;
			this.stateToWaitTagHash = stateToWaitTagHash;
		}

		// Token: 0x06001267 RID: 4711 RVA: 0x0003A01E File Offset: 0x0003821E
		private IEnumerator DisablingRoutine(Action<Animator> additionalParamsAction)
		{
			bool setAdditionalParams = true;
			this.inProgress = true;
			AnimatorStateInfo animatorStateInfo;
			do
			{
				animatorStateInfo = this.targetAnimator.GetCurrentAnimatorStateInfo(0);
				yield return null;
				if (setAdditionalParams)
				{
					if (additionalParamsAction != null)
					{
						additionalParamsAction(this.targetAnimator);
					}
					setAdditionalParams = false;
				}
			}
			while (animatorStateInfo.tagHash != this.stateToWaitTagHash || animatorStateInfo.normalizedTime < 0.999f);
			this.inProgress = false;
			this.targetAnimator.enabled = false;
			yield break;
		}

		// Token: 0x06001268 RID: 4712 RVA: 0x0003A039 File Offset: 0x00038239
		public void DisableAnimator(MonoBehaviour coroutineLauncher, int? targetStateFlagID = null, Action<Animator> additionalParamsAction = null)
		{
			if (this.inProgress)
			{
				return;
			}
			if (targetStateFlagID != null)
			{
				this.targetAnimator.SetBool(targetStateFlagID.Value, true);
			}
			coroutineLauncher.StartCoroutine(this.DisablingRoutine(additionalParamsAction));
		}

		// Token: 0x04000A7F RID: 2687
		private readonly Animator targetAnimator;

		// Token: 0x04000A80 RID: 2688
		private readonly int stateToWaitTagHash;

		// Token: 0x04000A81 RID: 2689
		private bool inProgress;
	}
}
