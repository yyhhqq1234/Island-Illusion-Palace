using System;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000156 RID: 342
	[DisallowMultipleComponent]
	public sealed class PlayerMobGroup : GameMobGroupComponentBase<PlayerMobsGroupController>
	{
		// Token: 0x06000975 RID: 2421 RVA: 0x000203E0 File Offset: 0x0001E5E0
		private void Reset()
		{
			if (this._groupController != null)
			{
				this._groupController.Faction = GameMobFactions.PLAYER;
			}
		}

		// Token: 0x06000976 RID: 2422 RVA: 0x000203F8 File Offset: 0x0001E5F8
		protected override void Start()
		{
			PlayerBehaviour leader;
			if (base.TryGetComponent<PlayerBehaviour>(out leader))
			{
				this._groupController.Leader = leader;
			}
			base.Start();
		}
	}
}
