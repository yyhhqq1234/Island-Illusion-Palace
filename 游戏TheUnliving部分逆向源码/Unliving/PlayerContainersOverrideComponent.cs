using System;
using System.Collections.Generic;
using Common.Editor;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving
{
	// Token: 0x02000015 RID: 21
	public class PlayerContainersOverrideComponent : GameBehaviourBase
	{
		// Token: 0x0600011C RID: 284 RVA: 0x00004E94 File Offset: 0x00003094
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				PlayerBehaviour currentPlayer = playerProvider.CurrentPlayer;
				IAbilityActivatedContainersController abilityActivatedContainersController = ((currentPlayer != null) ? currentPlayer.HitPointsController : null) as IAbilityActivatedContainersController;
				if (abilityActivatedContainersController != null)
				{
					abilityActivatedContainersController.SetContainersData(this.healthContainersOverrides);
				}
			}
		}

		// Token: 0x04000078 RID: 120
		[SerializeReference]
		[ManagedObjectField(typeof(IAbilityActivatedContainerData))]
		public List<IAbilityActivatedContainerData> healthContainersOverrides;
	}
}
