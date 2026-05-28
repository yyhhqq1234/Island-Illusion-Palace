using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x0200014A RID: 330
	[CreateAssetMenu(fileName = "PlayerAbilityParameters", menuName = "Abilities/Extensions/Player Ability Parameters")]
	public sealed class PlayerAbilityParameters : AbilityExtensionAssetBase
	{
		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060008FE RID: 2302 RVA: 0x0001E394 File Offset: 0x0001C594
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x04000500 RID: 1280
		public bool isForcedUsingAbility;

		// Token: 0x04000501 RID: 1281
		public bool isMainBattleAbility;

		// Token: 0x04000502 RID: 1282
		public bool canBePartiallyPrepared;

		// Token: 0x04000503 RID: 1283
		public bool preventAutoUsing;
	}
}
