using System;
using System.Collections;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Abilities
{
	// Token: 0x02000390 RID: 912
	[Serializable]
	public sealed class NecromancyAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001E14 RID: 7700 RVA: 0x0005F34E File Offset: 0x0005D54E
		private IEnumerator RevivingRoutine(IRevivableGameMob deadMob, float delay)
		{
			yield return (delay > 0f) ? new WaitForSeconds(delay) : null;
			if (!deadMob.IsNull())
			{
				BaseGameMob baseGameMob = deadMob.Revive(this.reviver, this, false);
				if (baseGameMob != null)
				{
					MonoBehaviour monoBehaviour = (MonoBehaviour)deadMob;
					base.NotifyEffectUsed(monoBehaviour, 0f);
					this.reviver.OnMobRevived(baseGameMob, deadMob);
					UnityEngine.Object.Destroy(monoBehaviour.gameObject);
				}
			}
			yield break;
		}

		// Token: 0x06001E15 RID: 7701 RVA: 0x0005F36C File Offset: 0x0005D56C
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.remainingMobsCount <= 0)
			{
				return false;
			}
			IRevivableGameMob revivableGameMob = effectTarget.CastOrGetComponent<IRevivableGameMob>();
			if (this.IsUnrevivableTarget(revivableGameMob))
			{
				return false;
			}
			float delay = (revivableGameMob is BaseGameMob) ? UnityEngine.Random.Range(this.minCorpseEffectDelay, this.maxCorpseEffectDelay) : UnityEngine.Random.Range(this.minEffectDelay, this.maxEffectDelay);
			this.reviver.StartCoroutine(this.RevivingRoutine(revivableGameMob, delay));
			this.remainingMobsCount--;
			return true;
		}

		// Token: 0x06001E16 RID: 7702 RVA: 0x0005F3E9 File Offset: 0x0005D5E9
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001E17 RID: 7703 RVA: 0x0005F3F0 File Offset: 0x0005D5F0
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x0005F3F2 File Offset: 0x0005D5F2
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new NecromancyAbilityEffect((NecromancyAbilityEffect)originalBaseEffect);
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x0005F3FF File Offset: 0x0005D5FF
		public NecromancyAbilityEffect()
		{
		}

		// Token: 0x06001E1A RID: 7706 RVA: 0x0005F41C File Offset: 0x0005D61C
		public NecromancyAbilityEffect(NecromancyAbilityEffect effectPrototype)
		{
			base.CopyCommonParameters(effectPrototype);
			this.minEffectDelay = effectPrototype.minEffectDelay;
			this.maxEffectDelay = effectPrototype.maxEffectDelay;
			this.minCorpseEffectDelay = effectPrototype.minCorpseEffectDelay;
			this.maxCorpseEffectDelay = effectPrototype.maxCorpseEffectDelay;
			this.affectCorpsesOnly = effectPrototype.affectCorpsesOnly;
			this.canAffectIndividualMobs = effectPrototype.canAffectIndividualMobs;
		}

		// Token: 0x06001E1B RID: 7707 RVA: 0x0005F490 File Offset: 0x0005D690
		public bool IsUnrevivableTarget(IRevivableGameMob target)
		{
			bool flag = target is BaseGameMob;
			return target == null || !target.CanBeRevived(this.reviver, this) || (this.affectCorpsesOnly && !flag);
		}

		// Token: 0x06001E1C RID: 7708 RVA: 0x0005F4CC File Offset: 0x0005D6CC
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.reviver == null)
			{
				if (this.forceReviveToPlayerGroup)
				{
					GameBehaviourBase gameBehaviourBase = this.currentAbility.Owner as GameBehaviourBase;
					BaseGameMob baseGameMob;
					if (gameBehaviourBase == null)
					{
						baseGameMob = null;
					}
					else
					{
						IPlayerProvider playerProvider = gameBehaviourBase.CurrentGame.Services.Get<IPlayerProvider>();
						baseGameMob = ((playerProvider != null) ? playerProvider.CurrentPlayer : null);
					}
					this.reviver = baseGameMob;
					if (this.reviver == null)
					{
					}
				}
				else
				{
					this.reviver = this.currentAbility.Owner.CastOrGetComponent<BaseGameMob>();
				}
			}
			this.remainingMobsCount = int.MaxValue;
			if (this.maxReviverGroupSize > 0 && this.reviver.Group != null)
			{
				this.remainingMobsCount = this.maxReviverGroupSize - this.reviver.Group.Mobs.Count;
			}
			if (this.reviver != null)
			{
				base.Use(abilityUsingArgs, dt);
			}
		}

		// Token: 0x040010EA RID: 4330
		public float minEffectDelay;

		// Token: 0x040010EB RID: 4331
		public float maxEffectDelay = 0.5f;

		// Token: 0x040010EC RID: 4332
		public float minCorpseEffectDelay;

		// Token: 0x040010ED RID: 4333
		public float maxCorpseEffectDelay;

		// Token: 0x040010EE RID: 4334
		public bool affectCorpsesOnly;

		// Token: 0x040010EF RID: 4335
		public bool canAffectIndividualMobs;

		// Token: 0x040010F0 RID: 4336
		public bool forceReviveToPlayerGroup;

		// Token: 0x040010F1 RID: 4337
		public int maxReviverGroupSize = -1;

		// Token: 0x040010F2 RID: 4338
		private BaseGameMob reviver;

		// Token: 0x040010F3 RID: 4339
		private int remainingMobsCount;
	}
}
