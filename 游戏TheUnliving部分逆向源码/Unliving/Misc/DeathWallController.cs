using System;
using System.Collections.Generic;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Player;

namespace Unliving.Misc
{
	// Token: 0x0200023F RID: 575
	public class DeathWallController : GameBehaviourBase
	{
		// Token: 0x0600138D RID: 5005 RVA: 0x0003D240 File Offset: 0x0003B440
		private void Start()
		{
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				this.currentPlayer = playerProvider.CurrentPlayer;
				this.currentPlayer.LocationChunkEntered += this.OnPlayerLocationChunkEntered;
				if (this.currentPlayer.CurrentLocationChunk != null)
				{
					this.OnPlayerLocationChunkEntered(null, this.currentPlayer.CurrentLocationChunk);
				}
			}
		}

		// Token: 0x0600138E RID: 5006 RVA: 0x0003D2A4 File Offset: 0x0003B4A4
		private void OnPlayerLocationChunkEntered(ILocationChunk lastChunk, ILocationChunk newChunk)
		{
			DeathWall deathWall;
			if (this.lastVisitedChunk != null && this.deathWallsDict.TryGetValue(this.lastVisitedChunk, out deathWall) && deathWall.TryToStartMoving())
			{
				this.lastVisitedChunk = null;
			}
			if (!newChunk.HasType(LocationChunk.TypeID.BattleChunk) || this.deathWallsDict.ContainsKey(newChunk))
			{
				return;
			}
			float num = float.MinValue;
			Vector2 direction = default(Vector2);
			ILocationChunk locationChunk = null;
			foreach (ILocationChunkGateway locationChunkGateway in newChunk.Gateways)
			{
				LocationChunkGateway locationChunkGateway2 = locationChunkGateway as LocationChunkGateway;
				if (locationChunkGateway2 != null)
				{
					float sqrMagnitude = (locationChunkGateway2.transform.position - this.currentPlayer.transform.position).sqrMagnitude;
					if (sqrMagnitude > num)
					{
						num = sqrMagnitude;
						direction = locationChunkGateway.TransitionDirection;
						locationChunk = locationChunkGateway2.GetNextChunk();
					}
				}
			}
			this.lastVisitedChunk = newChunk;
			deathWall = UnityEngine.Object.Instantiate<DeathWall>(this.deathWallPrefab, base.transform);
			deathWall.SetData(direction, newChunk as LocationChunk, locationChunk as LocationChunk, this.currentPlayer);
			this.deathWallsDict.Add(newChunk, deathWall);
		}

		// Token: 0x0600138F RID: 5007 RVA: 0x0003D3D4 File Offset: 0x0003B5D4
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.currentPlayer.LocationChunkEntered -= this.OnPlayerLocationChunkEntered;
		}

		// Token: 0x04000B5E RID: 2910
		private const LocationChunk.TypeID DeathWallChunkType = LocationChunk.TypeID.BattleChunk;

		// Token: 0x04000B5F RID: 2911
		public DeathWall deathWallPrefab;

		// Token: 0x04000B60 RID: 2912
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000B61 RID: 2913
		private readonly Dictionary<ILocationChunk, DeathWall> deathWallsDict = new Dictionary<ILocationChunk, DeathWall>();

		// Token: 0x04000B62 RID: 2914
		private ILocationChunk lastVisitedChunk;
	}
}
