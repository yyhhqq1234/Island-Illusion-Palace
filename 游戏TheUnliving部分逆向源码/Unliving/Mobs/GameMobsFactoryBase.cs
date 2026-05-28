using System;
using System.Runtime.CompilerServices;
using AK.Wwise;
using Common;
using Common.Factories;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.ObjectPool;
using UnityEngine;
using Unliving.Mobs.SFX;
using Unliving.Mobs.VFX;

namespace Unliving.Mobs
{
	// Token: 0x020001BF RID: 447
	public abstract class GameMobsFactoryBase<TMobData> : PrototypeBasedFactory<TMobData, IGameMob>, IInitializable<IGame> where TMobData : class, IUnityObjectDescription
	{
		// Token: 0x06000DAF RID: 3503
		protected abstract GameMobFactionInfo GetMobFactionInfo(BaseGameMob mob, GameMobFactions faction);

		// Token: 0x06000DB0 RID: 3504 RVA: 0x0002B948 File Offset: 0x00029B48
		protected virtual void SetMobParams(TMobData mobData, GameObject mobPrefab, IGameMob mob, GameMobsFactoryArgsBase args)
		{
			GameMobFactions gameMobFactions = args.mobFaction;
			BaseGameMob baseGameMob = mob as BaseGameMob;
			if (baseGameMob != null)
			{
				baseGameMob.MobPrototypeObject = mobPrefab;
				if (gameMobFactions == GameMobFactions.None)
				{
					gameMobFactions = baseGameMob.defaultFaction;
				}
				GameMobFactionInfo mobFactionInfo = this.GetMobFactionInfo(baseGameMob, gameMobFactions);
				if (mobFactionInfo.IsValid())
				{
					baseGameMob.SetLayer(mobFactionInfo.mobsLayer);
					baseGameMob.defaultFaction = mobFactionInfo.faction;
					baseGameMob.enemyMobLayers = mobFactionInfo.enemyMobLayers;
				}
				if (!baseGameMob.isEnvironmentMob)
				{
					baseGameMob.isEnvironmentMob = args.isEnvironmentMob;
				}
				if (this.disableAbilityResourcesGeneration)
				{
					baseGameMob.resourcesGeneratorData = null;
				}
				else if (this.globalResourcesGeneratorData != null && this.globalResourcesGeneratorData.HasValues() && (baseGameMob.overrideResourcesGeneratorDataByFactory || baseGameMob.resourcesGeneratorData == null))
				{
					baseGameMob.resourcesGeneratorData = this.globalResourcesGeneratorData;
				}
				GameMobVFXController gameMobVFXController;
				if (baseGameMob.TryGetComponent<GameMobVFXController>(out gameMobVFXController) && this.useAttachableMobsEffectsPool && (this.attachableMobEffectsPool != null || this.managersRegistry.TryGet<IUnityObjectPool<ParticleSystem>>(out this.attachableMobEffectsPool)))
				{
					gameMobVFXController.AttachableEffectsPool = this.attachableMobEffectsPool;
				}
				if (gameMobFactions == GameMobFactions.PLAYER)
				{
					if (this.playerMobsDecayParameters != null && this.playerMobsDecayParameters.IsValid())
					{
						MobHealthController mobHealthController = baseGameMob.HitPointsController as MobHealthController;
						if (mobHealthController != null)
						{
							MobHealthController.DecayControllerParams decayControllerParams = mobHealthController.decayControllerParams;
							if (decayControllerParams == null || !decayControllerParams.IsValid())
							{
								mobHealthController.decayControllerParams = this.playerMobsDecayParameters;
							}
						}
					}
					if (gameMobVFXController != null)
					{
						if (this.playerMobsResourcesCollectionEffects != null && this.playerMobsResourcesCollectionEffects.Length != 0 && gameMobVFXController.ResourcesCollectionEffects.Length == 0)
						{
							GameMobVFXController.ResourcesCollectionEffect[] array = new GameMobVFXController.ResourcesCollectionEffect[this.playerMobsResourcesCollectionEffects.Length];
							this.playerMobsResourcesCollectionEffects.CopyTo(array, 0);
							gameMobVFXController.ResourcesCollectionEffects = array;
						}
						if (this.playerMobsSacrificationIndicationEffect.IsActive)
						{
							gameMobVFXController.sacrificationIndicationEffect = this.playerMobsSacrificationIndicationEffect;
						}
						if (this.playerMobsDeathIndicationEffect.IsActive)
						{
							gameMobVFXController.deathIndicationEffect = this.playerMobsDeathIndicationEffect;
						}
						if (this.playerMobsArmyLimitDeathIndicationEffect.IsActive)
						{
							gameMobVFXController.armyLimitDeathIndicationEffect = this.playerMobsArmyLimitDeathIndicationEffect;
						}
					}
					IGameMobWwiseSoundController gameMobWwiseSoundController;
					if (baseGameMob.TryGetComponent<IGameMobWwiseSoundController>(out gameMobWwiseSoundController))
					{
						if (GameMobsFactoryBase<TMobData>.<SetMobParams>g__IsEmptySFXEvent|13_0(gameMobWwiseSoundController.SacrificationIndicationEvent))
						{
							gameMobWwiseSoundController.SacrificationIndicationEvent = this.playerMobsSacrificationIndicationEvent;
						}
						if (GameMobsFactoryBase<TMobData>.<SetMobParams>g__IsEmptySFXEvent|13_0(gameMobWwiseSoundController.DeathIndicationEvent))
						{
							gameMobWwiseSoundController.DeathIndicationEvent = this.playerMobsDeathIndicationEvent;
						}
					}
				}
			}
			mob.Position = args.spawnPosition;
			if (mob.NavMeshAgent != null)
			{
				mob.NavMeshAgent.Warp(args.spawnPosition);
				mob.NavMeshAgent.nextPosition = args.spawnPosition;
				mob.NavMeshAgent.enabled = false;
			}
		}

