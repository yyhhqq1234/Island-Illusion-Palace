using System;
using System.Collections;
using System.Collections.Generic;
using Common.Editor;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Tutorial
{
	// Token: 0x02000034 RID: 52
	public sealed class TutorialTriggerArea : MonoBehaviour
	{
		// Token: 0x060001C1 RID: 449 RVA: 0x0000728F File Offset: 0x0000548F
		private static bool LockAbility(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			return false;
		}

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x060001C2 RID: 450 RVA: 0x00007292 File Offset: 0x00005492
		public PlayerBehaviour CurrentPlayer
		{
			get
			{
				return this.currentPlayer;
			}
		}

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x060001C3 RID: 451 RVA: 0x0000729A File Offset: 0x0000549A
		public TutorialTriggerArea.Event PlayerEntered
		{
			get
			{
				return this._playerEntered;
			}
		}

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x060001C4 RID: 452 RVA: 0x000072A2 File Offset: 0x000054A2
		public TutorialTriggerArea.Event PlayerExited
		{
			get
			{
				return this._playerExited;
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x060001C5 RID: 453 RVA: 0x000072AA File Offset: 0x000054AA
		public TutorialTriggerArea.Event NecromancyUsedByPlayer
		{
			get
			{
				return this._necromancyUsedByPlayer;
			}
		}

		// Token: 0x060001C6 RID: 454 RVA: 0x000072B2 File Offset: 0x000054B2
		private IEnumerator MobsAdditionRoutine(GameMobsGroupControllerBase playerGroup, MobBehaviour.ID targetMobID)
		{
			yield return new WaitForSecondsRealtime(this.requiredMobsSpawningDelay);
			for (int i = 0; i < this.requiredMobs.Length; i++)
			{
				TutorialTriggerArea.RequiredMobsInfo requiredMobsInfo = this.requiredMobs[i];
				if (targetMobID == MobBehaviour.ID.None || requiredMobsInfo.mobsID == targetMobID)
				{
					int spawningCount = requiredMobsInfo.GetSpawningCount(playerGroup);
					for (int j = 0; j < spawningCount; j++)
					{
						playerGroup.AddMob((int)requiredMobsInfo.mobsID, null);
					}
				}
			}
			this.mobsAdditionCoroutine = null;
			yield break;
		}

		// Token: 0x060001C7 RID: 455 RVA: 0x000072D0 File Offset: 0x000054D0
		private void AddRequiredMobs(MobBehaviour.ID removedMobID = MobBehaviour.ID.None)
		{
			if (this.IsNull() || !base.gameObject.activeInHierarchy || this.killPlayerSquad || this.currentPlayer == null || this.currentPlayer.IsKilled || this.mobsAdditionCoroutine != null)
			{
				return;
			}
			GameMobsGroupControllerBase group = this.currentPlayer.Group;
			if (group == null)
			{
				return;
			}
			this.mobsAdditionCoroutine = base.StartCoroutine(this.MobsAdditionRoutine(group, removedMobID));
		}

		// Token: 0x060001C8 RID: 456 RVA: 0x00007342 File Offset: 0x00005542
		private void InterruptRequiredMobsAddition()
		{
			if (this.mobsAdditionCoroutine == null)
			{
				return;
			}
			base.StopCoroutine(this.mobsAdditionCoroutine);
			this.mobsAdditionCoroutine = null;
		}

		// Token: 0x060001C9 RID: 457 RVA: 0x00007360 File Offset: 0x00005560
		private void SetAbilitiesLockActive(BaseGameMob playerMob, bool isActive)
		{
			if (this.playerMobsForbiddenAbilitySlots == null || this.playerMobsForbiddenAbilitySlots.Length == 0)
			{
				return;
			}
			BaseAbilitiesController baseAbilitiesController = (playerMob != null) ? playerMob.AbilitiesController : null;
			if (baseAbilitiesController != null)
			{
				IReadOnlyList<BaseAbility> abilities = baseAbilitiesController.Abilities;
				for (int i = 0; i < this.playerMobsForbiddenAbilitySlots.Length; i++)
				{
					int num = this.playerMobsForbiddenAbilitySlots[i];
					if (num >= 0 && num < abilities.Count)
					{
						BaseAbility baseAbility = abilities[num];
						if (isActive)
						{
							baseAbility.Complete();
							baseAbility.AddPreActivationCondition(new BaseAbility.ActivationCondition(TutorialTriggerArea.LockAbility));
						}
						else
						{
							baseAbility.RemovePreActivationCondition(new BaseAbility.ActivationCondition(TutorialTriggerArea.LockAbility));
						}
					}
				}
			}
		}

		// Token: 0x060001CA RID: 458 RVA: 0x000073FC File Offset: 0x000055FC
		private void RestorePlayerHealth()
		{
			IDamageable hitPointsController = this.currentPlayer.HitPointsController;
			float hitPointsLack = hitPointsController.HitPointsLack;
			if (hitPointsLack <= 0f)
			{
				return;
			}
			hitPointsController.ModifyHitPoints(this, new HitPointsController.HPChangingArgs(false)
			{
				amount = hitPointsLack
			});
		}

		// Token: 0x060001CB RID: 459 RVA: 0x0000743C File Offset: 0x0000563C
		private void KillPlayerSquad()
		{
			IReadOnlyList<BaseGameMob> mobs = this.currentPlayer.Group.Mobs;
			for (int i = mobs.Count - 1; i >= 0; i--)
			{
				mobs[i].KillMob(null);
			}
		}

		// Token: 0x060001CC RID: 460 RVA: 0x0000747C File Offset: 0x0000567C
		private void ResetPlayer()
		{
			if (this.currentPlayer == null)
			{
				return;
			}
			if (this.currentPlayer.Group != null)
			{
				this.currentPlayer.Group.MobRemoved -= this.OnPlayerMobRemovedFromGroup;
			}
			this.currentPlayer.RevivedMob -= this.OnMobRevivedByPlayer;
			this.currentPlayer = null;
		}

		// Token: 0x060001CD RID: 461 RVA: 0x000074E0 File Offset: 0x000056E0
		private void OnPlayerMobRemovedFromGroup(GameMobsGroupControllerBase playerGroup, BaseGameMob mob)
		{
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour != null)
			{
				this.AddRequiredMobs(mobBehaviour.ObjectID);
			}
		}

		// Token: 0x060001CE RID: 462 RVA: 0x00007503 File Offset: 0x00005703
		private void OnMobRevivedByPlayer(BaseGameMob revivedMob, IRevivableGameMob deadMob)
		{
			this._necromancyUsedByPlayer.Invoke(this);
		}

		// Token: 0x060001CF RID: 463 RVA: 0x00007514 File Offset: 0x00005714
		private void Awake()
		{
			Collider2D collider2D;
			if (base.TryGetComponent<Collider2D>(out collider2D))
			{
				collider2D.isTrigger = true;
			}
		}

		// Token: 0x060001D0 RID: 464 RVA: 0x00007534 File Offset: 0x00005734
		private void OnTriggerEnter2D(Collider2D collider)
		{
			BaseGameMob baseGameMob;
			if (!collider.TryGetComponent<BaseGameMob>(out baseGameMob))
			{
				return;
			}
			if (!(baseGameMob is PlayerBehaviour))
			{
				if (baseGameMob.IsPlayerMob)
				{
					this.SetAbilitiesLockActive(baseGameMob, true);
				}
				return;
			}
			if (this.currentPlayer != null)
			{
				return;
			}
			this.currentPlayer = (baseGameMob as PlayerBehaviour);
			if (this.currentPlayer != null)
			{
				if (this.restorePlayerHealth && this.currentPlayer.HitPointsController != null)
				{
					this.RestorePlayerHealth();
				}
				if (this.currentPlayer.Group != null)
				{
					if (this.killPlayerSquad)
					{
						this.KillPlayerSquad();
					}
					this.currentPlayer.Group.MobRemoved += this.OnPlayerMobRemovedFromGroup;
				}
				this.AddRequiredMobs(MobBehaviour.ID.None);
				this.currentPlayer.RevivedMob += this.OnMobRevivedByPlayer;
				this._playerEntered.Invoke(this);
			}
		}

		// Token: 0x060001D1 RID: 465 RVA: 0x0000760C File Offset: 0x0000580C
		private void OnTriggerExit2D(Collider2D collider)
		{
			BaseGameMob component = collider.GetComponent<BaseGameMob>();
			if (component != null && component.IsPlayerMob)
			{
				this.SetAbilitiesLockActive(component, false);
			}
			if (collider.HasSameGameObject(this.currentPlayer))
			{
				this._playerExited.Invoke(this);
				this.ResetPlayer();
			}
		}

		// Token: 0x060001D2 RID: 466 RVA: 0x00007659 File Offset: 0x00005859
		private void OnDisable()
		{
			this.InterruptRequiredMobsAddition();
			this.ResetPlayer();
		}

		// Token: 0x060001D3 RID: 467 RVA: 0x00007667 File Offset: 0x00005867
		private void OnDestroy()
		{
			this.ResetPlayer();
		}

		// Token: 0x040000EC RID: 236
		public TutorialTriggerArea.RequiredMobsInfo[] requiredMobs;

		// Token: 0x040000ED RID: 237
		public float requiredMobsSpawningDelay = 1.5f;

		// Token: 0x040000EE RID: 238
		public int[] playerMobsForbiddenAbilitySlots;

		// Token: 0x040000EF RID: 239
		public bool killPlayerSquad;

		// Token: 0x040000F0 RID: 240
		public bool restorePlayerHealth;

		// Token: 0x040000F1 RID: 241
		[SerializeField]
		private TutorialTriggerArea.Event _playerEntered;

		// Token: 0x040000F2 RID: 242
		[SerializeField]
		private TutorialTriggerArea.Event _playerExited;

		// Token: 0x040000F3 RID: 243
		[SerializeField]
		private TutorialTriggerArea.Event _necromancyUsedByPlayer;

		// Token: 0x040000F4 RID: 244
		private PlayerBehaviour currentPlayer;

		// Token: 0x040000F5 RID: 245
		private Coroutine mobsAdditionCoroutine;

		// Token: 0x0200040C RID: 1036
		[Serializable]
		public sealed class Event : UnityEvent<TutorialTriggerArea>
		{
		}

		// Token: 0x0200040D RID: 1037
		[Serializable]
		public struct RequiredMobsInfo
		{
			// Token: 0x06002245 RID: 8773 RVA: 0x0006A7D8 File Offset: 0x000689D8
			public int GetSpawningCount(GameMobsGroupControllerBase playerGroup)
			{
				IReadOnlyList<BaseGameMob> mobs = playerGroup.Mobs;
				int num = 0;
				for (int i = 0; i < mobs.Count; i++)
				{
					MobBehaviour mobBehaviour = mobs[i] as MobBehaviour;
					if (mobBehaviour != null && mobBehaviour.ObjectID == this.mobsID && ++num == this.count)
					{
						return 0;
					}
				}
				return this.count - num;
			}

			// Token: 0x040015AE RID: 5550
			[EnumPopup]
			public MobBehaviour.ID mobsID;

			// Token: 0x040015AF RID: 5551
			public int count;
		}
	}
}
