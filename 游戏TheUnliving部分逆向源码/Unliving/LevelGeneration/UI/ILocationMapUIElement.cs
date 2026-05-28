using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027C RID: 636
	public interface ILocationMapUIElement
	{
		// Token: 0x17000498 RID: 1176
		// (get) Token: 0x060015AC RID: 5548
		RectTransform Transform { get; }

		// Token: 0x17000499 RID: 1177
		// (get) Token: 0x060015AD RID: 5549
		// (set) Token: 0x060015AE RID: 5550
		Graphic MainRenderer { get; set; }

		// Token: 0x1700049A RID: 1178
		// (get) Token: 0x060015AF RID: 5551
		// (set) Token: 0x060015B0 RID: 5552
		LocationMapUI CurrentMapUI { get; set; }

		// Token: 0x1700049B RID: 1179
		// (get) Token: 0x060015B1 RID: 5553
		// (set) Token: 0x060015B2 RID: 5554
		RectTransform CurrentLayer { get; set; }
	}
}
