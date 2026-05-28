using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Player;

namespace Unliving.Misc
{
	// Token: 0x0200023B RID: 571
	public class AllowedAbilityTrigger : GameBehaviourBase
	{
		// Token: 0x06001379 RID: 4985 RVA: 0x0003CDB4 File Offset: 0x0003AFB4
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				this.currentPlayer = playerProvider.CurrentPlayer;
				this.playerAbilitiesController = this.currentPlayer.AbilitiesController;
			}
		}

		// Token: 0x0600137A RID: 4986 RVA: 0x0003CDF2 File Offset: 0x0003AFF2
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!collider.HasSameGameObject(this.currentPlayer))
			{
				return;
			}
			this.playerAbilitiesController.SetAllowedAbilityDescription(this.allowedAbilityDescription, true);
		}

		// Token: 0x0600137B RID: 4987 RVA: 0x0003CE15 File Offset: 0x0003B015
		private void OnTriggerExit2D(Collider2D collider)
		{
			if (!collider.HasSameGameObject(this.currentPlayer))
			{
				return;
			}
			this.playerAbilitiesController.ResetAllowedAbilitiesDescription();
		}

		// Token: 0x04000B48 RID: 2888
		public AbilityDescription allowedAbilityDescription;

		// Token: 0x04000B49 RID: 2889
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000B4A RID: 2890
		private GameAbilitiesController playerAbilitiesController;
	}
}
