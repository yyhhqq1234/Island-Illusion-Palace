using System;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Mobs.VFX
{
	// Token: 0x02000206 RID: 518
	[Serializable]
	public sealed class ShakeImpulse
	{
		// Token: 0x0600118C RID: 4492 RVA: 0x00037190 File Offset: 0x00035390
		public void AddShakeImpulse(Transform objectTransform)
		{
			this.objectTransform = objectTransform;
			if (objectTransform == null)
			{
				this.ResetShakeImpulse();
				return;
			}
			this.startPosition = new Vector2?(this.startPosition ?? objectTransform.localPosition);
			this.startRotation = new Quaternion?(this.startRotation ?? objectTransform.localRotation);
			this.shakePositionAmplitude.x = Mathf.Max(this.positionImpulse.x, 0f);
			this.shakePositionAmplitude.y = Mathf.Max(this.positionImpulse.y, 0f);
			this.shakeRotationAmplitude = Mathf.Max(this.rotationImpulse, 0f);
		}

		// Token: 0x0600118D RID: 4493 RVA: 0x00037264 File Offset: 0x00035464
		public void ResetShakeImpulse()
		{
			if (this.startPosition != null)
			{
				this.objectTransform.localPosition = this.startPosition.Value;
			}
			if (this.startRotation != null)
			{
				this.objectTransform.localRotation = this.startRotation.Value;
			}
			this.shakePositionAmplitude = Vector2.zero;
			this.shakeRotationAmplitude = 0f;
		}

		// Token: 0x0600118E RID: 4494 RVA: 0x000372D4 File Offset: 0x000354D4
		public void Update()
		{
			if (this.objectTransform.IsNull())
			{
				return;
			}
			if (this.shakeRotationAmplitude > 1E-05f || this.shakePositionAmplitude.SqrMagnitude() > 1E-07f)
			{
				float time = Time.time;
				float num = Mathf.Clamp01(1f - this.shakeDamping * Time.deltaTime);
				this.currentShakeOffset.x = ((this.shakePositionAmplitude.x > 0f) ? ((Mathf.PerlinNoise((7f + time) * this.shakeFrequency, 0f) - 0.5f) * this.shakePositionAmplitude.x) : 0f);
				this.currentShakeOffset.y = ((this.shakePositionAmplitude.y > 0f) ? ((Mathf.PerlinNoise(0f, (5f + time) * this.shakeFrequency) - 0.5f) * this.shakePositionAmplitude.y) : 0f);
				float angle = (this.shakeRotationAmplitude > 0f) ? ((Mathf.PerlinNoise((13f + time) * this.shakeFrequency, 0f) - 0.5f) * this.shakeRotationAmplitude) : 0f;
				this.shakePositionAmplitude *= num;
				this.shakeRotationAmplitude *= num;
				this.currentRotation = QuaternionExtensions.Get2DRotation(angle);
				if (this.currentShakeOffset.sqrMagnitude > 1E-06f)
				{
					this.objectTransform.localPosition = this.startPosition.Value + new Vector2(this.currentShakeOffset.x, this.currentShakeOffset.y);
					this.objectTransform.localRotation = this.currentRotation;
					return;
				}
				this.ResetShakeImpulse();
			}
		}

		// Token: 0x04000A00 RID: 2560
		public Vector2 positionImpulse;

		// Token: 0x04000A01 RID: 2561
		public float rotationImpulse;

		// Token: 0x04000A02 RID: 2562
		[SerializeField]
		private float shakeFrequency = 15f;

		// Token: 0x04000A03 RID: 2563
		[SerializeField]
		private float shakeDamping = 10f;

		// Token: 0x04000A04 RID: 2564
		private Vector2? startPosition;

		// Token: 0x04000A05 RID: 2565
		private Quaternion? startRotation;

		// Token: 0x04000A06 RID: 2566
		private Transform objectTransform;

		// Token: 0x04000A07 RID: 2567
		private float shakeRotationAmplitude;

		// Token: 0x04000A08 RID: 2568
		private Vector2 shakePositionAmplitude;

		// Token: 0x04000A09 RID: 2569
		private Vector2 currentShakeOffset;

		// Token: 0x04000A0A RID: 2570
		private Quaternion currentRotation;
	}
}
