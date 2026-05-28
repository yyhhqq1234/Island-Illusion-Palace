using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001EA RID: 490
	public interface IMobsSummoner
	{
		// Token: 0x17000350 RID: 848
		// (get) Token: 0x06001041 RID: 4161
		UnityEngine.Object SummonedMobsOwner { get; }

		// Token: 0x17000351 RID: 849
		// (get) Token: 0x06001042 RID: 4162
		IReadOnlyList<IGameMob> SummonedMobs { get; }

		// Token: 0x140000B5 RID: 181
		// (add) Token: 0x06001043 RID: 4163
		// (remove) Token: 0x06001044 RID: 4164
		event Action<object, IGameMob, Vector2> MobSummoned;

		// Token: 0x140000B6 RID: 182
		// (add) Token: 0x06001045 RID: 4165
		// (remove) Token: 0x06001046 RID: 4166
		event Action<object, IMobsSummoner, BaseAbility.UsingArgs> SummoningCompleted;
	}
}
