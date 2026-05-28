using System;
using Common.Math;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000041 RID: 65
	public sealed class SectorCircleIntersectionTests : MonoBehaviour
	{
		// Token: 0x0600022D RID: 557 RVA: 0x00008EC0 File Offset: 0x000070C0
		private bool IsIntersected(Vector2 center, Vector2 sectorDir, Vector2 position, float radius)
		{
			Vector2 vector = position - center;
			float num = Vector2.Dot(vector, sectorDir) / this.sectorHeight;
			if (num <= 0f || num >= 1f)
			{
				return false;
			}
			Vector2 vector2 = new Vector2
			{
				x = sectorDir.y,
				y = -sectorDir.x
			};
			if (Mathf.Abs(Vector2.Dot(position - center, vector2)) < num * this.maxDistance)
			{
				return true;
			}
			float d = Mathf.Sign(sectorDir.x * vector.y - sectorDir.y * vector.x);
			return Mathf.Abs(Vector2.Dot(position + d * vector2 * radius - center, vector2)) < num * this.maxDistance;
		}

		// Token: 0x0600022E RID: 558 RVA: 0x00008F90 File Offset: 0x00007190
		private void Start()
		{
			this.sectorCosine = Mathf.Cos(this.sectorAngle * 0.5f * 0.017453292f);
			float num = this.sectorHeight / this.sectorCosine;
			Debug.Log(string.Format("maxSide: {0}", num));
			this.maxDistance = Mathf.Sqrt(num * num - this.sectorHeight * this.sectorHeight);
			Debug.Log(this.maxDistance);
			this.testData = new Vector3[32];
			int num2 = UnityEngine.Random.Range(0, 1000000);
			Vector2 a = base.transform.position;
			for (int i = 0; i < this.testData.Length; i++)
			{
				Vector2 r2Value = PhiSequence.GetR2Value(num2 + i, false);
				r2Value.x = r2Value.x * 2f - 1f;
				r2Value.y = r2Value.y * 2f - 1f;
				Vector2 vector = a + r2Value * this.sectorHeight * 1.5f;
				this.testData[i] = new Vector3(vector.x, vector.y, UnityEngine.Random.Range(0.2f, 2f));
			}
		}

		// Token: 0x0600022F RID: 559 RVA: 0x000090D8 File Offset: 0x000072D8
		private void LateUpdate()
		{
			Vector2 vector = base.transform.position;
			Vector2 vector2 = Quaternion.AngleAxis(this.sectorOrientation, new Vector3
			{
				z = 1f
			}) * new Vector3
			{
				y = 1f
			};
			Quaternion rotation = Quaternion.AngleAxis(this.sectorAngle * 0.5f, new Vector3
			{
				z = 1f
			});
			Debug.DrawRay(vector, vector2 * this.sectorHeight, Color.blue);
			Debug.DrawRay(vector, rotation * vector2 * this.sectorHeight, Color.magenta);
			Debug.DrawRay(vector, Quaternion.Inverse(rotation) * vector2 * this.sectorHeight, Color.magenta);
			Debug.DrawRay(vector, new Vector2
			{
				x = vector2.y,
				y = -vector2.x
			} * this.maxDistance, Color.yellow);
			for (int i = 0; i < this.testData.Length; i++)
			{
				this.IsIntersected(vector, vector2, this.testData[i], this.testData[i].z);
			}
		}

		// Token: 0x0400014A RID: 330
		public float sectorHeight;

		// Token: 0x0400014B RID: 331
		public float sectorAngle;

		// Token: 0x0400014C RID: 332
		public float sectorOrientation;

		// Token: 0x0400014D RID: 333
		private float sectorCosine;

		// Token: 0x0400014E RID: 334
		private float maxDistance;

		// Token: 0x0400014F RID: 335
		private Vector3[] testData;
	}
}
