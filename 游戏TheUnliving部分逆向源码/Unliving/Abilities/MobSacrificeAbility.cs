using System;
using System.Collections;
using Common;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Currencies;
using Unliving.Mobs;
using Unliving.Mobs.Motion;
using Unliving.Pickables;

namespace Unliving.Abilities
{
	// Token: 0x020003B1 RID: 945
	[CreateAssetMenu(fileName = "MobSacrificeAbility", menuName = "Abilities/Mob Sacrifice Ability")]
	public sealed class MobSacrificeAbility : BaseAbility
	{
		// Token: 0x1700063D RID: 1597
		// (get) Token: 0x06001F2B RID: 7979 RVA: 0x0006272D File Offset: 0x0006092D
		// (set) Token: 0x06001F2C RID: 7980 RVA: 0x00062735 File Offset: 0x00060935
		public override int ID { get; set; }

		// Token: 0x1700063E RID: 1598
		// (get) Token: 0x06001F2D RID: 7981 RVA: 0x0006273E File Offset: 0x0006093E
		// (set) Token: 0x06001F2E RID: 7982 RVA: 0x00062746 File Offset: 0x00060946
		public override int Type
		{
			get
			{
				return (int)this.abilityType;
			}
			set
			{
				this.abilityType = (AbilityTypes)value;
			}
		}

		// Token: 0x1700063F RID: 1599
		// (get) Token: 0x06001F2F RID: 7983 RVA: 0x0006274F File Offset: 0x0006094F
		public override bool IsTargetedAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000640 RID: 1600
		// (get) Token: 0x06001F30 RID: 7984 RVA: 0x00062752 File Offset: 0x00060952
		public override bool IsObjectTargetRequired
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17000641 RID: 1601
		// (get) Token: 0x06001F31 RID: 7985 RVA: 0x00062755 File Offset: 0x00060955
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000642 RID: 1602
		// (get) Token: 0x06001F32 RID: 7986 RVA: 0x00062758 File Offset: 0x00060958
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000643 RID: 1603
		// (get) Token: 0x06001F33 RID: 7987 RVA: 0x0006275B File Offset: 0x0006095B
		public override bool IsContinuous
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000644 RID: 1604
		// (get) Token: 0x06001F34 RID: 7988 RVA: 0x0006275E File Offset: 0x0006095E
		protected override bool ConsumeEnergyOnActivation
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x00062764 File Offset: 0x00060964
		public bool IsActivationEnergyAvailable(float mobActivationCost)
		{
			IAbilitiesEnergyProvider abilitiesEnergyProvider = base.OwnerBehaviour as IAbilitiesEnergyProvider;
			return abilitiesEnergyProvider != null && abilitiesEnergyProvider.HasEnoughEnergy(mobActivationCost);
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x0006278C File Offset: 0x0006098C
		private bool IsEnergyRestoringRequired(float mobActivationCost)
		{
			if (!this.restoreEnergyOnSacrifice)
			{
				return false;
			}
			if (mobActivationCost > 0f)
			{
				IAbilitiesEnergyProvider abilitiesEnergyProvider = base.OwnerBehaviour as IAbilitiesEnergyProvider;
				if (abilitiesEnergyProvider != null)
				{
					return !abilitiesEnergyProvider.HasEnoughEnergy(mobActivationCost);
				}
			}
			return false;
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x000627C6 File Offset: 0x000609C6
		private IEnumerator DeferredActivationRoutine(BaseGameMob targetMob, MobSacrificeAbility.Args activationArgs)
		{
			BaseGameMob sacrificer = (BaseGameMob)base.Owner;
			Vector2 position = targetMob.Position;
			Vector2 vector = activationArgs.targetActivationPoint;
			Vector2 b = (position - vector).normalized * targetMob.Radius;
			NavMeshHit navMeshHit;
			if (NavMesh.Raycast(position, vector, out navMeshHit, -1))
			{
				vector = navMeshHit.position + b;
			}
			int layerMask = this.sacrificeMotionObstacleLayers & ~(targetMob.LayerMask | sacrificer.LayerMask);
			RaycastHit2D raycastHit2D = Physics2D.Linecast(position, vector, layerMask);
			if (raycastHit2D.collider != null)
			{
				vector = raycastHit2D.point + b;
			}
			GameMobKinematicMotionBase gameMobKinematicMotionBase = targetMob.MotionController.MoveToPoint(vector, true, null);
			if (gameMobKinematicMotionBase != null)
			{
				yield return gameMobKinematicMotionBase;
			}
			targetMob.KillMob(sacrificer);
			yield break;
		}

		// Token: 0x06001F38 RID: 7992 RVA: 0x000627E4 File Offset: 0x000609E4
		protected override float GetCost(BaseAbility.UsingArgs usingArgs)
		{
			MobBehaviour mobBehaviour = usingArgs.targetObject.CastOrGetComponent<MobBehaviour>();
			float activationCost = this.GetActivationCost(mobBehaviour);
			if (!(mobBehaviour != null))
			{
				return activationCost;
			}
			float num = mobBehaviour.activationEnergyReturnAmount;
			if (num <= 0f)
			{
				num = activationCost;
			}
			if (!this.IsEnergyRestoringRequired(activationCost))
			{
				return activationCost;
			}
			return -num;
		}

		// Token: 0x06001F39 RID: 7993 RVA: 0x00062830 File Offset: 0x00060A30
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorData)
		{
			BaseGameMob baseGameMob = usingArgs.targetObject.CastOrGetComponent<BaseGameMob>();
			if (baseGameMob == null || !baseGameMob.CanBeSacrificed(base.OwnerBehaviour, true))
			{
				return BaseAbility.ActivationErrorType.UnallowedTarget;
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorData);
		}

		// Token: 0x06001F3A RID: 7994 RVA: 0x0006286C File Offset: 0x00060A6C
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			BaseGameMob baseGameMob = usingArgs.targetObject.CastOrGetComponent<BaseGameMob>();
			if (this.rewardAmountMultiplier > 0f)
			{
				MobBehaviour mobBehaviour = baseGameMob as MobBehaviour;
				if (mobBehaviour != null)
				{
					ICurrencyOperationPerformer currencyOperationPerformer = base.OwnerBehaviour as ICurrencyOperationPerformer;
					if (currencyOperationPerformer != null)
					{
						currencyOperationPerformer.PerformCurrencyOperation(this.CreateActivationRewardArgs(mobBehaviour));
					}
				}
			}
			if (this.energyRestoringAmount > 0f)
			{
				if (this.restoreEnergyPackPrefab != null)
				{
					UnityEngine.Object.Instantiate<VitalEnergyPack>(this.restoreEnergyPackPrefab, baseGameMob.Position, Quaternion.identity).Amount = this.energyRestoringAmount;
				}
				else
				{
					((IAbilitiesEnergyProvider)base.OwnerBehaviour).RestoreEnergy(this.energyRestoringAmount);
				}
				baseGameMob.KillMob(base.OwnerBehaviour);
				return;
			}
			BaseAbility baseAbility;
			MobActivationAbilityType mobActivationAbilityType;
			if (!baseGameMob.TryGetMobActivationAbility(out baseAbility, out mobActivationAbilityType))
			{
				return;
			}
			bool killMob = true;
			MobSacrificeAbility.Args args;
			if (usingArgs.TryGetAdditionalContext(out args))
			{
				if (baseAbility is ProjectileAbilityBase)
				{
					BaseAbility.UsingArgs usingArgs2 = usingArgs.Clone();
					usingArgs2.additionalContext = args.Clone();
					baseAbility.SetSpecificPostMortemUsingArgs(usingArgs2);
				}
				else if (baseGameMob.MotionController != null)
				{
					baseGameMob.StartCoroutine(this.DeferredActivationRoutine(baseGameMob, args));
					killMob = false;
				}
			}
			baseGameMob.Sacrifice(base.OwnerBehaviour, killMob, true);
		}

