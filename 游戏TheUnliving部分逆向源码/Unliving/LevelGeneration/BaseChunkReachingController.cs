using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Core;
using Game.Gameplay;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000263 RID: 611
	public sealed class BaseChunkReachingController : LocationChunkControllerBase
	{
		// Token: 0x17000450 RID: 1104
		// (get) Token: 0x06001447 RID: 5191 RVA: 0x0003FDF3 File Offset: 0x0003DFF3
		public override bool NeedsEntrancePointsReachingNotifications
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000451 RID: 1105
		// (get) Token: 0x06001448 RID: 5192 RVA: 0x0003FDF6 File Offset: 0x0003DFF6
		public bool HasCompletedChunk
		{
			get
			{
				return this.hasCompletedChunk;
			}
		}

		// Token: 0x17000452 RID: 1106
		// (get) Token: 0x06001449 RID: 5193 RVA: 0x0003FDFE File Offset: 0x0003DFFE
		public int ChunkVisibilityAffectorsCount
		{
			get
			{
				return this.chunkVisibilityAffectorsCount;
			}
		}

		// Token: 0x0600144A RID: 5194 RVA: 0x0003FE08 File Offset: 0x0003E008
		private void CountVisibilityAffector(ILocationChunkVisitor visitor, bool visitorAdded, bool updateVisibility = true)
		{
			if (!base.ControlVisibility || !this.currentChunk.IsLocationExplorer(visitor))
			{
				return;
			}
			if (visitor.ForceGetCurrentChunk().IsDeadEndChunk())
			{
				return;
			}
			BaseGameMob baseGameMob = visitor as BaseGameMob;
			if (baseGameMob != null && baseGameMob.LastLocationChunk.IsDeadEndChunk())
			{
				return;
			}
			this.chunkVisibilityAffectorsCount += (visitorAdded ? 1 : -1);
			if (this.chunkVisibilityAffectorsCount < 0)
			{
				this.chunkVisibilityAffectorsCount = 0;
			}
			if (updateVisibility)
			{
				this.SetChunkVisibilityDirty();
			}
		}

		// Token: 0x0600144B RID: 5195 RVA: 0x0003FE80 File Offset: 0x0003E080
		private async void VisibilityUpdateRoutine()
		{
			if (!GameApplication.IsGameStateChanging)
			{
				await new WaitForEndOfFrame();
				bool flag = this.GetSelfVisibilityState();
				if (!flag)
				{
					for (int i = 0; i < this.neighbourChunkControllers.Count; i++)
					{
						BaseChunkReachingController baseChunkReachingController = this.neighbourChunkControllers[i];
						if (!baseChunkReachingController.IsNull() && baseChunkReachingController.GetSelfVisibilityState())
						{
							flag = true;
							break;
						}
					}
				}
				if (!this.currentChunk.IsNull())
				{
					this.currentChunk.IsVisible = flag;
				}
				this.isVisibilityDirty = false;
			}
		}

		// Token: 0x0600144C RID: 5196 RVA: 0x0003FEB9 File Offset: 0x0003E0B9
		private bool GetSelfVisibilityState()
		{
			return !base.ControlVisibility || this.chunkVisibilityAffectorsCount > 0;
		}

		// Token: 0x0600144D RID: 5197 RVA: 0x0003FECE File Offset: 0x0003E0CE
		protected override void UpdateChunkVisibility(bool force = false)
		{
			this.currentChunk.IsVisible = this.GetSelfVisibilityState();
		}

		// Token: 0x0600144E RID: 5198 RVA: 0x0003FEE1 File Offset: 0x0003E0E1
		private void SetChunkVisibilityDirty()
		{
			if (this.isVisibilityDirty)
			{
				return;
			}
			this.isVisibilityDirty = true;
			this.VisibilityUpdateRoutine();
		}

		// Token: 0x0600144F RID: 5199 RVA: 0x0003FEFC File Offset: 0x0003E0FC
		private void TryReturnStrayedMobs(ILocationChunk lastChunk, ILocationChunk currentChunk)
		{
			if (lastChunk == null)
			{
				return;
			}
			IList<ILocationChunkGateway> gateways = lastChunk.Gateways;
			for (int i = 0; i < gateways.Count; i++)
			{
				ILocationChunkGateway nextChunkGateway = gateways[i].NextChunkGateway;
				ILocationChunk locationChunk = (nextChunkGateway != null) ? nextChunkGateway.CurrentLocationChunk : null;
				if (locationChunk != null && locationChunk != currentChunk)
				{
					BaseChunkReachingController.<TryReturnStrayedMobs>g__ReturnMobs|18_0(locationChunk);
				}
			}
		}

		// Token: 0x06001450 RID: 5200 RVA: 0x0003FF4C File Offset: 0x0003E14C
		public void SetChunkCompleted(ILocationChunk currentChunk)
		{
			if (this.hasCompletedChunk || this.currentChunk == currentChunk)
			{
				return;
			}
			IList<ILocationChunkGateway> gateways = this.currentChunk.Gateways;
			for (int i = 0; i < gateways.Count; i++)
			{
				ILocationChunkGateway locationChunkGateway = gateways[i];
				if (locationChunkGateway.GetNextChunk() != currentChunk)
				{
					if (locationChunkGateway.GateController != null)
					{
						locationChunkGateway.IsLocked = true;
					}
					ILocationChunkGateway nextChunkGateway = locationChunkGateway.NextChunkGateway;
					if (nextChunkGateway != null && nextChunkGateway.GateController != null)
					{
						nextChunkGateway.IsLocked = true;
					}
				}
			}
			this.hasCompletedChunk = true;
		}

		// Token: 0x06001451 RID: 5201 RVA: 0x0003FFC8 File Offset: 0x0003E1C8
		protected override void OnChunkAssigned()
		{
			base.OnChunkAssigned();
			this.chunkVisibilityAffectorsCount = 0;
			IList<ILocationChunkVisitor> visitors = this.currentChunk.Visitors;
			for (int i = 0; i < visitors.Count; i++)
			{
				this.CountVisibilityAffector(visitors[i], true, false);
			}
			this.UpdateChunkVisibility(true);
			IList<ILocationChunkGateway> gateways = this.currentChunk.Gateways;
			for (int j = 0; j < gateways.Count; j++)
			{
				ILocationChunkGateway nextChunkGateway = gateways[j].NextChunkGateway;
				ILocationChunk locationChunk = (nextChunkGateway != null) ? nextChunkGateway.CurrentLocationChunk : null;
				Component component = locationChunk as Component;
				BaseChunkReachingController item;
				if (!(component == null) && component.TryGetComponent<BaseChunkReachingController>(out item))
				{
					locationChunk.VisitorAdded += this.OnNeighbourChunkVisitorAdded;
					locationChunk.VisitorRemoved += this.OnNeighbourChunkVisitorRemoved;
					this.neighbourChunkControllers.Add(item);
				}
			}
		}

		// Token: 0x06001452 RID: 5202 RVA: 0x0004009D File Offset: 0x0003E29D
		private void OnNeighbourChunkVisitorAdded(ILocationChunk neighbourChunk, ILocationChunkVisitor visitor)
		{
			this.SetChunkVisibilityDirty();
		}

		// Token: 0x06001453 RID: 5203 RVA: 0x000400A5 File Offset: 0x0003E2A5
		private void OnNeighbourChunkVisitorRemoved(ILocationChunk neighbourChunk, ILocationChunkVisitor visitor)
		{
			this.SetChunkVisibilityDirty();
		}

		// Token: 0x06001454 RID: 5204 RVA: 0x000400B0 File Offset: 0x0003E2B0
		protected override void OnVisitorEntered(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.CountVisibilityAffector(visitor, true, true);
			if (this.currentChunk.IsLocationExplorer(visitor))
			{
				BaseGameMob baseGameMob = visitor as BaseGameMob;
				if (baseGameMob != null)
				{
					if (this.returnPlayerGroupFromLastChunk)
					{
						PlayerMobsGroupController playerMobsGroupController = baseGameMob.Group as PlayerMobsGroupController;
						if (playerMobsGroupController != null)
						{
							playerMobsGroupController.PlaceAsFormation(visitor.Position, 0);
							playerMobsGroupController.FollowPlayer();
							return;
						}
					}
					if (this.returnStrayedMobs)
					{
						this.TryReturnStrayedMobs(baseGameMob.LastLocationChunk, baseGameMob.CurrentLocationChunk);
					}
				}
			}
		}

		// Token: 0x06001455 RID: 5205 RVA: 0x00040123 File Offset: 0x0003E323
		protected override void OnVisitorExited(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			this.CountVisibilityAffector(visitor, false, true);
		}

		// Token: 0x06001457 RID: 5207 RVA: 0x00040150 File Offset: 0x0003E350
		[CompilerGenerated]
		internal static void <TryReturnStrayedMobs>g__ReturnMobs|18_0(ILocationChunk chunk)
		{
			IList<ILocationChunkVisitor> visitors = chunk.Visitors;
			IGameMob gameMob = chunk.CurrentLocation.LocationExplorer as IGameMob;
			if (gameMob == null)
			{
				return;
			}
			IMovableObject movableObject = gameMob as IMovableObject;
			Vector2 vector = (movableObject != null) ? (-movableObject.CurrentVelocity) : default(Vector3);
			if (vector == default(Vector2))
			{
				vector = new Vector2(-1f, 0f);
			}
			vector.Normalize();
			for (int i = 0; i < visitors.Count; i++)
			{
				BaseGameMob baseGameMob = visitors[i] as BaseGameMob;
				if (baseGameMob != null)
				{
					GameMobsGroupControllerBase group = baseGameMob.Group;
					IGameMob gameMob2 = (group != null) ? group.Leader : null;
					if (gameMob2 == gameMob)
					{
						Vector2 b = QuaternionExtensions.Get2DRotation(UnityEngine.Random.Range(-110f, 110f)) * vector * (gameMob2.Radius + baseGameMob.Radius + 0.3f);
						baseGameMob.Position = gameMob2.Position + b;
					}
				}
			}
		}

		// Token: 0x04000BD4 RID: 3028
		public bool returnPlayerGroupFromLastChunk;

		// Token: 0x04000BD5 RID: 3029
		public bool returnStrayedMobs = true;

		// Token: 0x04000BD6 RID: 3030
		public bool lockCompletedChunksGates = true;

		// Token: 0x04000BD7 RID: 3031
		private readonly List<BaseChunkReachingController> neighbourChunkControllers = new List<BaseChunkReachingController>(4);

		// Token: 0x04000BD8 RID: 3032
		private int chunkVisibilityAffectorsCount;

		// Token: 0x04000BD9 RID: 3033
		private bool hasCompletedChunk;

		// Token: 0x04000BDA RID: 3034
		private bool isVisibilityDirty;
	}
}
