using System;

namespace Unliving.Pickables
{
	// Token: 0x02000191 RID: 401
	public interface IPickableObjectSpawner
	{
		// Token: 0x06000B5F RID: 2911
		void InvokeAfterPickableObjectSpawned(Action<IPickableObject> onPickableObjectSpawned);
	}
}
