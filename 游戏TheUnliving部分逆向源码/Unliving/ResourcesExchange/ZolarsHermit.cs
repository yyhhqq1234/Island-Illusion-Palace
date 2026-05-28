using System;
using Common.UnityExtensions;
using Game.Core;
using Game.Damage;
using Game.Factories;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Abilities;
using Unliving.Challenges;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.ResourcesExchange
{
	// Token: 0x020000DC RID: 220
	public sealed class ZolarsHermit : GameBehaviourBase
	{
		// Token: 0x170000C5 RID: 197
		// (get) Token: 0x0600056C RID: 1388 RVA: 0x00013810 File Offset: 0x00011A10
		public float PlayerDistanceProgress
		{
			get
			{
				if (!this.isPlayerInsideCollider)
				{
					return 0f;
				}
				return 1f - this.currentPlayerDistance / this.colliderRadius;
			}
		}

		// Token: 0x14000033 RID: 51
		// (add) Token: 0x0600056D RID: 1389 RVA: 0x00013834 File Offset: 0x00011A34
		// (remove) Token: 0x0600056E RID: 1390 RVA: 0x0001386C File Offset: 0x00011A6C
		public event Action RewardSpawned;

		// Token: 0x14000034 RID: 52
		// (add) Token: 0x0600056F RID: 1391 RVA: 0x000138A4 File Offset: 0x00011AA4
		// (remove) Token: 0x06000570 RID: 1392 RVA: 0x000138DC File Offset: 0x00011ADC
		public event Action HermitMobKilled;

		// Token: 0x06000571 RID: 1393 RVA: 0x00013914 File Offset: 0x00011B14
		private void SetPlayerProjectileAbilitiesLocked(bool isLocked)
		{
			if (this.currentPlayer.IsNull())
			{
				return;
			}
			AbilityDescription abilityDescription = new AbilityDescription
			{
				abilityID = AbilityID.MeleeComboBase
			};
			if (isLocked)
			{
				this.currentPlayer.AbilitiesController.SetAllowedAbilityDescription(abilityDescription, true);
				return;
			}
			this.currentPlayer.AbilitiesController.ResetAllowedAbilitiesDescription();
		}

		// Token: 0x06000572 RID: 1394 RVA: 0x00013968 File Offset: 0x00011B68
		private void Start()
		{
			if (!this.projectileObstacle.IsNull())
			{
				this.projectileObstacle.gameObject.SetActive(true);
			}
			base.TryGetComponent<PlayerMobsMovementBlocker>(out this.mobsMovementBlocker);
			CircleCollider2D circleCollider2D;
			if (base.TryGetComponent<CircleCollider2D>(out circleCollider2D))
			{
				this.colliderRadius = circleCollider2D.radius;
			}
			IGameMobsFactory gameMobsFactory;
			if (base.CurrentGame.Services.TryGet<IGameMobsFactory>(out gameMobsFactory))
			{
				MobBehaviour.FactoryArgs args = new MobBehaviour.FactoryArgs
				{
					mobID = this.hermitMobID,
					isEnvironmentMob = true,
					isBrainlessMob = true,
					hitPointsAmountOverride = 1f,
					spawnPosition = base.transform.position
				};
				if ((this.hermitMob = (gameMobsFactory.Create(args) as BaseGameMob)) != null)
				{
					this.hermitMob.isMinorAttackTarget = true;
					this.hermitMob.SetTotalBuffsImmunityActive(true);
					IPlayerProvider playerProvider = base.CurrentGame.Services.Get<IPlayerProvider>();
					this.currentPlayer = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
					HitPointsController hitPointsController = this.hermitMob.HitPointsController as HitPointsController;
					if (hitPointsController != null)
					{
						if (this.currentPlayer != null)
						{
							hitPointsController.ignorableDamageSenderLayers = ~this.currentPlayer.LayerMask;
						}
						hitPointsController.ModifyHealingResistance(1f);
					}
					this.hermitMob.Killed += this.OnHermitMobKilled;
				}
			}
		}

		// Token: 0x06000573 RID: 1395 RVA: 0x00013AC4 File Offset: 0x00011CC4
		private void Update()
		{
			if (this.isPlayerInsideCollider)
			{
				this.currentPlayerDistance = (this.currentPlayer.Position - base.transform.position).magnitude;
			}
		}

		// Token: 0x06000574 RID: 1396 RVA: 0x00013B08 File Offset: 0x00011D08
		private void OnHermitMobKilled(IGameMob mob)
		{
			base.GetComponent<Collider2D>().enabled = false;
			this.projectileObstacle.enabled = false;
			this.SetPlayerProjectileAbilitiesLocked(false);
			this.mobsMovementBlocker.SetActiveState(false);
			this.reward.CreateReward(this.currentPlayer.TempItemsStorageController, this.validChunckTypes);
			mob.Killed -= this.OnHermitMobKilled;
			this.hermitMob = null;
			Action rewardSpawned = this.RewardSpawned;
			if (rewardSpawned != null)
			{
				rewardSpawned();
			}
			Action hermitMobKilled = this.HermitMobKilled;
			if (hermitMobKilled != null)
			{
				hermitMobKilled();
			}
			UnityEvent hermitMobKilledEvents = this.HermitMobKilledEvents;
			if (hermitMobKilledEvents == null)
			{
				return;
			}
			hermitMobKilledEvents.Invoke();
		}

		// Token: 0x06000575 RID: 1397 RVA: 0x00013BA7 File Offset: 0x00011DA7
		private void OnTriggerEnter2D(Collider2D collider)
		{
			if (!this.currentPlayer.HasSameGameObject(collider))
			{
				return;
			}
			this.isPlayerInsideCollider = true;
			this.SetPlayerProjectileAbilitiesLocked(true);
			Physics2D.IgnoreCollision(collider, this.projectileObstacle, true);
			this.mobsMovementBlocker.SetActiveState(true);
		}

		// Token: 0x06000576 RID: 1398 RVA: 0x00013BDF File Offset: 0x00011DDF
		private void OnTriggerExit2D(Collider2D collider)
		{
			if (!this.currentPlayer.HasSameGameObject(collider))
			{
				return;
			}
			this.isPlayerInsideCollider = false;
			this.SetPlayerProjectileAbilitiesLocked(false);
			Physics2D.IgnoreCollision(collider, this.projectileObstacle, false);
			this.mobsMovementBlocker.SetActiveState(false);
		}

		// Token: 0x06000577 RID: 1399 RVA: 0x00013C17 File Offset: 0x00011E17
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.hermitMob != null)
			{
				this.hermitMob.Killed -= this.OnHermitMobKilled;
			}
		}

		// Token: 0x040003B4 RID: 948
		public UnityEvent HermitMobKilledEvents;

		// Token: 0x040003B5 RID: 949
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID hermitMobID;

		// Token: 0x040003B6 RID: 950
		public Transform rewardTransform;

		// Token: 0x040003B7 RID: 951
		public Collider2D projectileObstacle;

		// Token: 0x040003B8 RID: 952
		public ChallengeRewardInfo reward;

		// Token: 0x040003B9 RID: 953
		public LocationChunk.TypeID[] validChunckTypes;

		// Token: 0x040003BA RID: 954
		private BaseGameMob hermitMob;

		// Token: 0x040003BB RID: 955
		private PlayerBehaviour currentPlayer;

		// Token: 0x040003BC RID: 956
		private PlayerMobsMovementBlocker mobsMovementBlocker;

		// Token: 0x040003BD RID: 957
		private float colliderRadius;

		// Token: 0x040003BE RID: 958
		private float currentPlayerDistance;

		// Token: 0x040003BF RID: 959
		private bool isPlayerInsideCollider;
	}
}
