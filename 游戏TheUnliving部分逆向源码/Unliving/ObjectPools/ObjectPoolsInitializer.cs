using System;
using Game.Core;
using Game.Damage.Projectiles;
using Game.GameLoop;
using Game.ObjectPool;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.AbilityResources;

namespace Unliving.ObjectPools
{
	// Token: 0x020001AC RID: 428
	[CreateAssetMenu(fileName = "ObjectPoolsInitializer", menuName = "Game/Object Pools Initializer")]
	public sealed class ObjectPoolsInitializer : GlobalManagerBase
	{
		// Token: 0x06000C2F RID: 3119 RVA: 0x000262F2 File Offset: 0x000244F2
		private void UpdatePools()
		{
			if (GameApplication.IsGameStateChanging)
			{
				return;
			}
			ParticleSystemEffectsPool particleSystemEffectsPool = this.particleSystemEffectsPool;
			if (particleSystemEffectsPool != null)
			{
				particleSystemEffectsPool.Update();
			}
			ProjectileRenderersPool projectileRenderersPool = this.projectileRenderersPool;
			if (projectileRenderersPool != null)
			{
				projectileRenderersPool.Update();
			}
			CollectableAbilityResourcesPool collectableAbilityResourcesPool = this.collectableResourcesPool;
			if (collectableAbilityResourcesPool == null)
			{
				return;
			}
			collectableAbilityResourcesPool.Update();
		}

		// Token: 0x06000C30 RID: 3120 RVA: 0x00026330 File Offset: 0x00024530
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (this.particleSystemEffectsPoolParams.isPoolActive)
			{
				this.particleSystemEffectsPool = new ParticleSystemEffectsPool();
				currentGame.Services.Register(typeof(IUnityObjectPool<ParticleSystem>), this.particleSystemEffectsPool);
			}
			if (this.projectileRenderersPoolParams.isPoolActive)
			{
				this.projectileRenderersPool = new ProjectileRenderersPool();
				currentGame.Services.Register(typeof(IUnityObjectPool<ProjectileRendererComponent>), this.projectileRenderersPool);
			}
			if (this.collectableResourcesPoolParams.isPoolActive)
			{
				this.collectableResourcesPool = new CollectableAbilityResourcesPool();
				currentGame.Services.Register(typeof(IUnityObjectPool<CollectableAbilityResource>), this.collectableResourcesPool);
			}
			IGameLoopAccessProvider gameLoopAccessProvider;
			if (currentGame.Services.TryGet<IGameLoopAccessProvider>(out gameLoopAccessProvider))
			{
				gameLoopAccessProvider.UpdatePerformed += this.UpdatePools;
			}
		}

		// Token: 0x06000C31 RID: 3121 RVA: 0x00026400 File Offset: 0x00024600
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			ParticleSystemEffectsPool particleSystemEffectsPool = this.particleSystemEffectsPool;
			if (particleSystemEffectsPool != null)
			{
				particleSystemEffectsPool.Initialize(this.particleSystemEffectsPoolParams.localObjectPoolsData);
			}
			ProjectileRenderersPool projectileRenderersPool = this.projectileRenderersPool;
			if (projectileRenderersPool != null)
			{
				projectileRenderersPool.Initialize(this.projectileRenderersPoolParams.localObjectPoolsData);
			}
			CollectableAbilityResourcesPool collectableAbilityResourcesPool = this.collectableResourcesPool;
			if (collectableAbilityResourcesPool == null)
			{
				return;
			}
			collectableAbilityResourcesPool.Initialize(this.collectableResourcesPoolParams.localObjectPoolsData);
		}

		// Token: 0x040006F8 RID: 1784
		public ObjectPoolInitializationParams particleSystemEffectsPoolParams;

		// Token: 0x040006F9 RID: 1785
		public ObjectPoolInitializationParams projectileRenderersPoolParams;

		// Token: 0x040006FA RID: 1786
		public ObjectPoolInitializationParams collectableResourcesPoolParams;

		// Token: 0x040006FB RID: 1787
		private ParticleSystemEffectsPool particleSystemEffectsPool;

		// Token: 0x040006FC RID: 1788
		private ProjectileRenderersPool projectileRenderersPool;

		// Token: 0x040006FD RID: 1789
		private CollectableAbilityResourcesPool collectableResourcesPool;
	}
}
