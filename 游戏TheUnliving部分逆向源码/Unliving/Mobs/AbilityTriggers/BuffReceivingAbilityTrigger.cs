using System;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000228 RID: 552
	[CreateAssetMenu(fileName = "BuffReceivingAbilityTrigger", menuName = "Abilities/Triggers/Buff Receiving Trigger")]
	public sealed class BuffReceivingAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x170003FC RID: 1020
		// (get) Token: 0x060012E9 RID: 4841 RVA: 0x0003C05A File Offset: 0x0003A25A
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170003FD RID: 1021
		// (get) Token: 0x060012EA RID: 4842 RVA: 0x0003C05D File Offset: 0x0003A25D
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170003FE RID: 1022
		// (get) Token: 0x060012EB RID: 4843 RVA: 0x0003C064 File Offset: 0x0003A264
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x060012EC RID: 4844 RVA: 0x0003C068 File Offset: 0x0003A268
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (this.expectedBuffsGeneratorAsset != null)
			{
				BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
				IBuffsController buffsController = (baseGameMob != null) ? baseGameMob.BuffsController : null;
				if (buffsController != null)
				{
					return buffsController.HasBuff(this.expectedBuffsGeneratorAsset.GetBuffsID());
				}
			}
			return false;
		}

		// Token: 0x04000B18 RID: 2840
		public BuffsGeneratorBuilderAsset expectedBuffsGeneratorAsset;
	}
}
