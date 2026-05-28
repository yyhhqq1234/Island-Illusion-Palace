using System;
using Common.CollectionsExtensions;
using Common.Editor;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Core;
using Game.Factories;
using Game.LevelGeneration;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000143 RID: 323
	public sealed class PlayerSpawner : ObjectSpawnerBase<PlayerBehaviour.ID, PlayerBehaviour>
	{
		// Token: 0x0600085F RID: 2143 RVA: 0x0001C002 File Offset: 0x0001A202
		protected override IFactory GetFactory(IGame currentGame)
		{
			return currentGame.Services.Get<IPlayerFactory>();
		}

		// Token: 0x06000860 RID: 2144 RVA: 0x0001C010 File Offset: 0x0001A210
		protected override object Spawn(ObjectSpawnerBase<PlayerBehaviour.ID, PlayerBehaviour>.BaseSpawningInfoItem spawningInfo, IFactory targetFactory)
		{
			if (this.sessionManager.CurrentPlayer != null)
			{
				return null;
			}
			GameMobSpawningInfo spawnerInfo = new GameMobSpawningInfo
			{
				spawner = this
			};
			PlayerFactoryArgs playerFactoryArgs = new PlayerFactoryArgs
			{
				playerID = spawningInfo.ObjectID,
				spawnerInfo = spawnerInfo,
				spawnPosition = base.transform.position,
				playerMobsSpawnerOverrides = this.playerMobsSpawnerOverrides
			};
			if (this.appearanceAnimationTriggers != null && this.appearanceAnimationTriggers.Length != 0)
			{
				playerFactoryArgs.appearanceAnimationTrigger = this.appearanceAnimationTriggers.GetRandomItem(0, -1);
			}
			if (this.initialPlayerAbilitiesOverrides != null && this.initialPlayerAbilitiesOverrides.IsValid())
			{
				playerFactoryArgs.initialPlayerAbilitiesOverrides = this.initialPlayerAbilitiesOverrides;
			}
			return targetFactory.Create(playerFactoryArgs);
		}

		// Token: 0x06000861 RID: 2145 RVA: 0x0001C0C4 File Offset: 0x0001A2C4
		protected override async void Start()
		{
			IGameSessionManager gameSessionManager = await base.CurrentGame.Services.GetServiceAsync<IGameSessionManager>();
			this.sessionManager = gameSessionManager;
			base.Start();
		}

		// Token: 0x06000862 RID: 2146 RVA: 0x0001C100 File Offset: 0x0001A300
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			ObjectSpawnerBase<PlayerBehaviour.ID, PlayerBehaviour>.BaseSpawningInfoItem[] spawningInfo = this.initialSpawningInfo;
			base.SpawningInfo = spawningInfo;
			this.currentChunk = base.GetComponentInParent<LocationChunk>();
			if (this.currentChunk != null)
			{
				this.currentChunk.PlacedByLocationGenerator += this.OnCurrentChunkPlaced;
				this.currentChunk.RegisteredInLocation += this.OnRegisteredInLocation;
			}
		}

		// Token: 0x06000863 RID: 2147 RVA: 0x0001C16A File Offset: 0x0001A36A
		private void OnRegisteredInLocation(IGameLocation gameLocation, int chunkIndex)
		{
			this.currentChunk.RegisteredInLocation -= this.OnRegisteredInLocation;
			if (chunkIndex != 0)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}

		// Token: 0x06000864 RID: 2148 RVA: 0x0001C191 File Offset: 0x0001A391
		private void OnCurrentChunkPlaced(IGameLocationGenerator generator, LocationChunk currentChunk)
		{
			base.transform.parent = null;
			currentChunk.PlacedByLocationGenerator -= this.OnCurrentChunkPlaced;
		}

		// Token: 0x040004C2 RID: 1218
		[SerializeField]
		private PlayerSpawner.PlayerSpawningInfoItem[] initialSpawningInfo;

		// Token: 0x040004C3 RID: 1219
		public InitialPlayerAbilitiesInfo initialPlayerAbilitiesOverrides;

		// Token: 0x040004C4 RID: 1220
		public MobSpawnerOverrides playerMobsSpawnerOverrides;

		// Token: 0x040004C5 RID: 1221
		public string[] appearanceAnimationTriggers;

		// Token: 0x040004C6 RID: 1222
		private IGameSessionManager sessionManager;

		// Token: 0x040004C7 RID: 1223
		private LocationChunk currentChunk;

		// Token: 0x0200044F RID: 1103
		[Serializable]
		public sealed class PlayerSpawningInfoItem : ObjectSpawnerBase<PlayerBehaviour.ID, PlayerBehaviour>.BaseSpawningInfoItem
		{
			// Token: 0x1700072F RID: 1839
			// (get) Token: 0x06002372 RID: 9074 RVA: 0x0006DD16 File Offset: 0x0006BF16
			// (set) Token: 0x06002373 RID: 9075 RVA: 0x0006DD1E File Offset: 0x0006BF1E
			public override PlayerBehaviour.ID ObjectID
			{
				get
				{
					return this._objectID;
				}
				set
				{
					this._objectID = value;
				}
			}

			// Token: 0x040016DE RID: 5854
			[SerializeField]
			[EnumPopup]
			private PlayerBehaviour.ID _objectID;
		}
	}
}
