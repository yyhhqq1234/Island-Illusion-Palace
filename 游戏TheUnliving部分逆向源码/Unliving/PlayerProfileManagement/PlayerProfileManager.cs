using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Common.RestorableState;
using Common.ServiceRegistry;
using Game.Core;
using Game.Serialization;
using UnityEngine;
using Unliving.Achievements;
using Unliving.Currencies;
using Unliving.GameSession.DifficultyAdvancing;
using Unliving.GameSession.PlayerLeveling;
using Unliving.GameSettings;
using Unliving.Player;
using Unliving.Player.Cheats;
using Unliving.Player.Upgrades;
using Unliving.PlayerUpgradesStore;
using Unliving.Plot;
using Unliving.Plot.Milestones;
using Unliving.Purchasing;
using Unliving.Statistics;
using Unliving.Tutorial;

namespace Unliving.PlayerProfileManagement
{
	// Token: 0x02000122 RID: 290
	[CreateAssetMenu(fileName = "PlayerProfileManager", menuName = "Game/Global/Player Profile Manager")]
	public sealed class PlayerProfileManager : GlobalManagerBase
	{
		// Token: 0x1700012B RID: 299
		// (get) Token: 0x06000728 RID: 1832 RVA: 0x0001703B File Offset: 0x0001523B
		public PlayerProfile CurrentPlayerProfile
		{
			get
			{
				return this.currentPlayerProfile;
			}
		}

		// Token: 0x1700012C RID: 300
		// (get) Token: 0x06000729 RID: 1833 RVA: 0x00017043 File Offset: 0x00015243
		public PlayerProfile DefaultPlayerProfile
		{
			get
			{
				return this.defaultPlayerProfile;
			}
		}

		// Token: 0x14000043 RID: 67
		// (add) Token: 0x0600072A RID: 1834 RVA: 0x0001704C File Offset: 0x0001524C
		// (remove) Token: 0x0600072B RID: 1835 RVA: 0x00017084 File Offset: 0x00015284
		public event Action ProfileSavingStarted;

		// Token: 0x14000044 RID: 68
		// (add) Token: 0x0600072C RID: 1836 RVA: 0x000170BC File Offset: 0x000152BC
		// (remove) Token: 0x0600072D RID: 1837 RVA: 0x000170F4 File Offset: 0x000152F4
		public event Action ProfileSavingFinalized;

		// Token: 0x14000045 RID: 69
		// (add) Token: 0x0600072E RID: 1838 RVA: 0x0001712C File Offset: 0x0001532C
		// (remove) Token: 0x0600072F RID: 1839 RVA: 0x00017164 File Offset: 0x00015364
		public event Action<PlayerProfile> ProfileLoaded;

		// Token: 0x06000730 RID: 1840 RVA: 0x00017199 File Offset: 0x00015399
		public PlayerLevelRuntimeState GetCurrentPlayerLevelRuntimeState()
		{
			if (this.CurrentPlayerProfile.playerLevelState == null)
			{
				this.CurrentPlayerProfile.playerLevelState = this.DefaultPlayerProfile.playerLevelState.Clone();
			}
			return this.CurrentPlayerProfile.playerLevelState;
		}

		// Token: 0x06000731 RID: 1841 RVA: 0x000171CE File Offset: 0x000153CE
		private string GetCurrentPlayerProfileFileName()
		{
			return this.currentPlayerProfile.name + GameApplicationSettings.GetSerializationFileExtension();
		}

		// Token: 0x06000732 RID: 1842 RVA: 0x000171E5 File Offset: 0x000153E5
		private string GetPlayerProfilesFolderPath()
		{
			return GameApplication.GetDataPath("Saves");
		}

		// Token: 0x06000733 RID: 1843 RVA: 0x000171F1 File Offset: 0x000153F1
		private string GetCurrentProfileFilePath()
		{
			return GameApplication.GetDataPath(Path.Combine("Saves", this.GetCurrentPlayerProfileFileName()));
		}

