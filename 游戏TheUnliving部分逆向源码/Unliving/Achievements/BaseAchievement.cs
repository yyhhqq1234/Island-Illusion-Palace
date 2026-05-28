using System;

namespace Unliving.Achievements
{
	// Token: 0x02000132 RID: 306
	public abstract class BaseAchievement<T> : IAchievement
	{
		// Token: 0x17000134 RID: 308
		// (get) Token: 0x060007BC RID: 1980
		public abstract T AchievmentItem { get; }

		// Token: 0x17000135 RID: 309
		// (get) Token: 0x060007BD RID: 1981
		public abstract object UnlockState { get; }

		// Token: 0x17000136 RID: 310
		// (get) Token: 0x060007BE RID: 1982
		public abstract string AchievementName { get; }

		// Token: 0x17000137 RID: 311
		// (get) Token: 0x060007BF RID: 1983 RVA: 0x00019A27 File Offset: 0x00017C27
		// (set) Token: 0x060007C0 RID: 1984 RVA: 0x00019A2F File Offset: 0x00017C2F
		public bool IsAchieved { get; protected set; }

		// Token: 0x060007C1 RID: 1985 RVA: 0x00019A38 File Offset: 0x00017C38
		public virtual void UnlockAchievment()
		{
			if (this.IsAchieved)
			{
				return;
			}
			this.IsAchieved = true;
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00019A4A File Offset: 0x00017C4A
		bool IAchievement.ChangeState(object item, object data)
		{
			return this.ChangeState((T)((object)item), data);
		}

		// Token: 0x060007C3 RID: 1987
		public abstract bool ChangeState(T item, object data);

		// Token: 0x060007C4 RID: 1988
		public abstract bool CheckItem(object item);
	}
}
