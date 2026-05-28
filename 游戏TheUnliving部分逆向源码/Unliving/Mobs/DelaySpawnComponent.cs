using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Core;
using Game.Damage;
using UnityEngine;
using UnityEngine.Events;

namespace Unliving.Mobs
{
	// Token: 0x020001B8 RID: 440
	public sealed class DelaySpawnComponent : GameBehaviourBase
	{
		// Token: 0x06000D89 RID: 3465 RVA: 0x0002AB54 File Offset: 0x00028D54
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (!this.isActive)
			{
				return;
			}
			if (base.TryGetComponent<MobBehaviourSpawner>(out this.currentSpawner))
			{
				this.currentSpawner.enabled = false;
				this.currentSpawner.spawnOnStart = false;
			}
			if (base.TryGetComponent<IGroupDestinationsGenerator>(out this.destinationsGenerator))
			{
				this.destinationsGenerator.IsActive = false;
			}
			if (this.targetSpawner != null)
			{
				this.targetSpawner.GroupSpawned += this.OnTargetGroupSpawned;
			}
		}

		// Token: 0x06000D8A RID: 3466 RVA: 0x0002ABD8 File Offset: 0x00028DD8
		private void OnTargetGroupSpawned(GameMobsGroupControllerBase spawnedGroup)
		{
			IReadOnlyList<BaseGameMob> mobs = this.targetSpawner.SpawnedGroup.Mobs;
			if (mobs.Count == 1)
			{
				this.targetMob = mobs[0];
				if (!this.targetMob.IsNull() && this.targetMob.HitPointsController != null)
				{
					this.targetMob.HitPointsController.HitPointsChanged += this.OnTargetMobHPChanged;
					return;
				}
			}
			else
			{
				this.targetSpawner.SpawnedGroup.MobRemoved += this.OnTargetGroupMobRemoved;
				this.spawnedMobsCount = mobs.Count;
			}
		}

