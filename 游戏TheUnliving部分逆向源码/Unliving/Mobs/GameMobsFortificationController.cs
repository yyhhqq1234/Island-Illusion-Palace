using System;
using Game.Damage;
using Game.Gameplay;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001D7 RID: 471
	public sealed class GameMobsFortificationController : DestructibleObjectChainTrigger
	{
		// Token: 0x170002F9 RID: 761
		// (get) Token: 0x06000F1D RID: 3869 RVA: 0x0003000B File Offset: 0x0002E20B
		public GameMobFactions Faction
		{
			get
			{
				return this._faction;
			}
		}

		// Token: 0x170002FA RID: 762
		// (get) Token: 0x06000F1E RID: 3870 RVA: 0x00030013 File Offset: 0x0002E213
		// (set) Token: 0x06000F1F RID: 3871 RVA: 0x0003001B File Offset: 0x0002E21B
		public MobBehaviourSpawner TargetMobsSpawner
		{
			get
			{
				return this._targetMobsSpawner;
			}
			set
			{
				if (!this.isInitialized)
				{
					this._targetMobsSpawner = value;
				}
			}
		}

		// Token: 0x06000F20 RID: 3872 RVA: 0x0003002C File Offset: 0x0002E22C
		private bool TryDestroyFortification()
		{
			if (GameMobGroupController.IsDeadGroup(this.groupController))
			{
				base.DestroyAllObjects();
				return true;
			}
			return false;
		}

		// Token: 0x06000F21 RID: 3873 RVA: 0x00030044 File Offset: 0x0002E244
		protected override void OnDestructibleRegistered(IDamageable destructible)
		{
			if (!(this._targetMobsSpawner == null))
			{
				BaseGameMob baseGameMob = destructible.Behaviour as BaseGameMob;
				if (baseGameMob != null)
				{
					if (this._faction != GameMobFactions.None)
					{
						baseGameMob.defaultFaction = this._faction;
					}
					if (this.currentFactionMobsLayers != 0)
					{
						HitPointsController hitPointsController = destructible as HitPointsController;
						if (hitPointsController != null)
						{
							HitPointsController hitPointsController2 = hitPointsController;
							hitPointsController2.ignorableDamageSenderLayers |= this.currentFactionMobsLayers;
						}
					}
					return;
				}
			}
		}

		// Token: 0x06000F22 RID: 3874 RVA: 0x000300B3 File Offset: 0x0002E2B3
		protected override void OnHealthThresholdReached()
		{
			if (this._targetMobsSpawner != null && this._targetMobsSpawner.MobsMovementPointLimiter != null)
			{
				this._targetMobsSpawner.MobsMovementPointLimiter.IsActive = false;
			}
		}

		// Token: 0x06000F23 RID: 3875 RVA: 0x000300E1 File Offset: 0x0002E2E1
		private void OnGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob removedMob)
		{
			if (this.TryDestroyFortification())
			{
				this.groupController.MobRemoved -= this.OnGroupMobRemoved;
			}
		}

		// Token: 0x06000F24 RID: 3876 RVA: 0x00030104 File Offset: 0x0002E304
		protected override async void Start()
		{
			this._faction = GameMobFactions.None;
			if (this._targetMobsSpawner == null)
			{
				this._targetMobsSpawner = base.GetComponentInParent<MobBehaviourSpawner>();
			}
			if (this._targetMobsSpawner != null)
			{
				this._faction = this._targetMobsSpawner.GroupOwner;
				GameMobsFactory gameMobsFactory;
				if (base.CurrentGame.Services.TryGet<GameMobsFactory>(out gameMobsFactory))
				{
					GameMobFactionInfo factionInfo = gameMobsFactory.GetFactionInfo(this._faction);
					if (factionInfo.IsValid())
					{
						this.currentFactionMobsLayers = 1 << factionInfo.mobsLayer;
					}
				}
				if (this.destroyEmptyFortification)
				{
					GameMobsGroupControllerBase gameMobsGroupControllerBase = await this._targetMobsSpawner.GetGroupAsync();
					this.groupController = gameMobsGroupControllerBase;
					if (this.groupController != null)
					{
						this.groupController.MobRemoved += this.OnGroupMobRemoved;
					}
				}
			}
			base.Start();
			this.isInitialized = true;
		}

		// Token: 0x06000F25 RID: 3877 RVA: 0x0003013D File Offset: 0x0002E33D
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.groupController != null)
			{
				this.groupController.MobRemoved -= this.OnGroupMobRemoved;
			}
		}

		// Token: 0x040008E8 RID: 2280
		[SerializeField]
		private MobBehaviourSpawner _targetMobsSpawner;

		// Token: 0x040008E9 RID: 2281
		public bool destroyEmptyFortification = true;

		// Token: 0x040008EA RID: 2282
		private GameMobsGroupControllerBase groupController;

		// Token: 0x040008EB RID: 2283
		[NonSerialized]
		private bool isInitialized;

		// Token: 0x040008EC RID: 2284
		private GameMobFactions _faction;

		// Token: 0x040008ED RID: 2285
		private int currentFactionMobsLayers;
	}
}
