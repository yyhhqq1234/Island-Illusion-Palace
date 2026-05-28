using System;
using System.Collections.Generic;
using UnityEngine;
using Unliving.DataParsing;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003F1 RID: 1009
	public abstract class AbilitySpecialBehavioursGenerationOptionsAssetBase : ScriptableObject
	{
		// Token: 0x060021FB RID: 8699 RVA: 0x00069E38 File Offset: 0x00068038
		protected static bool TryGetBehaviourIDValue(string modifierID, out int value)
		{
			AbilitySpecialBehaviourID abilitySpecialBehaviourID;
			if (ParsingUtility.TryParseEnum<AbilitySpecialBehaviourID>(modifierID, out abilitySpecialBehaviourID))
			{
				value = (int)abilitySpecialBehaviourID;
				return true;
			}
			value = 0;
			return false;
		}

		// Token: 0x060021FC RID: 8700 RVA: 0x00069E58 File Offset: 0x00068058
		protected static bool TryGetBehaviourActivatorIDValue(string activatorID, out int value)
		{
			AbilitySpecialBehaviourActivatorID abilitySpecialBehaviourActivatorID;
			if (ParsingUtility.TryParseEnum<AbilitySpecialBehaviourActivatorID>(activatorID, out abilitySpecialBehaviourActivatorID))
			{
				value = (int)abilitySpecialBehaviourActivatorID;
				return true;
			}
			value = 0;
			return false;
		}

		// Token: 0x060021FD RID: 8701
		public abstract ICollection<AbilitySpecialBehaviourGenerationOption> GetAbilityBehavioursDescriptions();
	}
}
