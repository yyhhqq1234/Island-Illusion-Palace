using System;
using System.Collections.Generic;
using Game.Damage.Projectiles;
using Game.ObjectPool;
using UnityEngine;

namespace Unliving.ObjectPools
{
	// Token: 0x020001B1 RID: 433
	public sealed class ProjectileRenderersPool : UnityObjectPoolBase<ProjectileRendererComponent>
	{
		// Token: 0x06000C4B RID: 3147 RVA: 0x0002674C File Offset: 0x0002494C
		protected override IPoolableObjectAdapter InstantiateObject(UnityEngine.Object rendererPrototype)
		{
			ProjectileRendererComponent component = UnityEngine.Object.Instantiate<GameObject>((GameObject)rendererPrototype).GetComponent<ProjectileRendererComponent>();
			ProjectileRendererComponent projectileRendererComponent = component;
			projectileRendererComponent.name += "_pooled";
			return new ProjectileRenderersPool.PoolableRenderer(rendererPrototype, component);
		}

		// Token: 0x0200047E RID: 1150
		private sealed class PoolableRenderer : ParticlesEffectPoolableAdapter
		{
			// Token: 0x1700074F RID: 1871
			// (get) Token: 0x060023FE RID: 9214 RVA: 0x0006F552 File Offset: 0x0006D752
			public override object Object
			{
				get
				{
					return this.rendererComponent;
				}
			}

			// Token: 0x17000750 RID: 1872
			// (get) Token: 0x060023FF RID: 9215 RVA: 0x0006F55A File Offset: 0x0006D75A
			public override bool CanBePooled
			{
				get
				{
					return base.HasCompletedParticleEffects();
				}
			}

			// Token: 0x06002400 RID: 9216 RVA: 0x0006F564 File Offset: 0x0006D764
			private void SetActive(bool isActive)
			{
				if (this.projectileRenderers != null)
				{
					for (int i = 0; i < this.projectileRenderers.Count; i++)
					{
						this.projectileRenderers[i].enabled = isActive;
					}
				}
				base.SetParticlesActive(isActive);
			}

			// Token: 0x06002401 RID: 9217 RVA: 0x0006F5A8 File Offset: 0x0006D7A8
			public PoolableRenderer(UnityEngine.Object rendererPrototype, ProjectileRendererComponent projectileRenderer) : base(rendererPrototype, (projectileRenderer.ParticleSystem != null) ? projectileRenderer.gameObject : null, (projectileRenderer.ParticleSystem != null) ? projectileRenderer.ParticleSystem.gameObject : null)
			{
				this.rendererComponent = projectileRenderer;
				if (projectileRenderer.SpriteRenderer != null)
				{
					this.projectileRenderers = new List<SpriteRenderer>(4);
					projectileRenderer.GetComponentsInChildren<SpriteRenderer>(true, this.projectileRenderers);
					SpriteRenderer spriteRenderer = this.projectileRenderers[0];
					this.storedProjectileColor = spriteRenderer.color;
					this.storedProjectileSprite = spriteRenderer.sprite;
				}
			}

			// Token: 0x06002402 RID: 9218 RVA: 0x0006F642 File Offset: 0x0006D842
			public override bool Take(object args)
			{
				this.SetActive(true);
				this.rendererComponent.Activate();
				return true;
			}

			// Token: 0x06002403 RID: 9219 RVA: 0x0006F658 File Offset: 0x0006D858
			public override void ResetState()
			{
				if (this.CanBePooled)
				{
					if (this.projectileRenderers != null)
					{
						SpriteRenderer spriteRenderer = this.projectileRenderers[0];
						spriteRenderer.color = this.storedProjectileColor;
						spriteRenderer.sprite = this.storedProjectileSprite;
					}
					this.rendererComponent.gameObject.SetActive(false);
					return;
				}
				this.rendererComponent.Deactivate();
				this.SetActive(false);
			}

			// Token: 0x04001788 RID: 6024
			private readonly ProjectileRendererComponent rendererComponent;

			// Token: 0x04001789 RID: 6025
			private readonly List<SpriteRenderer> projectileRenderers;

			// Token: 0x0400178A RID: 6026
			private readonly Color storedProjectileColor;

			// Token: 0x0400178B RID: 6027
			private readonly Sprite storedProjectileSprite;
		}
	}
}
