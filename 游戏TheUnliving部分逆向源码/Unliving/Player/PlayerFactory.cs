using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Abilities;
using Game.Core;
using UnityEngine;
using Unliving.Abilities;
using Unliving.AbilityResources;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Player.Upgrades;
using Unliving.PlayerProfileManagement;
using Unliving.PlayerUpgradesStore;
using Unliving.Purchasing;

namespace Unliving.Player
{
	// Token: 0x02000140 RID: 320
	[Service(typeof(PlayerFactory), new Type[]
	{
		typeof(IPlayerFactory)
	})]
	public sealed class PlayerFactory : GameMobsFactoryBase<PlayerBehaviour.FactoryPrototype>, IPlayerFactory, IObjectFactory<PlayerBehaviour>, IFactory<IBaseObjectDescription, PlayerBehaviour>, IFactory
	{
		// Token: 0x1700015B RID: 347
		// (get) Token: 0x0600084C RID: 2124 RVA: 0x0001BAF3 File Offset: 0x00019CF3
		public static int PlayerLayerMask
		{
			get
			{
				if (PlayerFactory.PlayerLayer >= 0)
				{
					return 1 << PlayerFactory.PlayerLayer;
				}
				return 0;
			}
		}

		// Token: 0x0600084D RID: 2125 RVA: 0x0001BB0C File Offset: 0x00019D0C
		private static void CreateNecromancyTargetsWatcher(PlayerBehaviour player)
		{
			GameObject gameObject = new GameObject("NecromancyTargetsWatcher");
			gameObject.layer = 2;
			CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
			circleCollider2D.isTrigger = true;
			circleCollider2D.radius = 1f;
			RevivableMobsAreaWatcher revivableMobsAreaWatcher = gameObject.AddComponent<RevivableMobsAreaWatcher>();
			revivableMobsAreaWatcher.UpdateRate = 3f;
			revivableMobsAreaWatcher.isContinuousObjectValidationEnabled = true;
			revivableMobsAreaWatcher.AreaCollider = circleCollider2D;
			gameObject.transform.parent = player.transform;
			gameObject.transform.localPosition = default(Vector3);
			gameObject.SetActive(false);
			player.NecromancyTargetsWatcher = revivableMobsAreaWatcher;
			PlayerFactory.<CreateNecromancyTargetsWatcher>g__StartNecromancyTargetsWatcherSetupTask|4_0(player, revivableMobsAreaWatcher);
		}

		// Token: 0x0600084E RID: 2126 RVA: 0x0001BB9C File Offset: 0x00019D9C
		private static void CreateResourcesWatcher(PlayerBehaviour player)
		{
			GameObject gameObject = new GameObject("AbilityResourcesWatcher");
			gameObject.layer = 2;
			CircleCollider2D circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
			circleCollider2D.isTrigger = true;
			circleCollider2D.radius = 1f;
			AbilityResourcesWatcher abilityResourcesWatcher = gameObject.AddComponent<AbilityResourcesWatcher>();
			abilityResourcesWatcher.observableLayers = AbilityResourcesConsumer.CollectableResourcesLayerMask;
			abilityResourcesWatcher.UpdateRate = 3f;
			abilityResourcesWatcher.isContinuousObjectValidationEnabled = false;
			abilityResourcesWatcher.AreaCollider = circleCollider2D;
			gameObject.transform.parent = player.transform;
			gameObject.transform.localPosition = default(Vector3);
			player.AbilityResourcesWatcher = abilityResourcesWatcher;
			PlayerFactory.<CreateResourcesWatcher>g__PassAbilitiesSource|5_0(player, abilityResourcesWatcher);
		}

		// Token: 0x0600084F RID: 2127 RVA: 0x0001BC38 File Offset: 0x00019E38
		protected override GameMobFactionInfo GetMobFactionInfo(BaseGameMob mob, GameMobFactions faction)
		{
			return new GameMobFactionInfo
			{
				faction = GameMobFactions.PLAYER,
				mobsLayer = PlayerFactory.PlayerLayer,
				enemyMobLayers = this.mobsFactory.GetFactionInfo(GameMobFactions.PLAYER).enemyMobLayers
			};
		}

