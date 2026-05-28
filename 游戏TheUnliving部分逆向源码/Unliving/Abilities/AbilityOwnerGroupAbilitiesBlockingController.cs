using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;
using Unliving.Abilities.Buffs;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000370 RID: 880
	[CreateAssetMenu(fileName = "AbilityOwnerGroupAbilitiesBlockingController", menuName = "Abilities/Controllers/Owner Group Abilities Blocking Controller")]
	public sealed class AbilityOwnerGroupAbilitiesBlockingController : AbilityOwnerGroupBuffsControllerBase
	{
		// Token: 0x06001CF1 RID: 7409 RVA: 0x0005B6B8 File Offset: 0x000598B8
		protected override void SendBuffs(IAbility ability, BaseGameMob abilityOwner, IBuffsController groupMobBuffsController)
		{
			if (this.abilitiesToBlock == null || this.abilitiesToBlock.Length == 0 || this.blockingDuration <= 0f)
			{
				return;
			}
			BuffBase.InitializationArgs initializationArgs = new BuffBase.InitializationArgs
			{
				duration = this.blockingDuration,
				isAdditive = false,
				isConstant = false
			};
			AbilityDescription[] abilitiesDescriptions;
			if (this.sameAbilitiesBlockingData == null || !this.sameAbilitiesBlockingData.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out abilitiesDescriptions))
			{
				abilitiesDescriptions = this.abilitiesToBlock;
			}
			AbilitiesUsingRestrictionBuff buff = new AbilitiesUsingRestrictionBuff(BuffBase.TryGetNewBuffTypeID(ref this.blockingBuffsID), abilityOwner, initializationArgs, abilitiesDescriptions, this.interruptActiveAbilities);
			groupMobBuffsController.AddBuff(buff);
		}

		// Token: 0x06001CF2 RID: 7410 RVA: 0x0005B754 File Offset: 0x00059954
		public override void OnAddedToAbility(BaseAbility ability)
		{
			if (this.blockSameAbilitiesOnly)
			{
				if (this.sameAbilitiesBlockingData == null)
				{
					this.sameAbilitiesBlockingData = new Dictionary<int, AbilityDescription[]>(32);
				}
				AbilityDescription[] value = new AbilityDescription[]
				{
					new AbilityDescription
					{
						abilityPrototypeReference = ability.Prototype,
						abilityID = (AbilityID)ability.ID
					}
				};
				this.sameAbilitiesBlockingData.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), value);
			}
			base.OnAddedToAbility(ability);
		}

		// Token: 0x06001CF3 RID: 7411 RVA: 0x0005B7C8 File Offset: 0x000599C8
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			Dictionary<int, AbilityDescription[]> dictionary = this.sameAbilitiesBlockingData;
			if (dictionary != null)
			{
				dictionary.Remove(AbilityExtensionAssetBase.GetAbilityInstanceID(ability));
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x04001062 RID: 4194
		public AbilityDescription[] abilitiesToBlock;

		// Token: 0x04001063 RID: 4195
		public float blockingDuration = 5f;

		// Token: 0x04001064 RID: 4196
		public bool interruptActiveAbilities;

		// Token: 0x04001065 RID: 4197
		[Tooltip("Если активно, то блокироваться будут только абилити с таким же прототипом или айди как у данной абилити.")]
		public bool blockSameAbilitiesOnly;

		// Token: 0x04001066 RID: 4198
		private Dictionary<int, AbilityDescription[]> sameAbilitiesBlockingData;

		// Token: 0x04001067 RID: 4199
		private int? blockingBuffsID;
	}
}
