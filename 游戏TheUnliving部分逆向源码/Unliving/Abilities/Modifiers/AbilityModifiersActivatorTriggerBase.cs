using System;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C8 RID: 968
	public abstract class AbilityModifiersActivatorTriggerBase
	{
		// Token: 0x170006AC RID: 1708
		// (get) Token: 0x060020E5 RID: 8421 RVA: 0x00067798 File Offset: 0x00065998
		public bool IsFired
		{
			get
			{
				return this.isFired;
			}
		}

		// Token: 0x060020E6 RID: 8422
		protected abstract bool GetNewState(AbilityModifiersActivatorArgs args);

		// Token: 0x060020E7 RID: 8423 RVA: 0x000677A0 File Offset: 0x000659A0
		public bool UpdateState(AbilityModifiersActivatorArgs args)
		{
			if (!this.isLocked && !this.isFired)
			{
				this.isFired = this.GetNewState(args);
			}
			AbilityUsingStage abilityUsingStage = args.abilityUsingStage;
			this.isLocked = (abilityUsingStage == AbilityUsingStage.Preparing || abilityUsingStage == AbilityUsingStage.Prepared);
			return this.isFired;
		}

		// Token: 0x060020E8 RID: 8424 RVA: 0x000677E7 File Offset: 0x000659E7
		public virtual void Reset()
		{
			this.isLocked = false;
			this.isFired = false;
		}

		// Token: 0x0400149E RID: 5278
		private bool isLocked;

		// Token: 0x0400149F RID: 5279
		private bool isFired;
	}
}
