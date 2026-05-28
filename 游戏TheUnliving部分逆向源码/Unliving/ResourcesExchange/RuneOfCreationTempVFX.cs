using System;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000DA RID: 218
	public class RuneOfCreationTempVFX : GameBehaviourBase
	{
		// Token: 0x0600055D RID: 1373 RVA: 0x0001357C File Offset: 0x0001177C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.rune = base.GetComponent<RuneOfCreation>();
			this.rune.MobSacrificed += this.OnMobSacrificed;
			this.rune.RewardSpawned += this.OnRewardSpawned;
		}

		// Token: 0x0600055E RID: 1374 RVA: 0x000135CC File Offset: 0x000117CC
		private void OnRewardSpawned()
		{
			SpriteRenderer[] array = this.sacrificeTriggersSptires;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].enabled = false;
			}
			this.runeSprite.enabled = false;
			this.rewardPositionSprite.enabled = true;
		}

		// Token: 0x0600055F RID: 1375 RVA: 0x0001360F File Offset: 0x0001180F
		private void OnMobSacrificed(BaseGameMob mob, SacrificePositionTrigger trigger)
		{
		}

		// Token: 0x06000560 RID: 1376 RVA: 0x00013611 File Offset: 0x00011811
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.rune.MobSacrificed -= this.OnMobSacrificed;
			this.rune.RewardSpawned -= this.OnRewardSpawned;
		}

		// Token: 0x040003AA RID: 938
		public SpriteRenderer[] sacrificeTriggersSptires;

		// Token: 0x040003AB RID: 939
		public SpriteRenderer runeSprite;

		// Token: 0x040003AC RID: 940
		public SpriteRenderer rewardPositionSprite;

		// Token: 0x040003AD RID: 941
		private RuneOfCreation rune;
	}
}
