using System;
using UnityEngine;
using Unliving.Mobs.Motion;

namespace Unliving.Player
{
	// Token: 0x0200015F RID: 351
	public sealed class PlayerMotionController : GameMobMotionControllerBase
	{
		// Token: 0x060009DB RID: 2523 RVA: 0x00021B07 File Offset: 0x0001FD07
		public PlayerMotionController(PlayerBehaviour player) : base(player)
		{
			this.inputController = player.PlayerInputController;
			this.NavMeshAgentComponent.autoBraking = false;
			this.NavMeshAgentComponent.autoRepath = false;
			this.NavMeshAgentComponent.updatePosition = false;
		}

		// Token: 0x060009DC RID: 2524 RVA: 0x00021B40 File Offset: 0x0001FD40
		protected override void ApplyVelocity(Vector2 desiredVelocity, float dt)
		{
			Vector3 vector = desiredVelocity;
			this.RigidbodyComponent.velocity = vector;
			if (!this.ControllerOwner.IsKinematic && this.NavMeshAgentComponent.enabled)
			{
				this.NavMeshAgentComponent.velocity = vector;
				this.NavMeshAgentComponent.nextPosition = this.TransformComponent.position;
			}
		}

		// Token: 0x060009DD RID: 2525 RVA: 0x00021BA4 File Offset: 0x0001FDA4
		public override void OnUpdate()
		{
			Vector2 movementInput = this.inputController.GetMovementInput();
			movementInput.Normalize();
			base.MoveInDirection(movementInput);
		}

		// Token: 0x040005CA RID: 1482
		private readonly PlayerInputController inputController;
	}
}
