using System;
using Common.Editor;
using Game.Stats;

namespace Unliving.MobsStats
{
	// Token: 0x02000073 RID: 115
	[Serializable]
	public struct TargetedMobStatModifier : ITargetedStatModifier<MobStatModifier>
	{
		// Token: 0x06000332 RID: 818 RVA: 0x0000BAF6 File Offset: 0x00009CF6
		public static implicit operator MobStatModifier(TargetedMobStatModifier modifier)
		{
			return modifier.ToStatModifier();
		}

		// Token: 0x06000333 RID: 819 RVA: 0x0000BB00 File Offset: 0x00009D00
		public static TargetedMobStatModifier GetDefault()
		{
			TargetedMobStatModifier result = new TargetedMobStatModifier
			{
				targetStat = MobStatID.Undefined
			};
			result.SetValue(MobStatModifier.Neutral);
			return result;
		}

		// Token: 0x17000099 RID: 153
		// (get) Token: 0x06000334 RID: 820 RVA: 0x0000BB2C File Offset: 0x00009D2C
		// (set) Token: 0x06000335 RID: 821 RVA: 0x0000BB34 File Offset: 0x00009D34
		public int TargetStatID
		{
			get
			{
				return (int)this.targetStat;
			}
			set
			{
				this.targetStat = (MobStatID)value;
			}
		}

		// Token: 0x06000336 RID: 822 RVA: 0x0000BB40 File Offset: 0x00009D40
		public void SetValue(MobStatModifier statModifier)
		{
			if (statModifier.AbsoluteModifier != 0f)
			{
				this.modifierType = MobStatModifierType.BaseValue;
				this.value = statModifier.AbsoluteModifier;
				return;
			}
			if (statModifier.BaseModifier != 0f)
			{
				this.modifierType = MobStatModifierType.BaseModifier;
				this.value = statModifier.BaseModifier;
				return;
			}
			this.modifierType = MobStatModifierType.ExtraModifier;
			this.value = statModifier.ExtraModifier;
		}

		// Token: 0x06000337 RID: 823 RVA: 0x0000BBA7 File Offset: 0x00009DA7
		public MobStatModifier ToStatModifier()
		{
			return this.modifierType.ToStatModifier(this.value);
		}

		// Token: 0x06000338 RID: 824 RVA: 0x0000BBBA File Offset: 0x00009DBA
		public void Invalidate()
		{
			this.targetStat = MobStatID.Undefined;
			this.value = 0f;
		}

		// Token: 0x06000339 RID: 825 RVA: 0x0000BBCE File Offset: 0x00009DCE
		public override string ToString()
		{
			return string.Format("{0}: {1}", this.targetStat, this.ToStatModifier());
		}

		// Token: 0x040001FC RID: 508
		[EnumPopup]
		public MobStatID targetStat;

		// Token: 0x040001FD RID: 509
		[EnumPopup]
		public MobStatModifierType modifierType;

		// Token: 0x040001FE RID: 510
		public float value;
	}
}
