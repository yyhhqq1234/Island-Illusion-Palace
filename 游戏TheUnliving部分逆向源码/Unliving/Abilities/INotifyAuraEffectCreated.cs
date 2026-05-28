using System;
using Game.Abilities;

namespace Unliving.Abilities
{
	// Token: 0x020003AC RID: 940
	public interface INotifyAuraEffectCreated
	{
		// Token: 0x06001F1E RID: 7966
		void OnAuraEffectCreated(BaseAbility ability, AbilityEffectZone auraEffect);
	}
}
