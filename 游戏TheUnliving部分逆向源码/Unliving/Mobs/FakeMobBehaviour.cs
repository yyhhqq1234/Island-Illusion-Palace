using System;
using System.Collections.Generic;
using Common;
using Common.CollectionsExtensions;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.Stats;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Unliving.Abilities;
using Unliving.Abilities.VFX;
using Unliving.AbilityResources;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;

namespace Unliving.Mobs
{
	// Token: 0x020001CE RID: 462
	public sealed class FakeMobBehaviour : GameBehaviourBase, IGameMob, IBuffableObject, ILifetimeDependent, IStatsOwner<MobStatModifier>, INotifyAbilityUsedOnTarget
	{
		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000E78 RID: 3704 RVA: 0x0002DD63 File Offset: 0x0002BF63
		public GameObject GameObject
		{
			get
			{
				return this.mobObject;
			}
		}

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000E79 RID: 3705 RVA: 0x0002DD6C File Offset: 0x0002BF6C
		public string Name
		{
			get
			{
				string result;
				if ((result = this.mobName) == null)
				{
					result = (this.mobName = base.name);
				}
				return result;
			}
		}

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000E7A RID: 3706 RVA: 0x0002DD92 File Offset: 0x0002BF92
		public int LayerMask
		{
			get
			{
				if (this.mobLayerMask == 0)
				{
					this.mobLayerMask = 1 << this.mobObject.layer;
				}
				return this.mobLayerMask;
			}
		}

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000E7B RID: 3707 RVA: 0x0002DDB8 File Offset: 0x0002BFB8
		public bool IsCharacter
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000E7C RID: 3708 RVA: 0x0002DDBB File Offset: 0x0002BFBB
		public float Radius
		{
			get
			{
				return 0.1f;
			}
		}

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000E7D RID: 3709 RVA: 0x0002DDC2 File Offset: 0x0002BFC2
		public NavMeshAgent NavMeshAgent
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000E7E RID: 3710 RVA: 0x0002DDC5 File Offset: 0x0002BFC5
		public Collider2D HitCollider
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000E7F RID: 3711 RVA: 0x0002DDC8 File Offset: 0x0002BFC8
		public Vector2 HitColliderCenter
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000E80 RID: 3712 RVA: 0x0002DDDA File Offset: 0x0002BFDA
		// (set) Token: 0x06000E81 RID: 3713 RVA: 0x0002DDEC File Offset: 0x0002BFEC
		public Vector2 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000E82 RID: 3714 RVA: 0x0002DDFF File Offset: 0x0002BFFF
		public GameMobFactions Faction
		{
			get
			{
				GameMobsGroupControllerBase gameMobsGroupControllerBase = this.Group;
				if (gameMobsGroupControllerBase == null)
				{
					return this.faction;
				}
				return gameMobsGroupControllerBase.Faction;
			}
		}

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000E83 RID: 3715 RVA: 0x0002DE17 File Offset: 0x0002C017
		// (set) Token: 0x06000E84 RID: 3716 RVA: 0x0002DE1F File Offset: 0x0002C01F
		public bool IsKinematic { get; set; }

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000E85 RID: 3717 RVA: 0x0002DE28 File Offset: 0x0002C028
		public GameMobMotionControllerBase MotionController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002DE RID: 734
		// (get) Token: 0x06000E86 RID: 3718 RVA: 0x0002DE2B File Offset: 0x0002C02B
		public IDamageable HitPointsController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002DF RID: 735
		// (get) Token: 0x06000E87 RID: 3719 RVA: 0x0002DE2E File Offset: 0x0002C02E
		// (set) Token: 0x06000E88 RID: 3720 RVA: 0x0002DE36 File Offset: 0x0002C036
		public GameMobsGroupControllerBase Group
		{
			get
			{
				return this.group;
			}
			set
			{
				this.LastGroup = this.group;
				this.group = value;
			}
		}

		// Token: 0x170002E0 RID: 736
		// (get) Token: 0x06000E89 RID: 3721 RVA: 0x0002DE4B File Offset: 0x0002C04B
		// (set) Token: 0x06000E8A RID: 3722 RVA: 0x0002DE53 File Offset: 0x0002C053
		public GameMobsGroupControllerBase LastGroup { get; private set; }

