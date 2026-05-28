using System;
using Game.Stats;

namespace Unliving.MobsStats
{
	// Token: 0x02000064 RID: 100
	public abstract class MobStatBase : StatBase<MobStatModifier>
	{
		// Token: 0x1700008F RID: 143
		// (get) Token: 0x060002D7 RID: 727 RVA: 0x0000AD78 File Offset: 0x00008F78
		public MobStatModifier CurrentModifiers
		{
			get
			{
				return this.currentModifiers;
			}
		}

		// Token: 0x060002D8 RID: 728 RVA: 0x0000AD80 File Offset: 0x00008F80
		protected override float GetModifiedStatValue(float initialValue)
		{
			return this.currentModifiers.GetModifiedStatValue(initialValue);
		}

		// Token: 0x060002D9 RID: 729 RVA: 0x0000AD8E File Offset: 0x00008F8E
		public MobStatBase(MobStatID statID, object statOwner)
		{
			this.currentModifiers = MobStatModifier.Neutral;
			base.Name = statID.ToString();
		}

		// Token: 0x060002DA RID: 730 RVA: 0x0000ADB4 File Offset: 0x00008FB4
		public void UpdateInitialValue(bool restoreModifiers)
		{
			MobStatModifier modifier = this.currentModifiers;
			this.currentModifiers = MobStatModifier.Neutral;
			base.UpdateInitialValue();
			if (restoreModifiers)
			{
				this.AddModifier(modifier);
			}
		}

		// Token: 0x060002DB RID: 731 RVA: 0x0000ADE3 File Offset: 0x00008FE3
		public override void UpdateInitialValue()
		{
			this.UpdateInitialValue(true);
		}

		// Token: 0x060002DC RID: 732 RVA: 0x0000ADEC File Offset: 0x00008FEC
		public override void AddModifier(MobStatModifier modifier)
		{
			if (modifier.IsNeutral())
			{
				return;
			}
			this.currentModifiers += modifier;
			base.UpdateStatValue();
		}

		// Token: 0x060002DD RID: 733 RVA: 0x0000AE10 File Offset: 0x00009010
		public override void RemoveModifier(MobStatModifier modifier)
		{
			if (modifier.IsNeutral())
			{
				return;
			}
			this.currentModifiers -= modifier;
			base.UpdateStatValue();
		}

		// Token: 0x060002DE RID: 734 RVA: 0x0000AE34 File Offset: 0x00009034
		public override string ToString()
		{
			return string.Format("{0}: (value: {1}({2}) modifiers: {3} owner: {4})", new object[]
			{
				base.Name,
				this.CurrentValue,
				this.GetInitialValue(),
				this.currentModifiers,
				this.owner
			});
		}

		// Token: 0x040001A1 RID: 417
		private MobStatModifier currentModifiers;
	}
}
