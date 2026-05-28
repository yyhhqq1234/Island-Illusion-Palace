using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x020003AF RID: 943
	[CreateAssetMenu(fileName = "MobAbilityParameters", menuName = "Abilities/Extensions/Mob Ability Parameters")]
	public sealed class MobAbilityParameters : AbilityExtensionAssetBase
	{
		// Token: 0x1700063A RID: 1594
		// (get) Token: 0x06001F21 RID: 7969 RVA: 0x0006266B File Offset: 0x0006086B
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700063B RID: 1595
		// (get) Token: 0x06001F22 RID: 7970 RVA: 0x0006266E File Offset: 0x0006086E
		// (set) Token: 0x06001F23 RID: 7971 RVA: 0x00062676 File Offset: 0x00060876
		public float AbilityUsingDistanceMultiplier
		{
			get
			{
				return this._abilityUsingDistanceMultiplier;
			}
			set
			{
				this._abilityUsingDistanceMultiplier = Mathf.Clamp01(value);
			}
		}

		// Token: 0x1700063C RID: 1596
		// (get) Token: 0x06001F24 RID: 7972 RVA: 0x00062684 File Offset: 0x00060884
		// (set) Token: 0x06001F25 RID: 7973 RVA: 0x0006268C File Offset: 0x0006088C
		public float AimStoppingPrepThreshold
		{
			get
			{
				return this._aimStoppingPrepThreshold;
			}
			set
			{
				this._aimStoppingPrepThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x0006269A File Offset: 0x0006089A
		public bool IsAllowedTarget(Component target)
		{
			return !this.useDescriptionForTargetsFiltering || this.allowedTargetsDescription.IsMobComponentMatch(target);
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x000626B2 File Offset: 0x000608B2
		public bool TryPassAllowedTargetsFilter(ref GameMobDescription allowedTargetsDescription)
		{
			return this.useDescriptionForTargetsFiltering && this.allowedTargetsDescription.TryPassTo(ref allowedTargetsDescription);
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x000626CA File Offset: 0x000608CA
		public bool TryPassAllowedTargetsFilter(ref Predicate<Component> targetsFilter)
		{
			return this.useDescriptionForTargetsFiltering && this.allowedTargetsDescription.TryPassAsTargetsFilter(ref targetsFilter);
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x000626E2 File Offset: 0x000608E2
		public bool TryPassAllowedTargetsFilter(ref Predicate<BaseGameMob> targetsFilter)
		{
			return this.useDescriptionForTargetsFiltering && this.allowedTargetsDescription.TryPassAsTargetsFilter(ref targetsFilter);
		}

		// Token: 0x040013AF RID: 5039
		public GameAbilityUsingContext usingContext = GameAbilityUsingContext.BattleAbility;

		// Token: 0x040013B0 RID: 5040
		[SerializeField]
		[Range(0f, 1f)]
		private float _abilityUsingDistanceMultiplier = 0.95f;

		// Token: 0x040013B1 RID: 5041
		public float nextAbilityUsingDelay;

		// Token: 0x040013B2 RID: 5042
		[Obsolete]
		[HideInInspector]
		public bool canSupportAnyMobs;

		// Token: 0x040013B3 RID: 5043
		public bool forceIgnoreAggressionObstacles;

		// Token: 0x040013B4 RID: 5044
		public GameMobTargetSelector.SelectionMethod targetSelectionMethodOverride = GameMobTargetSelector.SelectionMethod.None;

		// Token: 0x040013B5 RID: 5045
		public GameMobTargetSelector.PrioritySelector targetSelectionPriorityOverride = GameMobTargetSelector.PrioritySelector.Default;

		// Token: 0x040013B6 RID: 5046
		public GameMobDescription allowedTargetsDescription = GameMobDescription.BlankDescription;

		// Token: 0x040013B7 RID: 5047
		public bool useDescriptionForTargetsFiltering;

		// Token: 0x040013B8 RID: 5048
		public bool useFakeShootPrediction;

		// Token: 0x040013B9 RID: 5049
		[SerializeField]
		[Range(0f, 1f)]
		private float _aimStoppingPrepThreshold;
	}
}
