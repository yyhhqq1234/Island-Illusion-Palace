using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities.ComboAbilities
{
	// Token: 0x020003D5 RID: 981
	public sealed class ComboAbilityUsingContext
	{
		// Token: 0x06002174 RID: 8564 RVA: 0x00068AF0 File Offset: 0x00066CF0
		public Vector2 GetAbilityOwnerPosition()
		{
			return this.childAbility.ParentAbility.OwnerPosition;
		}

		// Token: 0x06002175 RID: 8565 RVA: 0x00068B07 File Offset: 0x00066D07
		public float GetChildAbilityRange()
		{
			return this.childAbility.AbilityInstance.Range;
		}

		// Token: 0x06002176 RID: 8566 RVA: 0x00068B19 File Offset: 0x00066D19
		public void Reset()
		{
			this.childAbility = null;
			this.anyChildAbilityTriggerWasFired = false;
			this.usingArgs = null;
		}

		// Token: 0x040014DE RID: 5342
		public ComboAbility.ChildAbility childAbility;

		// Token: 0x040014DF RID: 5343
		public bool anyChildAbilityTriggerWasFired;

		// Token: 0x040014E0 RID: 5344
		public BaseAbility.UsingArgs usingArgs;
	}
}
