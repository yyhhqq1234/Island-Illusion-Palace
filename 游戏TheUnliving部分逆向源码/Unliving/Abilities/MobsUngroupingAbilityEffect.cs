using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200038F RID: 911
	[Serializable]
	public sealed class MobsUngroupingAbilityEffect : StateBasedAbilityEffect
	{
		// Token: 0x06001E0C RID: 7692 RVA: 0x0005F1A3 File Offset: 0x0005D3A3
		public MobsUngroupingAbilityEffect()
		{
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x0005F1BF File Offset: 0x0005D3BF
		public MobsUngroupingAbilityEffect(MobsUngroupingAbilityEffect effectPrototype)
		{
			this.newFaction = effectPrototype.newFaction;
			this.useAbilityOwnerFaction = effectPrototype.useAbilityOwnerFaction;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x0005F1FC File Offset: 0x0005D3FC
		private GameMobGroupController GetTargetGroup()
		{
			if (this.targetGroup == null)
			{
				BaseGameMob baseGameMob = this.currentAbility.Owner as BaseGameMob;
				this.targetGroup = new GameMobGroupController
				{
					Faction = ((this.useAbilityOwnerFaction && baseGameMob != null) ? baseGameMob.Faction : this.newFaction)
				};
				if (baseGameMob != null)
				{
					GameMobsGroupControllerBase group = baseGameMob.Group;
					int num = (group != null) ? group.GroupID : baseGameMob.GetInstanceID();
					GameMobsGroupControllerBase gameMobsGroupControllerBase = this.targetGroup;
					int groupID = num;
					GameObject groupHolder = null;
					GameMobsGroupControllerBase group2 = baseGameMob.Group;
					gameMobsGroupControllerBase.Initialize(groupID, groupHolder, (group2 != null) ? group2.InitialPosition : baseGameMob.Position);
				}
			}
			return this.targetGroup;
		}

		// Token: 0x06001E0F RID: 7695 RVA: 0x0005F2A0 File Offset: 0x0005D4A0
		protected override void SetEffectActive(Component effectTarget, bool isActive)
		{
			if (isActive)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				GameMobsGroupControllerBase gameMobsGroupControllerBase = (baseGameMob != null) ? baseGameMob.Group : null;
				if (gameMobsGroupControllerBase != null && this.GetTargetGroup().AddMob(baseGameMob, null))
				{
					this.storedGroupsInfo.Add(new MobsUngroupingAbilityEffect.StoredGroupInfo(baseGameMob, gameMobsGroupControllerBase));
					return;
				}
			}
			else
			{
				for (int i = this.storedGroupsInfo.Count - 1; i >= 0; i--)
				{
					if (this.storedGroupsInfo[i].RestoreGroup(effectTarget))
					{
						this.storedGroupsInfo.RemoveAt(i);
						return;
					}
				}
			}
		}

		// Token: 0x06001E10 RID: 7696 RVA: 0x0005F325 File Offset: 0x0005D525
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001E11 RID: 7697 RVA: 0x0005F32C File Offset: 0x0005D52C
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x0005F32E File Offset: 0x0005D52E
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new MobsUngroupingAbilityEffect((MobsUngroupingAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x0005F33B File Offset: 0x0005D53B
		public override void Reset()
		{
			this.storedGroupsInfo.Clear();
			base.Reset();
		}

		// Token: 0x040010E6 RID: 4326
		public GameMobFactions newFaction = GameMobFactions.PLAYER_ALLIES;

		// Token: 0x040010E7 RID: 4327
		public bool useAbilityOwnerFaction;

		// Token: 0x040010E8 RID: 4328
		private GameMobGroupController targetGroup;

		// Token: 0x040010E9 RID: 4329
		private readonly List<MobsUngroupingAbilityEffect.StoredGroupInfo> storedGroupsInfo = new List<MobsUngroupingAbilityEffect.StoredGroupInfo>(50);

		// Token: 0x02000574 RID: 1396
		private readonly struct StoredGroupInfo
		{
			// Token: 0x06002729 RID: 10025 RVA: 0x0007A419 File Offset: 0x00078619
			public StoredGroupInfo(BaseGameMob mob, GameMobsGroupControllerBase lastMobGroup)
			{
				this.Mob = mob;
				this.LastMobGroup = lastMobGroup;
			}

			// Token: 0x0600272A RID: 10026 RVA: 0x0007A429 File Offset: 0x00078629
			public bool IsCurrentMobComponent(Component mobComponent)
			{
				return !this.Mob.IsNull() && this.Mob.gameObject.GetInstanceID() == mobComponent.gameObject.GetInstanceID();
			}

			// Token: 0x0600272B RID: 10027 RVA: 0x0007A457 File Offset: 0x00078657
			public bool RestoreGroup()
			{
				GameMobsGroupControllerBase lastMobGroup = this.LastMobGroup;
				return lastMobGroup != null && lastMobGroup.AddMob(this.Mob, null);
			}

			// Token: 0x0600272C RID: 10028 RVA: 0x0007A471 File Offset: 0x00078671
			public bool RestoreGroup(Component mobComponent)
			{
				return this.IsCurrentMobComponent(mobComponent) && this.RestoreGroup();
			}

			// Token: 0x04001C53 RID: 7251
			public readonly BaseGameMob Mob;

			// Token: 0x04001C54 RID: 7252
			public readonly GameMobsGroupControllerBase LastMobGroup;
		}
	}
}
