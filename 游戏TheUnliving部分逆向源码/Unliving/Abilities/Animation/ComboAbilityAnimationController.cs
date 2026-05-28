using System;
using Common.Animation;
using Game.Abilities;
using Game.Abilities.Animation;
using UnityEngine;
using Unliving.Abilities.ComboAbilities;

namespace Unliving.Abilities.Animation
{
	// Token: 0x020003E4 RID: 996
	[CreateAssetMenu(fileName = "ComboAbilityAnimationController", menuName = "Abilities/Controllers/Combo Ability Animation Controller")]
	public sealed class ComboAbilityAnimationController : AbilityAnimationControllerBase
	{
		// Token: 0x170006D4 RID: 1748
		// (get) Token: 0x060021AD RID: 8621 RVA: 0x00069330 File Offset: 0x00067530
		// (set) Token: 0x060021AE RID: 8622 RVA: 0x00069338 File Offset: 0x00067538
		public string AnimationStateHoldParameterName
		{
			get
			{
				return this._animationStateHoldParameterName;
			}
			set
			{
				this._animationStateHoldParameterName = value;
			}
		}

		// Token: 0x170006D5 RID: 1749
		// (get) Token: 0x060021AF RID: 8623 RVA: 0x00069341 File Offset: 0x00067541
		// (set) Token: 0x060021B0 RID: 8624 RVA: 0x00069349 File Offset: 0x00067549
		public bool KeepAnimationAfterLastAbility
		{
			get
			{
				return this._keepAnimationAfterLastAbility;
			}
			set
			{
				this._keepAnimationAfterLastAbility = value;
			}
		}

		// Token: 0x170006D6 RID: 1750
		// (get) Token: 0x060021B1 RID: 8625 RVA: 0x00069352 File Offset: 0x00067552
		// (set) Token: 0x060021B2 RID: 8626 RVA: 0x0006935A File Offset: 0x0006755A
		public string ProgressTriggerParameterName
		{
			get
			{
				return this._progressTriggerParameterName;
			}
			set
			{
				this._progressTriggerParameterName = value;
			}
		}

		// Token: 0x170006D7 RID: 1751
		// (get) Token: 0x060021B3 RID: 8627 RVA: 0x00069363 File Offset: 0x00067563
		// (set) Token: 0x060021B4 RID: 8628 RVA: 0x0006936B File Offset: 0x0006756B
		public float ProgressTriggerThreshold
		{
			get
			{
				return this._progressTriggerThreshold;
			}
			set
			{
				this._progressTriggerThreshold = value;
			}
		}

		// Token: 0x060021B5 RID: 8629 RVA: 0x00069374 File Offset: 0x00067574
		private BaseAbilityAnimationController.AnimationTrigger SelectAnimationTrigger(int abilityIndex)
		{
			if (this.randomizeAnimations)
			{
				return this._abilityAnimationTriggers[UnityEngine.Random.Range(0, this._abilityAnimationTriggers.Length)];
			}
			int num = this._abilityAnimationTriggers.Length;
			int num2;
			if (!this.isCycledAnimationTriggersList)
			{
				num2 = Mathf.Min(abilityIndex, num - 1);
			}
			else
			{
				int num3 = this.cycledAnimationTriggerIndex;
				this.cycledAnimationTriggerIndex = num3 + 1;
				num2 = num3 % num;
			}
			int num4 = num2;
			return this._abilityAnimationTriggers[num4];
		}

		// Token: 0x060021B6 RID: 8630 RVA: 0x000693DF File Offset: 0x000675DF
		private void SetAnimationStateHoldActive(bool isActive)
		{
			if (this.currentAnimationController == null || string.IsNullOrEmpty(this._animationStateHoldParameterName))
			{
				return;
			}
			this.currentAnimationController.TargetAnimator.SetBool(this._animationStateHoldParameterName, isActive);
		}

		// Token: 0x060021B7 RID: 8631 RVA: 0x00069414 File Offset: 0x00067614
		private void UpdateComboProgressTrigger(int comboAbilityIndex)
		{
			if (this.currentAnimationController == null || string.IsNullOrEmpty(this._progressTriggerParameterName))
			{
				return;
			}
			float num = (float)comboAbilityIndex / (float)this.currentComboAbility.AbilitiesSequence.Length;
			this.currentAnimationController.TargetAnimator.SetBool(this._progressTriggerParameterName, num > this._progressTriggerThreshold);
		}

