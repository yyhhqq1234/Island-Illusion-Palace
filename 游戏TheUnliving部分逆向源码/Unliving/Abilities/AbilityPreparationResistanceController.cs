using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage.Projectiles;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000375 RID: 885
	[CreateAssetMenu(fileName = "AbilityPreparartionResistanceController", menuName = "Abilities/Controllers/Preparation Resistance Controller")]
	public sealed class AbilityPreparationResistanceController : AbilityExtensionAssetBase
	{
		// Token: 0x17000601 RID: 1537
		// (get) Token: 0x06001D07 RID: 7431 RVA: 0x0005BAD9 File Offset: 0x00059CD9
		// (set) Token: 0x06001D08 RID: 7432 RVA: 0x0005BAE1 File Offset: 0x00059CE1
		public float Resistance
		{
			get
			{
				return this._resistance;
			}
			set
			{
				this._resistance = value;
			}
		}

		// Token: 0x17000602 RID: 1538
		// (get) Token: 0x06001D09 RID: 7433 RVA: 0x0005BAEA File Offset: 0x00059CEA
		// (set) Token: 0x06001D0A RID: 7434 RVA: 0x0005BAF2 File Offset: 0x00059CF2
		public float ResistanceDamage
		{
			get
			{
				return this._resistanceDamage;
			}
			set
			{
				this._resistanceDamage = value;
			}
		}

		// Token: 0x17000603 RID: 1539
		// (get) Token: 0x06001D0B RID: 7435 RVA: 0x0005BAFB File Offset: 0x00059CFB
		// (set) Token: 0x06001D0C RID: 7436 RVA: 0x0005BB03 File Offset: 0x00059D03
		public float[] ResistanceDamageGains
		{
			get
			{
				return this._resistanceDamageGains;
			}
			set
			{
				this._resistanceDamageGains = value;
			}
		}

		// Token: 0x17000604 RID: 1540
		// (get) Token: 0x06001D0D RID: 7437 RVA: 0x0005BB0C File Offset: 0x00059D0C
		public bool HasResistanceDamageGains
		{
			get
			{
				return this._resistanceDamageGains != null && this._resistanceDamageGains.Length != 0;
			}
		}

		// Token: 0x17000605 RID: 1541
		// (get) Token: 0x06001D0E RID: 7438 RVA: 0x0005BB22 File Offset: 0x00059D22
		public float CurrentResistanceDamage
		{
			get
			{
				return this.currentResistanceDamage;
			}
		}

		// Token: 0x14000116 RID: 278
		// (add) Token: 0x06001D0F RID: 7439 RVA: 0x0005BB2C File Offset: 0x00059D2C
		// (remove) Token: 0x06001D10 RID: 7440 RVA: 0x0005BB64 File Offset: 0x00059D64
		public event Action<AbilityPreparationResistanceController, BaseAbility> OtherAbilityCastInterrupted;

		// Token: 0x06001D11 RID: 7441 RVA: 0x0005BB9C File Offset: 0x00059D9C
		private int AffectOtherAbilities(Component otherAbilityOwner, float resistanceDamage)
		{
			if (!this.isActive)
			{
				return 0;
			}
			BaseGameMob baseGameMob = otherAbilityOwner.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob == null || baseGameMob.IsKilled)
			{
				return 0;
			}
			GameAbilitiesController abilitiesController = baseGameMob.AbilitiesController;
			IReadOnlyList<BaseAbility> readOnlyList = (abilitiesController != null) ? abilitiesController.Abilities : null;
			int num = 0;
			if (readOnlyList != null)
			{
				float amountModifier = this.currentAbility.GetAmountModifier();
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					BaseAbility baseAbility = readOnlyList[i];
					if (!(baseAbility == null) && baseAbility.HasPrepTime() && baseAbility.PrepProgress > 0f)
					{
						AbilityPreparationResistanceController extension = baseAbility.GetExtension<AbilityPreparationResistanceController>();
						if (extension != null && extension.Resistance > 0f)
						{
							float num2 = resistanceDamage / extension.Resistance * amountModifier;
							baseAbility.ModifyPreparationProgress(-num2);
							if (baseAbility.PrepProgress == 0f)
							{
								Action<AbilityPreparationResistanceController, BaseAbility> otherAbilityCastInterrupted = this.OtherAbilityCastInterrupted;
								if (otherAbilityCastInterrupted != null)
								{
									otherAbilityCastInterrupted(this, baseAbility);
								}
							}
							num++;
						}
					}
				}
			}
			return num;
		}

		// Token: 0x06001D12 RID: 7442 RVA: 0x0005BC99 File Offset: 0x00059E99
		private void AffectOtherAbilities(Component abilitiesOwner)
		{
			this.AffectOtherAbilities(abilitiesOwner, this.currentResistanceDamage);
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x0005BCAC File Offset: 0x00059EAC
		private void AffectOtherAbilities(BaseAbility.UsingArgs abilityUsingArgs)
		{
			if (abilityUsingArgs.HasTargetsList)
			{
				IList<Component> targetsList = abilityUsingArgs.targetsList;
				for (int i = 0; i < targetsList.Count; i++)
				{
					this.AffectOtherAbilities(targetsList[i]);
				}
				return;
			}
			if (abilityUsingArgs.HasTargetObject)
			{
				this.AffectOtherAbilities(abilityUsingArgs.targetObject);
			}
		}

		// Token: 0x06001D14 RID: 7444 RVA: 0x0005BCFC File Offset: 0x00059EFC
		private void HandleAbilityProjectile(IProjectile projectile, BaseAbility.UsingArgs projectileUsingArgs)
		{
			AbilityPreparationResistanceController.<>c__DisplayClass30_0 CS$<>8__locals1 = new AbilityPreparationResistanceController.<>c__DisplayClass30_0();
			CS$<>8__locals1.projectile = projectile;
			if (this.launchedProjectilesInfo.Count == 0)
			{
				return;
			}
			int num = this.launchedProjectilesInfo.FindIndex(new Predicate<AbilityPreparationResistanceController.AbilityProjectileInfo>(CS$<>8__locals1.<HandleAbilityProjectile>g__IsTargetProjectile|0));
			if (num != -1)
			{
				if (projectileUsingArgs != null)
				{
					this.AffectOtherAbilities(projectileUsingArgs);
					return;
				}
				this.launchedProjectilesInfo.RemoveAt(num);
				CS$<>8__locals1.projectile.Destroyed -= this.OnAbilityProjectileDestroyed;
			}
		}

		// Token: 0x06001D15 RID: 7445 RVA: 0x0005BD70 File Offset: 0x00059F70
		private void ClearLaunchedAbilityProjectilesInfo()
		{
			for (int i = this.launchedProjectilesInfo.Count - 1; i >= 0; i--)
			{
				this.HandleAbilityProjectile(this.launchedProjectilesInfo[i].Projectile, null);
			}
			this.launchedProjectilesInfo.Clear();
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x0005BDB8 File Offset: 0x00059FB8
		private void Reset()
		{
			this.expectedNextActivationTime = -1f;
			this.currentUsingGainIndex = 0;
			this.currentResistanceDamage = this._resistanceDamage;
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x0005BDD8 File Offset: 0x00059FD8
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.isProjectileAbility = false;
			this.Reset();
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched += this.OnAbilityProjectileLaunched;
				projectileAbilityBase.ProjectileUsingPrepared += this.OnAbilityProjectileUsingPrepared;
				this.isProjectileAbility = true;
			}
			ability.Activated += this.OnAbilityActivated;
			ability.Used += this.OnAbilityUsed;
			ability.Completed += this.OnAbilityCompleted;
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x0005BE64 File Offset: 0x0005A064
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			this.ClearLaunchedAbilityProjectilesInfo();
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectileLaunched -= this.OnAbilityProjectileLaunched;
				projectileAbilityBase.ProjectileUsingPrepared -= this.OnAbilityProjectileUsingPrepared;
			}
			ability.Activated -= this.OnAbilityActivated;
			ability.Used -= this.OnAbilityUsed;
			ability.Completed -= this.OnAbilityCompleted;
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x0005BEE4 File Offset: 0x0005A0E4
		private void OnAbilityActivated(IAbility ability, object usingArgs)
		{
			if (this.HasResistanceDamageGains)
			{
				if (Time.realtimeSinceStartup < this.expectedNextActivationTime)
				{
					if (this.currentUsingGainIndex < this._resistanceDamageGains.Length - 1)
					{
						this.currentUsingGainIndex++;
					}
				}
				else
				{
					this.Reset();
				}
				this.currentResistanceDamage = this._resistanceDamage * this._resistanceDamageGains[this.currentUsingGainIndex];
				return;
			}
			this.Reset();
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x0005BF4F File Offset: 0x0005A14F
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			if (this.isProjectileAbility || this._resistanceDamage <= 0f)
			{
				return;
			}
			this.AffectOtherAbilities(usingArgs as BaseAbility.UsingArgs);
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x0005BF73 File Offset: 0x0005A173
		private void OnAbilityProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs eventArgs)
		{
			this.launchedProjectilesInfo.Add(new AbilityPreparationResistanceController.AbilityProjectileInfo(eventArgs.launchedProjectile, this.currentResistanceDamage));
			eventArgs.launchedProjectile.Destroyed += this.OnAbilityProjectileDestroyed;
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x0005BFA8 File Offset: 0x0005A1A8
		private void OnAbilityProjectileUsingPrepared(ProjectileAbilityBase.UsingEventArgs projectileUsingArgs)
		{
			ProjectileHitInfo projectileHitArgs = projectileUsingArgs.projectileHitArgs;
			this.HandleAbilityProjectile(projectileHitArgs.projectile, projectileUsingArgs.abilityUsingArgs);
			projectileHitArgs.isEffectiveHit = true;
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x0005BFD5 File Offset: 0x0005A1D5
		private void OnAbilityProjectileDestroyed(IProjectile projectile)
		{
			this.HandleAbilityProjectile(projectile, null);
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x0005BFDF File Offset: 0x0005A1DF
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			if (!this.currentAbility.WasUsed)
			{
				return;
			}
			if (this.HasResistanceDamageGains)
			{
				this.expectedNextActivationTime = Time.realtimeSinceStartup + this.currentAbility.GetTotalPreparationTime() + this._maxNextUseWaitingTime;
				return;
			}
			this.Reset();
		}

		// Token: 0x04001072 RID: 4210
		public bool isActive = true;

		// Token: 0x04001073 RID: 4211
		[SerializeField]
		[Tooltip("Устойчивость к сбиванию способности.")]
		private float _resistance;

		// Token: 0x04001074 RID: 4212
		[SerializeField]
		[Tooltip("Урон, наносимый прогрессу подготовки способностей, против которых используется данный контроллер.")]
		private float _resistanceDamage;

		// Token: 0x04001075 RID: 4213
		[SerializeField]
		[Tooltip("Усиления урона (множители). Выбираются по количеству последовательных использований способности (комбо).")]
		private float[] _resistanceDamageGains;

		// Token: 0x04001076 RID: 4214
		[SerializeField]
		[Tooltip("Максимальное время ожидания следующего использования способности для учета комбо.")]
		private float _maxNextUseWaitingTime = 0.3f;

		// Token: 0x04001077 RID: 4215
		private readonly List<AbilityPreparationResistanceController.AbilityProjectileInfo> launchedProjectilesInfo = new List<AbilityPreparationResistanceController.AbilityProjectileInfo>();

		// Token: 0x04001078 RID: 4216
		private bool isProjectileAbility;

		// Token: 0x04001079 RID: 4217
		private float currentResistanceDamage;

		// Token: 0x0400107A RID: 4218
		private int currentUsingGainIndex;

		// Token: 0x0400107B RID: 4219
		private float expectedNextActivationTime;

		// Token: 0x02000565 RID: 1381
		private readonly struct AbilityProjectileInfo
		{
			// Token: 0x0600270B RID: 9995 RVA: 0x0007994A File Offset: 0x00077B4A
			public AbilityProjectileInfo(IProjectile projectile, float resistanceDamage)
			{
				this.Projectile = projectile;
				this.ResistanceDamage = resistanceDamage;
			}

			// Token: 0x04001C16 RID: 7190
			public readonly IProjectile Projectile;

			// Token: 0x04001C17 RID: 7191
			public readonly float ResistanceDamage;
		}
	}
}
