using System;
using Game.Buffs;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000144 RID: 324
	public interface IMobsActivationAssistanceManager
	{
		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06000867 RID: 2151
		int HighlightedMobsCount { get; }

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06000868 RID: 2152
		// (set) Token: 0x06000869 RID: 2153
		float MobsNormalizedHPThreshold { get; set; }

		// Token: 0x17000160 RID: 352
		// (get) Token: 0x0600086A RID: 2154
		// (set) Token: 0x0600086B RID: 2155
		bool IsActive { get; set; }

		// Token: 0x0600086C RID: 2156
		IBuff GetAssistanceBuff(BaseGameMob mob, out IBuffsGenerator buffGenerator);

		// Token: 0x0600086D RID: 2157
		BaseGameMob GetClosestHighlightedMob(Vector2 point, MobActivationAbilityType activationType);
	}
}