		// Token: 0x170002E1 RID: 737
		// (get) Token: 0x06000E8B RID: 3723 RVA: 0x0002DE5C File Offset: 0x0002C05C
		IBuffsController IBuffableObject.BuffsController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002E2 RID: 738
		// (get) Token: 0x06000E8C RID: 3724 RVA: 0x0002DE60 File Offset: 0x0002C060
		public GameLocation CurrentLocation
		{
			get
			{
				IGameLocationProvider gameLocationProvider;
				if (this.currentLocation == null && base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
				{
					this.currentLocation = gameLocationProvider.CurrentLocation;
				}
				return this.currentLocation;
			}
		}

		// Token: 0x170002E3 RID: 739
		// (get) Token: 0x06000E8D RID: 3725 RVA: 0x0002DE9B File Offset: 0x0002C09B
		public AbilityResourcesGenerator ResourcesGenerator
		{
			get
			{
				return this.resourcesGenerator;
			}
		}

		// Token: 0x170002E4 RID: 740
		// (get) Token: 0x06000E8E RID: 3726 RVA: 0x0002DEA3 File Offset: 0x0002C0A3
		// (set) Token: 0x06000E8F RID: 3727 RVA: 0x0002DEAB File Offset: 0x0002C0AB
		public GameMobSummoningContext SummonerInfo { get; private set; }

		// Token: 0x170002E5 RID: 741
		// (get) Token: 0x06000E90 RID: 3728 RVA: 0x0002DEB4 File Offset: 0x0002C0B4
		public bool IsPlayerMob
		{
			get
			{
				IGameMob owningMob = this.GetOwningMob();
				return owningMob != null && owningMob.IsPlayerMob;
			}
		}

		// Token: 0x170002E6 RID: 742
		// (get) Token: 0x06000E91 RID: 3729 RVA: 0x0002DEC7 File Offset: 0x0002C0C7
		public bool IsCrowdObstacle
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002E7 RID: 743
		// (get) Token: 0x06000E92 RID: 3730 RVA: 0x0002DECA File Offset: 0x0002C0CA
		public bool IsMinorAttackTarget
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002E8 RID: 744
		// (get) Token: 0x06000E93 RID: 3731 RVA: 0x0002DECD File Offset: 0x0002C0CD
		public bool IsSummoned
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002E9 RID: 745
		// (get) Token: 0x06000E94 RID: 3732 RVA: 0x0002DED0 File Offset: 0x0002C0D0
		public bool IsRevived
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170002EA RID: 746
		// (get) Token: 0x06000E95 RID: 3733 RVA: 0x0002DED3 File Offset: 0x0002C0D3
		public bool IsSacrificed
		{
			get
			{
				IGameMob owningMob = this.GetOwningMob();
				return owningMob != null && owningMob.IsSacrificed;
			}
		}

		// Token: 0x170002EB RID: 747
		// (get) Token: 0x06000E96 RID: 3734 RVA: 0x0002DEE6 File Offset: 0x0002C0E6
		public bool IsKilled
		{
			get
			{
				return this.isKilled;
			}
		}

		// Token: 0x170002EC RID: 748
		// (get) Token: 0x06000E97 RID: 3735 RVA: 0x0002DEEE File Offset: 0x0002C0EE
		public float RemainingLifetime
		{
			get
			{
				if (this.destructionTime <= 0f)
				{
					return this.lifetime;
				}
				return this.destructionTime - Time.time;
			}
		}

		// Token: 0x170002ED RID: 749
		// (get) Token: 0x06000E98 RID: 3736 RVA: 0x0002DF10 File Offset: 0x0002C110
		// (set) Token: 0x06000E99 RID: 3737 RVA: 0x0002DF18 File Offset: 0x0002C118
		float ILifetimeDependent.Lifetime
		{
			get
			{
				return this.lifetime;
			}
			set
			{
				this.lifetime = value;
			}
		}

		// Token: 0x170002EE RID: 750
		// (get) Token: 0x06000E9A RID: 3738 RVA: 0x0002DF21 File Offset: 0x0002C121
		public IStatsController<MobStatModifier> StatsController
		{
			get
			{
				return this.statsController;
			}
		}

		// Token: 0x170002EF RID: 751
		// (get) Token: 0x06000E9B RID: 3739 RVA: 0x0002DF29 File Offset: 0x0002C129
		public UnityEvent AllAbilitiesCompleted
		{
			get
			{
				return this.allAbilitiesCompleted;
			}
		}

		// Token: 0x1400009F RID: 159
		// (add) Token: 0x06000E9C RID: 3740 RVA: 0x0002DF34 File Offset: 0x0002C134
		// (remove) Token: 0x06000E9D RID: 3741 RVA: 0x0002DF6C File Offset: 0x0002C16C
		public event Action<IAbility, object, object> AbilityUsedOnTarget;

		// Token: 0x140000A0 RID: 160
		// (add) Token: 0x06000E9E RID: 3742 RVA: 0x0002DFA4 File Offset: 0x0002C1A4
		// (remove) Token: 0x06000E9F RID: 3743 RVA: 0x0002DFDC File Offset: 0x0002C1DC
		public event Action<IGameMob> Killed;

		// Token: 0x06000EA0 RID: 3744 RVA: 0x0002E014 File Offset: 0x0002C214
		private IGameMob GetOwningMob()
		{
			GameMobSummoningContext summonerInfo = this.SummonerInfo;
			object obj = (summonerInfo != null) ? summonerInfo.summoningSource : null;
			IGameMob result;
			if ((result = (obj as IGameMob)) == null)
			{
				IAbility ability = obj as IAbility;
				result = (((ability != null) ? ability.Owner : null) as IGameMob);
			}
			return result;
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x0002E058 File Offset: 0x0002C258
		private FakeMobBehaviour.AbilityDescription GetAbilityDescription(BaseAbility ability)
		{
			for (int i = 0; i < this.abilities.Length; i++)
			{
				ref FakeMobBehaviour.AbilityDescription ptr = ref this.abilities[i];
				if (ptr.IsTargetAbility(ability))
				{
					return ptr;
				}
			}
			throw new InvalidOperationException(base.name + ": " + ability.Name);
		}

		// Token: 0x06000EA2 RID: 3746 RVA: 0x0002E0B0 File Offset: 0x0002C2B0
		private void SetAbilityExpired(BaseAbility ability, FakeMobBehaviour.AbilityDescription abilityDescription)
		{
			this.expiredAbilitiesCount++;
			ability.SetBlocked(true);
			if (this.expiredAbilitiesCount == this.abilities.Length)
			{
				UnityEvent unityEvent = this.allAbilitiesCompleted;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
			}
			if (abilityDescription.forceDestroyOwnerAfterUsing || (!this.HasLifetime() && this.expiredAbilitiesCount == this.abilities.Length))
			{
				this.SetKilled();
			}
		}

		// Token: 0x06000EA3 RID: 3747 RVA: 0x0002E11C File Offset: 0x0002C31C
		private bool TryCreateAuraEffect(ref FakeMobBehaviour.AbilityDescription abilityDescription)
		{
			BaseAbility abilityPrototype = abilityDescription.abilityPrototype;
			if (abilityDescription.isAuraEffectAbility || abilityPrototype.IsZoneEffectAbility)
			{
				MobStatModifier statModifiersSum = this.statsController.GetStatModifiersSum(MobStatID.MobDamage);
				AbilityEffectZone abilityEffectZone = AbilitiesFactory.CreateAuraEffect(this, abilityPrototype, new MobStatModifier?(statModifiersSum));
				if (abilityEffectZone != null)
				{
					this.auraEffects.Add(abilityEffectZone);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000EA4 RID: 3748 RVA: 0x0002E174 File Offset: 0x0002C374
		private void PrepareAbility(BaseAbility ability, int abilityIndex)
		{
			MobAbilityParameters mobAbilityParams = ability.GetMobAbilityParams();
			bool flag = mobAbilityParams != null && mobAbilityParams.targetSelectionMethodOverride != GameMobTargetSelector.SelectionMethod.None;
			if (flag)
			{
				this.abilitiesTargetsData[abilityIndex] = new FakeMobBehaviour.AbilityTargetsData
				{
					targetSearchParams = mobAbilityParams,
					targetInfo = new BaseAbility.UsingArgs()
				};
				if (this.abilitiesTargetsGatheringArgs == null)
				{
					this.abilitiesTargetsGatheringArgs = new GameLocation.MobsGatheringArgs
					{
						position = base.transform.position
					};
				}
				if (this.abilityTargetSelector == null)
				{
					this.abilityTargetSelector = new GameMobTargetSelector
					{
						AdditionalTargetValidator = new Predicate<BaseGameMob>(this.IsAbilityTargetValid)
					};
				}
				if (ability.Range > this.abilitiesTargetsGatheringArgs.range)
				{
					this.abilitiesTargetsGatheringArgs.range = ability.Range;
				}
				this.abilitiesTargetsGatheringArgs.layers |= ability.ValidObjectLayers;
			}
			else
			{
				this.abilitiesTargetsData[abilityIndex] = new FakeMobBehaviour.AbilityTargetsData
				{
					targetInfo = new BaseAbility.UsingArgs()
				};
			}
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.isObjectTargetRequired = flag;
			}
			if (!this.hasVFX)
			{
				AbilityVFXController abilityVFXController;
				this.hasVFX = ability.TryGetExtension(out abilityVFXController);
			}
			ProxyStat<MobStatModifier> proxyStat = this.statsController.GetStat(2) as ProxyStat<MobStatModifier>;
			if (proxyStat != null)
			{
				IMobStatsListProvider mobStatsListProvider = ability as IMobStatsListProvider;
				IReadOnlyList<IModifiableStat<MobStatModifier>> readOnlyList = (mobStatsListProvider != null) ? mobStatsListProvider.Stats : null;
				if (readOnlyList != null)
				{
					for (int i = 0; i < readOnlyList.Count; i++)
					{
						IModifiableStat<MobStatModifier> modifiableStat = readOnlyList[i];
						if (modifiableStat.ID == 2)
						{
							proxyStat.AddStat(modifiableStat);
						}
					}
				}
			}
		}

		// Token: 0x06000EA5 RID: 3749 RVA: 0x0002E30B File Offset: 0x0002C50B
		private void SetCurrentTargetSearchingAbility(BaseAbility ability)
		{
			this.currentTargetSearchAbilityLayers = ability.ValidObjectLayers;
		}

		// Token: 0x06000EA6 RID: 3750 RVA: 0x0002E31E File Offset: 0x0002C51E
		private bool IsAbilityTargetValid(BaseGameMob abilityTarget)
		{
			return (this.currentTargetSearchAbilityLayers & abilityTarget.LayerMask) != 0;
		}

		// Token: 0x06000EA7 RID: 3751 RVA: 0x0002E330 File Offset: 0x0002C530
		private void CollectAbilitiesTargets()
		{
			if (this.CurrentLocation == null || this.abilitiesTargetsGatheringArgs == null)
			{
				this.abilitiesTargetsCount = new int?(0);
				return;
			}
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			if (realtimeSinceStartup < this.nextAbilitiesTargetsUpdateTime)
			{
				return;
			}
			this.abilitiesTargetsCount = new int?(this.currentLocation.GetMobsInRange(this.abilitiesTargetsGatheringArgs, out this.abilitiesTargets));
			this.nextAbilitiesTargetsUpdateTime = realtimeSinceStartup + 0.2f;
		}

		// Token: 0x06000EA8 RID: 3752 RVA: 0x0002E39C File Offset: 0x0002C59C
		private void FindAbilityTarget(ref FakeMobBehaviour.AbilityTargetsData targetedAbilityData, BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			if (this.abilitiesTargetsCount == null || targetedAbilityData.isExpiredAbilityData)
			{
				return;
			}
			int? num = this.abilitiesTargetsCount;
			int num2 = 0;
			if ((num.GetValueOrDefault() == num2 & num != null) && !targetedAbilityData.isExpiredAbilityData)
			{
				targetedAbilityData.isExpiredAbilityData = true;
				if (this.expiredAbilitiesCount < this.abilities.Length)
				{
					this.SetAbilityExpired(ability, this.GetAbilityDescription(ability));
				}
				return;
			}
			this.abilityTargetSelector.targetSelectionRadius = ability.Range;
			MobAbilityParameters targetSearchParams = targetedAbilityData.targetSearchParams;
			this.abilityTargetSelector.SetTargetsEstimationParams(targetSearchParams.targetSelectionMethodOverride, targetSearchParams.targetSelectionPriorityOverride);
			this.SetCurrentTargetSearchingAbility(ability);
			BaseGameMob baseGameMob = this.abilityTargetSelector.FindNewTarget(this.abilitiesTargets, this.abilitiesTargetsCount.Value);
			usingArgs.targetObject = baseGameMob;
			if (baseGameMob != null)
			{
				usingArgs.targetPosition = baseGameMob.Position;
			}
		}

		// Token: 0x06000EA9 RID: 3753 RVA: 0x0002E47E File Offset: 0x0002C67E
		private bool HasLifetime()
		{
			return this.destructionTime > 0f;
		}

		// Token: 0x06000EAA RID: 3754 RVA: 0x0002E48D File Offset: 0x0002C68D
		private bool IsLifetimeExpired()
		{
			return this.isInitialized && this.HasLifetime() && Time.time > this.destructionTime;
		}

		// Token: 0x06000EAB RID: 3755 RVA: 0x0002E4B0 File Offset: 0x0002C6B0
		private void TryDestroyVFX()
		{
			if (!this.hasVFX)
			{
				return;
			}
			Transform transform = base.transform;
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				ParticleSystem particleSystem;
				if (transform.GetChild(i).TryGetComponent<ParticleSystem>(out particleSystem))
				{
					particleSystem.DestroyAfterEmission(true, this.forceShowVFX);
				}
			}
		}

		// Token: 0x06000EAC RID: 3756 RVA: 0x0002E4FD File Offset: 0x0002C6FD
		private void SetKilled()
		{
			this.killMob = true;
		}

		// Token: 0x06000EAD RID: 3757 RVA: 0x0002E508 File Offset: 0x0002C708
		public void SetCreationType(GameMobCreationType creationType, object context)
		{
			GameMobSummoningContext gameMobSummoningContext = context as GameMobSummoningContext;
			if (gameMobSummoningContext != null)
			{
				this.SummonerInfo = gameMobSummoningContext;
				ValueTuple<MobStatID, MobStatModifier>[] statsModifiers = gameMobSummoningContext.statsModifiers;
				if (creationType == GameMobCreationType.Default && statsModifiers != null)
				{
					for (int i = 0; i < statsModifiers.Length; i++)
					{
						this.statsController.AddModifier((int)statsModifiers[i].Item1, statsModifiers[i].Item2);
					}
				}
			}
		}

		// Token: 0x06000EAE RID: 3758 RVA: 0x0002E565 File Offset: 0x0002C765
		public void SetNavMeshAgentActive(bool isActive)
		{
		}

		// Token: 0x06000EAF RID: 3759 RVA: 0x0002E567 File Offset: 0x0002C767
		public bool CanBeAttackedBy(IGameMob mob)
		{
			return false;
		}

		// Token: 0x06000EB0 RID: 3760 RVA: 0x0002E56C File Offset: 0x0002C76C
		public void KillMob(object mobKiller)
		{
			if (this.isKilled)
			{
				return;
			}
			this.isKilled = true;
			foreach (AbilityEffectZone abilityEffectZone in this.auraEffects)
			{
				abilityEffectZone.gameObject.SetActive(false);
			}
			this.auraEffects.Clear();
			this.TryDestroyVFX();
			Action<IGameMob> killed = this.Killed;
			if (killed != null)
			{
				killed(this);
			}
			UnityEngine.Object.Destroy(this.mobObject);
		}

		// Token: 0x06000EB1 RID: 3761 RVA: 0x0002E600 File Offset: 0x0002C800
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.mobObject = base.gameObject;
			ICollectableAbilityResourcesFactory resourcesFactory;
			if (currentGame.Services.TryGet<ICollectableAbilityResourcesFactory>(out resourcesFactory))
			{
				this.resourcesGenerator = new AbilityResourcesGenerator(this, resourcesFactory);
			}
			this.statsController = new StatsController(this, false);
			this.statsController.AddStat(new MobProxyStat(MobStatID.MobDamage, this));
		}

		// Token: 0x06000EB2 RID: 3762 RVA: 0x0002E65C File Offset: 0x0002C85C
		public void Initialize(FakeMobBehaviour.AbilityDescription[] abilitiesOverride = null)
		{
			if (this.isInitialized)
			{
				return;
			}
			this.abilitiesController = new BaseAbilitiesController(this);
			IGameAbilitiesFactory gameAbilitiesFactory;
			if (base.CurrentGame.Services.TryGet<IGameAbilitiesFactory>(out gameAbilitiesFactory))
			{
				if (abilitiesOverride != null && abilitiesOverride.Length != 0 && this.abilities != abilitiesOverride)
				{
					this.abilities = abilitiesOverride.CloneArray<FakeMobBehaviour.AbilityDescription>();
				}
				this.abilitiesTargetsData = new FakeMobBehaviour.AbilityTargetsData[this.abilities.Length];
				for (int i = 0; i < this.abilities.Length; i++)
				{
					ref FakeMobBehaviour.AbilityDescription ptr = ref this.abilities[i];
					if (!(ptr.abilityPrototype == null) && !this.TryCreateAuraEffect(ref ptr))
					{
						ptr.SetFactoryArgs(this, FakeMobBehaviour.AbilityFactoryArgs);
						BaseAbility baseAbility = (BaseAbility)gameAbilitiesFactory.Create(FakeMobBehaviour.AbilityFactoryArgs);
						if (baseAbility != null)
						{
							if (!ptr.useAbilityRepeatedly)
							{
								baseAbility.MaxUsingCount = 1;
							}
							if (ptr.useCooldownOnStart)
							{
								baseAbility.SetReloadingProgress(0f);
							}
							if (this.lifetime > 0f && ptr.useOnMobDestruction)
							{
								baseAbility.ReloadingTime = this.lifetime;
								baseAbility.MaxUsingCount = 1;
								baseAbility.SetReloadingProgress(0f);
							}
							this.abilitiesController.AddAbility(baseAbility);
							this.PrepareAbility(baseAbility, i);
							baseAbility.Used += this.OnAbilityUsed;
							baseAbility.Completed += this.OnAbilityCompleted;
							baseAbility.Destroyed += this.OnAbilityDestroyed;
						}
					}
				}
			}
			this.isInitialized = true;
		}

		// Token: 0x06000EB3 RID: 3763 RVA: 0x0002E7D8 File Offset: 0x0002C9D8
		public void ActivateAbilities(BaseAbility.UsingArgs usingArgs)
		{
			this.hasAbilitiesInUse = false;
			if (this.isKilled)
			{
				return;
			}
			if (this.isAbilitiesAutoUsingActive)
			{
				if (usingArgs == this.abilitiesAutoUsingArgs)
				{
					if (this.abilityTargetSelector != null)
					{
						this.abilityTargetSelector.targetSelectionPoint = new Vector2?(base.transform.position);
						this.abilityTargetSelector.skipTargetInRangeCheck = false;
					}
				}
				else
				{
					this.abilitiesAutoUsingArgs.Reset();
					this.isAbilitiesAutoUsingActive = false;
				}
			}
			IReadOnlyList<BaseAbility> readOnlyList = this.abilitiesController.Abilities;
			bool flag = this.IsLifetimeExpired();
			for (int i = 0; i < readOnlyList.Count; i++)
			{
				BaseAbility baseAbility = readOnlyList[i];
				BaseAbility.UsingArgs usingArgs2 = usingArgs;
				bool flag2 = this.lifetime > 0f && baseAbility.ReloadingTime == this.lifetime;
				if (this.isAbilitiesAutoUsingActive)
				{
					ref FakeMobBehaviour.AbilityTargetsData ptr = ref this.abilitiesTargetsData[i];
					usingArgs2 = ptr.targetInfo;
					if (ptr.IsTargetSearchRequired())
					{
						if (!baseAbility.IsActivatedOrInPrep())
						{
							this.FindAbilityTarget(ref ptr, baseAbility, usingArgs2);
						}
					}
					else
					{
						usingArgs2.targetPosition = base.transform.position;
					}
				}
				if (!flag || flag2 || baseAbility.PrepProgress > 0f)
				{
					baseAbility.Activate(usingArgs2);
				}
				if (!this.hasAbilitiesInUse)
				{
					this.hasAbilitiesInUse = (baseAbility.PrepProgress > 0f || baseAbility.HasActiveProjectiles() || (flag2 && baseAbility.CurrentUseCount < 1 && baseAbility.IsReloading()));
				}
			}
		}

		// Token: 0x06000EB4 RID: 3764 RVA: 0x0002E94C File Offset: 0x0002CB4C
		public void ActivateAbilitiesAutoUsing(BaseAbility.UsingArgs usingArgs)
		{
			if (this.isKilled)
			{
				return;
			}
			this.isAbilitiesAutoUsingActive = true;
			usingArgs.CopyValuesTo(this.abilitiesAutoUsingArgs);
		}

		// Token: 0x06000EB5 RID: 3765 RVA: 0x0002E96C File Offset: 0x0002CB6C
		private void OnAbilityUsed(IAbility ability, object args)
		{
			if (this.AbilityUsedOnTarget != null)
			{
				BaseAbility.UsingArgs usingArgs = args as BaseAbility.UsingArgs;
				if (usingArgs != null)
				{
					int targetsCount = usingArgs.TargetsCount;
					if (targetsCount != 0)
					{
						IList<Component> targetsList = usingArgs.targetsList;
						for (int i = 0; i < targetsCount; i++)
						{
							this.AbilityUsedOnTarget(ability, targetsList[i], args);
						}
						return;
					}
					if (usingArgs.HasTargetObject)
					{
						this.AbilityUsedOnTarget(ability, usingArgs.targetObject, args);
					}
				}
			}
		}

		// Token: 0x06000EB6 RID: 3766 RVA: 0x0002E9D8 File Offset: 0x0002CBD8
		private void OnAbilityCompleted(IAbility ability, object args)
		{
			BaseAbility baseAbility = (BaseAbility)ability;
			if (!baseAbility.IsMaxUsingCountReached())
			{
				return;
			}
			for (int i = 0; i < this.abilities.Length; i++)
			{
				ref FakeMobBehaviour.AbilityDescription ptr = ref this.abilities[i];
				if (ptr.IsTargetAbility(baseAbility))
				{
					this.abilitiesTargetsData[i].ResetTargetInfo();
					this.SetAbilityExpired(baseAbility, ptr);
					return;
				}
			}
		}

		// Token: 0x06000EB7 RID: 3767 RVA: 0x0002EA3D File Offset: 0x0002CC3D
		private void OnAbilityDestroyed(object obj)
		{
			BaseAbility baseAbility = (BaseAbility)obj;
			baseAbility.Used -= this.OnAbilityUsed;
			baseAbility.Completed -= this.OnAbilityCompleted;
			baseAbility.Destroyed -= this.OnAbilityDestroyed;
		}

		// Token: 0x06000EB8 RID: 3768 RVA: 0x0002EA7A File Offset: 0x0002CC7A
		private void Start()
		{
			this.Initialize(this.abilities);
			if (this.lifetime > 0f)
			{
				this.destructionTime = Time.time + this.lifetime;
			}
			if (this.activateOnStart)
			{
				this.isAbilitiesAutoUsingActive = true;
			}
		}

		// Token: 0x06000EB9 RID: 3769 RVA: 0x0002EAB8 File Offset: 0x0002CCB8
		private void Update()
		{
			if (this.isKilled)
			{
				return;
			}
			if (this.isAbilitiesAutoUsingActive)
			{
				this.CollectAbilitiesTargets();
				this.ActivateAbilities(this.abilitiesAutoUsingArgs);
			}
			this.abilitiesController.UpdateController();
			if (this.IsLifetimeExpired())
			{
				this.SetKilled();
			}
			if (this.killMob && !this.hasAbilitiesInUse)
			{
				this.KillMob(this);
			}
		}

		// Token: 0x06000EBA RID: 3770 RVA: 0x0002EB18 File Offset: 0x0002CD18
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.KillMob(null);
			if (this.abilitiesController != null)
			{
				this.abilitiesController.RemoveAllAbilities(null);
			}
		}

		// Token: 0x04000891 RID: 2193
		private const float AbilitiesTargetsUpdateStep = 0.2f;

		// Token: 0x04000892 RID: 2194
		private static readonly AbilityFactoryArgs AbilityFactoryArgs = new AbilityFactoryArgs
		{
			canGenerateBuffs = true,
			preventRandomSpecialBehaviourGeneration = true
		};

		// Token: 0x04000898 RID: 2200
		[SerializeField]
		private GameMobFactions faction = GameMobFactions.None;

		// Token: 0x04000899 RID: 2201
		[SerializeField]
		private FakeMobBehaviour.AbilityDescription[] abilities;

		// Token: 0x0400089A RID: 2202
		[SerializeField]
		private UnityEvent allAbilitiesCompleted;

		// Token: 0x0400089B RID: 2203
		public bool activateOnStart = true;

		// Token: 0x0400089C RID: 2204
		public float lifetime;

		// Token: 0x0400089D RID: 2205
		public bool hasVFX;

		// Token: 0x0400089E RID: 2206
		public bool forceShowVFX;

		// Token: 0x0400089F RID: 2207
		private readonly BaseAbility.UsingArgs abilitiesAutoUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x040008A0 RID: 2208
		private readonly List<AbilityEffectZone> auraEffects = new List<AbilityEffectZone>(4);

		// Token: 0x040008A1 RID: 2209
		private string mobName;

		// Token: 0x040008A2 RID: 2210
		private GameObject mobObject;

		// Token: 0x040008A3 RID: 2211
		private int mobLayerMask;

		// Token: 0x040008A4 RID: 2212
		private GameMobsGroupControllerBase group;

		// Token: 0x040008A5 RID: 2213
		private GameLocation currentLocation;

		// Token: 0x040008A6 RID: 2214
		private AbilityResourcesGenerator resourcesGenerator;

		// Token: 0x040008A7 RID: 2215
		private BaseAbilitiesController abilitiesController;

		// Token: 0x040008A8 RID: 2216
		private StatsController statsController;

		// Token: 0x040008A9 RID: 2217
		private FakeMobBehaviour.AbilityTargetsData[] abilitiesTargetsData;

		// Token: 0x040008AA RID: 2218
		private GameLocation.MobsGatheringArgs abilitiesTargetsGatheringArgs;

		// Token: 0x040008AB RID: 2219
		private GameMobTargetSelector abilityTargetSelector;

		// Token: 0x040008AC RID: 2220
		private BaseGameMob[] abilitiesTargets;

		// Token: 0x040008AD RID: 2221
		private int? abilitiesTargetsCount;

		// Token: 0x040008AE RID: 2222
		private int currentTargetSearchAbilityLayers;

		// Token: 0x040008AF RID: 2223
		private float nextAbilitiesTargetsUpdateTime;

		// Token: 0x040008B0 RID: 2224
		private bool isAbilitiesAutoUsingActive;

		// Token: 0x040008B1 RID: 2225
		private int expiredAbilitiesCount;

		// Token: 0x040008B2 RID: 2226
		private bool isInitialized;

		// Token: 0x040008B3 RID: 2227
		private bool hasAbilitiesInUse;

		// Token: 0x040008B4 RID: 2228
		private float destructionTime;

		// Token: 0x040008B5 RID: 2229
		private bool killMob;

		// Token: 0x040008B6 RID: 2230
		private bool isKilled;

		// Token: 0x02000493 RID: 1171
		[Serializable]
		public struct AbilityDescription
		{
			// Token: 0x06002455 RID: 9301 RVA: 0x000708FA File Offset: 0x0006EAFA
			internal void SetFactoryArgs(FakeMobBehaviour owner, AbilityFactoryArgs args)
			{
				args.abilityPrototype = this.abilityPrototype;
				args.abilityLevel = this.abilityLevel;
				args.abilityOwner = owner;
			}

			// Token: 0x06002456 RID: 9302 RVA: 0x0007091B File Offset: 0x0006EB1B
			internal bool IsTargetAbility(BaseAbility abilityInstance)
			{
				return this.abilityPrototype != null && abilityInstance.Prototype == this.abilityPrototype;
			}

			// Token: 0x040018C1 RID: 6337
			public BaseAbility abilityPrototype;

			// Token: 0x040018C2 RID: 6338
			public int abilityLevel;

			// Token: 0x040018C3 RID: 6339
			public bool isAuraEffectAbility;

			// Token: 0x040018C4 RID: 6340
			[Space]
			public bool useCooldownOnStart;

			// Token: 0x040018C5 RID: 6341
			public bool useAbilityRepeatedly;

			// Token: 0x040018C6 RID: 6342
			public bool useOnMobDestruction;

			// Token: 0x040018C7 RID: 6343
			public bool forceDestroyOwnerAfterUsing;
		}

		// Token: 0x02000494 RID: 1172
		private struct AbilityTargetsData
		{
			// Token: 0x06002457 RID: 9303 RVA: 0x0007093E File Offset: 0x0006EB3E
			public bool IsTargetSearchRequired()
			{
				return this.targetSearchParams != null;
			}

			// Token: 0x06002458 RID: 9304 RVA: 0x0007094C File Offset: 0x0006EB4C
			public void ResetTargetInfo()
			{
				BaseAbility.UsingArgs usingArgs = this.targetInfo;
				if (usingArgs == null)
				{
					return;
				}
				usingArgs.Reset();
			}

			// Token: 0x040018C8 RID: 6344
			public MobAbilityParameters targetSearchParams;

			// Token: 0x040018C9 RID: 6345
			public BaseAbility.UsingArgs targetInfo;

			// Token: 0x040018CA RID: 6346
			public bool isExpiredAbilityData;
		}
	}
}
