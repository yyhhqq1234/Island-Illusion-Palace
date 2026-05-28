using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;

namespace Unliving.Pickables.VFX
{
	// Token: 0x020001A0 RID: 416
	public class PickableVFXController : GameBehaviourBase
	{
		// Token: 0x06000BE3 RID: 3043 RVA: 0x000259CD File Offset: 0x00023BCD
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.TryGetComponent<PickableObjectBase>(out this.pickableObject))
			{
				this.pickableObject.ObjectCollectionStarted += this.OnObjectCollectionStarted;
			}
		}

		// Token: 0x06000BE4 RID: 3044 RVA: 0x000259FB File Offset: 0x00023BFB
		private void OnObjectCollectionStarted(IPickableObjectCollector collector)
		{
			PickableVFXController.ActivateEffects(this.destroyEffects);
		}

		// Token: 0x06000BE5 RID: 3045 RVA: 0x00025A08 File Offset: 0x00023C08
		private static void ActivateEffects(PickableVFXController.VisualEffect[] effects)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Activate();
			}
		}

		// Token: 0x06000BE6 RID: 3046 RVA: 0x00025A30 File Offset: 0x00023C30
		private static void DestroyEffects(PickableVFXController.VisualEffect[] effects)
		{
			for (int i = 0; i < effects.Length; i++)
			{
				effects[i].Destroy();
			}
		}

		// Token: 0x06000BE7 RID: 3047 RVA: 0x00025A57 File Offset: 0x00023C57
		protected override void OnDestroy()
		{
			base.OnDestroy();
			PickableVFXController.DestroyEffects(this.fadingOutEffects);
			if (!this.pickableObject.IsNull())
			{
				this.pickableObject.ObjectCollectionStarted -= this.OnObjectCollectionStarted;
			}
		}

		// Token: 0x0400069B RID: 1691
		[Tooltip("Время проигрывания эффекта исчезновения")]
		public float fadeOutTimeout = -1f;

		// Token: 0x0400069C RID: 1692
		public PickableVFXController.VisualEffect[] fadingOutEffects;

		// Token: 0x0400069D RID: 1693
		public PickableVFXController.VisualEffect[] destroyEffects;

		// Token: 0x0400069E RID: 1694
		private PickableObjectBase pickableObject;

		// Token: 0x02000478 RID: 1144
		[Serializable]
		public struct VisualEffect
		{
			// Token: 0x060023EA RID: 9194 RVA: 0x0006F034 File Offset: 0x0006D234
			public void Activate()
			{
				if (this.particleSystemPrefab != null)
				{
					if (this.particleSystem == null)
					{
						this.particleSystem = UnityEngine.Object.Instantiate<GameObject>(this.particleSystemPrefab).GetComponentOrDestroy<ParticleSystem>();
						if (this.fxTransform.IsNull())
						{
							this.particleSystem.transform.SetParent(null);
						}
						else
						{
							this.particleSystem.transform.SetParent(this.fxTransform);
							this.particleSystem.transform.localPosition = default(Vector3);
						}
						this.particleSystem.transform.localScale = Vector3.one;
						this.particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
						this.mainParticlesModule = this.particleSystem.main;
						this.mainParticlesModule.loop = false;
					}
					int num = (this.maxParticles > this.minParticles) ? UnityEngine.Random.Range(this.minParticles, this.maxParticles) : this.minParticles;
					if (!this.particleSystem.isPlaying && num > 0 && (this.activationProbability <= 0f || UnityEngine.Random.value <= this.activationProbability))
					{
						this.particleSystem.Emit(num);
					}
				}
				if (this.animatorObject != null)
				{
					if (!this.animatorObject.IsPrefab())
					{
						this.animator = this.animatorObject.GetComponentOrDestroy<Animator>();
					}
					if (this.animator.IsNull())
					{
						this.animator = UnityEngine.Object.Instantiate<GameObject>(this.animatorObject).GetComponentOrDestroy<Animator>();
						this.animator.transform.SetParent(this.animatorTransform ?? null);
						this.animator.transform.localPosition = Vector3.zero;
						this.animator.transform.localScale = Vector3.one;
						this.animator.transform.localRotation = Quaternion.identity;
					}
					if (!string.IsNullOrEmpty(this.activationTrigger))
					{
						this.animator.SetTrigger(this.activationTrigger);
					}
				}
			}

			// Token: 0x060023EB RID: 9195 RVA: 0x0006F230 File Offset: 0x0006D430
			public void Destroy()
			{
				if (!GameApplication.IsGameStateChanging)
				{
					this.particleSystem.DestroyAfterEmission(true, false);
				}
				if (this.animatorObject.IsPrefab() && this.animator)
				{
					UnityEngine.Object.Destroy(this.animator.gameObject, this.animator.GetCurrentAnimatorStateInfo(0).length);
				}
			}

			// Token: 0x04001770 RID: 6000
			public Transform fxTransform;

			// Token: 0x04001771 RID: 6001
			public GameObject particleSystemPrefab;

			// Token: 0x04001772 RID: 6002
			public int minParticles;

			// Token: 0x04001773 RID: 6003
			public int maxParticles;

			// Token: 0x04001774 RID: 6004
			[Range(0f, 1f)]
			public float activationProbability;

			// Token: 0x04001775 RID: 6005
			public Transform animatorTransform;

			// Token: 0x04001776 RID: 6006
			public GameObject animatorObject;

			// Token: 0x04001777 RID: 6007
			public string activationTrigger;

			// Token: 0x04001778 RID: 6008
			private ParticleSystem particleSystem;

			// Token: 0x04001779 RID: 6009
			private ParticleSystem.MainModule mainParticlesModule;

			// Token: 0x0400177A RID: 6010
			private Animator animator;
		}
	}
}
