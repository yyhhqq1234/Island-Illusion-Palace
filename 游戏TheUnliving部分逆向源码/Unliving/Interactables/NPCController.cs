using System;
using Unliving.Pickables;

namespace Unliving.Interactables
{
	// Token: 0x0200029B RID: 667
	public class NPCController : NPCControllerBase<NonFactoryPickableType>
	{
		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x060016EF RID: 5871 RVA: 0x00049477 File Offset: 0x00047677
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.None;
			}
		}
	}
}
