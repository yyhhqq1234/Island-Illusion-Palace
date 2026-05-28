using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Damage.Projectiles;
using Game.Stats;
using UnityEngine;
using Unliving.LeveledItems;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x020003B2 RID: 946
	[CreateAssetMenu(fileName = "ProjectileAbility", menuName = "Abilities/Projectile Ability")]
	public class ProjectileAbility : ProjectileAbilityBase, ILeveledItem, IItemLevelProvider, IMobStatsListProvider, IStatsListProvider<MobStatModifier>, ITempMobStatsModifiersReceiver, ITempStatsModifiersReceiver<TargetedMobStatModifier>, ITypedMobActivationAbility, IAbility, IDestroyable
	{
		// Token: 0x06001F41 RID: 8001 RVA: 0x00062A60 File Offset: 0x00060C60
		public static void TryAimToEnemyTarget(ProjectileAbilityBase ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			IGameMob gameMob = ability.FindAttackTarget();
			if (gameMob != null)
			{
				abilityUsingArgs.targetPosition = gameMob.HitColliderCenter;
				abilityUsingArgs.targetObject = (Component)gameMob;
				return;
			}
			Vector2 v;
			if (baseGameMob != null)
			{
				Vector2 currentLookDirection = baseGameMob.CurrentLookDirection;
				v = QuaternionExtensions.RotateVector2D(UnityEngine.Random.Range(-75f, 75f), currentLookDirection, false);
			}
			else
			{
				v = QuaternionExtensions.RotateVector2D(UnityEngine.Random.Range(-180f, 180f), new Vector2
				{
					y = 1f
				}, false);
			}
			abilityUsingArgs.targetPosition = ability.OwnerPosition + v * ability.Range;
		}

		// Token: 0x17000645 RID: 1605
		// (get) Token: 0x06001F42 RID: 8002 RVA: 0x00062B18 File Offset: 0x00060D18
		// (set) Token: 0x06001F43 RID: 8003 RVA: 0x00062B20 File Offset: 0x00060D20
		public override int ID { get; set; }

		// Token: 0x17000646 RID: 1606
		// (get) Token: 0x06001F44 RID: 8004 RVA: 0x00062B29 File Offset: 0x00060D29
		// (set) Token: 0x06001F45 RID: 8005 RVA: 0x00062B31 File Offset: 0x00060D31
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

		// Token: 0x17000647 RID: 1607
		// (get) Token: 0x06001F46 RID: 8006 RVA: 0x00062B3A File Offset: 0x00060D3A
		// (set) Token: 0x06001F47 RID: 8007 RVA: 0x00062B42 File Offset: 0x00060D42
		public bool UseAbilityOnProjectileHit
		{
			get
			{
				return this._useAbilityOnProjectileHit;
			}
			set
			{
				this._useAbilityOnProjectileHit = value;
			}
		}

		// Token: 0x17000648 RID: 1608
		// (get) Token: 0x06001F48 RID: 8008 RVA: 0x00062B4B File Offset: 0x00060D4B
		// (set) Token: 0x06001F49 RID: 8009 RVA: 0x00062B53 File Offset: 0x00060D53
		public float ProjectileZoneEffectLifetime
		{
			get
			{
				return this._projectileZoneEffectLifetime;
			}
			set
			{
				this._projectileZoneEffectLifetime = value;
			}
		}

		// Token: 0x17000649 RID: 1609
		// (get) Token: 0x06001F4A RID: 8010 RVA: 0x00062B5C File Offset: 0x00060D5C
		// (set) Token: 0x06001F4B RID: 8011 RVA: 0x00062B71 File Offset: 0x00060D71
		public override BuffsGeneratorBuilderAsset.ReferenceBase[] BuffsGeneratorsBuilders
		{
			get
			{
				return this.buffsGenerators;
			}
			set
			{
				this.buffsGenerators = (BuffsGeneratorBuilderAsset.Reference[])value;
			}
		}

		// Token: 0x1700064A RID: 1610
		// (get) Token: 0x06001F4C RID: 8012 RVA: 0x00062B7F File Offset: 0x00060D7F
		public override bool IsZoneEffectAbility
		{
			get
			{
				return this._projectileZoneEffectLifetime > 0f;
			}
		}

		// Token: 0x1700064B RID: 1611
		// (get) Token: 0x06001F4D RID: 8013 RVA: 0x00062B8E File Offset: 0x00060D8E
		// (set) Token: 0x06001F4E RID: 8014 RVA: 0x00062B96 File Offset: 0x00060D96
		public int ItemLevel { get; set; }

		// Token: 0x1700064C RID: 1612
		// (get) Token: 0x06001F4F RID: 8015 RVA: 0x00062B9F File Offset: 0x00060D9F
		IReadOnlyList<IModifiableStat<MobStatModifier>> IStatsListProvider<MobStatModifier>.Stats
		{
			get
			{
				return this.statsListProvider.Stats;
			}
		}

		// Token: 0x1700064D RID: 1613
		// (get) Token: 0x06001F50 RID: 8016 RVA: 0x00062BAC File Offset: 0x00060DAC
		MobActivationAbilityType ITypedMobActivationAbility.ActivationAbilityType
		{
			get
			{
				if (!this._isPostMortemAbility)
				{
					return MobActivationAbilityType.None;
				}
				return this.mobActivationType;
			}
		}

		// Token: 0x06001F51 RID: 8017 RVA: 0x00062BBE File Offset: 0x00060DBE
		protected override ProjectileDataBase GetProjectileData()
		{
			return this.projectilePrototypeAsset;
		}

		// Token: 0x06001F52 RID: 8018 RVA: 0x00062BC8 File Offset: 0x00060DC8
		protected override bool PrepareProjectileEffectUsing(ProjectileHitInfo projectileHitInfo, BaseAbility.UsingArgs projectileEffectUsingArgs, out bool sendBuffs)
		{
			sendBuffs = true;
			BaseProjectile.HitInfo hitInfo = projectileHitInfo as BaseProjectile.HitInfo;
			if (hitInfo != null && (this._useAbilityOnProjectileHit || hitInfo.IsFinalHit))
			{
				hitInfo.isEffectiveHit = true;
				Component hitReceiver = hitInfo.hitReceiver;
				projectileEffectUsingArgs.targetPosition = hitInfo.point;
				if (hitReceiver.InLayerMask(base.ValidObjectLayers))
				{
					projectileEffectUsingArgs.targetObject = hitReceiver;
				}
				if (!this.IsZoneEffectAbility)
				{
					return true;
				}
				MobStatModifier ownerTotalDamageModifier = this.GetOwnerTotalDamageModifier();
				AbilityEffectZone abilityEffectZone = AbilitiesFactory.CreateEffectZone(this, projectileEffectUsingArgs, base.OwnerBehaviour, this._projectileZoneEffectLifetime, base.ProjectileEffectRange, new MobStatModifier?(ownerTotalDamageModifier));
				if (abilityEffectZone != null)
				{
					sendBuffs = false;
					base.RaiseEffectZoneEvent(abilityEffectZone);
					return this._abilityEffects.Length != 0;
				}
			}
			return false;
		}

		// Token: 0x06001F53 RID: 8019 RVA: 0x00062C7C File Offset: 0x00060E7C
		protected override void CollectProjectileEffectTargets(BaseAbility.UsingArgs hitUsingArgs)
		{
			ProjectileAbility.TargetsCollectionArgs.PrepareForTargetsCollection(this, hitUsingArgs);
			this.CollectTargets(ProjectileAbility.TargetsCollectionArgs, hitUsingArgs);
		}

		// Token: 0x06001F54 RID: 8020 RVA: 0x00062C96 File Offset: 0x00060E96
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler != null)
			{
				tempMobStatsModifiersHandler.ApplyModifiers(new Predicate<IStat>(ProjectileAbility.<PerformAbility>g__IsProjectileStat|41_0));
			}
			base.PerformAbility(usingArgs);
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler2 = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler2 == null)
			{
				return;
			}
			tempMobStatsModifiersHandler2.RemoveAppliedModifiers();
		}

		// Token: 0x06001F55 RID: 8021 RVA: 0x00062CCC File Offset: 0x00060ECC
		public void AddTempStatModifier(TargetedMobStatModifier statModifier)
		{
			if (this.tempStatsModifiersHandler == null)
			{
				this.tempStatsModifiersHandler = new TempMobStatsModifiersHandler<TargetedMobStatModifier>(this);
			}
			this.tempStatsModifiersHandler.AddStatModifier(statModifier);
		}

		// Token: 0x06001F56 RID: 8022 RVA: 0x00062CEE File Offset: 0x00060EEE
		protected override void OnInitialize(object context)
		{
			base.OnInitialize(context);
			this.statsListProvider = new AbilityStatsListGenerator(this);
		}

		// Token: 0x06001F57 RID: 8023 RVA: 0x00062D03 File Offset: 0x00060F03
		protected override void OnPrepared(BaseAbility.UsingArgs usingArgs)
		{
			if (this.forceLaunchProjectileToEnemyTarget)
			{
				ProjectileAbility.TryAimToEnemyTarget(this, usingArgs);
			}
			else if (this.forceLaunchProjectileToCursor)
			{
				usingArgs.PassWorldCursorPosition(this);
			}
			base.OnPrepared(usingArgs);
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x00062D30 File Offset: 0x00060F30
		protected override bool SetCustomLaunchParams(BaseAbility.UsingArgs abilityUsingArgs, int launchPointIndex, ProjectileLaunchArgs launchArgs)
		{
			ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint;
			Vector3 position;
			Vector3 direction;
			base.GetProjectileLaunchParams(abilityUsingArgs, launchPointIndex, out projectileLaunchPoint, out position, out direction);
			launchArgs.launcher = base.Owner;
			MobSacrificeAbility.Args args;
			if (abilityUsingArgs.TryGetAdditionalContext(out args))
			{
				abilityUsingArgs.targetPosition = args.targetActivationPoint;
				launchArgs.position = position;
				launchArgs.direction = (args.targetActivationPoint - launchArgs.position).normalized;
				return true;
			}
			if (this.forceLaunchProjectileToEnemyTarget || this.forceLaunchProjectileToCursor)
			{
				launchArgs.position = position;
				launchArgs.direction = direction;
				return true;
			}
			return false;
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x00062DC6 File Offset: 0x00060FC6
		protected override void OnProjectileUsingPrepared(BaseAbility.UsingArgs hitUsingArgs, ProjectileHitInfo hitArgs)
		{
			base.OnProjectileUsingPrepared(hitUsingArgs, hitArgs);
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler == null)
			{
				return;
			}
			tempMobStatsModifiersHandler.ApplyModifiers(null);
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x00062DE1 File Offset: 0x00060FE1
		protected override void OnProjectileHit(ProjectileHitInfo hitArgs)
		{
			base.OnProjectileHit(hitArgs);
			TempMobStatsModifiersHandler<TargetedMobStatModifier> tempMobStatsModifiersHandler = this.tempStatsModifiersHandler;
			if (tempMobStatsModifiersHandler == null)
			{
				return;
			}
			tempMobStatsModifiersHandler.RemoveAppliedModifiers();
		}

		// Token: 0x06001F5D RID: 8029 RVA: 0x00062E1C File Offset: 0x0006101C
		[CompilerGenerated]
		internal static bool <PerformAbility>g__IsProjectileStat|41_0(IStat stat)
		{
			return ((MobStatID)stat.ID).IsProjectileStat();
		}

		// Token: 0x040013D0 RID: 5072
		private static readonly GameLocation.MobsGatheringArgs TargetsCollectionArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x040013D3 RID: 5075
		public AbilityTypes abilityType;

		// Token: 0x040013D4 RID: 5076
		public MobActivationAbilityType mobActivationType = MobActivationAbilityType.Ranged;

		// Token: 0x040013D5 RID: 5077
		[SerializeField]
		private BuffsGeneratorBuilderAsset.Reference[] buffsGenerators;

		// Token: 0x040013D6 RID: 5078
		[SerializeField]
		[Tooltip("Будет ли снаряд накладывать эффект при столкновении с объектами. Если неактивно, то будет пролетать насквозь.")]
		private bool _useAbilityOnProjectileHit = true;

		// Token: 0x040013D7 RID: 5079
		[SerializeField]
		[Tooltip("Время активности зоны с эффектом, созданной проджектайлом. Если значение <= 0, то зона создаваться не будет.")]
		private float _projectileZoneEffectLifetime;

		// Token: 0x040013D8 RID: 5080
		[SerializeField]
		private ProjectileDataBase projectilePrototypeAsset;

		// Token: 0x040013D9 RID: 5081
		public bool forceLaunchProjectileToCursor;

		// Token: 0x040013DA RID: 5082
		public bool forceLaunchProjectileToEnemyTarget;

		// Token: 0x040013DB RID: 5083
		private TempMobStatsModifiersHandler<TargetedMobStatModifier> tempStatsModifiersHandler;

		// Token: 0x040013DC RID: 5084
		private AbilityStatsListGenerator statsListProvider;
	}
}
