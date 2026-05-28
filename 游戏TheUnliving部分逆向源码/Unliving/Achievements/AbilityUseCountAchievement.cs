using System;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Achievements
{
	// Token: 0x02000131 RID: 305
	[Serializable]
	public class AbilityUseCountAchievement : BaseAchievement<AbilityID>
	{
		// Token: 0x17000131 RID: 305
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x000199AB File Offset: 0x00017BAB
		public override string AchievementName
		{
			get
			{
				return this.achievementName;
			}
		}

		// Token: 0x17000132 RID: 306
		// (get) Token: 0x060007B7 RID: 1975 RVA: 0x000199B3 File Offset: 0x00017BB3
		public override AbilityID AchievmentItem
		{
			get
			{
				return this.ability;
			}
		}

		// Token: 0x17000133 RID: 307
		// (get) Token: 0x060007B8 RID: 1976 RVA: 0x000199BB File Offset: 0x00017BBB
		public override object UnlockState
		{
			get
			{
				return this.abilityUseCount;
			}
		}

		// Token: 0x060007B9 RID: 1977 RVA: 0x000199C8 File Offset: 0x00017BC8
		public override bool ChangeState(AbilityID item, object data)
		{
			if (base.IsAchieved)
			{
				return false;
			}
			if (item != this.AchievmentItem)
			{
				return false;
			}
			if (!(data is int))
			{
				return false;
			}
			if ((int)data >= (int)this.UnlockState)
			{
				this.UnlockAchievment();
				return true;
			}
			return false;
		}

		// Token: 0x060007BA RID: 1978 RVA: 0x00019A05 File Offset: 0x00017C05
		public override bool CheckItem(object item)
		{
			return item is int && (AbilityID)item == this.ability;
		}

		// Token: 0x04000475 RID: 1141
		[SerializeField]
		private string achievementName;

		// Token: 0x04000476 RID: 1142
		[SerializeField]
		private AbilityID ability;

		// Token: 0x04000477 RID: 1143
		[SerializeField]
		private int abilityUseCount;
	}
}
