using System;
using Common;
using Common.Math.Gameplay;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Mobs;
using Unliving.Mobs.AbilityTriggers;
using Unliving.Mobs.Motion;

namespace Unliving.Abilities
{
	// Token: 0x020003B7 RID: 951
	[CreateAssetMenu(fileName = "TargetedDashAbility", menuName = "Abilities/Targeted Dash Ability")]
	public sealed class TargetedDashAbility : BaseAbility, IDamageSender, IGameMobJumpMotion, IInterruptableActionWithProgress, IInterruptableAction, IProgressBasedAction
	{
		// Token: 0x06001FDB RID: 8155 RVA: 0x00064450 File Offset: 0x00062650
		private static void BakePossibleDashDirections()
		{
			if (TargetedDashAbility.possibleDashDirections == null)
			{
				TargetedDashAbility.possibleDashDirections = new Vector2[9];
				float num = 6.2831855f / 8f;
				for (int i = 1; i < TargetedDashAbility.possibleDashDirections.Length; i++)
				{
					float f = (float)i * num;
					TargetedDashAbility.possibleDashDirections[i] = new Vector2
					{
						x = Mathf.Cos(f),
						y = Mathf.Sin(f)
					};
				}
			}
		}

		// Token: 0x17000677 RID: 1655
		// (get) Token: 0x06001FDC RID: 8156 RVA: 0x000644C1 File Offset: 0x000626C1
		// (set) Token: 0x06001FDD RID: 8157 RVA: 0x000644C9 File Offset: 0x000626C9
		public override int ID { get; set; }

		// Token: 0x17000678 RID: 1656
		// (get) Token: 0x06001FDE RID: 8158 RVA: 0x000644D2 File Offset: 0x000626D2
		// (set) Token: 0x06001FDF RID: 8159 RVA: 0x000644DA File Offset: 0x000626DA
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

		// Token: 0x17000679 RID: 1657
		// (get) Token: 0x06001FE0 RID: 8160 RVA: 0x000644E3 File Offset: 0x000626E3
		public override bool IsTargetedAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700067A RID: 1658
		// (get) Token: 0x06001FE1 RID: 8161 RVA: 0x000644E6 File Offset: 0x000626E6
		public override bool IsObjectTargetRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700067B RID: 1659
		// (get) Token: 0x06001FE2 RID: 8162 RVA: 0x000644E9 File Offset: 0x000626E9
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700067C RID: 1660
		// (get) Token: 0x06001FE3 RID: 8163 RVA: 0x000644EC File Offset: 0x000626EC
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700067D RID: 1661
		// (get) Token: 0x06001FE4 RID: 8164 RVA: 0x000644EF File Offset: 0x000626EF
		public override bool IsContinuous
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700067E RID: 1662
		// (get) Token: 0x06001FE5 RID: 8165 RVA: 0x000644F2 File Offset: 0x000626F2
		public override bool CanBeUsed
		{
			get
			{
				return this.movableOwner != null;
			}
		}

		// Token: 0x1700067F RID: 1663
		// (get) Token: 0x06001FE6 RID: 8166 RVA: 0x00064500 File Offset: 0x00062700
		public override bool InUse
		{
			get
			{
				return !this.isPerformed;
			}
		}

		// Token: 0x17000680 RID: 1664
		// (get) Token: 0x06001FE7 RID: 8167 RVA: 0x0006450B File Offset: 0x0006270B
		// (set) Token: 0x06001FE8 RID: 8168 RVA: 0x00064512 File Offset: 0x00062712
		public override float UsingDuration
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		// Token: 0x17000681 RID: 1665
		// (get) Token: 0x06001FE9 RID: 8169 RVA: 0x00064514 File Offset: 0x00062714
		// (set) Token: 0x06001FEA RID: 8170 RVA: 0x0006451B File Offset: 0x0006271B
		DamageGenerator IDamageSender.DamageGenerator
		{
			get
			{
				return DamageGenerator.Empty;
			}
			set
			{
			}
		}

		// Token: 0x17000682 RID: 1666
		// (get) Token: 0x06001FEB RID: 8171 RVA: 0x0006451D File Offset: 0x0006271D
		float IGameMobJumpMotion.MaxHeight
		{
			get
			{
				if (!this.isJumpMotion)
				{
					return 0f;
				}
				return FallMotion.GetMaxHeight(this.dashImpulse, 40f);
			}
		}

