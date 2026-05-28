using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Common;
using Common.Editor;
using Newtonsoft.Json;
using UnityEngine;
using Unliving.Achievements;
using Unliving.Currencies;
using Unliving.GameScene;
using Unliving.GameSession.PlayerLeveling;
using Unliving.LevelGeneration;
using Unliving.Player;
using Unliving.Player.Cheats;
using Unliving.PlayerUpgradesStore;
using Unliving.Plot;
using Unliving.Plot.Milestones;
using Unliving.Purchasing;
using Unliving.Statistics;
using Unliving.Tutorial;

namespace Unliving.PlayerProfileManagement
{
	// Token: 0x0200011F RID: 287
	[Serializable]
	public sealed class PlayerProfile : ICloneable<PlayerProfile>
	{
		// Token: 0x17000128 RID: 296
		// (get) Token: 0x06000711 RID: 1809 RVA: 0x000169E0 File Offset: 0x00014BE0
		public bool HasLastSessionTime
		{
			get
			{
				return this.lastSessionTime != null;
			}
		}

		// Token: 0x17000129 RID: 297
		// (get) Token: 0x06000712 RID: 1810 RVA: 0x000169ED File Offset: 0x00014BED
		public bool LaunchTutorial
		{
			get
			{
				return this.hasTutorial && !this.HasLastSessionTime;
			}
		}

		// Token: 0x14000040 RID: 64
		// (add) Token: 0x06000713 RID: 1811 RVA: 0x00016A04 File Offset: 0x00014C04
		// (remove) Token: 0x06000714 RID: 1812 RVA: 0x00016A3C File Offset: 0x00014C3C
		public event Action<ICurrencyOperationArgs> PerformingCurrencyOperation;

		// Token: 0x14000041 RID: 65
		// (add) Token: 0x06000715 RID: 1813 RVA: 0x00016A74 File Offset: 0x00014C74
		// (remove) Token: 0x06000716 RID: 1814 RVA: 0x00016AAC File Offset: 0x00014CAC
		public event Action<ICurrencyOperationArgs> CurrencyOperationSucceed;

		// Token: 0x14000042 RID: 66
		// (add) Token: 0x06000717 RID: 1815 RVA: 0x00016AE4 File Offset: 0x00014CE4
		// (remove) Token: 0x06000718 RID: 1816 RVA: 0x00016B1C File Offset: 0x00014D1C
		public event Action<ICurrencyOperationArgs> CurrencyOperationFailed;

		// Token: 0x06000719 RID: 1817 RVA: 0x00016B54 File Offset: 0x00014D54
		private ICurrency CreateCurrencyInstance(ICurrencyOperationArgs args)
		{
			ICurrency currency = null;
			switch (args.CurrencyID)
			{
			case CurrencyID.Gold:
				currency = default(Gold);
				break;
			case CurrencyID.Meta:
				currency = default(Meta);
				break;
			case CurrencyID.Ash:
				currency = default(Ash);
				break;
			case CurrencyID.Prima:
				currency = default(Prima);
				break;
			case CurrencyID.Cinder:
				currency = default(Cinder);
				break;
			case CurrencyID.RerollCurrency:
				currency = default(RerollCurrency);
				break;
			}
			currency.Amount = args.Amount;
			return currency;
		}

		// Token: 0x0600071A RID: 1818 RVA: 0x00016C04 File Offset: 0x00014E04
		private int GetDifficultyLevelRewardStateIndex(int difficultyLevel, GameLocation.TypeID locationID)
		{
			if (this.gameDifficultyLevelRewardsState == null)
			{
				return -1;
			}
			return this.gameDifficultyLevelRewardsState.FindIndex((PlayerProfileGameDifficultyRewardState state) => state.DifficultyLevel == difficultyLevel && state.CompletedLocationID == locationID);
		}

		// Token: 0x0600071B RID: 1819 RVA: 0x00016C46 File Offset: 0x00014E46
		public PlayerProfile()
		{
		}

