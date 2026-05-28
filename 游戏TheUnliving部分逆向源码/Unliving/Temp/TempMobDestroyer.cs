using System;
using System.Collections;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Temp
{
	// Token: 0x02000004 RID: 4
	public class TempMobDestroyer : MonoBehaviour
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020B5 File Offset: 0x000002B5
		private void Awake()
		{
			this.mobBehaviourSpawner = base.GetComponent<MobBehaviourSpawner>();
			if (!this.mobBehaviourSpawner.IsNull())
			{
				this.mobBehaviourSpawner.GroupMobSpawned += this.OnGroupMobSpawned;
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x000020E7 File Offset: 0x000002E7
		private void OnDestroy()
		{
			if (!this.mobBehaviourSpawner.IsNull())
			{
				this.mobBehaviourSpawner.GroupMobSpawned -= this.OnGroupMobSpawned;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x0000210D File Offset: 0x0000030D
		private void OnGroupMobSpawned(BaseGameMob mob)
		{
			base.StartCoroutine(this.DamageRoutine(mob));
		}

		// Token: 0x06000008 RID: 8 RVA: 0x0000211D File Offset: 0x0000031D
		private IEnumerator DamageRoutine(BaseGameMob mob)
		{
			yield return new WaitForSeconds(this.damageDelay);
			if (!mob.IsNull())
			{
				mob.HitPointsController.ModifyHitPoints(this, this.damageArgs);
			}
			yield break;
		}

		// Token: 0x04000004 RID: 4
		public DamageGenerator.DamageSendingArgs damageArgs;

		// Token: 0x04000005 RID: 5
		public float damageDelay;

		// Token: 0x04000006 RID: 6
		private MobBehaviourSpawner mobBehaviourSpawner;
	}
}
