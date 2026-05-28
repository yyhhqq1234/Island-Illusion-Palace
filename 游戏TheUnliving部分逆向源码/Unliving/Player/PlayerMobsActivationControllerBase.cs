using System;
using Game.Core;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200015C RID: 348
	public abstract class PlayerMobsActivationControllerBase : BaseGameMob.ControllerBase<PlayerBehaviour>, IPlayerMobsActivationController
	{
		// Token: 0x14000063 RID: 99
		// (add) Token: 0x060009A7 RID: 2471
		// (remove) Token: 0x060009A8 RID: 2472
		public abstract event Action<bool> ActivationModeStateChanged;

		// Token: 0x14000064 RID: 100
		// (add) Token: 0x060009A9 RID: 2473
		// (remove) Token: 0x060009AA RID: 2474
		public abstract event Action<BaseGameMob> MobSelected;

		// Token: 0x14000065 RID: 101
		// (add) Token: 0x060009AB RID: 2475
		// (remove) Token: 0x060009AC RID: 2476
		public abstract event Action<MobActivationAbilityType> MobActivationFailed;

		// Token: 0x060009AD RID: 2477 RVA: 0x00020FEB File Offset: 0x0001F1EB
		public PlayerMobsActivationControllerBase(PlayerBehaviour targetMob) : base(targetMob)
		{
			this.currentGame = targetMob.CurrentGame;
			this.playerInput = targetMob.PlayerInputController;
			this.playerInput.PlayerActionPerformed += this.OnPlayerActionPerformed;
		}

		// Token: 0x060009AE RID: 2478
		public abstract void SetSilentState(bool isActive);

		// Token: 0x060009AF RID: 2479
		public abstract void OnUpdate();

		// Token: 0x060009B0 RID: 2480
		protected abstract void OnPlayerActionPerformed(PlayerInputController.ActionArgs args);

		// Token: 0x060009B1 RID: 2481 RVA: 0x00021024 File Offset: 0x0001F224
		protected override void OnOwnerKilled(IGameMob owner)
		{
			base.OnOwnerKilled(owner);
			if (this.playerInput != null)
			{
				this.playerInput.PlayerActionPerformed -= this.OnPlayerActionPerformed;
			}
		}

		// Token: 0x040005AD RID: 1453
		protected IGame currentGame;

		// Token: 0x040005AE RID: 1454
		protected PlayerInputController playerInput;
	}
}
