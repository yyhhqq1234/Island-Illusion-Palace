using System;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Abilities
{
	// Token: 0x02000374 RID: 884
	[CreateAssetMenu(fileName = "AbilityPrepTimeRandomizer", menuName = "Abilities/Controllers/Prep Time Randomizer")]
	public sealed class AbilityPrepTimeRandomizer : AbilityExtensionAssetBase
	{
		// Token: 0x17000600 RID: 1536
		// (get) Token: 0x06001D04 RID: 7428 RVA: 0x0005BA94 File Offset: 0x00059C94
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x0005BA97 File Offset: 0x00059C97
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			if (this.minPrepTime > 0f || this.maxPrepTime > 0f)
			{
				ability.PrepTime = UnityEngine.Random.Range(this.minPrepTime, this.maxPrepTime);
			}
		}

		// Token: 0x0400106F RID: 4207
		public float minPrepTime;

		// Token: 0x04001070 RID: 4208
		public float maxPrepTime;
	}
}
