using System;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000371 RID: 881
	[CreateAssetMenu(fileName = "AbilityOwnerGroupBuffsController", menuName = "Abilities/Controllers/Owner Group Buffs Controller")]
	public sealed class AbilityOwnerGroupBuffsController : AbilityOwnerGroupBuffsControllerBase
	{
		// Token: 0x06001CF5 RID: 7413 RVA: 0x0005B7FC File Offset: 0x000599FC
		protected override void SendBuffs(IAbility ability, BaseGameMob abilityOwner, IBuffsController groupMobBuffsController)
		{
			if (this.buffsGenerators == null)
			{
				BuffsGeneratorBuilderAsset.ReferenceBase[] generatorsBuilders = this.buffsGeneratorsAssets;
				generatorsBuilders.Instantiate(out this.buffsGenerators);
			}
			for (int i = 0; i < this.buffsGenerators.Length; i++)
			{
				groupMobBuffsController.AddBuff(this.buffsGenerators[i].GenerateBuff(abilityOwner, false));
			}
		}

		// Token: 0x04001068 RID: 4200
		public BuffsGeneratorBuilderAsset.Reference[] buffsGeneratorsAssets;

		// Token: 0x04001069 RID: 4201
		private IBuffsGenerator[] buffsGenerators;
	}
}
