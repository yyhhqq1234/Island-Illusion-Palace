using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000397 RID: 919
	[Serializable]
	public sealed class TriggerAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E50 RID: 7760 RVA: 0x000600F1 File Offset: 0x0005E2F1
		public TriggerAbilityEffect()
		{
		}

		// Token: 0x06001E51 RID: 7761 RVA: 0x00060100 File Offset: 0x0005E300
		public TriggerAbilityEffect(TriggerAbilityEffect effectPrototype)
		{
			this.affectableLayers = effectPrototype.affectableLayers;
			this.affectableFaction = effectPrototype.affectableFaction;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x0006012E File Offset: 0x0005E32E
		private bool HasAffectableLayer(Component effectTarget)
		{
			return this.affectableLayers == 0 || effectTarget.InLayerMask(this.affectableLayers);
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x00060150 File Offset: 0x0005E350
		private bool HasAffectableFaction(Component effectTarget)
		{
			if (this.affectableFaction == GameMobFactions.None)
			{
				return true;
			}
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			return baseGameMob == null || baseGameMob.Faction == this.affectableFaction;
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x00060188 File Offset: 0x0005E388
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.HasAffectableLayer(effectTarget) || this.HasAffectableFaction(effectTarget))
			{
				base.NotifyEffectUsed(effectTarget, 0f);
				return true;
			}
			return false;
		}

		// Token: 0x06001E55 RID: 7765 RVA: 0x000601AB File Offset: 0x0005E3AB
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001E56 RID: 7766 RVA: 0x000601B2 File Offset: 0x0005E3B2
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x000601B4 File Offset: 0x0005E3B4
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new TriggerAbilityEffect((TriggerAbilityEffect)originalBaseEffect);
		}

		// Token: 0x04001116 RID: 4374
		[Tooltip("Если не задано, то эффект будет применен ко всем слоям.")]
		public LayerMask affectableLayers;

		// Token: 0x04001117 RID: 4375
		[Tooltip("Если не задано, то эффект будет применен ко всем фракциям.")]
		public GameMobFactions affectableFaction = GameMobFactions.None;
	}
}
