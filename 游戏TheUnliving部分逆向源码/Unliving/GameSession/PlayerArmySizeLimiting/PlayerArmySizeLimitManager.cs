using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.CollectionsExtensions;
using Common.ServiceRegistry;
using Game.Core;
using Game.Damage;
using Game.GameLoop;
using Game.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Abilities.VFX;
using Unliving.AbilityResources;
using Unliving.GameSession.PlayerMobsLeveling;
using Unliving.Mobs;
using Unliving.Mobs.VFX;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.GameSession.PlayerArmySizeLimiting
{
	// Token: 0x020002C1 RID: 705
	[CreateAssetMenu(fileName = "PlayerArmySizeLimitManager", menuName = "Game/Player Army Size Limit Manager")]
	[Service(typeof(PlayerArmySizeLimitManager), new Type[]
	{
		typeof(IPlayerArmySizeLimitManager),
		typeof(IPlayerMobsLevelingManager)
	})]
	public sealed class PlayerArmySizeLimitManager : GlobalManagerBase, IPlayerArmySizeLimitManager, IPlayerMobsLevelingManager
	{
		// Token: 0x17000538 RID: 1336
		// (get) Token: 0x06001872 RID: 6258 RVA: 0x0004C85C File Offset: 0x0004AA5C
		// (set) Token: 0x06001873 RID: 6259 RVA: 0x0004C864 File Offset: 0x0004AA64
		public bool IsActive
		{
			get
			{
				return this.isActive;
			}
			set
			{
				this.isActive = value;
			}
		}

		// Token: 0x17000539 RID: 1337
		// (get) Token: 0x06001874 RID: 6260 RVA: 0x0004C86D File Offset: 0x0004AA6D
		// (set) Token: 0x06001875 RID: 6261 RVA: 0x0004C882 File Offset: 0x0004AA82
		public int MaxPlayerArmySize
		{
			get
			{
				return (int)this.armySizeModifiersSum.GetModifiedStatValue((float)this.maxPlayerArmySize);
			}
			set
			{
				this.maxPlayerArmySize = value;
			}
		}

		// Token: 0x1700053A RID: 1338
		// (get) Token: 0x06001876 RID: 6262 RVA: 0x0004C88B File Offset: 0x0004AA8B
		// (set) Token: 0x06001877 RID: 6263 RVA: 0x0004C89E File Offset: 0x0004AA9E
		public float MobStatsGainCoeff
		{
			get
			{
				return this.mobsGainCoeffModifiersSum.GetModifiedStatValue(this.mobStatsGainCoeff);
			}
			set
			{
				this.mobStatsGainCoeff = value;
			}
		}

		// Token: 0x1700053B RID: 1339
		// (get) Token: 0x06001878 RID: 6264 RVA: 0x0004C8A8 File Offset: 0x0004AAA8
		public int CurrentMaxMobLevel
		{
			get
			{
				if (this.currentMaxMobLevel < 0)
				{
					this.currentMaxMobLevel = 0;
					for (int i = this.mobsByLevel.Count - 1; i >= 0; i--)
					{
						if (this.mobsByLevel[i].Count != 0)
						{
							this.currentMaxMobLevel = i + 1;
							break;
						}
					}
				}
				return this.currentMaxMobLevel;
			}
		}

		// Token: 0x1700053C RID: 1340
		// (get) Token: 0x06001879 RID: 6265 RVA: 0x0004C901 File Offset: 0x0004AB01
		public int TotalPlayerArmyPower
		{
			get
			{
				return this.totalPlayerArmyPower;
			}
		}

		// Token: 0x1700053D RID: 1341
		// (get) Token: 0x0600187A RID: 6266 RVA: 0x0004C909 File Offset: 0x0004AB09
		public int CurrentPlayerArmySize
		{
			get
			{
				return this.mobsCount;
			}
		}

		// Token: 0x140000F0 RID: 240
		// (add) Token: 0x0600187B RID: 6267 RVA: 0x0004C914 File Offset: 0x0004AB14
		// (remove) Token: 0x0600187C RID: 6268 RVA: 0x0004C94C File Offset: 0x0004AB4C
		public event Action<IPlayerMobsLevelingManager> RegisteredMobsCollectionChanged;

		// Token: 0x140000F1 RID: 241
		// (add) Token: 0x0600187D RID: 6269 RVA: 0x0004C984 File Offset: 0x0004AB84
		// (remove) Token: 0x0600187E RID: 6270 RVA: 0x0004C9BC File Offset: 0x0004ABBC
		public event Action<IPlayerMobsLevelingManager, int> TotalPlayerArmyPowerChanged;

		// Token: 0x140000F2 RID: 242
		// (add) Token: 0x0600187F RID: 6271 RVA: 0x0004C9F4 File Offset: 0x0004ABF4
		// (remove) Token: 0x06001880 RID: 6272 RVA: 0x0004CA2C File Offset: 0x0004AC2C
		public event Action<IPlayerMobsLevelingManager, ILevelablePlayerMob> PlayerMobLevelAdvanced;

		// Token: 0x140000F3 RID: 243
		// (add) Token: 0x06001881 RID: 6273 RVA: 0x0004CA64 File Offset: 0x0004AC64
		// (remove) Token: 0x06001882 RID: 6274 RVA: 0x0004CA9C File Offset: 0x0004AC9C
		public event Action<IPlayerArmySizeLimitManager, BaseGameMob, BaseGameMob> PlayerMobConsumed;

		// Token: 0x06001883 RID: 6275 RVA: 0x0004CAD4 File Offset: 0x0004ACD4
		private void CountMob(ref PlayerArmySizeLimitManager.MobNode mobNode, bool add)
		{
			int mobType = mobNode.MobType;
			int i = 0;
			while (i < this.mobTypesCount)
			{
				if (this.mobsByTypeCounters[i].MobsType == mobType)
				{
					if (add)
					{
						PlayerArmySizeLimitManager.MobsCounter[] array = this.mobsByTypeCounters;
						int num = i;
						array[num].mobsCount = array[num].mobsCount + 1;
						mobNode.mobsCounterIndex = i;
						return;
					}
					PlayerArmySizeLimitManager.MobsCounter[] array2 = this.mobsByTypeCounters;
					int num2 = i;
					array2[num2].mobsCount = array2[num2].mobsCount - 1;
					return;
				}
				else
				{
					i++;
				}
			}
			if (add)
			{
				mobNode.mobsCounterIndex = this.mobTypesCount;
				Common.CollectionsExtensions.Extensions.Add<PlayerArmySizeLimitManager.MobsCounter>(new PlayerArmySizeLimitManager.MobsCounter(mobType), ref this.mobsByTypeCounters, ref this.mobTypesCount, 16);
			}
		}

		// Token: 0x06001884 RID: 6276 RVA: 0x0004CB6D File Offset: 0x0004AD6D
		private void CountInvalidMob()
		{
			this.invalidMobsCount++;
		}

		// Token: 0x06001885 RID: 6277 RVA: 0x0004CB80 File Offset: 0x0004AD80
		private void RegisterMob(BaseGameMob mob)
		{
			if (!(mob is ILevelablePlayerMob))
			{
				return;
			}
			PlayerArmySizeLimitManager.MobNode item;
			if (PlayerArmySizeLimitManager.MobNode.TryCreateMobNode(mob, out item))
			{
				this.CountMob(ref item, true);
				item.ApplyCurrentMobLevelStatModifiers(this.mobsLevelingData, this.MobStatsGainCoeff);
				this.RegisterMobWithLevel(ref item);
				this.totalPlayerArmyPower += item.GetMobPowerEstimation();
				Common.CollectionsExtensions.Extensions.Add<PlayerArmySizeLimitManager.MobNode>(item, ref this.mobs, ref this.mobsCount, 16);
			}
		}

		// Token: 0x06001886 RID: 6278 RVA: 0x0004CBED File Offset: 0x0004ADED
		private void UnregisterMob(ref PlayerArmySizeLimitManager.MobNode mobNode, int mobNodeIndex)
		{
			this.CountMob(ref mobNode, false);
			this.UnregisterMobWithLevel(ref mobNode);
			this.totalPlayerArmyPower -= mobNode.GetMobPowerEstimation();
			this.mobs.RemoveBySwap(mobNodeIndex, ref this.mobsCount);
		}

		// Token: 0x06001887 RID: 6279 RVA: 0x0004CC24 File Offset: 0x0004AE24
		private List<ILevelablePlayerMob> GetMobsWithLevel(int mobsLevel, bool isAddingNewMob = false)
		{
			int num = mobsLevel - 1;
			if (isAddingNewMob && num == this.mobsByLevel.Count)
			{
				this.mobsByLevel.Add(new List<ILevelablePlayerMob>(32));
			}
			return this.mobsByLevel[num];
		}

		// Token: 0x06001888 RID: 6280 RVA: 0x0004CC64 File Offset: 0x0004AE64
		private void SetCurrentMaxMobLevelDirty()
		{
			this.currentMaxMobLevel = -1;
		}

		// Token: 0x06001889 RID: 6281 RVA: 0x0004CC6D File Offset: 0x0004AE6D
		private void RegisterMobWithLevel(ref PlayerArmySizeLimitManager.MobNode node)
		{
			this.GetMobsWithLevel(node.CurrentMobLevel, true).Add(node.MobLevelingInterface);
			this.SetCurrentMaxMobLevelDirty();
			this.isRegisteredMobGroupsDirty = true;
		}

		// Token: 0x0600188A RID: 6282 RVA: 0x0004CC94 File Offset: 0x0004AE94
		private void UnregisterMobWithLevel(ILevelablePlayerMob mob, int lastMobLevel = -1)
		{
			int mobsLevel = (lastMobLevel > 0) ? lastMobLevel : mob.MobLevel;
			List<ILevelablePlayerMob> mobsWithLevel = this.GetMobsWithLevel(mobsLevel, false);
			int index = mobsWithLevel.IndexOf(mob);
			mobsWithLevel.RemoveBySwap(index);
			this.SetCurrentMaxMobLevelDirty();
			this.isRegisteredMobGroupsDirty = true;
		}

		// Token: 0x0600188B RID: 6283 RVA: 0x0004CCD2 File Offset: 0x0004AED2
		private void UnregisterMobWithLevel(ref PlayerArmySizeLimitManager.MobNode node)
		{
			this.UnregisterMobWithLevel(node.MobLevelingInterface, -1);
		}

		// Token: 0x0600188C RID: 6284 RVA: 0x0004CCE1 File Offset: 0x0004AEE1
		private void CheckTotalArmyPower()
		{
			if (this.totalPlayerArmyPower == this.lastTotalPlayerArmyPower)
			{
				return;
			}
			Action<IPlayerMobsLevelingManager, int> totalPlayerArmyPowerChanged = this.TotalPlayerArmyPowerChanged;
			if (totalPlayerArmyPowerChanged != null)
			{
				totalPlayerArmyPowerChanged(this, this.totalPlayerArmyPower);
			}
			this.lastTotalPlayerArmyPower = this.totalPlayerArmyPower;
		}

		// Token: 0x0600188D RID: 6285 RVA: 0x0004CD16 File Offset: 0x0004AF16
		private void ResetModifiers()
		{
			this.armySizeModifiersSum = MobStatModifier.Neutral;
			this.mobsGainCoeffModifiersSum = MobStatModifier.Neutral;
		}

		// Token: 0x0600188E RID: 6286 RVA: 0x0004CD2E File Offset: 0x0004AF2E
		private void CreateMobLevelupEffect(GameMobVFXController mobVFX)
		{
			if (this.levelupEffectInfo.EffectPrefab != null)
			{
				this.levelupEffectInfo.isOneShotEffect = true;
				mobVFX.AttachEffect(this.levelupEffectInfo, this);
			}
		}

		// Token: 0x0600188F RID: 6287 RVA: 0x0004CD60 File Offset: 0x0004AF60
		private void CreateMobLevelHighlightEffect(GameMobVFXController mobVFX, int mobLevel)
		{
			int num = this.levelDependentMobEffects.Length;
			if (num == 0)
			{
				return;
			}
			int num2 = mobLevel - 2;
			if (num2 >= 0)
			{
				if (num2 >= num)
				{
					num2 = num - 1;
				}
				AbilityVFXController.ObjectEffectInfo objectEffectInfo = this.levelDependentMobEffects[num2];
				objectEffectInfo.HasDuration = true;
				mobVFX.AttachEffect(objectEffectInfo, this);
			}
		}

		// Token: 0x06001890 RID: 6288 RVA: 0x0004CDA3 File Offset: 0x0004AFA3
		private void ResetNextUpdateTime(float time)
		{
			this.nextUpdateTime = time + 1f / (float)Mathf.Max(this.updateRate, 1);
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x0004CDC0 File Offset: 0x0004AFC0
		private void ResetNextUpdateTime()
		{
			this.ResetNextUpdateTime(Time.time);
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x0004CDD0 File Offset: 0x0004AFD0
		private void RemoveInvalidMobs()
		{
			if (this.invalidMobsCount == 0)
			{
				return;
			}
			GameMobsGroupControllerBase group = this.currentPlayer.Group;
			int num = 0;
			while (this.invalidMobsCount != 0 && num < this.mobsCount)
			{
				ref PlayerArmySizeLimitManager.MobNode ptr = ref this.mobs[num];
				if (!ptr.IsValid(group))
				{
					BaseGameMob baseGameMob = ptr.shouldBeKilled ? ptr.TargetMob : null;
					this.UnregisterMob(ref ptr, num);
					this.invalidMobsCount--;
					if (baseGameMob != null)
					{
						if (this.destroyRedundantMobsInsteadOfKill)
						{
							UnityEngine.Object.Destroy(baseGameMob.gameObject);
						}
						else
						{
							baseGameMob.KillMob(this);
						}
					}
				}
				else
				{
					num++;
				}
			}
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x0004CE70 File Offset: 0x0004B070
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.mobsByTypeCounters = new PlayerArmySizeLimitManager.MobsCounter[32];
			this.mobs = new PlayerArmySizeLimitManager.MobNode[512];
			this.lastTotalPlayerArmyPower = -1;
			this.ResetModifiers();
		}

		// Token: 0x06001894 RID: 6292 RVA: 0x0004CEA4 File Offset: 0x0004B0A4
		public void ModifyMaxArmySize(MobStatModifier modifier)
		{
			if (modifier == default(MobStatModifier) || modifier == MobStatModifier.Neutral)
			{
				return;
			}
			this.armySizeModifiersSum.Combine(modifier);
		}

		// Token: 0x06001895 RID: 6293 RVA: 0x0004CEDC File Offset: 0x0004B0DC
		public void ModifyMobsGainCoeff(MobStatModifier modifier)
		{
			if (modifier == default(MobStatModifier) || modifier == MobStatModifier.Neutral)
			{
				return;
			}
			this.mobsGainCoeffModifiersSum.Combine(modifier);
		}

		// Token: 0x06001896 RID: 6294 RVA: 0x0004CF14 File Offset: 0x0004B114
		IReadOnlyList<ILevelablePlayerMob> IPlayerMobsLevelingManager.GetMobsWithLevel(int mobsLevel)
		{
			if (mobsLevel <= 0 || mobsLevel > this.mobsByLevel.Count)
			{
				return null;
			}
			return this.GetMobsWithLevel(mobsLevel, false);
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x0004CF34 File Offset: 0x0004B134
		protected override void OnSceneLoaded(Scene scene)
		{
			this.lastTotalPlayerArmyPower = -1;
			this.totalPlayerArmyPower = 0;
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider) && (this.currentPlayer = playerProvider.CurrentPlayer) != null)
			{
				GameMobsGroupControllerBase group = this.currentPlayer.Group;
				IReadOnlyList<BaseGameMob> readOnlyList = group.Mobs;
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					this.RegisterMob(readOnlyList[i]);
				}
				group.MobAdded += this.OnPlayerGroupMobAdded;
				group.MobRemoved += this.OnPlayerGroupMobRemoved;
				this.currentPlayer.Destroyed += this.OnPlayerDestroyed;
				if (base.CurrentGame.Services.TryGet<IGameLoopAccessProvider>(out this.gameLoopAccessProvider))
				{
					this.gameLoopAccessProvider.UpdatePerformed += this.OnUpdate;
				}
			}
			this.nextUpdateTime = 0.5f;
		}

		// Token: 0x06001898 RID: 6296 RVA: 0x0004D02C File Offset: 0x0004B22C
		private void OnUpdate()
		{
			if (!this.isActive)
			{
				return;
			}
			this.RemoveInvalidMobs();
			this.CheckTotalArmyPower();
			if (this.isRegisteredMobGroupsDirty)
			{
				Action<IPlayerMobsLevelingManager> registeredMobsCollectionChanged = this.RegisteredMobsCollectionChanged;
				if (registeredMobsCollectionChanged != null)
				{
					registeredMobsCollectionChanged(this);
				}
				this.isRegisteredMobGroupsDirty = false;
			}
			if ((float)this.maxPlayerArmySize <= 0f)
			{
				return;
			}
			float time = Time.time;
			if (time < this.nextUpdateTime)
			{
				return;
			}
			int num = this.MaxPlayerArmySize;
			if (num < 0)
			{
				num = 0;
			}
			if (this.mobsCount > num)
			{
				GameMobsGroupControllerBase group = this.currentPlayer.Group;
				for (int i = 0; i < this.mobsCount; i++)
				{
					ref PlayerArmySizeLimitManager.MobNode ptr = ref this.mobs[i];
					ptr.UpdateSortingKey(this.mobsByTypeCounters[ptr.mobsCounterIndex].mobsCount);
				}
				Array.Sort<PlayerArmySizeLimitManager.MobNode>(this.mobs, 0, this.mobsCount);
				int num2 = this.mobsCount;
				float mobGainCoeff = this.MobStatsGainCoeff;
				int num3 = 0;
				while (num3 < this.mobsCount - 1 && num2 > num)
				{
					ref PlayerArmySizeLimitManager.MobNode ptr2 = ref this.mobs[num3];
					ref PlayerArmySizeLimitManager.MobNode ptr3 = ref this.mobs[num3 + 1];
					if (ptr2.MobType != ptr3.MobType)
					{
						num3++;
					}
					else
					{
						num2--;
						ptr2.Invalidate(true);
						int currentMobLevel = ptr3.CurrentMobLevel;
						int mobPowerEstimation = ptr3.GetMobPowerEstimation();
						if (ptr3.AdvanceMobLevel(this.mobsLevelingData, mobGainCoeff))
						{
							BaseGameMob targetMob = ptr3.TargetMob;
							this.totalPlayerArmyPower -= mobPowerEstimation;
							this.totalPlayerArmyPower += ptr3.GetMobPowerEstimation();
							this.UnregisterMobWithLevel(ptr3.MobLevelingInterface, currentMobLevel);
							this.RegisterMobWithLevel(ref ptr3);
							GameMobVFXController mobVFX;
							if (targetMob.TryGetComponent<GameMobVFXController>(out mobVFX))
							{
								this.CreateMobLevelupEffect(mobVFX);
								this.CreateMobLevelHighlightEffect(mobVFX, ptr3.CurrentMobLevel);
							}
							Action<IPlayerMobsLevelingManager, ILevelablePlayerMob> playerMobLevelAdvanced = this.PlayerMobLevelAdvanced;
							if (playerMobLevelAdvanced != null)
							{
								playerMobLevelAdvanced(this, (ILevelablePlayerMob)targetMob);
							}
						}
						Action<IPlayerArmySizeLimitManager, BaseGameMob, BaseGameMob> playerMobConsumed = this.PlayerMobConsumed;
						if (playerMobConsumed != null)
						{
							playerMobConsumed(this, ptr2.TargetMob, ptr3.TargetMob);
						}
						group.RemoveMob(ptr2.TargetMob);
						num3 += 2;
					}
				}
				this.CheckTotalArmyPower();
			}
			this.ResetNextUpdateTime(time);
		}

		// Token: 0x06001899 RID: 6297 RVA: 0x0004D25A File Offset: 0x0004B45A
		private void OnPlayerGroupMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.RegisterMob(mob);
			this.ResetNextUpdateTime();
		}

		// Token: 0x0600189A RID: 6298 RVA: 0x0004D269 File Offset: 0x0004B469
		private void OnPlayerGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.CountInvalidMob();
		}

		// Token: 0x0600189B RID: 6299 RVA: 0x0004D274 File Offset: 0x0004B474
		private void OnPlayerDestroyed(object obj)
		{
			this.mobTypesCount = 0;
			this.mobsCount = 0;
			this.invalidMobsCount = 0;
			this.mobsByLevel.Clear();
			this.ResetModifiers();
			this.SetCurrentMaxMobLevelDirty();
			if (this.gameLoopAccessProvider != null)
			{
				this.gameLoopAccessProvider.UpdatePerformed -= this.OnUpdate;
				this.gameLoopAccessProvider = null;
			}
			this.currentPlayer.Group.MobAdded -= this.OnPlayerGroupMobAdded;
			this.currentPlayer.Destroyed -= this.OnPlayerDestroyed;
			this.currentPlayer = null;
		}

		// Token: 0x04000DBE RID: 3518
		[SerializeField]
		private bool isActive;

		// Token: 0x04000DBF RID: 3519
		public PlayerMobsLevelingDataAsset mobsLevelingData;

		// Token: 0x04000DC0 RID: 3520
		[SerializeField]
		private int maxPlayerArmySize = 90;

		// Token: 0x04000DC1 RID: 3521
		[SerializeField]
		private float mobStatsGainCoeff = 1f;

		// Token: 0x04000DC2 RID: 3522
		public int updateRate = 10;

		// Token: 0x04000DC3 RID: 3523
		public bool destroyRedundantMobsInsteadOfKill;

		// Token: 0x04000DC4 RID: 3524
		[Space]
		public GameMobVFXController.GenericAttachableEffectInfo levelupEffectInfo;

		// Token: 0x04000DC5 RID: 3525
		public AbilityVFXController.ObjectEffectInfo[] levelDependentMobEffects;

		// Token: 0x04000DC6 RID: 3526
		private readonly List<List<ILevelablePlayerMob>> mobsByLevel = new List<List<ILevelablePlayerMob>>(16);

		// Token: 0x04000DC7 RID: 3527
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000DC8 RID: 3528
		private IGameLoopAccessProvider gameLoopAccessProvider;

		// Token: 0x04000DC9 RID: 3529
		private PlayerArmySizeLimitManager.MobsCounter[] mobsByTypeCounters;

		// Token: 0x04000DCA RID: 3530
		private int mobTypesCount;

		// Token: 0x04000DCB RID: 3531
		private PlayerArmySizeLimitManager.MobNode[] mobs;

		// Token: 0x04000DCC RID: 3532
		private int mobsCount;

		// Token: 0x04000DCD RID: 3533
		private int invalidMobsCount;

		// Token: 0x04000DCE RID: 3534
		private float nextUpdateTime;

		// Token: 0x04000DCF RID: 3535
		private MobStatModifier armySizeModifiersSum;

		// Token: 0x04000DD0 RID: 3536
		private MobStatModifier mobsGainCoeffModifiersSum;

		// Token: 0x04000DD1 RID: 3537
		private int lastTotalPlayerArmyPower;

		// Token: 0x04000DD2 RID: 3538
		private int totalPlayerArmyPower;

		// Token: 0x04000DD3 RID: 3539
		private int currentMaxMobLevel;

		// Token: 0x04000DD4 RID: 3540
		private bool isRegisteredMobGroupsDirty;

		// Token: 0x02000526 RID: 1318
		private struct MobNode : IComparable<PlayerArmySizeLimitManager.MobNode>
		{
			// Token: 0x06002648 RID: 9800 RVA: 0x00077E7C File Offset: 0x0007607C
			public static bool TryCreateMobNode(BaseGameMob mob, out PlayerArmySizeLimitManager.MobNode mobNode)
			{
				if (!mob.IsSummoned && !mob.IsSacrificed)
				{
					ILevelablePlayerMob levelablePlayerMob = mob as ILevelablePlayerMob;
					if (levelablePlayerMob != null)
					{
						mobNode = new PlayerArmySizeLimitManager.MobNode(mob, levelablePlayerMob);
						return mobNode.IsValid();
					}
				}
				mobNode = new PlayerArmySizeLimitManager.MobNode(0);
				return false;
			}

			// Token: 0x170007BB RID: 1979
			// (get) Token: 0x06002649 RID: 9801 RVA: 0x00077EC4 File Offset: 0x000760C4
			public int CurrentMobLevel
			{
				get
				{
					return this.MobLevelingInterface.MobLevel;
				}
			}

			// Token: 0x0600264A RID: 9802 RVA: 0x00077ED4 File Offset: 0x000760D4
			private bool SetMobLevel(PlayerMobsLevelingDataAsset levelingData, int newMobLevel, float mobGainCoeff)
			{
				if (newMobLevel < 1)
				{
					newMobLevel = 1;
				}
				if (levelingData != null)
				{
					int mobLevel = this.MobLevelingInterface.MobLevel;
					IReadOnlyList<TargetedMobStatModifier> modifiers;
					if (mobLevel > 1 && levelingData.TryGetMobStatsModifiers(mobLevel, out modifiers))
					{
						PlayerArmySizeLimitManager.MobNode.<SetMobLevel>g__HandleModifiers|11_0(this.TargetMob.StatsController, modifiers, false, mobGainCoeff);
					}
					IReadOnlyList<TargetedMobStatModifier> modifiers2;
					if (levelingData.TryGetMobStatsModifiers(newMobLevel, out modifiers2))
					{
						PlayerArmySizeLimitManager.MobNode.<SetMobLevel>g__HandleModifiers|11_0(this.TargetMob.StatsController, modifiers2, true, (newMobLevel > 1) ? mobGainCoeff : 1f);
					}
					this.MobLevelingInterface.MobLevel = newMobLevel;
					return true;
				}
				return false;
			}

			// Token: 0x0600264B RID: 9803 RVA: 0x00077F57 File Offset: 0x00076157
			public MobNode(int mobType)
			{
				this = default(PlayerArmySizeLimitManager.MobNode);
				this.MobType = mobType;
			}

			// Token: 0x0600264C RID: 9804 RVA: 0x00077F68 File Offset: 0x00076168
			public MobNode(BaseGameMob targetMob, ILevelablePlayerMob mobLevelingInterface)
			{
				this.MobType = mobLevelingInterface.MobType;
				this.TargetMob = targetMob;
				this.MobLevelingInterface = mobLevelingInterface;
				this.hpController = targetMob.HitPointsController;
				this.mobsCounterIndex = -1;
				this.sortingKey = 0;
				this.isInvalidated = false;
				this.shouldBeKilled = false;
			}

			// Token: 0x0600264D RID: 9805 RVA: 0x00077FB7 File Offset: 0x000761B7
			public bool IsValid()
			{
				return !this.isInvalidated && this.MobType > 0;
			}

			// Token: 0x0600264E RID: 9806 RVA: 0x00077FCC File Offset: 0x000761CC
			public bool IsValid(GameMobsGroupControllerBase playerGroup)
			{
				return this.IsValid() && this.TargetMob != null && this.TargetMob.Group == playerGroup;
			}

			// Token: 0x0600264F RID: 9807 RVA: 0x00077FF4 File Offset: 0x000761F4
			public void UpdateSortingKey(int sameTypeMobsCount)
			{
				ushort num = (ushort)Mathf.Max(255 - sameTypeMobsCount, 0);
				byte b = (byte)Mathf.Max(this.MobLevelingInterface.MobLevel, 1);
				byte b2 = (byte)(this.hpController.GetNormalizedHitPoints() * 255f);
				this.sortingKey = ((int)num << 16 | (int)((ushort)((int)b | (int)b2 << 8)));
			}

			// Token: 0x06002650 RID: 9808 RVA: 0x00078047 File Offset: 0x00076247
			public int CompareTo(PlayerArmySizeLimitManager.MobNode other)
			{
				return this.sortingKey.CompareTo(other.sortingKey);
			}

			// Token: 0x06002651 RID: 9809 RVA: 0x0007805A File Offset: 0x0007625A
			public int GetMobPowerEstimation()
			{
				return 1 << Mathf.Max(this.MobLevelingInterface.MobLevel - 1, 0);
			}

			// Token: 0x06002652 RID: 9810 RVA: 0x00078074 File Offset: 0x00076274
			public bool ApplyCurrentMobLevelStatModifiers(PlayerMobsLevelingDataAsset levelingData, float mobGainCoeff)
			{
				return this.SetMobLevel(levelingData, this.MobLevelingInterface.MobLevel, mobGainCoeff);
			}

			// Token: 0x06002653 RID: 9811 RVA: 0x00078089 File Offset: 0x00076289
			public bool AdvanceMobLevel(PlayerMobsLevelingDataAsset levelingData, float mobGainCoeff)
			{
				return this.SetMobLevel(levelingData, this.MobLevelingInterface.MobLevel + 1, mobGainCoeff);
			}

			// Token: 0x06002654 RID: 9812 RVA: 0x000780A0 File Offset: 0x000762A0
			public void Invalidate(bool killMob)
			{
				this.mobsCounterIndex = -1;
				this.isInvalidated = true;
				this.shouldBeKilled = killMob;
				if (killMob)
				{
					AbilityResourcesGenerator resourcesGenerator = this.TargetMob.ResourcesGenerator;
					GameMobAIController aicontroller = this.TargetMob.AIController;
					if (resourcesGenerator != null)
					{
						resourcesGenerator.IsActive = false;
					}
					if (aicontroller != null)
					{
						aicontroller.IsActive = false;
					}
				}
			}

			// Token: 0x06002655 RID: 9813 RVA: 0x000780F4 File Offset: 0x000762F4
			[CompilerGenerated]
			internal static void <SetMobLevel>g__HandleModifiers|11_0(StatsControllerBase<MobStatModifier> statsController, IReadOnlyList<TargetedMobStatModifier> modifiers, bool add, float gainCoeff)
			{
				MobStatModifier modifier = new MobStatModifier(0f, 0f, gainCoeff);
				for (int i = 0; i < modifiers.Count; i++)
				{
					MobStatModifier statModifier = modifiers[i].ToStatModifier();
					MobStatID targetStat = modifiers[i].targetStat;
					if (gainCoeff != 1f && targetStat != MobStatID.MobRottingSpeed && targetStat != MobStatID.MobCrowdPassPriority)
					{
						statModifier.Combine(modifier);
					}
					if (add)
					{
						statsController.AddModifier((int)targetStat, statModifier);
					}
					else
					{
						statsController.RemoveModifier((int)targetStat, statModifier);
					}
				}
			}

			// Token: 0x04001B42 RID: 6978
			public readonly int MobType;

			// Token: 0x04001B43 RID: 6979
			public readonly BaseGameMob TargetMob;

			// Token: 0x04001B44 RID: 6980
			public readonly ILevelablePlayerMob MobLevelingInterface;

			// Token: 0x04001B45 RID: 6981
			public int mobsCounterIndex;

			// Token: 0x04001B46 RID: 6982
			public bool shouldBeKilled;

			// Token: 0x04001B47 RID: 6983
			private readonly IDamageable hpController;

			// Token: 0x04001B48 RID: 6984
			private int sortingKey;

			// Token: 0x04001B49 RID: 6985
			private bool isInvalidated;
		}

		// Token: 0x02000527 RID: 1319
		private struct MobsCounter
		{
			// Token: 0x06002656 RID: 9814 RVA: 0x00078171 File Offset: 0x00076371
			public MobsCounter(int mobsType)
			{
				this.MobsType = mobsType;
				this.mobsCount = 1;
			}

			// Token: 0x04001B4A RID: 6986
			public readonly int MobsType;

			// Token: 0x04001B4B RID: 6987
			public int mobsCount;
		}
	}
}
