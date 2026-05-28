using System;
using Game.Abilities;

namespace Unliving.LeveledItems
{
	// Token: 0x0200024E RID: 590
	public interface IAbilityPropertyAccessor
	{
		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x060013DA RID: 5082
		string PropertyName { get; }

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x060013DB RID: 5083
		bool IsReadOnly { get; }

		// Token: 0x060013DC RID: 5084
		float GetValue(BaseAbility ability);

		// Token: 0x060013DD RID: 5085
		void SetValue(BaseAbility ability, float value);
	}
}
