using System;
using System.Collections.Generic;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.GameScene;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026E RID: 622
	public sealed class LocationChunksVisibilityController : GameBehaviourBase
	{
		// Token: 0x0600155D RID: 5469 RVA: 0x000443C8 File Offset: 0x000425C8
		private static void RemoveUnusedResources(ILocationChunk chunk)
		{
			if (!chunk.IsExplored)
			{
				return;
			}
			IList<ILocationObject> environmentObjects = chunk.EnvironmentObjects;
			for (int i = environmentObjects.Count - 1; i >= 0; i--)
			{
				CollectableAbilityResource collectableAbilityResource = environmentObjects[i] as CollectableAbilityResource;
				if (collectableAbilityResource != null && collectableAbilityResource.gameObject.activeSelf && (collectableAbilityResource.type != AbilityResourceType.Corpse || !collectableAbilityResource.GetComponent<BaseGameMob>().IsSpawnedAsDeadMob()))
				{
					collectableAbilityResource.Destroy();
				}
			}
		}

		// Token: 0x0600155E RID: 5470 RVA: 0x00044434 File Offset: 0x00042634
		private static void ReturnStrayedMobs(PlayerBehaviour player, ILocationChunk chunk)
		{
			IList<ILocationChunkVisitor> visitors = chunk.Visitors;
			int count = visitors.Count;
			float radius = player.Radius;
			GameMobsGroupControllerBase group = player.Group;
			Vector2 position = player.Position;
			Vector2 vector = -player.CurrentVelocity;
			if (vector == default(Vector2))
			{
				vector = new Vector2(-1f, 0f);
			}
			for (int i = 0; i < count; i++)
			{
				BaseGameMob baseGameMob = visitors[i] as BaseGameMob;
				if (baseGameMob != null && !baseGameMob.isEnvironmentMob && !baseGameMob.IsSacrificed && baseGameMob.Group == group && baseGameMob.MotionController != null)
				{
					Vector2 b = QuaternionExtensions.Get2DRotation(UnityEngine.Random.Range(-110f, 110f)) * ((radius + baseGameMob.Radius + 0.3f) * vector);
					baseGameMob.Position = position + b;
				}
			}
		}

		// Token: 0x0600155F RID: 5471 RVA: 0x00044534 File Offset: 0x00042734
		private void UpdateChunksVisibility(ILocationChunk newPlayerChunk)
		{
			if (this.currentChunks.Count != 0 && this.currentChunks[0] == newPlayerChunk)
			{
				return;
			}
			IList<ILocationChunkGateway> gateways = newPlayerChunk.Gateways;
			if (gateways.Count == 1 && this.currentChunks.IndexOf(newPlayerChunk) >= 0)
			{
				return;
			}
			newPlayerChunk.IsVisible = true;
			this.newChunks.Add(newPlayerChunk);
			for (int i = 0; i < gateways.Count; i++)
			{
				ILocationChunk nextChunk = gateways[i].GetNextChunk();
				if (nextChunk != null)
				{
					nextChunk.IsVisible = true;
					this.newChunks.Add(nextChunk);
				}
			}
			for (int j = 0; j < this.currentChunks.Count; j++)
			{
				ILocationChunk locationChunk = this.currentChunks[j];
				if (this.newChunks.IndexOf(locationChunk) < 0)
				{
					if (this.returnStrayedMobs)
					{
						LocationChunksVisibilityController.ReturnStrayedMobs(this.player, locationChunk);
					}
					LocationChunksVisibilityController.RemoveUnusedResources(locationChunk);
					locationChunk.IsVisible = false;
				}
			}
			this.currentChunks.Clear();
			this.currentChunks.AddRange(this.newChunks);
			this.newChunks.Clear();
		}

		// Token: 0x06001560 RID: 5472 RVA: 0x00044644 File Offset: 0x00042844
		private void OnLocationGenerated(GameSceneManager sceneGenerator)
		{
			GameLocation generatedLocation = sceneGenerator.GeneratedLocation;
			IReadOnlyList<ILocationChunk> chunks = generatedLocation.Chunks;
			if (this.keepAllChunksVisible)
			{
				for (int i = 0; i < chunks.Count; i++)
				{
					chunks[i].IsVisible = true;
				}
				this.currentChunks.AddRange(chunks);
				return;
			}
			ILocationChunk locationChunkAtPoint = generatedLocation.GetLocationChunkAtPoint(this.player.Position, false);
			for (int j = 0; j < chunks.Count; j++)
			{
				chunks[j].IsVisible = false;
			}
			this.UpdateChunksVisibility(locationChunkAtPoint);
		}

		// Token: 0x06001561 RID: 5473 RVA: 0x000446CF File Offset: 0x000428CF
		private void OnLocationChunkEntered(ILocationChunk lastPlayerChunk, ILocationChunk currentPlayerChunk)
		{
			this.UpdateChunksVisibility(currentPlayerChunk);
		}

		// Token: 0x06001562 RID: 5474 RVA: 0x000446D8 File Offset: 0x000428D8
		private void OnPlayerKilled(IGameMob killedPlayer)
		{
			this.player.LocationChunkEntered -= this.OnLocationChunkEntered;
			this.player.Killed -= this.OnPlayerKilled;
			bool isGameStateChanging = GameApplication.IsGameStateChanging;
		}

		// Token: 0x06001563 RID: 5475 RVA: 0x00044710 File Offset: 0x00042910
		private async void Start()
		{
			IPlayerProvider playerProvider = await base.CurrentGame.Services.GetServiceAsync<IPlayerProvider>();
			if (((playerProvider != null) ? playerProvider.CurrentPlayer : null) != null)
			{
				this.player = playerProvider.CurrentPlayer;
				GameSceneManager gameSceneManager;
				if (base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
				{
					gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
					if (!this.keepAllChunksVisible)
					{
						this.player.LocationChunkEntered += this.OnLocationChunkEntered;
					}
				}
				this.player.Killed += this.OnPlayerKilled;
			}
		}

		// Token: 0x04000C5C RID: 3164
		public bool returnStrayedMobs = true;

		// Token: 0x04000C5D RID: 3165
		public bool keepAllChunksVisible;

		// Token: 0x04000C5E RID: 3166
		private readonly List<ILocationChunk> newChunks = new List<ILocationChunk>(4);

		// Token: 0x04000C5F RID: 3167
		private readonly List<ILocationChunk> currentChunks = new List<ILocationChunk>(4);

		// Token: 0x04000C60 RID: 3168
		private PlayerBehaviour player;
	}
}
