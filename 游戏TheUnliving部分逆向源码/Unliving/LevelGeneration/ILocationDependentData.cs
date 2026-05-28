using System;
using UnityEngine;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026B RID: 619
	public interface ILocationDependentData
	{
		// Token: 0x1700045F RID: 1119
		// (get) Token: 0x060014D9 RID: 5337
		GameLocation.TypeID LocationID { get; }

		// Token: 0x17000460 RID: 1120
		// (get) Token: 0x060014DA RID: 5338
		UnityEngine.Object Data { get; }
	}
}