		// Token: 0x06000D8B RID: 3467 RVA: 0x0002AC6B File Offset: 0x00028E6B
		private void OnTargetMobHPChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (hitPointsSource.CurrentHitPoints / hitPointsSource.InitialHitPoints <= this.groupHealthSpawnThreshold)
			{
				this.targetMob.HitPointsController.HitPointsChanged -= this.OnTargetMobHPChanged;
				this.SpawnCurrentSpawner();
			}
		}

		// Token: 0x06000D8C RID: 3468 RVA: 0x0002ACA4 File Offset: 0x00028EA4
		private void OnTargetGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			if (GameApplication.IsGameStateChanging || mob.IsSummoned)
			{
				return;
			}
			float num = 1f;
			int num2 = this.destroyedMobsCount + 1;
			this.destroyedMobsCount = num2;
			if (num - (float)num2 / (float)this.spawnedMobsCount <= this.groupHealthSpawnThreshold)
			{
				this.targetSpawner.SpawnedGroup.MobRemoved -= this.OnTargetGroupMobRemoved;
				this.SpawnCurrentSpawner();
			}
		}

		// Token: 0x06000D8D RID: 3469 RVA: 0x0002AD0C File Offset: 0x00028F0C
		public void SpawnCurrentSpawner()
		{
			this.currentSpawner.enabled = true;
			this.currentSpawner.GroupMobSpawned += this.OnCurrentGroupMobSpawned;
			this.currentSpawner.GroupSpawned += this.OnCurrentGroupSpawned;
			foreach (DelaySpawnComponent.DelayedEvent delayedEvent in this.spawnStartedEvents)
			{
				if (delayedEvent != null)
				{
					delayedEvent.Execute();
				}
			}
		}

		// Token: 0x06000D8E RID: 3470 RVA: 0x0002AD78 File Offset: 0x00028F78
		private void OnCurrentGroupSpawned(GameMobsGroupControllerBase spawnedGroup)
		{
			if (this.destinationsGenerator != null)
			{
				this.destinationsGenerator.IsActive = true;
			}
			foreach (BaseGameMob baseGameMob in this.currentSpawner.SpawnedGroup.Mobs)
			{
				GameMobAIController aicontroller = baseGameMob.AIController;
				if (aicontroller != null)
				{
					aicontroller.CurrentParams.ResetTempTargetSearchRadius();
				}
			}
			foreach (DelaySpawnComponent.DelayedEvent delayedEvent in this.spawnFinishedEvents)
			{
				if (delayedEvent != null)
				{
					delayedEvent.Execute();
				}
			}
		}

		// Token: 0x06000D8F RID: 3471 RVA: 0x0002AE14 File Offset: 0x00029014
		private void OnCurrentGroupMobSpawned(BaseGameMob m)
		{
			GameMobAIController aicontroller = m.AIController;
			if (aicontroller != null)
			{
				aicontroller.CurrentParams.SetTempTargetSearchRadius(this.tempAggressionRadius);
			}
			foreach (DelaySpawnComponent.DelayedEvent delayedEvent in this.mobSpawnedEvents)
			{
				if (delayedEvent != null)
				{
					delayedEvent.Execute();
				}
			}
		}

		// Token: 0x06000D90 RID: 3472 RVA: 0x0002AE60 File Offset: 0x00029060
		public override void Destroy()
		{
			base.Destroy();
			if (!this.targetSpawner.IsNull())
			{
				this.targetSpawner.GroupSpawned -= this.OnTargetGroupSpawned;
				this.targetSpawner.SpawnedGroup.MobRemoved -= this.OnTargetGroupMobRemoved;
			}
			if (!this.currentSpawner.IsNull())
			{
				this.currentSpawner.GroupMobSpawned -= this.OnCurrentGroupMobSpawned;
				this.currentSpawner.GroupSpawned -= this.OnCurrentGroupSpawned;
			}
			if (!this.targetMob.IsNull())
			{
				this.targetMob.HitPointsController.HitPointsChanged -= this.OnTargetMobHPChanged;
			}
		}

		// Token: 0x040007BF RID: 1983
		public bool isActive = true;

		// Token: 0x040007C0 RID: 1984
		[Header("Основной спаунер мобов")]
		public MobBehaviourSpawner targetSpawner;

		// Token: 0x040007C1 RID: 1985
		[Header("Условие для спауна: процент оставшихся в живых", order = 0)]
		[Space(-10f, order = 1)]
		[Header("мобов основной группы, но, если моб всего один,", order = 2)]
		[Space(-10f, order = 3)]
		[Header("то процент оставшегося у него НР.", order = 4)]
		[Space(10f, order = 5)]
		public float groupHealthSpawnThreshold = 0.3f;

		// Token: 0x040007C2 RID: 1986
		[Header("Радиус агрессии, пока не была заспаунена вся группа")]
		public float tempAggressionRadius = 1f;

		// Token: 0x040007C3 RID: 1987
		[Header("События при старте спауна мобов их дополнительного спаунера")]
		public DelaySpawnComponent.DelayedEvent[] spawnStartedEvents;

		// Token: 0x040007C4 RID: 1988
		[Header("События при спауне каждого моба")]
		public DelaySpawnComponent.DelayedEvent[] mobSpawnedEvents;

		// Token: 0x040007C5 RID: 1989
		[Header("События при окончании спауна мобов")]
		public DelaySpawnComponent.DelayedEvent[] spawnFinishedEvents;

		// Token: 0x040007C6 RID: 1990
		private MobBehaviourSpawner currentSpawner;

		// Token: 0x040007C7 RID: 1991
		private int spawnedMobsCount;

		// Token: 0x040007C8 RID: 1992
		private int destroyedMobsCount;

		// Token: 0x040007C9 RID: 1993
		private IGroupDestinationsGenerator destinationsGenerator;

		// Token: 0x040007CA RID: 1994
		private BaseGameMob targetMob;

		// Token: 0x02000485 RID: 1157
		[Serializable]
		public class DelayedEvent
		{
			// Token: 0x06002420 RID: 9248 RVA: 0x0006FD54 File Offset: 0x0006DF54
			public async void Execute()
			{
				await new WaitForSeconds(this.delay);
				if (GameApplication.IsGameLoopRunning())
				{
					UnityEvent unityEvent = this.delayedEvent;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
				}
			}

			// Token: 0x040017AB RID: 6059
			public float delay;

			// Token: 0x040017AC RID: 6060
			public UnityEvent delayedEvent;
		}
	}
}
