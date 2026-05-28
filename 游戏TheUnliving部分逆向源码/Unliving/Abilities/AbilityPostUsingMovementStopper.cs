using System;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities
{
	// Token: 0x02000373 RID: 883
	[CreateAssetMenu(fileName = "AbilityPostUsingMovementStopper", menuName = "Abilities/Controllers/Post Using Movement Stopper")]
	public sealed class AbilityPostUsingMovementStopper : AbilityExtensionAssetBase
	{
		// Token: 0x170005FF RID: 1535
		// (get) Token: 0x06001CFF RID: 7423 RVA: 0x0005B9FA File Offset: 0x00059BFA
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x0005B9FD File Offset: 0x00059BFD
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ability.Completed += this.OnAbilityCompleted;
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x0005BA18 File Offset: 0x00059C18
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Completed -= this.OnAbilityCompleted;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x0005BA34 File Offset: 0x00059C34
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			if (this.movementFreezingDuration > 0f)
			{
				BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
				if (baseGameMob != null && ((BaseAbility)ability).WasUsed)
				{
					GameMobMotionControllerBase motionController = baseGameMob.MotionController;
					if (motionController == null)
					{
						return;
					}
					motionController.FreezeMovement(this.movementFreezingDuration, false);
				}
			}
		}

		// Token: 0x0400106E RID: 4206
		public float movementFreezingDuration = 2f;
	}
}
