using System;

namespace Unliving.Misc
{
	// Token: 0x02000245 RID: 581
	public interface IMessageManager<T, V> where T : Enum where V : Enum
	{
		// Token: 0x060013A0 RID: 5024
		IMessage GetMessage(T objectID, V messageType);
	}
}
