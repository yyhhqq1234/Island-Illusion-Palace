using System;
using UnityEngine;

namespace Unliving.MobsStats
{
	// Token: 0x02000066 RID: 102
	[Serializable]
	public struct MobStatModifier : IEquatable<MobStatModifier>
	{
		// Token: 0x060002DF RID: 735 RVA: 0x0000AE8D File Offset: 0x0000908D
		private static bool IsZero(float value)
		{
			return Mathf.Approximately(value, 0f);
		}

		// Token: 0x060002E0 RID: 736 RVA: 0x0000AE9A File Offset: 0x0000909A
		private static float ClampModifier(float modifier)
		{
			if (modifier >= 0f)
			{
				return modifier;
			}
			return 0f;
		}

		// Token: 0x060002E1 RID: 737 RVA: 0x0000AEAB File Offset: 0x000090AB
		public static MobStatModifier operator +(MobStatModifier m0, MobStatModifier m1)
		{
			m0.Combine(m1);
			return m0;
		}

		// Token: 0x060002E2 RID: 738 RVA: 0x0000AEB6 File Offset: 0x000090B6
		public static MobStatModifier operator -(MobStatModifier m0, MobStatModifier m1)
		{
			m0.Remove(m1);
			return m0;
		}

		// Token: 0x060002E3 RID: 739 RVA: 0x0000AEC1 File Offset: 0x000090C1
		public static bool operator ==(MobStatModifier m0, MobStatModifier m1)
		{
			return m0.Equals(m1);
		}

		// Token: 0x060002E4 RID: 740 RVA: 0x0000AECB File Offset: 0x000090CB
		public static bool operator !=(MobStatModifier m0, MobStatModifier m1)
		{
			return !m0.Equals(m1);
		}

		// Token: 0x17000090 RID: 144
		// (get) Token: 0x060002E5 RID: 741 RVA: 0x0000AED8 File Offset: 0x000090D8
		public float AbsoluteModifier
		{
			get
			{
				return this.absoulteModifier;
			}
		}

		// Token: 0x17000091 RID: 145
		// (get) Token: 0x060002E6 RID: 742 RVA: 0x0000AEE0 File Offset: 0x000090E0
		public float BaseModifier
		{
			get
			{
				return this.baseModifier;
			}
		}

		// Token: 0x17000092 RID: 146
		// (get) Token: 0x060002E7 RID: 743 RVA: 0x0000AEE8 File Offset: 0x000090E8
		public float ExtraModifier
		{
			get
			{
				if (this.zeroMultipliersCount == 0)
				{
					return this.extraModifier;
				}
				return 0f;
			}
		}

		// Token: 0x060002E8 RID: 744 RVA: 0x0000AEFE File Offset: 0x000090FE
		public MobStatModifier(float absoluteModifier, float baseModifier, float extraModifier)
		{
			this.absoulteModifier = absoluteModifier;
			this.baseModifier = baseModifier;
			this.extraModifier = extraModifier;
			this.zeroMultipliersCount = (MobStatModifier.IsZero(extraModifier) ? int.MaxValue : 0);
		}

		// Token: 0x060002E9 RID: 745 RVA: 0x0000AF2B File Offset: 0x0000912B
		public MobStatModifier(float absoluteModifier, float baseModifier)
		{
			this = new MobStatModifier(absoluteModifier, baseModifier, 1f);
		}

		// Token: 0x060002EA RID: 746 RVA: 0x0000AF3A File Offset: 0x0000913A
		private bool IsZeroByDefault()
		{
			return this.zeroMultipliersCount == int.MaxValue;
		}

		// Token: 0x060002EB RID: 747 RVA: 0x0000AF49 File Offset: 0x00009149
		public bool Equals(MobStatModifier other)
		{
			return this.absoulteModifier == other.AbsoluteModifier && this.baseModifier == other.BaseModifier && this.ExtraModifier == other.ExtraModifier;
		}

		// Token: 0x060002EC RID: 748 RVA: 0x0000AF7C File Offset: 0x0000917C
		public void Combine(MobStatModifier modifier)
		{
			this.absoulteModifier += modifier.AbsoluteModifier;
			this.baseModifier += modifier.BaseModifier;
			if (!this.IsZeroByDefault())
			{
				if (MobStatModifier.IsZero(modifier.ExtraModifier))
				{
					this.zeroMultipliersCount++;
					return;
				}
				this.extraModifier *= modifier.ExtraModifier;
			}
		}

		// Token: 0x060002ED RID: 749 RVA: 0x0000AFEC File Offset: 0x000091EC
		public void Remove(MobStatModifier modifier)
		{
			this.absoulteModifier -= modifier.AbsoluteModifier;
			this.baseModifier -= modifier.BaseModifier;
			if (!this.IsZeroByDefault())
			{
				if (MobStatModifier.IsZero(modifier.ExtraModifier))
				{
					this.zeroMultipliersCount = Mathf.Max(this.zeroMultipliersCount - 1, 0);
					return;
				}
				this.extraModifier /= modifier.ExtraModifier;
			}
		}

		// Token: 0x060002EE RID: 750 RVA: 0x0000B060 File Offset: 0x00009260
		public void Inverse()
		{
			this.absoulteModifier = -this.absoulteModifier;
			this.baseModifier = -this.baseModifier;
			this.extraModifier = ((this.extraModifier != 0f) ? (1f / this.extraModifier) : 0f);
		}

		// Token: 0x060002EF RID: 751 RVA: 0x0000B0AD File Offset: 0x000092AD
		public bool IsNeutral()
		{
			return this.Equals(MobStatModifier.Neutral);
		}

		// Token: 0x060002F0 RID: 752 RVA: 0x0000B0BC File Offset: 0x000092BC
		public override bool Equals(object obj)
		{
			if (obj is MobStatModifier)
			{
				MobStatModifier other = (MobStatModifier)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x060002F1 RID: 753 RVA: 0x0000B0E4 File Offset: 0x000092E4
		public override int GetHashCode()
		{
			return this.absoulteModifier.GetHashCode() ^ this.baseModifier.GetHashCode() << 2 ^ this.ExtraModifier.GetHashCode() >> 2;
		}

		// Token: 0x060002F2 RID: 754 RVA: 0x0000B11B File Offset: 0x0000931B
		public float GetModifiedStatValue(float initialStatValue)
		{
			return MobStatModifier.ClampModifier(initialStatValue + this.absoulteModifier) * MobStatModifier.ClampModifier(1f + this.baseModifier) * this.ExtraModifier;
		}

		// Token: 0x060002F3 RID: 755 RVA: 0x0000B144 File Offset: 0x00009344
		public override string ToString()
		{
			return string.Format("({0}, {1}{2}%, {3}x)", new object[]
			{
				this.absoulteModifier,
				(this.baseModifier >= 0f) ? "+" : "",
				Mathf.Round(this.baseModifier * 100f),
				this.ExtraModifier
			});
		}

		// Token: 0x040001EB RID: 491
		public static readonly MobStatModifier Neutral = new MobStatModifier(0f, 0f, 1f);

		// Token: 0x040001EC RID: 492
		[SerializeField]
		private float absoulteModifier;

		// Token: 0x040001ED RID: 493
		[SerializeField]
		private float baseModifier;

		// Token: 0x040001EE RID: 494
		[SerializeField]
		private float extraModifier;

		// Token: 0x040001EF RID: 495
		private int zeroMultipliersCount;
	}
}
