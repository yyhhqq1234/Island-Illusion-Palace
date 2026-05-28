using System;
using Game.Buffs;
using Game.Damage;
using UnityEngine;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003E1 RID: 993
	[Serializable]
	public sealed class OwnerDeathBuffInterruptionSource : IBuffInterruptionSource, ICloneable
	{
		// Token: 0x060021A3 RID: 8611 RVA: 0x0006921C File Offset: 0x0006741C
		public void Initialize(IBuff buff)
		{
			Component component = buff.Sender as Component;
			this.damageReceiver = ((component != null) ? component.GetComponent<IDamageable>() : null);
		}

		// Token: 0x060021A4 RID: 8612 RVA: 0x0006923B File Offset: 0x0006743B
		public bool InterruptBuff(IBuff buff)
		{
			if (!this.interruptBuff)
			{
				this.interruptBuff = (this.damageReceiver != null && !this.damageReceiver.IsAlive);
			}
			return this.interruptBuff;
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x0006926A File Offset: 0x0006746A
		public object Clone()
		{
			return new OwnerDeathBuffInterruptionSource();
		}

		// Token: 0x040014F9 RID: 5369
		private IDamageable damageReceiver;

		// Token: 0x040014FA RID: 5370
		private bool interruptBuff;
	}
}
