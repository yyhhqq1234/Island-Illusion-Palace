using System;
using Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Mobs.Motion
{
	// Token: 0x0200020F RID: 527
	[Serializable]
	public class GameMobImpulseDampingParams
	{
		// Token: 0x060011C4 RID: 4548 RVA: 0x00037AEF File Offset: 0x00035CEF
		public GameMobImpulseDampingParams(float absoluteDamping, float damping)
		{
			this.absoluteDamping = absoluteDamping;
			this.damping = damping;
		}

		// Token: 0x060011C5 RID: 4549 RVA: 0x00037B08 File Offset: 0x00035D08
		public void Apply(ref Vector2 impulse)
		{
			if (this.absoluteDamping != 0f)
			{
				float num = impulse.SqrMagnitude();
				if (num < this.absoluteDamping * this.absoluteDamping)
				{
					impulse.x = 0f;
					impulse.y = 0f;
					return;
				}
				num = Mathf.Sqrt(num);
				impulse = impulse / num * (num - Mathf.Abs(this.absoluteDamping));
			}
			if (this.damping != 0f)
			{
				impulse *= 1f - this.damping;
			}
		}

		// Token: 0x04000A29 RID: 2601
		public static readonly GameMobImpulseDampingParams Default = new GameMobImpulseDampingParams(0f, 0f);

		// Token: 0x04000A2A RID: 2602
		[FormerlySerializedAs("_absoluteDamping")]
		[ExportField("AbsoluteDamping")]
		public float absoluteDamping;

		// Token: 0x04000A2B RID: 2603
		[FormerlySerializedAs("_damping")]
		[Range(0f, 1f)]
		[ExportField("Damping")]
		public float damping;
	}
}
