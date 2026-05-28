using System;
using System.Collections.Generic;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Pickables;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000D9 RID: 217
	public class RuneOfCreation : DefaultObjectPickable
	{
		// Token: 0x14000030 RID: 48
		// (add) Token: 0x06000554 RID: 1364 RVA: 0x000132FC File Offset: 0x000114FC
		// (remove) Token: 0x06000555 RID: 1365 RVA: 0x00013334 File Offset: 0x00011534
		public event Action RewardSpawned;

		// Token: 0x14000031 RID: 49
		// (add) Token: 0x06000556 RID: 1366 RVA: 0x0001336C File Offset: 0x0001156C
		// (remove) Token: 0x06000557 RID: 1367 RVA: 0x000133A4 File Offset: 0x000115A4
		public event Action<BaseGameMob, SacrificePositionTrigger> MobSacrificed;

		// Token: 0x06000558 RID: 1368 RVA: 0x000133DC File Offset: 0x000115DC
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			foreach (SacrificePositionTrigger sacrificePositionTrigger in this.sacrificeTriggers)
			{
				sacrificePositionTrigger.MobSacrificed += this.OnMobSacrificed;
				sacrificePositionTrigger.SetMobsLayerMask(this.mobsLayerMask);
			}
		}

		// Token: 0x06000559 RID: 1369 RVA: 0x00013425 File Offset: 0x00011625
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			this.ActivateRune();
		}

		// Token: 0x0600055A RID: 1370 RVA: 0x00013430 File Offset: 0x00011630
		public void ActivateRune()
		{
			if (this.rewardSpawned)
			{
				return;
			}
			this.rewardSpawned = true;
			int num = int.MaxValue;
			foreach (SacrificePositionTrigger sacrificePositionTrigger in this.sacrificeTriggers)
			{
				if (sacrificePositionTrigger.SacrificedMobsCount < num)
				{
					num = sacrificePositionTrigger.SacrificedMobsCount;
				}
				sacrificePositionTrigger.Deactivate();
			}
			GameObject gameObject = null;
			foreach (KeyValuePair<int, GameObject> keyValuePair in this.rewards)
			{
				if (keyValuePair.Key <= num)
				{
					gameObject = keyValuePair.Value;
				}
			}
			if (gameObject != null)
			{
				UnityEngine.Object.Instantiate<GameObject>(gameObject, this.rewardPosition).transform.localPosition = Vector3.zero;
				Action action = this.RewardSpawned;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x0600055B RID: 1371 RVA: 0x00013510 File Offset: 0x00011710
		private void OnMobSacrificed(BaseGameMob mob, SacrificePositionTrigger trigger)
		{
			Action<BaseGameMob, SacrificePositionTrigger> mobSacrificed = this.MobSacrificed;
			if (mobSacrificed != null)
			{
				mobSacrificed(mob, trigger);
			}
			Debug.Log(string.Concat(new string[]
			{
				mob.name,
				" ",
				trigger.name,
				" ",
				trigger.SacrificedMobsCount.ToString()
			}));
		}

		// Token: 0x040003A5 RID: 933
		public LayerMask mobsLayerMask;

		// Token: 0x040003A6 RID: 934
		public SacrificePositionTrigger[] sacrificeTriggers;

		// Token: 0x040003A7 RID: 935
		public RuneOfCreation.RewardsDictionary rewards;

		// Token: 0x040003A8 RID: 936
		public Transform rewardPosition;

		// Token: 0x040003A9 RID: 937
		private bool rewardSpawned;

		// Token: 0x0200042D RID: 1069
		[Serializable]
		public class RewardsDictionary : SerializableDictionary<int, GameObject>
		{
		}
	}
}
