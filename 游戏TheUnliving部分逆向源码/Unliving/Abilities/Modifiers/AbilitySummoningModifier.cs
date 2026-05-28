using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C3 RID: 963
	[Serializable]
	public sealed class AbilitySummoningModifier : AbilityModifierBase, IMobsSummoner
	{
		// Token: 0x170006A9 RID: 1705
		// (get) Token: 0x060020BB RID: 8379 RVA: 0x00067130 File Offset: 0x00065330
		public override bool IsActive
		{
			get
			{
				return this.mobsSummoner != null && this.mobsSummoner.IsActive;
			}
		}

		// Token: 0x170006AA RID: 1706
		// (get) Token: 0x060020BC RID: 8380 RVA: 0x00067147 File Offset: 0x00065347
		public UnityEngine.Object SummonedMobsOwner
		{
			get
			{
				return this.currentAbility.OwnerBehaviour;
			}
		}

		// Token: 0x170006AB RID: 1707
		// (get) Token: 0x060020BD RID: 8381 RVA: 0x00067154 File Offset: 0x00065354
		public IReadOnlyList<IGameMob> SummonedMobs
		{
			get
			{
				return this.summonedMobs;
			}
		}

		// Token: 0x14000121 RID: 289
		// (add) Token: 0x060020BE RID: 8382 RVA: 0x0006715C File Offset: 0x0006535C
		// (remove) Token: 0x060020BF RID: 8383 RVA: 0x00067194 File Offset: 0x00065394
		public event Action<object, IGameMob, Vector2> MobSummoned;

		// Token: 0x14000122 RID: 290
		// (add) Token: 0x060020C0 RID: 8384 RVA: 0x000671CC File Offset: 0x000653CC
		// (remove) Token: 0x060020C1 RID: 8385 RVA: 0x00067204 File Offset: 0x00065404
		public event Action<object, IMobsSummoner, BaseAbility.UsingArgs> SummoningCompleted;

		// Token: 0x060020C2 RID: 8386 RVA: 0x0006723C File Offset: 0x0006543C
		protected override void OnUse(AbilityModifierUsingArgs usingArgs)
		{
			AbilityModifiersOverrides modifiersOverrides = usingArgs.GetModifiersOverrides();
			MobStatModifier item;
			bool flag = usingArgs.TryGetStatModifier(MobStatID.GroupMobsActivationModifiersDamage, out item);
			if (flag)
			{
				AbilitySummoningModifier.SummonedMobStatModifiers[0].Item2 = item;
			}
			GameMobSummoningContext summoningContext = new GameMobSummoningContext
			{
				summoner = (IGameMob)usingArgs.targetAbility.Owner,
				summoningSource = usingArgs.targetAbility,
				abilitiesLevelOverride = ((modifiersOverrides != null) ? modifiersOverrides.levelOverride : 0),
				statsModifiers = (flag ? AbilitySummoningModifier.SummonedMobStatModifiers : null)
			};
			int modifiersUsingCount = usingArgs.modifiersUsingCount;
			IList<CollectableAbilityResource> collectedResources = usingArgs.GetCollectedResources();
			Vector2 vector = usingArgs.targetsInfo.targetPosition;
			bool flag2 = usingArgs.tryUseAtMultiplePositions && collectedResources != null;
			int num = flag2 ? (collectedResources.Count / modifiersUsingCount) : 0;
			float summonedMobsLifetime = this.mobsSummoner.summonedMobsLifetime;
			if (modifiersOverrides != null)
			{
				modifiersOverrides.SetSummonedMobsLifetime(ref this.mobsSummoner.summonedMobsLifetime);
			}
			for (int i = 0; i < modifiersUsingCount; i++)
			{
				if (flag2)
				{
					vector = collectedResources[i * num].GetPosition(true);
				}
				bool flag3;
				IGameMob gameMob = this.mobsSummoner.SummonMob(summoningContext, vector, out flag3, !flag2 && modifiersUsingCount > 1);
				if (gameMob != null)
				{
					this.summonedMobs.Add(gameMob);
					gameMob.Killed += this.OnSummonedMobKilled;
					Action<object, IGameMob, Vector2> mobSummoned = this.MobSummoned;
					if (mobSummoned != null)
					{
						mobSummoned(usingArgs.targetAbility, gameMob, vector);
					}
				}
			}
			Action<object, IMobsSummoner, BaseAbility.UsingArgs> summoningCompleted = this.SummoningCompleted;
			if (summoningCompleted != null)
			{
				summoningCompleted(usingArgs.targetAbility, this, usingArgs.targetsInfo);
			}
			this.mobsSummoner.summonedMobsLifetime = summonedMobsLifetime;
		}

		// Token: 0x060020C3 RID: 8387 RVA: 0x000673DE File Offset: 0x000655DE
		protected override void OnReset(AbilityModifierUsingArgs usingArgs)
		{
		}

		// Token: 0x060020C4 RID: 8388 RVA: 0x000673E0 File Offset: 0x000655E0
		public AbilitySummoningModifier(AbilitySummoningModifier modifierPrototype) : base(modifierPrototype)
		{
			GameMobsSummoner gameMobsSummoner = modifierPrototype.mobsSummoner;
			this.mobsSummoner = ((gameMobsSummoner != null) ? gameMobsSummoner.Clone() : null);
			this.summonedMobs = new List<IGameMob>(32);
		}

		// Token: 0x060020C5 RID: 8389 RVA: 0x0006740E File Offset: 0x0006560E
		public override AbilityModifierBase Clone()
		{
			return new AbilitySummoningModifier(this);
		}

		// Token: 0x060020C6 RID: 8390 RVA: 0x00067416 File Offset: 0x00065616
		public override void OnAddedToAbility(BaseAbility ability)
		{
			this.currentAbility = ability;
		}

		// Token: 0x060020C7 RID: 8391 RVA: 0x00067420 File Offset: 0x00065620
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			foreach (IGameMob gameMob in this.summonedMobs)
			{
				((BaseGameMob)gameMob).Killed -= this.OnSummonedMobKilled;
			}
			this.summonedMobs.Clear();
			this.summonedMobs.TrimExcess();
		}

		// Token: 0x060020C8 RID: 8392 RVA: 0x00067498 File Offset: 0x00065698
		private void OnSummonedMobKilled(IGameMob summonedMob)
		{
			this.summonedMobs.Remove(summonedMob);
			summonedMob.Killed -= this.OnSummonedMobKilled;
		}

		// Token: 0x04001486 RID: 5254
		private static readonly ValueTuple<MobStatID, MobStatModifier>[] SummonedMobStatModifiers = new ValueTuple<MobStatID, MobStatModifier>[]
		{
			new ValueTuple<MobStatID, MobStatModifier>(MobStatID.MobDamage, MobStatModifier.Neutral)
		};

		// Token: 0x04001489 RID: 5257
		public GameMobsSummoner mobsSummoner;

		// Token: 0x0400148A RID: 5258
		private readonly List<IGameMob> summonedMobs;

		// Token: 0x0400148B RID: 5259
		private BaseAbility currentAbility;
	}
}
