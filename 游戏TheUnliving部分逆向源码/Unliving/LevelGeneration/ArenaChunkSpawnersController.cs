using System;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Mobs;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000262 RID: 610
	public sealed class ArenaChunkSpawnersController : MonoBehaviour, IGameMobsKillingProgressProvider
	{
		// Token: 0x1700044D RID: 1101
		// (get) Token: 0x0600143D RID: 5181 RVA: 0x0003FB89 File Offset: 0x0003DD89
		public float MobsKillingProgress
		{
			get
			{
				return this.mobsKillingProgress;
			}
		}

		// Token: 0x1700044E RID: 1102
		// (get) Token: 0x0600143E RID: 5182 RVA: 0x0003FB91 File Offset: 0x0003DD91
		public UnityEvent Activated
		{
			get
			{
				return this.activated;
			}
		}

		// Token: 0x1700044F RID: 1103
		// (get) Token: 0x0600143F RID: 5183 RVA: 0x0003FB99 File Offset: 0x0003DD99
		public UnityEvent Completed
		{
			get
			{
				return this.completed;
			}
		}

		// Token: 0x140000C8 RID: 200
		// (add) Token: 0x06001440 RID: 5184 RVA: 0x0003FBA4 File Offset: 0x0003DDA4
		// (remove) Token: 0x06001441 RID: 5185 RVA: 0x0003FBDC File Offset: 0x0003DDDC
		public event Action<IGameMobsKillingProgressProvider, float, float> MobsKillingProgressChanged;

		// Token: 0x06001442 RID: 5186 RVA: 0x0003FC14 File Offset: 0x0003DE14
		private void UpdateMobsKillingProgress(float newProgress)
		{
			if (this.mobsKillingProgress == newProgress)
			{
				return;
			}
			float arg = this.mobsKillingProgress;
			this.mobsKillingProgress = newProgress;
			Action<IGameMobsKillingProgressProvider, float, float> mobsKillingProgressChanged = this.MobsKillingProgressChanged;
			if (mobsKillingProgressChanged == null)
			{
				return;
			}
			mobsKillingProgressChanged(this, arg, newProgress);
		}

		// Token: 0x06001443 RID: 5187 RVA: 0x0003FC4C File Offset: 0x0003DE4C
		private void Awake()
		{
			for (int i = 0; i < this.spawnersGroups.Length; i++)
			{
				this.spawnersGroups[i].Initialize();
			}
		}

		// Token: 0x06001444 RID: 5188 RVA: 0x0003FC7C File Offset: 0x0003DE7C
		private void Start()
		{
			if (this.spawnersGroups.Length != 0)
			{
				ArenaChunkSpawnersController.SpawnersGroup[] array = this.spawnersGroups;
				int num = this.nextSpawnersGroupIndex;
				this.nextSpawnersGroupIndex = num + 1;
				this.currentSpawnersGroup = array[num];
				this.currentSpawnersGroup.Activate(0f, true);
				this.activated.Invoke();
			}
		}

		// Token: 0x06001445 RID: 5189 RVA: 0x0003FCD0 File Offset: 0x0003DED0
		private void LateUpdate()
		{
			if (this.isArenaCompleted)
			{
				this.UpdateMobsKillingProgress(1f);
				return;
			}
			float num = 0f;
			for (int i = 0; i < this.spawnersGroups.Length; i++)
			{
				ArenaChunkSpawnersController.SpawnersGroup spawnersGroup = this.spawnersGroups[i];
				spawnersGroup.Update();
				if (spawnersGroup == this.currentSpawnersGroup)
				{
					num = spawnersGroup.KilledMobsRatio;
				}
			}
			if (this.nextSpawnersGroupIndex < this.spawnersGroups.Length)
			{
				ArenaChunkSpawnersController.SpawnersGroup spawnersGroup2 = this.spawnersGroups[this.nextSpawnersGroupIndex];
				if (spawnersGroup2.Activate(num, false))
				{
					this.currentSpawnersGroup = spawnersGroup2;
					this.nextSpawnersGroupIndex++;
				}
			}
			else if (num == 0f)
			{
				this.isArenaCompleted = true;
				this.completed.Invoke();
				base.enabled = false;
			}
			float num2 = 0f;
			int num3 = 0;
			for (int j = 0; j < this.spawnersGroups.Length; j++)
			{
				ArenaChunkSpawnersController.SpawnersGroup spawnersGroup3 = this.spawnersGroups[j];
				if (spawnersGroup3.IsSpawningCompleted)
				{
					num2 += spawnersGroup3.KilledMobsRatio;
					num3++;
				}
			}
			if (num3 != 0)
			{
				num2 = 1f - Mathf.Clamp01(num2 / (float)num3);
			}
			this.UpdateMobsKillingProgress(num2);
		}

		// Token: 0x04000BCD RID: 3021
		public ArenaChunkSpawnersController.SpawnersGroup[] spawnersGroups;

		// Token: 0x04000BCE RID: 3022
		[Space]
		[SerializeField]
		private UnityEvent activated;

		// Token: 0x04000BCF RID: 3023
		[SerializeField]
		private UnityEvent completed;

		// Token: 0x04000BD0 RID: 3024
		private ArenaChunkSpawnersController.SpawnersGroup currentSpawnersGroup;

		// Token: 0x04000BD1 RID: 3025
		private int nextSpawnersGroupIndex;

		// Token: 0x04000BD2 RID: 3026
		private float mobsKillingProgress;

		// Token: 0x04000BD3 RID: 3027
		private bool isArenaCompleted;

		// Token: 0x020004E0 RID: 1248
		[Serializable]
		public sealed class SpawnersGroup
		{
			// Token: 0x170007A0 RID: 1952
			// (get) Token: 0x0600257F RID: 9599 RVA: 0x000747CB File Offset: 0x000729CB
			// (set) Token: 0x06002580 RID: 9600 RVA: 0x000747D3 File Offset: 0x000729D3
			public float LastSpawnersKilledMobsActivationThreshold
			{
				get
				{
					return this.lastSpawnersKilledMobsActivationThreshold;
				}
				set
				{
					this.lastSpawnersKilledMobsActivationThreshold = Mathf.Clamp01(value);
				}
			}

			// Token: 0x170007A1 RID: 1953
			// (get) Token: 0x06002581 RID: 9601 RVA: 0x000747E1 File Offset: 0x000729E1
			public bool IsActivated
			{
				get
				{
					return this.isActivated;
				}
			}

			// Token: 0x170007A2 RID: 1954
			// (get) Token: 0x06002582 RID: 9602 RVA: 0x000747E9 File Offset: 0x000729E9
			public bool IsSpawningCompleted
			{
				get
				{
					return this.isSpawningCompleted;
				}
			}

			// Token: 0x170007A3 RID: 1955
			// (get) Token: 0x06002583 RID: 9603 RVA: 0x000747F1 File Offset: 0x000729F1
			public float KilledMobsRatio
			{
				get
				{
					return this.killedMobsRatio;
				}
			}

			// Token: 0x06002584 RID: 9604 RVA: 0x000747FC File Offset: 0x000729FC
			public void Initialize()
			{
				for (int i = 0; i < this.spawners.Length; i++)
				{
					MobBehaviourSpawner mobBehaviourSpawner = this.spawners[i];
					if (!(mobBehaviourSpawner == null))
					{
						mobBehaviourSpawner.gameObject.SetActive(false);
					}
				}
				this.killedMobsRatio = 1.1f;
				this.isSpawningCompleted = false;
			}

			// Token: 0x06002585 RID: 9605 RVA: 0x0007484C File Offset: 0x00072A4C
			public bool Activate(float lastSpawnerKilledMobsRatio, bool force = false)
			{
				if (this.isActivated || (!force && lastSpawnerKilledMobsRatio > this.lastSpawnersKilledMobsActivationThreshold))
				{
					return false;
				}
				for (int i = 0; i < this.spawners.Length; i++)
				{
					MobBehaviourSpawner mobBehaviourSpawner = this.spawners[i];
					if (!(mobBehaviourSpawner == null))
					{
						mobBehaviourSpawner.gameObject.SetActive(true);
					}
				}
				this.isActivated = true;
				return true;
			}

			// Token: 0x06002586 RID: 9606 RVA: 0x000748A8 File Offset: 0x00072AA8
			public void Update()
			{
				if (!this.isActivated)
				{
					return;
				}
				if (!this.isSpawningCompleted)
				{
					this.totalMobsCount = 0;
					this.totalMobsHP = 0f;
					for (int i = 0; i < this.spawners.Length; i++)
					{
						MobBehaviourSpawner mobBehaviourSpawner = this.spawners[i];
						if (!(mobBehaviourSpawner == null))
						{
							if (!mobBehaviourSpawner.IsGroupSpawned)
							{
								return;
							}
							GameMobGroupController gameMobGroupController = mobBehaviourSpawner.SpawnedGroup as GameMobGroupController;
							if (gameMobGroupController != null)
							{
								this.totalMobsCount += gameMobGroupController.CharactersCount;
								this.totalMobsHP += mobBehaviourSpawner.MaxGroupHitPointsSum;
							}
						}
					}
					this.isSpawningCompleted = true;
					return;
				}
				int num = 0;
				float num2 = 0f;
				for (int j = 0; j < this.spawners.Length; j++)
				{
					MobBehaviourSpawner mobBehaviourSpawner2 = this.spawners[j];
					GameMobGroupController gameMobGroupController2 = ((mobBehaviourSpawner2 != null) ? mobBehaviourSpawner2.SpawnedGroup : null) as GameMobGroupController;
					if (gameMobGroupController2 != null)
					{
						num += gameMobGroupController2.CharactersCount;
						num2 += gameMobGroupController2.CurrentGroupCharactersHitPointsSum;
					}
				}
				this.killedMobsRatio = (this.countMobsHitPoints ? (num2 / this.totalMobsHP) : ((float)num / (float)this.totalMobsCount));
			}

			// Token: 0x04001A19 RID: 6681
			[SerializeField]
			[Range(0f, 1f)]
			private float lastSpawnersKilledMobsActivationThreshold;

			// Token: 0x04001A1A RID: 6682
			public bool countMobsHitPoints;

			// Token: 0x04001A1B RID: 6683
			public MobBehaviourSpawner[] spawners;

			// Token: 0x04001A1C RID: 6684
			private int totalMobsCount;

			// Token: 0x04001A1D RID: 6685
			private float totalMobsHP;

			// Token: 0x04001A1E RID: 6686
			private float killedMobsRatio;

			// Token: 0x04001A1F RID: 6687
			private bool isActivated;

			// Token: 0x04001A20 RID: 6688
			private bool isSpawningCompleted;
		}
	}
}
