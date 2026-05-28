using System;

namespace Unliving.Player.Cheats
{
	// Token: 0x02000178 RID: 376
	[Serializable]
	public abstract class CheatBase
	{
		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06000A6B RID: 2667 RVA: 0x00022752 File Offset: 0x00020952
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06000A6C RID: 2668
		public abstract string ID { get; }

		// Token: 0x06000A6D RID: 2669
		protected abstract bool Activate(CheatContext context);

		// Token: 0x06000A6E RID: 2670
		protected abstract void Deactivate(CheatContext context);

		// Token: 0x06000A6F RID: 2671 RVA: 0x0002275A File Offset: 0x0002095A
		internal void SetActive(CheatContext context, bool isActive)
		{
			if (this.isActive != isActive)
			{
				if (isActive)
				{
					this.isActive = this.Activate(context);
					return;
				}
				this.Deactivate(context);
				this.isActive = false;
			}
		}

		// Token: 0x06000A70 RID: 2672 RVA: 0x00022784 File Offset: 0x00020984
		public bool HasID(string id)
		{
			return string.Equals(id, this.ID, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x04000612 RID: 1554
		private bool isActive;
	}
}
