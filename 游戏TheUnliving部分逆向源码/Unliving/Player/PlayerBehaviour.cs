using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CollectionsExtensions;
using Common.DataBinding;
using Common.Editor;
using Common.Factories;
using Common.RestorableState;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using Game.Factories;
using Game.Gameplay;
using Game.InputManager;
using Game.LevelGeneration;
using Game.PassiveAbilities;
using Game.Stats;
using SpriteTrail;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Abilities;
using Unliving.AbilityResources;
using Unliving.Currencies;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.PassiveAbilities;
using Unliving.Pickables;
using Unliving.Player.TemporaryItemsStorage;
using Unliving.Player.Upgrades;
using Unliving.PlayerProfileManagement;
using Unliving.Purchasing;
using Unliving.Stores;

namespace Unliving.Player
{
	// Token: 0x0200013F RID: 319
	[DisallowMultipleComponent]
	[RequireComponent(typeof(CircleCollider2D))]
	[RequireComponent(typeof(Rigidbody2D))]
	[RequireComponent(typeof(Animator))]
	public sealed class PlayerBehaviour : BaseGameMob, PrototypeBasedFactory<PlayerBehaviour.FactoryPrototype, IGameMob>.IInitializableByFactory, IStaminaStatOwner, IGroupMobDamageFeedbackReceiver, ICurrencyOperationPerformer
	{
		// Token: 0x17000142 RID: 322
		// (get) Token: 0x06000801 RID: 2049 RVA: 0x0001A275 File Offset: 0x00018475
		// (set) Token: 0x06000802 RID: 2050 RVA: 0x0001A27D File Offset: 0x0001847D
		public PlayerBehaviour.ID ObjectID { get; set; }

		// Token: 0x17000143 RID: 323
		// (get) Token: 0x06000803 RID: 2051 RVA: 0x0001A286 File Offset: 0x00018486
		public PlayerMobsActivationControllerBase PlayerMobsActivationController
		{
			get
			{
				if (this.playerMobsActivationController == null)
				{
					this.playerMobsActivationController = new ClosestMobActivationController(this);
				}
				return this.playerMobsActivationController;
			}
		}