		// Token: 0x06000850 RID: 2128 RVA: 0x0001BC7C File Offset: 0x00019E7C
		protected override void SetMobParams(PlayerBehaviour.FactoryPrototype mobData, GameObject mobPrefab, IGameMob mob, GameMobsFactoryArgsBase args)
		{
			base.SetMobParams(mobData, mobPrefab, mob, args);
			PlayerBehaviour playerBehaviour = (PlayerBehaviour)mob;
			playerBehaviour.ObjectID = mobData.objectID;
			PlayerFactory.CreateNecromancyTargetsWatcher(playerBehaviour);
			PlayerFactory.CreateResourcesWatcher(playerBehaviour);
			PlayerFactoryArgs playerFactoryArgs = args as PlayerFactoryArgs;
			if (playerFactoryArgs != null)
			{
				if (playerFactoryArgs.initialPlayerAbilitiesOverrides != null)
				{
					playerBehaviour.OverrideInitialAbilities(playerFactoryArgs.initialPlayerAbilitiesOverrides);
				}
				MobSpawnerOverrides playerMobsSpawnerOverrides = playerFactoryArgs.playerMobsSpawnerOverrides;
				MobBehaviourSpawner mobBehaviourSpawner;
				if (playerMobsSpawnerOverrides != null && playerMobsSpawnerOverrides.overrideSpawnerParams && playerBehaviour.TryGetComponent<MobBehaviourSpawner>(out mobBehaviourSpawner))
				{
					mobBehaviourSpawner.enabled = true;
					playerMobsSpawnerOverrides.Use(mobBehaviourSpawner);
				}
				if (!string.IsNullOrEmpty(playerFactoryArgs.appearanceAnimationTrigger))
				{
					playerBehaviour.AnimationController.ActivateStateTrigger(playerFactoryArgs.appearanceAnimationTrigger, false, false);
				}
			}
			if (!playerBehaviour.InHomespace)
			{
				IPlayerUpgradesRegistry playerUpgradesRegistry;
				if (!playerBehaviour.TryGetComponent<IPlayerUpgradesRegistry>(out playerUpgradesRegistry))
				{
					playerUpgradesRegistry = playerBehaviour.gameObject.AddComponent<PlayerUpgradesRegistryComponent>();
				}
				PlayerProfile currentPlayerProfile = this.profileManager.CurrentPlayerProfile;
				IReadOnlyList<PlayerUpgradeInfo> readOnlyList;
				if (currentPlayerProfile == null)
				{
					readOnlyList = null;
				}
				else
				{
					PlayerBehaviour.RestorableState playerCharacterState = currentPlayerProfile.playerCharacterState;
					readOnlyList = ((playerCharacterState != null) ? playerCharacterState.Upgrades : null);
				}
				IReadOnlyList<PlayerUpgradeInfo> readOnlyList2 = readOnlyList;
				if ((readOnlyList2 != null && readOnlyList2.Count != 0) || (this.upgradesStore != null && this.upgradesStore.IsStoreActive))
				{
					IPlayerUpgradesFactory playerUpgradesFactory = this.upgradesFactory;
					IPlayerUpgrade playerUpgrade = (playerUpgradesFactory != null) ? playerUpgradesFactory.CreatePlayerFeaturesBlocker() : null;
					if (playerUpgrade != null)
					{
						playerUpgradesRegistry.AddUpgrade(playerUpgrade);
					}
				}
				IPlayerUpgrade rerollCurrencyUpgrade = this.purchaseManager.GetRerollCurrencyUpgrade();
				if (rerollCurrencyUpgrade != null)
				{
					playerUpgradesRegistry.AddUpgrade(rerollCurrencyUpgrade);
				}
				List<PurchasablePlayerUpgrade> purchasablesOfType = this.purchaseManager.GetPurchasablesOfType<PurchasablePlayerUpgrade>(true);
				if (purchasablesOfType != null)
				{
					for (int i = 0; i < purchasablesOfType.Count; i++)
					{
						PurchasablePlayerUpgrade purchasablePlayerUpgrade = purchasablesOfType[i];
						if (purchasablePlayerUpgrade != null)
						{
							PlayerUpgradeID upgradeID = (PlayerUpgradeID)purchasablePlayerUpgrade.ObjectID;
							IPlayerUpgrade playerUpgrade2 = (IPlayerUpgrade)purchasablePlayerUpgrade.upgradePrototype;
							if (playerUpgrade2 != null)
							{
								PlayerUpgradesFactoryArgs args2 = new PlayerUpgradesFactoryArgs
								{
									upgradePrototype = playerUpgrade2,
									upgradeID = upgradeID
								};
								IPlayerUpgradesFactory playerUpgradesFactory2 = this.upgradesFactory;
								IPlayerUpgrade playerUpgrade3 = (playerUpgradesFactory2 != null) ? playerUpgradesFactory2.Create(args2) : null;
								playerUpgradesRegistry.AddUpgrade(playerUpgrade3);
							}
						}
					}
				}
				playerBehaviour.UpgradesRegistry = playerUpgradesRegistry;
			}
			GameObject gameObject;
			LocationDependentObjectAttacher.TryAttachObject(this.locationDependentData, playerBehaviour.gameObject, playerBehaviour.CurrentGame, out gameObject);
		}

