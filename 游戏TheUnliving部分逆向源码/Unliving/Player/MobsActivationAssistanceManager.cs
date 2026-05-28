using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.CollectionsExtensions;
using Common.Editor;
using Common.ServiceRegistry;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.GameLoop;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000148 RID: 328
	[Service(typeof(MobsActivationAssistanceManager), new Type[]
	{
		typeof(IMobsActivationAssistanceManager)
	})]
	[CreateAssetMenu(fileName = "MobsActivationAssistanceManager", menuName = "Game/Mobs Activation Assistance Manager")]
	public sealed class MobsActivationAssistanceManager : GlobalManagerBase, IMobsActivationAssistanceManager
	{
		// Token: 0x06000887 RID: 2183 RVA: 0x0001C1C1 File Offset: 0x0001A3C1
		private static float GetTime()
		{
			return Time.unscaledTime;
		}

		// Token: 0x17000169 RID: 361
		// (get) Token: 0x06000888 RID: 2184 RVA: 0x0001C1C8 File Offset: 0x0001A3C8
		// (set) Token: 0x06000889 RID: 2185 RVA: 0x0001C1D0 File Offset: 0x0001A3D0
		public float MobsNormalizedHPThreshold
		{
			get
			{
				return this.mobsNormalizedHPThreshold;
			}
			set
			{
				this.mobsNormalizedHPThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x1700016A RID: 362
		// (get) Token: 0x0600088A RID: 2186 RVA: 0x0001C1DE File Offset: 0x0001A3DE
		public int HighlightedMobsCount
		{
			get
			{
				return this.currentBuffCount;
			}
		}

		// Token: 0x1700016B RID: 363
		// (get) Token: 0x0600088B RID: 2187 RVA: 0x0001C1E6 File Offset: 0x0001A3E6
		// (set) Token: 0x0600088C RID: 2188 RVA: 0x0001C1EE File Offset: 0x0001A3EE
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

		// Token: 0x0600088D RID: 2189 RVA: 0x0001C1F8 File Offset: 0x0001A3F8
		private int GetActivationTypeConstraintIndex(MobActivationAbilityType activationType)
		{
			for (int i = 0; i < this.activationTypeConstraints.Length; i++)
			{
				if (this.activationTypeConstraints[i].activationType == activationType)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x0600088E RID: 2190 RVA: 0x0001C22C File Offset: 0x0001A42C
		private void RegisterMob(BaseGameMob mob)
		{
			BaseAbility baseAbility;
			MobActivationAbilityType mobActivationAbilityType;
			if (mob.BuffsController == null || !mob.CanBeSacrificed(this.player, false) || !mob.TryGetMobActivationAbility(out baseAbility, out mobActivationAbilityType))
			{
				return;
			}
			Common.CollectionsExtensions.Extensions.Add<MobsActivationAssistanceManager.PlayerMobInfo>(new MobsActivationAssistanceManager.PlayerMobInfo(this, mob), ref this.currentMobs, ref this.currentMobsCount, 16);
			mob.Sacrificed += this.OnPlayerMobActivated;
		}

		// Token: 0x0600088F RID: 2191 RVA: 0x0001C289 File Offset: 0x0001A489
		private void UnregisterMob(BaseGameMob mob)
		{
			this.currentMobs.RemoveBySwap(this.GetMobIndex(mob), ref this.currentMobsCount);
			mob.Sacrificed -= this.OnPlayerMobActivated;
		}

		// Token: 0x06000890 RID: 2192 RVA: 0x0001C2B8 File Offset: 0x0001A4B8
		private int GetMobIndex(BaseGameMob mob)
		{
			for (int i = 0; i < this.currentMobsCount; i++)
			{
				if (this.currentMobs[i].Mob == mob)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000891 RID: 2193 RVA: 0x0001C2F4 File Offset: 0x0001A4F4
		private int GetAppliedBuffIndex(BaseGameMob mob)
		{
			int instanceID = mob.GetInstanceID();
			for (int i = 0; i < this.currentBuffCount; i++)
			{
				if (this.currentBuffs[i].MobID == instanceID)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000892 RID: 2194 RVA: 0x0001C330 File Offset: 0x0001A530
		private bool IsAssistanceRequired()
		{
			return this.isActive && this.playerMobsGroup.InBattle;
		}

		// Token: 0x06000893 RID: 2195 RVA: 0x0001C348 File Offset: 0x0001A548
		private void SetBuffsExpired(float time)
		{
			for (int i = 0; i < this.currentBuffCount; i++)
			{
				ref MobsActivationAssistanceManager.AppliedBuffInfo ptr = ref this.currentBuffs[i];
				float buffCompletionTime = ptr.buffCompletionTime;
				if (buffCompletionTime > 0f && buffCompletionTime < time)
				{
					ptr.isBuffExpired = true;
				}
			}
		}

		// Token: 0x06000894 RID: 2196 RVA: 0x0001C390 File Offset: 0x0001A590
		private void SetAllBuffsExpired()
		{
			for (int i = 0; i < this.currentBuffCount; i++)
			{
				this.currentBuffs[i].isBuffExpired = true;
			}
		}

		// Token: 0x06000895 RID: 2197 RVA: 0x0001C3C0 File Offset: 0x0001A5C0
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			for (int i = 0; i < this.activationTypeConstraints.Length; i++)
			{
				this.activationTypeConstraints[i].Initialize();
			}
			this.currentMobs = new MobsActivationAssistanceManager.PlayerMobInfo[64];
			this.selectedMobs = new MobsActivationAssistanceManager.SelectedPlayerMobInfo[16];
			this.currentBuffs = new MobsActivationAssistanceManager.AppliedBuffInfo[16];
			this.activationTypeBuffCount = new int[this.activationTypeConstraints.Length];
			this.nextActivationTypeBuffTime = new float[this.activationTypeConstraints.Length];
		}

		// Token: 0x06000896 RID: 2198 RVA: 0x0001C444 File Offset: 0x0001A644
		public IBuff GetAssistanceBuff(BaseGameMob mob, out IBuffsGenerator buffGenerator)
		{
			buffGenerator = null;
			if (mob != null)
			{
				int appliedBuffIndex = this.GetAppliedBuffIndex(mob);
				if (appliedBuffIndex >= 0)
				{
					ref MobsActivationAssistanceManager.AppliedBuffInfo ptr = ref this.currentBuffs[appliedBuffIndex];
					buffGenerator = this.activationTypeConstraints[ptr.ActivationTypeConstraintIndex].BuffGenerator;
					return ptr.Buff;
				}
			}
			return null;
		}

		// Token: 0x06000897 RID: 2199 RVA: 0x0001C494 File Offset: 0x0001A694
		public BaseGameMob GetClosestHighlightedMob(Vector2 point, MobActivationAbilityType activationType)
		{
			bool flag = activationType == MobActivationAbilityType.None;
			float num = float.PositiveInfinity;
			BaseGameMob result = null;
			for (int i = 0; i < this.currentBuffCount; i++)
			{
				ref MobsActivationAssistanceManager.AppliedBuffInfo ptr = ref this.currentBuffs[i];
				if (flag || ptr.ActivationType == activationType)
				{
					float num2 = (point - ptr.Mob.Position).SqrMagnitude();
					if (num2 < num)
					{
						num = num2;
						result = ptr.Mob;
					}
				}
			}
			return result;
		}

		// Token: 0x06000898 RID: 2200 RVA: 0x0001C508 File Offset: 0x0001A708
		protected override async void OnSceneLoaded(Scene loadedScene)
		{
			this.assistanceStartTime = 0f;
			this.nextBuffTime = 0f;
			this.nextActivationTypeBuffTime.Reset(0f, -1);
			PlayerBehaviour playerBehaviour = await base.CurrentGame.GetPlayerAsync();
			this.player = playerBehaviour;
			PlayerBehaviour playerBehaviour2 = this.player;
			this.playerMobsGroup = ((playerBehaviour2 != null) ? playerBehaviour2.Group : null);
			IGameLoopAccessProvider gameLoopAccessProvider;
			if (this.activationTypeConstraints.All((MobsActivationAssistanceManager.MobActivationTypeConstraints c) => c.IsInitialized) && this.playerMobsGroup != null && base.CurrentGame.Services.TryGet<IGameLoopAccessProvider>(out gameLoopAccessProvider))
			{
				IReadOnlyList<BaseGameMob> mobs = this.playerMobsGroup.Mobs;
				for (int i = 0; i < mobs.Count; i++)
				{
					this.RegisterMob(mobs[i]);
				}
				gameLoopAccessProvider.UpdatePerformed += this.OnUpdate;
				this.playerMobsGroup.MobAdded += this.OnPlayerMobAdded;
				this.playerMobsGroup.MobRemoved += this.OnPlayerMobRemoved;
				this.player.Destroyed += this.OnPlayerDestroyed;
			}
		}

		// Token: 0x06000899 RID: 2201 RVA: 0x0001C541 File Offset: 0x0001A741
		private void OnPlayerMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.RegisterMob(mob);
		}

		// Token: 0x0600089A RID: 2202 RVA: 0x0001C54A File Offset: 0x0001A74A
		private void OnPlayerMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.UnregisterMob(mob);
		}

		// Token: 0x0600089B RID: 2203 RVA: 0x0001C554 File Offset: 0x0001A754
		private void OnPlayerMobActivated(BaseGameMob mob)
		{
			if (this.GetAppliedBuffIndex(mob) < 0)
			{
				return;
			}
			BaseAbility baseAbility;
			MobActivationAbilityType activationType;
			mob.TryGetMobActivationAbility(out baseAbility, out activationType);
			int activationTypeConstraintIndex = this.GetActivationTypeConstraintIndex(activationType);
			float time = MobsActivationAssistanceManager.GetTime();
			if (activationTypeConstraintIndex >= 0)
			{
				float cooldown = this.activationTypeConstraints[activationTypeConstraintIndex].mobActivationCooldown;
				MobsActivationAssistanceManager.<OnPlayerMobActivated>g__UpdateActivationCooldown|50_0(time, cooldown, ref this.nextActivationTypeBuffTime[activationTypeConstraintIndex]);
			}
			MobsActivationAssistanceManager.<OnPlayerMobActivated>g__UpdateActivationCooldown|50_0(time, this.mobActivationCooldown, ref this.nextBuffTime);
		}

		// Token: 0x0600089C RID: 2204 RVA: 0x0001C5C0 File Offset: 0x0001A7C0
		private void OnPlayerDestroyed(object obj)
		{
			this.currentMobsCount = 0;
			this.playerMobsGroup.MobAdded -= this.OnPlayerMobAdded;
			this.playerMobsGroup.MobRemoved -= this.OnPlayerMobRemoved;
			this.player.Destroyed -= this.OnPlayerDestroyed;
			IGameLoopAccessProvider gameLoopAccessProvider;
			if (base.CurrentGame.Services.TryGet<IGameLoopAccessProvider>(out gameLoopAccessProvider))
			{
				gameLoopAccessProvider.UpdatePerformed -= this.OnUpdate;
			}
			this.player = null;
		}

		// Token: 0x0600089D RID: 2205 RVA: 0x0001C648 File Offset: 0x0001A848
		private void UpdateCurrentBuffs(BaseGameMob selectedMob, int activationTypeConstraintIndex, float time)
		{
			int appliedBuffIndex = this.GetAppliedBuffIndex(selectedMob);
			if (appliedBuffIndex < 0)
			{
				if (time < this.nextBuffTime || (this.maxBuffCount > 0 && this.currentBuffCount == this.maxBuffCount))
				{
					return;
				}
				if (activationTypeConstraintIndex >= 0)
				{
					float num = this.nextActivationTypeBuffTime[activationTypeConstraintIndex];
					if (time < num)
					{
						return;
					}
					int num2 = this.activationTypeConstraints[activationTypeConstraintIndex].maxBuffCount;
					if (num2 > 0 && this.activationTypeBuffCount[activationTypeConstraintIndex] == num2)
					{
						return;
					}
					IBuffsGenerator buffGenerator = this.activationTypeConstraints[activationTypeConstraintIndex].BuffGenerator;
					if (buffGenerator != null)
					{
						IBuff buff = buffGenerator.GenerateBuff(this.player, true);
						if (selectedMob.BuffsController.AddBuff(buff))
						{
							float buffCompletionTime = (this.minBuffLifetime > 0f) ? (time + this.minBuffLifetime) : -1f;
							Common.CollectionsExtensions.Extensions.Add<MobsActivationAssistanceManager.AppliedBuffInfo>(new MobsActivationAssistanceManager.AppliedBuffInfo(selectedMob, buff, activationTypeConstraintIndex, buffCompletionTime), ref this.currentBuffs, ref this.currentBuffCount, 16);
							this.nextBuffTime = time + this.buffSpawnedCooldown;
							float num3 = this.activationTypeConstraints[activationTypeConstraintIndex].buffSpawnedCooldown;
							this.nextActivationTypeBuffTime[activationTypeConstraintIndex] = time + num3;
							this.activationTypeBuffCount[activationTypeConstraintIndex]++;
							return;
						}
					}
				}
			}
			else
			{
				this.currentBuffs[appliedBuffIndex].KeepBuffAlive(time);
			}
		}

		// Token: 0x0600089E RID: 2206 RVA: 0x0001C778 File Offset: 0x0001A978
		private void RemoveExpiredBuffs()
		{
			int i = 0;
			while (i < this.currentBuffCount)
			{
				ref MobsActivationAssistanceManager.AppliedBuffInfo ptr = ref this.currentBuffs[i];
				if (ptr.TryCompleteBuff())
				{
					int activationTypeConstraintIndex = ptr.ActivationTypeConstraintIndex;
					if (activationTypeConstraintIndex >= 0)
					{
						this.activationTypeBuffCount[activationTypeConstraintIndex]--;
					}
					this.currentBuffs.RemoveBySwap(i, ref this.currentBuffCount);
				}
				else
				{
					i++;
				}
			}
		}

		// Token: 0x0600089F RID: 2207 RVA: 0x0001C7DC File Offset: 0x0001A9DC
		private void OnUpdate()
		{
			if (GameApplication.IsGameStateChanging)
			{
				return;
			}
			float time = MobsActivationAssistanceManager.GetTime();
			if (time < this.nextUpdateTime)
			{
				return;
			}
			bool flag = this.IsAssistanceRequired();
			if (flag != this.isAssistanceInProgress)
			{
				if (flag)
				{
					this.assistanceStartTime = time + this.battleStartCooldown;
				}
				this.isAssistanceInProgress = flag;
			}
			if (this.isAssistanceInProgress && time > this.assistanceStartTime)
			{
				int num = 0;
				bool flag2 = this.trySelectMostAttackedMobs;
				bool flag3 = false;
				for (int i = 0; i < this.currentMobsCount; i++)
				{
					ref MobsActivationAssistanceManager.PlayerMobInfo ptr = ref this.currentMobs[i];
					if (ptr.CanBeSelected(this.mobsNormalizedHPThreshold))
					{
						int num2 = 0;
						if (flag2)
						{
							num2 = ptr.GetActivationPriority();
							flag3 |= (num2 > 0);
						}
						Common.CollectionsExtensions.Extensions.Add<MobsActivationAssistanceManager.SelectedPlayerMobInfo>(new MobsActivationAssistanceManager.SelectedPlayerMobInfo(ptr.Mob, ptr.ActivationTypeConstraintIndex, num2), ref this.selectedMobs, ref num, 16);
					}
				}
				if (flag3)
				{
					Array.Sort<MobsActivationAssistanceManager.SelectedPlayerMobInfo>(this.selectedMobs, 0, num);
				}
				this.SetBuffsExpired(time);
				for (int j = 0; j < num; j++)
				{
					ref MobsActivationAssistanceManager.SelectedPlayerMobInfo ptr2 = ref this.selectedMobs[j];
					this.UpdateCurrentBuffs(ptr2.Mob, ptr2.ActivationTypeConstraintIndex, time);
				}
			}
			else
			{
				this.SetAllBuffsExpired();
			}
			this.RemoveExpiredBuffs();
			this.nextUpdateTime = time + 0.1f;
		}

		// Token: 0x060008A1 RID: 2209 RVA: 0x0001C984 File Offset: 0x0001AB84
		[CompilerGenerated]
		internal static void <OnPlayerMobActivated>g__UpdateActivationCooldown|50_0(float currentTime, float cooldown, ref float nextTime)
		{
			float num = currentTime + cooldown;
			if (num > nextTime)
			{
				nextTime = num;
			}
		}

		// Token: 0x040004C8 RID: 1224
		private const float UpdateStep = 0.1f;

		// Token: 0x040004C9 RID: 1225
		[SerializeField]
		private bool isActive = true;

		// Token: 0x040004CA RID: 1226
		[Space]
		[Tooltip("Система запустится через столько секунд после начала сражения")]
		public float battleStartCooldown = 2f;

		// Token: 0x040004CB RID: 1227
		[Tooltip("Столько секунд кулдауна получит система после того как подсветит им юнита")]
		public float buffSpawnedCooldown = 2f;

		// Token: 0x040004CC RID: 1228
		[Tooltip("Столько секунд кулдауна получит система после того как игрок активирует подсвеченного юнита")]
		public float mobActivationCooldown = 2f;

		// Token: 0x040004CD RID: 1229
		public int maxBuffCount = 6;

		// Token: 0x040004CE RID: 1230
		public float minBuffLifetime = 3f;

		// Token: 0x040004CF RID: 1231
		public MobsActivationAssistanceManager.MobActivationTypeConstraints[] activationTypeConstraints;

		// Token: 0x040004D0 RID: 1232
		[Space]
		[SerializeField]
		[Range(0f, 1f)]
		private float mobsNormalizedHPThreshold = 0.2f;

		// Token: 0x040004D1 RID: 1233
		[Space]
		public bool trySelectMostAttackedMobs = true;

		// Token: 0x040004D2 RID: 1234
		private PlayerBehaviour player;

		// Token: 0x040004D3 RID: 1235
		private GameMobsGroupControllerBase playerMobsGroup;

		// Token: 0x040004D4 RID: 1236
		private MobsActivationAssistanceManager.PlayerMobInfo[] currentMobs;

		// Token: 0x040004D5 RID: 1237
		private int currentMobsCount;

		// Token: 0x040004D6 RID: 1238
		private MobsActivationAssistanceManager.SelectedPlayerMobInfo[] selectedMobs;

		// Token: 0x040004D7 RID: 1239
		private int[] activationTypeBuffCount;

		// Token: 0x040004D8 RID: 1240
		private MobsActivationAssistanceManager.AppliedBuffInfo[] currentBuffs;

		// Token: 0x040004D9 RID: 1241
		private int currentBuffCount;

		// Token: 0x040004DA RID: 1242
		private float assistanceStartTime;

		// Token: 0x040004DB RID: 1243
		private float nextUpdateTime;

		// Token: 0x040004DC RID: 1244
		private float nextBuffTime;

		// Token: 0x040004DD RID: 1245
		private float[] nextActivationTypeBuffTime;

		// Token: 0x040004DE RID: 1246
		private bool isAssistanceInProgress;

		// Token: 0x02000451 RID: 1105
		[Serializable]
		public class MobActivationTypeConstraints
		{
			// Token: 0x17000730 RID: 1840
			// (get) Token: 0x06002377 RID: 9079 RVA: 0x0006DE0A File Offset: 0x0006C00A
			public bool IsInitialized
			{
				get
				{
					return this.buffGenerator != null;
				}
			}

			// Token: 0x17000731 RID: 1841
			// (get) Token: 0x06002378 RID: 9080 RVA: 0x0006DE15 File Offset: 0x0006C015
			public IBuffsGenerator BuffGenerator
			{
				get
				{
					return this.buffGenerator;
				}
			}

			// Token: 0x06002379 RID: 9081 RVA: 0x0006DE1D File Offset: 0x0006C01D
			public void Initialize()
			{
				this.buffGenerator = this.buffGeneratorAsset.InstantiateBuffsGenerator();
				if (this.buffGenerator != null)
				{
					IInitializable initializable = this.buffGenerator as IInitializable;
					if (initializable == null)
					{
						return;
					}
					initializable.Initialize();
				}
			}

			// Token: 0x040016E3 RID: 5859
			[EnumPopup]
			public MobActivationAbilityType activationType;

			// Token: 0x040016E4 RID: 5860
			[Tooltip("Столько секунд кулдауна этот тип баффа получит после того как система подсветит им юнита")]
			public float buffSpawnedCooldown;

			// Token: 0x040016E5 RID: 5861
			[Tooltip("Столько секунд кулдауна этот тип баффа получит когда игрок активирует моба с таким баффом")]
			public float mobActivationCooldown;

			// Token: 0x040016E6 RID: 5862
			public int maxBuffCount;

			// Token: 0x040016E7 RID: 5863
			public BuffsGeneratorBuilderAsset.Reference buffGeneratorAsset;

			// Token: 0x040016E8 RID: 5864
			private IBuffsGenerator buffGenerator;
		}

		// Token: 0x02000452 RID: 1106
		private readonly struct PlayerMobInfo
		{
			// Token: 0x0600237B RID: 9083 RVA: 0x0006DE58 File Offset: 0x0006C058
			public PlayerMobInfo(MobsActivationAssistanceManager manager, BaseGameMob mob)
			{
				BaseAbility baseAbility;
				mob.TryGetMobActivationAbility(out baseAbility, out this.ActivationType);
				this.ActivationTypeConstraintIndex = manager.GetActivationTypeConstraintIndex(this.ActivationType);
				this.Mob = mob;
				this.HPController = mob.HitPointsController;
				this.AIController = mob.AIController;
				this.initialHitPoints = this.HPController.InitialHitPoints;
				this.currentAttackers = mob.CurrentAttackers;
			}

			// Token: 0x0600237C RID: 9084 RVA: 0x0006DEC2 File Offset: 0x0006C0C2
			public bool CanBeSelected(float hpThreshold)
			{
				return this.HPController.CurrentHitPoints / this.initialHitPoints <= hpThreshold && (this.AIController == null || this.AIController.IsAttackDistanceReached);
			}

			// Token: 0x0600237D RID: 9085 RVA: 0x0006DEF0 File Offset: 0x0006C0F0
			public int GetActivationPriority()
			{
				return this.currentAttackers.Count;
			}

			// Token: 0x040016E9 RID: 5865
			public readonly BaseGameMob Mob;

			// Token: 0x040016EA RID: 5866
			public readonly MobActivationAbilityType ActivationType;

			// Token: 0x040016EB RID: 5867
			public readonly int ActivationTypeConstraintIndex;

			// Token: 0x040016EC RID: 5868
			public readonly IDamageable HPController;

			// Token: 0x040016ED RID: 5869
			public readonly GameMobAIController AIController;

			// Token: 0x040016EE RID: 5870
			private readonly float initialHitPoints;

			// Token: 0x040016EF RID: 5871
			private readonly IReadOnlyCollection<BaseGameMob> currentAttackers;
		}

		// Token: 0x02000453 RID: 1107
		private readonly struct SelectedPlayerMobInfo : IComparable<MobsActivationAssistanceManager.SelectedPlayerMobInfo>
		{
			// Token: 0x0600237E RID: 9086 RVA: 0x0006DEFD File Offset: 0x0006C0FD
			public SelectedPlayerMobInfo(BaseGameMob mob, int activationTypeConstraintIndex, int activationPriority)
			{
				this.Mob = mob;
				this.ActivationTypeConstraintIndex = activationTypeConstraintIndex;
				this.ActivationPriority = activationPriority;
			}

			// Token: 0x0600237F RID: 9087 RVA: 0x0006DF14 File Offset: 0x0006C114
			public int CompareTo(MobsActivationAssistanceManager.SelectedPlayerMobInfo other)
			{
				return other.ActivationPriority.CompareTo(this.ActivationPriority);
			}

			// Token: 0x040016F0 RID: 5872
			public readonly BaseGameMob Mob;

			// Token: 0x040016F1 RID: 5873
			public readonly int ActivationTypeConstraintIndex;

			// Token: 0x040016F2 RID: 5874
			public readonly int ActivationPriority;
		}

		// Token: 0x02000454 RID: 1108
		private struct AppliedBuffInfo
		{
			// Token: 0x06002380 RID: 9088 RVA: 0x0006DF38 File Offset: 0x0006C138
			public AppliedBuffInfo(BaseGameMob mob, IBuff buff, int activationTypeConstraintIndex, float buffCompletionTime)
			{
				this.Mob = mob;
				this.MobID = mob.GetInstanceID();
				this.Buff = buff;
				this.ActivationTypeConstraintIndex = activationTypeConstraintIndex;
				BaseAbility baseAbility;
				mob.TryGetMobActivationAbility(out baseAbility, out this.ActivationType);
				this.buffCompletionTime = buffCompletionTime;
				this.isBuffExpired = false;
			}

			// Token: 0x06002381 RID: 9089 RVA: 0x0006DF84 File Offset: 0x0006C184
			public void KeepBuffAlive(float time)
			{
				if (this.isBuffExpired)
				{
					float num = time + 1f;
					if (num > this.buffCompletionTime)
					{
						this.buffCompletionTime = num;
					}
					this.isBuffExpired = false;
				}
			}

			// Token: 0x06002382 RID: 9090 RVA: 0x0006DFB8 File Offset: 0x0006C1B8
			public bool TryCompleteBuff()
			{
				if (this.isBuffExpired || this.Buff.IsCompleted)
				{
					this.Buff.Complete();
					return true;
				}
				return false;
			}

			// Token: 0x040016F3 RID: 5875
			public readonly BaseGameMob Mob;

			// Token: 0x040016F4 RID: 5876
			public readonly int MobID;

			// Token: 0x040016F5 RID: 5877
			public MobActivationAbilityType ActivationType;

			// Token: 0x040016F6 RID: 5878
			public readonly IBuff Buff;

			// Token: 0x040016F7 RID: 5879
			public readonly int ActivationTypeConstraintIndex;

			// Token: 0x040016F8 RID: 5880
			public float buffCompletionTime;

			// Token: 0x040016F9 RID: 5881
			public bool isBuffExpired;
		}
	}
}
