using System;
using System.Collections.Generic;
using Game.ObjectPool;
using UnityEngine;

namespace Unliving.ObjectPools
{
	// Token: 0x020001AF RID: 431
	internal abstract class ParticlesEffectPoolableAdapter : IPoolableObjectAdapter, IPoolableObject
	{
		// Token: 0x1700022A RID: 554
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x000264CE File Offset: 0x000246CE
		// (set) Token: 0x06000C37 RID: 3127 RVA: 0x000264D6 File Offset: 0x000246D6
		public object ObjectPrototype { get; private set; }

		// Token: 0x1700022B RID: 555
		// (get) Token: 0x06000C38 RID: 3128
		public abstract object Object { get; }

		// Token: 0x1700022C RID: 556
		// (get) Token: 0x06000C39 RID: 3129
		public abstract bool CanBePooled { get; }

		// Token: 0x06000C3A RID: 3130 RVA: 0x000264E0 File Offset: 0x000246E0
		protected void SetParticlesActive(bool isActive)
		{
			if (this.particleSystems != null)
			{
				for (int i = 0; i < this.particleSystems.Count; i++)
				{
					ParticleSystem particleSystem = this.particleSystems[i];
					ParticleSystem.MainModule main = particleSystem.main;
					if (isActive)
					{
						main.loop = ((1 << i & this.loopStateMask) != 0);
						particleSystem.Play(false);
					}
					else
					{
						main.loop = false;
						this.particleSystems[i].Stop(false);
					}
				}
			}
		}

		// Token: 0x06000C3B RID: 3131 RVA: 0x0002655C File Offset: 0x0002475C
		protected bool HasCompletedParticleEffects()
		{
			if (this.particleSystems != null)
			{
				for (int i = 0; i < this.particleSystems.Count; i++)
				{
					if (this.particleSystems[i].IsAlive(false))
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x06000C3C RID: 3132 RVA: 0x000265A0 File Offset: 0x000247A0
		public ParticlesEffectPoolableAdapter(UnityEngine.Object objectPrototype, GameObject particleSystemsRoot, GameObject mainParticleSystemObject = null)
		{
			this.ObjectPrototype = objectPrototype;
			this.ParticleSystemsRoot = particleSystemsRoot;
			if (particleSystemsRoot != null)
			{
				this.effectName = particleSystemsRoot.name;
				this.particleSystems = new List<ParticleSystem>(4);
				particleSystemsRoot.GetComponentsInChildren<ParticleSystem>(true, this.particleSystems);
				if (this.particleSystems.Count != 0)
				{
					if (mainParticleSystemObject == null)
					{
						mainParticleSystemObject = this.particleSystems[0].gameObject;
					}
					for (int i = 0; i < this.particleSystems.Count; i++)
					{
						ParticleSystem particleSystem = this.particleSystems[i];
						if (particleSystem.gameObject != mainParticleSystemObject)
						{
							particleSystem.transform.parent = mainParticleSystemObject.transform;
						}
						ParticleSystem.MainModule main = particleSystem.main;
						if (main.loop)
						{
							this.loopStateMask |= 1 << i;
						}
						main.playOnAwake = false;
						main.stopAction = ParticleSystemStopAction.None;
						particleSystem.Stop();
					}
				}
			}
		}

		// Token: 0x06000C3D RID: 3133
		public abstract bool Take(object args);

		// Token: 0x06000C3E RID: 3134
		public abstract void ResetState();

		// Token: 0x04000700 RID: 1792
		protected readonly GameObject ParticleSystemsRoot;

		// Token: 0x04000701 RID: 1793
		private readonly string effectName;

		// Token: 0x04000702 RID: 1794
		private readonly List<ParticleSystem> particleSystems;

		// Token: 0x04000703 RID: 1795
		private readonly int loopStateMask;
	}
}
