using System;
using Common;
using FlowCanvas;
using FlowCanvas.Nodes;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas.Animation
{
	// Token: 0x020000D5 RID: 213
	public abstract class PlayAnimationNodeBase : FlowControlNode
	{
		// Token: 0x06000531 RID: 1329 RVA: 0x00012C8C File Offset: 0x00010E8C
		protected static void PerformFinalAction(Animator animator, PlayAnimationNodeBase.FinalAction finalAction)
		{
			if (finalAction == PlayAnimationNodeBase.FinalAction.HideObject)
			{
				animator.gameObject.SetActive(false);
				return;
			}
			if (finalAction != PlayAnimationNodeBase.FinalAction.DestroyObject)
			{
				return;
			}
			IDestroyable destroyable;
			if (animator.TryGetComponent<IDestroyable>(out destroyable))
			{
				destroyable.Destroy();
				return;
			}
			UnityEngine.Object.Destroy(animator.gameObject);
		}

		// Token: 0x06000532 RID: 1330
		protected abstract bool CanPlayAnimation();

		// Token: 0x06000533 RID: 1331
		protected abstract void PlayAnimation(Animator animator);

		// Token: 0x06000534 RID: 1332 RVA: 0x00012CCB File Offset: 0x00010ECB
		protected virtual PlayAnimationNodeBase.FinalAction GetDefaultFinalAction()
		{
			return PlayAnimationNodeBase.FinalAction.None;
		}

		// Token: 0x06000535 RID: 1333 RVA: 0x00012CCE File Offset: 0x00010ECE
		protected float GetAnimationStartDelay()
		{
			return UnityEngine.Random.Range(this.minStartDelay.value, this.maxStartDelay.value);
		}

		// Token: 0x06000536 RID: 1334 RVA: 0x00012CEB File Offset: 0x00010EEB
		protected PlayAnimationNodeBase.FinalAction GetFinalAction()
		{
			return this.finalAction.value;
		}

		// Token: 0x06000537 RID: 1335 RVA: 0x00012CF8 File Offset: 0x00010EF8
		protected void InvokeAnimationCompletionEvent(Animator animator)
		{
			this.completedAnimationAnimator = animator;
			this.animationCompleted.Call(default(Flow));
		}

		// Token: 0x06000538 RID: 1336 RVA: 0x00012D20 File Offset: 0x00010F20
		private void PlayAnimation(Flow flow)
		{
			GameObject value = this.targetObject.value;
			Animator animator;
			if (value != null && this.CanPlayAnimation() && value.TryGetComponent<Animator>(out animator))
			{
				value.SetActive(true);
				this.PlayAnimation(animator);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x06000539 RID: 1337 RVA: 0x00012D70 File Offset: 0x00010F70
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.PlayAnimation), "");
			this.targetObject = base.AddValueInput<GameObject>("targetObject", "");
			this.finalAction = base.AddValueInput<PlayAnimationNodeBase.FinalAction>("finalAction", "");
			this.finalAction.SetDefaultAndSerializedValue(this.GetDefaultFinalAction());
			this.minStartDelay = base.AddValueInput<float>("minStartDelay", "");
			this.maxStartDelay = base.AddValueInput<float>("maxStartDelay", "");
			this.flowOut = base.AddFlowOutput("", "");
			this.animationCompleted = base.AddFlowOutput("animationCompleted", "");
			base.AddValueOutput<Animator>("Last Animator", () => this.completedAnimationAnimator, "");
		}

		// Token: 0x04000386 RID: 902
		private ValueInput<GameObject> targetObject;

		// Token: 0x04000387 RID: 903
		private ValueInput<PlayAnimationNodeBase.FinalAction> finalAction;

		// Token: 0x04000388 RID: 904
		private ValueInput<float> minStartDelay;

		// Token: 0x04000389 RID: 905
		private ValueInput<float> maxStartDelay;

		// Token: 0x0400038A RID: 906
		private FlowOutput flowOut;

		// Token: 0x0400038B RID: 907
		private FlowOutput animationCompleted;

		// Token: 0x0400038C RID: 908
		private Animator completedAnimationAnimator;

		// Token: 0x02000428 RID: 1064
		public enum FinalAction
		{
			// Token: 0x0400161A RID: 5658
			None,
			// Token: 0x0400161B RID: 5659
			HideObject,
			// Token: 0x0400161C RID: 5660
			DestroyObject
		}
	}
}
