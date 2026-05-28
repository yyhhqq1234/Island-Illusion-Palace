using System;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000036 RID: 54
	public sealed class BezierTrajectoryTests : MonoBehaviour
	{
		// Token: 0x060001D8 RID: 472 RVA: 0x000077C8 File Offset: 0x000059C8
		private static Vector2 EvaluateBezier2(float t, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			float num = 1f - t;
			return num * num * p0 + 2f * num * t * p1 + t * t * p2;
		}

		// Token: 0x060001D9 RID: 473 RVA: 0x00007808 File Offset: 0x00005A08
		private static void DrawBezier2(Vector2 p0, Vector2 p1, Vector2 p2)
		{
			for (int i = 0; i < 25; i++)
			{
				float t = (float)i / 25f;
				float t2 = (float)(i + 1) / 25f;
				Gizmos.DrawLine(BezierTrajectoryTests.EvaluateBezier2(t, p0, p1, p2), BezierTrajectoryTests.EvaluateBezier2(t2, p0, p1, p2));
			}
			Gizmos.DrawSphere(BezierTrajectoryTests.EvaluateBezier2(0.5f, p0, p1, p2), 0.05f);
		}

		// Token: 0x060001DA RID: 474 RVA: 0x00007874 File Offset: 0x00005A74
		private void OnDrawGizmos()
		{
			if (this.curveCount <= 0)
			{
				return;
			}
			Vector2 vector = base.transform.position;
			for (int i = 0; i < this.curveCount; i++)
			{
				float f = (float)i / (float)this.curveCount * 3.1415927f * 2f;
				Vector2 vector2 = new Vector2(Mathf.Cos(f), Mathf.Sin(f));
				Vector2 vector3 = vector + vector2 * this.radius;
				Vector2 vector4 = vector + (vector3 - vector) * 0.5f;
				Vector2 p = vector4 + new Vector2
				{
					y = this.height * Mathf.Abs(vector2.x)
				};
				Gizmos.color = new Color(0f, 0f, 1f, 0.25f);
				Gizmos.DrawLine(vector, vector3);
				Gizmos.DrawLine(vector4, BezierTrajectoryTests.EvaluateBezier2(0.5f, vector, p, vector3));
				Gizmos.DrawSphere(vector4, 0.05f);
				Gizmos.color = Color.red;
				BezierTrajectoryTests.DrawBezier2(vector, p, vector3);
			}
		}

		// Token: 0x040000FA RID: 250
		public float radius = 5f;

		// Token: 0x040000FB RID: 251
		public float height = 2f;

		// Token: 0x040000FC RID: 252
		public int curveCount = 36;
	}
}
