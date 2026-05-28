using System;
using UnityEngine;
using Unliving.DropSystem;

namespace Unliving.Purchasing
{
	// Token: 0x020000FF RID: 255
	public class IngameStoreObjectsSpawner : DropSpawner
	{
		// Token: 0x06000631 RID: 1585 RVA: 0x00015013 File Offset: 0x00013213
		protected override bool SpawnCurrentContextObject(ref IDropable dropable, Transform parentTransform)
		{
			return this.dropGenerator.SpawnIngameStoreItem(dropable, parentTransform);
		}
	}
}
