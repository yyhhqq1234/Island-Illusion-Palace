using System;
using Common.CollectionsExtensions;
using Game.Core;
using Game.GameLoop;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035A RID: 858
	[CreateAssetMenu(fileName = "AbilityResourcesLifetimeManager", menuName = "Game/Ability Resources Lifetime Manager")]
	public sealed class AbilityResourcesLifetimeManager : GlobalManagerBase
	{
		// Token: 0x06001BF5 RID: 7157 RVA: 0x000583A7 File Offset: 0x000565A7
		private void RemoveResource(int resourceIndex)
		{
			this.registeredResources.RemoveBySwap(resourceIndex, ref this.registeredResourcesCount);
		}

		// Token: 0x06001BF6 RID: 7158 RVA: 0x000583BC File Offset: 0x000565BC
		private void UpdateResources()
		{
			if (this.registeredResourcesCount == 0 || GameApplication.IsGameStateChanging)
			{
				return;
			}
			PlayerBehaviour playerBehaviour = this.currentPlayer;
			ILocationChunk locationChunk = (playerBehaviour != null) ? playerBehaviour.CurrentLocationChunk : null;
			int num = (locationChunk != null) ? locationChunk.ChunkID : -1;
			float num2 = -Time.deltaTime;
			int i = 0;
			while (i < this.registeredResourcesCount)
			{
				ref AbilityResourcesLifetimeManager.ResourceInfo ptr = ref this.registeredResources[i];
				int resourceChunkID = ptr.ResourceChunkID;
				if (resourceChunkID != num || num < 0 || resourceChunkID < 0)
				{
					ptr.remainingLifetime += num2;
					if (ptr.remainingLifetime < 0f)
					{
						if (ptr.Resource != null)
						{
							ptr.Resource.Destroy();
						}
						this.RemoveResource(i);
						continue;
					}
				}
				CollectableAbilityResource resource = ptr.Resource;
				if (resource.IsDestroyed || resource.CurrentCollector != null)
				{
					this.RemoveResource(i);
				}
				else
				{
					i++;
				}
			}
		}

		// Token: 0x06001BF7 RID: 7159 RVA: 0x00058498 File Offset: 0x00056698
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			ICollectableAbilityResourcesFactory collectableAbilityResourcesFactory;
			if (currentGame.Services.TryGet<ICollectableAbilityResourcesFactory>(out collectableAbilityResourcesFactory))
			{
				collectableAbilityResourcesFactory.ResourceCreated += this.OnResourceCreated;
			}
			IGameLoopAccessProvider gameLoopAccessProvider;
			if (currentGame.Services.TryGet<IGameLoopAccessProvider>(out gameLoopAccessProvider))
			{
				gameLoopAccessProvider.UpdatePerformed += this.UpdateResources;
			}
			this.currentGame = currentGame;
		}

		// Token: 0x06001BF8 RID: 7160 RVA: 0x000584F8 File Offset: 0x000566F8
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			for (int i = 0; i < this.registeredResourcesCount; i++)
			{
				this.registeredResources[i] = default(AbilityResourcesLifetimeManager.ResourceInfo);
			}
			this.registeredResourcesCount = 0;
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x00058530 File Offset: 0x00056730
		private void OnResourceCreated(CollectableAbilityResourcesFactoryArgs args, CollectableAbilityResource resource)
		{
			if (this.resourcesLifetime < 0f)
			{
				return;
			}
			if (this.currentPlayer == null)
			{
				this.currentGame.TryGetPlayer(out this.currentPlayer);
			}
			if (this.ignorableResources == null || Array.IndexOf<AbilityResourceType>(this.ignorableResources, args.resourceType) == -1)
			{
				if (resource.type == AbilityResourceType.Corpse && resource.GetComponent<BaseGameMob>().IsSpawnedAsDeadMob())
				{
					return;
				}
				Common.CollectionsExtensions.Extensions.Add<AbilityResourcesLifetimeManager.ResourceInfo>(new AbilityResourcesLifetimeManager.ResourceInfo(resource, this.resourcesLifetime), ref this.registeredResources, ref this.registeredResourcesCount, 64);
			}
		}

		// Token: 0x04000FD0 RID: 4048
		public float resourcesLifetime = 60f;

		// Token: 0x04000FD1 RID: 4049
		public AbilityResourceType[] ignorableResources;

		// Token: 0x04000FD2 RID: 4050
		private IGame currentGame;

		// Token: 0x04000FD3 RID: 4051
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000FD4 RID: 4052
		private AbilityResourcesLifetimeManager.ResourceInfo[] registeredResources;

		// Token: 0x04000FD5 RID: 4053
		private int registeredResourcesCount;

		// Token: 0x0200055D RID: 1373
		private struct ResourceInfo
		{
			// Token: 0x060026F4 RID: 9972 RVA: 0x00079238 File Offset: 0x00077438
			public ResourceInfo(CollectableAbilityResource resource, float lifetime)
			{
				this.Resource = resource;
				ILocationChunkVisitor locationChunkVisitor = resource.Owner as ILocationChunkVisitor;
				int? num;
				if (locationChunkVisitor == null)
				{
					num = null;
				}
				else
				{
					ILocationChunk currentLocationChunk = locationChunkVisitor.CurrentLocationChunk;
					num = ((currentLocationChunk != null) ? new int?(currentLocationChunk.ChunkID) : null);
				}
				this.ResourceChunkID = (num ?? -1);
				this.remainingLifetime = lifetime;
			}

			// Token: 0x04001BEC RID: 7148
			public readonly CollectableAbilityResource Resource;

			// Token: 0x04001BED RID: 7149
			public readonly int ResourceChunkID;

			// Token: 0x04001BEE RID: 7150
			public float remainingLifetime;
		}
	}
}
