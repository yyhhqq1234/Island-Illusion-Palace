using System;
using System.Threading.Tasks;
using Common.UnityExtensions;
using FlowCanvas;
using Game.Core;
using ParadoxNotion.Design;
using UnityEngine;

namespace Unliving.Scripting.FlowCanvas.Animation
{
	// Token: 0x020000D4 RID: 212
	[Name("Play Animation", 0)]
	[Category("Unliving/Animation")]
	public sealed class PlayAnimationNode : PlayAnimationNodeBase
	{
		// Token: 0x0600052B RID: 1323 RVA: 0x00012AF4 File Offset: 0x00010CF4
		private static bool SetAnimatorParameter(Animator animator, string parameterName, object value)
		{
			TypeCode typeCode = Type.GetTypeCode(value.GetType());
			if (typeCode == TypeCode.Boolean)
			{
				animator.SetBool(parameterName, (bool)value);
				return true;
			}
			if (typeCode == TypeCode.Int32)
			{
				animator.SetInteger(parameterName, (int)value);
				return true;
			}
			if (typeCode - TypeCode.Single > 1)
			{
				return false;
			}
			animator.SetFloat(parameterName, (float)value);
			return true;
		}

		// Token: 0x0600052C RID: 1324 RVA: 0x00012B50 File Offset: 0x00010D50
		private static async void StartAnimatorFinalActionTask(Animator animator, string targetState, PlayAnimationNodeBase.FinalAction finalAction, Action<Animator> callback)
		{
			int targetStateID = Animator.StringToHash(targetState);
			while (GameApplication.IsGameLoopRunning() && !animator.IsNull())
			{
				AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
				bool isTargetState = animatorStateInfo.shortNameHash == targetStateID;
				await Task.Yield();
				if (isTargetState && (!isTargetState || animatorStateInfo.normalizedTime >= 0.999f))
				{
					PlayAnimationNodeBase.PerformFinalAction(animator, finalAction);
					if (callback != null)
					{
						callback(animator);
					}
					return;
				}
			}
		}

		// Token: 0x0600052D RID: 1325 RVA: 0x00012BA1 File Offset: 0x00010DA1
		protected override bool CanPlayAnimation()
		{
			return !this.isWaitingForFinalAction && !string.IsNullOrEmpty(this.animatorParamName.value) && !string.IsNullOrEmpty(this.targetStateName.value);
		}

		// Token: 0x0600052E RID: 1326 RVA: 0x00012BD4 File Offset: 0x00010DD4
		protected override void PlayAnimation(Animator animator)
		{
			if (PlayAnimationNode.SetAnimatorParameter(animator, this.animatorParamName.value, this.animatorParamValue.value))
			{
				PlayAnimationNode.StartAnimatorFinalActionTask(animator, this.targetStateName.value, base.GetFinalAction(), new Action<Animator>(base.InvokeAnimationCompletionEvent));
				this.isWaitingForFinalAction = true;
			}
		}

		// Token: 0x0600052F RID: 1327 RVA: 0x00012C2C File Offset: 0x00010E2C
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.animatorParamName = base.AddValueInput<string>("animatorParamName", "");
			this.animatorParamValue = base.AddValueInput<object>("animatorParamValue", "");
			this.targetStateName = base.AddValueInput<string>("targetStateName", "");
		}

		// Token: 0x04000382 RID: 898
		private ValueInput<string> animatorParamName;

		// Token: 0x04000383 RID: 899
		private ValueInput<object> animatorParamValue;

		// Token: 0x04000384 RID: 900
		private ValueInput<string> targetStateName;

		// Token: 0x04000385 RID: 901
		private bool isWaitingForFinalAction;
	}
}
