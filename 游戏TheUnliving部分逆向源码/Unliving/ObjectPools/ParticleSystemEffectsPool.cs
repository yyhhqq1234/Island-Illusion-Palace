using System;
using Common.UnityExtensions;
using Game.ObjectPool;
using UnityEngine;

namespace Unliving.ObjectPools
{
	// Token: 0x020001AD RID: 429
	public sealed class ParticleSystemEffectsPool : UnityObjectPoolBase<ParticleSystem>
	{
		// Token: 0x06000C33 RID: 3123 RVA: 0x00026468 File Offset: 0x00024668
		protected override IPoolableObjectAdapter InstantiateObject(UnityEngine.Object effectPrototype)
		{
			ParticleSystem component = UnityEngine.Object.Instantiate<GameObject>((GameObject)effectPrototype).GetComponent<ParticleSystem>();
			ParticleSystem particleSystem = component;
			particleSystem.name += "_pooled";
			PooledParticleSystemReturningComponent orAddComponent = component.gameObject.GetOrAddComponent<PooledParticleSystemReturningComponent>();
			orAddComponent.Pool = this;
			orAddComponent.ParticleSystem = component;
			return new ParticleSystemEffectsPool.PoolableEffect(effectPrototype, component, orAddComponent);
		}

		// Token: 0x0200047D RID: 1149
		private sealed class PoolableEffect : ParticlesEffectPoolableAdapter
		{
			// Token: 0x1700074D RID: 1869
			// (get) Token: 0x060023F9 RID: 9209 RVA: 0x0006F3F6 File Offset: 0x0006D5F6
			public override object Object
			{
				get
				{
					return this.mainParticleSystem;
				}
			}

			// Token: 0x1700074E RID: 1870
			// (get) Token: 0x060023FA RID: 9210 RVA: 0x0006F3FE File Offset: 0x0006D5FE
			public override bool CanBePooled
			{
				get
				{
					return base.HasCompletedParticleEffects();
				}
			}

			// Token: 0x060023FB RID: 9211 RVA: 0x0006F408 File Offset: 0x0006D608
			public PoolableEffect(UnityEngine.Object effectPrototype, ParticleSystem mainParticleSystem, PooledParticleSystemReturningComponent effectReturner) : base(effectPrototype, mainParticleSystem.gameObject, null)
			{
				this.mainParticleSystem = mainParticleSystem;
				this.effectReturner = effectReturner;
				this.storedScale = mainParticleSystem.transform.localScale;
				this.storedShape = mainParticleSystem.shape.shapeType;
			}

			// Token: 0x060023FC RID: 9212 RVA: 0x0006F458 File Offset: 0x0006D658
			public override bool Take(object args)
			{
				UnityObjectPoolArgs unityObjectPoolArgs = args as UnityObjectPoolArgs;
				if (unityObjectPoolArgs != null)
				{
					Vector3? position = unityObjectPoolArgs.position;
					if (position != null)
					{
						this.mainParticleSystem.transform.position = position.Value;
					}
					ParticleSystemEffectsPoolArgs particleSystemEffectsPoolArgs = args as ParticleSystemEffectsPoolArgs;
					if (particleSystemEffectsPoolArgs != null)
					{
						this.effectReturner.IsSuppressed = particleSystemEffectsPoolArgs.effectWillBeReturnedManually;
					}
				}
				this.mainParticleSystem.gameObject.SetActive(true);
				base.SetParticlesActive(true);
				this.effectReturner.PrepareParticleSystem();
				return true;
			}

			// Token: 0x060023FD RID: 9213 RVA: 0x0006F4D8 File Offset: 0x0006D6D8
			public override void ResetState()
			{
				if (this.CanBePooled)
				{
					this.effectReturner.IsSuppressed = false;
					this.mainParticleSystem.transform.parent = null;
					this.mainParticleSystem.transform.localScale = this.storedScale;
					this.mainParticleSystem.shape.shapeType = this.storedShape;
					this.mainParticleSystem.gameObject.SetActive(false);
					return;
				}
				base.SetParticlesActive(false);
			}

			// Token: 0x04001784 RID: 6020
			private readonly ParticleSystem mainParticleSystem;

			// Token: 0x04001785 RID: 6021
			private readonly PooledParticleSystemReturningComponent effectReturner;

			// Token: 0x04001786 RID: 6022
			private readonly Vector3 storedScale;

			// Token: 0x04001787 RID: 6023
			private readonly ParticleSystemShapeType storedShape;
		}
	}
}
