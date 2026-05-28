using System;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000231 RID: 561
	public sealed class MobAbilityTriggerArgs
	{
		// Token: 0x06001339 RID: 4921 RVA: 0x0003C8FE File Offset: 0x0003AAFE
		public void Reset()
		{
			this.aiController = null;
			this.target = null;
			this.targetDistance = 0f;
		}

		// Token: 0x04000B31 RID: 2865
		public GameMobAIController aiController;

		// Token: 0x04000B32 RID: 2866
		public IGameMob target;

		// Token: 0x04000B33 RID: 2867
		public float targetDistance;
	}
}
