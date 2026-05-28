using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000396 RID: 918
	[Serializable]
	public sealed class SummoningAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E48 RID: 7752 RVA: 0x0005FF96 File Offset: 0x0005E196
		public SummoningAbilityEffect()
		{
		}

		// Token: 0x06001E49 RID: 7753 RVA: 0x0005FFA8 File Offset: 0x0005E1A8
		public SummoningAbilityEffect(SummoningAbilityEffect effectPrototype)
		{
			GameMobsSummoner gameMobsSummoner = effectPrototype.mobsSummoner;
			this.mobsSummoner = ((gameMobsSummoner != null) ? gameMobsSummoner.Clone() : null);
			this.mobsCount = effectPrototype.mobsCount;
			this.forceRandomizePosition = effectPrototype.forceRandomizePosition;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E4A RID: 7754 RVA: 0x0005FFF9 File Offset: 0x0005E1F9
		protected override float GetEffectAmount()
		{
			return (float)this.mobsCount;
		}

		// Token: 0x06001E4B RID: 7755 RVA: 0x00060002 File Offset: 0x0005E202
		protected override void SetEffectAmount(float newAmount)
		{
			this.mobsCount = (int)newAmount;
		}

		// Token: 0x06001E4C RID: 7756 RVA: 0x0006000C File Offset: 0x0005E20C
		protected override bool Use(Component effectTarget, float dt)
		{
			return false;
		}

		// Token: 0x06001E4D RID: 7757 RVA: 0x0006000F File Offset: 0x0005E20F
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new SummoningAbilityEffect((SummoningAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E4E RID: 7758 RVA: 0x0006001C File Offset: 0x0005E21C
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			IGameMob gameMob = base.GetEffectOwner().CastOrGetComponent<IGameMob>();
			if (gameMob == null || this.mobsSummoner == null || !this.mobsSummoner.IsActive || this.mobsCount <= 0 || (!this.hasMultipleShotsPerUsing && base.WasUsed()))
			{
				return;
			}
			GameMobSummoningContext summoningContext = new GameMobSummoningContext
			{
				summoner = gameMob,
				summoningSource = base.CurrentAbility
			};
			Vector2 targetPosition = abilityUsingArgs.targetPosition;
			for (int i = 0; i < this.mobsCount; i++)
			{
				bool flag;
				this.mobsSummoner.SummonMob(summoningContext, targetPosition, out flag, this.forceRandomizePosition || this.mobsCount > 1);
			}
			base.MarkAsUsed();
		}

		// Token: 0x06001E4F RID: 7759 RVA: 0x000600C8 File Offset: 0x0005E2C8
		protected override void OnAbilityChanged(BaseAbility newAbility)
		{
			ProjectileAbilityBase projectileAbilityBase = newAbility as ProjectileAbilityBase;
			this.hasMultipleShotsPerUsing = (projectileAbilityBase != null && projectileAbilityBase.MaxShotsPerUsing > 1);
		}

		// Token: 0x04001112 RID: 4370
		public GameMobsSummoner mobsSummoner;

		// Token: 0x04001113 RID: 4371
		public int mobsCount = 1;

		// Token: 0x04001114 RID: 4372
		public bool forceRandomizePosition;

		// Token: 0x04001115 RID: 4373
		private bool hasMultipleShotsPerUsing;
	}
}
