using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000382 RID: 898
	[Serializable]
	public sealed class AbilityPrepInterruptionEffect : AbilityEffectBase
	{
		// Token: 0x1700061C RID: 1564
		// (get) Token: 0x06001DA3 RID: 7587 RVA: 0x0005E29C File Offset: 0x0005C49C
		// (set) Token: 0x06001DA4 RID: 7588 RVA: 0x0005E2A4 File Offset: 0x0005C4A4
		public float PrepProgressReduction
		{
			get
			{
				return this._prepProgressReduction;
			}
			set
			{
				this._prepProgressReduction = Mathf.Clamp01(value);
			}
		}

		// Token: 0x1700061D RID: 1565
		// (get) Token: 0x06001DA5 RID: 7589 RVA: 0x0005E2B2 File Offset: 0x0005C4B2
		// (set) Token: 0x06001DA6 RID: 7590 RVA: 0x0005E2BA File Offset: 0x0005C4BA
		public bool ForceCompleteAbility
		{
			get
			{
				return this._forceCompleteAbility;
			}
			set
			{
				this._forceCompleteAbility = value;
			}
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x0005E2C3 File Offset: 0x0005C4C3
		public AbilityPrepInterruptionEffect()
		{
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x0005E2D6 File Offset: 0x0005C4D6
		public AbilityPrepInterruptionEffect(AbilityPrepInterruptionEffect effectPrototype)
		{
			this._prepProgressReduction = effectPrototype.PrepProgressReduction;
			this._forceCompleteAbility = effectPrototype.ForceCompleteAbility;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x0005E308 File Offset: 0x0005C508
		private void ModifyPrepProgress(IAbility targetAbility)
		{
			BaseAbility baseAbility = targetAbility as BaseAbility;
			if (baseAbility != null)
			{
				if (this._forceCompleteAbility)
				{
					baseAbility.Complete();
					return;
				}
				baseAbility.ModifyPreparationProgress(-baseAbility.PrepTime * this._prepProgressReduction);
			}
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x0005E344 File Offset: 0x0005C544
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this._prepProgressReduction > 0f)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				if (((baseGameMob != null) ? baseGameMob.AbilitiesController : null) != null)
				{
					baseGameMob.AbilitiesController.ForAll(new Action<IAbility>(this.ModifyPrepProgress));
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x0005E38D File Offset: 0x0005C58D
		protected override float GetEffectAmount()
		{
			return this._prepProgressReduction;
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x0005E395 File Offset: 0x0005C595
		protected override void SetEffectAmount(float newAmount)
		{
			this._prepProgressReduction = newAmount;
		}

		// Token: 0x06001DAD RID: 7597 RVA: 0x0005E39E File Offset: 0x0005C59E
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new AbilityPrepInterruptionEffect((AbilityPrepInterruptionEffect)originalBaseEffect);
		}

		// Token: 0x040010C7 RID: 4295
		[SerializeField]
		[Range(0f, 1f)]
		private float _prepProgressReduction = 1f;

		// Token: 0x040010C8 RID: 4296
		[SerializeField]
		private bool _forceCompleteAbility;
	}
}
