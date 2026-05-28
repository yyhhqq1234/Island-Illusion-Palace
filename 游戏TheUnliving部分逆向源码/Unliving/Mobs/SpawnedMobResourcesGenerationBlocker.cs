using System;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001CB RID: 459
	public sealed class SpawnedMobResourcesGenerationBlocker : MonoBehaviour
	{
		// Token: 0x06000E71 RID: 3697 RVA: 0x0002DC0C File Offset: 0x0002BE0C
		private void OnMobSpawned(BaseGameMob mob)
		{
			if (mob.ResourcesGenerator != null)
			{
				mob.ResourcesGenerator.IsActive = false;
			}
		}

		// Token: 0x06000E72 RID: 3698 RVA: 0x0002DC22 File Offset: 0x0002BE22
		private void Awake()
		{
			if (base.TryGetComponent<MobBehaviourSpawner>(out this.spawner))
			{
				this.spawner.GroupMobSpawned += this.OnMobSpawned;
			}
		}

		// Token: 0x06000E73 RID: 3699 RVA: 0x0002DC49 File Offset: 0x0002BE49
		private void OnDestroy()
		{
			if (this.spawner != null)
			{
				this.spawner.GroupMobSpawned -= this.OnMobSpawned;
			}
		}

		// Token: 0x04000888 RID: 2184
		private MobBehaviourSpawner spawner;
	}
}
