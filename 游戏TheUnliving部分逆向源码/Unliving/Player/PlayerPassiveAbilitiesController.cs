using System;
using Common.Factories;
using Game.Core;
using Game.PassiveAbilities;
using UnityEngine;
using Unliving.PassiveAbilities;

namespace Unliving.Player
{
	// Token: 0x02000160 RID: 352
	[Serializable]
	public sealed class PlayerPassiveAbilitiesController : BasePassiveAbilitiesController
	{
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x060009DE RID: 2526 RVA: 0x00021BCB File Offset: 0x0001FDCB
		public PlayerBehaviour CurrentPlayer { get; }

		// Token: 0x060009DF RID: 2527 RVA: 0x00021BD3 File Offset: 0x0001FDD3
		public PlayerPassiveAbilitiesController(PlayerBehaviour player, IGame currentGame, Func<int, PassiveAbilityBase> abilityCreator = null) : base(player, abilityCreator)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.CurrentPlayer = player;
			if (currentGame != null)
			{
				currentGame.Services.Get<IObjectFactory<PassiveAbilityBase>>();
			}
		}

		// Token: 0x060009E0 RID: 2528 RVA: 0x00021BFC File Offset: 0x0001FDFC
		public void Initialize(PlayerPassiveAbilitiesController.AbilityInfo[] passiveAbilities)
		{
			base.RemoveAllAbilities();
			foreach (PlayerPassiveAbilitiesController.AbilityInfo abilityInfo in passiveAbilities)
			{
				if (abilityInfo.abilityCount > 0)
				{
					int abilityID = (int)abilityInfo.abilityID;
					for (int j = 0; j < abilityInfo.abilityCount; j++)
					{
						base.AddAbility(abilityID);
					}
				}
			}
		}

		// Token: 0x02000467 RID: 1127
		[Serializable]
		public struct AbilityInfo
		{
			// Token: 0x04001738 RID: 5944
			public PassiveAbilityID abilityID;

			// Token: 0x04001739 RID: 5945
			public int abilityCount;
		}
	}
}
