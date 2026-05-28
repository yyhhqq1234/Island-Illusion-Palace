using System;
using Game.Core;

namespace Unliving.DropSystem
{
	// Token: 0x02000299 RID: 665
	public class RandomDropSpawner : GameBehaviourBase
	{
		// Token: 0x060016E4 RID: 5860 RVA: 0x00049268 File Offset: 0x00047468
		private void Start()
		{
			DropGenerator dropGenerator;
			if (base.CurrentGame.Services.TryGet<DropGenerator>(out dropGenerator))
			{
				dropGenerator.SpawnRandomItem(this.allowedDropTypes, base.transform);
			}
		}

		// Token: 0x04000D42 RID: 3394
		public RandomDropItemType allowedDropTypes;
	}
}
