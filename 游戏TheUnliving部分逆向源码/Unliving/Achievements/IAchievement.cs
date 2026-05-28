using System;

namespace Unliving.Achievements
{
	// Token: 0x02000133 RID: 307
	public interface IAchievement
	{
		// Token: 0x17000138 RID: 312
		// (get) Token: 0x060007C6 RID: 1990
		string AchievementName { get; }

		// Token: 0x17000139 RID: 313
		// (get) Token: 0x060007C7 RID: 1991
		bool IsAchieved { get; }

		// Token: 0x060007C8 RID: 1992
		bool CheckItem(object item);

		// Token: 0x060007C9 RID: 1993
		void UnlockAchievment();

		// Token: 0x060007CA RID: 1994
		bool ChangeState(object item, object data);
	}
}
