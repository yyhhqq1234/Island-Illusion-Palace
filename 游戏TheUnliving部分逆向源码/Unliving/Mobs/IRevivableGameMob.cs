using System;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001EB RID: 491
	public interface IRevivableGameMob
	{
		// Token: 0x17000352 RID: 850
		// (get) Token: 0x06001047 RID: 4167
		Component Component { get; }

		// Token: 0x140000B7 RID: 183
		// (add) Token: 0x06001048 RID: 4168
		// (remove) Token: 0x06001049 RID: 4169
		event Action<BaseGameMob, BaseGameMob> Revived;

		// Token: 0x0600104A RID: 4170
		bool CanBeRevived(BaseGameMob reviver, object context);

		// Token: 0x0600104B RID: 4171
		BaseGameMob Revive(BaseGameMob reviver, object context, bool destroySourceMob = true);
	}
}
