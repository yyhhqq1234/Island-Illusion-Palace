using System;
using UnityEngine;

namespace Unliving.LevelGeneration.UI
{
	// Token: 0x0200027B RID: 635
	public interface ILocationMapElement
	{
		// Token: 0x17000494 RID: 1172
		// (get) Token: 0x060015A6 RID: 5542
		Transform Transform { get; }

		// Token: 0x17000495 RID: 1173
		// (get) Token: 0x060015A7 RID: 5543
		Vector2 Position { get; }

		// Token: 0x17000496 RID: 1174
		// (get) Token: 0x060015A8 RID: 5544
		// (set) Token: 0x060015A9 RID: 5545
		SpriteRenderer MainRenderer { get; set; }

		// Token: 0x17000497 RID: 1175
		// (get) Token: 0x060015AA RID: 5546
		// (set) Token: 0x060015AB RID: 5547
		LocationMapRenderer CurrentMapRenderer { get; set; }
	}
}
