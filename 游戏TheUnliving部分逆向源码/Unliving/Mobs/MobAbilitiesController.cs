using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Abilities;

namespace Unliving.Mobs
{
	// Token: 0x020001ED RID: 493
	[Serializable]
	public sealed class MobAbilitiesController : GameAbilitiesController
	{
		// Token: 0x17000355 RID: 853
		// (get) Token: 0x0600105E RID: 4190 RVA: 0x00033156 File Offset: 0x00031356
		public int SpecialAbilitiesStartIndex
		{
			get
			{
				return this.specialAbilitiesStartIndex;
			}
		}

		// Token: 0x0600105F RID: 4191 RVA: 0x0003315E File Offset: 0x0003135E
		public MobAbilitiesController(MobBehaviour mob, IGameAbilitiesFactory abilitiesFactory) : base(mob, abilitiesFactory)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.CurrentMob = mob;
			mob.Sacrificed += this.OnMobSacrificed;
			mob.Destroyed += this.OnMobDestroyed;
		}

		// Token: 0x06001060 RID: 4192 RVA: 0x0003319C File Offset: 0x0003139C
		public void Initialize(MobAttackData attackData, int attackLayers)
		{
			if (this.isInitialized)
			{
				return;
			}
			this.defaultAttackTargetsLayers = attackLayers;
			this.defaultSupportTargetsLayers = 1 << this.CurrentMob.gameObject.layer;
			this.specialAbilitiesStartIndex = 0;
			if (attackData != null)
			{
				this.AddAbility(this.CurrentMob.MeleeAttackAbility, MobAbilitiesController.MobAbilitySlots.Melee, attackData.MeleeAttackTimeout, attackData.MeleeAttackDistance, attackData.MeleeAttackDamage);
				this.AddAbility(this.CurrentMob.RangeAttackAbility, MobAbilitiesController.MobAbilitySlots.Ranged, attackData.RangeAttackTimeout, attackData.RangeAttackDistance, attackData.RangeAttackDamage);
			}
			this.AddAbility(this.CurrentMob.Slot3, MobAbilitiesController.MobAbilitySlots.Extra, -1f, -1f, -1f);
			this.AddAbility(this.CurrentMob.Slot4, MobAbilitiesController.MobAbilitySlots.Extra, -1f, -1f, -1f);
			this.AddAbility(this.CurrentMob.Slot5, MobAbilitiesController.MobAbilitySlots.Extra, -1f, -1f, -1f);
			this.AddAbility(this.CurrentMob.Slot6, MobAbilitiesController.MobAbilitySlots.Extra, -1f, -1f, -1f);
			base.RegisterAbilitiesStats();
			this.isInitialized = true;
		}

		// Token: 0x06001061 RID: 4193 RVA: 0x000332C0 File Offset: 0x000314C0
		public BaseAbility AddAbility(AbilityID id, MobAbilitiesController.MobAbilitySlots slot, float timeoutOverride = -1f, float rangeOverride = -1f, float damageOverride = -1f)
		{
			if (id != AbilityID.None)
			{
				BaseAbility baseAbility = base.AddAbility((AbilityInfo)id);
				if (baseAbility != null)
				{
					if (timeoutOverride > 0f)
					{
						baseAbility.ReloadingTime = timeoutOverride;
					}
					if (rangeOverride > 0f)
					{
						baseAbility.Range = rangeOverride;
					}
					bool flag = baseAbility.ValidObjectLayers == 0;
					if (baseAbility.IsBattleAbility())
					{
						if (flag)
						{
							baseAbility.ValidObjectLayers = this.defaultAttackTargetsLayers;
						}
						if (damageOverride > 0f)
						{
							IDamageSender damageSender = baseAbility as IDamageSender;
							DamageGenerator damageGenerator = (damageSender != null) ? damageSender.DamageGenerator : null;
							if (damageGenerator != null)
							{
								damageGenerator.amount = damageOverride;
							}
						}
					}
					else if (flag)
					{
						BaseAbility baseAbility2 = baseAbility;
						baseAbility2.ValidObjectLayers |= this.defaultSupportTargetsLayers;
					}
					if (slot == MobAbilitiesController.MobAbilitySlots.Melee || slot == MobAbilitiesController.MobAbilitySlots.Ranged)
					{
						this.specialAbilitiesStartIndex++;
					}
					return baseAbility;
				}
			}
			return null;
		}

		// Token: 0x06001062 RID: 4194 RVA: 0x00033398 File Offset: 0x00031598
		public override bool IsSpecialAbility(int abilityID)
		{
			int num;
			this.GetAbilityByID(abilityID, out num);
			return num >= this.specialAbilitiesStartIndex;
		}

		// Token: 0x06001063 RID: 4195 RVA: 0x000333BC File Offset: 0x000315BC
		protected override void UpdateAbilities(float deltaTime)
		{
			Vector2 hitColliderCenter = this.CurrentMob.HitColliderCenter;
			for (int i = 0; i < this.abilities.Count; i++)
			{
				BaseAbility baseAbility = this.abilities[i];
				baseAbility.OwnerPosition = hitColliderCenter;
				baseAbility.UpdateAbility(deltaTime);
			}
		}

		// Token: 0x06001064 RID: 4196 RVA: 0x0003340C File Offset: 0x0003160C
		private void OnMobSacrificed(BaseGameMob mob)
		{
			IReadOnlyList<BaseAbility> abilities = base.Abilities;
			for (int i = abilities.Count - 1; i >= 0; i--)
			{
				BaseAbility baseAbility = abilities[i];
				if (baseAbility.IsMobActivationAbility())
				{
					if (baseAbility.IsActivationBlockedByTriggers(null))
					{
						base.RemoveAbilityAt(i);
					}
					else
					{
						baseAbility.IgnoreAbilitiesControllerInactivity = true;
					}
				}
			}
		}

		// Token: 0x06001065 RID: 4197 RVA: 0x0003345D File Offset: 0x0003165D
		private void OnMobDestroyed(object mob)
		{
			this.CurrentMob.Sacrificed -= this.OnMobSacrificed;
			this.CurrentMob.Destroyed -= this.OnMobDestroyed;
		}

		// Token: 0x0400094D RID: 2381
		public readonly MobBehaviour CurrentMob;

		// Token: 0x0400094E RID: 2382
		private int specialAbilitiesStartIndex;

		// Token: 0x0400094F RID: 2383
		private int defaultAttackTargetsLayers;

		// Token: 0x04000950 RID: 2384
		private int defaultSupportTargetsLayers;

		// Token: 0x04000951 RID: 2385
		private bool isInitialized;

		// Token: 0x020004A5 RID: 1189
		public enum MobAbilitySlots
		{
			// Token: 0x0400190F RID: 6415
			Melee,
			// Token: 0x04001910 RID: 6416
			Ranged,
			// Token: 0x04001911 RID: 6417
			Extra
		}
	}
}
