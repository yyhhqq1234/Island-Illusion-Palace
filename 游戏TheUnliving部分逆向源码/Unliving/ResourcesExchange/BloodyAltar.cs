using System;
using Common;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;
using Unliving.Challenges;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;
using Unliving.Pickables;
using Unliving.Player;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000D6 RID: 214
	public class BloodyAltar : DefaultObjectPickable, IProgressBasedAction
	{
		// Token: 0x170000C2 RID: 194
		// (get) Token: 0x0600053C RID: 1340 RVA: 0x00012E5D File Offset: 0x0001105D
		public BloodyAltar.State CurrentState
		{
			get
			{
				return this.currentState;
			}
		}

		// Token: 0x170000C3 RID: 195
		// (get) Token: 0x0600053D RID: 1341 RVA: 0x00012E65 File Offset: 0x00011065
		// (set) Token: 0x0600053E RID: 1342 RVA: 0x00012E6D File Offset: 0x0001106D
		public float CurrentProgress { get; private set; }

		// Token: 0x1400002E RID: 46
		// (add) Token: 0x0600053F RID: 1343 RVA: 0x00012E78 File Offset: 0x00011078
		// (remove) Token: 0x06000540 RID: 1344 RVA: 0x00012EB0 File Offset: 0x000110B0
		public event Action<BloodyAltar.State> StateChanged;

		// Token: 0x06000541 RID: 1345 RVA: 0x00012EE8 File Offset: 0x000110E8
		protected override void Start()
		{
			base.Start();
			this.player = base.CurrentGame.Services.Get<IPlayerProvider>().CurrentPlayer;
			this.playerPosition.PlayerTriggerEnter += this.OnPlayerInTriggerEnter;
			this.args = new HitPointsController.HPChangingArgs(true)
			{
				isForcedChanging = true,
				amount = this.damageSettings.amount
			};
			this.hitPointsController = base.GetComponentInChildren<IDamageable>();
			this.hitPointsController.Behaviour = this;
			this.hitPointsController.TotallyDestroyed += this.OnTotallyDestroyed;
			this.ChangeState(BloodyAltar.State.Initialized);
		}

		// Token: 0x06000542 RID: 1346 RVA: 0x00012F87 File Offset: 0x00011187
		private void OnTotallyDestroyed(IDamageable obj)
		{
			this.SpawnSmallReward();
		}

		// Token: 0x06000543 RID: 1347 RVA: 0x00012F8F File Offset: 0x0001118F
		private void OnPlayerInTriggerEnter(PlayerBehaviour player)
		{
			if (this.CurrentState != BloodyAltar.State.Initialized)
			{
				return;
			}
			if (this.player != player)
			{
				return;
			}
			this.activationTime = Time.time;
			this.deactivationTime = Time.time + this.activeTime;
			this.ChangeState(BloodyAltar.State.Active);
		}

		// Token: 0x06000544 RID: 1348 RVA: 0x00012FCE File Offset: 0x000111CE
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (this.CurrentState != BloodyAltar.State.Initialized)
			{
				return;
			}
			this.playerPosition.transform.position = this.player.Position;
			this.playerPosition.Activate();
		}

		// Token: 0x06000545 RID: 1349 RVA: 0x00013008 File Offset: 0x00011208
		private void Update()
		{
			if (this.lockInput)
			{
				this.player.PlayerInputController.LockInput();
			}
			if (this.CurrentState != BloodyAltar.State.Active)
			{
				return;
			}
			this.CurrentProgress = (Time.time - this.activationTime) / this.activeTime;
			this.player.BlockMovement(0f);
			if (Time.time - this.damageTime >= this.damageSettings.timeout)
			{
				this.damageTime = Time.time;
				if (this.player.HitPointsController.CurrentHitPoints > this.args.amount)
				{
					this.player.HitPointsController.ModifyHitPoints(this, this.args);
				}
				else
				{
					this.SpawnSmallReward();
				}
			}
			if (Time.time > this.deactivationTime)
			{
				this.SpawnBigReward();
			}
		}

		// Token: 0x06000546 RID: 1350 RVA: 0x000130D4 File Offset: 0x000112D4
		public async void SpawnBigReward()
		{
			this.ChangeState(BloodyAltar.State.BigRewardSpawned);
			this.CurrentProgress = 1f;
			this.playerPosition.Deactivate();
			await new WaitForSeconds(this.rewardSpawnDelay);
			this.SpawnReward(this.bigReward);
		}

		// Token: 0x06000547 RID: 1351 RVA: 0x00013110 File Offset: 0x00011310
		private async void SpawnSmallReward()
		{
			this.ChangeState(BloodyAltar.State.SmallRewardSpawned);
			await new WaitForSeconds(this.rewardSpawnDelay);
			this.SpawnReward(this.smallReward);
		}

		// Token: 0x06000548 RID: 1352 RVA: 0x00013149 File Offset: 0x00011349
		private void SpawnReward(ChallengeRewardInfo reward)
		{
			this.lockInput = false;
			if (this.CurrentState == BloodyAltar.State.SmallRewardSpawned)
			{
				return;
			}
			reward.CreateReward(base.CurrentPlayer.TempItemsStorageController, this.validChunckTypes);
		}

		// Token: 0x06000549 RID: 1353 RVA: 0x00013173 File Offset: 0x00011373
		private void ChangeState(BloodyAltar.State state)
		{
			if (this.CurrentState == state)
			{
				return;
			}
			this.currentState = state;
			Action<BloodyAltar.State> stateChanged = this.StateChanged;
			if (stateChanged == null)
			{
				return;
			}
			stateChanged(this.CurrentState);
		}

		// Token: 0x0600054A RID: 1354 RVA: 0x0001319C File Offset: 0x0001139C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.hitPointsController != null)
			{
				this.hitPointsController.TotallyDestroyed -= this.OnTotallyDestroyed;
			}
			if (this.currentPlayerMotion != null && !this.player.IsNull())
			{
				this.lockInput = false;
			}
		}

		// Token: 0x0400038F RID: 911
		public float rewardSpawnDelay;

		// Token: 0x04000390 RID: 912
		public ChallengeRewardInfo smallReward;

		// Token: 0x04000391 RID: 913
		public ChallengeRewardInfo bigReward;

		// Token: 0x04000392 RID: 914
		public float activeTime;

		// Token: 0x04000393 RID: 915
		public PlayerPositionTrigger playerPosition;

		// Token: 0x04000394 RID: 916
		public BloodyAltar.DamageSettings damageSettings;

		// Token: 0x04000395 RID: 917
		private float deactivationTime;

		// Token: 0x04000396 RID: 918
		private float activationTime;

		// Token: 0x04000397 RID: 919
		private HitPointsController.HPChangingArgs args;

		// Token: 0x04000398 RID: 920
		private float damageTime;

		// Token: 0x04000399 RID: 921
		private IDamageable hitPointsController;

		// Token: 0x0400039A RID: 922
		private PlayerBehaviour player;

		// Token: 0x0400039B RID: 923
		private BloodyAltar.State currentState;

		// Token: 0x0400039C RID: 924
		private bool lockInput;

		// Token: 0x0400039D RID: 925
		private Coroutine currentPlayerMovementCoroutine;

		// Token: 0x0400039E RID: 926
		private GameMobKinematicMotionBase currentPlayerMotion;

		// Token: 0x0400039F RID: 927
		private readonly LocationChunk.TypeID[] validChunckTypes = new LocationChunk.TypeID[]
		{
			LocationChunk.TypeID.BattleChunk,
			LocationChunk.TypeID.BossChunk
		};

		// Token: 0x02000429 RID: 1065
		public enum State
		{
			// Token: 0x0400161E RID: 5662
			NotInitialized,
			// Token: 0x0400161F RID: 5663
			Initialized,
			// Token: 0x04001620 RID: 5664
			Active,
			// Token: 0x04001621 RID: 5665
			SmallRewardSpawned,
			// Token: 0x04001622 RID: 5666
			BigRewardSpawned,
			// Token: 0x04001623 RID: 5667
			Destroyed
		}

		// Token: 0x0200042A RID: 1066
		[Serializable]
		public struct DamageSettings
		{
			// Token: 0x04001624 RID: 5668
			public float amount;

			// Token: 0x04001625 RID: 5669
			public float timeout;
		}
	}
}
