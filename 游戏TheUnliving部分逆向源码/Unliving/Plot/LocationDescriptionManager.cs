using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CollectionsExtensions;
using Common.RestorableState;
using Common.ServiceRegistry;
using Game.Core;
using Game.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.PlayerProfileManagement;
using Unliving.Plot.TreeBasedCharacterPlot;

namespace Unliving.Plot
{
	// Token: 0x020002E2 RID: 738
	[CreateAssetMenu(fileName = "LocationDescriptionManager", menuName = "Game/Plot/Location Description Manager")]
	[Service(typeof(LocationDescriptionManager), new Type[]
	{

	})]
	public sealed class LocationDescriptionManager : GlobalManagerBase
	{
		// Token: 0x14000100 RID: 256
		// (add) Token: 0x06001981 RID: 6529 RVA: 0x0004FEBC File Offset: 0x0004E0BC
		// (remove) Token: 0x06001982 RID: 6530 RVA: 0x0004FEF4 File Offset: 0x0004E0F4
		public event Action LocationDescriptionGenerated;

		// Token: 0x06001983 RID: 6531 RVA: 0x0004FF2C File Offset: 0x0004E12C
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.CurrentGame.Services.TryGet<GameManager>(out this.gameManager))
			{
				this.gameManager.ChangingGameScene += this.OnChangingGameScene;
			}
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x0004FFAF File Offset: 0x0004E1AF
		private void OnChangingGameScene(IGameManager arg1, string arg2)
		{
			this.currentLocationNode = null;
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x0004FFB8 File Offset: 0x0004E1B8
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			base.OnSceneLoaded(loadedScene);
			this.CreateCurrentLocationDescription();
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x0004FFC8 File Offset: 0x0004E1C8
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			if (base.CurrentGame.Services.TryGet<LocalizationManager>(out this.localizationManager))
			{
				List<LocationDescriptionPlotNode> nodes = this.locationDescriptionPlotNodeGraph.GetNodes();
				for (int i = 0; i < nodes.Count; i++)
				{
					LocationDescriptionPlotNode locationDescriptionPlotNode = nodes[i];
					locationDescriptionPlotNode.ResetUsedDescriptions();
					ILocalizableDataHolder localizableDataHolder = locationDescriptionPlotNode;
					if (localizableDataHolder != null)
					{
						this.localizationManager.TrySetLocalizedData(localizableDataHolder);
					}
				}
			}
			this.profileManager.LoadLocationsDescriptionsState(this);
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x00050034 File Offset: 0x0004E234
		public bool HasCurrentLocationMetadata()
		{
			return this.currentLocationNode != null;
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x00050040 File Offset: 0x0004E240
		public Metadata GetCurrentLocationMetadata()
		{
			LocationDescriptionPlotNode locationDescriptionPlotNode = this.currentLocationNode;
			Metadata metadata = (locationDescriptionPlotNode != null) ? locationDescriptionPlotNode.GetLocationDescriptionMetadata() : null;
			if (metadata == null)
			{
				return null;
			}
			string id = this.currentLocationNode.tutorialNode ? "level_tutorial" : this.gameLocationProvider.LevelID;
			metadata.Title = this.localizationManager.GetMetadata(id, Array.Empty<string>()).Description;
			return metadata;
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x000500A4 File Offset: 0x0004E2A4
		private void CreateCurrentLocationDescription()
		{
			if (!base.CurrentGame.Services.TryGet<IGameLocationProvider>(out this.gameLocationProvider))
			{
				this.currentLocationNode = null;
				return;
			}
			PlayerProfileManager playerProfileManager;
			if (this.playerPlotProgress == null && base.CurrentGame.Services.TryGet<PlayerProfileManager>(out playerProfileManager))
			{
				PlayerProfile currentPlayerProfile = playerProfileManager.CurrentPlayerProfile;
				this.playerPlotProgress = ((currentPlayerProfile != null) ? currentPlayerProfile.gamePlotProgress : null);
			}
			CharacterPlotContext context = new CharacterPlotContext
			{
				currentGame = base.CurrentGame,
				totalPlotProgress = this.playerPlotProgress
			};
			GameLocation.TypeID locationType = this.gameLocationProvider.LocationType;
			List<LocationDescriptionPlotNode> nodesOfType = this.locationDescriptionPlotNodeGraph.GetNodesOfType(locationType, LocationDescriptionPlotNode.NodeType.Exposition, context, new Predicate<LocationDescriptionPlotNode>(this.ExpositionNodesFilterPredicate));
			if (nodesOfType.Count == 0)
			{
				this.locationDescriptionPlotNodeGraph.GetNodesOfType(locationType, LocationDescriptionPlotNode.NodeType.Main, context, new Predicate<LocationDescriptionPlotNode>(this.MainNodesFilterPredicate));
			}
			LocationDescriptionPlotNode locationDescriptionPlotNode = null;
			for (int i = 0; i < nodesOfType.Count; i++)
			{
				LocationDescriptionPlotNode locationDescriptionPlotNode2 = nodesOfType[i];
				if (locationDescriptionPlotNode == null || locationDescriptionPlotNode.priority < locationDescriptionPlotNode2.priority)
				{
					locationDescriptionPlotNode = locationDescriptionPlotNode2;
				}
			}
			this.currentLocationNode = locationDescriptionPlotNode;
			Action locationDescriptionGenerated = this.LocationDescriptionGenerated;
			if (locationDescriptionGenerated == null)
			{
				return;
			}
			locationDescriptionGenerated();
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x000501C4 File Offset: 0x0004E3C4
		private bool MainNodesFilterPredicate(LocationDescriptionPlotNode node)
		{
			IGameLocationProvider gameLocationProvider = this.gameLocationProvider;
			GameLocation gameLocation = (gameLocationProvider != null) ? gameLocationProvider.CurrentLocation : null;
			return gameLocation == null || (!node.tutorialNode ^ gameLocation.IsTutorialLocation);
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x000501F9 File Offset: 0x0004E3F9
		private bool ExpositionNodesFilterPredicate(LocationDescriptionPlotNode node)
		{
			return node.UsingProgress == 0f;
		}

		// Token: 0x0600198C RID: 6540 RVA: 0x00050208 File Offset: 0x0004E408
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
			if (this.gameManager != null)
			{
				this.gameManager.ChangingGameScene -= this.OnChangingGameScene;
			}
		}

		// Token: 0x04000E4A RID: 3658
		private const string TutorialLocalizationKey = "level_tutorial";

		// Token: 0x04000E4B RID: 3659
		[SerializeField]
		private LocationDescriptionPlotNodeGraph locationDescriptionPlotNodeGraph;

		// Token: 0x04000E4C RID: 3660
		private TotalGamePlotProgressBase playerPlotProgress;

		// Token: 0x04000E4D RID: 3661
		private IGameLocationProvider gameLocationProvider;

		// Token: 0x04000E4E RID: 3662
		private PlayerProfileManager profileManager;

		// Token: 0x04000E4F RID: 3663
		private LocationDescriptionPlotNode currentLocationNode;

		// Token: 0x04000E50 RID: 3664
		private LocalizationManager localizationManager;

		// Token: 0x04000E51 RID: 3665
		private GameManager gameManager;

		// Token: 0x0200053C RID: 1340
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<LocationDescriptionManager>, ICloneable<LocationDescriptionManager.RestorableState>
		{
			// Token: 0x0600269B RID: 9883 RVA: 0x00078521 File Offset: 0x00076721
			public RestorableState() : base(null)
			{
			}

			// Token: 0x0600269C RID: 9884 RVA: 0x00078536 File Offset: 0x00076736
			public RestorableState(LocationDescriptionManager manager) : base(manager)
			{
			}

			// Token: 0x0600269D RID: 9885 RVA: 0x0007854C File Offset: 0x0007674C
			public override void Store(LocationDescriptionManager manager)
			{
				List<LocationDescriptionPlotNode> nodes = manager.locationDescriptionPlotNodeGraph.GetNodes();
				this.nodesData = new LocationDescriptionManager.RestorableState.DescriptionData[nodes.Count];
				for (int i = 0; i < nodes.Count; i++)
				{
					LocationDescriptionPlotNode locationDescriptionPlotNode = nodes[i];
					this.nodesData[i] = new LocationDescriptionManager.RestorableState.DescriptionData
					{
						id = locationDescriptionPlotNode.descriptionID,
						usedDescriptions = locationDescriptionPlotNode.GetUsedDescriptions().ToArray()
					};
				}
			}

			// Token: 0x0600269E RID: 9886 RVA: 0x000785BC File Offset: 0x000767BC
			public override void Restore(LocationDescriptionManager manager, object args = null)
			{
				for (int i = 0; i < this.nodesData.Length; i++)
				{
					LocationDescriptionManager.RestorableState.DescriptionData descriptionData = this.nodesData[i];
					LocationDescriptionPlotNode locationDescriptionPlotNode;
					if (manager.locationDescriptionPlotNodeGraph.TryGetNodeWithID(descriptionData.id, out locationDescriptionPlotNode))
					{
						locationDescriptionPlotNode.SetUsedDescriptions(descriptionData.usedDescriptions.ToList<int>());
					}
				}
			}

			// Token: 0x0600269F RID: 9887 RVA: 0x0007860B File Offset: 0x0007680B
			public LocationDescriptionManager.RestorableState Clone()
			{
				return new LocationDescriptionManager.RestorableState
				{
					nodesData = this.nodesData.CloneArray<LocationDescriptionManager.RestorableState.DescriptionData>()
				};
			}

			// Token: 0x04001B84 RID: 7044
			public LocationDescriptionManager.RestorableState.DescriptionData[] nodesData = new LocationDescriptionManager.RestorableState.DescriptionData[0];

			// Token: 0x020005B7 RID: 1463
			[Serializable]
			public class DescriptionData
			{
				// Token: 0x04001D3D RID: 7485
				public string id;

				// Token: 0x04001D3E RID: 7486
				public int[] usedDescriptions = new int[0];
			}
		}
	}
}
