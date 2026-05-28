using System;
using System.Collections.Generic;
using Common;
using Common.Editor;
using Common.PivotGroup;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Utility;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000379 RID: 889
	[CreateAssetMenu(fileName = "AbilityUsingDirectionPreviewController", menuName = "Abilities/Controllers/Using Direction Preview Controller")]
	public sealed class AbilityUsingDirectionPreviewController : AbilityExtensionAssetBase
	{
		// Token: 0x1700060D RID: 1549
		// (get) Token: 0x06001D3F RID: 7487 RVA: 0x0005C8F5 File Offset: 0x0005AAF5
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D40 RID: 7488 RVA: 0x0005C8F8 File Offset: 0x0005AAF8
		private void SetDirectionPreviewActive(BaseAbility ability, bool isActive)
		{
			AbilityUsingDirectionPreviewController.DirectionPreview directionPreview;
			if (this.directionPreviews.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out directionPreview))
			{
				directionPreview.SetActive(isActive);
			}
		}

		// Token: 0x06001D41 RID: 7489 RVA: 0x0005C924 File Offset: 0x0005AB24
		private Vector2 GetAbilityUsingDirection(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			Vector2 result = usingArgs.targetPosition - ability.OwnerPosition;
			result.Normalize();
			return result;
		}

		// Token: 0x06001D42 RID: 7490 RVA: 0x0005C950 File Offset: 0x0005AB50
		private void PassUsingDirectionToAnimator(Animator animator, Vector2 usingDirection)
		{
			animator.enabled = true;
			animator.SetFloat(this.animatorDirectionXParamID.Value, usingDirection.x);
			animator.SetFloat(this.animatorDirectionYParamID.Value, usingDirection.y);
		}

		// Token: 0x06001D43 RID: 7491 RVA: 0x0005C988 File Offset: 0x0005AB88
		private void UpdateDirectionPreview(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			AbilityUsingDirectionPreviewController.DirectionPreview directionPreview;
			if (this.directionPreviews.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out directionPreview))
			{
				Vector3 worldPosition = ((BaseGameMob)ability.OwnerBehaviour).TaggedPivotsGroup.GetPivot(directionPreview.attachmentPointID).WorldPosition;
				Vector2 abilityUsingDirection = this.GetAbilityUsingDirection(ability, usingArgs);
				Transform transform = directionPreview.drawerObject.transform;
				transform.position = worldPosition + this.attachmentPointOffset;
				if (directionPreview.spriteSelector != null)
				{
					directionPreview.spriteSelector.SetSprite(abilityUsingDirection);
				}
				else if (directionPreview.animator != null)
				{
					this.PassUsingDirectionToAnimator(directionPreview.animator, abilityUsingDirection);
				}
				else
				{
					transform.rotation = QuaternionExtensions.Get2DRotation(abilityUsingDirection, this.angleOffset);
				}
				directionPreview.TryUpdatePrepIndicator(ability);
			}
		}

		// Token: 0x06001D44 RID: 7492 RVA: 0x0005CA48 File Offset: 0x0005AC48
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			if (this.directionPreviewPrefab != null)
			{
				if (this.animationSpeedParamID == null)
				{
					this.animatorDirectionXParamID = new int?(Animator.StringToHash(this.animatorDirectionXParamName));
					this.animatorDirectionYParamID = new int?(Animator.StringToHash(this.animatorDirectionYParamName));
					this.animationSpeedParamID = new int?(Animator.StringToHash(this.animationSpeedParamName));
				}
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.directionPreviewPrefab);
				gameObject.name = string.Format("{0}_directionDrawer", ability);
				AbilityUsingDirectionPreviewController.DirectionPreview value = new AbilityUsingDirectionPreviewController.DirectionPreview
				{
					drawerObject = gameObject,
					attachmentPointID = TaggedPivotGroup.TagToHash(this.previewAttachmentPointTag),
					prepIndicator = gameObject.GetComponent<IAlterableProgressAction>()
				};
				AngleDependentSpriteSelector spriteSelector;
				if (!this.hasIdleAnimation && gameObject.TryGetComponent<AngleDependentSpriteSelector>(out spriteSelector))
				{
					value.spriteSelector = spriteSelector;
				}
				Animator animator;
				if (this.animationSpeedParamID != null && gameObject.TryGetComponent<Animator>(out animator))
				{
					value.animator = animator;
				}
				value.SetActive(false);
				this.directionPreviews.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), value);
			}
			ability.Activated += this.OnAbilityActivated;
			if (ability.HasPrepTime())
			{
				ability.Activating += this.OnActivatingAbility;
			}
			if (ability.HasUsingDuration())
			{
				ability.Used += this.OnAbilityUsed;
			}
			ability.Completed += this.OnAbilityCompleted;
		}

		// Token: 0x06001D45 RID: 7493 RVA: 0x0005CBB8 File Offset: 0x0005ADB8
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ability.Activated -= this.OnAbilityActivated;
			ability.Activating -= this.OnActivatingAbility;
			ability.Used -= this.OnAbilityUsed;
			ability.Completed -= this.OnAbilityCompleted;
			int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
			AbilityUsingDirectionPreviewController.DirectionPreview directionPreview;
			if (this.directionPreviews.TryGetValue(abilityInstanceID, out directionPreview))
			{
				directionPreview.Destroy();
				this.directionPreviews.Remove(abilityInstanceID);
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D46 RID: 7494 RVA: 0x0005CC40 File Offset: 0x0005AE40
		private void OnActivatingAbility(IAbility ability, object usingArgs)
		{
			BaseAbility ability2 = (BaseAbility)ability;
			if (ability.PrepProgress == 0f)
			{
				this.SetDirectionPreviewActive(ability2, true);
			}
			this.UpdateDirectionPreview(ability2, (BaseAbility.UsingArgs)usingArgs);
		}

		// Token: 0x06001D47 RID: 7495 RVA: 0x0005CC78 File Offset: 0x0005AE78
		private void OnAbilityActivated(IAbility ability, object usingArgs)
		{
			if (ability.UsingDelay > 0f)
			{
				bool flag = !string.IsNullOrEmpty(this.animationTriggerParamName);
				AbilityUsingDirectionPreviewController.DirectionPreview directionPreview;
				if ((flag || !this.hasIdleAnimation) && this.directionPreviews.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out directionPreview))
				{
					Animator animator = directionPreview.animator;
					if (animator != null)
					{
						float num = this.referenceAnimation.length * animator.speed * animator.GetFloat(this.animationSpeedParamID.Value);
						Vector2 abilityUsingDirection = this.GetAbilityUsingDirection((BaseAbility)ability, (BaseAbility.UsingArgs)usingArgs);
						this.PassUsingDirectionToAnimator(animator, abilityUsingDirection);
						if (flag)
						{
							animator.SetTrigger(this.animationTriggerParamName);
						}
						animator.SetFloat(this.animationSpeedParamID.Value, num / ability.UsingDelay);
					}
				}
			}
		}

		// Token: 0x06001D48 RID: 7496 RVA: 0x0005CD41 File Offset: 0x0005AF41
		private void OnAbilityUsed(IAbility ability, object usingArgs)
		{
			this.UpdateDirectionPreview((BaseAbility)ability, (BaseAbility.UsingArgs)usingArgs);
		}

		// Token: 0x06001D49 RID: 7497 RVA: 0x0005CD55 File Offset: 0x0005AF55
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.SetDirectionPreviewActive((BaseAbility)ability, false);
		}

		// Token: 0x04001086 RID: 4230
		public GameObject directionPreviewPrefab;

		// Token: 0x04001087 RID: 4231
		public float angleOffset;

		// Token: 0x04001088 RID: 4232
		[Tag]
		public string previewAttachmentPointTag;

		// Token: 0x04001089 RID: 4233
		public Vector3 attachmentPointOffset;

		// Token: 0x0400108A RID: 4234
		[Space]
		public bool hasIdleAnimation;

		// Token: 0x0400108B RID: 4235
		public AnimationClip referenceAnimation;

		// Token: 0x0400108C RID: 4236
		public string animatorDirectionXParamName;

		// Token: 0x0400108D RID: 4237
		public string animatorDirectionYParamName;

		// Token: 0x0400108E RID: 4238
		public string animationSpeedParamName;

		// Token: 0x0400108F RID: 4239
		public string animationTriggerParamName;

		// Token: 0x04001090 RID: 4240
		private readonly Dictionary<int, AbilityUsingDirectionPreviewController.DirectionPreview> directionPreviews = new Dictionary<int, AbilityUsingDirectionPreviewController.DirectionPreview>(16);

		// Token: 0x04001091 RID: 4241
		public int? animatorDirectionXParamID;

		// Token: 0x04001092 RID: 4242
		public int? animatorDirectionYParamID;

		// Token: 0x04001093 RID: 4243
		public int? animationSpeedParamID;

		// Token: 0x02000568 RID: 1384
		private struct DirectionPreview
		{
			// Token: 0x06002710 RID: 10000 RVA: 0x000799A0 File Offset: 0x00077BA0
			public void SetActive(bool isActive)
			{
				if (this.animator != null)
				{
					this.animator.enabled = false;
				}
				this.drawerObject.SetActive(isActive);
			}

			// Token: 0x06002711 RID: 10001 RVA: 0x000799C8 File Offset: 0x00077BC8
			public void TryUpdatePrepIndicator(BaseAbility ability)
			{
				if (this.prepIndicator != null)
				{
					this.prepIndicator.CurrentProgress = ability.PrepProgress;
				}
			}

			// Token: 0x06002712 RID: 10002 RVA: 0x000799E3 File Offset: 0x00077BE3
			public void Destroy()
			{
				UnityEngine.Object.Destroy(this.drawerObject);
			}

			// Token: 0x04001C1B RID: 7195
			public GameObject drawerObject;

			// Token: 0x04001C1C RID: 7196
			public int attachmentPointID;

			// Token: 0x04001C1D RID: 7197
			public AngleDependentSpriteSelector spriteSelector;

			// Token: 0x04001C1E RID: 7198
			public Animator animator;

			// Token: 0x04001C1F RID: 7199
			public IAlterableProgressAction prepIndicator;
		}
	}
}