		// Token: 0x060021B8 RID: 8632 RVA: 0x00069470 File Offset: 0x00067670
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			this.isLastChildAbility = false;
			ComboAbility comboAbility = ability as ComboAbility;
			if (comboAbility != null)
			{
				comboAbility.ChildAbilitySelected += this.OnNextComboAbilitySelected;
				comboAbility.ComboResetted += this.OnAbilityComboResetted;
				comboAbility.Completed += this.OnAbilityCompleted;
				this.currentComboAbility = comboAbility;
				return;
			}
			ability != null;
		}

		// Token: 0x060021B9 RID: 8633 RVA: 0x000694DC File Offset: 0x000676DC
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			if (this.currentComboAbility != null)
			{
				this.currentComboAbility.ChildAbilitySelected -= this.OnNextComboAbilitySelected;
				this.currentComboAbility.ComboResetted -= this.OnAbilityComboResetted;
				this.currentComboAbility.Completed -= this.OnAbilityCompleted;
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x060021BA RID: 8634 RVA: 0x00069544 File Offset: 0x00067744
		protected override void OnAnimationControllerChanged(CommonAnimationController newAnimationController)
		{
			if (this._abilityAnimationTriggers != null)
			{
				for (int i = 0; i < this._abilityAnimationTriggers.Length; i++)
				{
					this._abilityAnimationTriggers[i].Initialize(this.currentAbility, newAnimationController);
				}
			}
		}

		// Token: 0x060021BB RID: 8635 RVA: 0x00069584 File Offset: 0x00067784
		private void OnNextComboAbilitySelected(ComboAbility.ChildAbility childAbility)
		{
			this.SetAnimationStateHoldActive(false);
			int index = childAbility.Index;
			this.isLastChildAbility = (index == this.currentComboAbility.AbilitiesSequence.Length - 1);
			if (this._abilityAnimationTriggers != null && this._abilityAnimationTriggers.Length != 0)
			{
				BaseAbilityAnimationController.AnimationTrigger animationTrigger = this.SelectAnimationTrigger(index);
				animationTrigger.Activate(false, false);
				base.SyncAbilityUsingTime(animationTrigger.targetAnimationStateTag);
			}
			this.UpdateComboProgressTrigger(index);
		}

		// Token: 0x060021BC RID: 8636 RVA: 0x000695EC File Offset: 0x000677EC
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			if (!this.isLastChildAbility || this._keepAnimationAfterLastAbility)
			{
				this.SetAnimationStateHoldActive(true);
			}
		}

		// Token: 0x060021BD RID: 8637 RVA: 0x00069605 File Offset: 0x00067805
		private void OnAbilityComboResetted(ComboAbility comboAbility)
		{
			this.SetAnimationStateHoldActive(false);
			this.isLastChildAbility = false;
			this.cycledAnimationTriggerIndex = 0;
			this.UpdateComboProgressTrigger(0);
		}

		// Token: 0x040014FD RID: 5373
		[SerializeField]
		private BaseAbilityAnimationController.AnimationTrigger[] _abilityAnimationTriggers;

		// Token: 0x040014FE RID: 5374
		public bool isCycledAnimationTriggersList;

		// Token: 0x040014FF RID: 5375
		public bool randomizeAnimations;

		// Token: 0x04001500 RID: 5376
		[SerializeField]
		private string _animationStateHoldParameterName;

		// Token: 0x04001501 RID: 5377
		[SerializeField]
		private bool _keepAnimationAfterLastAbility;

		// Token: 0x04001502 RID: 5378
		[SerializeField]
		[Range(0f, 1f)]
		private float _progressTriggerThreshold;

		// Token: 0x04001503 RID: 5379
		[SerializeField]
		private string _progressTriggerParameterName;

		// Token: 0x04001504 RID: 5380
		private ComboAbility currentComboAbility;

		// Token: 0x04001505 RID: 5381
		private int cycledAnimationTriggerIndex;

		// Token: 0x04001506 RID: 5382
		private bool isLastChildAbility;
	}
}
