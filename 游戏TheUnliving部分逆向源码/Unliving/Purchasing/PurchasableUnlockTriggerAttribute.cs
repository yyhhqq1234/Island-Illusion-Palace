using System;
using Common.Editor;

namespace Unliving.Purchasing
{
	// Token: 0x02000111 RID: 273
	public sealed class PurchasableUnlockTriggerAttribute : ManagedObjectFieldAttribute
	{
		// Token: 0x060006A1 RID: 1697 RVA: 0x00015CAC File Offset: 0x00013EAC
		public PurchasableUnlockTriggerAttribute() : base(typeof(PurchasableUnlockTriggerBase))
		{
		}
	}
}
