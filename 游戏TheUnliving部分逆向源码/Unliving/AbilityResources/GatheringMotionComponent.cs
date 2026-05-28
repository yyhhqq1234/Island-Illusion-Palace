using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x02000362 RID: 866
	public sealed class GatheringMotionComponent : MonoBehaviour
	{
		// Token: 0x170005DF RID: 1503
		// (get) Token: 0x06001C63 RID: 7267 RVA: 0x00059AB0 File Offset: 0x00057CB0
		// (set) Token: 0x06001C64 RID: 7268 RVA: 0x00059AB8 File Offset: 0x00057CB8
		public ParticleSystem MotionVFXRenderer
		{
			get
			{
				return this.motionVFXRenderer;
			}
			set
			{
				if (value != null)
				{
					value.transform.parent = base.transform;
				}
				this.motionVFXRenderer = value;
			}
		}

		// Token: 0x170005E0 RID: 1504
		// (get) Token: 0x06001C65 RID: 7269 RVA: 0x00059ADB File Offset: 0x00057CDB
		public bool IsGatheringMotionInProgress
		{
			get
			{
				return this.gatheringMotionCoroutine != null;
			}
		}

		// Token: 0x14000114 RID: 276
		// (add) Token: 0x06001C66 RID: 7270 RVA: 0x00059AE8 File Offset: 0x00057CE8
		// (remove) Token: 0x06001C67 RID: 7271 RVA: 0x00059B20 File Offset: 0x00057D20
		public event Action<GatheringMotionComponent> MotionStarted;

		// Token: 0x14000115 RID: 277
		// (add) Token: 0x06001C68 RID: 7272 RVA: 0x00059B58 File Offset: 0x00057D58
		// (remove) Token: 0x06001C69 RID: 7273 RVA: 0x00059B90 File Offset: 0x00057D90
		public event Action<GatheringMotionComponent> MotionCompleted;

		// Token: 0x06001C6A RID: 7274 RVA: 0x00059BC5 File Offset: 0x00057DC5
		private IEnumerator GatheringMotionRoutine(Vector3 startPosition, Transform gatheringTarget, float duration)
		{
			float motionSpeed = 1f / duration;
			float t = 0f;
			Vector3 targetPosition = default(Vector3);
			GatheringMotionComponent.<GatheringMotionRoutine>g__UpdateFinalPosition|14_0(gatheringTarget, ref targetPosition);
			base.transform.position = startPosition;
			Action<GatheringMotionComponent> motionStarted = this.MotionStarted;
			if (motionStarted != null)
			{
				motionStarted(this);
			}
			if (this.motionVFXRenderer != null)
			{
				this.motionVFXRenderer.Play();
			}
			while (t < 1f)
			{
				yield return null;
				GatheringMotionComponent.<GatheringMotionRoutine>g__UpdateFinalPosition|14_0(gatheringTarget, ref targetPosition);
				t += motionSpeed * Time.deltaTime;
				base.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
			}
			Action<GatheringMotionComponent> motionCompleted = this.MotionCompleted;
			if (motionCompleted != null)
			{
				motionCompleted(this);
			}
			this.gatheringMotionCoroutine = null;
			yield break;
		}

		// Token: 0x06001C6B RID: 7275 RVA: 0x00059BEC File Offset: 0x00057DEC
		public bool TryStartGatheringMotion(Transform gatheringTarget, float durationOverride = 0f, Vector3? startPositionOverride = null)
		{
			float num = (durationOverride > 0f) ? durationOverride : this.motionDuration;
			if (num > 0f && this.gatheringMotionCoroutine == null)
			{
				this.gatheringMotionCoroutine = base.StartCoroutine(this.GatheringMotionRoutine(startPositionOverride ?? base.transform.position, gatheringTarget, num));
				return true;
			}
			return false;
		}

		// Token: 0x06001C6C RID: 7276 RVA: 0x00059C51 File Offset: 0x00057E51
		public void StartVFXRendererEmission()
		{
			if (this.motionVFXRenderer != null)
			{
				this.motionVFXRenderer.Play();
			}
		}

		// Token: 0x06001C6D RID: 7277 RVA: 0x00059C6C File Offset: 0x00057E6C
		public void StopVFXRendererEmission()
		{
			if (this.motionVFXRenderer != null)
			{
				this.motionVFXRenderer.Stop();
			}
		}

		// Token: 0x06001C6E RID: 7278 RVA: 0x00059C87 File Offset: 0x00057E87
		public void DestroyVFXRendererAfterEmission()
		{
			this.motionVFXRenderer.DestroyAfterEmission(true, false);
		}

		// Token: 0x06001C6F RID: 7279 RVA: 0x00059C98 File Offset: 0x00057E98
		private void Start()
		{
			if (this.motionVFXRenderer == null)
			{
				int childCount = base.transform.childCount;
				if (childCount != 0)
				{
					base.transform.GetChild(childCount - 1).TryGetComponent<ParticleSystem>(out this.motionVFXRenderer);
				}
			}
			if (this.motionVFXRenderer != null)
			{
				ParticleSystem.MainModule main = this.motionVFXRenderer.main;
				main.stopAction = ParticleSystemStopAction.None;
				main.playOnAwake = false;
			}
		}

		// Token: 0x06001C71 RID: 7281 RVA: 0x00059D19 File Offset: 0x00057F19
		[CompilerGenerated]
		internal static void <GatheringMotionRoutine>g__UpdateFinalPosition|14_0(Transform target, ref Vector3 finalPosition)
		{
			if (!target.IsNull())
			{
				finalPosition = target.position;
			}
		}

		// Token: 0x0400100F RID: 4111
		public float motionDuration = 0.5f;

		// Token: 0x04001010 RID: 4112
		[SerializeField]
		private ParticleSystem motionVFXRenderer;

		// Token: 0x04001011 RID: 4113
		private Coroutine gatheringMotionCoroutine;
	}
}
