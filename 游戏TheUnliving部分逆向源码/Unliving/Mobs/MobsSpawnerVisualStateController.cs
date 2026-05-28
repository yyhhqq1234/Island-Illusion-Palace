using System;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001CA RID: 458
	public sealed class MobsSpawnerVisualStateController : MonoBehaviour
	{
		// Token: 0x06000E6C RID: 3692 RVA: 0x0002DABC File Offset: 0x0002BCBC
		private void SetState(bool newState)
		{
			bool? flag = this.isOpened;
			if (flag.GetValueOrDefault() == newState & flag != null)
			{
				return;
			}
			for (int i = 0; i < this.closedStateObjects.Length; i++)
			{
				if (!(this.closedStateObjects[i] == null))
				{
					this.closedStateObjects[i].SetActive(!newState);
				}
			}
			for (int j = 0; j < this.closedStateObjects.Length; j++)
			{
				if (!(this.openedStateObjects[j] == null))
				{
					this.openedStateObjects[j].SetActive(newState);
				}
			}
			this.isOpened = new bool?(newState);
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x0002DB58 File Offset: 0x0002BD58
		private void OnSpawningStarted(MobBehaviourSpawner spawner)
		{
			this.SetState(true);
			spawner.SpawningStarted -= this.OnSpawningStarted;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x0002DB74 File Offset: 0x0002BD74
		private void Start()
		{
			if (this.targetSpawner == null)
			{
				base.TryGetComponent<MobBehaviourSpawner>(out this.targetSpawner);
			}
			this.SetState(false);
			if (this.targetSpawner != null)
			{
				if (this.targetSpawner.IsSpawningStarted)
				{
					this.SetState(true);
					return;
				}
				this.targetSpawner.SpawningStarted += this.OnSpawningStarted;
			}
		}

		// Token: 0x06000E6F RID: 3695 RVA: 0x0002DBDD File Offset: 0x0002BDDD
		private void OnDestroy()
		{
			if (this.targetSpawner != null)
			{
				this.targetSpawner.SpawningStarted -= this.OnSpawningStarted;
			}
		}

		// Token: 0x04000884 RID: 2180
		public MobBehaviourSpawner targetSpawner;

		// Token: 0x04000885 RID: 2181
		[Space]
		public GameObject[] closedStateObjects;

		// Token: 0x04000886 RID: 2182
		public GameObject[] openedStateObjects;

		// Token: 0x04000887 RID: 2183
		private bool? isOpened;
	}
}
