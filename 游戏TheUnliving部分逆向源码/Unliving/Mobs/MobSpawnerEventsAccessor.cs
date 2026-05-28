using System;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.Mobs
{
	// Token: 0x020001C6 RID: 454
	public sealed class MobSpawnerEventsAccessor : MonoBehaviour
	{
		// Token: 0x170002C4 RID: 708
		// (get) Token: 0x06000E49 RID: 3657 RVA: 0x0002D6E9 File Offset: 0x0002B8E9
		public MobBehaviourSpawner TargetSpawner
		{
			get
			{
				return this.targetSpawner;
			}
		}

		// Token: 0x170002C5 RID: 709
		// (get) Token: 0x06000E4A RID: 3658 RVA: 0x0002D6F1 File Offset: 0x0002B8F1
		public UnityEvent<BaseGameMob> MobSpawned
		{
			get
			{
				return this.mobSpawned;
			}
		}

		// Token: 0x170002C6 RID: 710
		// (get) Token: 0x06000E4B RID: 3659 RVA: 0x0002D6F9 File Offset: 0x0002B8F9
		public UnityEvent<GameMobsGroupControllerBase> GroupSpawned
		{
			get
			{
				return this.groupSpawned;
			}
		}

		// Token: 0x170002C7 RID: 711
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x0002D701 File Offset: 0x0002B901
		public UnityEvent<GameMobsGroupControllerBase> GroupDestroyed
		{
			get
			{
				return this.groupDestroyed;
			}
		}

		// Token: 0x06000E4D RID: 3661 RVA: 0x0002D709 File Offset: 0x0002B909
		private void OnGroupMobSpawned(BaseGameMob mob)
		{
			this.mobSpawned.Invoke(mob);
		}

		// Token: 0x06000E4E RID: 3662 RVA: 0x0002D717 File Offset: 0x0002B917
		private void OnGroupSpawned(GameMobsGroupControllerBase group)
		{
			this.groupSpawned.Invoke(group);
		}

		// Token: 0x06000E4F RID: 3663 RVA: 0x0002D725 File Offset: 0x0002B925
		private void OnGroupDestroyed(MobBehaviourSpawner spawner, GameMobsGroupControllerBase group)
		{
			this.groupDestroyed.Invoke(group);
		}

		// Token: 0x06000E50 RID: 3664 RVA: 0x0002D733 File Offset: 0x0002B933
		private void Reset()
		{
			if (this.targetSpawner == null)
			{
				base.TryGetComponent<MobBehaviourSpawner>(out this.targetSpawner);
			}
		}

		// Token: 0x06000E51 RID: 3665 RVA: 0x0002D750 File Offset: 0x0002B950
		private void Start()
		{
			if (base.isActiveAndEnabled && (this.targetSpawner != null || base.TryGetComponent<MobBehaviourSpawner>(out this.targetSpawner)))
			{
				this.targetSpawner.GroupMobSpawned += this.OnGroupMobSpawned;
				this.targetSpawner.GroupSpawned += this.OnGroupSpawned;
				this.targetSpawner.GroupDestroyed += this.OnGroupDestroyed;
			}
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x0002D7C8 File Offset: 0x0002B9C8
		private void OnDestroy()
		{
			if (this.targetSpawner != null)
			{
				this.targetSpawner.GroupMobSpawned -= this.OnGroupMobSpawned;
				this.targetSpawner.GroupSpawned -= this.OnGroupSpawned;
				this.targetSpawner.GroupDestroyed -= this.OnGroupDestroyed;
			}
		}

		// Token: 0x04000875 RID: 2165
		[SerializeField]
		private MobBehaviourSpawner targetSpawner;

		// Token: 0x04000876 RID: 2166
		[SerializeField]
		private UnityEvent<BaseGameMob> mobSpawned;

		// Token: 0x04000877 RID: 2167
		[SerializeField]
		private UnityEvent<GameMobsGroupControllerBase> groupSpawned;

		// Token: 0x04000878 RID: 2168
		[SerializeField]
		private UnityEvent<GameMobsGroupControllerBase> groupDestroyed;
	}
}
