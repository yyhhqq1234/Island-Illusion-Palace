using System;
using Game.Abilities;
using Game.Factories;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000237 RID: 567
	[CreateAssetMenu(fileName = "QueuedUsingAbilityTrigger", menuName = "Abilities/Triggers/Queued Using Trigger")]
	public sealed class QueuedUsingAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x1700042B RID: 1067
		// (get) Token: 0x06001363 RID: 4963 RVA: 0x0003CC21 File Offset: 0x0003AE21
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700042C RID: 1068
		// (get) Token: 0x06001364 RID: 4964 RVA: 0x0003CC24 File Offset: 0x0003AE24
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x1700042D RID: 1069
		// (get) Token: 0x06001365 RID: 4965 RVA: 0x0003CC2B File Offset: 0x0003AE2B
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001366 RID: 4966 RVA: 0x0003CC2E File Offset: 0x0003AE2E
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return true;
		}

		// Token: 0x04000B3F RID: 2879
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID expectedAbilityID;

		// Token: 0x04000B40 RID: 2880
		[Tooltip("При активной опции триггер будет активирован когда не сам моб использует заданную абилити, а лидер группы, в которой этот моб находится.")]
		public bool isGroupLeaderAbility;
	}
}
