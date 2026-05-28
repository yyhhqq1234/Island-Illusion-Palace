using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x0200022E RID: 558
	[CreateAssetMenu(fileName = "GroupMateDeathAbilityTrigger", menuName = "Abilities/Triggers/Group Mate Death Trigger")]
	public sealed class GroupMateDeathAbilityTrigger : MobAbilityTriggerBase
	{
		// Token: 0x17000411 RID: 1041
		// (get) Token: 0x0600131C RID: 4892 RVA: 0x0003C5C1 File Offset: 0x0003A7C1
		public override bool RequiresTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000412 RID: 1042
		// (get) Token: 0x0600131D RID: 4893 RVA: 0x0003C5C4 File Offset: 0x0003A7C4
		public override float ActivationRange
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x17000413 RID: 1043
		// (get) Token: 0x0600131E RID: 4894 RVA: 0x0003C5CB File Offset: 0x0003A7CB
		protected override bool InstantiateForEveryAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600131F RID: 4895 RVA: 0x0003C5CE File Offset: 0x0003A7CE
		protected override void ResetTrigger(BaseAbility ability)
		{
			base.ResetTrigger(ability);
			this.isConditionReached = false;
		}

		// Token: 0x06001320 RID: 4896 RVA: 0x0003C5E0 File Offset: 0x0003A7E0
		protected override void OnAbilityOwnerChanged(BaseAbility ability, object lastOwner, object newOwner)
		{
			base.OnAbilityOwnerChanged(ability, lastOwner, newOwner);
			BaseGameMob baseGameMob = lastOwner as BaseGameMob;
			if (baseGameMob != null)
			{
				baseGameMob.GroupModified -= this.OnMobGroupModified;
			}
			BaseGameMob baseGameMob2 = newOwner as BaseGameMob;
			if (baseGameMob2 != null)
			{
				baseGameMob2.GroupModified += this.OnMobGroupModified;
			}
		}

		// Token: 0x06001321 RID: 4897 RVA: 0x0003C630 File Offset: 0x0003A830
		private void OnMobGroupModified(BaseGameMob mob, BaseGameMob.GroupModificationArgs args)
		{
			if (!this.isConditionReached && args.mobWasRemovedFromGroup && args.affectedGroupMob.IsKilled && (this.isConditionReached = (this.triggerEvent == GroupMateDeathAbilityTrigger.Event.SingleMobKilled || (this.triggerEvent == GroupMateDeathAbilityTrigger.Event.AllGroupKilled && !args.group.HasMobs))))
			{
				base.TryTriggerAutoUseAbility(this.currentAbility, null, false);
			}
		}

		// Token: 0x06001322 RID: 4898 RVA: 0x0003C699 File Offset: 0x0003A899
		public override bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return this.isConditionReached;
		}

		// Token: 0x04000B27 RID: 2855
		public GroupMateDeathAbilityTrigger.Event triggerEvent;

		// Token: 0x04000B28 RID: 2856
		private bool isConditionReached;

		// Token: 0x020004C9 RID: 1225
		public enum Event
		{
			// Token: 0x040019BF RID: 6591
			SingleMobKilled,
			// Token: 0x040019C0 RID: 6592
			AllGroupKilled
		}
	}
}
