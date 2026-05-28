using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.CollectionsExtensions;
using Common.Math;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Gameplay;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000265 RID: 613
	public sealed class ConnectingChunkController : LocationChunkControllerBase
	{
		// Token: 0x0600145D RID: 5213 RVA: 0x00040327 File Offset: 0x0003E527
		private static EnemyLocationChunkClearingTrigger GetChunkClearingTrigger(ILocationChunk chunk)
		{
			Component component = chunk as Component;
			if (component == null)
			{
				return null;
			}
			return component.GetComponent<EnemyLocationChunkClearingTrigger>();
		}

		// Token: 0x0600145E RID: 5214 RVA: 0x0004033A File Offset: 0x0003E53A
		public static float ClampTransitionThreshold(float newValue)
		{
			return Mathf.Clamp(newValue, 0.1f, 0.5f);
		}

		// Token: 0x17000453 RID: 1107
		// (get) Token: 0x0600145F RID: 5215 RVA: 0x0004034C File Offset: 0x0003E54C
		// (set) Token: 0x06001460 RID: 5216 RVA: 0x00040354 File Offset: 0x0003E554
		public float TransitionThreshold0
		{
			get
			{
				return this._transitionThreshold0;
			}
			set
			{
				this._transitionThreshold0 = ConnectingChunkController.ClampTransitionThreshold(value);
			}
		}

		// Token: 0x17000454 RID: 1108
		// (get) Token: 0x06001461 RID: 5217 RVA: 0x00040362 File Offset: 0x0003E562
		// (set) Token: 0x06001462 RID: 5218 RVA: 0x0004036A File Offset: 0x0003E56A
		public float TransitionThreshold1
		{
			get
			{
				return this._transitionThreshold1;
			}
			set
			{
				this._transitionThreshold1 = ConnectingChunkController.ClampTransitionThreshold(value);
			}
		}

		// Token: 0x17000455 RID: 1109
		// (get) Token: 0x06001463 RID: 5219 RVA: 0x00040378 File Offset: 0x0003E578
		public override bool NeedsEntrancePointsReachingNotifications
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001464 RID: 5220 RVA: 0x0004037B File Offset: 0x0003E57B
		private bool VisibilityModificationRestrictionFilter(ILocationChunk targetChunk)
		{
			return targetChunk.VisitorsWatcherHasVisitor(this.currentChunkExplorer);
		}

		// Token: 0x06001465 RID: 5221 RVA: 0x00040389 File Offset: 0x0003E589
		private bool IsMobChunkVisitor(ILocationChunkVisitor chunkVisitor)
		{
			return !chunkVisitor.IsLocationObject(LocationObjectType.MinorMob) && !(chunkVisitor is StaticGameMob);
		}

		// Token: 0x06001466 RID: 5222 RVA: 0x000403A3 File Offset: 0x0003E5A3
		private void ControlChunkVisibility(ILocationChunk chunk, bool isVisible)
		{
			if (isVisible && !chunk.IsInitialized)
			{
				chunk.SetChunkObjectActive(true);
			}
			chunk.SetVisibleWithConnectedChunks(isVisible, new Predicate<ILocationChunk>(this.VisibilityModificationRestrictionFilter));
		}

		// Token: 0x06001467 RID: 5223 RVA: 0x000403CC File Offset: 0x0003E5CC
		private void PrepareMobsTransfer()
		{
			BaseGameMob baseGameMob = this.currentChunkExplorer as BaseGameMob;
			if (baseGameMob == null || baseGameMob.Group == null || !baseGameMob.Group.HasMobs)
			{
				return;
			}
			this.mobsTransferer = new ConnectingChunkController.GroupMobsTransferer(this, ((GameMobGroupController)baseGameMob.Group).MinGroupRadius * 2f);
			baseGameMob.Group.CopyMobsTo(ConnectingChunkController.TransferingMobsBuffer, -1, new Predicate<BaseGameMob>(this.<PrepareMobsTransfer>g__IsTransferableMob|31_0));
			if (ConnectingChunkController.TransferingMobsBuffer.Count != 0)
			{
				PlayerMobsGroupController playerMobsGroupController = baseGameMob.Group as PlayerMobsGroupController;
				if (playerMobsGroupController != null)
				{
					playerMobsGroupController.FollowPlayer();
				}
			}
		}

		// Token: 0x06001468 RID: 5224 RVA: 0x00040460 File Offset: 0x0003E660
		private void TransferExplorerGroup()
		{
			if (ConnectingChunkController.TransferingMobsBuffer.Count == 0)
			{
				return;
			}
			for (int i = 0; i < ConnectingChunkController.TransferingMobsBuffer.Count; i++)
			{
				if (this.mobsTransferer.TransferMob(ConnectingChunkController.TransferingMobsBuffer[i]))
				{
					ConnectingChunkController.TransferingMobsBuffer.RemoveBySwap(i);
				}
			}
		}

		// Token: 0x06001469 RID: 5225 RVA: 0x000404B4 File Offset: 0x0003E6B4
		private bool TrySetGatewayLocked(ILocationChunkGateway gatewayToLock, ILocationChunkGateway otherGateway)
		{
			ILocationChunk locationChunk = otherGateway.GetNextChunk();
			EnemyLocationChunkClearingTrigger chunkClearingTrigger = ConnectingChunkController.GetChunkClearingTrigger(locationChunk);
			if (chunkClearingTrigger != null && chunkClearingTrigger.ForceGetEnemyMobsCount(locationChunk) > 0)
			{
				gatewayToLock.CanBeLocked = true;
				gatewayToLock.IsLocked = true;
				otherGateway.CanBeLocked = false;
				return true;
			}
			return false;
		}

		// Token: 0x0600146A RID: 5226 RVA: 0x000404FC File Offset: 0x0003E6FC
		private void TryLockThisChunk()
		{
			if (!this.TrySetGatewayLocked(this.entranceGateway, this.exitGateway) && !this.TrySetGatewayLocked(this.exitGateway, this.entranceGateway))
			{
				this.entranceGateway.IsLocked = false;
				this.exitGateway.IsLocked = false;
			}
		}

		// Token: 0x0600146B RID: 5227 RVA: 0x0004054C File Offset: 0x0003E74C
		private async void TrySetMainChunkVisible(ILocationChunk mainChunk)
		{
			mainChunk.IsVisible = false;
			mainChunk.VisitorAdded += this.<TrySetMainChunkVisible>g__UpdateVisibility|35_0;
			for (int i = 0; i < 5; i++)
			{
				await Task.Yield();
			}
			if (mainChunk != null)
			{
				mainChunk.VisitorAdded -= this.<TrySetMainChunkVisible>g__UpdateVisibility|35_0;
			}
		}

		// Token: 0x0600146C RID: 5228 RVA: 0x00040590 File Offset: 0x0003E790
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (this.currentChunk != null)
			{
				if (base.ControlVisibility)
				{
					this.currentChunk.IsVisible = false;
				}
				foreach (ILocationChunkGateway gateway in this.currentChunk.Gateways)
				{
					ILocationChunk locationChunk = gateway.GetNextChunk();
					if (locationChunk != null)
					{
						if (base.ControlVisibility)
						{
							this.TrySetMainChunkVisible(locationChunk);
						}
						locationChunk.VisitorRemoved += this.OnMainChunkVisitorExited;
					}
				}
			}
		}

		// Token: 0x0600146D RID: 5229 RVA: 0x00040628 File Offset: 0x0003E828
		private void OnMainChunkVisitorExited(ILocationChunk exitedChunk, ILocationChunkVisitor visitor)
		{
			if (this.currentChunkExplorer == null && this.currentChunk.IsLocationExplorer(visitor))
			{
				IList<ILocationChunkGateway> gateways = this.currentChunk.Gateways;
				ILocationChunk locationChunk = visitor.ForceGetCurrentChunk();
				if (locationChunk == this.currentChunk)
				{
					this.lastChunk = exitedChunk;
					this.entranceGateway = null;
					this.exitGateway = null;
					for (int i = 0; i < gateways.Count; i++)
					{
						ILocationChunkGateway gateway = gateways[i];
						if (gateway.GetNextChunk() == exitedChunk)
						{
							this.entranceGateway = gateway;
						}
						else
						{
							this.exitGateway = gateway;
						}
					}
					this.nextChunk = this.exitGateway.GetNextChunk();
					Vector2 position = this.entranceGateway.Position;
					this.chunkEnteringDirection = this.exitGateway.Position - position;
					this.transitionPoint0 = new ConnectingChunkController.TransitionPoint(position + this.chunkEnteringDirection * this._transitionThreshold0, new Action<bool>(this.OnTransitionPoint0StateChanged));
					this.transitionPoint1 = new ConnectingChunkController.TransitionPoint(position + this.chunkEnteringDirection * (1f - this._transitionThreshold1), new Action<bool>(this.OnTransitionPoint1StateChanged));
					this.chunkEnteringDirection.Normalize();
					this.currentChunkExplorer = visitor;
					return;
				}
				if (!locationChunk.IsDeadEndChunk())
				{
					for (int j = 0; j < gateways.Count; j++)
					{
						gateways[j].CanBeLocked = true;
					}
				}
			}
		}

		// Token: 0x0600146E RID: 5230 RVA: 0x0004078F File Offset: 0x0003E98F
		private void OnTransitionPoint0StateChanged(bool isReached)
		{
			if (base.ControlVisibility)
			{
				this.ControlChunkVisibility(this.nextChunk, isReached);
			}
		}

		// Token: 0x0600146F RID: 5231 RVA: 0x000407A6 File Offset: 0x0003E9A6
		private void OnTransitionPoint1StateChanged(bool isReached)
		{
			if (isReached && !this.isConnectingChunkFullyReached)
			{
				this.TryLockThisChunk();
				this.PrepareMobsTransfer();
				this.isConnectingChunkFullyReached = true;
			}
			if (base.ControlVisibility)
			{
				this.ControlChunkVisibility(this.lastChunk, !isReached);
			}
		}

		// Token: 0x06001470 RID: 5232 RVA: 0x000407DE File Offset: 0x0003E9DE
		protected override void OnVisitorEntered(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (this.IsMobChunkVisitor(visitor))
			{
				this.mobsInConnectingChunkCount++;
			}
		}

		// Token: 0x06001471 RID: 5233 RVA: 0x000407F8 File Offset: 0x0003E9F8
		protected override void OnVisitorExited(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (!this.IsMobChunkVisitor(visitor))
			{
				return;
			}
			this.mobsInConnectingChunkCount--;
			if (this.currentChunkExplorer == visitor)
			{
				this.currentChunkExplorer = null;
				this.isConnectingChunkFullyReached = false;
			}
			if (this.entranceGateway != null && this.mobsInConnectingChunkCount == 0)
			{
				ILocationChunkGateway locationChunkGateway = this.entranceGateway.IsLocked ? this.entranceGateway : this.exitGateway;
				ILocationChunkGateway locationChunkGateway2 = (this.entranceGateway == locationChunkGateway) ? this.exitGateway : this.entranceGateway;
				EnemyLocationChunkClearingTrigger chunkClearingTrigger = ConnectingChunkController.GetChunkClearingTrigger(locationChunkGateway2.GetNextChunk());
				locationChunkGateway.IsLocked = false;
				if (chunkClearingTrigger != null && !chunkClearingTrigger.IsChunkCleared)
				{
					locationChunkGateway2.CanBeLocked = true;
					locationChunkGateway2.IsLocked = true;
				}
			}
		}

		// Token: 0x06001472 RID: 5234 RVA: 0x000408AC File Offset: 0x0003EAAC
		private void LateUpdate()
		{
			if (this.currentChunkExplorer != null)
			{
				Vector2 position = this.currentChunkExplorer.Position;
				this.transitionPoint0.Update(position, this.chunkEnteringDirection);
				this.transitionPoint1.Update(position, this.chunkEnteringDirection);
				if (this.isConnectingChunkFullyReached)
				{
					this.TransferExplorerGroup();
				}
			}
		}

		// Token: 0x06001473 RID: 5235 RVA: 0x00040900 File Offset: 0x0003EB00
		private void OnDrawGizmos()
		{
			if (this.currentChunkExplorer != null)
			{
				ConnectingChunkController.<OnDrawGizmos>g__SetColor|43_0(this.transitionPoint0.IsReached);
				Gizmos.DrawWireSphere(this.transitionPoint0.Position, 0.5f);
				Gizmos.DrawLine(this.currentChunkExplorer.Position, this.transitionPoint0.Position);
				ConnectingChunkController.<OnDrawGizmos>g__SetColor|43_0(this.transitionPoint1.IsReached);
				Gizmos.DrawWireSphere(this.transitionPoint1.Position, 0.5f);
				Gizmos.DrawLine(this.currentChunkExplorer.Position, this.transitionPoint1.Position);
			}
		}

		// Token: 0x06001476 RID: 5238 RVA: 0x000409E2 File Offset: 0x0003EBE2
		[CompilerGenerated]
		private bool <PrepareMobsTransfer>g__IsTransferableMob|31_0(BaseGameMob groupMob)
		{
			ILocationChunk currentLocationChunk = groupMob.CurrentLocationChunk;
			return !groupMob.IsSummoned && !this.mobsTransferer.IsTargetChunkReached(groupMob);
		}

		// Token: 0x06001477 RID: 5239 RVA: 0x00040A04 File Offset: 0x0003EC04
		[CompilerGenerated]
		private void <TrySetMainChunkVisible>g__UpdateVisibility|35_0(ILocationChunk chunk, ILocationChunkVisitor visitor)
		{
			if (visitor.AffectLocationChunkVisibility)
			{
				this.currentChunk.IsVisible = true;
				chunk.SetChunkObjectActive(true);
				chunk.IsVisible = true;
			}
		}

		// Token: 0x06001478 RID: 5240 RVA: 0x00040A28 File Offset: 0x0003EC28
		[CompilerGenerated]
		internal static void <OnDrawGizmos>g__SetColor|43_0(bool isPointReached)
		{
			Gizmos.color = (isPointReached ? Color.green : Color.magenta);
		}

		// Token: 0x04000BDC RID: 3036
		public const float MinTransitionThreshold = 0.1f;

		// Token: 0x04000BDD RID: 3037
		public const float MaxTransitionThreshold = 0.5f;

		// Token: 0x04000BDE RID: 3038
		private static readonly List<BaseGameMob> TransferingMobsBuffer = new List<BaseGameMob>(100);

		// Token: 0x04000BDF RID: 3039
		[SerializeField]
		[Range(0.1f, 0.5f)]
		private float _transitionThreshold0 = 0.5f;

		// Token: 0x04000BE0 RID: 3040
		[SerializeField]
		[Range(0.1f, 0.5f)]
		private float _transitionThreshold1 = 0.5f;

		// Token: 0x04000BE1 RID: 3041
		private ILocationChunkVisitor currentChunkExplorer;

		// Token: 0x04000BE2 RID: 3042
		private ILocationChunkGateway entranceGateway;

		// Token: 0x04000BE3 RID: 3043
		private ILocationChunkGateway exitGateway;

		// Token: 0x04000BE4 RID: 3044
		private ILocationChunk lastChunk;

		// Token: 0x04000BE5 RID: 3045
		private ILocationChunk nextChunk;

		// Token: 0x04000BE6 RID: 3046
		private ConnectingChunkController.TransitionPoint transitionPoint0;

		// Token: 0x04000BE7 RID: 3047
		private ConnectingChunkController.TransitionPoint transitionPoint1;

		// Token: 0x04000BE8 RID: 3048
		private Vector2 chunkEnteringDirection;

		// Token: 0x04000BE9 RID: 3049
		private ConnectingChunkController.GroupMobsTransferer mobsTransferer;

		// Token: 0x04000BEA RID: 3050
		private int mobsInConnectingChunkCount;

		// Token: 0x04000BEB RID: 3051
		private bool isConnectingChunkFullyReached;

		// Token: 0x020004E2 RID: 1250
		private struct TransitionPoint
		{
			// Token: 0x170007A4 RID: 1956
			// (get) Token: 0x0600258A RID: 9610 RVA: 0x00074AFE File Offset: 0x00072CFE
			public bool IsReached
			{
				get
				{
					return this._isReached;
				}
			}

			// Token: 0x0600258B RID: 9611 RVA: 0x00074B06 File Offset: 0x00072D06
			public TransitionPoint(Vector2 position, Action<bool> pointStateChangedCallback)
			{
				this.Position = position;
				this.pointStateChangedCallback = pointStateChangedCallback;
				this._isReached = false;
			}

			// Token: 0x0600258C RID: 9612 RVA: 0x00074B1D File Offset: 0x00072D1D
			public TransitionPoint(Vector2 position)
			{
				this = new ConnectingChunkController.TransitionPoint(position, null);
			}

			// Token: 0x0600258D RID: 9613 RVA: 0x00074B28 File Offset: 0x00072D28
			public void Update(Vector2 chunkExplorerPosition, Vector2 transitionDirection)
			{
				bool flag = Vector2.Dot(chunkExplorerPosition - this.Position, transitionDirection) > 0f;
				if (this._isReached != flag)
				{
					Action<bool> action = this.pointStateChangedCallback;
					if (action != null)
					{
						action(flag);
					}
					this._isReached = flag;
				}
			}

			// Token: 0x04001A25 RID: 6693
			public readonly Vector2 Position;

			// Token: 0x04001A26 RID: 6694
			private readonly Action<bool> pointStateChangedCallback;

			// Token: 0x04001A27 RID: 6695
			private bool _isReached;
		}

		// Token: 0x020004E3 RID: 1251
		private struct GroupMobsTransferer
		{
			// Token: 0x0600258E RID: 9614 RVA: 0x00074B74 File Offset: 0x00072D74
			public GroupMobsTransferer(ConnectingChunkController controller, float maxTransferingDistance)
			{
				ILocationChunkGateway entranceGateway = controller.entranceGateway;
				Vector2 vector = -entranceGateway.TransitionDirection;
				this.gatewayAxis1 = new Vector2
				{
					x = vector.y,
					y = vector.x
				};
				Vector2 localSize = entranceGateway.GetLocalSize();
				this.startPoint = entranceGateway.Position + (vector * localSize.y - this.gatewayAxis1 * localSize.x) * 0.5f;
				this.transferingDirection = controller.chunkEnteringDirection;
				this.maxOffsets = new Vector2(localSize.x, maxTransferingDistance);
				this.randomPointIndex = UnityEngine.Random.Range(1, 10000);
			}

			// Token: 0x0600258F RID: 9615 RVA: 0x00074C30 File Offset: 0x00072E30
			public bool IsTargetChunkReached(BaseGameMob groupMob)
			{
				return Vector2.Dot(groupMob.Position - this.startPoint, this.transferingDirection) > groupMob.Radius;
			}

			// Token: 0x06002590 RID: 9616 RVA: 0x00074C58 File Offset: 0x00072E58
			public bool TransferMob(BaseGameMob groupMob)
			{
				if (this.IsTargetChunkReached(groupMob))
				{
					return true;
				}
				float radius = groupMob.Radius;
				int num = this.randomPointIndex;
				this.randomPointIndex = num + 1;
				Vector2 r2Value = PhiSequence.GetR2Value(num, false);
				r2Value.x = r2Value.x * Mathf.Max(0f, this.maxOffsets.x - radius) + radius;
				r2Value.y = r2Value.y * Mathf.Max(0f, this.maxOffsets.y - radius) + radius;
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(new Vector3
				{
					x = this.startPoint.x + this.gatewayAxis1.x * r2Value.x + this.transferingDirection.x * r2Value.y,
					y = this.startPoint.y + this.gatewayAxis1.y * r2Value.x + this.transferingDirection.y * r2Value.y
				}, out navMeshHit, groupMob.Radius, -1))
				{
					groupMob.Position = navMeshHit.position;
				}
				return false;
			}

			// Token: 0x04001A28 RID: 6696
			private readonly Vector2 gatewayAxis1;

			// Token: 0x04001A29 RID: 6697
			private readonly Vector2 transferingDirection;

			// Token: 0x04001A2A RID: 6698
			private readonly Vector2 maxOffsets;

			// Token: 0x04001A2B RID: 6699
			private readonly Vector2 startPoint;

			// Token: 0x04001A2C RID: 6700
			private int randomPointIndex;
		}
	}
}