		// Token: 0x0600071C RID: 1820 RVA: 0x00016C58 File Offset: 0x00014E58
		public PlayerProfile(PlayerProfile otherProfile)
		{
			this.name = otherProfile.name;
			this.version = otherProfile.version;
			this.currencies = (from c in otherProfile.currencies
			select c.Clone()).ToArray<ICurrency>();
			this.hasTutorial = otherProfile.hasTutorial;
			this.currentDifficultyLevel = otherProfile.currentDifficultyLevel;
			this.playerCharacterState = otherProfile.playerCharacterState.Clone();
			this.playerLevelState = otherProfile.playerLevelState.Clone();
			this.lastSceneInfo = otherProfile.lastSceneInfo.Clone();
			this.lastGamerunProfileInfo = otherProfile.lastGamerunProfileInfo.Clone();
			this.purchaseState = otherProfile.purchaseState.Clone();
			this.purchasedUpgradesState = otherProfile.purchasedUpgradesState.Clone();
			this.statisticsState = otherProfile.statisticsState.Clone();
			this.milestonesState = otherProfile.milestonesState.Clone();
			this.achievementsState = otherProfile.achievementsState.Clone();
			this.mobsTutorialState = otherProfile.mobsTutorialState.Clone();
			this.tutorialHintsState = otherProfile.tutorialHintsState.Clone();
			this.gamePlotProgress = otherProfile.gamePlotProgress.Clone();
			this.locationDescriptionState = otherProfile.locationDescriptionState.Clone();
			this.gameDifficultyLevelRewardsState = new List<PlayerProfileGameDifficultyRewardState>(otherProfile.gameDifficultyLevelRewardsState);
			this.selectedCheatsState = otherProfile.selectedCheatsState.Clone();
		}

		// Token: 0x0600071D RID: 1821 RVA: 0x00016DD8 File Offset: 0x00014FD8
		public void ResetCurrencyLocalProgress()
		{
			for (int i = 0; i < this.currencies.Length; i++)
			{
				ICurrency currency = this.currencies[i];
				if (currency.LocalProgressCurrency)
				{
					currency.Amount = 0f;
				}
			}
		}

		// Token: 0x0600071E RID: 1822 RVA: 0x00016E14 File Offset: 0x00015014
		public int GetCurrencyAmount(CurrencyID currencyID)
		{
			for (int i = 0; i < this.currencies.Length; i++)
			{
				ICurrency currency = this.currencies[i];
				if (currency.CurrencyID == currencyID)
				{
					return Mathf.FloorToInt(currency.Amount);
				}
			}
			return 0;
		}

