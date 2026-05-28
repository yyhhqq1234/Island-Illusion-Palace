using System;
using Common.CollectionsExtensions;
using FlowCanvas;
using Game.Core;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Unliving.Scripting.FlowCanvas.Animation
{
	// Token: 0x020000D3 RID: 211
	[Name("Play Animation Clip", 0)]
	[Category("Unliving/Animation")]
	public sealed class PlayAnimationClipNode : PlayAnimationNodeBase, IUpdatable, IGraphElement
	{
		// Token: 0x06000522 RID: 1314 RVA: 0x00012936 File Offset: 0x00010B36
		private void StartAnimation(Animator animator)
		{
			Common.CollectionsExtensions.Extensions.Add<PlayAnimationClipNode.Animation>(new PlayAnimationClipNode.Animation(animator, this.animationClip.value), ref this.animations, ref this.animationsCount, 16);
		}

		// Token: 0x06000523 RID: 1315 RVA: 0x0001295C File Offset: 0x00010B5C
		private async void StartAnimationWithDelay(Animator animator, float delay)
		{
			this.delayedAnimationsCount++;
			await new WaitForSeconds(delay);
			if (GameApplication.IsGameLoopRunning())
			{
				this.StartAnimation(animator);
				this.delayedAnimationsCount--;
			}
		}

		// Token: 0x06000524 RID: 1316 RVA: 0x000129A5 File Offset: 0x00010BA5
		protected override void RegisterPorts()
		{
			this.animationClip = base.AddValueInput<AnimationClip>("animationClip", "");
			base.AddValueOutput<bool>("Is Last Animation", () => this.delayedAnimationsCount == 0 && this.animationsCount == 1, "");
			base.RegisterPorts();
		}

		// Token: 0x06000525 RID: 1317 RVA: 0x000129E0 File Offset: 0x00010BE0
		protected override bool CanPlayAnimation()
		{
			return this.animationClip.value != null;
		}

		// Token: 0x06000526 RID: 1318 RVA: 0x000129F4 File Offset: 0x00010BF4
		protected override void PlayAnimation(Animator animator)
		{
			float animationStartDelay = base.GetAnimationStartDelay();
			if (animationStartDelay > 0f)
			{
				this.StartAnimationWithDelay(animator, animationStartDelay);
				return;
			}
			this.StartAnimation(animator);
		}

		// Token: 0x06000527 RID: 1319 RVA: 0x00012A20 File Offset: 0x00010C20
		void IUpdatable.Update()
		{
			int i = 0;
			while (i < this.animationsCount)
			{
				ref PlayAnimationClipNode.Animation ptr = ref this.animations[i];
				if (ptr.IsCompleted())
				{
					PlayAnimationNodeBase.PerformFinalAction(ptr.Animator, base.GetFinalAction());
					base.InvokeAnimationCompletionEvent(ptr.Animator);
					ptr.SetCompleted();
					this.animations.RemoveBySwap(i, ref this.animationsCount);
				}
				else
				{
					i++;
				}
			}
		}

		// Token: 0x06000528 RID: 1320 RVA: 0x00012A8C File Offset: 0x00010C8C
		public override void OnGraphStoped()
		{
			for (int i = 0; i < this.animationsCount; i++)
			{
				this.animations[i].Destroy();
			}
			this.animationsCount = 0;
			base.OnGraphStoped();
		}

		// Token: 0x0400037E RID: 894
		private ValueInput<AnimationClip> animationClip;

		// Token: 0x0400037F RID: 895
		private PlayAnimationClipNode.Animation[] animations = new PlayAnimationClipNode.Animation[1];

		// Token: 0x04000380 RID: 896
		private int animationsCount;

		// Token: 0x04000381 RID: 897
		private int delayedAnimationsCount;

		// Token: 0x02000425 RID: 1061
		private readonly struct Animation
		{
			// Token: 0x0600229E RID: 8862 RVA: 0x0006BBCF File Offset: 0x00069DCF
			public Animation(Animator animator, AnimationClip animationClip)
			{
				this.Animator = animator;
				this.Clip = AnimationPlayableUtilities.PlayClip(animator, animationClip, out this.Graph);
				this.CompletionTime = this.Clip.GetTime<AnimationClipPlayable>() + (double)animationClip.length;
			}

			// Token: 0x0600229F RID: 8863 RVA: 0x0006BC04 File Offset: 0x00069E04
			public bool IsCompleted()
			{
				return this.Clip.GetTime<AnimationClipPlayable>() > this.CompletionTime;
			}

			// Token: 0x060022A0 RID: 8864 RVA: 0x0006BC1C File Offset: 0x00069E1C
			public void SetCompleted()
			{
				this.Graph.Stop();
			}

			// Token: 0x060022A1 RID: 8865 RVA: 0x0006BC38 File Offset: 0x00069E38
			public void Destroy()
			{
				this.Graph.Destroy();
			}

			// Token: 0x04001605 RID: 5637
			public readonly Animator Animator;

			// Token: 0x04001606 RID: 5638
			public readonly PlayableGraph Graph;

			// Token: 0x04001607 RID: 5639
			public readonly AnimationClipPlayable Clip;

			// Token: 0x04001608 RID: 5640
			public readonly double CompletionTime;
		}
	}
}
