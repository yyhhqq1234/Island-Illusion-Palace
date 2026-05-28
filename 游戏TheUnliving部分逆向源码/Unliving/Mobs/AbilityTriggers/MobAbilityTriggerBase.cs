using System;
using System.Collections.Generic;
using Game.Abilities;
using UnityEngine;

namespace Unliving.Mobs.AbilityTriggers
{
	// Token: 0x02000232 RID: 562
	public abstract class MobAbilityTriggerBase : AbilityExtensionAssetBase, IMobAbilityTrigger
	{
		// Token: 0x0600133B RID: 4923 RVA: 0x0003C921 File Offset: 0x0003AB21
		private static int GetAbilityID(BaseAbility ability)
		{
			return ability.GetInstanceID();
		}

		// Token: 0x1700041B RID: 1051
		// (get) Token: 0x0600133C RID: 4924 RVA: 0x0003C929 File Offset: 0x0003AB29
		public sealed override bool IsSharedExtension
		{
			get
			{
				return !this.InstantiateForEveryAbility;
			}
		}

		// Token: 0x1700041C RID: 1052
		// (get) Token: 0x0600133D RID: 4925
		public abstract bool RequiresTarget { get; }

		// Token: 0x1700041D RID: 1053
		// (get) Token: 0x0600133E RID: 4926
		public abstract float ActivationRange { get; }

		// Token: 0x1700041E RID: 1054
		// (get) Token: 0x0600133F RID: 4927
		protected abstract bool InstantiateForEveryAbility { get; }

		// Token: 0x06001340 RID: 4928 RVA: 0x0003C934 File Offset: 0x0003AB34
		private void HandleStoredTriggerState(BaseAbility ability, bool register)
		{
			if (register)
			{
				MobAbilityTriggerBase.TriggerState triggerState = this.CreateTriggerState(ability);
				if (triggerState != null)
				{
					this.storedTriggerStates.Add(MobAbilityTriggerBase.GetAbilityID(ability), triggerState);
					return;
				}
			}
			else
			{
				this.storedTriggerStates.Remove(MobAbilityTriggerBase.GetAbilityID(ability));
			}
		}

		// Token: 0x06001341 RID: 4929 RVA: 0x0003C974 File Offset: 0x0003AB74
		protected BaseGameMob GetAbilityOwningMob(BaseAbility ability)
		{
			return ability.Owner as BaseGameMob;
		}

		// Token: 0x06001342 RID: 4930 RVA: 0x0003C981 File Offset: 0x0003AB81
		protected virtual MobAbilityTriggerBase.TriggerState CreateTriggerState(BaseAbility ability)
		{
			return null;
		}

		// Token: 0x06001343 RID: 4931 RVA: 0x0003C984 File Offset: 0x0003AB84
		protected virtual void ResetTrigger(BaseAbility ability)
		{
			this.ResetStoredTriggerState(ability);
		}

		// Token: 0x06001344 RID: 4932 RVA: 0x0003C990 File Offset: 0x0003AB90
		protected MobAbilityTriggerBase.TriggerState GetStoredTriggerState(BaseAbility ability)
		{
			MobAbilityTriggerBase.TriggerState result;
			this.storedTriggerStates.TryGetValue(MobAbilityTriggerBase.GetAbilityID(ability), out result);
			return result;
		}

		// Token: 0x06001345 RID: 4933 RVA: 0x0003C9B2 File Offset: 0x0003ABB2
		protected void ResetStoredTriggerState(BaseAbility ability)
		{
			MobAbilityTriggerBase.TriggerState storedTriggerState = this.GetStoredTriggerState(ability);
			if (storedTriggerState == null)
			{
				return;
			}
			storedTriggerState.Reset();
		}

		// Token: 0x06001346 RID: 4934 RVA: 0x0003C9C8 File Offset: 0x0003ABC8
		protected bool TryTriggerAutoUseAbility(BaseAbility ability, object args, bool checkStoredState = false)
		{
			if (checkStoredState)
			{
				MobAbilityTriggerBase.TriggerState storedTriggerState = this.GetStoredTriggerState(ability);
				if (storedTriggerState != null && !storedTriggerState.isConditionReached)
				{
					return false;
				}
			}
			return ability.TryActivateAsAutoUseAbility(args);
		}

		// Token: 0x06001347 RID: 4935
		public abstract bool IsConditionReached(BaseAbility ability, BaseAbility.UsingArgs abilityUsingArgs);

		// Token: 0x06001348 RID: 4936 RVA: 0x0003C9F4 File Offset: 0x0003ABF4
		public bool IsConditionReached(BaseAbility ability)
		{
			return this.IsConditionReached(ability, null);
		}

		// Token: 0x06001349 RID: 4937 RVA: 0x0003C9FE File Offset: 0x0003ABFE
		protected virtual void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.ResetTrigger((BaseAbility)ability);
		}

		// Token: 0x0600134A RID: 4938 RVA: 0x0003CA0C File Offset: 0x0003AC0C
		public override void OnAddedToAbility(BaseAbility ability)
		{
			ability.AddPreActivationCondition(new BaseAbility.ActivationCondition(this.IsConditionReached));
			ability.Completed += this.OnAbilityCompleted;
			this.HandleStoredTriggerState(ability, true);
			base.OnAddedToAbility(ability);
		}

		// Token: 0x0600134B RID: 4939 RVA: 0x0003CA43 File Offset: 0x0003AC43
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.RemovePreActivationCondition(new BaseAbility.ActivationCondition(this.IsConditionReached));
			ability.Completed -= this.OnAbilityCompleted;
			this.HandleStoredTriggerState(ability, false);
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x04000B34 RID: 2868
		[Tooltip("Если активно, то попытки активировать триггер будут игнорироваться пока абилити перезаряжается.")]
		[Obsolete]
		[HideInInspector]
		public bool activateOnlyForReloadedAbility = true;

		// Token: 0x04000B35 RID: 2869
		[Tooltip("Если активно, то срабатывание триггера будет проверяться во время каста абилити. В крайнем случае прогресс каста дойдет до 100% и будет ждать триггер.")]
		[Obsolete]
		[HideInInspector]
		public bool checkConditionAfterAbilityPreparation;

		// Token: 0x04000B36 RID: 2870
		[Tooltip("Будет ли состояние триггера сброшено после использования абилити.")]
		[Obsolete]
		[HideInInspector]
		public bool resetAfterAbilityUsing = true;

		// Token: 0x04000B37 RID: 2871
		[Obsolete]
		[HideInInspector]
		public bool tryActivateImmediately;

		// Token: 0x04000B38 RID: 2872
		private readonly Dictionary<int, MobAbilityTriggerBase.TriggerState> storedTriggerStates = new Dictionary<int, MobAbilityTriggerBase.TriggerState>(16);

		// Token: 0x020004CA RID: 1226
		protected class TriggerState
		{
			// Token: 0x06002545 RID: 9541 RVA: 0x00073979 File Offset: 0x00071B79
			public virtual void Reset()
			{
				this.isConditionReached = false;
			}

			// Token: 0x040019C1 RID: 6593
			public bool isConditionReached;
		}
	}
}
