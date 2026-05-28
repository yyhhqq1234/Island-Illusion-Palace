using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Unliving.Cutscenes.Timeline;
using Unliving.Interactables;
using Unliving.Mobs;
using Unliving.Mobs.Animation;
using Unliving.Player;
using Unliving.Plot.Milestones;

namespace Unliving.Cutscenes
{
	// Token: 0x0200031C RID: 796
	public class CutsceneBase : GameBehaviourBase
	{
		// Token: 0x17000594 RID: 1428
		// (get) Token: 0x06001AB8 RID: 6840 RVA: 0x00053882 File Offset: 0x00051A82
		public bool IsCutsceneActive
		{
			get
			{
				return this.isCutsceneActive;
			}
		}

		// Token: 0x14000103 RID: 259
		// (add) Token: 0x06001AB9 RID: 6841 RVA: 0x0005388C File Offset: 0x00051A8C
		// (remove) Token: 0x06001ABA RID: 6842 RVA: 0x000538C4 File Offset: 0x00051AC4
		public event Action CutsceneStarted;

		// Token: 0x14000104 RID: 260
		// (add) Token: 0x06001ABB RID: 6843 RVA: 0x000538FC File Offset: 0x00051AFC
		// (remove) Token: 0x06001ABC RID: 6844 RVA: 0x00053934 File Offset: 0x00051B34
		public event Action CutsceneFinished;

		// Token: 0x14000105 RID: 261
		// (add) Token: 0x06001ABD RID: 6845 RVA: 0x0005396C File Offset: 0x00051B6C
		// (remove) Token: 0x06001ABE RID: 6846 RVA: 0x000539A4 File Offset: 0x00051BA4
		public event Action ConversationStarted;

		// Token: 0x14000106 RID: 262
		// (add) Token: 0x06001ABF RID: 6847 RVA: 0x000539DC File Offset: 0x00051BDC
		// (remove) Token: 0x06001AC0 RID: 6848 RVA: 0x00053A14 File Offset: 0x00051C14
		public event Action ConversationCompleted;

