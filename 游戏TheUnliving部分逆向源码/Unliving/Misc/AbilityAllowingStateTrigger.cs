using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Player;

namespace Unliving.Misc
{
	// Token: 0x0200023A RID: 570
	public class AbilityAllowingStateTrigger : GameBehaviourBase
	{
		// Token: 0x06001375 RID: 4981 RVA: 0x0003CCE8 File Offset: 0x0003AEE8
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				this.currentPlayer = playerProvider.CurrentPlayer;
				this.playerAbilitiesController = this.currentPlayer.AbilitiesController;
			}
		}

		// Token: 0x06001376 RID: 4982 RVA: 0x0003CD26 File Offset: 0x0003AF26
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!collider.HasSameGameObject(this.currentPlayer))
			{
				return;
			}
			if (this.allowAbilityOnTriggerEnter)
			{
				this.playerAbilitiesController.RemoveUnallowedAbilitiesDescription(this.abilityDescription);
				return;
			}
			this.playerAbilitiesController.AddUnallowedAbilitiesDescription(this.abilityDescription, true);
		}

		// Token: 0x06001377 RID: 4983 RVA: 0x0003CD64 File Offset: 0x0003AF64
		private void OnTriggerExit2D(Collider2D collider)
		{
			if (!collider.HasSameGameObject(this.currentPlayer))
			{
				return;
			}
			if (this.allowAbilityOnTriggerEnter)
			{
				this.playerAbilitiesController.AddUnallowedAbilitiesDescription(this.abilityDescription, true);
				return;
			}
			this.playerAbilitiesController.RemoveUnallowedAbilitiesDescription(this.abilityDescription);
		}

		// Token: 0x04000B44 RID: 2884
		public AbilityDescription abilityDescription;

		// Token: 0x04000B45 RID: 2885
		[Tooltip("Разрешить/запретить использование абилити при попадании игрока в триггер")]
		public bool allowAbilityOnTriggerEnter = true;

		// Token: 0x04000B46 RID: 2886
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000B47 RID: 2887
		private GameAbilitiesController playerAbilitiesController;
	}
}
