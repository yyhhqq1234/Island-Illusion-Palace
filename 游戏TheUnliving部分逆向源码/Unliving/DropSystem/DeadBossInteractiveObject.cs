using System;
using System.Runtime.CompilerServices;
using Common.Editor;
using Game.Abilities;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Abilities;
using Unliving.Interactables;
using Unliving.Pickables;
using Unliving.Player;

namespace Unliving.DropSystem
{
	// Token: 0x02000286 RID: 646
	public sealed class DeadBossInteractiveObject : NPCControllerBase<NonFactoryPickableType>
	{
		// Token: 0x170004CC RID: 1228
		// (get) Token: 0x06001649 RID: 5705 RVA: 0x000476A0 File Offset: 0x000458A0
		private bool IsValidSessionState
		{
			get
			{
				SessionState currentSessionState = this.gameSessionManager.CurrentSessionState;
				return currentSessionState == SessionState.VictoryCutscene || currentSessionState == SessionState.Cutscene;
			}
		}

		// Token: 0x170004CD RID: 1229
		// (get) Token: 0x0600164A RID: 5706 RVA: 0x000476C3 File Offset: 0x000458C3
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.None;
			}
		}

		// Token: 0x0600164B RID: 5707 RVA: 0x000476C6 File Offset: 0x000458C6
		private void OnInteractionAnimationEnds()
		{
			this.animationEndsEvents.Invoke();
		}

		// Token: 0x0600164C RID: 5708 RVA: 0x000476D3 File Offset: 0x000458D3
		private void OnEffectsActivation()
		{
			this.effectsActivationEvents.Invoke();
		}

		// Token: 0x0600164D RID: 5709 RVA: 0x000476E0 File Offset: 0x000458E0
		protected override void Start()
		{
			base.Start();
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out this.gameSessionManager))
			{
				if (this.IsValidSessionState)
				{
					this.BuffEnemyMobs();
					PlayerAbilitiesController playerAbilitiesController = base.CurrentPlayer.AbilitiesController as PlayerAbilitiesController;
					if (playerAbilitiesController != null)
					{
						this.playerAbilitiesController = playerAbilitiesController;
						playerAbilitiesController.AddUnallowedAbilitiesDescription(this.necromancyAbilityDescription, true);
						return;
					}
				}
				else
				{
					UnityEngine.Object.Destroy(this);
				}
			}
		}

		// Token: 0x0600164E RID: 5710 RVA: 0x00047748 File Offset: 0x00045948
		private void BuffEnemyMobs()
		{
			DeadBossInteractiveObject.<BuffEnemyMobs>d__16 <BuffEnemyMobs>d__;
			<BuffEnemyMobs>d__.<>4__this = this;
			<BuffEnemyMobs>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<BuffEnemyMobs>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <BuffEnemyMobs>d__.<>t__builder;
			<>t__builder.Start<DeadBossInteractiveObject.<BuffEnemyMobs>d__16>(ref <BuffEnemyMobs>d__);
		}

		// Token: 0x0600164F RID: 5711 RVA: 0x00047784 File Offset: 0x00045984
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			base.PerformPickingUp(targetCollector);
			if (this.IsValidSessionState && this.undeadExchangeAbilityID != AbilityID.None)
			{
				BaseAbility.UsingArgs usingArgs = new BaseAbility.UsingArgs
				{
					targetPosition = targetCollector.Component.transform.position
				};
				this.playerAbilitiesController.UseExternalAbility((AbilityInfo)this.undeadExchangeAbilityID, usingArgs);
			}
		}

		// Token: 0x04000CE5 RID: 3301
		public UnityEvent interactionEvents;

		// Token: 0x04000CE6 RID: 3302
		public UnityEvent animationEndsEvents;

		// Token: 0x04000CE7 RID: 3303
		public UnityEvent effectsActivationEvents;

		// Token: 0x04000CE8 RID: 3304
		public float mobFearRetreatRadius = 50f;

		// Token: 0x04000CE9 RID: 3305
		[EnumPopup]
		public AbilityID undeadExchangeAbilityID;

		// Token: 0x04000CEA RID: 3306
		public int killedPerFrameMobsCount = 10;

		// Token: 0x04000CEB RID: 3307
		private readonly AbilityDescription necromancyAbilityDescription = new AbilityDescription
		{
			abilityID = AbilityID.Necromancy
		};

		// Token: 0x04000CEC RID: 3308
		private GameSessionManager gameSessionManager;

		// Token: 0x04000CED RID: 3309
		private PlayerAbilitiesController playerAbilitiesController;
	}
}
