using System;
using Game.Abilities;
using Game.Utility;

namespace Unliving.Mobs
{
	// Token: 0x02000200 RID: 512
	public sealed class RevivableMobsAreaWatcher : AreaWatcher<IRevivableGameMob>
	{
		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06001114 RID: 4372 RVA: 0x000356EE File Offset: 0x000338EE
		// (set) Token: 0x06001115 RID: 4373 RVA: 0x000356F6 File Offset: 0x000338F6
		public BaseAbility ParentAbility { get; set; }

		// Token: 0x06001116 RID: 4374 RVA: 0x00035700 File Offset: 0x00033900
		protected override bool IsObjectValid(IRevivableGameMob revivableMob)
		{
			if (base.IsObjectValid(revivableMob))
			{
				MobBehaviour mobBehaviour = revivableMob as MobBehaviour;
				return mobBehaviour == null || (mobBehaviour.IsRevivable && !mobBehaviour.IsAlive());
			}
			return false;
		}
	}
}