		// Token: 0x17000683 RID: 1667
		// (get) Token: 0x06001FEC RID: 8172 RVA: 0x0006453D File Offset: 0x0006273D
		float IGameMobJumpMotion.MaxDistance
		{
			get
			{
				if (this.oppositeDashDistance == 0f)
				{
					return base.Range;
				}
				return Mathf.Abs(this.oppositeDashDistance);
			}
		}

		// Token: 0x17000684 RID: 1668
		// (get) Token: 0x06001FED RID: 8173 RVA: 0x0006455E File Offset: 0x0006275E
		float IProgressBasedAction.CurrentProgress
		{
			get
			{
				if (this.currentMotion == null)
				{
					return 0f;
				}
				return this.currentMotion.CurrentProgress;
			}
		}

		// Token: 0x06001FEE RID: 8174 RVA: 0x0006457C File Offset: 0x0006277C
		private float GetDashDistance()
		{
			if (this.mobsCountDependentDistances.Length != 0)
			{
				if (this.mobsCountTrigger == null)
				{
					if (base.ParentAbility != null)
					{
						this.mobsCountTrigger = base.ParentAbility.GetExtension<IndividualUsingAbilityTrigger>();
					}
					else
					{
						this.mobsCountTrigger = base.GetExtension<IndividualUsingAbilityTrigger>();
					}
					Array.Sort<TargetedDashAbility.CountDependentDistance>(this.mobsCountDependentDistances, new Comparison<TargetedDashAbility.CountDependentDistance>(TargetedDashAbility.CountDependentDistance.SortingComparison));
				}
				int currentAbilitiesCount = this.mobsCountTrigger.CurrentAbilitiesCount;
				float num = 0f;
				for (int i = 0; i < this.mobsCountDependentDistances.Length; i++)
				{
					TargetedDashAbility.CountDependentDistance countDependentDistance = this.mobsCountDependentDistances[i];
					if (countDependentDistance.targetMobsCount > currentAbilitiesCount)
					{
						break;
					}
					num = countDependentDistance.dashDistance;
				}
				if (num != 0f)
				{
					return num;
				}
			}
			return this.oppositeDashDistance;
		}

