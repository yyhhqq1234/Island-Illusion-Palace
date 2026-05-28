using System;
using Game.ObjectPool;
using UnityEngine;

namespace Unliving.ObjectPools
{
	// Token: 0x020001B0 RID: 432
	internal sealed class PooledParticleSystemReturningComponent : MonoBehaviour
	{
		// Token: 0x1700022D RID: 557
		// (get) Token: 0x06000C3F RID: 3135 RVA: 0x00026699 File Offset: 0x00024899
		// (set) Token: 0x06000C40 RID: 3136 RVA: 0x000266A1 File Offset: 0x000248A1
		public IUnityObjectPool<ParticleSystem> Pool { get; set; }

		// Token: 0x1700022E RID: 558
		// (get) Token: 0x06000C41 RID: 3137 RVA: 0x000266AA File Offset: 0x000248AA
		// (set) Token: 0x06000C42 RID: 3138 RVA: 0x000266B2 File Offset: 0x000248B2
		public ParticleSystem ParticleSystem { get; set; }

		// Token: 0x1700022F RID: 559
		// (get) Token: 0x06000C43 RID: 3139 RVA: 0x000266BB File Offset: 0x000248BB
		// (set) Token: 0x06000C44 RID: 3140 RVA: 0x000266C3 File Offset: 0x000248C3
		public bool IsSuppressed { get; set; }

		// Token: 0x06000C45 RID: 3141 RVA: 0x000266CC File Offset: 0x000248CC
		private void ReturnToPool()
		{
			this.Pool.ReturnObject(this.ParticleSystem);
		}

		// Token: 0x06000C46 RID: 3142 RVA: 0x000266E0 File Offset: 0x000248E0
		public void PrepareParticleSystem()
		{
			if (this.IsSuppressed || this.ParticleSystem == null)
			{
				return;
			}
			ParticleSystem.MainModule main = this.ParticleSystem.main;
			main.loop = false;
			main.stopAction = ParticleSystemStopAction.Callback;
		}

		// Token: 0x06000C47 RID: 3143 RVA: 0x00026720 File Offset: 0x00024920
		private void Start()
		{
			this.PrepareParticleSystem();
		}

		// Token: 0x06000C48 RID: 3144 RVA: 0x00026728 File Offset: 0x00024928
		private void OnEnable()
		{
			this.PrepareParticleSystem();
		}

		// Token: 0x06000C49 RID: 3145 RVA: 0x00026730 File Offset: 0x00024930
		private void OnParticleSystemStopped()
		{
			if (this.IsSuppressed)
			{
				return;
			}
			this.ReturnToPool();
		}
	}
}
