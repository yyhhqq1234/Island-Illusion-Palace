using System;
using System.Collections;
using Game.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using Unliving.Player;

namespace Unliving.Cutscenes
{
	// Token: 0x0200031B RID: 795
	public sealed class CutsceneActivator : GameBehaviourBase
	{
		// Token: 0x17000593 RID: 1427
		// (get) Token: 0x06001AA9 RID: 6825 RVA: 0x000535BB File Offset: 0x000517BB
		public bool IsCutsceneActive
		{
			get
			{
				return this.isCutsceneActive;
			}
		}

		// Token: 0x06001AAA RID: 6826 RVA: 0x000535C4 File Offset: 0x000517C4
		private void SetGlobalCutsceneState(bool isActive)
		{
			IGameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out gameSessionManager))
			{
				gameSessionManager.SetCutsceneActive(isActive, this);
			}
		}

		// Token: 0x06001AAB RID: 6827 RVA: 0x000535F0 File Offset: 0x000517F0
		private void SetPlayerInputActive(bool isActive)
		{
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			PlayerInputController playerInputController = (playerBehaviour != null) ? playerBehaviour.PlayerInputController : null;
			if (playerInputController == null)
			{
				return;
			}
			playerInputController.IsActive = isActive;
		}

		// Token: 0x06001AAC RID: 6828 RVA: 0x0005361B File Offset: 0x0005181B
		private void SetGameCameraActive(bool isActive)
		{
			if (this.gameCamera == null)
			{
				return;
			}
			if (!isActive)
			{
				this.returnGameCamera = (this.toGameCameraTransitionSpeed > 0f);
			}
			this.gameCamera.enabled = isActive;
		}

		// Token: 0x06001AAD RID: 6829 RVA: 0x00053650 File Offset: 0x00051850
		private void StartCutscene()
		{
			if (this.isCutsceneActive || this.timelineController == null || this.timelineController.playableAsset == null)
			{
				return;
			}
			this.SetPlayerInputActive(false);
			if (this.disableGameCamera)
			{
				this.SetGameCameraActive(false);
			}
			this.isCutsceneActive = true;
			this.SetGlobalCutsceneState(true);
			UnityEvent unityEvent = this.cutsceneStarted;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			base.StartCoroutine(this.CutsceneStartRoutine());
		}

		// Token: 0x06001AAE RID: 6830 RVA: 0x000536CC File Offset: 0x000518CC
		private void CompleteCutscene()
		{
			if (!this.isCutsceneActive)
			{
				return;
			}
			this.SetGameCameraActive(true);
			this.SetPlayerInputActive(true);
			UnityEvent unityEvent = this.cutsceneCompleted;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			base.gameObject.SetActive(false);
			this.isCutsceneActive = false;
			this.SetGlobalCutsceneState(false);
		}

		// Token: 0x06001AAF RID: 6831 RVA: 0x0005371B File Offset: 0x0005191B
		private IEnumerator PlayerMovementRoutine(Vector2 targetPosition)
		{
			yield return this.currentPlayer.MotionController.MoveToPoint(targetPosition, true, this.currentPlayer);
			this.currentPlayerMovementCoroutine = null;
			yield break;
		}

		// Token: 0x06001AB0 RID: 6832 RVA: 0x00053731 File Offset: 0x00051931
		private IEnumerator CameraReturningRoutine()
		{
			Vector3 startCameraPosition = this.gameCamera.transform.position;
			Vector3 targetCameraPosition = this.currentPlayer.Position;
			targetCameraPosition.z = startCameraPosition.z;
			float returningTime = (targetCameraPosition - startCameraPosition).magnitude / this.toGameCameraTransitionSpeed;
			for (float t = 0f; t <= returningTime; t += Time.deltaTime)
			{
				yield return null;
				float t2 = Mathf.SmoothStep(0f, 1f, t / returningTime);
				this.gameCamera.transform.position = Vector3.Lerp(startCameraPosition, targetCameraPosition, t2);
			}
			this.gameCamera.transform.position = targetCameraPosition;
			yield break;
		}

		// Token: 0x06001AB1 RID: 6833 RVA: 0x00053740 File Offset: 0x00051940
		private IEnumerator CutsceneStartRoutine()
		{
			if (this.endPlayerPoint != null)
			{
				this.finalPlayerPosition = new Vector2?(this.endPlayerPoint.position);
			}
			if (this.startPlayerPoint != null)
			{
				if (this.returnPlayerToLastPosition && this.finalPlayerPosition == null)
				{
					this.finalPlayerPosition = new Vector2?(this.currentPlayer.Position);
				}
				this.currentPlayerMovementCoroutine = base.StartCoroutine(this.PlayerMovementRoutine(this.startPlayerPoint.position));
				yield return this.currentPlayerMovementCoroutine;
			}
			this.timelineController.Play();
			yield break;
		}

		// Token: 0x06001AB2 RID: 6834 RVA: 0x0005374F File Offset: 0x0005194F
		private IEnumerator CutsceneFinalizationRoutine()
		{
			if (this.finalPlayerPosition != null)
			{
				if (this.currentPlayerMovementCoroutine != null)
				{
					base.StopCoroutine(this.currentPlayerMovementCoroutine);
				}
				yield return base.StartCoroutine(this.PlayerMovementRoutine(this.finalPlayerPosition.Value));
				this.finalPlayerPosition = null;
			}
			if (this.returnGameCamera)
			{
				this.returnGameCamera = false;
				yield return base.StartCoroutine(this.CameraReturningRoutine());
			}
			this.CompleteCutscene();
			yield break;
		}

		// Token: 0x06001AB3 RID: 6835 RVA: 0x0005375E File Offset: 0x0005195E
		private void OnTimelineCompleted(PlayableDirector timeline)
		{
			base.StartCoroutine(this.CutsceneFinalizationRoutine());
		}

		// Token: 0x06001AB4 RID: 6836 RVA: 0x00053770 File Offset: 0x00051970
		private void Start()
		{
			Collider2D collider2D;
			if (base.TryGetComponent<Collider2D>(out collider2D))
			{
				collider2D.isTrigger = true;
			}
			if (this.timelineController != null || base.TryGetComponent<PlayableDirector>(out this.timelineController))
			{
				this.timelineController.playOnAwake = false;
				this.timelineController.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
				this.timelineController.stopped += this.OnTimelineCompleted;
			}
			base.CurrentGame.TryGetPlayer(out this.currentPlayer);
			base.CurrentGame.Services.TryGet<PlayerCameraFollow>(out this.gameCamera);
		}

		// Token: 0x06001AB5 RID: 6837 RVA: 0x00053802 File Offset: 0x00051A02
		private void OnTriggerEnter2D(Collider2D collider)
		{
			UnityEngine.Object gameObject = collider.gameObject;
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			if (gameObject == ((playerBehaviour != null) ? playerBehaviour.gameObject : null))
			{
				this.StartCutscene();
			}
		}

		// Token: 0x06001AB6 RID: 6838 RVA: 0x00053829 File Offset: 0x00051A29
		public override void Destroy()
		{
			if (this.timelineController != null)
			{
				this.timelineController.Stop();
				this.timelineController.stopped -= this.OnTimelineCompleted;
			}
			base.Destroy();
		}

		// Token: 0x04000ED7 RID: 3799
		public PlayableDirector timelineController;

		// Token: 0x04000ED8 RID: 3800
		[Space]
		public Transform startPlayerPoint;

		// Token: 0x04000ED9 RID: 3801
		public Transform endPlayerPoint;

		// Token: 0x04000EDA RID: 3802
		public bool returnPlayerToLastPosition = true;

		// Token: 0x04000EDB RID: 3803
		[Space]
		public bool disableGameCamera = true;

		// Token: 0x04000EDC RID: 3804
		public float toGameCameraTransitionSpeed = 5f;

		// Token: 0x04000EDD RID: 3805
		[Space]
		public UnityEvent cutsceneStarted;

		// Token: 0x04000EDE RID: 3806
		public UnityEvent cutsceneCompleted;

		// Token: 0x04000EDF RID: 3807
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000EE0 RID: 3808
		private Vector2? finalPlayerPosition;

		// Token: 0x04000EE1 RID: 3809
		private PlayerCameraFollow gameCamera;

		// Token: 0x04000EE2 RID: 3810
		private bool returnGameCamera;

		// Token: 0x04000EE3 RID: 3811
		private Coroutine currentPlayerMovementCoroutine;

		// Token: 0x04000EE4 RID: 3812
		private bool isCutsceneActive;
	}
}
