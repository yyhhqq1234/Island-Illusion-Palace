using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Game.Factories;
using Game.Stats;
using Game.Utility;
using UnityEngine;
using UnityEngine.AI;
using Unliving.LeveledItems;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x020003B6 RID: 950
	[CreateAssetMenu(fileName = "SummoningAbility", menuName = "Abilities/Summoning Ability")]
	public sealed class SummoningAbility : BaseAbility, IDamageSender, IMobStatsListProvider, IStatsListProvider<MobStatModifier>, ILeveledItem, IItemLevelProvider, IMobsSummoner
	{
		// Token: 0x17000668 RID: 1640
		// (get) Token: 0x06001FB5 RID: 8117 RVA: 0x00063BEA File Offset: 0x00061DEA
		// (set) Token: 0x06001FB6 RID: 8118 RVA: 0x00063BF2 File Offset: 0x00061DF2
		public override int ID { get; set; }

		// Token: 0x17000669 RID: 1641
		// (get) Token: 0x06001FB7 RID: 8119 RVA: 0x00063BFB File Offset: 0x00061DFB
		// (set) Token: 0x06001FB8 RID: 8120 RVA: 0x00063C03 File Offset: 0x00061E03
		public override int Type
		{
			get
			{
				return (int)this.abilityType;
			}
			set
			{
				this.abilityType = (AbilityTypes)value;
			}
		}

		// Token: 0x1700066A RID: 1642
		// (get) Token: 0x06001FB9 RID: 8121 RVA: 0x00063C0C File Offset: 0x00061E0C
		// (set) Token: 0x06001FBA RID: 8122 RVA: 0x00063C14 File Offset: 0x00061E14
		public int SummoningCount
		{
			get
			{
				return this.summoningCount;
			}
			set
			{
				this.summoningCount = value;
			}
		}

		// Token: 0x1700066B RID: 1643
		// (get) Token: 0x06001FBB RID: 8123 RVA: 0x00063C1D File Offset: 0x00061E1D
		// (set) Token: 0x06001FBC RID: 8124 RVA: 0x00063C25 File Offset: 0x00061E25
		public int MaxSummoningCount
		{
			get
			{
				return this.maxSummoningCount;
			}
			set
			{
				this.maxSummoningCount = value;
			}
		}

		// Token: 0x1700066C RID: 1644
		// (get) Token: 0x06001FBD RID: 8125 RVA: 0x00063C2E File Offset: 0x00061E2E
		// (set) Token: 0x06001FBE RID: 8126 RVA: 0x00063C36 File Offset: 0x00061E36
		public float SummonedMobsMaxLifetime
		{
			get
			{
				return this.summonedMobsMaxLifetime;
			}
			set
			{
				this.summonedMobsMaxLifetime = value;
			}
		}

		// Token: 0x1700066D RID: 1645
		// (get) Token: 0x06001FBF RID: 8127 RVA: 0x00063C3F File Offset: 0x00061E3F
		// (set) Token: 0x06001FC0 RID: 8128 RVA: 0x00063C47 File Offset: 0x00061E47
		DamageGenerator IDamageSender.DamageGenerator
		{
			get
			{
				return this.summonableMobDamage;
			}
			set
			{
			}
		}

		// Token: 0x1700066E RID: 1646
		// (get) Token: 0x06001FC1 RID: 8129 RVA: 0x00063C49 File Offset: 0x00061E49
		public override bool IsTargetedAbility
		{
			get
			{
				return this.summoningTarget > SummoningAbility.SummoningTarget.AbilityOwner;
			}
		}

		// Token: 0x1700066F RID: 1647
		// (get) Token: 0x06001FC2 RID: 8130 RVA: 0x00063C54 File Offset: 0x00061E54
		public override bool IsObjectTargetRequired
		{
			get
			{
				return this.summoningTarget == SummoningAbility.SummoningTarget.AbilityTarget;
			}
		}

		// Token: 0x17000670 RID: 1648
		// (get) Token: 0x06001FC3 RID: 8131 RVA: 0x00063C5F File Offset: 0x00061E5F
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000671 RID: 1649
		// (get) Token: 0x06001FC4 RID: 8132 RVA: 0x00063C62 File Offset: 0x00061E62
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000672 RID: 1650
		// (get) Token: 0x06001FC5 RID: 8133 RVA: 0x00063C65 File Offset: 0x00061E65
		public override bool CanBeUsed
		{
			get
			{
				return this.maxSummoningCount <= 0 || this.summonedMobs.Count < this.maxSummoningCount;
			}
		}

		// Token: 0x17000673 RID: 1651
		// (get) Token: 0x06001FC6 RID: 8134 RVA: 0x00063C85 File Offset: 0x00061E85
		// (set) Token: 0x06001FC7 RID: 8135 RVA: 0x00063C8D File Offset: 0x00061E8D
		public int ItemLevel { get; set; }

		// Token: 0x17000674 RID: 1652
		// (get) Token: 0x06001FC8 RID: 8136 RVA: 0x00063C96 File Offset: 0x00061E96
		public IReadOnlyList<IGameMob> SummonedMobs
		{
			get
			{
				return this.summonedMobs;
			}
		}

		// Token: 0x17000675 RID: 1653
		// (get) Token: 0x06001FC9 RID: 8137 RVA: 0x00063C9E File Offset: 0x00061E9E
		UnityEngine.Object IMobsSummoner.SummonedMobsOwner
		{
			get
			{
				return base.OwnerBehaviour;
			}
		}

		// Token: 0x17000676 RID: 1654
		// (get) Token: 0x06001FCA RID: 8138 RVA: 0x00063CA6 File Offset: 0x00061EA6
		IReadOnlyList<IModifiableStat<MobStatModifier>> IStatsListProvider<MobStatModifier>.Stats
		{
			get
			{
				return this.statsListProvider.Stats;
			}
		}

		// Token: 0x1400011C RID: 284
		// (add) Token: 0x06001FCB RID: 8139 RVA: 0x00063CB4 File Offset: 0x00061EB4
		// (remove) Token: 0x06001FCC RID: 8140 RVA: 0x00063CEC File Offset: 0x00061EEC
		public event Action<object, IGameMob, Vector2> MobSummoned;

		// Token: 0x1400011D RID: 285
		// (add) Token: 0x06001FCD RID: 8141 RVA: 0x00063D24 File Offset: 0x00061F24
		// (remove) Token: 0x06001FCE RID: 8142 RVA: 0x00063D5C File Offset: 0x00061F5C
		public event Action<object, IMobsSummoner, BaseAbility.UsingArgs> SummoningCompleted;

		// Token: 0x06001FCF RID: 8143 RVA: 0x00063D94 File Offset: 0x00061F94
		private bool IsBusyPosition(Vector2 position, float radius)
		{
			for (int i = 0; i < this.summonedMobs.Count; i++)
			{
				IGameMob gameMob = this.summonedMobs[i];
				float num = (gameMob.Radius + radius) * 0.75f;
				if ((gameMob.Position - position).SqrMagnitude() < num * num)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001FD0 RID: 8144 RVA: 0x00063DF0 File Offset: 0x00061FF0
		private bool FindSummoningPosition(Vector2 summoningTargetPosition, float summoningTargetRadius, Vector2 spawningDirection, out Vector2 position)
		{
			if (this.summonableMobRadius > 0f && summoningTargetRadius > 0f)
			{
				if (spawningDirection == default(Vector2))
				{
					spawningDirection = new Vector2
					{
						x = 1f
					};
				}
				float num = summoningTargetRadius + this.summonableMobRadius + Mathf.Max(this.distanceFromSummoningTarget, 0f);
				float num2 = num * 6.2831855f;
				int num3 = (int)(num2 / (this.summonableMobRadius * 2f));
				float num4 = (float)this.summoningCount / (float)num3 * 0.5f - this.summonableMobRadius * 2f / num2;
				for (int i = 0; i <= num3; i++)
				{
					float f = ((float)(this.randomizeSummoningPoints ? UnityEngine.Random.Range(0, num3 + 1) : i) / (float)num3 - num4) * 6.2831855f;
					float num5 = Mathf.Cos(f);
					float num6 = Mathf.Sin(f);
					Vector2 vector = new Vector2
					{
						x = summoningTargetPosition.x + (spawningDirection.x * num5 - spawningDirection.y * num6) * num,
						y = summoningTargetPosition.y + (spawningDirection.x * num6 + spawningDirection.y * num5) * num
					};
					NavMeshHit navMeshHit;
					if (!this.IsBusyPosition(vector, this.summonableMobRadius) && NavMesh.SamplePosition(vector, out navMeshHit, this.summonableMobRadius, -1))
					{
						position = navMeshHit.position;
						return true;
					}
				}
				position = default(Vector2);
				return false;
			}
			position = summoningTargetPosition;
			return true;
		}

		// Token: 0x06001FD1 RID: 8145 RVA: 0x00063F7C File Offset: 0x0006217C
		private IGameMob SummonMob(Vector2 position, BaseAbility.UsingArgs usingArgs)
		{
			if (SummoningAbility.mobsFactory == null)
			{
				return null;
			}
			SummoningAbility.MobsFactoryArgs.mobID = this.summonableMobID;
			SummoningAbility.MobsFactoryArgs.arbitraryMobPrefab = this.summonableMobPrefab;
			SummoningAbility.MobsFactoryArgs.mobFaction = ((this.summonedMobsFaction == GameMobFactions.None) ? (base.OwnerBehaviour as BaseGameMob).Faction : this.summonedMobsFaction);
			SummoningAbility.MobsFactoryArgs.spawnPosition = position;
			SummoningAbility.MobSpawnerInfo.spawner = this;
			SummoningAbility.MobSpawnerInfo.isAggressiveMob = this.summonAggressiveMobs;
			SummoningAbility.MobSpawnerInfo.isAggressionReactiveMob = this.summonMobsWithReactiveAggression;
			SummoningAbility.MobSpawnerInfo.canShareAggression = this.summonedMobsCanShareAggression;
			IGameMob gameMob = SummoningAbility.mobsFactory.SummonMob(new GameMobSummoningContext(this), SummoningAbility.MobsFactoryArgs, this.summonToIndividualGroups, this.summonedMobsMaxLifetime, false);
			if (gameMob != null)
			{
				gameMob.Killed += this.OnSummonedMobKilled;
				this.summonedMobs.Add(gameMob);
				base.NotifyAbilityUsed((Component)gameMob, usingArgs);
				Action<object, IGameMob, Vector2> mobSummoned = this.MobSummoned;
				if (mobSummoned != null)
				{
					mobSummoned(this, gameMob, position);
				}
				if (this.summonedMobsUnhideDelay > 0f)
				{
					GameObject gameObject = gameMob.GameObject;
					if (gameObject != null)
					{
						gameObject.SetActive(false);
						SummoningAbility.<SummonMob>g__StartMobUnhideTask|75_0(gameObject, this.summonedMobsUnhideDelay);
					}
				}
			}
			return gameMob;
		}

		// Token: 0x06001FD2 RID: 8146 RVA: 0x000640C0 File Offset: 0x000622C0
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			Vector2 summoningTargetPosition;
			float summoningTargetRadius;
			if (this.summoningTarget == SummoningAbility.SummoningTarget.AbilityTargetPoint)
			{
				summoningTargetPosition = usingArgs.targetPosition;
				summoningTargetRadius = 0.5f;
			}
			else
			{
				Component component = (this.summoningTarget == SummoningAbility.SummoningTarget.AbilityOwner) ? base.OwnerBehaviour : usingArgs.targetObject;
				if (component.IsNull())
				{
					return;
				}
				summoningTargetRadius = GameplayUtility.GetObjectRadius(component);
				summoningTargetPosition = component.transform.position;
			}
			Vector2 spawningDirection = default(Vector2);
			if (this.summonInCursorDirection)
			{
				spawningDirection = usingArgs.targetPosition - base.OwnerPosition;
				spawningDirection.Normalize();
			}
			int num = 0;
			while (num < this.summoningCount && this.CanBeUsed)
			{
				Vector2 position;
				if (this.FindSummoningPosition(summoningTargetPosition, summoningTargetRadius, spawningDirection, out position))
				{
					this.SummonMob(position, usingArgs);
				}
				num++;
			}
			Action<object, IMobsSummoner, BaseAbility.UsingArgs> summoningCompleted = this.SummoningCompleted;
			if (summoningCompleted == null)
			{
				return;
			}
			summoningCompleted(this, this, usingArgs);
		}

		// Token: 0x06001FD3 RID: 8147 RVA: 0x00064198 File Offset: 0x00062398
		protected override void OnOwnerChanged(object lastOwner, object newOwner)
		{
			if (SummoningAbility.mobsFactory == null)
			{
				IGameBehaviour gameBehaviour = newOwner as IGameBehaviour;
				if (gameBehaviour != null)
				{
					SummoningAbility.mobsFactory = gameBehaviour.CurrentGame.Services.Get<GameMobsFactory>();
				}
			}
			if (this.summonableMobFactoryData == null)
			{
				GameMobsFactory gameMobsFactory = SummoningAbility.mobsFactory;
				this.summonableMobFactoryData = ((gameMobsFactory != null) ? gameMobsFactory.GetObjectPrototype((int)this.summonableMobID) : null);
				if (this.summonableMobFactoryData != null)
				{
					GameObject objectPrefab = this.summonableMobFactoryData.objectPrefab;
					NavMeshAgent navMeshAgent;
					if (objectPrefab != null && objectPrefab.TryGetComponent<NavMeshAgent>(out navMeshAgent))
					{
						Vector2 vector = navMeshAgent.transform.localScale;
						this.summonableMobRadius = navMeshAgent.radius * Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
					}
					ValueTuple<AbilityID, float> mainAttackAbilityInfo = this.summonableMobFactoryData.GetMainAttackAbilityInfo();
					AbilityID item = mainAttackAbilityInfo.Item1;
					float item2 = mainAttackAbilityInfo.Item2;
					if (item != AbilityID.None)
					{
						this.summonableMobDamage = new DamageGenerator
						{
							amount = item2
						};
					}
				}
			}
		}

		// Token: 0x06001FD4 RID: 8148 RVA: 0x00064288 File Offset: 0x00062488
		protected override void OnOwnerDestroyed(object abilityOwner)
		{
			if (this.killSummonedMobsAlongWithAbilityOwner)
			{
				for (int i = this.summonedMobs.Count - 1; i >= 0; i--)
				{
					IGameMob gameMob = this.summonedMobs[i];
					gameMob.Killed -= this.OnSummonedMobKilled;
					gameMob.KillMob(abilityOwner);
				}
			}
			this.summonedMobs.Clear();
			base.OnOwnerDestroyed(abilityOwner);
		}

		// Token: 0x06001FD5 RID: 8149 RVA: 0x000642EB File Offset: 0x000624EB
		private void OnSummonedMobKilled(IGameMob summonedMob)
		{
			summonedMob.Killed -= this.OnSummonedMobKilled;
			this.summonedMobs.Remove(summonedMob);
		}

		// Token: 0x06001FD6 RID: 8150 RVA: 0x0006430C File Offset: 0x0006250C
		protected override void OnInitialize(object context)
		{
			base.OnInitialize(context);
			this.UsingDuration = 0f;
			base.HasInfiniteUsingDuration = false;
			this.summonableMobRadius = this.defaultSummonableMobRadius;
			this.statsListProvider = new AbilityStatsListGenerator(this);
		}

		// Token: 0x06001FD7 RID: 8151 RVA: 0x00064340 File Offset: 0x00062540
		protected override void OnDestroy()
		{
			for (int i = 0; i < this.summonedMobs.Count; i++)
			{
				this.summonedMobs[i].Killed -= this.OnSummonedMobKilled;
			}
			base.OnDestroy();
		}

		// Token: 0x06001FDA RID: 8154 RVA: 0x0006440C File Offset: 0x0006260C
		[CompilerGenerated]
		internal static async void <SummonMob>g__StartMobUnhideTask|75_0(GameObject obj, float delay)
		{
			float startTime = Time.time;
			while (GameApplication.IsGameLoopRunning() && Time.time - startTime < delay)
			{
				await Task.Yield();
			}
			if (!obj.IsNull())
			{
				obj.SetActive(true);
			}
		}

		// Token: 0x040013FE RID: 5118
		private static readonly GameMobSpawningInfo MobSpawnerInfo = new GameMobSpawningInfo();

		// Token: 0x040013FF RID: 5119
		private static readonly MobBehaviour.FactoryArgs MobsFactoryArgs = new MobBehaviour.FactoryArgs
		{
			spawnerInfo = SummoningAbility.MobSpawnerInfo
		};

		// Token: 0x04001400 RID: 5120
		private static GameMobsFactory mobsFactory;

		// Token: 0x04001405 RID: 5125
		public AbilityTypes abilityType;

		// Token: 0x04001406 RID: 5126
		[Space(5f)]
		[Tooltip("Объект, возле которого будут отспавнены призванные мобы.")]
		public SummoningAbility.SummoningTarget summoningTarget;

		// Token: 0x04001407 RID: 5127
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		[Tooltip("Айди мобов для призыва.")]
		public MobBehaviour.ID summonableMobID;

		// Token: 0x04001408 RID: 5128
		public GameObject summonableMobPrefab;

		// Token: 0x04001409 RID: 5129
		[Tooltip("Фракция, к которой будет принадлежать моб. По умолчанию будет принадлежать к фракции призвавшего.")]
		public GameMobFactions summonedMobsFaction = GameMobFactions.None;

		// Token: 0x0400140A RID: 5130
		[SerializeField]
		[Tooltip("Сколько мобов будет призвано за 1 использование.")]
		private int summoningCount = 1;

		// Token: 0x0400140B RID: 5131
		[SerializeField]
		[Tooltip("Максимальное количество призванных мобов. При значениях <= 0 не ограничивается.")]
		private int maxSummoningCount = 5;

		// Token: 0x0400140C RID: 5132
		[Tooltip("Насколько далеко от цели призыва будут отспавнены мобы.")]
		public float distanceFromSummoningTarget = 0.3f;

		// Token: 0x0400140D RID: 5133
		public float defaultSummonableMobRadius = 1f;

		// Token: 0x0400140E RID: 5134
		public bool randomizeSummoningPoints;

		// Token: 0x0400140F RID: 5135
		public bool summonInCursorDirection;

		// Token: 0x04001410 RID: 5136
		[Tooltip("Будут ли призванные мобы агрессивными.")]
		public bool summonAggressiveMobs = true;

		// Token: 0x04001411 RID: 5137
		[Tooltip("Будут ли призванные мобы иметь ответную агрессию.")]
		public bool summonMobsWithReactiveAggression = true;

		// Token: 0x04001412 RID: 5138
		public bool summonedMobsCanShareAggression = true;

		// Token: 0x04001413 RID: 5139
		public bool summonToIndividualGroups;

		// Token: 0x04001414 RID: 5140
		[Tooltip("Будут ли призванные мобы убиты после смерти их хозяина.")]
		public bool killSummonedMobsAlongWithAbilityOwner;

		// Token: 0x04001415 RID: 5141
		[SerializeField]
		[Tooltip("Максимальное время жизни призванных мобов.")]
		private float summonedMobsMaxLifetime;

		// Token: 0x04001416 RID: 5142
		public bool blockHealingForSummonedMobs;

		// Token: 0x04001417 RID: 5143
		public float summonedMobsUnhideDelay;

		// Token: 0x04001418 RID: 5144
		private MobBehaviour.FactoryPrototype summonableMobFactoryData;

		// Token: 0x04001419 RID: 5145
		private float summonableMobRadius;

		// Token: 0x0400141A RID: 5146
		private DamageGenerator summonableMobDamage;

		// Token: 0x0400141B RID: 5147
		private AbilityStatsListGenerator statsListProvider;

		// Token: 0x0400141C RID: 5148
		[NonSerialized]
		private readonly List<IGameMob> summonedMobs = new List<IGameMob>(25);

		// Token: 0x0200057F RID: 1407
		public enum SummoningTarget
		{
			// Token: 0x04001C83 RID: 7299
			AbilityOwner,
			// Token: 0x04001C84 RID: 7300
			AbilityTarget,
			// Token: 0x04001C85 RID: 7301
			AbilityTargetPoint
		}
	}
}
