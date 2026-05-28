using System;
using System.Collections;
using Game.InputManager;
using Rewired;
using UnityEngine;
using Unliving.Abilities;
using Unliving.InputManager;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000159 RID: 345
	public class FreezeGameMobActivationController : PlayerMobsActivationControllerBase<FreezeGameMobActivationController.Data>
	{
		// Token: 0x1400005D RID: 93
		// (add) Token: 0x0600098E RID: 2446 RVA: 0x00020C14 File Offset: 0x0001EE14
		// (remove) Token: 0x0600098F RID: 2447 RVA: 0x00020C4C File Offset: 0x0001EE4C
		public override event Action<MobActivationAbilityType> MobActivationFailed;

		// Token: 0x1400005E RID: 94
		// (add) Token: 0x06000990 RID: 2448 RVA: 0x00020C84 File Offset: 0x0001EE84
		// (remove) Token: 0x06000991 RID: 2449 RVA: 0x00020CBC File Offset: 0x0001EEBC
		public override event Action<bool> ActivationModeStateChanged;

		// Token: 0x1400005F RID: 95
		// (add) Token: 0x06000992 RID: 2450 RVA: 0x00020CF4 File Offset: 0x0001EEF4
		// (remove) Token: 0x06000993 RID: 2451 RVA: 0x00020D2C File Offset: 0x0001EF2C
		public override event Action<BaseGameMob> MobSelected;

		// Token: 0x06000994 RID: 2452 RVA: 0x00020D64 File Offset: 0x0001EF64
		public FreezeGameMobActivationController(PlayerBehaviour targetMob) : base(targetMob)
		{
			this.currentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager);
			this.currentGame.Services.TryGet<IInputManager>(out this.inputManager);
			this.actionMask.AddActionFlag(PlayerAction.ACTIVATION_MODE);
			this.actionMask.AddActionFlag(PlayerAction.PLAYER_USE_NATIVE_ABILITY_2);
			this.ChangeRewiredLayoutRuleSet(7);
			this.inputManager.ActiveControllerChanged += this.OnActiveControllerChanged;
			this.OnActiveControllerChanged();
			this.playerGroup = targetMob.Group;
			this.playerGroup.MobRemoved += this.OnMobRemoved;
		}

		// Token: 0x06000995 RID: 2453 RVA: 0x00020E14 File Offset: 0x0001F014
		private void OnMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (!this.currentState || !mob.IsSacrificed)
			{
				return;
			}
			this.nextActivationTime = Time.time + this.data.activationTimeout;
			this.SetActivationModeState(false);
			this.playerInput.SetPlayerActionInterrupted(PlayerAction.ACTIVATION_MODE);
			this.ControllerOwner.StartCoroutine(this.LockInputRoutine());
		}

		// Token: 0x06000996 RID: 2454 RVA: 0x00020E6F File Offset: 0x0001F06F
		private void OnActiveControllerChanged()
		{
			this.inputManager.TryGetActionElementID(76, InputAxisContribution.Positive, out this.activationModeActionKeyID);
		}

		// Token: 0x06000997 RID: 2455 RVA: 0x00020E86 File Offset: 0x0001F086
		protected override void OnPlayerActionPerformed(PlayerInputController.ActionArgs args)
		{
			this.SetActivationModeState(args.HasActionFlag(PlayerAction.ACTIVATION_MODE));
		}

		// Token: 0x06000998 RID: 2456 RVA: 0x00020E96 File Offset: 0x0001F096
		private IEnumerator LockInputRoutine()
		{
			int num;
			for (int i = 0; i < this.data.lockInputFramesCount; i = num + 1)
			{
				yield return new WaitForEndOfFrame();
				this.playerInput.LockInput();
				num = i;
			}
			yield break;
		}

		// Token: 0x06000999 RID: 2457 RVA: 0x00020EA5 File Offset: 0x0001F0A5
		public override void SetSilentState(bool isActive)
		{
		}

		// Token: 0x0600099A RID: 2458 RVA: 0x00020EA7 File Offset: 0x0001F0A7
		public override void OnUpdate()
		{
			if (this.currentState)
			{
				this.ControllerOwner.OverrideLookDirection(this.originalCursorPosition);
				this.playerInput.SetAllowedActionsMask(this.actionMask);
			}
		}

		// Token: 0x0600099B RID: 2459 RVA: 0x00020ED4 File Offset: 0x0001F0D4
		private void SetActivationModeState(bool state)
		{
			if (this.currentState == state)
			{
				return;
			}
			if (state && this.gameSessionManager.CurrentSessionState != SessionState.InProgress)
			{
				return;
			}
			if (state && Time.time < this.nextActivationTime)
			{
				return;
			}
			this.currentState = state;
			GameManager.SetGameFreezed(this.currentState);
			if (this.currentState)
			{
				this.originalCursorPosition = this.playerInput.CurrentWorldCursorPosition;
				this.ChangeRewiredLayoutRuleSet(15);
			}
			else
			{
				this.ChangeRewiredLayoutRuleSet(7);
			}
			Action<bool> activationModeStateChanged = this.ActivationModeStateChanged;
			if (activationModeStateChanged == null)
			{
				return;
			}
			activationModeStateChanged(this.currentState);
		}

		// Token: 0x0600099C RID: 2460 RVA: 0x00020F60 File Offset: 0x0001F160
		private void ChangeRewiredLayoutRuleSet(int setIndex)
		{
			ControllerMapLayoutManager.RuleSet controllerMapLayoutManagerRuleSetInstance = ReInput.mapping.GetControllerMapLayoutManagerRuleSetInstance(setIndex);
			controllerMapLayoutManagerRuleSetInstance.enabled = true;
			ControllerMapLayoutManager layoutManager = (this.inputManager as RewiredInputManager).RewiredPlayer.controllers.maps.layoutManager;
			layoutManager.ruleSets.Clear();
			layoutManager.ruleSets.Add(controllerMapLayoutManagerRuleSetInstance);
			layoutManager.Apply();
		}

		// Token: 0x0600099D RID: 2461 RVA: 0x00020FBB File Offset: 0x0001F1BB
		protected override void OnOwnerKilled(IGameMob owner)
		{
			base.OnOwnerKilled(owner);
			if (this.playerGroup != null)
			{
				this.playerGroup.MobRemoved -= this.OnMobRemoved;
			}
		}

		// Token: 0x0400059E RID: 1438
		private const PlayerAction ActivationModeAction = PlayerAction.ACTIVATION_MODE;

		// Token: 0x0400059F RID: 1439
		private const PlayerAction MobActivationAbilityAction = PlayerAction.PLAYER_USE_NATIVE_ABILITY_2;

		// Token: 0x040005A0 RID: 1440
		private const int DefaultRulesetIndex = 7;

		// Token: 0x040005A1 RID: 1441
		private const int ActivationModeRulesetIndex = 15;

		// Token: 0x040005A5 RID: 1445
		private bool currentState;

		// Token: 0x040005A6 RID: 1446
		private GameSessionManager gameSessionManager;

		// Token: 0x040005A7 RID: 1447
		private IInputManager inputManager;

		// Token: 0x040005A8 RID: 1448
		private readonly PlayerActionsMask actionMask = new PlayerActionsMask();

		// Token: 0x040005A9 RID: 1449
		private Vector2 originalCursorPosition;

		// Token: 0x040005AA RID: 1450
		private float nextActivationTime;

		// Token: 0x040005AB RID: 1451
		private int activationModeActionKeyID;

		// Token: 0x040005AC RID: 1452
		private GameMobsGroupControllerBase playerGroup;

		// Token: 0x02000464 RID: 1124
		[Serializable]
		public struct Data
		{
			// Token: 0x0400172A RID: 5930
			public float activationTimeout;

			// Token: 0x0400172B RID: 5931
			public int lockInputFramesCount;
		}
	}
}
