using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022D RID: 557
	[CreateAssetMenu(fileName = "GroupBattleStateAbilityTrigger", menuName = "Abilities/Triggers/Group Battle State Trigger")]
	public sealed class GroupBattleStateAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x1700040E RID: 1038
		// (get) Token: 0x06001317 RID: 4887 RVA: 0x0003C540 File Offset: 0x0003A740
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700040F RID: 1039
		// (get) Token: 0x06001318 RID: 4888 RVA: 0x0003C543 File Offset: 0x0003A743
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000410 RID: 1040
		// (get) Token: 0x06001319 RID: 4889 RVA: 0x0003C54A File Offset: 0x0003A74A
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x0600131A RID: 4890 RVA: 0x0003C550 File Offset: 0x0003A750
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob baseGameMob = ability.OwnerBehaviour as BaseGameMob;
			GameMobGroupController gameMobGroupController = ((baseGameMob != null) ? baseGameMob.Group : null) as GameMobGroupController;
			if (gameMobGroupController != null)
			{
				switch (this.type)
				{
				case GroupBattleStateAbilityTrigger.Type.GroupHasAttackTargets:
					return gameMobGroupController.HasAttackTargets;
				case GroupBattleStateAbilityTrigger.Type.GroupIsAttacking:
					return gameMobGroupController.IsAttacking;
				case GroupBattleStateAbilityTrigger.Type.GroupIsUnderAttack:
					return gameMobGroupController.IsUnderAttack;
				case GroupBattleStateAbilityTrigger.Type.GroupInBattle:
					return gameMobGroupController.InBattle;
				}
			}
			return false;
		}

		// Token: 0x04000B26 RID: 2854
		[Tooltip("InBattle = IsAttacking or IsUnderAttack.")]
		public GroupBattleStateAbilityTrigger.Type type;

		// Token: 0x020004C8 RID: 1224
		public enum Type
		{
			// Token: 0x040019BA RID: 6586
			GroupHasAttackTargets,
			// Token: 0x040019BB RID: 6587
			GroupIsAttacking,
			// Token: 0x040019BC RID: 6588
			GroupIsUnderAttack,
			// Token: 0x040019BD RID: 6589
			GroupInBattle
		}
	}
}
