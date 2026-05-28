using System;
using Game.ObjectPool;
using UnityEngine;
using Unliving.AbilityResources;

namespace Unliving.ObjectPools
{
	// Token: 0x020001AA RID: 426
	public sealed class CollectableAbilityResourcesPool : UnityObjectPoolBase<CollectableAbilityResource>
	{
		// Token: 0x06000C2C RID: 3116 RVA: 0x000262A0 File Offset: 0x000244A0
		protected override IPoolableObjectAdapter InstantiateObject(UnityEngine.Object resourcePrototype)
		{
			CollectableAbilityResource component = UnityEngine.Object.Instantiate<GameObject>((GameObject)resourcePrototype).GetComponent<CollectableAbilityResource>();
			CollectableAbilityResource collectableAbilityResource = component;
			collectableAbilityResource.name += "_pooled";
			return new CollectableAbilityResourcesPool.PoolableResource(resourcePrototype, component);
		}

		// Token: 0x0200047C RID: 1148
		private sealed class PoolableResource : ParticlesEffectPoolableAdapter
		{
			// Token: 0x1700074B RID: 1867
			// (get) Token: 0x060023F4 RID: 9204 RVA: 0x0006F33A File Offset: 0x0006D53A
			public override object Object
			{
				get
				{
					return this.resource;
				}
			}

			// Token: 0x1700074C RID: 1868
			// (get) Token: 0x060023F5 RID: 9205 RVA: 0x0006F342 File Offset: 0x0006D542
			public override bool CanBePooled
			{
				get
				{
					return this.resource.IsDestroyed && base.HasCompletedParticleEffects();
				}
			}

			// Token: 0x060023F6 RID: 9206 RVA: 0x0006F359 File Offset: 0x0006D559
			public PoolableResource(UnityEngine.Object resourcePrototype, CollectableAbilityResource resource) : base(resourcePrototype, resource.gameObject, null)
			{
				this.resource = resource;
			}

			// Token: 0x060023F7 RID: 9207 RVA: 0x0006F370 File Offset: 0x0006D570
			public override bool Take(object args)
			{
				if (this.resource != null)
				{
					this.resource.enabled = true;
					this.resource.gameObject.SetActive(true);
					return true;
				}
				return false;
			}

			// Token: 0x060023F8 RID: 9208 RVA: 0x0006F3A0 File Offset: 0x0006D5A0
			public override void ResetState()
			{
				if (this.CanBePooled)
				{
					this.resource.ResetState();
					this.resource.transform.parent = null;
					this.resource.enabled = false;
					this.resource.gameObject.SetActive(false);
					return;
				}
				base.SetParticlesActive(false);
			}

			// Token: 0x04001783 RID: 6019
			private readonly CollectableAbilityResource resource;
		}
	}
}
