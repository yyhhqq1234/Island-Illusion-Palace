using System;
using Game.Abilities;

namespace Unliving.LeveledItems
{
	// Token: 0x0200024F RID: 591
	public sealed class AbilityPropertyAccessor<TAbility> : IAbilityPropertyAccessor where TAbility : BaseAbility
	{
		// Token: 0x1700043A RID: 1082
		// (get) Token: 0x060013DE RID: 5086 RVA: 0x0003ECD2 File Offset: 0x0003CED2
		public string PropertyName { get; }

		// Token: 0x1700043B RID: 1083
		// (get) Token: 0x060013DF RID: 5087 RVA: 0x0003ECDA File Offset: 0x0003CEDA
		public bool IsReadOnly
		{
			get
			{
				return this.setValueFunc == null;
			}
		}

		// Token: 0x060013E0 RID: 5088 RVA: 0x0003ECE8 File Offset: 0x0003CEE8
		private bool IsAccessible(BaseAbility ability, out TAbility targetAbility)
		{
			return (targetAbility = (ability as TAbility)) != null;
		}

		// Token: 0x060013E1 RID: 5089 RVA: 0x0003ED19 File Offset: 0x0003CF19
		public AbilityPropertyAccessor(string propertyName, Func<TAbility, float> getValueFunc, Action<TAbility, float> setValueFunc)
		{
			this.PropertyName = propertyName;
			this.getValueFunc = getValueFunc;
			this.setValueFunc = setValueFunc;
		}

		// Token: 0x060013E2 RID: 5090 RVA: 0x0003ED36 File Offset: 0x0003CF36
		public AbilityPropertyAccessor(string propertyName, Delegate getValueFunc, Delegate setValueFunc)
		{
			this.PropertyName = propertyName;
			this.getValueFunc = (Func<TAbility, float>)getValueFunc;
			this.setValueFunc = (Action<TAbility, float>)setValueFunc;
		}

		// Token: 0x060013E3 RID: 5091 RVA: 0x0003ED5D File Offset: 0x0003CF5D
		public float GetValue(TAbility ability)
		{
			return this.getValueFunc(ability);
		}

		// Token: 0x060013E4 RID: 5092 RVA: 0x0003ED6B File Offset: 0x0003CF6B
		public void SetValue(TAbility ability, float value)
		{
			this.setValueFunc(ability, value);
		}

		// Token: 0x060013E5 RID: 5093 RVA: 0x0003ED7C File Offset: 0x0003CF7C
		float IAbilityPropertyAccessor.GetValue(BaseAbility ability)
		{
			TAbility ability2;
			if (this.IsAccessible(ability, out ability2))
			{
				return this.GetValue(ability2);
			}
			return 0f;
		}

		// Token: 0x060013E6 RID: 5094 RVA: 0x0003EDA4 File Offset: 0x0003CFA4
		void IAbilityPropertyAccessor.SetValue(BaseAbility ability, float value)
		{
			TAbility ability2;
			if (this.IsAccessible(ability, out ability2))
			{
				this.SetValue(ability2, value);
			}
		}

		// Token: 0x04000B8F RID: 2959
		private readonly Func<TAbility, float> getValueFunc;

		// Token: 0x04000B90 RID: 2960
		private readonly Action<TAbility, float> setValueFunc;
	}
}
