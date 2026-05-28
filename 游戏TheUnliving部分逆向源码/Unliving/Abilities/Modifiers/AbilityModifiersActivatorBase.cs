using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Abilities.Modifiers
{
	// Token: 0x020003C6 RID: 966
	public abstract class AbilityModifiersActivatorBase : ScriptableObject
	{
		// Token: 0x060020CC RID: 8396 RVA: 0x000674FA File Offset: 0x000656FA
		private static IBuffsController GetBuffsReceiver(BaseAbility ability)
		{
			IBuffableObject buffableObject = (IBuffableObject)ability.OwnerBehaviour;
			if (buffableObject == null)
			{
				return null;
			}
			return buffableObject.BuffsController;
		}

		// Token: 0x060020CD RID: 8397 RVA: 0x00067512 File Offset: 0x00065712
		protected static int GetAbilityInstanceID(IAbility ability)
		{
			return ((UnityEngine.Object)ability).GetInstanceID();
		}

		// Token: 0x060020CE RID: 8398 RVA: 0x0006751F File Offset: 0x0006571F
		protected static int GetAbilityInstanceID(BaseAbility ability)
		{
			return ability.GetInstanceID();
		}

		// Token: 0x060020CF RID: 8399 RVA: 0x00067528 File Offset: 0x00065728
		private async void ActivateSpecialOwnerState(BaseAbility ability)
		{
			await Task.Yield();
			if (!ability.IsNull() && ability.HasModifiersActivator(this))
			{
				this.abilityStatsModifiers.ModifyStats(ability.GetAbilityStats(), true, -1, null);
				if (this.activatorOwnersStateGenerators != null)
				{
					int num = this.activatorOwnersStateGenerators.Length;
					if (num != 0)
					{
						IBuffsController buffsReceiver = AbilityModifiersActivatorBase.GetBuffsReceiver(ability);
						if (buffsReceiver != null)
						{
							if (this.ownersStateGenerators == null)
							{
								this.activatorOwnersStateGenerators.Instantiate(out this.ownersStateGenerators);
								this.activeOwnersStates = new Dictionary<int, IBuff[]>(16);
							}
							int abilityInstanceID = AbilityModifiersActivatorBase.GetAbilityInstanceID(ability);
							IBuff[] array = new IBuff[num];
							for (int i = 0; i < num; i++)
							{
								IBuff buff = this.ownersStateGenerators[i].GenerateBuff(this, true);
								if (buffsReceiver.AddBuff(buff))
								{
									array[i] = buff;
								}
							}
							this.activeOwnersStates.Add(abilityInstanceID, array);
						}
					}
				}
			}
		}

		// Token: 0x060020D0 RID: 8400 RVA: 0x0006756C File Offset: 0x0006576C
		private void DeactivateSpecialOwnerState(BaseAbility ability)
		{
			if (this.activeOwnersStates == null)
			{
				return;
			}
			this.abilityStatsModifiers.ModifyStats(ability.GetAbilityStats(), false, -1, null);
			int abilityInstanceID = AbilityModifiersActivatorBase.GetAbilityInstanceID(ability);
			IBuff[] array;
			if (this.activeOwnersStates.TryGetValue(abilityInstanceID, out array))
			{
				foreach (IBuff buff in array)
				{
					if (buff != null)
					{
						buff.Complete();
					}
				}
			}
			this.activeOwnersStates.Remove(abilityInstanceID);
		}

		// Token: 0x060020D1 RID: 8401 RVA: 0x000675D8 File Offset: 0x000657D8
		protected AbilityModifierUsingArgs InitializeModifiersUsingArgs()
		{
			this.modifiersUsingArgs.Reset();
			return this.modifiersUsingArgs;
		}

		// Token: 0x060020D2 RID: 8402 RVA: 0x000675EC File Offset: 0x000657EC
		protected void PrepareModifiersUsingArgs(AbilityModifiersActivatorArgs activatorArgs, AbilityModifierUsingArgs modifiersUsingArgs, int usingCount, bool clampUsingCount = false)
		{
			if (clampUsingCount)
			{
				AbilityModifiersOverrides overrides = activatorArgs.overrides;
				if (overrides != null)
				{
					overrides.ClampUsingCount(ref usingCount);
				}
			}
			else
			{
				AbilityModifiersOverrides overrides2 = activatorArgs.overrides;
				if (overrides2 != null)
				{
					overrides2.SetUsingCount(ref usingCount);
				}
			}
			activatorArgs.CopyCommonValues(modifiersUsingArgs);
			modifiersUsingArgs.modifiersUsingCount = Mathf.Max(usingCount, 1);
		}

		// Token: 0x060020D3 RID: 8403
		protected abstract bool SetActive(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError);

		// Token: 0x060020D4 RID: 8404
		public abstract bool TryUse(AbilityModifiersActivatorArgs args, out AbilityModifierUsingArgs modifiersUsingArgs);

		// Token: 0x060020D5 RID: 8405
		public abstract void Reset(AbilityModifiersController modifiersController, BaseAbility ability, bool force = false);

		// Token: 0x060020D6 RID: 8406 RVA: 0x00067639 File Offset: 0x00065839
		public void RegisterAbility(BaseAbility ability)
		{
			this.ActivateSpecialOwnerState(ability);
			this.OnAbilityRegistered(ability);
		}

		// Token: 0x060020D7 RID: 8407 RVA: 0x00067649 File Offset: 0x00065849
		public void UnregisterAbility(BaseAbility ability)
		{
			this.OnAbilityUnregistered(ability);
			this.DeactivateSpecialOwnerState(ability);
		}

		// Token: 0x060020D8 RID: 8408 RVA: 0x00067659 File Offset: 0x00065859
		public virtual bool IsAllowedActivationStage(AbilityUsingStage activationStage)
		{
			return activationStage != AbilityUsingStage.PostUsed;
		}

		// Token: 0x060020D9 RID: 8409 RVA: 0x00067664 File Offset: 0x00065864
		public virtual bool CanBeActivated(AbilityModifiersActivatorArgs args)
		{
			AbilityUsingStage abilityUsingStage = args.abilityUsingStage;
			if (!this.IsAllowedActivationStage(abilityUsingStage))
			{
				return false;
			}
			if (this.activateOnHitOnly)
			{
				BaseAbility.UsingArgs abilityUsingArgs = args.abilityUsingArgs;
				return (abilityUsingStage == AbilityUsingStage.Used || abilityUsingStage == AbilityUsingStage.PostUsed) && (abilityUsingArgs.HasTargetObject || abilityUsingArgs.HasTargetsList);
			}
			return true;
		}

		// Token: 0x060020DA RID: 8410 RVA: 0x000676AE File Offset: 0x000658AE
		public bool TryActivate(AbilityModifiersActivatorArgs args, out BaseAbility.ActivationError abilityActivationError)
		{
			abilityActivationError = null;
			return this.CanBeActivated(args) && this.SetActive(args, out abilityActivationError);
		}

		// Token: 0x060020DB RID: 8411 RVA: 0x000676C6 File Offset: 0x000658C6
		protected virtual void OnAbilityRegistered(BaseAbility ability)
		{
		}

		// Token: 0x060020DC RID: 8412 RVA: 0x000676C8 File Offset: 0x000658C8
		protected virtual void OnAbilityUnregistered(BaseAbility ability)
		{
		}

		// Token: 0x04001497 RID: 5271
		public TargetedMobStatModifier[] abilityStatsModifiers;

		// Token: 0x04001498 RID: 5272
		public BuffsGeneratorBuilderAsset.Reference[] activatorOwnersStateGenerators;

		// Token: 0x04001499 RID: 5273
		public bool activateOnHitOnly;

		// Token: 0x0400149A RID: 5274
		private readonly AbilityModifierUsingArgs modifiersUsingArgs = new AbilityModifierUsingArgs();

		// Token: 0x0400149B RID: 5275
		private IBuffsGenerator[] ownersStateGenerators;

		// Token: 0x0400149C RID: 5276
		private Dictionary<int, IBuff[]> activeOwnersStates;
	}
}
