using System;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001F2 RID: 498
	[DisallowMultipleComponent]
	public sealed class GameMobGroup : GameMobGroupComponentBase<GameMobGroupController>
	{
		// Token: 0x0600106F RID: 4207 RVA: 0x0003363C File Offset: 0x0003183C
		protected override void Start()
		{
			base.Start();
			GameMobGroupController groupController = this._groupController;
			if (groupController.GroupDestinationsGenerator == null)
			{
				groupController.GroupDestinationsGenerator = base.GetComponent<IGroupDestinationsGenerator>();
			}
		}
	}
}
