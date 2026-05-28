using System;
using System.Collections.Generic;
using Game.Buffs;
using Unliving.Mobs;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003D8 RID: 984
	public sealed class AbilitiesUsingRestrictionBuff : BuffBase
	{
		// Token: 0x0600217E RID: 8574 RVA: 0x00068C5B File Offset: 0x00066E5B
		private GameAbilitiesController GetAbilitiesController(object buffTarget)
		{
			BaseGameMob baseGameMob = buffTarget as BaseGameMob;
			if (baseGameMob == null)
			{
				return null;
			}
			return baseGameMob.AbilitiesController;
		}

		// Token: 0x0600217F RID: 8575 RVA: 0x00068C70 File Offset: 0x00066E70
		protected override void OnActivated()
		{
			for (int i = 0; i < this.restrictedAbilitiesDescriptions.Count; i++)
			{
				this.GetAbilitiesController(base.TargetObject).AddUnallowedAbilitiesDescription(this.restrictedAbilitiesDescriptions[i], this.TryToBlockAbilitiesImmediately);
			}
		}

		// Token: 0x06002180 RID: 8576 RVA: 0x00068CB6 File Offset: 0x00066EB6
		protected override void UseBuff(float updateStep)
		{
		}

		// Token: 0x06002181 RID: 8577 RVA: 0x00068CB8 File Offset: 0x00066EB8
		protected override void CancelBuff()
		{
			for (int i = 0; i < this.restrictedAbilitiesDescriptions.Count; i++)
			{
				this.GetAbilitiesController(base.TargetObject).RemoveUnallowedAbilitiesDescription(this.restrictedAbilitiesDescriptions[i]);
			}
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x00068CF9 File Offset: 0x00066EF9
		public override bool IsValidBuffTarget(object targetObject)
		{
			return base.IsValidBuffTarget(targetObject) && this.GetAbilitiesController(targetObject) != null;
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x00068D10 File Offset: 0x00066F10
		public AbilitiesUsingRestrictionBuff(int id, object buffSender, BuffBase.InitializationArgs initializationArgs, IList<AbilityDescription> abilitiesDescriptions, bool tryToBlockAbilitiesImmediately) : base(id, buffSender, initializationArgs)
		{
			this.TryToBlockAbilitiesImmediately = tryToBlockAbilitiesImmediately;
			this.restrictedAbilitiesDescriptions = abilitiesDescriptions;
		}

		// Token: 0x040014E9 RID: 5353
		public readonly bool TryToBlockAbilitiesImmediately;

		// Token: 0x040014EA RID: 5354
		private readonly IList<AbilityDescription> restrictedAbilitiesDescriptions;
	}
}
