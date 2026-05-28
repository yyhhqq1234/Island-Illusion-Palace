using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003EA RID: 1002
	[CreateAssetMenu(fileName = "AbilitySpecialBehaviourGeneratorData", menuName = "Game/Abilities Generation/Ability Special Behaviour Generator Data Asset")]
	public sealed class AbilitySpecialBehaviourGeneratorDataAsset : ScriptableObject
	{
		// Token: 0x170006D9 RID: 1753
		// (get) Token: 0x060021DC RID: 8668 RVA: 0x00069B04 File Offset: 0x00067D04
		public IReadOnlyList<AbilitySpecialBehaviourGenerator<AbilityID>.AbilityData> Data
		{
			get
			{
				return this.data;
			}
		}

		// Token: 0x04001532 RID: 5426
		[SerializeField]
		private AbilitySpecialBehaviourGeneratorDataAsset.AbilityData[] data;

		// Token: 0x0200059A RID: 1434
		[Serializable]
		private sealed class AbilityData : AbilitySpecialBehaviourGenerator<AbilityID>.AbilityData
		{
		}
	}
}
