using System;
using Game.Abilities;
using Game.Core;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x020003A6 RID: 934
	public sealed class GameAbilityInstantiator : GameBehaviourBase
	{
		// Token: 0x17000633 RID: 1587
		// (get) Token: 0x06001F10 RID: 7952 RVA: 0x000625EE File Offset: 0x000607EE
		// (set) Token: 0x06001F11 RID: 7953 RVA: 0x000625F6 File Offset: 0x000607F6
		public BaseAbility AbilityPrototype
		{
			get
			{
				return this.abilityPrototype;
			}
			set
			{
				this.abilityPrototype = value;
			}
		}

		// Token: 0x17000634 RID: 1588
		// (get) Token: 0x06001F12 RID: 7954 RVA: 0x000625FF File Offset: 0x000607FF
		// (set) Token: 0x06001F13 RID: 7955 RVA: 0x00062607 File Offset: 0x00060807
		public IGameAbilitiesFactory AbilitiesFactory
		{
			get
			{
				return this.abilitiesFactory;
			}
			set
			{
				this.abilitiesFactory = value;
			}
		}

		// Token: 0x06001F14 RID: 7956 RVA: 0x00062610 File Offset: 0x00060810
		public BaseAbility InstantiateAbility(AbilityFactoryArgs args)
		{
			if (this.abilitiesFactory == null && !base.CurrentGame.Services.TryGet<IGameAbilitiesFactory>(out this.abilitiesFactory))
			{
				return null;
			}
			args.abilityID = AbilityID.None;
			args.abilityPrototype = this.abilityPrototype;
			return (BaseAbility)this.abilitiesFactory.Create(args);
		}

		// Token: 0x040013A8 RID: 5032
		[SerializeField]
		private BaseAbility abilityPrototype;

		// Token: 0x040013A9 RID: 5033
		private IGameAbilitiesFactory abilitiesFactory;
	}
}
