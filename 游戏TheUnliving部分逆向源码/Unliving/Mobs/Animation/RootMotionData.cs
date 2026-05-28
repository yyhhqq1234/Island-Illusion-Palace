using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Mobs.Animation
{
	// Token: 0x0200021E RID: 542
	[CreateAssetMenu(fileName = "RootMotionData", menuName = "Game/Animation/Root Motion Data")]
	public sealed class RootMotionData : ScriptableObject, ISerializationCallbackReceiver
	{
		// Token: 0x06001291 RID: 4753 RVA: 0x0003AF4C File Offset: 0x0003914C
		private static float Differentiate(AnimationCurve curve, float x, float xMin, float xMax)
		{
			float num = Mathf.Max(xMin, x - 1E-06f);
			float num2 = Mathf.Min(xMax, x + 1E-06f);
			float num3 = curve.Evaluate(num);
			return (curve.Evaluate(num2) - num3) / (num2 - num);
		}

		// Token: 0x06001292 RID: 4754 RVA: 0x0003AF8C File Offset: 0x0003918C
		private static float Differentiate(AnimationCurve curve, float x)
		{
			return RootMotionData.Differentiate(curve, x, curve.keys.First<Keyframe>().time, curve.keys.Last<Keyframe>().time);
		}

		// Token: 0x06001293 RID: 4755 RVA: 0x0003AFC8 File Offset: 0x000391C8
		private static AnimationCurve Derivative(AnimationCurve curve, int resolution = 100, float smoothing = 0.05f)
		{
			float[] array = new float[resolution];
			for (int k = 0; k < resolution; k++)
			{
				array[k] = RootMotionData.Differentiate(curve, (float)k / (float)resolution);
			}
			List<Vector2> list = array.Select((float s, int i) => new Vector2((float)i / (float)resolution, s)).ToList<Vector2>();
			List<Vector2> list2 = new List<Vector2>();
			if (smoothing > 0f)
			{
				LineUtility.Simplify(list, smoothing, list2);
			}
			else
			{
				list2 = list;
			}
			AnimationCurve animationCurve = new AnimationCurve((from v in list2
			select new Keyframe(v.x, v.y)).ToArray<Keyframe>());
			Keyframe[] keys = animationCurve.keys;
			int j = 0;
			int num = keys.Length;
			while (j < num)
			{
				animationCurve.SmoothTangents(j, 0f);
				j++;
			}
			return animationCurve;
		}

		// Token: 0x06001294 RID: 4756 RVA: 0x0003B0A7 File Offset: 0x000392A7
		private void GetVelocityMaskCurve()
		{
		}

		// Token: 0x06001295 RID: 4757 RVA: 0x0003B0A9 File Offset: 0x000392A9
		private void UpdateData(bool force)
		{
		}

		// Token: 0x06001296 RID: 4758 RVA: 0x0003B0AB File Offset: 0x000392AB
		[ContextMenu("Update")]
		private void UpdateData()
		{
			this.UpdateData(true);
		}

		// Token: 0x06001297 RID: 4759 RVA: 0x0003B0B4 File Offset: 0x000392B4
		public Vector3 Evaluate(float t)
		{
			float num = (this.velocityMask != null && this.velocityMask.length > 1) ? this.velocityMask.Evaluate(t) : 1f;
			return new Vector3
			{
				x = this.velocityXCurve.Evaluate(t) * num,
				y = this.velocityYCurve.Evaluate(t) * num,
				z = this.velocityZCurve.Evaluate(t) * num
			};
		}

		// Token: 0x06001298 RID: 4760 RVA: 0x0003B133 File Offset: 0x00039333
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this.UpdateData(false);
		}

		// Token: 0x06001299 RID: 4761 RVA: 0x0003B13C File Offset: 0x0003933C
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x04000AB4 RID: 2740
		private const string LocalPositionPropertyName = "m_LocalPosition";

		// Token: 0x04000AB5 RID: 2741
		private const string IsActivePropertyName = "m_IsActive";

		// Token: 0x04000AB6 RID: 2742
		public AnimationClip clip;

		// Token: 0x04000AB7 RID: 2743
		public string motionPivotPropertyPath;

		// Token: 0x04000AB8 RID: 2744
		[FormerlySerializedAs("derivativeResoultion")]
		public int derivativeResolution = 100;

		// Token: 0x04000AB9 RID: 2745
		public float curvesSmoothing = 0.02f;

		// Token: 0x04000ABA RID: 2746
		[SerializeField]
		private AnimationCurve velocityXCurve;

		// Token: 0x04000ABB RID: 2747
		[SerializeField]
		private AnimationCurve velocityYCurve;

		// Token: 0x04000ABC RID: 2748
		[SerializeField]
		private AnimationCurve velocityZCurve;

		// Token: 0x04000ABD RID: 2749
		[SerializeField]
		private AnimationCurve velocityMask;

		// Token: 0x04000ABE RID: 2750
		[SerializeField]
		[HideInInspector]
		private string lastClipHash;
	}
}
