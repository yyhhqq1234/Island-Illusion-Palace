using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CollectionsExtensions;
using Common.RestorableState;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Factories;
using Game.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using Unliving.Abilities;
using Unliving.GameScene;
using Unliving.Mobs;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.Tutorial
{
	// Token: 0x0200002A RID: 42
	[CreateAssetMenu(fileName = "MobsTutorialManager", menuName = "Game/Tutorial/Mobs Manager")]
	public sealed class MobsTutorialManager : GlobalManagerBase
	{
		// Token: 0x1400001A RID: 26
		// (add) Token: 0x06000178 RID: 376 RVA: 0x0000653C File Offset: 0x0000473C
		// (remove) Token: 0x06000179 RID: 377 RVA: 0x00006574 File Offset: 0x00004774
		public event Action<MobBehaviour, MobsTutorialManager.MobLocalizedData> MobFirstTimeAddedInGroup;

		// Token: 0x0600017A RID: 378 RVA: 0x000065AC File Offset: 0x000047AC
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (!this.isActive)
			{
				return;
			}
			currentGame.Services.TryGet<LocalizationManager>(out this.localizationManager);
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
		}

		// Token: 0x0600017B RID: 379 RVA: 0x0000661B File Offset: 0x0000481B
		private void OnPlayerProfileLoaded(PlayerProfile obj)
		{
			this.profileManager.LoadMobsTutorial(this);
		}

		// Token: 0x0600017C RID: 380 RVA: 0x0000662C File Offset: 0x0000482C
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			base.OnSceneLoaded(loadedScene);
			if (!this.isActive)
			{
				return;
			}
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				this.playerProvider.PlayerRegistered += this.OnPlayerRegistered;
				if (!this.playerProvider.CurrentPlayer.IsNull())
				{
					this.OnPlayerRegistered(this.playerProvider.CurrentPlayer);
				}
			}
			if (this.profileManager != null)
			{
				this.profileManager.UpdateMobsTutorial(this);
			}
			GameSceneManager gameSceneManager;
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
		}

		// Token: 0x0600017D RID: 381 RVA: 0x000066DB File Offset: 0x000048DB
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			this.isTutorialLocation = sceneManager.GeneratedLocation.IsTutorialLocation;
		}

		// Token: 0x0600017E RID: 382 RVA: 0x000066F0 File Offset: 0x000048F0
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.Group.MobAdded -= this.OnPlayerGroupMobAdded;
			}
			this.currentPlayer = player;
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.Group.MobAdded += this.OnPlayerGroupMobAdded;
			}
		}

		// Token: 0x0600017F RID: 383 RVA: 0x00006758 File Offset: 0x00004958
		private void OnPlayerGroupMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (this.isTutorialLocation)
			{
				return;
			}
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour != null)
			{
				MobBehaviour.ID objectID = mobBehaviour.ObjectID;
				if (this.shownTutorialMobs.Contains(objectID) || this.ignoredMobs.Contains(objectID))
				{
					return;
				}
				Metadata metadata;
				if (this.localizationManager.TryGetMetadata<MobBehaviour.ID>(objectID, out metadata))
				{
					if (metadata.AdditionalText.Length < 4)
					{
						Debug.LogError(string.Format("Wrong metadada format for mob with ID: {0}", objectID));
						return;
					}
					MobsTutorialManager.MobLocalizedData arg = new MobsTutorialManager.MobLocalizedData
					{
						name = metadata.Title,
						type = metadata.AdditionalText[0],
						videoClip = Resources.Load<VideoClip>(metadata.AdditionalText[1]),
						mobDescription = metadata.AdditionalText[2],
						mobActivationDescription = metadata.AdditionalText[3]
					};
					BaseAbility baseAbility;
					MobActivationAbilityType activationAbilityType;
					if (mobBehaviour.TryGetMobActivationAbility(out baseAbility, out activationAbilityType))
					{
						arg.activationAbilityType = activationAbilityType;
					}
					Action<MobBehaviour, MobsTutorialManager.MobLocalizedData> mobFirstTimeAddedInGroup = this.MobFirstTimeAddedInGroup;
					if (mobFirstTimeAddedInGroup != null)
					{
						mobFirstTimeAddedInGroup(mobBehaviour, arg);
					}
					this.shownTutorialMobs.Add(objectID);
				}
			}
		}

		// Token: 0x06000180 RID: 384 RVA: 0x00006860 File Offset: 0x00004A60
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.playerProvider.IsNull())
			{
				this.playerProvider.PlayerRegistered -= this.OnPlayerRegistered;
			}
			if (!this.profileManager.IsNull())
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
		}

		// Token: 0x040000BA RID: 186
		[SerializeField]
		private bool isActive;

		// Token: 0x040000BB RID: 187
		[SerializeField]
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		private MobBehaviour.ID[] ignoredMobs;

		// Token: 0x040000BC RID: 188
		private LocalizationManager localizationManager;

		// Token: 0x040000BD RID: 189
		private IPlayerProvider playerProvider;

		// Token: 0x040000BE RID: 190
		private PlayerBehaviour currentPlayer;

		// Token: 0x040000BF RID: 191
		private PlayerProfileManager profileManager;

		// Token: 0x040000C0 RID: 192
		private List<MobBehaviour.ID> shownTutorialMobs = new List<MobBehaviour.ID>();

		// Token: 0x040000C1 RID: 193
		private bool isTutorialLocation;

		// Token: 0x02000409 RID: 1033
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<MobsTutorialManager>, ICloneable<MobsTutorialManager.RestorableState>
		{
			// Token: 0x0600223A RID: 8762 RVA: 0x0006A689 File Offset: 0x00068889
			public RestorableState() : base(null)
			{
			}

			// Token: 0x0600223B RID: 8763 RVA: 0x0006A692 File Offset: 0x00068892
			public RestorableState(MobsTutorialManager mobsTutorialManager) : base(mobsTutorialManager)
			{
			}

			// Token: 0x0600223C RID: 8764 RVA: 0x0006A69B File Offset: 0x0006889B
			public MobsTutorialManager.RestorableState Clone()
			{
				return new MobsTutorialManager.RestorableState
				{
					shownTutorialMobs = this.shownTutorialMobs.CloneArray<MobBehaviour.ID>()
				};
			}

			// Token: 0x0600223D RID: 8765 RVA: 0x0006A6B3 File Offset: 0x000688B3
			public override void Store(MobsTutorialManager mobsTutorialManager)
			{
				this.shownTutorialMobs = mobsTutorialManager.shownTutorialMobs.ToArray();
			}

			// Token: 0x0600223E RID: 8766 RVA: 0x0006A6C6 File Offset: 0x000688C6
			public override void Restore(MobsTutorialManager mobsTutorialManager, object args = null)
			{
				mobsTutorialManager.shownTutorialMobs = this.shownTutorialMobs.ToList<MobBehaviour.ID>();
			}

			// Token: 0x040015A6 RID: 5542
			public MobBehaviour.ID[] shownTutorialMobs;
		}

		// Token: 0x0200040A RID: 1034
		public struct MobLocalizedData
		{
			// Token: 0x040015A7 RID: 5543
			public string name;

			// Token: 0x040015A8 RID: 5544
			public string type;

			// Token: 0x040015A9 RID: 5545
			public string mobDescription;

			// Token: 0x040015AA RID: 5546
			public string mobActivationDescription;

			// Token: 0x040015AB RID: 5547
			public VideoClip videoClip;

			// Token: 0x040015AC RID: 5548
			public MobActivationAbilityType activationAbilityType;
		}
	}
}
