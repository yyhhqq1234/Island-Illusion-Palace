using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Unliving.Mobs;

namespace Unliving.Tutorial
{
	// Token: 0x02000029 RID: 41
	public sealed class MobsKillingTrigger : MonoBehaviour
	{
		// Token: 0x17000058 RID: 88
		// (get) Token: 0x06000170 RID: 368 RVA: 0x00006348 File Offset: 0x00004548
		public UnityEvent AllMobsWereKilled
		{
			get
			{
				return this._allMobsWereKilled;
			}
		}

		// Token: 0x06000171 RID: 369 RVA: 0x00006350 File Offset: 0x00004550
		private void TryFireEvent()
		{
			if (this.aliveGroupsCount <= 0 && (this.additionalMobsToKill == null || this.additionalMobsToKill.Count == 0))
			{
				this._allMobsWereKilled.Invoke();
			}
		}

		// Token: 0x06000172 RID: 370 RVA: 0x0000637C File Offset: 0x0000457C
		private void OnSpawnedMobKilled(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (this.aliveGroupsCount <= 0)
			{
				return;
			}
			for (int i = 0; i < this.targetMobSpawners.Length; i++)
			{
				MobBehaviourSpawner mobBehaviourSpawner = this.targetMobSpawners[i];
				if (mobBehaviourSpawner.IsGroupSpawned)
				{
					GameMobsGroupControllerBase spawnedGroup = mobBehaviourSpawner.SpawnedGroup;
					if (spawnedGroup == group && !spawnedGroup.HasMobs)
					{
						this.aliveGroupsCount--;
					}
				}
			}
			this.TryFireEvent();
		}

		// Token: 0x06000173 RID: 371 RVA: 0x000063DE File Offset: 0x000045DE
		private void OnTargetMobKilled(IGameMob killedMob)
		{
			this.additionalMobsToKill.Remove((BaseGameMob)killedMob);
			killedMob.Killed -= this.OnTargetMobKilled;
			this.TryFireEvent();
		}

		// Token: 0x06000174 RID: 372 RVA: 0x0000640A File Offset: 0x0000460A
		private IEnumerator Start()
		{
			if (this.additionalMobsToKill != null)
			{
				for (int i = this.additionalMobsToKill.Count - 1; i >= 0; i--)
				{
					BaseGameMob baseGameMob = this.additionalMobsToKill[i];
					if (baseGameMob == null || baseGameMob.IsKilled)
					{
						this.additionalMobsToKill.RemoveAt(i);
					}
					else
					{
						baseGameMob.Killed += this.OnTargetMobKilled;
					}
				}
			}
			if (this.targetMobSpawners != null)
			{
				yield return null;
				foreach (MobBehaviourSpawner mobBehaviourSpawner in this.targetMobSpawners)
				{
					if (!(mobBehaviourSpawner == null) && (mobBehaviourSpawner.RemainingSpawningCount > 0 || mobBehaviourSpawner.IsGroupSpawned))
					{
						mobBehaviourSpawner.SpawnedGroup.MobRemoved += this.OnSpawnedMobKilled;
						this.aliveGroupsCount++;
					}
				}
			}
			yield break;
		}

		// Token: 0x06000175 RID: 373 RVA: 0x0000641C File Offset: 0x0000461C
		private void OnDestroy()
		{
			if (this.additionalMobsToKill != null)
			{
				foreach (BaseGameMob baseGameMob in this.additionalMobsToKill)
				{
					if (baseGameMob != null)
					{
						baseGameMob.Killed -= this.OnTargetMobKilled;
					}
				}
			}
			if (this.targetMobSpawners != null)
			{
				foreach (MobBehaviourSpawner mobBehaviourSpawner in this.targetMobSpawners)
				{
					if (((mobBehaviourSpawner != null) ? mobBehaviourSpawner.SpawnedGroup : null) != null)
					{
						mobBehaviourSpawner.SpawnedGroup.MobRemoved -= this.OnSpawnedMobKilled;
					}
				}
			}
		}

		// Token: 0x06000176 RID: 374 RVA: 0x000064D8 File Offset: 0x000046D8
		private void OnDrawGizmosSelected()
		{
			if (this.targetMobSpawners != null)
			{
				Gizmos.color = Color.yellow;
				foreach (MobBehaviourSpawner mobBehaviourSpawner in this.targetMobSpawners)
				{
					if (!(mobBehaviourSpawner == null))
					{
						Gizmos.DrawLine(mobBehaviourSpawner.transform.position, base.transform.position);
					}
				}
			}
		}

		// Token: 0x040000B5 RID: 181
		[FormerlySerializedAs("unlockingSpawners")]
		public MobBehaviourSpawner[] targetMobSpawners;

		// Token: 0x040000B6 RID: 182
		public List<BaseGameMob> additionalMobsToKill;

		// Token: 0x040000B7 RID: 183
		[SerializeField]
		private UnityEvent _allMobsWereKilled;

		// Token: 0x040000B8 RID: 184
		private int aliveGroupsCount;
	}
}
