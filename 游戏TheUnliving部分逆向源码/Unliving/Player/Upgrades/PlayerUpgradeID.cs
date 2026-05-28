using System;
using Game.Localization;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000167 RID: 359
	[LocalizationObject(LocalizationPrefix.playerUpgradeID_)]
	public enum PlayerUpgradeID
	{
		// Token: 0x040005D1 RID: 1489
		None,
		// Token: 0x040005D2 RID: 1490
		RestoreStaminaOnSacrifice = 16,
		// Token: 0x040005D3 RID: 1491
		RestoreStaminaOnAbilityUsing,
		// Token: 0x040005D4 RID: 1492
		RestoreVitalEnergyOnMobSacrification = 32,
		// Token: 0x040005D5 RID: 1493
		RestoreVitalEnergyWithOwnHP,
		// Token: 0x040005D6 RID: 1494
		RestoreVitalEnergyOnHealing = 48,
		// Token: 0x040005D7 RID: 1495
		RestoreVitalEnergyOnMeleeAttack,
		// Token: 0x040005D8 RID: 1496
		BreakEnemyAbilityCastOnSacrifice = 64,
		// Token: 0x040005D9 RID: 1497
		DamageEnemyAbilityCastOnAttack,
		// Token: 0x040005DA RID: 1498
		IncreaseDamageOnArmyLoss = 80,
		// Token: 0x040005DB RID: 1499
		ActivateHealthContainerEffectOnArmyLoss,
		// Token: 0x040005DC RID: 1500
		SpawnMobOnDamage = 96,
		// Token: 0x040005DD RID: 1501
		GenerateMobsShieldOnArmyReduction,
		// Token: 0x040005DE RID: 1502
		DecreaseArmySizeIncreaseMobStats = 112,
		// Token: 0x040005DF RID: 1503
		IncreaseArmySizeDecreaseMobStats,
		// Token: 0x040005E0 RID: 1504
		AdditionalHPContainers = 4096,
		// Token: 0x040005E1 RID: 1505
		ContainersAmountIncrease,
		// Token: 0x040005E2 RID: 1506
		DamageResistTimeIncrease,
		// Token: 0x040005E3 RID: 1507
		ContainersPriceDecrease
	}
}
