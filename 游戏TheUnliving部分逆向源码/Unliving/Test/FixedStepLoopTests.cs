using System;
using Game.Gameplay;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x0200003A RID: 58
	public sealed class FixedStepLoopTests : MonoBehaviour
	{
		// Token: 0x06000206 RID: 518 RVA: 0x00007F04 File Offset: 0x00006104
		private void Start()
		{
			this.loop = new FixedStepTimer(this.duration, this.steps);
			this.loop.Reset();
			this.sum = 0f;
			Application.targetFrameRate = this.frameRate;
			Debug.Log(string.Format("{0}", this.loop.duration / this.loop.step));
		}

		// Token: 0x06000207 RID: 519 RVA: 0x00007F74 File Offset: 0x00006174
		private void Update()
		{
			if (Time.time < 5f)
			{
				return;
			}
			float num = this.testValue / this.duration;
			float num2;
			int num3;
			if (this.loop.Tick(out num2, out num3))
			{
				while (this.totalSteps < num3)
				{
					this.totalSteps++;
				}
				this.sum += num * num2;
				Debug.Log(string.Format("sum: {0}  {1}", this.sum, this.totalSteps));
			}
		}

		// Token: 0x04000114 RID: 276
		public float duration = 0.5f;

		// Token: 0x04000115 RID: 277
		public int steps = 45;

		// Token: 0x04000116 RID: 278
		public float testValue = 100f;

		// Token: 0x04000117 RID: 279
		public int frameRate = -1;

		// Token: 0x04000118 RID: 280
		private FixedStepTimer loop;

		// Token: 0x04000119 RID: 281
		private float sum;

		// Token: 0x0400011A RID: 282
		private int totalSteps;
	}
}