		// Token: 0x17000144 RID: 324
		// (get) Token: 0x06000804 RID: 2052 RVA: 0x0001A2A2 File Offset: 0x000184A2
		public IAimAssistController AimAssistController
		{
			get
			{
				return this.aimAssistController;
			}
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x06000805 RID: 2053 RVA: 0x0001A2AA File Offset: 0x000184AA
		public PlayerInputController PlayerInputController
		{
			get
			{
				if (this.playerInputController == null)
				{
					this.playerInputController = new PlayerInputController(this, base.CurrentGame);
				}
				return this.playerInputController;
			}
		}

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x06000806 RID: 2054 RVA: 0x0001A2CC File Offset: 0x000184CC
		public TemporaryItemsStorageController TempItemsStorageController
		{
			get
			{
				if (this.tempItemsStorageController == null)
				{
					this.tempItemsStorageController = new TemporaryItemsStorageController(this);
				}
				return this.tempItemsStorageController;
			}
		}

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x06000807 RID: 2055 RVA: 0x0001A2E8 File Offset: 0x000184E8
		// (set) Token: 0x06000808 RID: 2056 RVA: 0x0001A309 File Offset: 0x00018509
		public override GameMobsGroupControllerBase Group
		{
			get
			{
				if (this.group == null)
				{
					this.group = base.GetComponent<PlayerMobGroup>();
				}
				return this.group;
			}
			set
			{
			}
		}

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x06000809 RID: 2057 RVA: 0x0001A30B File Offset: 0x0001850B
		// (set) Token: 0x0600080A RID: 2058 RVA: 0x0001A313 File Offset: 0x00018513
		public IPlayerUpgradesRegistry UpgradesRegistry { get; internal set; }

		// Token: 0x17000149 RID: 329
		// (get) Token: 0x0600080B RID: 2059 RVA: 0x0001A31C File Offset: 0x0001851C
		public override bool IsCharacter
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700014A RID: 330
		// (get) Token: 0x0600080C RID: 2060 RVA: 0x0001A31F File Offset: 0x0001851F
		public override GameMobMotionControllerBase MotionController
		{
			get
			{
				return this.playerMotionController;
			}
		}

		// Token: 0x1700014B RID: 331
		// (get) Token: 0x0600080D RID: 2061 RVA: 0x0001A327 File Offset: 0x00018527
		public override GameMobAIController AIController
		{
			get
			{
				return null;
			}
		}

		// Token: 0x1700014C RID: 332
		// (get) Token: 0x0600080E RID: 2062 RVA: 0x0001A32A File Offset: 0x0001852A
		public override GameAbilitiesController AbilitiesController
		{
			get
			{
				return (GameAbilitiesController)this.abilitiesController;
			}
		}

		// Token: 0x1700014D RID: 333
		// (get) Token: 0x0600080F RID: 2063 RVA: 0x0001A337 File Offset: 0x00018537
		public override BasePassiveAbilitiesController PassiveAbilitiesController
		{
			get
			{
				return this.passiveAbilitiesController;
			}
		}

		// Token: 0x1700014E RID: 334
		// (get) Token: 0x06000810 RID: 2064 RVA: 0x0001A33F File Offset: 0x0001853F
		public override StatsControllerBase<MobStatModifier> StatsController
		{
			get
			{
				return this.statsController;
			}
		}

		// Token: 0x1700014F RID: 335
		// (get) Token: 0x06000811 RID: 2065 RVA: 0x0001A347 File Offset: 0x00018547
		public override bool AffectLocationChunkVisibility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000150 RID: 336
		// (get) Token: 0x06000812 RID: 2066 RVA: 0x0001A34A File Offset: 0x0001854A
		// (set) Token: 0x06000813 RID: 2067 RVA: 0x0001A352 File Offset: 0x00018552
		public Vector2 LookDirectionOverride { get; set; }

		// Token: 0x17000151 RID: 337
		// (get) Token: 0x06000814 RID: 2068 RVA: 0x0001A35B File Offset: 0x0001855B
		public override Vector2 CurrentLookDirection
		{
			get
			{
				return this.currentLookDirection;
			}
		}

		// Token: 0x17000152 RID: 338
		// (get) Token: 0x06000815 RID: 2069 RVA: 0x0001A363 File Offset: 0x00018563
		public MobsActivationModifiersController ActivationModifiersController
		{
			get
			{
				return this.activationModifiersController;
			}
		}

		// Token: 0x17000153 RID: 339
		// (get) Token: 0x06000816 RID: 2070 RVA: 0x0001A36B File Offset: 0x0001856B
		public bool InHomespace
		{
			get
			{
				return this.inHomespace;
			}
		}

		// Token: 0x17000154 RID: 340
		// (get) Token: 0x06000817 RID: 2071 RVA: 0x0001A373 File Offset: 0x00018573
		// (set) Token: 0x06000818 RID: 2072 RVA: 0x0001A37C File Offset: 0x0001857C
		[StatProperty(MobStatID.PlayerStaminaMax)]
		public float MaxStamina
		{
			get
			{
				return this._maxStamina;
			}
			set
			{
				if (value < 0f)
				{
					value = 0f;
				}
				if (this._maxStamina == value)
				{
					return;
				}
				if (this.isInitialized)
				{
					float num = (this._maxStamina > 0f) ? (this.currentStamina / this._maxStamina) : 0f;
					this.currentStamina = value * num;
				}
				this._maxStamina = value;
			}
		}

		// Token: 0x17000155 RID: 341
		// (get) Token: 0x06000819 RID: 2073 RVA: 0x0001A3DC File Offset: 0x000185DC
		// (set) Token: 0x0600081A RID: 2074 RVA: 0x0001A3E4 File Offset: 0x000185E4
		public float CurrentStamina
		{
			get
			{
				return this.currentStamina;
			}
			set
			{
				if (value > this._maxStamina)
				{
					value = this._maxStamina;
				}
				if (this.currentStamina == value)
				{
					return;
				}
				if (this.currentStamina - value > 0f)
				{
					this.staminaRestoringStartTime = ((this._staminaRestoringDelay > 0f) ? (Time.time + this._staminaRestoringDelay) : -1f);
				}
				this.currentStamina = value;
				Action<float> staminaChanged = this.StaminaChanged;
				if (staminaChanged == null)
				{
					return;
				}
				staminaChanged(value);
			}
		}

		// Token: 0x17000156 RID: 342
		// (get) Token: 0x0600081B RID: 2075 RVA: 0x0001A459 File Offset: 0x00018659
		// (set) Token: 0x0600081C RID: 2076 RVA: 0x0001A461 File Offset: 0x00018661
		[StatProperty(MobStatID.PlayerStaminaRestoringSpeed)]
		public float StaminaRestoringSpeed
		{
			get
			{
				return this._staminaRestoringSpeed;
			}
			set
			{
				this._staminaRestoringSpeed = value;
			}
		}

		// Token: 0x17000157 RID: 343
		// (get) Token: 0x0600081D RID: 2077 RVA: 0x0001A46A File Offset: 0x0001866A
		// (set) Token: 0x0600081E RID: 2078 RVA: 0x0001A472 File Offset: 0x00018672
		public float StaminaRestoringDelay
		{
			get
			{
				return this._staminaRestoringDelay;
			}
			set
			{
				this._staminaRestoringDelay = value;
			}
		}

		// Token: 0x17000158 RID: 344
		// (get) Token: 0x0600081F RID: 2079 RVA: 0x0001A47B File Offset: 0x0001867B
		// (set) Token: 0x06000820 RID: 2080 RVA: 0x0001A483 File Offset: 0x00018683
		public RevivableMobsAreaWatcher NecromancyTargetsWatcher { get; set; }

		// Token: 0x17000159 RID: 345
		// (get) Token: 0x06000821 RID: 2081 RVA: 0x0001A48C File Offset: 0x0001868C
		// (set) Token: 0x06000822 RID: 2082 RVA: 0x0001A494 File Offset: 0x00018694
		public AbilityResourcesWatcher AbilityResourcesWatcher { get; set; }

		// Token: 0x1700015A RID: 346
		// (get) Token: 0x06000823 RID: 2083 RVA: 0x0001A49D File Offset: 0x0001869D
		protected override bool CanGenerateResources
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1400004B RID: 75
		// (add) Token: 0x06000824 RID: 2084 RVA: 0x0001A4A0 File Offset: 0x000186A0
		// (remove) Token: 0x06000825 RID: 2085 RVA: 0x0001A4D8 File Offset: 0x000186D8
		public event Action<ILocationChunk> LocationChunkFullyReached;

		// Token: 0x1400004C RID: 76
		// (add) Token: 0x06000826 RID: 2086 RVA: 0x0001A510 File Offset: 0x00018710
		// (remove) Token: 0x06000827 RID: 2087 RVA: 0x0001A548 File Offset: 0x00018748
		public event Action<IDamageable, float> GroupMobDamageApplied;

		// Token: 0x1400004D RID: 77
		// (add) Token: 0x06000828 RID: 2088 RVA: 0x0001A580 File Offset: 0x00018780
		// (remove) Token: 0x06000829 RID: 2089 RVA: 0x0001A5B8 File Offset: 0x000187B8
		public event Action<float> StaminaChanged;

		// Token: 0x0600082A RID: 2090 RVA: 0x0001A5F0 File Offset: 0x000187F0
		private void CreateStatsController()
		{
			this.statsController = new StatsController(this, true);
			this.statsController.AddStat(new PlayerHPContainersCapacityStat(this));
			this.statsController.AddStat(new PlayerHPContainerInvulnerabilityDurationStat(this));
			this.statsController.AddStat(new MobDamageResistanceStat(this));
			if (this.activationModifiersController != null)
			{
				this.statsController.AddStat(new PlayerMobsActivationModifierStat(MobStatID.GroupMobsActivationModifiersDamage, this));
				this.statsController.AddStat(new PlayerMobsActivationModifierStat(MobStatID.GroupMobsActivationModifiersBuffDuration, this));
			}
			for (int i = 0; i < PlayerBehaviour.PlayerProxyStatsID.Length; i++)
			{
				this.statsController.AddStat(new MobProxyStat(PlayerBehaviour.PlayerProxyStatsID[i], this));
			}
			PlayerNoHordeProxyStat playerNoHordeProxyStat = new PlayerNoHordeProxyStat(MobStatID.PlayerNoHordeDamage, this);
			this.statsController.AddStat(playerNoHordeProxyStat);
			IModifiableStat<MobStatModifier> stat = this.statsController.GetStat(17);
			playerNoHordeProxyStat.AddStat(stat);
			ProxyStat<MobStatModifier> proxyStat = this.statsController.GetStat(2) as ProxyStat<MobStatModifier>;
			if (proxyStat != null)
			{
				proxyStat.AddStat(stat);
				proxyStat.AddStat(this.statsController.GetStat(35));
			}
			ProxyStat<MobStatModifier> proxyStat2 = stat as ProxyStat<MobStatModifier>;
			if (proxyStat2 != null)
			{
				proxyStat2.AddStat(this.statsController.GetStat(18));
				proxyStat2.AddStat(this.statsController.GetStat(19));
			}
			ProxyStat<MobStatModifier> proxyStat3 = this.statsController.GetStat(4) as ProxyStat<MobStatModifier>;
			if (proxyStat3 != null)
			{
				proxyStat3.AddStat(this.statsController.GetStat(27));
				proxyStat3.AddStat(this.statsController.GetStat(28));
				proxyStat3.AddStat(this.statsController.GetStat(29));
				proxyStat3.AddStat(this.statsController.GetStat(30));
			}
			ProxyStat<MobStatModifier> proxyStat4 = this.statsController.GetStat(9) as ProxyStat<MobStatModifier>;
			if (proxyStat4 != null)
			{
				proxyStat4.AddStat(this.statsController.GetStat(58));
				proxyStat4.AddStat(this.statsController.GetStat(59));
				proxyStat4.AddStat(this.statsController.GetStat(60));
				proxyStat4.AddStat(this.statsController.GetStat(61));
			}
			ProxyStat<MobStatModifier> proxyStat5 = this.statsController.GetStat(3) as ProxyStat<MobStatModifier>;
			if (proxyStat5 != null)
			{
				proxyStat5.AddStat(this.statsController.GetStat(31));
				proxyStat5.AddStat(this.statsController.GetStat(32));
				proxyStat5.AddStat(this.statsController.GetStat(33));
				proxyStat5.AddStat(this.statsController.GetStat(34));
			}
			ProxyStat<MobStatModifier> proxyStat6 = this.statsController.GetStat(44) as ProxyStat<MobStatModifier>;
			if (proxyStat6 != null)
			{
				proxyStat6.AddStat(this.statsController.GetStat(45));
				proxyStat6.AddStat(this.statsController.GetStat(46));
				proxyStat6.AddStat(this.statsController.GetStat(47));
				proxyStat6.AddStat(this.statsController.GetStat(48));
			}
			ProxyStat<MobStatModifier> proxyStat7 = this.statsController.GetStat(22) as ProxyStat<MobStatModifier>;
			if (proxyStat7 != null)
			{
				proxyStat7.AddStat(this.statsController.GetStat(23));
				proxyStat7.AddStat(this.statsController.GetStat(24));
				proxyStat7.AddStat(this.statsController.GetStat(25));
				proxyStat7.AddStat(this.statsController.GetStat(26));
			}
			ProxyStat<MobStatModifier> proxyStat8 = this.statsController.GetStat(38) as ProxyStat<MobStatModifier>;
			if (proxyStat8 != null)
			{
				proxyStat8.AddStat(this.statsController.GetStat(39));
				proxyStat8.AddStat(this.statsController.GetStat(40));
				proxyStat8.AddStat(this.statsController.GetStat(41));
				proxyStat8.AddStat(this.statsController.GetStat(42));
			}
			ProxyStat<MobStatModifier> proxyStat9 = this.statsController.GetStat(50) as ProxyStat<MobStatModifier>;
			if (proxyStat9 != null)
			{
				proxyStat9.AddStat(this.statsController.GetStat(51));
				proxyStat9.AddStat(this.statsController.GetStat(52));
				proxyStat9.AddStat(this.statsController.GetStat(53));
				proxyStat9.AddStat(this.statsController.GetStat(54));
			}
			ProxyStat<MobStatModifier> proxyStat10 = this.statsController.GetStat(65) as ProxyStat<MobStatModifier>;
			if (proxyStat10 != null)
			{
				proxyStat10.AddStat(this.statsController.GetStat(66));
				proxyStat10.AddStat(this.statsController.GetStat(67));
			}
		}

		// Token: 0x0600082B RID: 2091 RVA: 0x0001AA40 File Offset: 0x00018C40
		private void LoadPermanentUpgrades()
		{
			PermanentUpgradesStoreManager permanentUpgradesStoreManager;
			if (base.CurrentGame.Services.TryGet<PermanentUpgradesStoreManager>(out permanentUpgradesStoreManager))
			{
				foreach (PurchasablePermanentUpgradeCollection purchasablePermanentUpgradeCollection in permanentUpgradesStoreManager.UpgradesCollection)
				{
					purchasablePermanentUpgradeCollection.Apply(this);
				}
			}
		}

		// Token: 0x0600082C RID: 2092 RVA: 0x0001AAA8 File Offset: 0x00018CA8
		private void HandleAbilityStats(IAbility ability, bool registerStats)
		{
			GameAbilitiesController.HandleAbilityToMobStatsAttachment(this.statsController, ability, registerStats);
			IReadOnlyList<IModifiableStat<MobStatModifier>> abilityStats = ability.GetAbilityStats();
			if (abilityStats == null)
			{
				return;
			}
			int num = 6;
			for (int i = 0; i < abilityStats.Count; i++)
			{
				IModifiableStat<MobStatModifier> modifiableStat = abilityStats[i];
				if (modifiableStat.ID == num)
				{
					AmountStat amountStat = modifiableStat as AmountStat;
					if (amountStat != null && amountStat.AmountBasedEffect is IDamageSender)
					{
						BaseAbility baseAbility = (BaseAbility)ability;
						ProxyStat<MobStatModifier> proxyStat = this.statsController.GetProxyStat(MobStatID.MobDamage);
						MobStatID statID = MobStatID.Undefined;
						MobStatID statID2 = MobStatID.Undefined;
						if (baseAbility.IsPlayerMainBattleAbility())
						{
							statID = ((baseAbility is ProjectileAbilityBase) ? MobStatID.MainPlayerRangedDamage : MobStatID.MainPlayerMeleeDamage);
						}
						else
						{
							statID2 = MobStatID.SlotPlayerAbilitiesDamage;
						}
						if (registerStats)
						{
							if (proxyStat != null)
							{
								proxyStat.AddStat(amountStat);
							}
							ProxyStat<MobStatModifier> proxyStat2 = this.statsController.GetProxyStat(statID);
							if (proxyStat2 != null)
							{
								proxyStat2.AddStat(amountStat);
							}
							ProxyStat<MobStatModifier> proxyStat3 = this.statsController.GetProxyStat(statID2);
							if (proxyStat3 == null)
							{
								return;
							}
							proxyStat3.AddStat(amountStat);
							return;
						}
						else
						{
							if (proxyStat != null)
							{
								proxyStat.RemoveStat(amountStat, false);
							}
							ProxyStat<MobStatModifier> proxyStat4 = this.statsController.GetProxyStat(statID);
							if (proxyStat4 != null)
							{
								proxyStat4.RemoveStat(amountStat, false);
							}
							ProxyStat<MobStatModifier> proxyStat5 = this.statsController.GetProxyStat(statID2);
							if (proxyStat5 == null)
							{
								return;
							}
							proxyStat5.RemoveStat(amountStat, false);
							return;
						}
					}
				}
			}
		}

		// Token: 0x0600082D RID: 2093 RVA: 0x0001ABE0 File Offset: 0x00018DE0
		private void HandleMobTypeStats(StatsControllerBase<MobStatModifier> mobStatsController, MobActivationAbilityType activationType, bool addStats, bool isSacrificed)
		{
			switch (activationType)
			{
			case MobActivationAbilityType.Fighters:
				if (addStats)
				{
					this.statsController.AddToProxyStat(MobStatID.FightersActivationCost, mobStatsController, MobStatID.MobActivationCost);
					this.statsController.AddToProxyStat(MobStatID.FightersActivationDamage, mobStatsController, MobStatID.MobActivationDamage);
					this.statsController.AddToProxyStat(MobStatID.FightersActivationReward, mobStatsController, MobStatID.MobActivationReward);
					this.statsController.AddToProxyStat(MobStatID.FightersActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration);
					this.statsController.AddToProxyStat(MobStatID.FightersDamage, mobStatsController, MobStatID.MobDamage);
					this.statsController.AddToProxyStat(MobStatID.FightersAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed);
					this.statsController.AddToProxyStat(MobStatID.FightersHealth, mobStatsController, MobStatID.MobHealth);
					this.statsController.AddToProxyStat(MobStatID.FightersRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed);
					return;
				}
				this.statsController.RemoveFromProxyStat(MobStatID.FightersActivationCost, mobStatsController, MobStatID.MobActivationCost, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersActivationDamage, mobStatsController, MobStatID.MobActivationDamage, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersActivationReward, mobStatsController, MobStatID.MobActivationReward, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersDamage, mobStatsController, MobStatID.MobDamage, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersHealth, mobStatsController, MobStatID.MobHealth, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.FightersRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed, false, null);
				return;
			case MobActivationAbilityType.Giants:
				if (addStats)
				{
					this.statsController.AddToProxyStat(MobStatID.GiantsActivationCost, mobStatsController, MobStatID.MobActivationCost);
					this.statsController.AddToProxyStat(MobStatID.GiantsActivationDamage, mobStatsController, MobStatID.MobActivationDamage);
					this.statsController.AddToProxyStat(MobStatID.GiantsActivationReward, mobStatsController, MobStatID.MobActivationReward);
					this.statsController.AddToProxyStat(MobStatID.GiantsDamage, mobStatsController, MobStatID.MobDamage);
					this.statsController.AddToProxyStat(MobStatID.GiantsAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed);
					this.statsController.AddToProxyStat(MobStatID.GiantsHealth, mobStatsController, MobStatID.MobHealth);
					this.statsController.AddToProxyStat(MobStatID.GiantsRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed);
					return;
				}
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsActivationCost, mobStatsController, MobStatID.MobActivationCost, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsActivationDamage, mobStatsController, MobStatID.MobActivationDamage, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsActivationReward, mobStatsController, MobStatID.MobActivationReward, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsDamage, mobStatsController, MobStatID.MobDamage, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsHealth, mobStatsController, MobStatID.MobHealth, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.GiantsRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed, false, null);
				return;
			case MobActivationAbilityType.Fighters | MobActivationAbilityType.Giants:
				break;
			case MobActivationAbilityType.Ranged:
				if (addStats)
				{
					this.statsController.AddToProxyStat(MobStatID.RangedActivationCost, mobStatsController, MobStatID.MobActivationCost);
					this.statsController.AddToProxyStat(MobStatID.RangedActivationDamage, mobStatsController, MobStatID.MobActivationDamage);
					this.statsController.AddToProxyStat(MobStatID.RangedActivationReward, mobStatsController, MobStatID.MobActivationReward);
					this.statsController.AddToProxyStat(MobStatID.RangedDamage, mobStatsController, MobStatID.MobDamage);
					this.statsController.AddToProxyStat(MobStatID.RangedAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed);
					this.statsController.AddToProxyStat(MobStatID.RangedHealth, mobStatsController, MobStatID.MobHealth);
					this.statsController.AddToProxyStat(MobStatID.RangedRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed);
					return;
				}
				this.statsController.RemoveFromProxyStat(MobStatID.RangedActivationCost, mobStatsController, MobStatID.MobActivationCost, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedActivationDamage, mobStatsController, MobStatID.MobActivationDamage, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedActivationReward, mobStatsController, MobStatID.MobActivationReward, isSacrificed, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedDamage, mobStatsController, MobStatID.MobDamage, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedHealth, mobStatsController, MobStatID.MobHealth, false, null);
				this.statsController.RemoveFromProxyStat(MobStatID.RangedRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed, false, null);
				return;
			default:
				if (activationType == MobActivationAbilityType.Unholy)
				{
					if (addStats)
					{
						this.statsController.AddToProxyStat(MobStatID.UnholyActivationCost, mobStatsController, MobStatID.MobActivationCost);
						this.statsController.AddToProxyStat(MobStatID.UnholyActivationDamage, mobStatsController, MobStatID.MobActivationDamage);
						this.statsController.AddToProxyStat(MobStatID.UnholyActivationReward, mobStatsController, MobStatID.MobActivationReward);
						this.statsController.AddToProxyStat(MobStatID.UnholyActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration);
						this.statsController.AddToProxyStat(MobStatID.UnholyDamage, mobStatsController, MobStatID.MobDamage);
						this.statsController.AddToProxyStat(MobStatID.UnholyAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed);
						this.statsController.AddToProxyStat(MobStatID.UnholyHealth, mobStatsController, MobStatID.MobHealth);
						this.statsController.AddToProxyStat(MobStatID.UnholyRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed);
						return;
					}
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyActivationCost, mobStatsController, MobStatID.MobActivationCost, isSacrificed, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyActivationDamage, mobStatsController, MobStatID.MobActivationDamage, isSacrificed, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyActivationReward, mobStatsController, MobStatID.MobActivationReward, isSacrificed, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration, isSacrificed, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyDamage, mobStatsController, MobStatID.MobDamage, false, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed, false, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyHealth, mobStatsController, MobStatID.MobHealth, false, null);
					this.statsController.RemoveFromProxyStat(MobStatID.UnholyRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed, false, null);
					return;
				}
				break;
			}
			if (addStats)
			{
				this.statsController.AddToProxyStat(MobStatID.GroupMobsActivationCost, mobStatsController, MobStatID.MobActivationCost);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsActivationDamage, mobStatsController, MobStatID.MobActivationDamage);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsActivationReward, mobStatsController, MobStatID.MobActivationReward);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsDamage, mobStatsController, MobStatID.MobDamage);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsHealth, mobStatsController, MobStatID.MobHealth);
				this.statsController.AddToProxyStat(MobStatID.GroupMobsRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed);
				return;
			}
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsActivationCost, mobStatsController, MobStatID.MobActivationCost, isSacrificed, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsActivationDamage, mobStatsController, MobStatID.MobActivationDamage, isSacrificed, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsActivationReward, mobStatsController, MobStatID.MobActivationReward, isSacrificed, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsActivationBuffsDuration, mobStatsController, MobStatID.MobActivationBuffsDuration, isSacrificed, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsDamage, mobStatsController, MobStatID.MobDamage, false, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsAttackSpeed, mobStatsController, MobStatID.MobAttackSpeed, false, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsHealth, mobStatsController, MobStatID.MobHealth, false, null);
			this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsRottingSpeed, mobStatsController, MobStatID.MobRottingSpeed, false, null);
		}

		// Token: 0x0600082E RID: 2094 RVA: 0x0001B128 File Offset: 0x00019328
		private void HandleGroupMobStats(BaseGameMob targetMob, bool addStats)
		{
			StatsControllerBase<MobStatModifier> statsControllerBase = targetMob.StatsController;
			if (statsControllerBase == null)
			{
				return;
			}
			bool isSacrificed = targetMob.IsSacrificed;
			if (addStats)
			{
				this.statsController.AddToProxyStat(MobStatID.GroupMobsSpeed, statsControllerBase, MobStatID.MobSpeed);
			}
			else
			{
				this.statsController.RemoveFromProxyStat(MobStatID.GroupMobsSpeed, statsControllerBase, MobStatID.MobSpeed, false, null);
			}
			MobActivationAbilityType activationType;
			targetMob.TryGetMobActivationType(out activationType);
			this.HandleMobTypeStats(statsControllerBase, activationType, addStats, isSacrificed);
		}

		// Token: 0x0600082F RID: 2095 RVA: 0x0001B180 File Offset: 0x00019380
		private void RestoreStamina()
		{
			if (this._staminaRestoringSpeed <= 0f || this.currentStamina >= this._maxStamina)
			{
				return;
			}
			if (this.staminaRestoringStartTime < 0f || Time.time > this.staminaRestoringStartTime)
			{
				this.currentStamina += this._staminaRestoringSpeed * Time.deltaTime;
				if (this.currentStamina > this._maxStamina)
				{
					this.currentStamina = this._maxStamina;
				}
				Action<float> staminaChanged = this.StaminaChanged;
				if (staminaChanged == null)
				{
					return;
				}
				staminaChanged(this.currentStamina);
			}
		}

		// Token: 0x06000830 RID: 2096 RVA: 0x0001B20C File Offset: 0x0001940C
		private void RegisterPlayer()
		{
			IPlayerProvider playerProvider = base.CurrentGame.Services.Get<IPlayerProvider>();
			if (playerProvider == null)
			{
				return;
			}
			playerProvider.RegisterPlayer(this);
		}

		// Token: 0x06000831 RID: 2097 RVA: 0x0001B229 File Offset: 0x00019429
		private IEnumerator LocationChunkInitializationRoutine()
		{
			yield return new GameLocationGameplayExtensions.WaitForChunkInitialization(this.currentLocationChunk);
			Action<ILocationChunk> locationChunkFullyReached = this.LocationChunkFullyReached;
			if (locationChunkFullyReached != null)
			{
				locationChunkFullyReached(this.currentLocationChunk);
			}
			yield break;
		}

		// Token: 0x06000832 RID: 2098 RVA: 0x0001B238 File Offset: 0x00019438
		private Vector2 GetLookDirection(Vector2 targetPoint)
		{
			Vector2 result = targetPoint - base.transform.position;
			result.Normalize();
			return result;
		}

		// Token: 0x06000833 RID: 2099 RVA: 0x0001B264 File Offset: 0x00019464
		protected override LocationObjectType GetLocationObjectType()
		{
			return LocationObjectType.Player;
		}

		// Token: 0x06000834 RID: 2100 RVA: 0x0001B267 File Offset: 0x00019467
		public void OverrideInitialAbilities(InitialPlayerAbilitiesInfo abilitiesInfoOverride)
		{
			this.abilitiesInfoOverride = abilitiesInfoOverride;
		}

		// Token: 0x06000835 RID: 2101 RVA: 0x0001B270 File Offset: 0x00019470
		public void PerformCurrencyOperation(ICurrencyOperationArgs args)
		{
			PlayerProfile playerProfile = this.playerProfile;
			if (playerProfile == null)
			{
				return;
			}
			playerProfile.TryExecuteCurrencyOperation(args);
		}

		// Token: 0x06000836 RID: 2102 RVA: 0x0001B284 File Offset: 0x00019484
		public override bool CanBeSacrificed(UnityEngine.Object sacrificer, bool checkGroup = true)
		{
			return false;
		}

		// Token: 0x06000837 RID: 2103 RVA: 0x0001B287 File Offset: 0x00019487
		public void OverrideLookDirection(Vector2 targetPoint)
		{
			this.LookDirectionOverride = this.GetLookDirection(targetPoint);
		}

		// Token: 0x06000838 RID: 2104 RVA: 0x0001B296 File Offset: 0x00019496
		public void KeepLookDirectionOverride()
		{
			this.keepLookDirectionOverride = true;
		}

		// Token: 0x06000839 RID: 2105 RVA: 0x0001B2A0 File Offset: 0x000194A0
		public void ResetLookDirectionOverride()
		{
			this.LookDirectionOverride = default(Vector2);
			this.keepLookDirectionOverride = false;
		}

		// Token: 0x0600083A RID: 2106 RVA: 0x0001B2C4 File Offset: 0x000194C4
		public void SetCutsceneFacingDirection(Vector2 toward, bool updateLookDirection = false)
		{
			float x = (base.transform.position.x > toward.x) ? -1f : 1f;
			base.transform.localScale = new Vector3(x, 1f, 1f);
			if (updateLookDirection)
			{
				this.OverrideLookDirection(toward);
				this.currentLookDirection = this.LookDirectionOverride;
			}
			this.keepLookDirectionOverride = true;
		}

		// Token: 0x0600083B RID: 2107 RVA: 0x0001B330 File Offset: 0x00019530
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			currentGame.Services.TryGet<IInputManager>(out this.inputManager);
			this.staminaRestoringStartTime = -1f;
			GameManager gameManager;
			if (currentGame.Services.TryGet<GameManager>(out gameManager))
			{
				this.inHomespace = gameManager.IsHomespaceLoaded;
			}
			if (currentGame.BindDataDirectly(ref this.globalParameters) && this.PlayerInputController != null)
			{
				this.playerInputController.clickableLayers = this.globalParameters.ClickableMobLayers;
			}
			IGameMobGroupControllerProvider gameMobGroupControllerProvider = base.gameObject.GetComponent<IGameMobGroupControllerProvider>();
			if (gameMobGroupControllerProvider == null)
			{
				gameMobGroupControllerProvider = base.gameObject.AddComponent<PlayerMobGroup>();
				this.Group = gameMobGroupControllerProvider.GroupController;
				this.Group.Faction = GameMobFactions.PLAYER;
			}
			MobBehaviourSpawner component = base.GetComponent<MobBehaviourSpawner>();
			if (this.inHomespace)
			{
				if (component != null)
				{
					component.enabled = false;
				}
			}
			else
			{
				if (component != null)
				{
					component.PrepareForSpawning(gameMobGroupControllerProvider);
				}
				GameSceneManager gameSceneManager;
				if (currentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
				{
					gameSceneManager.LocationExplorer = this;
				}
				this.aimAssistController = new AimAssistController(this);
			}
			PlayerProfileManager playerProfileManager = currentGame.Services.Get<PlayerProfileManager>();
			this.playerProfile = ((playerProfileManager != null) ? playerProfileManager.CurrentPlayerProfile : null);
		}

		// Token: 0x0600083C RID: 2108 RVA: 0x0001B44C File Offset: 0x0001964C
		protected override void OnMobInitialization()
		{
			base.OnMobInitialization();
			if (this.PlayerInputController != null)
			{
				this.PlayerInputController.PlayerActionPerformed += this.OnInputActionActivated;
			}
			PlayerBehaviour.ExposedParameters exposedParameters = this.globalParameters;
			MobsActivationModifiersController.Slot[] array = (exposedParameters != null) ? exposedParameters.ActivationModifiersSlots : null;
			if (array != null)
			{
				IObjectFactory<MobActivationAbilityModifier> modifiersFactory = base.CurrentGame.Services.Get<IObjectFactory<MobActivationAbilityModifier>>();
				this.activationModifiersController = new MobsActivationModifiersController(this, array, modifiersFactory);
			}
			this.CreateStatsController();
			PlayerProfileManager playerProfileManager = base.CurrentGame.Services.Get<PlayerProfileManager>();
			PlayerBehaviour.RestorableState restorableState = (playerProfileManager != null) ? playerProfileManager.CurrentPlayerProfile.playerCharacterState : null;
			IGameAbilitiesFactory abilityFactory = base.CurrentGame.Services.Get<IGameAbilitiesFactory>();
			IAbilityActivatedContainersFactory hpContainersAbilityFactory = base.CurrentGame.Services.Get<IAbilityActivatedContainersFactory>();
			this.abilitiesController = new PlayerAbilitiesController(this, abilityFactory, hpContainersAbilityFactory)
			{
				useActiveAbilitySlot = this.globalParameters.UseActiveAbilitySlot,
				abilityActivationTimeout = this.globalParameters.AbilityActivationTimeout,
				abilitiesWithoutTimeout = this.globalParameters.AbilitiesWithoutTimeout,
				nativeAbilitySlotsFallback = this.globalParameters.NativeAbilitySlotsFallback
			};
			this.passiveAbilitiesController = new PassiveAbilitiesController(this, null);
			if (restorableState != null)
			{
				restorableState.Restore(this, this.abilitiesInfoOverride);
			}
			else
			{
				this.abilitiesController.Initialize(this.globalParameters.NativeAbilities, this.globalParameters.NativeAbilityTypes, this.globalParameters.Abilities, this.globalParameters.AbilityTypes);
			}
			this.abilitiesInfoOverride = null;
			this.abilitiesController.AbilityAddedToSlot += this.OnAbilityAddedToSlot;
			this.abilitiesController.AbilityRemovedFromSlot += this.OnAbilityRemovedFromSlot;
			this.currentStamina = this._maxStamina;
			this.playerMotionController = new PlayerMotionController(this);
			GameMobsGroupControllerBase group = this.Group;
			IReadOnlyList<IAbility> abilities = this.abilitiesController.Abilities;
			for (int i = 0; i < abilities.Count; i++)
			{
				this.HandleAbilityStats(abilities[i], true);
			}
			if (group != null)
			{
				IReadOnlyList<BaseGameMob> mobs = group.Mobs;
				for (int j = 0; j < mobs.Count; j++)
				{
					this.OnMobAddedToGroup(group, mobs[j]);
				}
				group.MobAdded += this.OnMobAddedToGroup;
				group.MobRemoved += this.OnMobRemovedFromGroup;
			}
			this.LoadPermanentUpgrades();
			this.RegisterPlayer();
		}

		// Token: 0x0600083D RID: 2109 RVA: 0x0001B698 File Offset: 0x00019898
		void PrototypeBasedFactory<PlayerBehaviour.FactoryPrototype, IGameMob>.IInitializableByFactory.OnCreatedByFactory(IObjectFactory<IGameMob> factory, IBaseObjectDescription args, PlayerBehaviour.FactoryPrototype playerData)
		{
			VitalEnergyHitPointsController vitalEnergyHitPointsController = base.HitPointsController as VitalEnergyHitPointsController;
			if (vitalEnergyHitPointsController != null)
			{
				float healthContrainerInitialAmount = playerData.healthContrainerInitialAmount;
				vitalEnergyHitPointsController.InitialHitPoints = healthContrainerInitialAmount;
				int count = playerData.defaultHealthContainers.Count;
				List<IEnergyContainer> list = new List<IEnergyContainer>(count);
				for (int i = 0; i < count; i++)
				{
					HealthContainer item = new HealthContainer(healthContrainerInitialAmount);
					list.Add(item);
				}
				vitalEnergyHitPointsController.SetContainersData(playerData.damageResistanceTimeout, list, playerData.maxHealthContainersSlotsCount, count);
				IAbilityActivatedContainersController abilityActivatedContainersController = vitalEnergyHitPointsController;
				if (abilityActivatedContainersController != null)
				{
					abilityActivatedContainersController.SetContainersData(playerData.defaultHealthContainers);
				}
			}
			else
			{
				base.MaxHitPoints = playerData.healthContrainerInitialAmount;
			}
			if (playerData.energyRegenerationEnabled && base.TryGetComponent<IEnergyRegenerable>(out this.energyRegenerationController))
			{
				this.energyRegenerationController.SetEnergyRegenerationData(playerData.energyRegenerationAmount, playerData.energyRegenerationTimeout, playerData.energyRegenerationDelay);
			}
			base.Speed = playerData.Speed;
			this._maxStamina = playerData.Stamina;
			this._staminaRestoringSpeed = playerData.StaminaRestoringSpeed;
			this._staminaRestoringDelay = playerData.StamingRestoringDelay;
			base.InitializeMob();
		}

		// Token: 0x0600083E RID: 2110 RVA: 0x0001B794 File Offset: 0x00019994
		private void OnInputActionActivated(PlayerInputController.ActionArgs args)
		{
			if (!args.HasActionFlag(PlayerAction.PLAYER_COLLECT_OBJECT))
			{
				return;
			}
			if (!base.PickableObjectsController.IsNull() && !base.PickableObjectsController.CurrentBestPickingCandidate.IsNull())
			{
				PickingArgs args2 = new PickingArgs
				{
					inputArgs = args
				};
				base.PickableObjectsController.PickUpBestObject(args2);
				args.Use(PlayerAction.PLAYER_COLLECT_OBJECT);
				int keyID;
				if (this.inputManager.TryGetActionElementID(24, InputAxisContribution.Positive, out keyID))
				{
					this.playerInputController.LockInput(keyID, PlayerAction.PLAYER_COLLECT_OBJECT);
				}
			}
		}

		// Token: 0x0600083F RID: 2111 RVA: 0x0001B814 File Offset: 0x00019A14
		protected override void OnKinematicStateChanged(bool isKinematic)
		{
			this.playerInputController.IsActive = !isKinematic;
			SpriteTrail spriteTrail;
			if (base.TryGetComponent<SpriteTrail>(out spriteTrail))
			{
				if (isKinematic)
				{
					spriteTrail.DisableTrail();
					return;
				}
				spriteTrail.EnableTrail();
			}
		}

		// Token: 0x06000840 RID: 2112 RVA: 0x0001B84A File Offset: 0x00019A4A
		protected override void OnLocationChunkEntered(ILocationChunk newLocationChunk)
		{
			if (this.currentChunkInitializationCoroutine != null)
			{
				base.StopCoroutine(this.currentChunkInitializationCoroutine);
				this.currentChunkInitializationCoroutine = null;
			}
			if (this.LocationChunkFullyReached != null)
			{
				this.currentChunkInitializationCoroutine = base.StartCoroutine(this.LocationChunkInitializationRoutine());
			}
			base.OnLocationChunkEntered(newLocationChunk);
		}

		// Token: 0x06000841 RID: 2113 RVA: 0x0001B888 File Offset: 0x00019A88
		private void OnAbilityAddedToSlot(IAbility playerAbility, int slotIndex)
		{
			this.HandleAbilityStats(playerAbility, true);
		}

		// Token: 0x06000842 RID: 2114 RVA: 0x0001B892 File Offset: 0x00019A92
		private void OnAbilityRemovedFromSlot(IAbility playerAbility, int slotIndex)
		{
			this.HandleAbilityStats(playerAbility, false);
		}

		// Token: 0x06000843 RID: 2115 RVA: 0x0001B89C File Offset: 0x00019A9C
		private void OnMobAddedToGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.HandleGroupMobStats(mob, true);
		}

		// Token: 0x06000844 RID: 2116 RVA: 0x0001B8A6 File Offset: 0x00019AA6
		private void OnMobRemovedFromGroup(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.HandleGroupMobStats(mob, false);
		}

		// Token: 0x06000845 RID: 2117 RVA: 0x0001B8B0 File Offset: 0x00019AB0
		void IGroupMobDamageFeedbackReceiver.OnGroupMobDamageApplied(IDamageable damagedObject, float damageAmount)
		{
			Action<IDamageable, float> groupMobDamageApplied = this.GroupMobDamageApplied;
			if (groupMobDamageApplied == null)
			{
				return;
			}
			groupMobDamageApplied(damagedObject, damageAmount);
		}

		// Token: 0x06000846 RID: 2118 RVA: 0x0001B8C4 File Offset: 0x00019AC4
		protected override void Update()
		{
			this.playerInputController.OnUpdate();
			this.playerInputController.SubmitInputActions();
			this.playerMotionController.OnUpdate();
			TemporaryItemsStorageController temporaryItemsStorageController = this.tempItemsStorageController;
			if (temporaryItemsStorageController != null)
			{
				temporaryItemsStorageController.Update();
			}
			PlayerMobsActivationControllerBase playerMobsActivationControllerBase = this.playerMobsActivationController;
			if (playerMobsActivationControllerBase != null)
			{
				playerMobsActivationControllerBase.OnUpdate();
			}
			base.Update();
		}

		// Token: 0x06000847 RID: 2119 RVA: 0x0001B91A File Offset: 0x00019B1A
		private void FixedUpdate()
		{
			this.playerMotionController.OnFixedUpdate();
		}

		// Token: 0x06000848 RID: 2120 RVA: 0x0001B928 File Offset: 0x00019B28
		private void LateUpdate()
		{
			if (this.LookDirectionOverride != default(Vector2))
			{
				this.currentLookDirection = this.LookDirectionOverride;
				if (!this.keepLookDirectionOverride)
				{
					this.ResetLookDirectionOverride();
				}
			}
			else if (this.playerInputController.IsActive && !this.playerInputController.IsCompletelyLocked && !base.IsKinematic)
			{
				this.currentLookDirection = this.GetLookDirection(this.playerInputController.CurrentWorldCursorPosition);
			}
			this.playerInputController.OnLateUpdate();
			this.playerMotionController.OnLateUpdate();
			IPlayerAbilitiesController playerAbilitiesController = this.abilitiesController;
			if (playerAbilitiesController != null)
			{
				playerAbilitiesController.OnLateUpdate();
			}
			IAimAssistController aimAssistController = this.aimAssistController;
			if (aimAssistController != null)
			{
				aimAssistController.OnLateUpdate();
			}
			this.RestoreStamina();
			if (!this.energyRegenerationController.IsNull())
			{
				this.energyRegenerationController.RegenerateEnergy();
			}
		}

		// Token: 0x06000849 RID: 2121 RVA: 0x0001B9F8 File Offset: 0x00019BF8
		protected override void OnKilled(IDamageable hitPointsController)
		{
			if (this.abilitiesController != null)
			{
				this.abilitiesController.AbilityAddedToSlot -= this.OnAbilityAddedToSlot;
				this.abilitiesController.AbilityRemovedFromSlot -= this.OnAbilityRemovedFromSlot;
				this.abilitiesController.ForAll(delegate(IAbility ability)
				{
					ability.Complete();
				});
			}
			if (this.playerInputController != null)
			{
				this.playerInputController.IsActive = false;
				this.playerInputController.PlayerActionPerformed -= this.OnInputActionActivated;
			}
			if (!this.Group.IsNull())
			{
				this.Group.MobAdded -= this.OnMobAddedToGroup;
				this.Group.MobRemoved -= this.OnMobRemovedFromGroup;
			}
			base.OnKilled(hitPointsController);
		}

		// Token: 0x04000493 RID: 1171
		private static readonly MobStatID[] PlayerProxyStatsID = new MobStatID[]
		{
			MobStatID.MobDamage,
			MobStatID.MobAttackSpeed,
			MobStatID.AbilityUsingCost,
			MobStatID.AbilityCooldown,
			MobStatID.MainPlayerDamage,
			MobStatID.MainPlayerMeleeDamage,
			MobStatID.MainPlayerRangedDamage,
			MobStatID.PlayerAbilitiesPower,
			MobStatID.SlotPlayerAbilitiesDamage,
			MobStatID.GroupMobsAttackSpeed,
			MobStatID.GroupMobsDamage,
			MobStatID.GroupMobsActivationDamage,
			MobStatID.GroupMobsActivationBuffsDuration,
			MobStatID.GroupMobsActivationCost,
			MobStatID.GroupMobsActivationReward,
			MobStatID.GroupMobsHealth,
			MobStatID.GroupMobsSpeed,
			MobStatID.GroupMobsRottingSpeed,
			MobStatID.FightersActivationDamage,
			MobStatID.RangedActivationDamage,
			MobStatID.GiantsActivationDamage,
			MobStatID.UnholyActivationDamage,
			MobStatID.FightersActivationBuffsDuration,
			MobStatID.UnholyActivationBuffsDuration,
			MobStatID.FightersActivationCost,
			MobStatID.RangedActivationCost,
			MobStatID.GiantsActivationCost,
			MobStatID.UnholyActivationCost,
			MobStatID.FightersActivationReward,
			MobStatID.RangedActivationReward,
			MobStatID.GiantsActivationReward,
			MobStatID.UnholyActivationReward,
			MobStatID.FightersDamage,
			MobStatID.RangedDamage,
			MobStatID.GiantsDamage,
			MobStatID.UnholyDamage,
			MobStatID.FightersAttackSpeed,
			MobStatID.RangedAttackSpeed,
			MobStatID.GiantsAttackSpeed,
			MobStatID.UnholyAttackSpeed,
			MobStatID.FightersHealth,
			MobStatID.RangedHealth,
			MobStatID.GiantsHealth,
			MobStatID.UnholyHealth,
			MobStatID.FightersRottingSpeed,
			MobStatID.RangedRottingSpeed,
			MobStatID.GiantsRottingSpeed,
			MobStatID.UnholyRottingSpeed
		};

		// Token: 0x0400049C RID: 1180
		[SerializeField]
		private float _maxStamina;

		// Token: 0x0400049D RID: 1181
		[SerializeField]
		private float _staminaRestoringSpeed;

		// Token: 0x0400049E RID: 1182
		[SerializeField]
		private float _staminaRestoringDelay;

		// Token: 0x0400049F RID: 1183
		[NonSerialized]
		public PlayerBehaviour.ExposedParameters globalParameters;

		// Token: 0x040004A0 RID: 1184
		private PlayerMobsActivationControllerBase playerMobsActivationController;

		// Token: 0x040004A1 RID: 1185
		private IAimAssistController aimAssistController;

		// Token: 0x040004A2 RID: 1186
		private PlayerInputController playerInputController;

		// Token: 0x040004A3 RID: 1187
		private PlayerMotionController playerMotionController;

		// Token: 0x040004A4 RID: 1188
		private IEnergyRegenerable energyRegenerationController;

		// Token: 0x040004A5 RID: 1189
		private StatsController statsController;

		// Token: 0x040004A6 RID: 1190
		private IPlayerAbilitiesController abilitiesController;

		// Token: 0x040004A7 RID: 1191
		private InitialPlayerAbilitiesInfo abilitiesInfoOverride;

		// Token: 0x040004A8 RID: 1192
		private PassiveAbilitiesController passiveAbilitiesController;

		// Token: 0x040004A9 RID: 1193
		private MobsActivationModifiersController activationModifiersController;

		// Token: 0x040004AA RID: 1194
		private TemporaryItemsStorageController tempItemsStorageController;

		// Token: 0x040004AB RID: 1195
		private Coroutine currentChunkInitializationCoroutine;

		// Token: 0x040004AC RID: 1196
		private PlayerProfile playerProfile;

		// Token: 0x040004AD RID: 1197
		private IInputManager inputManager;

		// Token: 0x040004AE RID: 1198
		private bool inHomespace;

		// Token: 0x040004AF RID: 1199
		private float currentStamina;

		// Token: 0x040004B0 RID: 1200
		private float staminaRestoringStartTime;

		// Token: 0x040004B1 RID: 1201
		private Vector2 currentLookDirection;

		// Token: 0x040004B2 RID: 1202
		private bool keepLookDirectionOverride;

		// Token: 0x02000447 RID: 1095
		public enum ID
		{
			// Token: 0x040016A2 RID: 5794
			None,
			// Token: 0x040016A3 RID: 5795
			Player
		}

		// Token: 0x02000448 RID: 1096
		[Serializable]
		public sealed class FactoryPrototype : IUnityObjectDescription, IBaseObjectDescription
		{
			// Token: 0x1700071F RID: 1823
			// (get) Token: 0x06002343 RID: 9027 RVA: 0x0006CF7D File Offset: 0x0006B17D
			// (set) Token: 0x06002344 RID: 9028 RVA: 0x0006CF85 File Offset: 0x0006B185
			int IBaseObjectDescription.ObjectID
			{
				get
				{
					return (int)this.objectID;
				}
				set
				{
					this.objectID = (PlayerBehaviour.ID)value;
				}
			}

			// Token: 0x17000720 RID: 1824
			// (get) Token: 0x06002345 RID: 9029 RVA: 0x0006CF8E File Offset: 0x0006B18E
			UnityEngine.Object IUnityObjectDescription.UnityObjectPrototype
			{
				get
				{
					return this.objectPrefab;
				}
			}

			// Token: 0x040016A4 RID: 5796
			public PlayerBehaviour.ID objectID;

			// Token: 0x040016A5 RID: 5797
			public GameObject objectPrefab;

			// Token: 0x040016A6 RID: 5798
			[Header("Контроллер здоровья игрока")]
			[Tooltip("Таймаут неуязвимости игрока после уничтожения контейнера")]
			public float damageResistanceTimeout;

			// Token: 0x040016A7 RID: 5799
			[SerializeReference]
			[ManagedObjectField(typeof(IAbilityActivatedContainerData))]
			public List<IAbilityActivatedContainerData> defaultHealthContainers;

			// Token: 0x040016A8 RID: 5800
			public int maxHealthContainersSlotsCount;

			// Token: 0x040016A9 RID: 5801
			public float healthContrainerInitialAmount;

			// Token: 0x040016AA RID: 5802
			[Header("Регенерация энергии")]
			public bool energyRegenerationEnabled;

			// Token: 0x040016AB RID: 5803
			[Tooltip("Кол-во энергии восстанавливаемое при каждой итерации регенерации")]
			public float energyRegenerationAmount;

			// Token: 0x040016AC RID: 5804
			[Tooltip("Таймаут регенерации энергии")]
			public float energyRegenerationTimeout;

			// Token: 0x040016AD RID: 5805
			[Tooltip("Задержка регенерации энергии после ее последней траты")]
			public float energyRegenerationDelay;

			// Token: 0x040016AE RID: 5806
			[Space]
			public float Speed;

			// Token: 0x040016AF RID: 5807
			public float Stamina;

			// Token: 0x040016B0 RID: 5808
			public float StaminaRestoringSpeed;

			// Token: 0x040016B1 RID: 5809
			public float StamingRestoringDelay;
		}

		// Token: 0x02000449 RID: 1097
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PlayerBehaviour>, ICloneable<PlayerBehaviour.RestorableState>
		{
			// Token: 0x17000721 RID: 1825
			// (get) Token: 0x06002347 RID: 9031 RVA: 0x0006CF9E File Offset: 0x0006B19E
			public AbilityInfo[] NativeAbilities
			{
				get
				{
					return this.nativeAbilities;
				}
			}

			// Token: 0x17000722 RID: 1826
			// (get) Token: 0x06002348 RID: 9032 RVA: 0x0006CFA6 File Offset: 0x0006B1A6
			public AbilityTypes[] NativeAbilitiesTypes
			{
				get
				{
					return this.nativeAbilitiesTypes;
				}
			}

			// Token: 0x17000723 RID: 1827
			// (get) Token: 0x06002349 RID: 9033 RVA: 0x0006CFAE File Offset: 0x0006B1AE
			public AbilityInfo[] Abilities
			{
				get
				{
					return this.abilities;
				}
			}

			// Token: 0x17000724 RID: 1828
			// (get) Token: 0x0600234A RID: 9034 RVA: 0x0006CFB6 File Offset: 0x0006B1B6
			public AbilityTypes[] AbilitiesTypes
			{
				get
				{
					return this.abilitiesTypes;
				}
			}

			// Token: 0x17000725 RID: 1829
			// (get) Token: 0x0600234B RID: 9035 RVA: 0x0006CFBE File Offset: 0x0006B1BE
			public PlayerBehaviour.RestorableState.PassiveAbilityInfo[] PassiveAbilities
			{
				get
				{
					return this.passiveAbilities;
				}
			}

			// Token: 0x17000726 RID: 1830
			// (get) Token: 0x0600234C RID: 9036 RVA: 0x0006CFC6 File Offset: 0x0006B1C6
			public IReadOnlyList<PlayerBehaviour.RestorableState.MobsActivationModifierInfo> ActivationModifiers
			{
				get
				{
					return this.activationModifiers;
				}
			}

			// Token: 0x17000727 RID: 1831
			// (get) Token: 0x0600234D RID: 9037 RVA: 0x0006CFCE File Offset: 0x0006B1CE
			public IReadOnlyList<PlayerUpgradeInfo> Upgrades
			{
				get
				{
					return this.upgrades;
				}
			}

			// Token: 0x17000728 RID: 1832
			// (get) Token: 0x0600234E RID: 9038 RVA: 0x0006CFD6 File Offset: 0x0006B1D6
			public int[] PickableTextShownCount
			{
				get
				{
					return this.pickableTextShownCount;
				}
			}

			// Token: 0x17000729 RID: 1833
			// (get) Token: 0x0600234F RID: 9039 RVA: 0x0006CFDE File Offset: 0x0006B1DE
			public GameLocation.TypeID[] ExploredLocations
			{
				get
				{
					return this.exploredLocations;
				}
			}

			// Token: 0x06002350 RID: 9040 RVA: 0x0006CFE6 File Offset: 0x0006B1E6
			public RestorableState() : base(null)
			{
			}

			// Token: 0x06002351 RID: 9041 RVA: 0x0006D005 File Offset: 0x0006B205
			public RestorableState(PlayerBehaviour targetObject) : base(targetObject)
			{
			}

			// Token: 0x06002352 RID: 9042 RVA: 0x0006D024 File Offset: 0x0006B224
			private PlayerBehaviour.RestorableState.PassiveAbilityInfo[] GetPassiveAbilitiesInfo(IPassiveAbilitiesController abilitiesController)
			{
				if (abilitiesController != null)
				{
					Dictionary<PassiveAbilityID, PlayerBehaviour.RestorableState.PassiveAbilityInfo> dictionary = new Dictionary<PassiveAbilityID, PlayerBehaviour.RestorableState.PassiveAbilityInfo>();
					for (int i = 0; i < abilitiesController.Abilities.Count; i++)
					{
						PassiveAbility passiveAbility = abilitiesController.Abilities[i] as PassiveAbility;
						if (!(passiveAbility == null))
						{
							PassiveAbilityID id = passiveAbility.ID;
							PlayerBehaviour.RestorableState.PassiveAbilityInfo value;
							if (!dictionary.TryGetValue(id, out value))
							{
								dictionary.Add(id, new PlayerBehaviour.RestorableState.PassiveAbilityInfo(id));
							}
							else
							{
								value.abilityCount++;
								dictionary[id] = value;
							}
						}
					}
					PlayerBehaviour.RestorableState.PassiveAbilityInfo[] array = new PlayerBehaviour.RestorableState.PassiveAbilityInfo[dictionary.Values.Count];
					if (array.Length != 0)
					{
						dictionary.Values.CopyTo(array, 0);
					}
					return array;
				}
				return new PlayerBehaviour.RestorableState.PassiveAbilityInfo[0];
			}

			// Token: 0x06002353 RID: 9043 RVA: 0x0006D0D4 File Offset: 0x0006B2D4
			private void GetMobsActivationModifiersInfo(MobsActivationModifiersController modifiersController)
			{
				this.activationModifiers.Clear();
				if (modifiersController != null)
				{
					MobsActivationModifiersController.Slot[] slots = modifiersController.Slots;
					for (int i = 0; i < slots.Length; i++)
					{
						MobsActivationModifiersController.Slot slot = slots[i];
						if (!slot.IsFree())
						{
							PlayerBehaviour.RestorableState.MobsActivationModifierInfo item = new PlayerBehaviour.RestorableState.MobsActivationModifierInfo
							{
								modifierID = slot.CurrentModifier.ModifierID,
								modifierLevel = slot.CurrentModifierLevel,
								modifierSlot = i + 1
							};
							this.activationModifiers.Add(item);
						}
					}
				}
			}

			// Token: 0x06002354 RID: 9044 RVA: 0x0006D158 File Offset: 0x0006B358
			private void GetPlayerUpgradesInfo(IPlayerUpgradesRegistry upgradesRegistry, IPlayerUpgradesFactory upgradesFactory)
			{
				this.upgrades.Clear();
				foreach (IPlayerUpgrade playerUpgrade in upgradesRegistry.GetUpgrades())
				{
					PlayerUpgradeID playerUpgradeID = upgradesFactory.GetPlayerUpgradeID(playerUpgrade);
					if (playerUpgradeID != PlayerUpgradeID.None)
					{
						this.upgrades.Add(new PlayerUpgradeInfo
						{
							upgradeID = playerUpgradeID,
							upgradeLevel = playerUpgrade.ItemLevel
						});
					}
				}
			}

			// Token: 0x06002355 RID: 9045 RVA: 0x0006D1E0 File Offset: 0x0006B3E0
			private void RestorePassiveAbilities(IPassiveAbilitiesController abilitiesController)
			{
				if (abilitiesController != null && this.passiveAbilities.Length != 0)
				{
					IInitializable<IEnumerable<PassiveAbilityID>> initializable = abilitiesController as IInitializable<IEnumerable<PassiveAbilityID>>;
					if (initializable != null)
					{
						List<PassiveAbilityID> list = new List<PassiveAbilityID>(this.passiveAbilities.Length);
						for (int i = 0; i < this.passiveAbilities.Length; i++)
						{
							PlayerBehaviour.RestorableState.PassiveAbilityInfo passiveAbilityInfo = this.passiveAbilities[i];
							for (int j = 0; j < passiveAbilityInfo.abilityCount; j++)
							{
								list.Add(passiveAbilityInfo.abilityID);
							}
						}
						initializable.Initialize(list);
					}
				}
			}

			// Token: 0x06002356 RID: 9046 RVA: 0x0006D25C File Offset: 0x0006B45C
			private void RestoreMobsActivationModifiers(MobsActivationModifiersController modifiersController)
			{
				if (modifiersController != null)
				{
					for (int i = 0; i < this.activationModifiers.Count; i++)
					{
						PlayerBehaviour.RestorableState.MobsActivationModifierInfo mobsActivationModifierInfo = this.activationModifiers[i];
						int modifierSlotIndex = mobsActivationModifierInfo.GetModifierSlotIndex();
						if (modifierSlotIndex < 0)
						{
							modifiersController.AddModifier(mobsActivationModifierInfo.modifierID, mobsActivationModifierInfo.modifierLevel);
						}
						else
						{
							modifiersController.AddModifier(modifierSlotIndex, mobsActivationModifierInfo.modifierID, mobsActivationModifierInfo.modifierLevel);
						}
					}
				}
			}

			// Token: 0x06002357 RID: 9047 RVA: 0x0006D2C4 File Offset: 0x0006B4C4
			private void RestorePlayerUpgrades(IPlayerUpgradesRegistry upgradesRegistry, IPlayerUpgradesFactory upgradesFactory)
			{
				int count = this.upgrades.Count;
				if (count != 0)
				{
					PlayerUpgradesFactoryArgs playerUpgradesFactoryArgs = new PlayerUpgradesFactoryArgs();
					for (int i = 0; i < count; i++)
					{
						PlayerUpgradeInfo playerUpgradeInfo = this.upgrades[i];
						if (playerUpgradeInfo.upgradeID != PlayerUpgradeID.None)
						{
							playerUpgradesFactoryArgs.upgradeID = playerUpgradeInfo.upgradeID;
							playerUpgradesFactoryArgs.UpgradeLevel = playerUpgradeInfo.upgradeLevel;
							IPlayerUpgrade playerUpgrade = upgradesFactory.Create(playerUpgradesFactoryArgs);
							if (playerUpgrade != null)
							{
								upgradesRegistry.AddUpgrade(playerUpgrade);
							}
						}
					}
				}
			}

			// Token: 0x06002358 RID: 9048 RVA: 0x0006D334 File Offset: 0x0006B534
			public PlayerBehaviour.RestorableState Clone()
			{
				return new PlayerBehaviour.RestorableState
				{
					nativeAbilities = this.nativeAbilities.CloneArray<AbilityInfo>(),
					nativeAbilitiesTypes = this.nativeAbilitiesTypes.CloneArray<AbilityTypes>(),
					abilities = this.abilities.CloneArray<AbilityInfo>(),
					abilitiesTypes = this.abilitiesTypes.CloneArray<AbilityTypes>(),
					passiveAbilities = this.passiveAbilities.CloneArray<PlayerBehaviour.RestorableState.PassiveAbilityInfo>(),
					activationModifiers = new List<PlayerBehaviour.RestorableState.MobsActivationModifierInfo>(this.activationModifiers),
					upgrades = new List<PlayerUpgradeInfo>(this.upgrades),
					pickableTextShownCount = this.pickableTextShownCount.CloneArray<int>(),
					exploredLocations = this.exploredLocations.CloneArray<GameLocation.TypeID>(),
					initialHitPoints = this.initialHitPoints,
					containersData = new List<VitalContainer.VitalContainerData>(this.containersData),
					abilityActivatedContainersData = new List<IAbilityActivatedContainerData>(this.abilityActivatedContainersData),
					emptyHPSlotsCount = this.emptyHPSlotsCount
				};
			}

			// Token: 0x06002359 RID: 9049 RVA: 0x0006D419 File Offset: 0x0006B619
			public void UpdatePlayerUpgrades(IEnumerable<PlayerUpgradeInfo> upgradesInfo)
			{
				this.upgrades.Clear();
				this.upgrades.AddRange(upgradesInfo);
			}

			// Token: 0x0600235A RID: 9050 RVA: 0x0006D434 File Offset: 0x0006B634
			public void ResetLocalProgress(PlayerBehaviour.RestorableState defaultState)
			{
				PlayerBehaviour.RestorableState restorableState = defaultState.Clone();
				this.abilities = restorableState.abilities;
				this.abilitiesTypes = restorableState.abilitiesTypes;
				this.nativeAbilities = restorableState.nativeAbilities;
				this.passiveAbilities = restorableState.passiveAbilities;
				this.activationModifiers = restorableState.activationModifiers;
				this.pickableTextShownCount = restorableState.pickableTextShownCount;
				this.initialHitPoints = restorableState.initialHitPoints;
				this.containersData = restorableState.containersData;
				this.abilityActivatedContainersData = restorableState.abilityActivatedContainersData;
				this.emptyHPSlotsCount = restorableState.emptyHPSlotsCount;
			}

			// Token: 0x0600235B RID: 9051 RVA: 0x0006D4C0 File Offset: 0x0006B6C0
			public override void Store(PlayerBehaviour player)
			{
				if (player == null)
				{
					return;
				}
				IServiceRegistry services = player.CurrentGame.Services;
				IPlayerAbilitiesController playerAbilitiesController = player.AbilitiesController as IPlayerAbilitiesController;
				if (playerAbilitiesController != null)
				{
					IGameAbilitiesFactory gameAbilitiesFactory = services.Get<IGameAbilitiesFactory>();
					int count = playerAbilitiesController.AbilitySlots.Count;
					int nonNativeAbilitiesStartSlotIndex = playerAbilitiesController.NonNativeAbilitiesStartSlotIndex;
					this.nativeAbilities = new AbilityInfo[nonNativeAbilitiesStartSlotIndex];
					this.nativeAbilitiesTypes = new AbilityTypes[nonNativeAbilitiesStartSlotIndex];
					this.abilities = new AbilityInfo[count - nonNativeAbilitiesStartSlotIndex];
					this.abilitiesTypes = new AbilityTypes[count - nonNativeAbilitiesStartSlotIndex];
					for (int i = 0; i < count; i++)
					{
						BaseAbility baseAbility = (BaseAbility)playerAbilitiesController.GetAbility(i);
						AbilityInfo abilityInfo = default(AbilityInfo);
						if (baseAbility != null)
						{
							abilityInfo = new AbilityInfo(baseAbility);
							abilityInfo.specialBehaviourDescription = ((gameAbilitiesFactory != null) ? gameAbilitiesFactory.GetAbilitySpecialBehaviourDescription(baseAbility) : null);
						}
						AbilityTypes abilitySlotType = (AbilityTypes)playerAbilitiesController.GetAbilitySlotType(i);
						if (i < nonNativeAbilitiesStartSlotIndex)
						{
							this.nativeAbilities[i] = abilityInfo;
							this.nativeAbilitiesTypes[i] = abilitySlotType;
						}
						else
						{
							int num = i - nonNativeAbilitiesStartSlotIndex;
							this.abilities[num] = abilityInfo;
							this.abilitiesTypes[num] = abilitySlotType;
						}
					}
				}
				else
				{
					this.nativeAbilities = new AbilityInfo[0];
					this.nativeAbilitiesTypes = new AbilityTypes[0];
					this.abilities = new AbilityInfo[0];
					this.abilitiesTypes = new AbilityTypes[0];
				}
				this.passiveAbilities = this.GetPassiveAbilitiesInfo(player.PassiveAbilitiesController);
				this.GetMobsActivationModifiersInfo(player.ActivationModifiersController);
				if (player.HitPointsController != null)
				{
					this.initialHitPoints = player.HitPointsController.InitialHitPoints;
					VitalEnergyHitPointsController vitalEnergyHitPointsController = player.HitPointsController as VitalEnergyHitPointsController;
					if (vitalEnergyHitPointsController != null)
					{
						this.containersData.Clear();
						foreach (VitalContainer vitalContainer in vitalEnergyHitPointsController.VitalContainers)
						{
							VitalContainer.VitalContainerData item = (VitalContainer.VitalContainerData)vitalContainer.GetContainerData();
							this.containersData.Add(item);
						}
						this.emptyHPSlotsCount = vitalEnergyHitPointsController.CurrentSlotsCount - vitalEnergyHitPointsController.VitalContainers.Count;
					}
					IAbilityActivatedContainersController abilityActivatedContainersController = player.HitPointsController as IAbilityActivatedContainersController;
					if (abilityActivatedContainersController != null)
					{
						this.abilityActivatedContainersData = abilityActivatedContainersController.GetContainersData();
					}
				}
				IPlayerUpgradesFactory upgradesFactory;
				if (player.UpgradesRegistry != null && services.TryGet<IPlayerUpgradesFactory>(out upgradesFactory))
				{
					this.GetPlayerUpgradesInfo(player.UpgradesRegistry, upgradesFactory);
				}
			}

			// Token: 0x0600235C RID: 9052 RVA: 0x0006D724 File Offset: 0x0006B924
			public override void Restore(PlayerBehaviour player, object args = null)
			{
				IServiceRegistry services = player.CurrentGame.Services;
				InitialPlayerAbilitiesInfo initialPlayerAbilitiesInfo = args as InitialPlayerAbilitiesInfo;
				if (initialPlayerAbilitiesInfo != null)
				{
					player.abilitiesController.Initialize(initialPlayerAbilitiesInfo.nativeAbilities.IsNotNullOrEmpty<AbilityInfo>() ? initialPlayerAbilitiesInfo.nativeAbilities : this.nativeAbilities, initialPlayerAbilitiesInfo.nativeAbilityTypes.IsNotNullOrEmpty<AbilityTypes>() ? initialPlayerAbilitiesInfo.nativeAbilityTypes : this.nativeAbilitiesTypes, initialPlayerAbilitiesInfo.abilities.IsNotNullOrEmpty<AbilityInfo>() ? initialPlayerAbilitiesInfo.abilities : this.abilities, initialPlayerAbilitiesInfo.abilityTypes.IsNotNullOrEmpty<AbilityTypes>() ? initialPlayerAbilitiesInfo.abilityTypes : this.abilitiesTypes);
				}
				else
				{
					player.abilitiesController.Initialize(this.nativeAbilities, this.nativeAbilitiesTypes, this.abilities, this.abilitiesTypes);
				}
				this.RestorePassiveAbilities(player.PassiveAbilitiesController);
				this.RestoreMobsActivationModifiers(player.ActivationModifiersController);
				if (player.HitPointsController != null)
				{
					if (this.initialHitPoints > 0f)
					{
						player.HitPointsController.InitialHitPoints = this.initialHitPoints;
					}
					if (this.containersData.Any((VitalContainer.VitalContainerData c) => c.IsAlive))
					{
						VitalEnergyHitPointsController vitalEnergyHitPointsController = player.HitPointsController as VitalEnergyHitPointsController;
						if (vitalEnergyHitPointsController != null)
						{
							List<IEnergyContainer> list = new List<IEnergyContainer>();
							foreach (VitalContainer.VitalContainerData containerData in this.containersData)
							{
								VitalContainer vitalContainer = new VitalContainer(new HealthContainer());
								vitalContainer.SetContainerData(containerData);
								list.Add(vitalContainer);
							}
							if (list.Count > 0)
							{
								vitalEnergyHitPointsController.SetContainersData(vitalEnergyHitPointsController.DamageResistanceTimeout, list, vitalEnergyHitPointsController.MaxSlotsCount, list.Count);
							}
							for (int i = 0; i < this.emptyHPSlotsCount; i++)
							{
								vitalEnergyHitPointsController.AddContainerSlot();
							}
						}
						IAbilityActivatedContainersController abilityActivatedContainersController = player.HitPointsController as IAbilityActivatedContainersController;
						if (abilityActivatedContainersController != null)
						{
							abilityActivatedContainersController.SetContainersData(this.abilityActivatedContainersData);
						}
					}
				}
				IPlayerUpgradesFactory upgradesFactory;
				if (player.UpgradesRegistry != null && services.TryGet<IPlayerUpgradesFactory>(out upgradesFactory))
				{
					this.RestorePlayerUpgrades(player.UpgradesRegistry, upgradesFactory);
				}
			}

			// Token: 0x0600235D RID: 9053 RVA: 0x0006D94C File Offset: 0x0006BB4C
			public void ForceAssignPlayerModifier(MobActivationModifierID modifierID)
			{
				if (this.activationModifiers.Count != 0)
				{
					this.activationModifiers[0] = new PlayerBehaviour.RestorableState.MobsActivationModifierInfo
					{
						modifierID = modifierID,
						modifierSlot = 1
					};
					return;
				}
				this.activationModifiers.Add(new PlayerBehaviour.RestorableState.MobsActivationModifierInfo
				{
					modifierID = modifierID,
					modifierSlot = 1
				});
			}

			// Token: 0x040016B2 RID: 5810
			[SerializeField]
			[FormerlySerializedAs("_nativeAbilities")]
			private AbilityInfo[] nativeAbilities;

			// Token: 0x040016B3 RID: 5811
			[SerializeField]
			private AbilityTypes[] nativeAbilitiesTypes;

			// Token: 0x040016B4 RID: 5812
			[SerializeField]
			[FormerlySerializedAs("_abilities")]
			private AbilityInfo[] abilities;

			// Token: 0x040016B5 RID: 5813
			[SerializeField]
			[FormerlySerializedAs("_abilitiesTypes")]
			private AbilityTypes[] abilitiesTypes;

			// Token: 0x040016B6 RID: 5814
			[SerializeField]
			[FormerlySerializedAs("_passiveAbilities")]
			private PlayerBehaviour.RestorableState.PassiveAbilityInfo[] passiveAbilities;

			// Token: 0x040016B7 RID: 5815
			[SerializeField]
			[FormerlySerializedAs("_activationModifiers")]
			private List<PlayerBehaviour.RestorableState.MobsActivationModifierInfo> activationModifiers;

			// Token: 0x040016B8 RID: 5816
			[SerializeField]
			private List<PlayerUpgradeInfo> upgrades;

			// Token: 0x040016B9 RID: 5817
			[SerializeField]
			[FormerlySerializedAs("_pickableTextShownCount")]
			private int[] pickableTextShownCount;

			// Token: 0x040016BA RID: 5818
			[SerializeField]
			private GameLocation.TypeID[] exploredLocations;

			// Token: 0x040016BB RID: 5819
			[SerializeField]
			private float initialHitPoints;

			// Token: 0x040016BC RID: 5820
			[SerializeField]
			private List<VitalContainer.VitalContainerData> containersData = new List<VitalContainer.VitalContainerData>();

			// Token: 0x040016BD RID: 5821
			[SerializeField]
			private List<IAbilityActivatedContainerData> abilityActivatedContainersData = new List<IAbilityActivatedContainerData>();

			// Token: 0x040016BE RID: 5822
			[SerializeField]
			private int emptyHPSlotsCount;

			// Token: 0x020005AE RID: 1454
			[Serializable]
			public struct PassiveAbilityInfo
			{
				// Token: 0x060027D2 RID: 10194 RVA: 0x0007C704 File Offset: 0x0007A904
				public PassiveAbilityInfo(PassiveAbilityID abilityID)
				{
					this.abilityID = abilityID;
					this.abilityCount = 1;
				}

				// Token: 0x04001D22 RID: 7458
				[EnumPopup]
				public PassiveAbilityID abilityID;

				// Token: 0x04001D23 RID: 7459
				public int abilityCount;
			}

			// Token: 0x020005AF RID: 1455
			[Serializable]
			public struct MobsActivationModifierInfo
			{
				// Token: 0x060027D3 RID: 10195 RVA: 0x0007C714 File Offset: 0x0007A914
				public int GetModifierSlotIndex()
				{
					return this.modifierSlot - 1;
				}

				// Token: 0x04001D24 RID: 7460
				[EnumPopup]
				public MobActivationModifierID modifierID;

				// Token: 0x04001D25 RID: 7461
				public int modifierLevel;

				// Token: 0x04001D26 RID: 7462
				[HideInInspector]
				public int modifierSlot;
			}
		}

		// Token: 0x0200044A RID: 1098
		[Serializable]
		public sealed class ExposedParameters : BaseBindableData
		{
			// Token: 0x1700072A RID: 1834
			// (get) Token: 0x0600235E RID: 9054 RVA: 0x0006D9B0 File Offset: 0x0006BBB0
			// (set) Token: 0x0600235F RID: 9055 RVA: 0x0006D9B8 File Offset: 0x0006BBB8
			public float MobsRadius
			{
				get
				{
					return this._mobsRadius;
				}
				set
				{
					DataBindingUtility.UpdatePropertyValue<float>(ref this._mobsRadius, value, this, "MobsRadius");
				}
			}

			// Token: 0x1700072B RID: 1835
			// (get) Token: 0x06002360 RID: 9056 RVA: 0x0006D9CC File Offset: 0x0006BBCC
			// (set) Token: 0x06002361 RID: 9057 RVA: 0x0006D9D4 File Offset: 0x0006BBD4
			public bool UseWSAD
			{
				get
				{
					return this._useWSAD;
				}
				set
				{
					DataBindingUtility.UpdatePropertyValue<bool>(ref this._useWSAD, value, this, "UseWSAD");
				}
			}

			// Token: 0x1700072C RID: 1836
			// (get) Token: 0x06002362 RID: 9058 RVA: 0x0006D9E8 File Offset: 0x0006BBE8
			// (set) Token: 0x06002363 RID: 9059 RVA: 0x0006D9F0 File Offset: 0x0006BBF0
			public bool UseActiveAbilitySlot
			{
				get
				{
					return this._useActiveAbilitySlot;
				}
				set
				{
					DataBindingUtility.UpdatePropertyValue<bool>(ref this._useActiveAbilitySlot, value, this, "UseActiveAbilitySlot");
				}
			}

			// Token: 0x040016BF RID: 5823
			[Header("Точка сбора мобов")]
			public bool useGroupDestinationPointIndicator;

			// Token: 0x040016C0 RID: 5824
			public GameObject destinationPointIndicatorPrefab;

			// Token: 0x040016C1 RID: 5825
			[Header("Таймаут приминения абилок")]
			public float AbilityActivationTimeout = 0.1f;

			// Token: 0x040016C2 RID: 5826
			[ObjectFactoryIDPopup(typeof(BaseAbility))]
			public AbilityID[] AbilitiesWithoutTimeout;

			// Token: 0x040016C3 RID: 5827
			[Header("Радиус мобов")]
			[SerializeField]
			[PropertyField("MobsRadius")]
			private float _mobsRadius;

			// Token: 0x040016C4 RID: 5828
			[Header("Через сколько времени мобы должны возвращаться к игроку")]
			public float DestinationPointIdlingTime = 5f;

			// Token: 0x040016C5 RID: 5829
			[Header("Радиус курсора игрока")]
			public float MobsCursorRadius = 3f;

			// Token: 0x040016C6 RID: 5830
			[Space(5f)]
			public AbilityInfo[] NativeAbilities;

			// Token: 0x040016C7 RID: 5831
			public AbilityTypes[] NativeAbilityTypes;

			// Token: 0x040016C8 RID: 5832
			public AbilityInfo[] Abilities;

			// Token: 0x040016C9 RID: 5833
			public AbilityTypes[] AbilityTypes;

			// Token: 0x040016CA RID: 5834
			public MobsActivationModifiersController.Slot[] ActivationModifiersSlots;

			// Token: 0x040016CB RID: 5835
			[Space(5f)]
			[Header("Управление")]
			[Header("Использовать WSAD")]
			[SerializeField]
			[PropertyField("UseWSAD")]
			private bool _useWSAD;

			// Token: 0x040016CC RID: 5836
			[Header("Активный слот способностей")]
			[SerializeField]
			[PropertyField("UseActiveAbilitySlot")]
			private bool _useActiveAbilitySlot;

			// Token: 0x040016CD RID: 5837
			[SerializeField]
			[Header("Данные для использования наиболее подходящих абилити")]
			public PlayerAbilitiesController.AbilitySlotFallback[] NativeAbilitySlotsFallback;

			// Token: 0x040016CE RID: 5838
			[FormerlySerializedAs("MobsLayerMask")]
			[Header("Кликабельные слои мобов")]
			public LayerMask ClickableMobLayers;
		}
	}
}
