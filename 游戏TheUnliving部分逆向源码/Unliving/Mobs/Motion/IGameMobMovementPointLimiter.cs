using System;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x02000217 RID: 535
	public interface IGameMobMovementPointLimiter
	{
		// Token: 0x170003E5 RID: 997
		// (get) Token: 0x0600124E RID: 4686
		Collider2D Area { get; }

		// Token: 0x170003E6 RID: 998
		// (get) Token: 0x0600124F RID: 4687
		// (set) Token: 0x06001250 RID: 4688
		bool IsActive { get; set; }

		// Token: 0x06001251 RID: 4689
		Vector2? LimitMovementPoint(Vector2 movementPoint);
	}
}
