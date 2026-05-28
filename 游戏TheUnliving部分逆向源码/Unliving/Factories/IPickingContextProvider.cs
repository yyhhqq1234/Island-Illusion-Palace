using System;

namespace Unliving.Factories
{
	// Token: 0x020002C4 RID: 708
	public interface IPickingContextProvider
	{
		// Token: 0x17000540 RID: 1344
		// (get) Token: 0x060018B8 RID: 6328
		// (set) Token: 0x060018B9 RID: 6329
		MultiRepresentationObjectInstantiator.ObjectType CurrentPickingContext { get; set; }
	}
}
