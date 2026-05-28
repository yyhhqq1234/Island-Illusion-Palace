using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.LeveledItems.Test
{
	// Token: 0x0200025B RID: 603
	[CreateAssetMenu(fileName = "AbilityPropertiesAlteringTests", menuName = "Test/AbilityPropertiesAlteringTests")]
	public sealed class AbilityPropertiesAlteringTests : ScriptableObject
	{
		// Token: 0x0600140D RID: 5133 RVA: 0x0003F270 File Offset: 0x0003D470
		private void Test(BaseAbility ability, AbilityPropertyID propertyID, float value, int buffsGeneratorIndex = -1)
		{
			IAbilityPropertyAccessor abilityPropertyAccessor = AbilityPropertiesAccessors.Get(ability, new AbilityPropertyDescription(propertyID, buffsGeneratorIndex));
			Debug.Log(string.Format("{0}_{1}_{2}: {3}", new object[]
			{
				ability,
				buffsGeneratorIndex,
				abilityPropertyAccessor.PropertyName,
				abilityPropertyAccessor.GetValue(ability)
			}));
			abilityPropertyAccessor.SetValue(ability, value);
			Debug.Log(string.Format("modified {0}_{1}_{2}: {3}", new object[]
			{
				ability,
				buffsGeneratorIndex,
				propertyID,
				abilityPropertyAccessor.GetValue(ability)
			}));
		}

		// Token: 0x0600140E RID: 5134 RVA: 0x0003F30C File Offset: 0x0003D50C
		[ContextMenu("Test")]
		private void Test()
		{
			BaseAbility baseAbility = UnityEngine.Object.Instantiate<BaseAbility>(this.targetAbility);
			baseAbility.Initialize(null);
			this.Test(baseAbility, AbilityPropertyID.PrepTime, 10f, -1);
			this.Test(baseAbility, AbilityPropertyID.MaxUsingCount, 3f, -1);
			this.Test(baseAbility, AbilityPropertyID.DamageEffectAmount, 25f, -1);
			this.Test(baseAbility, AbilityPropertyID.BuffsDuration, 10f, 0);
			UnityEngine.Object.DestroyImmediate(baseAbility);
		}

		// Token: 0x04000BB4 RID: 2996
		public BaseAbility targetAbility;
	}
}
