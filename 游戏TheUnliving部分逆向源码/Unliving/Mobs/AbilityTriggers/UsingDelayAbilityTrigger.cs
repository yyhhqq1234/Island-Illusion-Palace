using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000239 RID: 569
	[CreateAssetMenu(fileName = "UsingDelayAbilityTrigger", menuName = "Abilities/Triggers/Using Delay Trigger")]
	public sealed class UsingDelayAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000431 RID: 1073
		// (get) Token: 0x0600136D RID: 4973 RVA: 0x0003CC84 File Offset: 0x0003AE84
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000432 RID: 1074
		// (get) Token: 0x0600136E RID: 4974 RVA: 0x0003CC87 File Offset: 0x0003AE87
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000433 RID: 1075
		// (get) Token: 0x0600136F RID: 4975 RVA: 0x0003CC8E File Offset: 0x0003AE8E
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001370 RID: 4976 RVA: 0x0003CC91 File Offset: 0x0003AE91
		private void UpdateActivationTime()
		{
			this.activationTime = Time.time + this.delay;
		}

		// Token: 0x06001371 RID: 4977 RVA: 0x0003CCA5 File Offset: 0x0003AEA5
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return Time.time > this.activationTime;
		}

		// Token: 0x06001372 RID: 4978 RVA: 0x0003CCB4 File Offset: 0x0003AEB4
		protected override void ResetTrigger(BaseAbility ability)
		{
			base.ResetTrigger(ability);
			this.UpdateActivationTime();
		}

		// Token: 0x06001373 RID: 4979 RVA: 0x0003CCC3 File Offset: 0x0003AEC3
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.UpdateActivationTime();
		}

		// Token: 0x04000B41 RID: 2881
		public float delay = 2f;

		// Token: 0x04000B42 RID: 2882
		[Tooltip("Если неактивно, то таймер будет запущен только с того момента, когда достигнуто расстояние для применения абилити.")]
		public bool ignoreAbilityRange;

		// Token: 0x04000B43 RID: 2883
		private float activationTime;
	}
}
