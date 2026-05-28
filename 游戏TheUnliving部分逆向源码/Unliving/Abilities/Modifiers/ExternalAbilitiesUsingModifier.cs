using System;
using System.Collections.Generic;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003CF RID: 975
	[Serializable]
	public sealed class ExternalAbilitiesUsingModifier : AbilityModifierBase
	{
		// Token: 0x170006B0 RID: 1712
		// (get) Token: 0x06002116 RID: 8470 RVA: 0x00067DA0 File Offset: 0x00065FA0
		public override bool IsActive
		{
			get
			{
				return this.fakeMobPrefab != null || (this.abilities != null && this.abilities.Length != 0);
			}
		}

		// Token: 0x06002117 RID: 8471 RVA: 0x00067DC6 File Offset: 0x00065FC6
		public ExternalAbilitiesUsingModifier(ExternalAbilitiesUsingModifier modifierPrototype) : base(modifierPrototype)
		{
			this.fakeMobPrefab = modifierPrototype.fakeMobPrefab;
			this.abilities = modifierPrototype.abilities;
		}

		// Token: 0x06002118 RID: 8472 RVA: 0x00067DF2 File Offset: 0x00065FF2
		public override AbilityModifierBase Clone()
		{
			return new ExternalAbilitiesUsingModifier(this);
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x00067DFC File Offset: 0x00065FFC
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			AbilityModifiersOverrides modifiersOverrides = usingArgs.GetModifiersOverrides();
			MobStatModifier item;
			bool flag = usingArgs.TryGetStatModifier(MobStatID.GroupMobsActivationModifiersDamage, out item);
			if (flag)
			{
				ExternalAbilitiesUsingModifier.StatsModifiers[0] = new ValueTuple<MobStatID, MobStatModifier>(MobStatID.MobDamage, item);
			}
			ExternalAbilitiesUsingModifier.InstantiationArgs.mobPrefab = this.fakeMobPrefab;
			ExternalAbilitiesUsingModifier.InstantiationArgs.owner = usingArgs.targetAbility;
			FakeMobInstantiationArgs instantiationArgs = ExternalAbilitiesUsingModifier.InstantiationArgs;
			IGameMob gameMob = usingArgs.targetAbility.Owner as IGameMob;
			instantiationArgs.group = ((gameMob != null) ? gameMob.Group : null);
			ExternalAbilitiesUsingModifier.InstantiationArgs.abilitiesOverride = this.abilities;
			ExternalAbilitiesUsingModifier.InstantiationArgs.abilitiesLevelOverride = ((modifiersOverrides != null) ? modifiersOverrides.levelOverride : 0);
			ExternalAbilitiesUsingModifier.InstantiationArgs.statsModifiers = (flag ? ExternalAbilitiesUsingModifier.StatsModifiers : null);
			int modifiersUsingCount = usingArgs.modifiersUsingCount;
			IList<CollectableAbilityResource> collectedResources = usingArgs.GetCollectedResources();
			Vector2 v = usingArgs.targetsInfo.targetPosition;
			bool flag2 = usingArgs.tryUseAtMultiplePositions && collectedResources != null;
			int num = flag2 ? (collectedResources.Count / modifiersUsingCount) : 0;
			for (int i = 0; i < modifiersUsingCount; i++)
			{
				if (flag2)
				{
					v = collectedResources[i * num].GetPosition(true);
				}
				ExternalAbilitiesUsingModifier.InstantiationArgs.position = v;
				FakeMobBehaviour fakeMobBehaviour = this.mobInstantiator.Instantiate(ExternalAbilitiesUsingModifier.InstantiationArgs);
				if (fakeMobBehaviour != null)
				{
					fakeMobBehaviour.ActivateAbilitiesAutoUsing(usingArgs.targetsInfo);
				}
			}
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x00067F5D File Offset: 0x0006615D
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x040014AD RID: 5293
		private static readonly FakeMobInstantiationArgs InstantiationArgs = new FakeMobInstantiationArgs();

		// Token: 0x040014AE RID: 5294
		private static readonly ValueTuple<MobStatID, MobStatModifier>[] StatsModifiers = new ValueTuple<MobStatID, MobStatModifier>[1];

		// Token: 0x040014AF RID: 5295
		public GameObject fakeMobPrefab;

		// Token: 0x040014B0 RID: 5296
		public FakeMobBehaviour.AbilityDescription[] abilities;

		// Token: 0x040014B1 RID: 5297
		private readonly FakeMobInstantiator mobInstantiator = new FakeMobInstantiator();
	}
}
