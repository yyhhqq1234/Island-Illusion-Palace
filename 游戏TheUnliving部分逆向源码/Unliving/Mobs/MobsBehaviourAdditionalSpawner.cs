using System;
using Common;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs.Animation;

namespace Unliving.Mobs
{
	// Token: 0x020001C9 RID: 457
	public class MobsBehaviourAdditionalSpawner : GameBehaviourBase, ILocationObject, IGameMobsSpawner
	{
		// Token: 0x170002C8 RID: 712
		// (get) Token: 0x06000E5A RID: 3674 RVA: 0x0002D95A File Offset: 0x0002BB5A
		// (set) Token: 0x06000E5B RID: 3675 RVA: 0x0002D962 File Offset: 0x0002BB62
		ILocationChunk ILocationObject.CurrentLocationChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
			set
			{
				this.currentLocationChunk = value;
			}
		}

		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000E5C RID: 3676 RVA: 0x0002D96B File Offset: 0x0002BB6B
		ILocationChunk IGameMobsSpawner.SpawningChunk
		{
			get
			{
				return this.currentLocationChunk;
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000E5D RID: 3677 RVA: 0x0002D973 File Offset: 0x0002BB73
		public IGameMobsFactory MobsFactory
		{
			get
			{
				return this.groupSpawner.MobsFactory;
			}
		}

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000E5E RID: 3678 RVA: 0x0002D980 File Offset: 0x0002BB80
		int? ILocationObject.LocationObjectType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000E5F RID: 3679 RVA: 0x0002D996 File Offset: 0x0002BB96
		Vector2 ILocationObject.Position
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000E60 RID: 3680 RVA: 0x0002D9A8 File Offset: 0x0002BBA8
		float ILocationObject.Orientation
		{
			get
			{
				return 0f;
			}
		}

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000E61 RID: 3681 RVA: 0x0002D9AF File Offset: 0x0002BBAF
		bool IGameMobsSpawner.SpawnEnvironmentMobs
		{
			get
			{
				return ((IGameMobsSpawner)this.groupSpawner).SpawnEnvironmentMobs;
			}
		}

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000E62 RID: 3682 RVA: 0x0002D9BC File Offset: 0x0002BBBC
		GameMobFactions IGameMobsSpawner.GroupOwner
		{
			get
			{
				return this.groupSpawner.GroupOwner;
			}
		}

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000E63 RID: 3683 RVA: 0x0002D9C9 File Offset: 0x0002BBC9
		int IGameMobsSpawner.RemainingSpawningCount
		{
			get
			{
				return this.remainingSpawningCount;
			}
		}

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000E64 RID: 3684 RVA: 0x0002D9D1 File Offset: 0x0002BBD1
		bool ILocationObject.IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x0002D9D4 File Offset: 0x0002BBD4
		private void OnEnable()
		{
			GameSceneManager gameSceneManager;
			if (this.currentLocationChunk == null && this.groupSpawner.GroupOwner != GameMobFactions.PLAYER && base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				GameLocation generatedLocation = gameSceneManager.GeneratedLocation;
				if (generatedLocation == null)
				{
					return;
				}
				ILocationChunk locationChunkAtPoint = generatedLocation.GetLocationChunkAtPoint(base.transform.position, false);
				if (locationChunkAtPoint == null)
				{
					return;
				}
				locationChunkAtPoint.AddEnvironmentObject(this);
			}
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x0002DA38 File Offset: 0x0002BC38
		public override async void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			foreach (MobBehaviourSpawner.MobSpawningInfoItem mobSpawningInfoItem in this.initialSpawningInfo)
			{
				this.remainingSpawningCount += mobSpawningInfoItem.ForcedCount;
			}
			await new WaitUntil(() => this.groupSpawner.SpawnedGroup != null);
			foreach (MobBehaviourSpawner.MobSpawningInfoItem mobSpawningInfoItem2 in this.initialSpawningInfo)
			{
				for (int j = 0; j < mobSpawningInfoItem2.ForcedCount; j++)
				{
					MobBehaviour mobBehaviour = this.groupSpawner.Spawn(mobSpawningInfoItem2);
					mobBehaviour.Position = base.transform.position;
					GameMobAnimationController gameMobAnimationController = mobBehaviour.AnimationController as GameMobAnimationController;
					if (gameMobAnimationController != null)
					{
						gameMobAnimationController.SetIdleAnimationIndex(this.startIdleIndex);
						gameMobAnimationController.startDirection = this.startDirection;
					}
				}
			}
			this.remainingSpawningCount = 0;
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x0002DA79 File Offset: 0x0002BC79
		protected override void OnDestroy()
		{
			base.OnDestroy();
			ILocationChunk locationChunk = this.currentLocationChunk;
			if (locationChunk == null)
			{
				return;
			}
			locationChunk.RemoveEnvironmentObject(this);
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x0002DA92 File Offset: 0x0002BC92
		BaseGameMob IGameMobsSpawner.SpawnMob(int mobID)
		{
			throw new NotImplementedException();
		}

		// Token: 0x0400087E RID: 2174
		public MobBehaviourSpawner groupSpawner;

		// Token: 0x0400087F RID: 2175
		public int startIdleIndex;

		// Token: 0x04000880 RID: 2176
		public TransformDirection startDirection;

		// Token: 0x04000881 RID: 2177
		public MobBehaviourSpawner.MobSpawningInfoItem[] initialSpawningInfo;

		// Token: 0x04000882 RID: 2178
		private ILocationChunk currentLocationChunk;

		// Token: 0x04000883 RID: 2179
		private int remainingSpawningCount;
	}
}
