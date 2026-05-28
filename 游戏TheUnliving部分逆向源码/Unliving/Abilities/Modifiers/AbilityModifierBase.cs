using System;
using System.Collections.Generic;
using Common;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003BE RID: 958
	public abstract class AbilityModifierBase : ICloneable<AbilityModifierBase>
	{
		// Token: 0x0600205A RID: 8282 RVA: 0x00065FA8 File Offset: 0x000641A8
		public static void UseModifiers(AbilityModifierBase[] modifiers, AbilityModifierUsingArgs modifiersUsingArgs)
		{
			foreach (AbilityModifierBase abilityModifierBase in modifiers)
			{
				if (abilityModifierBase != null)
				{
					abilityModifierBase.Use(modifiersUsingArgs);
				}
			}
		}

		// Token: 0x17000698 RID: 1688
		// (get) Token: 0x0600205B RID: 8283
		public abstract bool IsActive { get; }

		// Token: 0x1400011E RID: 286
		// (add) Token: 0x0600205C RID: 8284 RVA: 0x00065FD4 File Offset: 0x000641D4
		// (remove) Token: 0x0600205D RID: 8285 RVA: 0x0006600C File Offset: 0x0006420C
		public event Action<AbilityModifierUsingArgs> Used;

		// Token: 0x0600205E RID: 8286 RVA: 0x00066041 File Offset: 0x00064241
		protected AbilityModifierBase(AbilityModifierBase modifierPrototype)
		{
		}

		// Token: 0x0600205F RID: 8287
		protected abstract void OnUse(AbilityModifierUsingArgs usingArgs);

		// Token: 0x06002060 RID: 8288
		protected abstract void OnReset(AbilityModifierUsingArgs usingArgs);

		// Token: 0x06002061 RID: 8289 RVA: 0x0006604C File Offset: 0x0006424C
		protected void UseOnTargets(AbilityModifierUsingArgs modifierUsingArgs, Action<AbilityModifierUsingArgs, Component> targetAction)
		{
			BaseAbility.UsingArgs targetsInfo = modifierUsingArgs.targetsInfo;
			int targetsCount = targetsInfo.TargetsCount;
			if (targetsCount != 0)
			{
				IList<Component> targetsList = targetsInfo.targetsList;
				for (int i = 0; i < targetsCount; i++)
				{
					Component component = targetsList[i];
					if (component != null)
					{
						targetAction(modifierUsingArgs, component);
					}
				}
				return;
			}
			if (targetsInfo.HasTargetObject)
			{
				Component targetObject = targetsInfo.targetObject;
				targetAction(modifierUsingArgs, targetObject);
			}
		}

		// Token: 0x06002062 RID: 8290 RVA: 0x000660B3 File Offset: 0x000642B3
		public virtual bool CanBeUsed(AbilityModifierUsingArgs usingArgs)
		{
			return this.IsActive && usingArgs.modifiersUsingCount > 0;
		}

		// Token: 0x06002063 RID: 8291
		public abstract AbilityModifierBase Clone();

		// Token: 0x06002064 RID: 8292 RVA: 0x000660C8 File Offset: 0x000642C8
		public void Use(AbilityModifierUsingArgs usingArgs)
		{
			if (this.CanBeUsed(usingArgs))
			{
				this.OnUse(usingArgs);
				Action<AbilityModifierUsingArgs> used = this.Used;
				if (used == null)
				{
					return;
				}
				used(usingArgs);
			}
		}

		// Token: 0x06002065 RID: 8293 RVA: 0x000660EB File Offset: 0x000642EB
		public void Reset(AbilityModifierUsingArgs usingArgs)
		{
			this.OnReset(usingArgs);
		}

		// Token: 0x06002066 RID: 8294 RVA: 0x000660F4 File Offset: 0x000642F4
		public virtual void OnAddedToAbility(BaseAbility ability)
		{
		}

		// Token: 0x06002067 RID: 8295 RVA: 0x000660F6 File Offset: 0x000642F6
		public virtual void OnRemovedFromAbility(BaseAbility ability)
		{
		}
	}
}
