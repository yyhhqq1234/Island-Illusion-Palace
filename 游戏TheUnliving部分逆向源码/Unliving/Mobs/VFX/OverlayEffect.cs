using System;
using System.Collections;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Mobs.VFX
{
	// Token: 0x02000205 RID: 517
	[Serializable]
	public sealed class OverlayEffect
	{
		// Token: 0x06001185 RID: 4485 RVA: 0x00036FBA File Offset: 0x000351BA
		private void UpdateEffectAmount(float newAmount)
		{
			this.mobSprite.GetPropertyBlock(this.mobMaterialProperties);
			this.mobMaterialProperties.SetFloat(OverlayEffect.OverlayEffectPropertyID, newAmount);
			this.mobSprite.SetPropertyBlock(this.mobMaterialProperties);
		}

		// Token: 0x06001186 RID: 4486 RVA: 0x00036FEF File Offset: 0x000351EF
		private void ResetEffect()
		{
			if (this.currentEffectCoroutine == null)
			{
				return;
			}
			this.mob.StopCoroutine(this.currentEffectCoroutine);
			this.currentEffectCoroutine = null;
		}

		// Token: 0x06001187 RID: 4487 RVA: 0x00037012 File Offset: 0x00035212
		private IEnumerator EffectRoutine(float overlayAmount, float time)
		{
			WaitForSeconds updateAwaiter = new WaitForSeconds(0.03f);
			float currentOverlayAmount = overlayAmount;
			float delta = 0.03f * overlayAmount / Mathf.Max(0.1f, time);
			this.UpdateEffectAmount(overlayAmount);
			while (currentOverlayAmount > 0f)
			{
				yield return updateAwaiter;
				currentOverlayAmount -= delta;
				if (currentOverlayAmount < 0f)
				{
					currentOverlayAmount = 0f;
				}
				this.UpdateEffectAmount(currentOverlayAmount);
			}
			this.currentEffectCoroutine = null;
			yield break;
		}

		// Token: 0x06001188 RID: 4488 RVA: 0x00037030 File Offset: 0x00035230
		public void Initialize(BaseGameMob mob, SpriteRenderer sprite)
		{
			this.mobSprite = sprite;
			this.mob = mob;
			if (sprite.IsNull() || mob.IsNull())
			{
				return;
			}
			if (!this.mobSprite.sharedMaterial.HasProperty(OverlayEffect.OverlayEffectPropertyID))
			{
				return;
			}
			this.mobMaterialProperties = new MaterialPropertyBlock();
			mob.HitPointsController.HitPointsChanged += this.OnHealthChanged;
		}

		// Token: 0x06001189 RID: 4489 RVA: 0x00037098 File Offset: 0x00035298
		private void OnHealthChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (this.mob.IsAlive())
			{
				if (this.currentEffectCoroutine == null)
				{
					HitPointsController.HPChangingArgs hpchangingArgs = args as HitPointsController.HPChangingArgs;
					if (hpchangingArgs == null || !hpchangingArgs.disableTargetReaction)
					{
						float num = this.MobDamageOverlayAmount;
						float num2 = this.MobDamageOverlayTime;
						if (sender is PlayerBehaviour)
						{
							num = this.PlayerDamageOverlayAmount;
							num2 = this.PlayerDamageOverlayTime;
						}
						if (num > 0f && num2 > 0f)
						{
							this.currentEffectCoroutine = this.mob.StartCoroutine(this.EffectRoutine(num, num2));
							return;
						}
						return;
					}
				}
				return;
			}
			this.ResetEffect();
			this.UpdateEffectAmount(0f);
			this.mob.HitPointsController.HitPointsChanged -= this.OnHealthChanged;
		}

		// Token: 0x040009F6 RID: 2550
		private const string EffectPropertyName = "_OverlayStrength";

		// Token: 0x040009F7 RID: 2551
		private static readonly int OverlayEffectPropertyID = Shader.PropertyToID("_OverlayStrength");

		// Token: 0x040009F8 RID: 2552
		[Range(0f, 3f)]
		public float PlayerDamageOverlayAmount = 1f;

		// Token: 0x040009F9 RID: 2553
		public float PlayerDamageOverlayTime = 0.5f;

		// Token: 0x040009FA RID: 2554
		[Range(0f, 3f)]
		public float MobDamageOverlayAmount = 1f;

		// Token: 0x040009FB RID: 2555
		public float MobDamageOverlayTime = 0.2f;

		// Token: 0x040009FC RID: 2556
		private BaseGameMob mob;

		// Token: 0x040009FD RID: 2557
		private SpriteRenderer mobSprite;

		// Token: 0x040009FE RID: 2558
		private MaterialPropertyBlock mobMaterialProperties;

		// Token: 0x040009FF RID: 2559
		private Coroutine currentEffectCoroutine;
	}
}