		// Token: 0x06000734 RID: 1844 RVA: 0x00017208 File Offset: 0x00015408
		private List<string> GetSavedPlayerProfilePaths()
		{
			string playerProfilesFolderPath = this.GetPlayerProfilesFolderPath();
			if (!Directory.Exists(playerProfilesFolderPath))
			{
				return new List<string>(0);
			}
			List<string> list = new List<string>(10);
			string serializationFileExtension = GameApplicationSettings.GetSerializationFileExtension();
			foreach (string text in Directory.EnumerateFiles(playerProfilesFolderPath))
			{
				if (string.Equals(Path.GetExtension(text), serializationFileExtension))
				{
					list.Add(text);
				}
			}
			return list;
		}

		// Token: 0x06000735 RID: 1845 RVA: 0x0001728C File Offset: 0x0001548C
		public PlayerProfile LoadPlayerProfileWithName(string profileName)
		{
			List<string> savedPlayerProfilePaths = this.GetSavedPlayerProfilePaths();
			if (!string.IsNullOrEmpty(profileName))
			{
				for (int i = 0; i < savedPlayerProfilePaths.Count; i++)
				{
					if (profileName == Path.GetFileNameWithoutExtension(savedPlayerProfilePaths[i]))
					{
						try
						{
							return this.LoadPlayerProfile(savedPlayerProfilePaths[i], this.defaultPlayerProfile);
						}
						catch
						{
							return null;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06000736 RID: 1846 RVA: 0x000172FC File Offset: 0x000154FC
		public bool DeleteProfile(string profileName)
		{
			string playerProfilesFolderPath = this.GetPlayerProfilesFolderPath();
			string serializationFileExtension = GameApplicationSettings.GetSerializationFileExtension();
			string path = Path.Combine(playerProfilesFolderPath, profileName + serializationFileExtension);
			if (File.Exists(path))
			{
				File.Delete(path);
				return true;
			}
			return false;
		}

		// Token: 0x06000737 RID: 1847 RVA: 0x00017334 File Offset: 0x00015534
		private PlayerProfile LoadPlayerProfile(string profilePath, PlayerProfile defaultPlayerProfile)
		{
			PlayerProfile result;
			try
			{
				if (Path.GetExtension(profilePath) == ".sav")
				{
					result = UnitySerializationUtility.FromBsonFile<PlayerProfile>(profilePath);
				}
				else
				{
					result = UnitySerializationUtility.FromJsonFile<PlayerProfile>(profilePath);
				}
			}
			catch (Exception)
			{
				return defaultPlayerProfile.Clone();
			}
			return result;
		}

		// Token: 0x06000738 RID: 1848 RVA: 0x00017384 File Offset: 0x00015584
		public void LoadCurrentPlayerProfile()
		{
			this.currentPlayerProfile = this.LoadPlayerProfileWithName(this.gameSettingsManager.CurrentPlayerProfileName);
			if (this.currentPlayerProfile == null)
			{
				this.ResetPlayerProfileFullProgress();
			}
			this.currentPlayerProfile.name = this.gameSettingsManager.CurrentPlayerProfileName;
			this.LoadDifficultyLevel();
			Action<PlayerProfile> profileLoaded = this.ProfileLoaded;
			if (profileLoaded == null)
			{
				return;
			}
			profileLoaded(this.currentPlayerProfile);
		}

		// Token: 0x06000739 RID: 1849 RVA: 0x000173E8 File Offset: 0x000155E8
		private void UpdateGlobalPlayerProfileData()
		{
			IServiceRegistry services = base.CurrentGame.Services;
			this.UpdatePlayerPurchasements(services.Get<PurchaseManager>());
			this.UpdatePlayerStatistics(services.Get<PlayerStatisticsManager>());
			this.UpdatePlayerMilestones(services.Get<PlotMilestoneManager>());
			this.UpdateLocationsDescriptionsState(services.Get<LocationDescriptionManager>());
			this.UpdatePlayerAchievements(services.Get<PlayerAchievementsManager>());
			this.UpdateMobsTutorial(services.Get<MobsTutorialManager>());
			this.UpdateTutorialHintsState(services.Get<TutorialHintsManager>());
			this.UpdateSelectedCheats(services.Get<CheatManager>());
			this.UpdateDifficultyLevel();
		}

		// Token: 0x0600073A RID: 1850 RVA: 0x00017467 File Offset: 0x00015667
		public void UpdatePlayerPurchasements(PurchaseManager purchaseManager)
		{
			this.currentPlayerProfile.purchaseState.Store(purchaseManager);
		}

		// Token: 0x0600073B RID: 1851 RVA: 0x0001747A File Offset: 0x0001567A
		public void UpdatePlayerStatistics(PlayerStatisticsManager statisticsManager)
		{
			this.currentPlayerProfile.statisticsState.Store(statisticsManager);
		}

		// Token: 0x0600073C RID: 1852 RVA: 0x0001748D File Offset: 0x0001568D
		public void UpdatePlayerMilestones(PlotMilestoneManager milestoneManager)
		{
			this.currentPlayerProfile.milestonesState.Store(milestoneManager);
		}

		// Token: 0x0600073D RID: 1853 RVA: 0x000174A0 File Offset: 0x000156A0
		public void UpdatePlayerAchievements(PlayerAchievementsManager achievementsManager)
		{
			this.currentPlayerProfile.achievementsState.Store(achievementsManager);
		}

		// Token: 0x0600073E RID: 1854 RVA: 0x000174B3 File Offset: 0x000156B3
		public void UpdateMobsTutorial(MobsTutorialManager mobsTutorialManager)
		{
			this.currentPlayerProfile.mobsTutorialState.Store(mobsTutorialManager);
		}

		// Token: 0x0600073F RID: 1855 RVA: 0x000174C6 File Offset: 0x000156C6
		public void UpdateTutorialHintsState(TutorialHintsManager tutorialHintsManager)
		{
			this.currentPlayerProfile.tutorialHintsState.Store(tutorialHintsManager);
		}

		// Token: 0x06000740 RID: 1856 RVA: 0x000174D9 File Offset: 0x000156D9
		public void UpdatePurchasedPlayerUpgrades(IPlayerUpgradesStoreManager upgradesStore)
		{
			this.currentPlayerProfile.purchasedUpgradesState.Store(upgradesStore);
		}

		// Token: 0x06000741 RID: 1857 RVA: 0x000174EC File Offset: 0x000156EC
		public void UpdateLocationsDescriptionsState(LocationDescriptionManager locationDescriptionManager)
		{
			this.currentPlayerProfile.locationDescriptionState.Store(locationDescriptionManager);
		}

		// Token: 0x06000742 RID: 1858 RVA: 0x000174FF File Offset: 0x000156FF
		public void UpdateSelectedPlayerUpgrades(IEnumerable<PlayerUpgradeInfo> selectedUpgrades)
		{
			this.currentPlayerProfile.playerCharacterState.UpdatePlayerUpgrades(selectedUpgrades);
		}

		// Token: 0x06000743 RID: 1859 RVA: 0x00017512 File Offset: 0x00015712
		public void UpdateSelectedCheats(CheatManager cheatManager)
		{
			this.currentPlayerProfile.selectedCheatsState.Store(cheatManager);
		}

		// Token: 0x06000744 RID: 1860 RVA: 0x00017528 File Offset: 0x00015728
		public void UpdateDifficultyLevel()
		{
			DifficultyLevelManager difficultyLevelManager;
			if (base.CurrentGame.Services.TryGet<DifficultyLevelManager>(out difficultyLevelManager))
			{
				this.currentPlayerProfile.currentDifficultyLevel = difficultyLevelManager.CurrentDifficultyLevel;
			}
		}

		// Token: 0x06000745 RID: 1861 RVA: 0x0001755A File Offset: 0x0001575A
		public void LoadPlayerPurchasements(PurchaseManager purchaseManager)
		{
			this.currentPlayerProfile.purchaseState.Restore(purchaseManager, null);
		}

		// Token: 0x06000746 RID: 1862 RVA: 0x0001756E File Offset: 0x0001576E
		public void LoadPlayerStatistics(PlayerStatisticsManager statisticsManager)
		{
			this.currentPlayerProfile.statisticsState.Restore(statisticsManager, null);
		}

		// Token: 0x06000747 RID: 1863 RVA: 0x00017582 File Offset: 0x00015782
		public void LoadPlayerMilestones(PlotMilestoneManager milestoneManager)
		{
			this.currentPlayerProfile.milestonesState.Restore(milestoneManager, null);
		}

		// Token: 0x06000748 RID: 1864 RVA: 0x00017596 File Offset: 0x00015796
		public void LoadPlayerAchievements(PlayerAchievementsManager achievementsManager)
		{
			this.currentPlayerProfile.achievementsState.Restore(achievementsManager, null);
		}

		// Token: 0x06000749 RID: 1865 RVA: 0x000175AA File Offset: 0x000157AA
		public void LoadMobsTutorial(MobsTutorialManager mobsTutorialManager)
		{
			this.currentPlayerProfile.mobsTutorialState.Restore(mobsTutorialManager, null);
		}

		// Token: 0x0600074A RID: 1866 RVA: 0x000175BE File Offset: 0x000157BE
		public void LoadTutorialHintsState(TutorialHintsManager tutorialHintsManager)
		{
			this.currentPlayerProfile.tutorialHintsState.Restore(tutorialHintsManager, null);
		}

		// Token: 0x0600074B RID: 1867 RVA: 0x000175D2 File Offset: 0x000157D2
		public void LoadLocationsDescriptionsState(LocationDescriptionManager locationDescriptionManager)
		{
			this.currentPlayerProfile.locationDescriptionState.Restore(locationDescriptionManager, null);
		}

		// Token: 0x0600074C RID: 1868 RVA: 0x000175E6 File Offset: 0x000157E6
		public void LoadSelectedCheats(CheatManager cheatManager)
		{
			this.currentPlayerProfile.selectedCheatsState.Restore(cheatManager, null);
		}

		// Token: 0x0600074D RID: 1869 RVA: 0x000175FC File Offset: 0x000157FC
		private void LoadDifficultyLevel()
		{
			DifficultyLevelManager difficultyLevelManager;
			if (base.CurrentGame.Services.TryGet<DifficultyLevelManager>(out difficultyLevelManager))
			{
				difficultyLevelManager.CurrentDifficultyLevel = Mathf.Max(this.currentPlayerProfile.currentDifficultyLevel, 1);
			}
		}

		// Token: 0x0600074E RID: 1870 RVA: 0x00017634 File Offset: 0x00015834
		public void LoadLastGamerunProfileState()
		{
			if (this.currentPlayerProfile == null)
			{
				return;
			}
			PlayerProfileManager.RestorableState.RestorableStateArgs args = new PlayerProfileManager.RestorableState.RestorableStateArgs
			{
				restoreOnlyCurrencies = true
			};
			PlayerProfileManager.RestorableState lastGamerunProfileInfo = this.currentPlayerProfile.lastGamerunProfileInfo;
			if (lastGamerunProfileInfo == null)
			{
				return;
			}
			lastGamerunProfileInfo.Restore(this, args);
		}

		// Token: 0x0600074F RID: 1871 RVA: 0x00017670 File Offset: 0x00015870
		public void LoadPurchasedPlayerUpgrades(IPlayerUpgradesStoreManager upgradesStore)
		{
			IReadOnlyList<PlayerUpgradeInfo> upgrades = this.currentPlayerProfile.playerCharacterState.Upgrades;
			this.currentPlayerProfile.purchasedUpgradesState.Restore(upgradesStore, upgrades);
		}

		// Token: 0x06000750 RID: 1872 RVA: 0x000176A0 File Offset: 0x000158A0
		public void UpdateLocalPlayerProgress(PlayerBehaviour playerCharacter)
		{
			this.currentPlayerProfile.lastSceneInfo.Store(playerCharacter);
			this.currentPlayerProfile.playerCharacterState.Store(playerCharacter);
			PlayerProfile playerProfile = this.currentPlayerProfile;
			if (playerProfile.lastGamerunProfileInfo == null)
			{
				playerProfile.lastGamerunProfileInfo = new PlayerProfileManager.RestorableState();
			}
			this.currentPlayerProfile.lastGamerunProfileInfo.Store(this);
		}

		// Token: 0x06000751 RID: 1873 RVA: 0x000176FC File Offset: 0x000158FC
		public void ResetLocalPlayerProgress()
		{
			PlayerProfileManager.RestorableState lastGamerunProfileInfo = this.currentPlayerProfile.lastGamerunProfileInfo;
			if (lastGamerunProfileInfo != null)
			{
				lastGamerunProfileInfo.Reset();
			}
			this.currentPlayerProfile.lastSceneInfo.Reset();
			this.currentPlayerProfile.ResetCurrencyLocalProgress();
			this.currentPlayerProfile.playerCharacterState.ResetLocalProgress(this.defaultPlayerProfile.playerCharacterState);
			PlayerLevelRuntimeState playerLevelState = this.currentPlayerProfile.playerLevelState;
			if (playerLevelState == null)
			{
				return;
			}
			playerLevelState.Reset();
		}

		// Token: 0x06000752 RID: 1874 RVA: 0x0001776A File Offset: 0x0001596A
		public void DeleteCurrentPlayerProfile()
		{
			this.DeleteProfile(this.currentPlayerProfile.name);
		}

		// Token: 0x06000753 RID: 1875 RVA: 0x0001777E File Offset: 0x0001597E
		public void ResetPlayerProfileFullProgress()
		{
			this.currentPlayerProfile = this.defaultPlayerProfile.Clone();
		}

		// Token: 0x06000754 RID: 1876 RVA: 0x00017794 File Offset: 0x00015994
		public void SavePlayerProfile(bool updateLastSessionTime)
		{
			this.UpdateGlobalPlayerProfileData();
			if (updateLastSessionTime)
			{
				this.currentPlayerProfile.UpdateLastSessionTime();
			}
			Action profileSavingStarted = this.ProfileSavingStarted;
			if (profileSavingStarted != null)
			{
				profileSavingStarted();
			}
			string currentProfileFilePath = this.GetCurrentProfileFilePath();
			string directoryName = Path.GetDirectoryName(currentProfileFilePath);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (GameApplicationSettings.UseBinarySerialization())
			{
				UnitySerializationUtility.ToBsonFile(this.currentPlayerProfile, currentProfileFilePath);
			}
			else
			{
				UnitySerializationUtility.ToJsonFile(this.currentPlayerProfile, currentProfileFilePath);
			}
			Action profileSavingFinalized = this.ProfileSavingFinalized;
			if (profileSavingFinalized == null)
			{
				return;
			}
			profileSavingFinalized();
		}

		// Token: 0x06000755 RID: 1877 RVA: 0x00017814 File Offset: 0x00015A14
		public void SavePlayerProfile(PlayerBehaviour playerToUpdateLocalProgress)
		{
			if (playerToUpdateLocalProgress != null)
			{
				this.UpdateLocalPlayerProgress(playerToUpdateLocalProgress);
			}
			else
			{
				this.ResetLocalPlayerProgress();
			}
			this.SavePlayerProfile(true);
		}

		// Token: 0x06000756 RID: 1878 RVA: 0x00017835 File Offset: 0x00015A35
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.gameSettingsManager = currentGame.Services.Get<GameSettingsManager>();
			this.LoadCurrentPlayerProfile();
		}

		// Token: 0x04000450 RID: 1104
		[SerializeField]
		private PlayerProfile defaultPlayerProfile = new PlayerProfile
		{
			name = "Player"
		};

		// Token: 0x04000451 RID: 1105
		private PlayerProfile currentPlayerProfile;

		// Token: 0x04000452 RID: 1106
		private GameSettingsManager gameSettingsManager;

		// Token: 0x02000436 RID: 1078
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PlayerProfileManager>, ICloneable<PlayerProfileManager.RestorableState>
		{
			// Token: 0x060022C4 RID: 8900 RVA: 0x0006C3CD File Offset: 0x0006A5CD
			public RestorableState() : base(null)
			{
			}

			// Token: 0x060022C5 RID: 8901 RVA: 0x0006C3DD File Offset: 0x0006A5DD
			public RestorableState(PlayerProfileManager profileManager) : base(profileManager)
			{
			}

			// Token: 0x060022C6 RID: 8902 RVA: 0x0006C3F0 File Offset: 0x0006A5F0
			public PlayerProfileManager.RestorableState Clone()
			{
				PlayerProfileManager.RestorableState restorableState = new PlayerProfileManager.RestorableState();
				restorableState.version = this.version;
				restorableState.name = this.name;
				restorableState.hasTutorial = this.hasTutorial;
				restorableState.currencies = this.CloneCurrenciesArray(this.currencies);
				PlayerLevelRuntimeState playerLevelRuntimeState = this.playerLevelState;
				restorableState.playerLevelState = ((playerLevelRuntimeState != null) ? playerLevelRuntimeState.Clone() : null);
				restorableState.lastSessionTime = this.lastSessionTime;
				return restorableState;
			}

			// Token: 0x060022C7 RID: 8903 RVA: 0x0006C45C File Offset: 0x0006A65C
			public override void Restore(PlayerProfileManager profileManager, object args = null)
			{
				PlayerProfile currentPlayerProfile = profileManager.CurrentPlayerProfile;
				if (currentPlayerProfile == null)
				{
					return;
				}
				currentPlayerProfile.currencies = this.CloneCurrenciesArray(this.currencies);
				PlayerProfile playerProfile = currentPlayerProfile;
				PlayerLevelRuntimeState playerLevelRuntimeState = this.playerLevelState;
				playerProfile.playerLevelState = ((playerLevelRuntimeState != null) ? playerLevelRuntimeState.Clone() : null);
				PlayerProfileManager.RestorableState.RestorableStateArgs restorableStateArgs = args as PlayerProfileManager.RestorableState.RestorableStateArgs;
				if (restorableStateArgs != null && restorableStateArgs.restoreOnlyCurrencies)
				{
					return;
				}
				currentPlayerProfile.version = this.version;
				currentPlayerProfile.name = this.name;
				currentPlayerProfile.hasTutorial = this.hasTutorial;
				currentPlayerProfile.lastSessionTime = this.lastSessionTime;
			}

			// Token: 0x060022C8 RID: 8904 RVA: 0x0006C4E4 File Offset: 0x0006A6E4
			public override void Store(PlayerProfileManager profileManager)
			{
				PlayerProfile currentPlayerProfile = profileManager.CurrentPlayerProfile;
				if (currentPlayerProfile == null)
				{
					return;
				}
				this.version = currentPlayerProfile.version;
				this.name = currentPlayerProfile.name;
				this.hasTutorial = currentPlayerProfile.hasTutorial;
				this.currencies = this.CloneCurrenciesArray(currentPlayerProfile.currencies);
				PlayerLevelRuntimeState playerLevelRuntimeState = currentPlayerProfile.playerLevelState;
				this.playerLevelState = ((playerLevelRuntimeState != null) ? playerLevelRuntimeState.Clone() : null);
				DateTime? dateTime = currentPlayerProfile.lastSessionTime;
				this.lastSessionTime = ((dateTime != null) ? dateTime : null);
			}

			// Token: 0x060022C9 RID: 8905 RVA: 0x0006C56C File Offset: 0x0006A76C
			public void Reset()
			{
				this.version = string.Empty;
				this.name = string.Empty;
				this.hasTutorial = true;
				this.currencies = null;
				this.playerLevelState = null;
				this.lastSessionTime = null;
			}

			// Token: 0x060022CA RID: 8906 RVA: 0x0006C5A8 File Offset: 0x0006A7A8
			private ICurrency[] CloneCurrenciesArray(ICurrency[] currencies)
			{
				if (currencies == null)
				{
					return new ICurrency[0];
				}
				ICurrency[] array = new ICurrency[currencies.Length];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = currencies[i].Clone();
				}
				return array;
			}

			// Token: 0x04001642 RID: 5698
			public string version;

			// Token: 0x04001643 RID: 5699
			public string name;

			// Token: 0x04001644 RID: 5700
			public bool hasTutorial = true;

			// Token: 0x04001645 RID: 5701
			public ICurrency[] currencies;

			// Token: 0x04001646 RID: 5702
			[HideInInspector]
			public PlayerLevelRuntimeState playerLevelState;

			// Token: 0x04001647 RID: 5703
			public DateTime? lastSessionTime;

			// Token: 0x020005A5 RID: 1445
			[Serializable]
			public sealed class RestorableStateArgs
			{
				// Token: 0x04001D0D RID: 7437
				public bool restoreOnlyCurrencies;
			}
		}
	}
}
