using System;
using Game.Core;
using Game.Factories;
using UnityEngine;
using Unliving.GameSession.PlayerLeveling;
using Unliving.Mobs.Animation;

namespace Unliving.Mobs
{
	// Token: 0x020001FE RID: 510
	[DisallowMultipleComponent]
	public sealed class NecromancySpot : GameBehaviourBase, IRevivableGameMob, IPlayerLevelingEXPSource
	{
		// Token: 0x17000383 RID: 899
		// (get) Token: 0x060010F9 RID: 4345 RVA: 0x00035300 File Offset: 0x00033500
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x060010FA RID: 4346 RVA: 0x00035303 File Offset: 0x00033503
		int IPlayerLevelingEXPSource.EXPAmount
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x140000BC RID: 188
		// (add) Token: 0x060010FB RID: 4347 RVA: 0x00035308 File Offset: 0x00033508
		// (remove) Token: 0x060010FC RID: 4348 RVA: 0x00035340 File Offset: 0x00033540
		public event Action<BaseGameMob, BaseGameMob> Revived;

		// Token: 0x060010FD RID: 4349 RVA: 0x00035375 File Offset: 0x00033575
		private void PrepareRevivedMob(BaseGameMob mob)
		{
			mob.PrepareRevivedMob(this, base.transform.position);
		}

		// Token: 0x060010FE RID: 4350 RVA: 0x00035390 File Offset: 0x00033590
		private void SetAnimationTrigger(BaseGameMob revivedMob)
		{
			if (string.IsNullOrEmpty(this.reviveAnimationTriggerName))
			{
				return;
			}
			GameMobAnimationController gameMobAnimationController = revivedMob.AnimationController as GameMobAnimationController;
			if (gameMobAnimationController != null)
			{
				gameMobAnimationController.reviveStateTriggerName = this.reviveAnimationTriggerName;
			}
		}

		// Token: 0x060010FF RID: 4351 RVA: 0x000353CC File Offset: 0x000335CC
		public bool CanBeRevived(BaseGameMob reviver, object context)
		{
			return base.enabled && reviver.IsValidReviver(this);
		}

		// Token: 0x06001100 RID: 4352 RVA: 0x000353E0 File Offset: 0x000335E0
		public BaseGameMob Revive(BaseGameMob reviver, object context, bool destroySourceMob = true)
		{
			if (this.CanBeRevived(reviver, context))
			{
				BaseGameMob baseGameMob = reviver.Group.AddMob((int)this.mobID, new Action<BaseGameMob>(this.PrepareRevivedMob));
				if (baseGameMob != null)
				{
					this.SetAnimationTrigger(baseGameMob);
					Action<BaseGameMob, BaseGameMob> revived = this.Revived;
					if (revived != null)
					{
						revived(reviver, baseGameMob);
					}
					if (destroySourceMob)
					{
						UnityEngine.Object.Destroy(base.gameObject);
					}
					return baseGameMob;
				}
			}
			return null;
		}

		// Token: 0x0400099B RID: 2459
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID mobID;

		// Token: 0x0400099C RID: 2460
		public string reviveAnimationTriggerName = "ReviveFromGrave";
	}
}