		// Token: 0x06001F3B RID: 7995 RVA: 0x00062990 File Offset: 0x00060B90
		public CurrencyOperationArgs CreateActivationRewardArgs(MobBehaviour targetMob)
		{
			float amount = targetMob.ActivationReward * this.rewardAmountMultiplier;
			return new CurrencyOperationArgs
			{
				amount = amount,
				currencyID = this.rewardCurrency,
				sender = targetMob.gameObject
			};
		}

		// Token: 0x06001F3C RID: 7996 RVA: 0x000629D6 File Offset: 0x00060BD6
		public float GetActivationCost(MobBehaviour targetMob)
		{
			if (targetMob == null || targetMob.ActivationCost <= 0f)
			{
				return base.Cost;
			}
			return targetMob.ActivationCost;
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x000629F5 File Offset: 0x00060BF5
		public bool IsActivationEnergyAvailable(MobBehaviour targetMob)
		{
			return this.IsActivationEnergyAvailable(this.GetActivationCost(targetMob));
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x00062A04 File Offset: 0x00060C04
		protected override void OnPrepared(BaseAbility.UsingArgs usingArgs)
		{
			float cost = this.GetCost(usingArgs);
			if (cost < 0f)
			{
				this.energyRestoringAmount = -cost;
			}
			base.OnPrepared(usingArgs);
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x00062A30 File Offset: 0x00060C30
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			this.energyRestoringAmount = 0f;
			base.OnCompleted(usingArgs);
		}

		// Token: 0x040013C9 RID: 5065
		public AbilityTypes abilityType;

		// Token: 0x040013CA RID: 5066
		public LayerMask sacrificeMotionObstacleLayers = -1;

		// Token: 0x040013CB RID: 5067
		public bool restoreEnergyOnSacrifice = true;

		// Token: 0x040013CC RID: 5068
		public VitalEnergyPack restoreEnergyPackPrefab;

		// Token: 0x040013CD RID: 5069
		public CurrencyID rewardCurrency;

		// Token: 0x040013CE RID: 5070
		public float rewardAmountMultiplier;

		// Token: 0x040013CF RID: 5071
		private float energyRestoringAmount;

		// Token: 0x0200057A RID: 1402
		public sealed class Args : ICloneable<MobSacrificeAbility.Args>
		{
			// Token: 0x06002734 RID: 10036 RVA: 0x0007A56B File Offset: 0x0007876B
			public MobSacrificeAbility.Args Clone()
			{
				return (MobSacrificeAbility.Args)base.MemberwiseClone();
			}

			// Token: 0x04001C68 RID: 7272
			public Vector2 targetActivationPoint;
		}
	}
}