		// Token: 0x06000851 RID: 2129 RVA: 0x0001BE70 File Offset: 0x0001A070
		public PlayerFactory(LocationDependentObjectAttacher.Data[] locationDependentData) : base(null)
		{
			this.locationDependentData = locationDependentData;
		}

		// Token: 0x06000852 RID: 2130 RVA: 0x0001BE80 File Offset: 0x0001A080
		public override void Initialize(IGame game)
		{
			base.Initialize(game);
			game.Services.TryGet<IGameMobsFactory>(out this.mobsFactory);
			game.Services.TryGet<PlayerProfileManager>(out this.profileManager);
			game.Services.TryGet<IPlayerUpgradesFactory>(out this.upgradesFactory);
			game.Services.TryGet<IPlayerUpgradesStoreManager>(out this.upgradesStore);
			game.Services.TryGet<PurchaseManager>(out this.purchaseManager);
		}

		// Token: 0x06000853 RID: 2131 RVA: 0x0001BEEE File Offset: 0x0001A0EE
		public PlayerBehaviour Create(PlayerFactoryArgs args)
		{
			return (PlayerBehaviour)this.Create(base.GetObjectPrototype(args.ObjectID), args);
		}

		// Token: 0x06000854 RID: 2132 RVA: 0x0001BF08 File Offset: 0x0001A108
		PlayerBehaviour IFactory<IBaseObjectDescription, PlayerBehaviour>.Create(IBaseObjectDescription args)
		{
			return (PlayerBehaviour)base.Create(args);
		}

		// Token: 0x06000856 RID: 2134 RVA: 0x0001BF28 File Offset: 0x0001A128
		[CompilerGenerated]
		internal static async void <CreateNecromancyTargetsWatcher>g__StartNecromancyTargetsWatcherSetupTask|4_0(PlayerBehaviour targetPlayer, RevivableMobsAreaWatcher watcher)
		{
			await Task.Yield();
			if (!GameApplication.IsGameStateChanging && targetPlayer != null)
			{
				GameAbilitiesController abilitiesController = targetPlayer.AbilitiesController;
				int num;
				BaseAbility baseAbility = (BaseAbility)((abilitiesController != null) ? abilitiesController.GetAbilityByID(3, out num) : null);
				if (baseAbility != null)
				{
					((CircleCollider2D)watcher.AreaCollider).radius = baseAbility.Range;
					watcher.observableLayers = baseAbility.ValidObjectLayers;
					watcher.ParentAbility = baseAbility;
					watcher.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x06000857 RID: 2135 RVA: 0x0001BF6C File Offset: 0x0001A16C
		[CompilerGenerated]
		internal static async void <CreateResourcesWatcher>g__PassAbilitiesSource|5_0(PlayerBehaviour targetPlayer, AbilityResourcesWatcher watcher)
		{
			await Task.Yield();
			if (targetPlayer != null && targetPlayer.AbilitiesController != null)
			{
				watcher.AbilitiesSource = targetPlayer.AbilitiesController;
			}
		}

		// Token: 0x040004B3 RID: 1203
		private const int IgnoreRaycastLayer = 2;

		// Token: 0x040004B4 RID: 1204
		public static readonly int PlayerLayer = LayerMask.NameToLayer("Player");

		// Token: 0x040004B5 RID: 1205
		private readonly LocationDependentObjectAttacher.Data[] locationDependentData;

		// Token: 0x040004B6 RID: 1206
		private PlayerProfileManager profileManager;

		// Token: 0x040004B7 RID: 1207
		private IGameMobsFactory mobsFactory;

		// Token: 0x040004B8 RID: 1208
		private IPlayerUpgradesFactory upgradesFactory;

		// Token: 0x040004B9 RID: 1209
		private IPlayerUpgradesStoreManager upgradesStore;

		// Token: 0x040004BA RID: 1210
		private PurchaseManager purchaseManager;
	}
}
