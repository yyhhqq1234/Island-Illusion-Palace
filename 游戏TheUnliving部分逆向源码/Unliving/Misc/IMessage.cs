using System;

namespace Unliving.Misc
{
	// Token: 0x02000244 RID: 580
	public interface IMessage
	{
		// Token: 0x17000435 RID: 1077
		// (get) Token: 0x0600139E RID: 5022
		int Priority { get; }

		// Token: 0x0600139F RID: 5023
		string GetMessageText();
	}
}
