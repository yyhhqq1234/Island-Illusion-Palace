using System;
using Common;
using Game.Abilities;

namespace Unliving.Abilities
{
	// Token: 0x020003AE RID: 942
	public interface ITypedMobActivationAbility : IAbility, IDestroyable
	{
		// Token: 0x17000639 RID: 1593
		// (get) Token: 0x06001F20 RID: 7968
		MobActivationAbilityType ActivationAbilityType { get; }
	}
}