		// Token: 0x06000DB1 RID: 3505 RVA: 0x0002BBC0 File Offset: 0x00029DC0
		protected override IGameMob Create(TMobData mobData, IBaseObjectDescription args)
		{
			GameMobsFactoryArgsBase gameMobsFactoryArgsBase = (GameMobsFactoryArgsBase)args;
			GameObject gameObject = gameMobsFactoryArgsBase.arbitraryMobPrefab;
			if (gameObject == null)
			{
				TMobData tmobData = mobData;
				gameObject = (GameObject)((tmobData != null) ? tmobData.UnityObjectPrototype : null);
			}
			IGameMob gameMob = null;
			if (gameObject != null)
			{
				gameMob = UnityEngine.Object.Instantiate<GameObject>(gameObject).GetComponentOrDestroy<IGameMob>();
				if (gameMob != null)
				{
					this.SetMobParams(mobData, gameObject, gameMob, gameMobsFactoryArgsBase);
					PrototypeBasedFactory<TMobData, IGameMob>.IInitializableByFactory initializableByFactory = gameMob as PrototypeBasedFactory<TMobData, IGameMob>.IInitializableByFactory;
					if (initializableByFactory != null)
					{
						initializableByFactory.OnCreatedByFactory(this, args, mobData);
					}
				}
			}
			return gameMob;
		}

		// Token: 0x06000DB2 RID: 3506 RVA: 0x0002BC35 File Offset: 0x00029E35
		public GameMobsFactoryBase(BaseGameMob.ResourcesGeneratorData globalResourcesGeneratorData)
		{
			this.globalResourcesGeneratorData = globalResourcesGeneratorData;
		}

		// Token: 0x06000DB3 RID: 3507 RVA: 0x0002BC44 File Offset: 0x00029E44
		public virtual void Initialize(IGame game)
		{
			this.managersRegistry = game.Services;
		}

		// Token: 0x06000DB4 RID: 3508 RVA: 0x0002BC52 File Offset: 0x00029E52
		[CompilerGenerated]
		internal static bool <SetMobParams>g__IsEmptySFXEvent|13_0(AK.Wwise.Event wwiseEvent)
		{
			return wwiseEvent == null || !wwiseEvent.IsValid();
		}

		// Token: 0x040007FA RID: 2042
		public MobHealthController.DecayControllerParams playerMobsDecayParameters;

		// Token: 0x040007FB RID: 2043
		public bool disableAbilityResourcesGeneration;

		// Token: 0x040007FC RID: 2044
		public GameMobVFXController.ResourcesCollectionEffect[] playerMobsResourcesCollectionEffects;

		// Token: 0x040007FD RID: 2045
		public AttachableVisualEffectSpawner playerMobsSacrificationIndicationEffect;

		// Token: 0x040007FE RID: 2046
		public AttachableVisualEffectSpawner playerMobsDeathIndicationEffect;

		// Token: 0x040007FF RID: 2047
		public AttachableVisualEffectSpawner playerMobsArmyLimitDeathIndicationEffect;

		// Token: 0x04000800 RID: 2048
		public bool useAttachableMobsEffectsPool;

		// Token: 0x04000801 RID: 2049
		public AK.Wwise.Event playerMobsSacrificationIndicationEvent;

		// Token: 0x04000802 RID: 2050
		public AK.Wwise.Event playerMobsDeathIndicationEvent;

		// Token: 0x04000803 RID: 2051
		private readonly BaseGameMob.ResourcesGeneratorData globalResourcesGeneratorData;

		// Token: 0x04000804 RID: 2052
		private IServiceRegistry managersRegistry;

		// Token: 0x04000805 RID: 2053
		private IUnityObjectPool<ParticleSystem> attachableMobEffectsPool;
	}
}
