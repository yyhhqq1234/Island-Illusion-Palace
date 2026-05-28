using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000372 RID: 882
	public abstract class AbilityOwnerGroupBuffsControllerBase : AbilityExtensionAssetBase
	{
		// Token: 0x170005FE RID: 1534
		// (get) Token: 0x06001CF7 RID: 7415 RVA: 0x0005B855 File Offset: 0x00059A55
		public sealed override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001CF8 RID: 7416 RVA: 0x0005B858 File Offset: 0x00059A58
		private void SendBuffs(IAbility ability)
		{
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			GameMobsGroupControllerBase gameMobsGroupControllerBase = (baseGameMob != null) ? baseGameMob.Group : null;
			if (gameMobsGroupControllerBase == null)
			{
				return;
			}
			IReadOnlyList<BaseGameMob> mobs = gameMobsGroupControllerBase.Mobs;
			if (mobs.Count == 0)
			{
				return;
			}
			MobBehaviour.ID id;
			if (this.affectOwnerLikeMobsOnly)
			{
				MobBehaviour mobBehaviour = baseGameMob as MobBehaviour;
				if (mobBehaviour != null)
				{
					id = mobBehaviour.ObjectID;
					goto IL_4B;
				}
			}
			id = MobBehaviour.ID.None;
			IL_4B:
			MobBehaviour.ID id2 = id;
			bool flag = this.affectableMobsID.Length != 0;
			bool flag2 = flag || id2 > MobBehaviour.ID.None;
			for (int i = 0; i < mobs.Count; i++)
			{
				BaseGameMob baseGameMob2 = mobs[i];
				if (!this.ignoreOwner || !(baseGameMob2 == baseGameMob))
				{
					IBuffsController buffsController = baseGameMob2.BuffsController;
					if (buffsController != null)
					{
						if (flag2 && baseGameMob2.IsCharacter)
						{
							MobBehaviour mobBehaviour2 = baseGameMob2 as MobBehaviour;
							if (mobBehaviour2 != null && ((id2 != MobBehaviour.ID.None) ? (mobBehaviour2.ObjectID != id2) : (flag && Array.IndexOf<MobBehaviour.ID>(this.affectableMobsID, mobBehaviour2.ObjectID) < 0)))
							{
								goto IL_EA;
							}
						}
						this.SendBuffs(ability, baseGameMob, buffsController);
					}
				}
				IL_EA:;
			}
		}

		// Token: 0x06001CF9 RID: 7417
		protected abstract void SendBuffs(IAbility ability, BaseGameMob abilityOwner, IBuffsController groupMobBuffsController);

		// Token: 0x06001CFA RID: 7418 RVA: 0x0005B962 File Offset: 0x00059B62
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			if (this.sendBuffsBeforeAbilityUsed)
			{
				ability.Activating += this.OnAbilityActivating;
				return;
			}
			ability.Used += this.OnAbilityUsed;
		}

		// Token: 0x06001CFB RID: 7419 RVA: 0x0005B998 File Offset: 0x00059B98
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activating -= this.OnAbilityActivating;
			ability.Used -= this.OnAbilityUsed;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001CFC RID: 7420 RVA: 0x0005B9C5 File Offset: 0x00059BC5
		private void OnAbilityActivating(IAbility ability, object usingArgs)
		{
			if (ability.PrepProgress == 0f)
			{
				this.SendBuffs(ability);
			}
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x0005B9DB File Offset: 0x00059BDB
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			this.SendBuffs(ability);
		}

		// Token: 0x0400106A RID: 4202
		public MobBehaviour.ID[] affectableMobsID;

		// Token: 0x0400106B RID: 4203
		public bool affectOwnerLikeMobsOnly;

		// Token: 0x0400106C RID: 4204
		[Tooltip("Если активно, то бафф не будет накладываться на овнера абилити.")]
		public bool ignoreOwner = true;

		// Token: 0x0400106D RID: 4205
		public bool sendBuffsBeforeAbilityUsed = true;
	}
}
