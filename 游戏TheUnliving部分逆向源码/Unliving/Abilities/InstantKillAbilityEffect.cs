using System;
using System.Threading.Tasks;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000388 RID: 904
	public sealed class InstantKillAbilityEffect : AbilityEffectBase
	{
		// Token: 0x06001DD8 RID: 7640 RVA: 0x0005EAB4 File Offset: 0x0005CCB4
		private static async void StartDeferredDestruction(GameObject targetObject)
		{
			await Task.Yield();
			if (Application.isPlaying && targetObject != null)
			{
				UnityEngine.Object.Destroy(targetObject);
			}
		}

		// Token: 0x06001DD9 RID: 7641 RVA: 0x0005EAED File Offset: 0x0005CCED
		public InstantKillAbilityEffect()
		{
		}

		// Token: 0x06001DDA RID: 7642 RVA: 0x0005EAF5 File Offset: 0x0005CCF5
		public InstantKillAbilityEffect(InstantKillAbilityEffect effectPrototype)
		{
			this.totallyDestroyEffectTarget = effectPrototype.totallyDestroyEffectTarget;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001DDB RID: 7643 RVA: 0x0005EB10 File Offset: 0x0005CD10
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DDC RID: 7644 RVA: 0x0005EB17 File Offset: 0x0005CD17
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DDD RID: 7645 RVA: 0x0005EB1C File Offset: 0x0005CD1C
		protected override bool Use(Component effectTarget, float dt)
		{
			if (this.totallyDestroyEffectTarget)
			{
				if (this.forceUseOnOwner)
				{
					if (base.WasUsed())
					{
						return false;
					}
					InstantKillAbilityEffect.StartDeferredDestruction(effectTarget.gameObject);
				}
				else
				{
					UnityEngine.Object.Destroy(effectTarget.gameObject);
				}
				return true;
			}
			BaseAbility currentAbility = this.currentAbility;
			object obj = (currentAbility != null) ? currentAbility.Owner : null;
			BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
			bool result = false;
			if (baseGameMob != null)
			{
				result = !baseGameMob.IsKilled;
				baseGameMob.KillMob(obj);
			}
			else
			{
				IDamageable damageable = effectTarget.CastOrGetComponent<IDamageable>();
				if (damageable != null)
				{
					result = damageable.IsAlive;
					if (damageable != null)
					{
						damageable.ApplyLethalDamage(obj);
					}
				}
			}
			return result;
		}

		// Token: 0x06001DDE RID: 7646 RVA: 0x0005EBB1 File Offset: 0x0005CDB1
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new InstantKillAbilityEffect((InstantKillAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010D8 RID: 4312
		public bool totallyDestroyEffectTarget;
	}
}
