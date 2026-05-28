using System;
using System.Collections.Generic;
using Common;
using Common.Factories;
using Game.Core;
using Game.PassiveAbilities;
using Unliving.Mobs;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A7 RID: 423
	public sealed class PassiveAbilitiesController : BasePassiveAbilitiesController, IInitializable<IEnumerable<IPassiveAbility>>, IInitializable<IEnumerable<PassiveAbilityID>>
	{
		// Token: 0x06000C09 RID: 3081 RVA: 0x00025DB0 File Offset: 0x00023FB0
		public PassiveAbilitiesController(BaseGameMob abilitiesOwner, Func<int, IPassiveAbility> abilityCreator = null) : base(abilitiesOwner, abilityCreator)
		{
			if (abilityCreator == null)
			{
				object obj;
				if (abilitiesOwner == null)
				{
					obj = null;
				}
				else
				{
					IGame currentGame = abilitiesOwner.CurrentGame;
					obj = ((currentGame != null) ? currentGame.Services.Get<IObjectFactory<PassiveAbilityBase>>() : null);
				}
				PassiveAbilityFactory passiveAbilityFactory = obj as PassiveAbilityFactory;
				if (passiveAbilityFactory != null)
				{
					this.abilityCreator = new Func<int, IPassiveAbility>(passiveAbilityFactory.Create);
				}
			}
		}

		// Token: 0x06000C0A RID: 3082 RVA: 0x00025E04 File Offset: 0x00024004
		public void Initialize(IEnumerable<IPassiveAbility> abilities)
		{
			if (abilities != null)
			{
				base.RemoveAllAbilities();
				foreach (IPassiveAbility passiveAbility in abilities)
				{
					if (passiveAbility != null)
					{
						base.AddAbilityInternal(passiveAbility);
					}
				}
			}
		}

		// Token: 0x06000C0B RID: 3083 RVA: 0x00025E58 File Offset: 0x00024058
		public void Initialize(IEnumerable<PassiveAbilityID> abilities)
		{
			if (abilities != null)
			{
				if (this.abilityCreator == null)
				{
					return;
				}
				base.RemoveAllAbilities();
				foreach (PassiveAbilityID arg in abilities)
				{
					base.AddAbilityInternal(this.abilityCreator((int)arg));
				}
			}
		}
	}
}
