using System;
using UltEvents;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001B7 RID: 439
	public class CustomRevivableMob : MonoBehaviour, IRevivableGameMob
	{
		// Token: 0x1700028B RID: 651
		// (get) Token: 0x06000D83 RID: 3459 RVA: 0x0002AAA6 File Offset: 0x00028CA6
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x14000098 RID: 152
		// (add) Token: 0x06000D84 RID: 3460 RVA: 0x0002AAAC File Offset: 0x00028CAC
		// (remove) Token: 0x06000D85 RID: 3461 RVA: 0x0002AAE4 File Offset: 0x00028CE4
		public event Action<BaseGameMob, BaseGameMob> Revived;

		// Token: 0x06000D86 RID: 3462 RVA: 0x0002AB19 File Offset: 0x00028D19
		public bool CanBeRevived(BaseGameMob reviver, object context)
		{
			return base.enabled && reviver.IsValidReviver(this);
		}

		// Token: 0x06000D87 RID: 3463 RVA: 0x0002AB2C File Offset: 0x00028D2C
		public BaseGameMob Revive(BaseGameMob reviver, object context, bool destroySourceMob = true)
		{
			if (this.CanBeRevived(reviver, context))
			{
				UltEvent mobRevivedEvent = this.MobRevivedEvent;
				if (mobRevivedEvent != null)
				{
					mobRevivedEvent.Invoke();
				}
			}
			return null;
		}

		// Token: 0x040007BE RID: 1982
		public UltEvent MobRevivedEvent;
	}
}
