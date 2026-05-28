using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x0200003F RID: 63
	public sealed class NumericalIntegrationTests : MonoBehaviour
	{
		// Token: 0x0600021D RID: 541 RVA: 0x0000898B File Offset: 0x00006B8B
		private static int Pow2(int pow)
		{
			return 1 << pow;
		}

		// Token: 0x0600021E RID: 542 RVA: 0x00008993 File Offset: 0x00006B93
		private static int Pow4(int pow)
		{
			int num = NumericalIntegrationTests.Pow2(pow);
			return num * num;
		}

		// Token: 0x0600021F RID: 543 RVA: 0x000089A0 File Offset: 0x00006BA0
		private float TrapezoidalIntegration(int sliceCount, float a = 0f, float b = 1f)
		{
			float num = (b - a) / (float)sliceCount;
			float num2 = (this.testCurve.Evaluate(a) + this.testCurve.Evaluate(b)) * 0.5f;
			for (int i = 1; i < sliceCount; i++)
			{
				num2 += this.testCurve.Evaluate(a + (float)i * num);
			}
			return num * num2;
		}

		// Token: 0x06000220 RID: 544 RVA: 0x000089F8 File Offset: 0x00006BF8
		private float RombergIntegration(int order, float a = 0f, float b = 1f)
		{
			float[] array = new float[order + 1];
			float[,] array2 = new float[order + 1, order + 1];
			for (int i = 1; i < order + 1; i++)
			{
				array[i] = (b - a) / (float)NumericalIntegrationTests.Pows2[i - 1];
			}
			array2[1, 1] = array[1] / 2f * (this.testCurve.Evaluate(a) + this.testCurve.Evaluate(b));
			for (int j = 2; j < order + 1; j++)
			{
				float num = 0f;
				int num2 = NumericalIntegrationTests.Pows2[j - 2];
				for (int k = 1; k <= num2; k++)
				{
					num += this.testCurve.Evaluate(a + (2f * (float)k - 1f) * array[j]);
				}
				array2[j, 1] = 0.5f * (array2[j - 1, 1] + array[j - 1] * num);
			}
			for (int l = 2; l < order + 1; l++)
			{
				for (int m = 2; m <= l; m++)
				{
					array2[l, m] = array2[l, m - 1] + (array2[l, m - 1] - array2[l - 1, m - 1]) / (float)(NumericalIntegrationTests.Pows4[m - 1] - 1);
				}
			}
			return array2[order, order];
		}

		// Token: 0x06000221 RID: 545 RVA: 0x00008B48 File Offset: 0x00006D48
		[ContextMenu("Test")]
		private void Test()
		{
			UnityEngine.Debug.Log(string.Format("trapezoidal ({0}): {1}", this.integrationSteps, this.TrapezoidalIntegration(this.integrationSteps, 0f, 1f)));
			UnityEngine.Debug.Log(string.Format("romberg: {0}", this.RombergIntegration(3, 0f, 1f)));
		}

		// Token: 0x06000222 RID: 546 RVA: 0x00008BAF File Offset: 0x00006DAF
		private IEnumerator Start()
		{
			yield return new WaitForSeconds(5f);
			UnityEngine.Debug.Log(string.Format("trapezoidal (10000): {0}", this.TrapezoidalIntegration(10000, 0f, 1f)));
			Stopwatch stopwatch = new Stopwatch();
			GC.Collect();
			float num = this.TrapezoidalIntegration(this.integrationSteps, 0f, 1f);
			stopwatch.Start();
			for (int i = 0; i < 1000000; i++)
			{
				this.TrapezoidalIntegration(this.integrationSteps, 0f, 1f);
			}
			stopwatch.Stop();
			UnityEngine.Debug.Log(string.Format("trapezoidal ({0}): {1} {2}ms", this.integrationSteps, num, (float)stopwatch.ElapsedMilliseconds / 1000000f));
			num = this.RombergIntegration(3, 0f, 1f);
			stopwatch.Start();
			for (int i = 0; i < 1000000; i++)
			{
				this.RombergIntegration(3, 0f, 1f);
			}
			stopwatch.Stop();
			UnityEngine.Debug.Log(string.Format("romberg: {0} {1}ms", num, (float)stopwatch.ElapsedMilliseconds / 1000000f));
			yield break;
		}

		// Token: 0x0400013B RID: 315
		private static readonly int[] Pows2 = new int[]
		{
			1,
			2,
			4,
			8,
			16,
			32,
			64,
			128,
			256,
			512,
			1024
		};

		// Token: 0x0400013C RID: 316
		private static readonly int[] Pows4 = new int[]
		{
			1,
			4,
			16,
			64,
			256,
			1024,
			4096,
			16384,
			65536,
			262144,
			1048576
		};

		// Token: 0x0400013D RID: 317
		public AnimationCurve testCurve;

		// Token: 0x0400013E RID: 318
		public int integrationSteps = 200;
	}
}
