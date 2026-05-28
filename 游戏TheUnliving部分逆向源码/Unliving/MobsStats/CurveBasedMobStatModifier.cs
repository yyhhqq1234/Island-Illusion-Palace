using System;
using UnityEngine;

namespace Unliving.MobsStats
{
	// Token: 0x02000057 RID: 87
	[Serializable]
	public struct CurveBasedMobStatModifier
	{
		// Token: 0x060002AC RID: 684 RVA: 0x0000A931 File Offset: 0x00008B31
		public float GetValue(float t)
		{
			return this.valueCurve.Evaluate(t);
		}

		// Token: 0x060002AD RID: 685 RVA: 0x0000A940 File Offset: 0x00008B40
		public TargetedMobStatModifier ToTargetedStatModifier(float t)
		{
			return new TargetedMobStatModifier
			{
				targetStat = this.targetStat,
				modifierType = this.modifierType,
				value = this.GetValue(t)
			};
		}

		// Token: 0x060002AE RID: 686 RVA: 0x0000A97E File Offset: 0x00008B7E
		public MobStatModifier ToStatModifier(float t)
		{
			return this.modifierType.ToStatModifier(this.GetValue(t));
		}

		// Token: 0x04000191 RID: 401
		public MobStatID targetStat;

		// Token: 0x04000192 RID: 402
		public MobStatModifierType modifierType;

		// Token: 0x04000193 RID: 403
		public AnimationCurve valueCurve;
	}
}
