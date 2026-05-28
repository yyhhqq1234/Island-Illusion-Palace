using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Player;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000D7 RID: 215
	public class PlayerPositionTrigger : GameBehaviourBase
	{
		// Token: 0x1400002F RID: 47
		// (add) Token: 0x0600054C RID: 1356 RVA: 0x00013208 File Offset: 0x00011408
		// (remove) Token: 0x0600054D RID: 1357 RVA: 0x00013240 File Offset: 0x00011440
		public event Action<PlayerBehaviour> PlayerTriggerEnter;

		// Token: 0x0600054E RID: 1358 RVA: 0x00013275 File Offset: 0x00011475
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			base.TryGetComponent<Collider2D>(out this.collider);
		}

		// Token: 0x0600054F RID: 1359 RVA: 0x0001328C File Offset: 0x0001148C
		private void OnTriggerEnter2D(Collider2D col)
		{
			if (!col.InLayerMask(this.playerLayerMask))
			{
				return;
			}
			PlayerBehaviour obj;
			if (col.TryGetComponent<PlayerBehaviour>(out obj))
			{
				Action<PlayerBehaviour> playerTriggerEnter = this.PlayerTriggerEnter;
				if (playerTriggerEnter == null)
				{
					return;
				}
				playerTriggerEnter(obj);
			}
		}

		// Token: 0x06000550 RID: 1360 RVA: 0x000132C8 File Offset: 0x000114C8
		public void Activate()
		{
			this.collider.enabled = true;
		}

		// Token: 0x06000551 RID: 1361 RVA: 0x000132D6 File Offset: 0x000114D6
		public void Deactivate()
		{
			this.collider.enabled = false;
			this.PlayerTriggerEnter = null;
		}

		// Token: 0x040003A1 RID: 929
		public LayerMask playerLayerMask;

		// Token: 0x040003A2 RID: 930
		private Collider2D collider;
	}
}
