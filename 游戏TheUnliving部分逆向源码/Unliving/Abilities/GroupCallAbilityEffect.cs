using System;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000385 RID: 901
	public sealed class GroupCallAbilityEffect : AbilityEffectBase, IRevertibleAbilityEffect
	{
		// Token: 0x06001DBF RID: 7615 RVA: 0x0005E690 File Offset: 0x0005C890
		private void CallGroup(BaseGameMob effectOwner, GameMobGroupController actualOwnerGroup)
		{
			if (this.ownerGroup != actualOwnerGroup && this.ownerGroup != null)
			{
				this.ownerGroup.GroupDestination = null;
			}
			if (actualOwnerGroup != null)
			{
				actualOwnerGroup.SetForcedGroupDestination(effectOwner.Position, effectOwner);
			}
			this.ownerGroup = actualOwnerGroup;
		}

		// Token: 0x06001DC0 RID: 7616 RVA: 0x0005E6D9 File Offset: 0x0005C8D9
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DC1 RID: 7617 RVA: 0x0005E6E0 File Offset: 0x0005C8E0
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DC2 RID: 7618 RVA: 0x0005E6E4 File Offset: 0x0005C8E4
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.ownerPosition != null && effectTarget != base.GetEffectOwner() as Component)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				GameMobGroupController gameMobGroupController = ((baseGameMob != null) ? baseGameMob.Group : null) as GameMobGroupController;
				if (gameMobGroupController != null)
				{
					gameMobGroupController.SetForcedGroupDestination(this.ownerPosition.Value, null);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001DC3 RID: 7619 RVA: 0x0005E741 File Offset: 0x0005C941
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new GroupCallAbilityEffect((GroupCallAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001DC4 RID: 7620 RVA: 0x0005E74E File Offset: 0x0005C94E
		public GroupCallAbilityEffect()
		{
		}

		// Token: 0x06001DC5 RID: 7621 RVA: 0x0005E756 File Offset: 0x0005C956
		public GroupCallAbilityEffect(GroupCallAbilityEffect effectPrototype)
		{
			this.forceCallAnyMobs = effectPrototype.forceCallAnyMobs;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x0005E774 File Offset: 0x0005C974
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			BaseGameMob baseGameMob = base.GetEffectOwner().CastOrGetComponent<BaseGameMob>();
			this.ownerPosition = ((baseGameMob != null) ? new Vector2?(baseGameMob.Position) : null);
			if (!this.forceCallAnyMobs)
			{
				if (baseGameMob != null)
				{
					GameMobGroupController gameMobGroupController = baseGameMob.Group as GameMobGroupController;
					if (gameMobGroupController != null)
					{
						this.CallGroup(baseGameMob, gameMobGroupController);
					}
				}
				return;
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x06001DC7 RID: 7623 RVA: 0x0005E7E0 File Offset: 0x0005C9E0
		void IRevertibleAbilityEffect.RevertEffect(IAbility ability, object effectTarget)
		{
			if (this.forceCallAnyMobs)
			{
				if (effectTarget != base.GetEffectOwner())
				{
					BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
					GameMobGroupController gameMobGroupController = ((baseGameMob != null) ? baseGameMob.Group : null) as GameMobGroupController;
					if (gameMobGroupController != null)
					{
						gameMobGroupController.GroupDestination = null;
						return;
					}
				}
			}
			else if (this.ownerGroup != null)
			{
				this.CallGroup(null, null);
			}
		}

		// Token: 0x040010CF RID: 4303
		public bool forceCallAnyMobs;

		// Token: 0x040010D0 RID: 4304
		private Vector2? ownerPosition;

		// Token: 0x040010D1 RID: 4305
		private GameMobGroupController ownerGroup;
	}
}