		// Token: 0x0600071F RID: 1823 RVA: 0x00016E54 File Offset: 0x00015054
		public bool TryExecuteCurrencyOperation(ICurrencyOperationArgs args)
		{
			if (args.Spending)
			{
				args.Amount = -Mathf.Abs(args.Amount);
			}
			int i = 0;
			while (i < this.currencies.Length)
			{
				ICurrency currency = this.currencies[i];
				if (currency.CurrencyID == args.CurrencyID)
				{
					Action<ICurrencyOperationArgs> performingCurrencyOperation = this.PerformingCurrencyOperation;
					if (performingCurrencyOperation != null)
					{
						performingCurrencyOperation(args);
					}
					float num = args.Amount + currency.Amount;
					if (num < 0f)
					{
						Action<ICurrencyOperationArgs> currencyOperationFailed = this.CurrencyOperationFailed;
						if (currencyOperationFailed != null)
						{
							currencyOperationFailed(args);
						}
						return false;
					}
					currency.Amount = num;
					Action<ICurrencyOperationArgs> currencyOperationSucceed = this.CurrencyOperationSucceed;
					if (currencyOperationSucceed != null)
					{
						currencyOperationSucceed(args);
					}
					return true;
				}
				else
				{
					i++;
				}
			}
			if (!args.Spending && args.CurrencyID != CurrencyID.None)
			{
				int num2 = this.currencies.Length;
				Array.Resize<ICurrency>(ref this.currencies, num2 + 1);
				this.currencies[num2] = this.CreateCurrencyInstance(args);
				Action<ICurrencyOperationArgs> currencyOperationSucceed2 = this.CurrencyOperationSucceed;
				if (currencyOperationSucceed2 != null)
				{
					currencyOperationSucceed2(args);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06000720 RID: 1824 RVA: 0x00016F47 File Offset: 0x00015147
		public bool CanContinueGame(out GameSceneManager.RestorableState lastSceneState)
		{
			if (this.lastSceneInfo.IsValid())
			{
				lastSceneState = this.lastSceneInfo;
				return true;
			}
			lastSceneState = null;
			return false;
		}

		// Token: 0x06000721 RID: 1825 RVA: 0x00016F64 File Offset: 0x00015164
		public void UpdateLastSessionTime()
		{
			this.lastSessionTime = new DateTime?(DateTime.Now);
		}

		// Token: 0x06000722 RID: 1826 RVA: 0x00016F78 File Offset: 0x00015178
		public bool TryGetDifficultyLevelRewardState(int difficultyLevel, GameLocation.TypeID locationID, out PlayerProfileGameDifficultyRewardState state)
		{
			int difficultyLevelRewardStateIndex = this.GetDifficultyLevelRewardStateIndex(difficultyLevel, locationID);
			if (difficultyLevelRewardStateIndex != -1)
			{
				state = this.gameDifficultyLevelRewardsState[difficultyLevelRewardStateIndex];
				return true;
			}
			state = default(PlayerProfileGameDifficultyRewardState);
			return false;
		}

		// Token: 0x06000723 RID: 1827 RVA: 0x00016FB0 File Offset: 0x000151B0
		public void UpdateDifficultyLevelRewardState(int difficultyLevel, GameLocation.TypeID locationID, bool takeReward)
		{
			int difficultyLevelRewardStateIndex = this.GetDifficultyLevelRewardStateIndex(difficultyLevel, locationID);
			PlayerProfileGameDifficultyRewardState playerProfileGameDifficultyRewardState = new PlayerProfileGameDifficultyRewardState(difficultyLevel, locationID)
			{
				isRewardTaken = takeReward
			};
			if (this.gameDifficultyLevelRewardsState == null)
			{
				this.gameDifficultyLevelRewardsState = new List<PlayerProfileGameDifficultyRewardState>(16);
			}
			if (difficultyLevelRewardStateIndex != -1)
			{
				this.gameDifficultyLevelRewardsState[difficultyLevelRewardStateIndex] = playerProfileGameDifficultyRewardState;
				return;
			}
			this.gameDifficultyLevelRewardsState.Add(playerProfileGameDifficultyRewardState);
		}

		// Token: 0x06000724 RID: 1828 RVA: 0x0001700C File Offset: 0x0001520C
		public PlayerProfile Clone()
		{
			return new PlayerProfile(this);
		}

		// Token: 0x04000435 RID: 1077
		public string version;

		// Token: 0x04000436 RID: 1078
		public string name;

		// Token: 0x04000437 RID: 1079
		public bool hasTutorial = true;

		// Token: 0x04000438 RID: 1080
		[SerializeReference]
		[ManagedObjectField(typeof(ICurrency))]
		public ICurrency[] currencies;

		// Token: 0x04000439 RID: 1081
		public DateTime? lastSessionTime;

		// Token: 0x0400043A RID: 1082
		[OptionalField]
		public int currentDifficultyLevel;

		// Token: 0x0400043B RID: 1083
		public PlayerBehaviour.RestorableState playerCharacterState;

		// Token: 0x0400043C RID: 1084
		[JsonIgnore]
		public PlayerLevelRuntimeState playerLevelState;

		// Token: 0x0400043D RID: 1085
		public PurchaseManager.RestorableState purchaseState;

		// Token: 0x0400043E RID: 1086
		public PlayerUpgradesRestorableState purchasedUpgradesState;

		// Token: 0x0400043F RID: 1087
		public PlayerStatisticsManager.RestorableState statisticsState;

		// Token: 0x04000440 RID: 1088
		public PlotMilestoneManager.RestorableState milestonesState;

		// Token: 0x04000441 RID: 1089
		public PlayerAchievementsManager.RestorableState achievementsState;

		// Token: 0x04000442 RID: 1090
		public MobsTutorialManager.RestorableState mobsTutorialState;

		// Token: 0x04000443 RID: 1091
		public TutorialHintsManager.RestorableState tutorialHintsState;

		// Token: 0x04000444 RID: 1092
		public PlayerProfilePlotProgress gamePlotProgress;

		// Token: 0x04000445 RID: 1093
		public GameSceneManager.RestorableState lastSceneInfo;

		// Token: 0x04000446 RID: 1094
		public PlayerProfileManager.RestorableState lastGamerunProfileInfo;

		// Token: 0x04000447 RID: 1095
		public LocationDescriptionManager.RestorableState locationDescriptionState;

		// Token: 0x04000448 RID: 1096
		[SerializeField]
		private List<PlayerProfileGameDifficultyRewardState> gameDifficultyLevelRewardsState;

		// Token: 0x04000449 RID: 1097
		public CheatManagerRestorableState selectedCheatsState;
	}
}
