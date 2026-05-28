using System;

namespace Unliving.Abilities.ComboAbilities.Triggers
{
	// Token: 0x020003D6 RID: 982
	[Serializable]
	public abstract class ComboAbilityTriggerBase
	{
		// Token: 0x06002178 RID: 8568
		protected abstract bool GetState(ComboAbilityUsingContext context);

		// Token: 0x06002179 RID: 8569 RVA: 0x00068B38 File Offset: 0x00066D38
		public bool IsFired(ComboAbilityUsingContext context)
		{
			ComboAbilityTriggerBase.ActuationType actuationType = this.actuationType;
			if (actuationType != ComboAbilityTriggerBase.ActuationType.IsFiringBeforeAnyTrigger)
			{
				if (actuationType == ComboAbilityTriggerBase.ActuationType.IsFiringAfterAnyTrigger)
				{
					if (!context.anyChildAbilityTriggerWasFired)
					{
						return false;
					}
				}
			}
			else if (context.anyChildAbilityTriggerWasFired)
			{
				return false;
			}
			return this.GetState(context);
		}

		// Token: 0x040014E1 RID: 5345
		public ComboAbilityTriggerBase.ActuationType actuationType;

		// Token: 0x02000595 RID: 1429
		public enum ActuationType
		{
			// Token: 0x04001CFE RID: 7422
			Default,
			// Token: 0x04001CFF RID: 7423
			IsFiringBeforeAnyTrigger,
			// Token: 0x04001D00 RID: 7424
			IsFiringAfterAnyTrigger
		}
	}
}
