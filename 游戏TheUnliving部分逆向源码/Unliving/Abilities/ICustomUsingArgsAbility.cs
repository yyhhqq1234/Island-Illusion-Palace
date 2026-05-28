using System;
using Common;
using Game.Abilities;

namespace Unliving.Abilities
{
	// Token: 0x020003AA RID: 938
	public interface ICustomUsingArgsAbility : IAbility, IDestroyable
	{
		// Token: 0x17000637 RID: 1591
		// (get) Token: 0x06001F19 RID: 7961
		BaseAbility.UsingArgs SourceUsingArgs { get; }

		// Token: 0x17000638 RID: 1592
		// (get) Token: 0x06001F1A RID: 7962
		BaseAbility.UsingArgs CurrentUsingArgs { get; }
	}
}
