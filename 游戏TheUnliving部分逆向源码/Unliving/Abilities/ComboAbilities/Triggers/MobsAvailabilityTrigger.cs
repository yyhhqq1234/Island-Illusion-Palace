using System;
using Common.Editor;
using Game.Abilities.TargetsCollection;
using Game.Factories;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities.ComboAbilities.Triggers
{
	// Token: 0x020003D7 RID: 983
	[Serializable]
	public sealed class MobsAvailabilityTrigger : ComboAbilityTriggerBase
	{
		// Token: 0x0600217B RID: 8571 RVA: 0x00068B7C File Offset: 0x00066D7C
		protected override bool GetState(ComboAbilityUsingContext context)
		{
			float num = (this.customTargetsCapturingRange > 0f) ? this.customTargetsCapturingRange : context.GetChildAbilityRange();
			if (num <= 0f || this.mobsLayers.value == 0)
			{
				return false;
			}
			Collider2D[] tempTargetsBuffer = AbilityTargetsCollector<Collider2D>.TempTargetsBuffer;
			int num2 = Physics2D.OverlapCircleNonAlloc(context.GetAbilityOwnerPosition(), num, tempTargetsBuffer, this.mobsLayers);
			bool flag = false;
			if (num2 != 0)
			{
				MobsAvailabilityTrigger.mobsFilteringParams.mobFaction = this.allowedFaction;
				MobsAvailabilityTrigger.mobsFilteringParams.mobID = this.specificMobID;
				MobsAvailabilityTrigger.mobsFilteringParams.mobTag = this.allowedTag;
				int num3 = 0;
				BaseGameMob mob;
				while (num3 < num2 && (!tempTargetsBuffer[num3].TryGetComponent<BaseGameMob>(out mob) || !(flag = MobsAvailabilityTrigger.mobsFilteringParams.IsMatch(mob))))
				{
					num3++;
				}
			}
			if (!this.triggerIfThereAreNoMobs)
			{
				return flag;
			}
			return !flag;
		}

		// Token: 0x040014E2 RID: 5346
		private static GameMobDescription mobsFilteringParams;

		// Token: 0x040014E3 RID: 5347
		public bool triggerIfThereAreNoMobs;

		// Token: 0x040014E4 RID: 5348
		public float customTargetsCapturingRange;

		// Token: 0x040014E5 RID: 5349
		public LayerMask mobsLayers;

		// Token: 0x040014E6 RID: 5350
		public GameMobFactions allowedFaction;

		// Token: 0x040014E7 RID: 5351
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID specificMobID;

		// Token: 0x040014E8 RID: 5352
		[Tag]
		public string allowedTag;
	}
}
