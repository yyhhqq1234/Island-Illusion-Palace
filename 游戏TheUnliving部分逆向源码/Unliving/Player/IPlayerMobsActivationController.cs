using System;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200015B RID: 347
	public interface IPlayerMobsActivationController
	{
		// Token: 0x14000060 RID: 96
		// (add) Token: 0x0600099F RID: 2463
		// (remove) Token: 0x060009A0 RID: 2464
		event Action<bool> ActivationModeStateChanged;

		// Token: 0x14000061 RID: 97
		// (add) Token: 0x060009A1 RID: 2465
		// (remove) Token: 0x060009A2 RID: 2466
		event Action<BaseGameMob> MobSelected;

		// Token: 0x14000062 RID: 98
		// (add) Token: 0x060009A3 RID: 2467
		// (remove) Token: 0x060009A4 RID: 2468
		event Action<MobActivationAbilityType> MobActivationFailed;

		// Token: 0x060009A5 RID: 2469
		void OnUpdate();

		// Token: 0x060009A6 RID: 2470
		void SetSilentState(bool isActive);
	}
}
