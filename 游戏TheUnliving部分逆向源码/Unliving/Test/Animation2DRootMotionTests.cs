using System;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000035 RID: 53
	public sealed class Animation2DRootMotionTests : MonoBehaviour
	{
		// Token: 0x060001D5 RID: 469 RVA: 0x00007682 File Offset: 0x00005882
		private void Start()
		{
			if (this.rootMotionTransform == null)
			{
				return;
			}
			this.lastPosition = this.rootMotionTransform.localPosition;
		}

		// Token: 0x060001D6 RID: 470 RVA: 0x000076AC File Offset: 0x000058AC
		private void LateUpdate()
		{
			if (this.rootMotionTransform == null)
			{
				return;
			}
			float deltaTime = Time.deltaTime;
			Vector2 a = this.rootMotionTransform.localPosition;
			if (this.rootMotionTransform.gameObject.activeSelf)
			{
				Vector2 vector = (a - this.lastPosition) / deltaTime;
				if (this.rootMotionAxis == default(Vector2))
				{
					base.transform.position += this.movementDirection.normalized * vector.magnitude * deltaTime;
				}
				else
				{
					base.transform.position += this.movementDirection.normalized * Mathf.Abs(Vector2.Dot(vector, this.rootMotionAxis.normalized)) * deltaTime;
				}
				Debug.DrawRay(this.rootMotionTransform.position, vector, Color.yellow);
			}
			this.lastPosition = a;
		}

		// Token: 0x040000F6 RID: 246
		public Transform rootMotionTransform;

		// Token: 0x040000F7 RID: 247
		public Vector2 rootMotionAxis;

		// Token: 0x040000F8 RID: 248
		public Vector2 movementDirection;

		// Token: 0x040000F9 RID: 249
		private Vector2 lastPosition;
	}
}
