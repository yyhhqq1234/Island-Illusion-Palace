using System;
using Unliving.Essence;

namespace Unliving.Pickables
{
	// Token: 0x02000182 RID: 386
	public class PickableEssence : PickableObjectBase<EssenceType>
	{
		// Token: 0x170001D0 RID: 464
		// (get) Token: 0x06000ABA RID: 2746 RVA: 0x000235A8 File Offset: 0x000217A8
		public override EssenceType ID { get; }

		// Token: 0x06000ABB RID: 2747 RVA: 0x000235B0 File Offset: 0x000217B0
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return true;
		}
	}
}
