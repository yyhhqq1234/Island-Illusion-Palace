using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Unliving.Interactables;
using Unliving.Plot;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000333 RID: 819
	public sealed class NPCConversationBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B15 RID: 6933 RVA: 0x000557A2 File Offset: 0x000539A2
		private void SetTimelinePauseState(bool isPaused)
		{
			if (Application.isPlaying && this.rootPlayable.IsValid<Playable>())
			{
				this.rootPlayable.SetSpeed((double)(isPaused ? 0 : 1));
			}
		}

		// Token: 0x06001B16 RID: 6934 RVA: 0x000557CC File Offset: 0x000539CC
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (!this.isActivated)
			{
				this.targetNPC = (playerData as NPCController);
				if (this.targetNPC == null)
				{
					return;
				}
				this.conversationStarted = this.targetNPC.StartConversation();
				if (this.conversationStarted)
				{
					if (this.timelineController != null)
					{
						this.rootPlayable = this.timelineController.playableGraph.GetRootPlayable(0);
					}
					else
					{
						this.rootPlayable = playable.GetGraph<Playable>().GetRootPlayable(0);
					}
					if (this.rootPlayable.IsValid<Playable>())
					{
						PlayableDirector playableDirector = this.rootPlayable.GetGraph<Playable>().GetResolver() as PlayableDirector;
						CutsceneBase cutsceneBase;
						if (playableDirector != null && playableDirector.TryGetComponent<CutsceneBase>(out cutsceneBase))
						{
							cutsceneBase.OnConversationStarted();
						}
					}
					this.targetNPC.OnConversationCompletedEvent.AddListener(new UnityAction(this.OnConversationCompleted));
					this.targetNPC.OnPhraseStartedEvent.AddListener(new UnityAction<CharacterPhrase>(this.OnPhraseStarted));
					this.targetNPC.OnPhraseCompletedEvent.AddListener(new UnityAction<CharacterPhrase>(this.OnPhraseCompleted));
				}
				else if (Application.isPlaying)
				{
					CutsceneBase cutsceneBase2;
					(playable.GetGraph<Playable>().GetResolver() as PlayableDirector).gameObject.TryGetComponent<CutsceneBase>(out cutsceneBase2);
					string name = cutsceneBase2.gameObject.name;
					GameObject gameObject = this.targetNPC.gameObject;
					if (!gameObject.activeInHierarchy)
					{
						gameObject.SetActive(true);
						return;
					}
					if (!this.isActivated && this.targetNPC.PreparedConversation == null && this.targetNPC.plotProgressOverrideGenerator.IsEmpty())
					{
						bool activeInHierarchy = this.targetNPC.gameObject.activeInHierarchy;
					}
				}
				this.isActivated = true;
			}
		}

		// Token: 0x06001B17 RID: 6935 RVA: 0x00055980 File Offset: 0x00053B80
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!this.rootPlayable.IsValid<Playable>())
			{
				this.rootPlayable = playable;
			}
			double duration = playable.GetDuration<Playable>();
			double num = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num > duration;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			if ((flag || flag2) && !this.conversationFinished && this.conversationStarted)
			{
				this.SetTimelinePauseState(true);
			}
		}

		// Token: 0x06001B18 RID: 6936 RVA: 0x000559F8 File Offset: 0x00053BF8
		private void OnConversationCompleted()
		{
			this.SetTimelinePauseState(false);
			this.conversationFinished = true;
			this.targetNPC.OnConversationCompletedEvent.RemoveListener(new UnityAction(this.OnConversationCompleted));
			this.targetNPC.OnPhraseStartedEvent.RemoveListener(new UnityAction<CharacterPhrase>(this.OnPhraseStarted));
			this.targetNPC.OnPhraseCompletedEvent.RemoveListener(new UnityAction<CharacterPhrase>(this.OnPhraseCompleted));
			if (this.rootPlayable.IsValid<Playable>())
			{
				PlayableDirector playableDirector = this.rootPlayable.GetGraph<Playable>().GetResolver() as PlayableDirector;
				CutsceneBase cutsceneBase;
				if (playableDirector != null && playableDirector.TryGetComponent<CutsceneBase>(out cutsceneBase))
				{
					cutsceneBase.OnConversationCompleted();
				}
			}
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x00055AA0 File Offset: 0x00053CA0
		private void OnPhraseStarted(CharacterPhrase phrase)
		{
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x00055AA2 File Offset: 0x00053CA2
		private void OnPhraseCompleted(CharacterPhrase phrase)
		{
		}

		// Token: 0x04000F38 RID: 3896
		public PlayableDirector timelineController;

		// Token: 0x04000F39 RID: 3897
		private NPCController targetNPC;

		// Token: 0x04000F3A RID: 3898
		private Playable rootPlayable;

		// Token: 0x04000F3B RID: 3899
		private bool isActivated;

		// Token: 0x04000F3C RID: 3900
		private bool conversationFinished;

		// Token: 0x04000F3D RID: 3901
		private bool conversationStarted;
	}
}