		// Token: 0x06001AC1 RID: 6849 RVA: 0x00053A49 File Offset: 0x00051C49
		public void SetDisableAfterPlayedOnceFlag(bool flag = true)
		{
			this.disableAfterPlayedOnce = flag;
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x00053A54 File Offset: 0x00051C54
		private void SetPlayerInputActive(bool isActive)
		{
			if (this.currentPlayer == null)
			{
				return;
			}
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			PlayerInputController playerInputController = (playerBehaviour != null) ? playerBehaviour.PlayerInputController : null;
			if (playerInputController == null)
			{
				return;
			}
			playerInputController.IsActive = isActive;
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x00053A8E File Offset: 0x00051C8E
		public void OnConversationStarted()
		{
			Action conversationStarted = this.ConversationStarted;
			if (conversationStarted == null)
			{
				return;
			}
			conversationStarted();
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x00053AA0 File Offset: 0x00051CA0
		public void OnConversationCompleted()
		{
			if (this.disablePlayerInput)
			{
				this.SetPlayerInputActive(false);
			}
			Action conversationCompleted = this.ConversationCompleted;
			if (conversationCompleted == null)
			{
				return;
			}
			conversationCompleted();
		}

		// Token: 0x06001AC5 RID: 6853 RVA: 0x00053AC4 File Offset: 0x00051CC4
		public void SetGlobalCutsceneState(bool isActive)
		{
			IGameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.SetCutsceneActive(isActive, this);
			}
		}

		// Token: 0x06001AC6 RID: 6854 RVA: 0x00053AED File Offset: 0x00051CED
		public PlayerBehaviour GetPlayer()
		{
			return this.currentPlayer;
		}

		// Token: 0x06001AC7 RID: 6855 RVA: 0x00053AF8 File Offset: 0x00051CF8
		public void StartInteractiveScene()
		{
			if (this.currentPlayer == null || this.gameCamera == null)
			{
				this.activateOnStart = true;
				return;
			}
			bool flag = this.activateOnStart;
			bool flag2 = !base.transform.gameObject.activeInHierarchy;
			if (this.isCutsceneActive || this.timelineController == null || this.timelineController.playableAsset == null || flag2)
			{
				return;
			}
			Action cutsceneStarted = this.CutsceneStarted;
			if (cutsceneStarted != null)
			{
				cutsceneStarted();
			}
			if (this.disablePlayerInput)
			{
				this.SetPlayerInputActive(false);
				this.SetGlobalCutsceneState(true);
			}
			this.isCutsceneActive = true;
			UnityEvent unityEvent = this.interactiveSceneStarted;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			if (this.playerLookAtObject != null)
			{
				this.currentPlayer.SetCutsceneFacingDirection(this.playerLookAtObject.position, false);
			}
			if (this.allowCameraZoom && this.pixelPerfectCamera)
			{
				this.pixelPerfectCamera.enabled = false;
			}
			this.ManageTracksAccordingToMilestones();
			this.timelineController.Play();
		}

		// Token: 0x06001AC8 RID: 6856 RVA: 0x00053C14 File Offset: 0x00051E14
		protected void CompleteInteractiveScene()
		{
			if (!this.isCutsceneActive)
			{
				return;
			}
			Action cutsceneFinished = this.CutsceneFinished;
			if (cutsceneFinished != null)
			{
				cutsceneFinished();
			}
			this.SetPlayerInputActive(true);
			this.SetGlobalCutsceneState(false);
			UnityEvent unityEvent = this.interactiveSceneCompleted;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			if (this.allowCameraZoom && this.pixelPerfectCamera)
			{
				this.pixelPerfectCamera.enabled = true;
			}
			if (this.disableAfterPlayedOnce)
			{
				base.gameObject.SetActive(false);
				foreach (TrackAsset trackAsset in (this.timelineController.playableAsset as TimelineAsset).GetOutputTracks())
				{
					if (trackAsset.name.Contains("Debug"))
					{
						trackAsset.muted = false;
					}
				}
			}
			if (!string.IsNullOrEmpty(this.markMilestoneCompleted))
			{
				this.plotMilestoneManager.SetMilestoneReached(this.markMilestoneCompleted);
			}
			this.currentPlayer.ResetLookDirectionOverride();
			this.isCutsceneActive = false;
		}

		// Token: 0x06001AC9 RID: 6857 RVA: 0x00053D24 File Offset: 0x00051F24
		public void ActivateAndStartScene()
		{
			base.gameObject.SetActive(true);
			this.StartInteractiveScene();
		}

		// Token: 0x06001ACA RID: 6858 RVA: 0x00053D38 File Offset: 0x00051F38
		public virtual void Start()
		{
			if (!this.timelineController)
			{
				base.TryGetComponent<PlayableDirector>(out this.timelineController);
			}
			this.timelineController.stopped += this.OnTimelineCompleted;
			Animator value = null;
			Transform transform = null;
			bool flag = base.CurrentGame.TryGetPlayer(out this.currentPlayer) && this.currentPlayer.TryGetComponent<Animator>(out value) && this.currentPlayer.TryGetComponent<Transform>(out transform);
			bool flag2 = base.CurrentGame.Services.TryGet<PlayerCameraFollow>(out this.gameCamera) && this.gameCamera.TryGetComponent<CinemachineBrain>(out this.cinemachineBrain);
			CinemachineVirtualCamera value2;
			this.gameCamera.TryGetComponent<CinemachineVirtualCamera>(out value2);
			this.gameCamera.TryGetComponent<PixelPerfectCamera>(out this.pixelPerfectCamera);
			this.currentPlayer == null;
			if (this.timelineController.playableAsset != null)
			{
				this.timelineController.playableAsset = UnityEngine.Object.Instantiate<PlayableAsset>(this.timelineController.playableAsset);
			}
			TimelineAsset timelineAsset = this.timelineController.playableAsset as TimelineAsset;
			if (timelineAsset == null)
			{
				return;
			}
			base.CurrentGame.Services.TryGet<PlotMilestoneManager>(out this.plotMilestoneManager);
			IEnumerable<TrackAsset> outputTracks = timelineAsset.GetOutputTracks();
			List<NPCController> list = new List<NPCController>();
			foreach (TrackAsset trackAsset in outputTracks)
			{
				if (!trackAsset.muted)
				{
				}
				if (trackAsset.name.Contains("Player"))
				{
					if (flag)
					{
						if (trackAsset is AnimationTrack)
						{
							this.timelineController.SetGenericBinding(trackAsset, value);
						}
						if (trackAsset is MovementTrack)
						{
							this.timelineController.SetGenericBinding(trackAsset, transform);
						}
						if (trackAsset is ActivationTrack)
						{
							this.timelineController.SetGenericBinding(trackAsset, transform.gameObject);
						}
					}
				}
				else if (trackAsset.name.Contains("Debug") || trackAsset.name.Contains("debug"))
				{
					trackAsset.muted = true;
					GameObject gameObject = this.timelineController.GetGenericBinding(trackAsset) as GameObject;
					if (gameObject != null)
					{
						gameObject.SetActive(false);
					}
				}
				else if (trackAsset is AnimationTrack)
				{
					this.InitiateAnimationTracks(trackAsset);
				}
				else
				{
					if (trackAsset is CinemachineTrack && flag2)
					{
						this.timelineController.SetGenericBinding(trackAsset, this.cinemachineBrain);
						using (IEnumerator<TimelineClip> enumerator2 = trackAsset.GetClips().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								TimelineClip timelineClip = enumerator2.Current;
								if (timelineClip.displayName.Contains("GameCamera"))
								{
									CinemachineShot cinemachineShot = timelineClip.asset as CinemachineShot;
									this.timelineController.SetReferenceValue(cinemachineShot.VirtualCamera.exposedName, value2);
								}
								else if (timelineClip.displayName.Contains("follow"))
								{
									CinemachineShot cinemachineShot2 = timelineClip.asset as CinemachineShot;
									bool flag3 = false;
									((CinemachineVirtualCameraBase)this.timelineController.GetReferenceValue(cinemachineShot2.VirtualCamera.exposedName, out flag3)).Follow = transform;
								}
							}
							continue;
						}
					}
					if (trackAsset is NPCConversationTrack)
					{
						NPCController npccontroller = this.timelineController.GetGenericBinding(trackAsset) as NPCController;
						if (!(npccontroller == null))
						{
							if (list.Count == 0)
							{
								list.Add(npccontroller);
							}
							else if (!list.Contains(npccontroller))
							{
								list.Add(npccontroller);
							}
						}
					}
				}
			}
			foreach (WatchedSpawnersForAnimators watchedSpawnersForAnimators in this.subscribedSpawners)
			{
				watchedSpawnersForAnimators.timelineController = this.timelineController;
				bool flag4 = false;
				foreach (TrackAsset trackAsset2 in outputTracks)
				{
					if (trackAsset2 is AnimationTrack && trackAsset2.name.Equals(watchedSpawnersForAnimators.animatorTrackName))
					{
						watchedSpawnersForAnimators.track = (AnimationTrack)trackAsset2;
						flag4 = true;
						break;
					}
				}
				if (flag4)
				{
					if (watchedSpawnersForAnimators.spawner.IsGroupSpawned)
					{
						watchedSpawnersForAnimators.ProcessSpawnedGroup();
					}
					else
					{
						watchedSpawnersForAnimators.spawner.GroupMobSpawned += watchedSpawnersForAnimators.BindThisMobToAnimationTrack;
					}
				}
			}
			if (this.timelineController.playOnAwake)
			{
				this.timelineController.playOnAwake = false;
			}
			if (this.timelineController.extrapolationMode != DirectorWrapMode.None)
			{
				this.timelineController.extrapolationMode = DirectorWrapMode.None;
			}
			if (this.activateOnStart)
			{
				this.activateOnStart = false;
				this.StartInteractiveScene();
			}
		}

		// Token: 0x06001ACB RID: 6859 RVA: 0x0005424C File Offset: 0x0005244C
		private void InitiateAnimationTracks(TrackAsset track)
		{
			Animator animator = this.timelineController.GetGenericBinding(track) as Animator;
			if (animator == null)
			{
				return;
			}
			animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			GameObject gameObject = animator.gameObject;
			MobBehaviour component = gameObject.GetComponent<MobBehaviour>();
			if (component)
			{
				component.IsKinematic = true;
				GameMobAnimationController component2 = gameObject.GetComponent<GameMobAnimationController>();
				if (component2)
				{
					component2.updateLookDirection = false;
				}
			}
		}

		// Token: 0x06001ACC RID: 6860 RVA: 0x000542B0 File Offset: 0x000524B0
		private double GetMaxTrackDurationRecursive(TrackAsset rootTrack)
		{
			if (rootTrack.muted)
			{
				return 0.0;
			}
			double num = rootTrack.start + rootTrack.duration;
			foreach (TrackAsset rootTrack2 in rootTrack.GetChildTracks())
			{
				double maxTrackDurationRecursive = this.GetMaxTrackDurationRecursive(rootTrack2);
				if (maxTrackDurationRecursive > num)
				{
					num = maxTrackDurationRecursive;
				}
			}
			return num;
		}

		// Token: 0x06001ACD RID: 6861 RVA: 0x00054328 File Offset: 0x00052528
		protected void OnTimelineCompleted(PlayableDirector timeline)
		{
			if (timeline != this.timelineController)
			{
				return;
			}
			this.CompleteInteractiveScene();
		}

		// Token: 0x06001ACE RID: 6862 RVA: 0x0005433F File Offset: 0x0005253F
		private IEnumerator CameraReturningRoutine()
		{
			Vector3 startCameraPosition = this.gameCamera.transform.position;
			Vector3 targetCameraPosition = this.currentPlayer.Position;
			targetCameraPosition.z = startCameraPosition.z;
			for (float t = 0f; t <= this.toGameCameraTransitionTime; t += Time.deltaTime)
			{
				yield return null;
				float t2 = Mathf.SmoothStep(0f, 1f, t / this.toGameCameraTransitionTime);
				this.gameCamera.transform.position = Vector3.Lerp(startCameraPosition, targetCameraPosition, t2);
			}
			this.gameCamera.transform.position = targetCameraPosition;
			this.gameCamera.enabled = true;
			yield break;
		}

		// Token: 0x06001ACF RID: 6863 RVA: 0x00054350 File Offset: 0x00052550
		private void ManageTracksAccordingToMilestones()
		{
			if (this.cutsceneStages == null || this.cutsceneStages.Count == 0)
			{
				return;
			}
			TimelineAsset timelineAsset = this.timelineController.playableAsset as TimelineAsset;
			if (timelineAsset == null)
			{
				return;
			}
			uint num = 1U;
			bool flag = false;
			foreach (CutsceneStagesDescriptor cutsceneStagesDescriptor in this.cutsceneStages)
			{
				bool flag2 = this.plotMilestoneManager.IsMilestoneReached(cutsceneStagesDescriptor.exitConditionMilestone);
				foreach (TrackAsset trackAsset in timelineAsset.GetRootTracks())
				{
					if (trackAsset.name.Equals(cutsceneStagesDescriptor.stageTrackName))
					{
						trackAsset.muted = (flag2 || flag);
						flag = (flag || !trackAsset.muted);
						if (!flag)
						{
							num += 1U;
						}
						break;
					}
				}
			}
			double num2 = 0.0;
			foreach (TrackAsset rootTrack in timelineAsset.GetRootTracks())
			{
				double maxTrackDurationRecursive = this.GetMaxTrackDurationRecursive(rootTrack);
				if (maxTrackDurationRecursive > num2)
				{
					num2 = maxTrackDurationRecursive;
				}
			}
			timelineAsset.durationMode = TimelineAsset.DurationMode.FixedLength;
			timelineAsset.fixedDuration = num2;
			this.timelineController.RebuildGraph();
		}

		// Token: 0x06001AD0 RID: 6864 RVA: 0x000544E4 File Offset: 0x000526E4
		public override void Destroy()
		{
			if (this.timelineController != null)
			{
				this.timelineController.Stop();
				this.timelineController.stopped -= this.OnTimelineCompleted;
			}
			foreach (WatchedSpawnersForAnimators watchedSpawnersForAnimators in this.subscribedSpawners)
			{
				watchedSpawnersForAnimators.spawner.GroupMobSpawned -= watchedSpawnersForAnimators.BindThisMobToAnimationTrack;
			}
			base.Destroy();
		}

		// Token: 0x04000EE9 RID: 3817
		[Header("Setup parameters")]
		public PlayableDirector timelineController;

		// Token: 0x04000EEA RID: 3818
		public bool disableAfterPlayedOnce = true;

		// Token: 0x04000EEB RID: 3819
		public bool allowCameraZoom;

		// Token: 0x04000EEC RID: 3820
		public bool disablePlayerInput = true;

		// Token: 0x04000EED RID: 3821
		[Tooltip("Update the cutscene's duration, ignoring muted tracks (meant for Apotheosis)")]
		public bool recalculateLength;

		// Token: 0x04000EEE RID: 3822
		[Tooltip("This parameter is used by the bosses, to decide when enable their on-screen healht bar)")]
		public bool preBossBattleCutscene;

		// Token: 0x04000EEF RID: 3823
		public float toGameCameraTransitionTime = 1.5f;

		// Token: 0x04000EF0 RID: 3824
		[Tooltip("If set, the player's sprite will be altered to look in the direction of this object")]
		public Transform playerLookAtObject;

		// Token: 0x04000EF1 RID: 3825
		[Space]
		[Header("Finalization parameters")]
		[SerializeField]
		private string markMilestoneCompleted;

		// Token: 0x04000EF2 RID: 3826
		[SerializeField]
		[Tooltip("When following spawners would spawn mobs, their Animators will be binded to corresponding tracks")]
		private List<WatchedSpawnersForAnimators> subscribedSpawners;

		// Token: 0x04000EF3 RID: 3827
		[Space]
		[Header("Events")]
		public UnityEvent interactiveSceneStarted;

		// Token: 0x04000EF4 RID: 3828
		public UnityEvent interactiveSceneCompleted;

		// Token: 0x04000EF5 RID: 3829
		[Space]
		public List<CutsceneStagesDescriptor> cutsceneStages;

		// Token: 0x04000EF6 RID: 3830
		protected PlayerBehaviour currentPlayer;

		// Token: 0x04000EF7 RID: 3831
		protected PlayerCameraFollow gameCamera;

		// Token: 0x04000EF8 RID: 3832
		protected CinemachineBrain cinemachineBrain;

		// Token: 0x04000EF9 RID: 3833
		protected PlotMilestoneManager plotMilestoneManager;

		// Token: 0x04000EFA RID: 3834
		protected bool isCutsceneActive;

		// Token: 0x04000EFB RID: 3835
		protected PixelPerfectCamera pixelPerfectCamera;

		// Token: 0x04000EFC RID: 3836
		protected bool activateOnStart;
	}
}
