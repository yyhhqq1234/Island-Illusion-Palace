using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Game.Abilities;
using Game.Factories;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200037E RID: 894
	[CreateAssetMenu(fileName = "ProjectileAbilityUsingBlockController", menuName = "Abilities/Controllers/Projectile Ability Using Block Controller")]
	public sealed class ProjectileAbilityUsingBlockController : AbilityExtensionAssetBase
	{
		// Token: 0x06001D73 RID: 7539 RVA: 0x0005D760 File Offset: 0x0005B960
		private void RemoveAdditionalAbilitiesBlock()
		{
			if (this.additionalAbilitiesToBlock != null)
			{
				for (int i = 0; i < this.additionalAbilitiesToBlock.Count; i++)
				{
					BaseAbility baseAbility = this.additionalAbilitiesToBlock[i];
					if (baseAbility != null)
					{
						baseAbility.RemovePreActivationCondition(new BaseAbility.ActivationCondition(this.IsAbilityUsingAllowed));
					}
				}
			}
		}

		// Token: 0x06001D74 RID: 7540 RVA: 0x0005D7B3 File Offset: 0x0005B9B3
		private bool IsAbilityUsingAllowed(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs)
		{
			return this.maxLaunchedProjectileCount <= 0 || this.projectileAbility.LaunchedProjectilesCount < this.maxLaunchedProjectileCount;
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x0005D7D4 File Offset: 0x0005B9D4
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.projectileAbility = (ability as ProjectileAbilityBase);
			if (this.projectileAbility != null)
			{
				if (this.blockCurrentAbility)
				{
					this.projectileAbility.AddPreActivationCondition(new BaseAbility.ActivationCondition(this.IsAbilityUsingAllowed));
				}
				this.projectileAbility.Activated += this.OnAbilityPrepared;
			}
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x0005D838 File Offset: 0x0005BA38
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			if (this.projectileAbility != null)
			{
				this.currentAbilitiesController = null;
				this.RemoveAdditionalAbilitiesBlock();
				this.projectileAbility.RemovePreActivationCondition(new BaseAbility.ActivationCondition(this.IsAbilityUsingAllowed));
				this.projectileAbility.Activated -= this.OnAbilityPrepared;
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x0005D895 File Offset: 0x0005BA95
		protected override void OnAbilityOwnerChanged(BaseAbility currentAbility, object lastOwner, object newOwner)
		{
			if (newOwner == null)
			{
				this.RemoveAdditionalAbilitiesBlock();
			}
		}

		// Token: 0x06001D78 RID: 7544 RVA: 0x0005D8A0 File Offset: 0x0005BAA0
		private void OnAbilityPrepared(IAbility ability, object usingArgs)
		{
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			GameAbilitiesController gameAbilitiesController = (baseGameMob != null) ? baseGameMob.AbilitiesController : null;
			if (this.currentAbilitiesController != gameAbilitiesController)
			{
				if (gameAbilitiesController != null)
				{
					if (this.additionalAbilitiesToBlock == null)
					{
						this.additionalAbilitiesToBlock = new List<BaseAbility>(4);
					}
					else
					{
						this.RemoveAdditionalAbilitiesBlock();
						this.additionalAbilitiesToBlock.Clear();
					}
					foreach (BaseAbility baseAbility in gameAbilitiesController.Abilities)
					{
						if (baseAbility != null && this.<OnAbilityPrepared>g__IsAbilityToBlock|11_0(baseAbility.ID))
						{
							baseAbility.AddPreActivationCondition(new BaseAbility.ActivationCondition(this.IsAbilityUsingAllowed));
							this.additionalAbilitiesToBlock.Add(baseAbility);
						}
					}
				}
				this.currentAbilitiesController = gameAbilitiesController;
			}
		}

		// Token: 0x06001D7A RID: 7546 RVA: 0x0005D98C File Offset: 0x0005BB8C
		[CompilerGenerated]
		private bool <OnAbilityPrepared>g__IsAbilityToBlock|11_0(int abilityID)
		{
			AbilityID[] array = this.additionalAbilitiesIDToBlock;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == (AbilityID)abilityID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x040010A4 RID: 4260
		public int maxLaunchedProjectileCount = 1;

		// Token: 0x040010A5 RID: 4261
		public bool blockCurrentAbility = true;

		// Token: 0x040010A6 RID: 4262
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID[] additionalAbilitiesIDToBlock;

		// Token: 0x040010A7 RID: 4263
		private ProjectileAbilityBase projectileAbility;

		// Token: 0x040010A8 RID: 4264
		private List<BaseAbility> additionalAbilitiesToBlock;

		// Token: 0x040010A9 RID: 4265
		private BaseAbilitiesController currentAbilitiesController;
	}
}
