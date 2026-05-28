using System;
using Game.Buffs;
using UnityEngine;
using Unliving.Mobs.AbilityTriggers;

namespace Unliving.Mobs
{
	// Token: 0x020001B3 RID: 435
	[CreateAssetMenu(fileName = "BuffsBasedStatusBuilder", menuName = "Game/Mobs/Buffs Based Status Builder")]
	public sealed class BuffsBasedStatusBuilderAsset : ScriptableObject
	{
		// Token: 0x06000C52 RID: 3154 RVA: 0x000268E0 File Offset: 0x00024AE0
		public BuffsBasedStatus CreateBuffsBasedStatus(IBuffsController targetBuffsController)
		{
			BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffsGeneratorsBuilders;
			IBuffsGenerator[] buffsGenerators;
			generatorsBuilders.Instantiate(out buffsGenerators);
			return new BuffsBasedStatus(targetBuffsController, buffsGenerators, this.statusActivators);
		}

		// Token: 0x0400070D RID: 1805
		public BuffsGeneratorBuilderAsset.Reference[] buffsGeneratorsBuilders;

		// Token: 0x0400070E RID: 1806
		public MobAbilityTriggerBase[] statusActivators;
	}
}
