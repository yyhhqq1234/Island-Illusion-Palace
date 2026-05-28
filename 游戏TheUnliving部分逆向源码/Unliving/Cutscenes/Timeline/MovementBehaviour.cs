using System;
using UnityEngine;
using UnityEngine.Playables;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000330 RID: 816
	[Serializable]
	public sealed class MovementBehaviour : PlayableBehaviour
	{
		// Token: 0x06001B0B RID: 6923 RVA: 0x00055434 File Offset: 0x00053634
		public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			Transform transform = playerData as Transform;
			if (this.firstFrame)
			{
				this.firstFrame = false;
				this._movedThing = transform;
				this.start = transform.position;
				this.duration = (float)playable.GetDuration<Playable>() - this.overshootTime;
			}
			float num = (float)playable.GetTime<Playable>();
			if (num > this.duration + 0.5f * this.overshootTime)
			{
				num = this.duration + this.overshootTime - (num - this.duration);
			}
			Vector2 vector = this.start + this.startOffset;
			Vector2 vector2 = this.end + this.endOffset;
			float num2 = num / this.duration;
			float x = vector.x + (vector2.x - vector.x) * num2;
			float num3 = (vector.y + vector2.y) / 2f;
			float num4 = this.parabolaHeight;
			float y = vector.y + (vector2.y - vector.y) * num2 + this.parabolaHeight * (1f - Mathf.Abs(0.5f - num2) / 0.5f * (Mathf.Abs(0.5f - num2) / 0.5f));
			transform.transform.position = new Vector3(x, y, 0f);
		}

		// Token: 0x06001B0C RID: 6924 RVA: 0x00055584 File Offset: 0x00053784
		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			double num = playable.GetDuration<Playable>();
			double num2 = playable.GetTime<Playable>() + (double)info.deltaTime;
			bool flag = info.effectivePlayState == PlayState.Paused && num2 > num;
			bool flag2 = playable.GetGraph<Playable>().GetRootPlayable(0).IsDone<Playable>();
			if ((flag || flag2) && this._movedThing)
			{
				this._movedThing.transform.position = this.end + this.endOffset;
				this.firstFrame = true;
			}
		}

		// Token: 0x04000F2A RID: 3882
		public float parabolaHeight;

		// Token: 0x04000F2B RID: 3883
		public Vector2 end;

		// Token: 0x04000F2C RID: 3884
		public float overshootTime;

		// Token: 0x04000F2D RID: 3885
		private bool firstFrame = true;

		// Token: 0x04000F2E RID: 3886
		private Vector2 start;

		// Token: 0x04000F2F RID: 3887
		private float duration;

		// Token: 0x04000F30 RID: 3888
		private Transform _movedThing;

		// Token: 0x04000F31 RID: 3889
		public Vector2 startOffset;

		// Token: 0x04000F32 RID: 3890
		public Vector2 endOffset;
	}
}