		// Token: 0x06001FEF RID: 8175 RVA: 0x0006463C File Offset: 0x0006283C
		private bool TryFindDashDirection(Vector2 ownerPosition, float minDistance, ref float distance, ref Vector2 direction, int maxAttempts = -1)
		{
			TargetedDashAbility.possibleDashDirections[0] = direction;
			int num = (maxAttempts > 0) ? Mathf.Min(maxAttempts, TargetedDashAbility.possibleDashDirections.Length) : TargetedDashAbility.possibleDashDirections.Length;
			for (int i = 0; i < num; i++)
			{
				Vector2 vector = TargetedDashAbility.possibleDashDirections[i];
				if (i == 0 || direction.x * vector.x + direction.y * vector.y >= 0f)
				{
					float num2 = distance;
					RaycastHit2D raycastHit2D = Physics2D.Raycast(ownerPosition, vector, distance, this.obstacleLayers);
					NavMeshHit navMeshHit;
					if (!this.ignoreNavMesh && NavMesh.Raycast(ownerPosition, ownerPosition + direction * distance, out navMeshHit, -1) && navMeshHit.distance < raycastHit2D.distance)
					{
						raycastHit2D.distance = navMeshHit.distance;
					}
					if (raycastHit2D.distance > 0f)
					{
						num2 = raycastHit2D.distance - this.abilityOwnerRadius;
					}
					if (this.abilityOwnerRadius > 0f)
					{
						Vector2 vector2 = ownerPosition + vector * num2;
						NavMeshHit navMeshHit2;
						if ((this.finalPositionObstacleLayers != 0 && Physics2D.OverlapCircle(vector2, this.abilityOwnerRadius, this.finalPositionObstacleLayers)) || !NavMesh.SamplePosition(vector2, out navMeshHit2, this.abilityOwnerRadius, -1))
						{
							goto IL_161;
						}
					}
					if (num2 > minDistance)
					{
						distance = num2;
						direction = vector;
						return true;
					}
				}
				IL_161:;
			}
			return false;
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x000647B6 File Offset: 0x000629B6
		protected override BaseAbility.ActivationErrorType GetActivationError(BaseAbility.UsingArgs usingArgs, bool isAutoUsePhase, ref object errorSource)
		{
			if (this.movableOwner != null && this.movableOwner.MotionController.IsFullyStatic)
			{
				return BaseAbility.ActivationErrorType.Internal;
			}
			return base.GetActivationError(usingArgs, isAutoUsePhase, ref errorSource);
		}

		// Token: 0x06001FF1 RID: 8177 RVA: 0x000647E4 File Offset: 0x000629E4
		protected override void PerformAbility(BaseAbility.UsingArgs usingArgs)
		{
			Vector2 hitColliderCenter = this.movableOwner.HitColliderCenter;
			this.abilityOwnerRadius = this.movableOwner.Radius;
			Vector2 vector = usingArgs.targetPosition;
			IBoundingCircle boundingCircle = usingArgs.targetObject as IBoundingCircle;
			this.dashTargetRadius = ((boundingCircle != null) ? boundingCircle.Radius : 0f);
			float dashDistance = this.GetDashDistance();
			bool flag = dashDistance <= 0f;
			Vector2 a = flag ? (vector - hitColliderCenter) : (hitColliderCenter - vector);
			float magnitude = a.magnitude;
			float num = flag ? magnitude : dashDistance;
			if (num < 1E-05f)
			{
				return;
			}
			a /= magnitude;
			float minDistance;
			if (flag)
			{
				if (dashDistance != 0f && -dashDistance < magnitude)
				{
					num = -dashDistance;
				}
				float num2 = this.abilityOwnerRadius + this.dashTargetRadius;
				if (this.dashBehindTarget)
				{
					num += num2 + 0.1f;
					minDistance = num2 * 2f;
				}
				else
				{
					num -= num2 + 0.1f;
					minDistance = num2;
				}
			}
			else
			{
				minDistance = this.dashTargetRadius + this.abilityOwnerRadius * 2f;
			}
			this.isPerformed = true;
			if (this.TryFindDashDirection(hitColliderCenter, minDistance, ref num, ref a, flag ? 1 : -1))
			{
				GameMobMotionControllerBase motionController = this.movableOwner.MotionController;
				Vector2 targetPoint = this.movableOwner.Position + a * num;
				bool flag2;
				if (this.isJumpMotion)
				{
					float num3 = (dashDistance != 0f) ? Mathf.Abs(dashDistance) : base.Range;
					float num4 = Mathf.Clamp01(num / num3 * 1.5f);
					flag2 = (motionController.JumpToPoint(targetPoint, this.dashImpulse * num4, 0f, false, this) != null);
				}
				else
				{
					flag2 = (motionController.MoveToPoint(targetPoint, this.dashImpulse, false, this) != null);
				}
				if (flag2)
				{
					this.isPerformed = false;
					this.currentMotion = motionController.CurrentKinematicMotion;
					motionController.KinematicMotionCompleted += this.OnMobKinematicMotionCompleted;
				}
			}
			base.SetFullyUsed();
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x000649EC File Offset: 0x00062BEC
		protected override void OnOwnerChanged(object lastOwner, object newOwner)
		{
			this.movableOwner = null;
			base.OnOwnerChanged(lastOwner, newOwner);
			if (this.movableOwner != null)
			{
				this.movableOwner.MotionController.KinematicMotionCompleted -= this.OnMobKinematicMotionCompleted;
			}
			BaseGameMob baseGameMob = newOwner as BaseGameMob;
			if (baseGameMob != null && baseGameMob.MotionController != null)
			{
				this.movableOwner = baseGameMob;
				return;
			}
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x00064A4E File Offset: 0x00062C4E
		protected override void OnPreparing(BaseAbility.UsingArgs usingArgs)
		{
			this.isPerformed = false;
			base.OnPreparing(usingArgs);
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x00064A5E File Offset: 0x00062C5E
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			GameMobKinematicMotionBase gameMobKinematicMotionBase = this.currentMotion;
			if (gameMobKinematicMotionBase != null)
			{
				gameMobKinematicMotionBase.Interrupt();
			}
			this.currentMotion = null;
			this.isPerformed = true;
			base.OnCompleted(usingArgs);
		}

		// Token: 0x06001FF5 RID: 8181 RVA: 0x00064A86 File Offset: 0x00062C86
		private void OnMobKinematicMotionCompleted(GameMobKinematicMotionBase motion)
		{
			this.currentMotion = null;
			this.isPerformed = true;
			this.movableOwner.MotionController.KinematicMotionCompleted -= this.OnMobKinematicMotionCompleted;
		}

		// Token: 0x06001FF6 RID: 8182 RVA: 0x00064AB2 File Offset: 0x00062CB2
		protected override void OnInitialize(object context)
		{
			this.isPerformed = true;
			base.OnInitialize(context);
			TargetedDashAbility.BakePossibleDashDirections();
		}

		// Token: 0x06001FF7 RID: 8183 RVA: 0x00064AC7 File Offset: 0x00062CC7
		bool IInterruptableAction.TryInterrupt(bool force)
		{
			if (force && this.currentMotion != null)
			{
				base.Complete();
				return true;
			}
			return false;
		}

		// Token: 0x0400141D RID: 5149
		private const int MaxDashSearchingDirections = 8;

		// Token: 0x0400141E RID: 5150
		private static Vector2[] possibleDashDirections;

		// Token: 0x04001420 RID: 5152
		public AbilityTypes abilityType;

		// Token: 0x04001421 RID: 5153
		[Tooltip("При значениях > 0 дэш будет происходить от цели. Иначе - к ней. Если равно 0, то дэш к цели не будет ограничен по расстоянию.")]
		public float oppositeDashDistance = 5f;

		// Token: 0x04001422 RID: 5154
		[HideInInspector]
		public float dashSpeed = 25f;

		// Token: 0x04001423 RID: 5155
		[HideInInspector]
		public float dashHeight;

		// Token: 0x04001424 RID: 5156
		public float dashImpulse = 10f;

		// Token: 0x04001425 RID: 5157
		public bool isJumpMotion = true;

		// Token: 0x04001426 RID: 5158
		[Tooltip("Пытаться прыгнуть за цель в режиме дэша к цели?")]
		public bool dashBehindTarget;

		// Token: 0x04001427 RID: 5159
		[Tooltip("Слои объектов, блокирующих дэш при построении траектории.")]
		public LayerMask obstacleLayers;

		// Token: 0x04001428 RID: 5160
		[Tooltip("Слои объектов для проверки препятствий в конечной точке, т.е проверка хватит ли места для размещения выполнившего дэш юнита.")]
		public LayerMask finalPositionObstacleLayers;

		// Token: 0x04001429 RID: 5161
		[Tooltip("Если опция активна, то дырки в навмеше не будут учитываться при построении траектории дэша.")]
		public bool ignoreNavMesh;

		// Token: 0x0400142A RID: 5162
		[Space]
		public TargetedDashAbility.CountDependentDistance[] mobsCountDependentDistances;

		// Token: 0x0400142B RID: 5163
		private BaseGameMob movableOwner;

		// Token: 0x0400142C RID: 5164
		private IndividualUsingAbilityTrigger mobsCountTrigger;

		// Token: 0x0400142D RID: 5165
		private float abilityOwnerRadius;

		// Token: 0x0400142E RID: 5166
		private float dashTargetRadius;

		// Token: 0x0400142F RID: 5167
		private GameMobKinematicMotionBase currentMotion;

		// Token: 0x04001430 RID: 5168
		private bool isPerformed;

		// Token: 0x02000581 RID: 1409
		[Serializable]
		public struct CountDependentDistance
		{
			// Token: 0x06002754 RID: 10068 RVA: 0x0007AD12 File Offset: 0x00078F12
			public static int SortingComparison(TargetedDashAbility.CountDependentDistance d0, TargetedDashAbility.CountDependentDistance d1)
			{
				return d0.targetMobsCount.CompareTo(d1.targetMobsCount);
			}

			// Token: 0x04001C8C RID: 7308
			public int targetMobsCount;

			// Token: 0x04001C8D RID: 7309
			public float dashDistance;
		}
	}
}
