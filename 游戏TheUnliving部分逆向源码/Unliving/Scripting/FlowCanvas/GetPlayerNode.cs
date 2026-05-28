using System;
using Common.UnityExtensions;
using FlowCanvas;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x0200009E RID: 158
	[Name("Get Player", 0)]
	[Category("Unliving/Player")]
	public sealed class GetPlayerNode : GameContextDependentNodeBase
	{
		// Token: 0x0600042C RID: 1068 RVA: 0x0000EA9C File Offset: 0x0000CC9C
		private PlayerBehaviour GetPlayer()
		{
			if (this.currentPlayer == null)
			{
				if (this.isPlayerScript.value)
				{
					this.currentPlayer = base.graphAgent.CastOrGetComponent<PlayerBehaviour>();
				}
				else
				{
					base.CurrentGame.TryGetPlayer(out this.currentPlayer);
				}
				this.currentPlayer == null;
			}
			return this.currentPlayer;
		}

		// Token: 0x0600042D RID: 1069 RVA: 0x0000EAFC File Offset: 0x0000CCFC
		private GameObject GetPlayerObject()
		{
			PlayerBehaviour player = this.GetPlayer();
			if (player == null)
			{
				return null;
			}
			return player.gameObject;
		}

		// Token: 0x0600042E RID: 1070 RVA: 0x0000EB10 File Offset: 0x0000CD10
		private Vector2 GetPlayerPosition()
		{
			PlayerBehaviour player = this.GetPlayer();
			if (!(player != null))
			{
				return default(Vector2);
			}
			return player.Position;
		}

		// Token: 0x0600042F RID: 1071 RVA: 0x0000EB40 File Offset: 0x0000CD40
		protected override void RegisterPorts()
		{
			base.AddValueOutput<PlayerBehaviour>("currentPlayer", new ValueHandler<PlayerBehaviour>(this.GetPlayer), "");
			base.AddValueOutput<GameObject>("playerObject", new ValueHandler<GameObject>(this.GetPlayerObject), "");
			base.AddValueOutput<Vector2>("playerPosition", new ValueHandler<Vector2>(this.GetPlayerPosition), "");
		}

		// Token: 0x040002A2 RID: 674
		public BBParameter<bool> isPlayerScript;

		// Token: 0x040002A3 RID: 675
		private PlayerBehaviour currentPlayer;
	}
}
