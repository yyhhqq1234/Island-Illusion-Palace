using System;
using Game.Abilities;

namespace Unliving.Abilities
{
	// Token: 0x020003A9 RID: 937
	public interface IAbilityDecorator
	{
		// Token: 0x17000635 RID: 1589
		// (get) Token: 0x06001F17 RID: 7959
		BaseAbility AbilityPrototype { get; }

		// Token: 0x17000636 RID: 1590
		// (get) Token: 0x06001F18 RID: 7960
		BaseAbility AbilityInstance { get; }
	}
}
