using System;
using Game.Damage;

namespace Unliving.Mobs
{
	// Token: 0x020001E9 RID: 489
	public interface IGroupMobDamageFeedbackReceiver
	{
		// Token: 0x06001040 RID: 4160
		void OnGroupMobDamageApplied(IDamageable damagedObject, float damageAmount);
	}
}
