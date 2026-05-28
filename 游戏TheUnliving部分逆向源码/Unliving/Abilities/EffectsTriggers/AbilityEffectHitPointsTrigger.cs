using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities.EffectsTriggers
{
	// Token: 0x020003D2 RID: 978
	[CreateAssetMenu(fileName = "AbilityEffectHitPointsTrigger", menuName = "Abilities/Effects Triggers/Hit Points Trigger")]
	public sealed class AbilityEffectHitPointsTrigger : AbilityEffectTriggerBase
	{
		// Token: 0x170006B3 RID: 1715
		// (get) Token: 0x06002122 RID: 8482 RVA: 0x0006806A File Offset: 0x0006626A
		// (set) Token: 0x06002123 RID: 8483 RVA: 0x00068072 File Offset: 0x00066272
		public float HitPointsThreshold
		{
			get
			{
				return this._hitPointsThreshold;
			}
			set
			{
				this._hitPointsThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x170006B4 RID: 1716
		// (get) Token: 0x06002124 RID: 8484 RVA: 0x00068080 File Offset: 0x00066280
		public override bool IsInverted
		{
			get
			{
				return this.triggerIfLess;
			}
		}

		// Token: 0x06002125 RID: 8485 RVA: 0x00068088 File Offset: 0x00066288
		public override bool IsFired(AbilityEffectBase effect, Component effectTarget)
		{
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			IDamageable damageable = ((baseGameMob != null) ? baseGameMob.HitPointsController : null) ?? effectTarget.CastOrGetComponent<IDamageable>();
			return damageable != null && damageable.GetNormalizedHitPoints() > this._hitPointsThreshold;
		}

		// Token: 0x040014B8 RID: 5304
		[SerializeField]
		[Range(0f, 1f)]
		private float _hitPointsThreshold = 0.5f;

		// Token: 0x040014B9 RID: 5305
		[SerializeField]
		private bool triggerIfLess = true;
	}
}
