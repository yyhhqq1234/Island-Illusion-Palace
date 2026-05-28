using System;
using Game.Damage;
using Game.Utility;
using UnityEngine;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001D6 RID: 470
	public sealed class GameMobTargetSelector : TargetSelector<BaseGameMob>
	{
		// Token: 0x06000F03 RID: 3843 RVA: 0x0002FB14 File Offset: 0x0002DD14
		private static float GetMaxTargetHPCapacityEstimation(BaseGameMob newTarget)
		{
			IDamageable hitPointsController = newTarget.HitPointsController;
			if (hitPointsController == null)
			{
				return 0f;
			}
			return hitPointsController.MaxHitPoints / 10000f;
		}

		// Token: 0x06000F04 RID: 3844 RVA: 0x0002FB3D File Offset: 0x0002DD3D
		private static float GetMaxNormalizedTargetHealthEstimation(BaseGameMob newTarget)
		{
			return newTarget.HitPointsController.GetNormalizedHitPoints();
		}

		// Token: 0x06000F05 RID: 3845 RVA: 0x0002FB4A File Offset: 0x0002DD4A
		private static float GetRandomTargetEstimation(BaseGameMob newTarget)
		{
			return UnityEngine.Random.value;
		}

		// Token: 0x06000F06 RID: 3846 RVA: 0x0002FB51 File Offset: 0x0002DD51
		public static bool IsMinorTarget(BaseGameMob newTarget, out float minorTargetPriority)
		{
			if (newTarget.isMinorAttackTarget)
			{
				minorTargetPriority = 0.01f;
				return true;
			}
			minorTargetPriority = 0f;
			return false;
		}

		// Token: 0x06000F07 RID: 3847 RVA: 0x0002FB6C File Offset: 0x0002DD6C
		public static TargetSelector<BaseGameMob>.TargetPriorityEstimation GetAttackSpaceTargetPriority(BaseGameMob newTarget)
		{
			float multiplier;
			if (GameMobTargetSelector.IsMinorTarget(newTarget, out multiplier))
			{
				return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(0f, multiplier);
			}
			int count = newTarget.CurrentAttackers.Count;
			return new TargetSelector<BaseGameMob>.TargetPriorityEstimation((count < 10) ? ((10f - (float)count) * 2f) : 0f);
		}

		// Token: 0x06000F08 RID: 3848 RVA: 0x0002FBBA File Offset: 0x0002DDBA
		private static TargetSelector<BaseGameMob>.TargetPriorityEstimation GetPlayerTargetPriority(BaseGameMob newTarget)
		{
			return new TargetSelector<BaseGameMob>.TargetPriorityEstimation((newTarget is PlayerBehaviour) ? 1f : 0f);
		}

		// Token: 0x06000F09 RID: 3849 RVA: 0x0002FBD8 File Offset: 0x0002DDD8
		private static TargetSelector<BaseGameMob>.TargetPriorityEstimation GetMinorTargetPriority(BaseGameMob newTarget)
		{
			float multiplier;
			if (GameMobTargetSelector.IsMinorTarget(newTarget, out multiplier))
			{
				return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(0f, multiplier);
			}
			return TargetSelector<BaseGameMob>.TargetPriorityEstimation.Neutral;
		}

		// Token: 0x06000F0A RID: 3850 RVA: 0x0002FC00 File Offset: 0x0002DE00
		private static TargetSelector<BaseGameMob>.TargetPriorityEstimation GetMinHPCapacityTargetPriority(BaseGameMob newTarget)
		{
			float multiplier;
			if (GameMobTargetSelector.IsMinorTarget(newTarget, out multiplier))
			{
				return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(0f, multiplier);
			}
			IDamageable hitPointsController = newTarget.HitPointsController;
			if (hitPointsController == null)
			{
				return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(0f);
			}
			return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(Mathf.Max(1f, 10000f - hitPointsController.InitialHitPoints));
		}

		// Token: 0x170002F5 RID: 757
		// (get) Token: 0x06000F0B RID: 3851 RVA: 0x0002FC53 File Offset: 0x0002DE53
		// (set) Token: 0x06000F0C RID: 3852 RVA: 0x0002FC5B File Offset: 0x0002DE5B
		public GameMobTargetSelector.SelectionMethod TargetSelectionMethod
		{
			get
			{
				return this.targetSelectionMethod;
			}
			set
			{
				this.SetTargetsEstimationParams(value, this.priorityTargetSelector);
			}
		}

		// Token: 0x170002F6 RID: 758
		// (get) Token: 0x06000F0D RID: 3853 RVA: 0x0002FC6A File Offset: 0x0002DE6A
		// (set) Token: 0x06000F0E RID: 3854 RVA: 0x0002FC72 File Offset: 0x0002DE72
		public GameMobTargetSelector.PrioritySelector PriorityTargetSelector
		{
			get
			{
				return this.priorityTargetSelector;
			}
			set
			{
				this.SetTargetsEstimationParams(this.targetSelectionMethod, value);
			}
		}

		// Token: 0x170002F7 RID: 759
		// (get) Token: 0x06000F0F RID: 3855 RVA: 0x0002FC81 File Offset: 0x0002DE81
		public bool HasPriorityTargetSelector
		{
			get
			{
				return this.priorityTargetSelector != GameMobTargetSelector.PrioritySelector.Default;
			}
		}

		// Token: 0x170002F8 RID: 760
		// (get) Token: 0x06000F10 RID: 3856 RVA: 0x0002FC8F File Offset: 0x0002DE8F
		public bool IsDefaultTargetSelector
		{
			get
			{
				return this.targetSelectionMethod == GameMobTargetSelector.SelectionMethod.Closest && this.priorityTargetSelector == GameMobTargetSelector.PrioritySelector.Default;
			}
		}

		// Token: 0x06000F11 RID: 3857 RVA: 0x0002FCA4 File Offset: 0x0002DEA4
		private float GetFarthestTargetEstimation(BaseGameMob newTarget)
		{
			if (this.targetSelectionPoint == null)
			{
				Debug.LogWarning("Target Selection Position is undefined.");
				return 0f;
			}
			if (this.targetSelectionRadius > 0f)
			{
				float num = (newTarget.Position - this.targetSelectionPoint.Value).SqrMagnitude() / (this.targetSelectionRadius * this.targetSelectionRadius);
				if (num > 1f)
				{
					num = 1f;
				}
				return num;
			}
			return 0f;
		}

		// Token: 0x06000F12 RID: 3858 RVA: 0x0002FD1D File Offset: 0x0002DF1D
		private TargetSelector<BaseGameMob>.TargetPriorityEstimation GetClosestTargetPriority(BaseGameMob newTarget)
		{
			return new TargetSelector<BaseGameMob>.TargetPriorityEstimation(Mathf.Floor((1f - this.GetFarthestTargetEstimation(newTarget)) * 5f) * 2f);
		}

		// Token: 0x06000F13 RID: 3859 RVA: 0x0002FD44 File Offset: 0x0002DF44
		private TargetSelector<BaseGameMob>.TargetsEstimator GetTargetsEstimator(GameMobTargetSelector.SelectionMethod selectionMethod, GameMobTargetSelector.PrioritySelector prioritySelector)
		{
			Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation> priorityEstimationFunc;
			switch (prioritySelector)
			{
			case GameMobTargetSelector.PrioritySelector.MaxAttackSpace:
				priorityEstimationFunc = new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(GameMobTargetSelector.GetAttackSpaceTargetPriority);
				break;
			case GameMobTargetSelector.PrioritySelector.Player:
				priorityEstimationFunc = new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(GameMobTargetSelector.GetPlayerTargetPriority);
				break;
			case GameMobTargetSelector.PrioritySelector.Closest:
				priorityEstimationFunc = new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(this.GetClosestTargetPriority);
				break;
			case GameMobTargetSelector.PrioritySelector.MinHealthCapacity:
				priorityEstimationFunc = new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(GameMobTargetSelector.GetMinHPCapacityTargetPriority);
				break;
			default:
				priorityEstimationFunc = new Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation>(GameMobTargetSelector.GetMinorTargetPriority);
				break;
			}
			switch (selectionMethod)
			{
			case GameMobTargetSelector.SelectionMethod.Closest:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(this.GetFarthestTargetEstimation), priorityEstimationFunc, true);
			case GameMobTargetSelector.SelectionMethod.Farthest:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(this.GetFarthestTargetEstimation), priorityEstimationFunc, false);
			case GameMobTargetSelector.SelectionMethod.MinHealthCapacity:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(GameMobTargetSelector.GetMaxTargetHPCapacityEstimation), priorityEstimationFunc, true);
			case GameMobTargetSelector.SelectionMethod.MaxHealthCapacity:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(GameMobTargetSelector.GetMaxTargetHPCapacityEstimation), priorityEstimationFunc, false);
			case GameMobTargetSelector.SelectionMethod.MinCurrentHealth:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(GameMobTargetSelector.GetMaxNormalizedTargetHealthEstimation), priorityEstimationFunc, true);
			case GameMobTargetSelector.SelectionMethod.MaxCurrentHealth:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(GameMobTargetSelector.GetMaxNormalizedTargetHealthEstimation), priorityEstimationFunc, false);
			case GameMobTargetSelector.SelectionMethod.RandomTarget:
				return new TargetSelector<BaseGameMob>.TargetsEstimator(new Func<BaseGameMob, float>(GameMobTargetSelector.GetRandomTargetEstimation), priorityEstimationFunc, false);
			default:
				return default(TargetSelector<BaseGameMob>.TargetsEstimator);
			}
		}

		// Token: 0x06000F14 RID: 3860 RVA: 0x0002FE6E File Offset: 0x0002E06E
		private void SetTargetsEstimationParams(GameMobTargetSelector.SelectionMethod selectionMethod, GameMobTargetSelector.PrioritySelector prioritySelector, bool force)
		{
			if (!force && this.targetSelectionMethod == selectionMethod && this.priorityTargetSelector == prioritySelector)
			{
				return;
			}
			base.CurrentTargetsEstimator = this.GetTargetsEstimator(selectionMethod, prioritySelector);
			this.targetSelectionMethod = selectionMethod;
			this.priorityTargetSelector = prioritySelector;
		}

		// Token: 0x06000F15 RID: 3861 RVA: 0x0002FEA4 File Offset: 0x0002E0A4
		public GameMobTargetSelector(GameMobTargetSelector.SelectionMethod targetSelectionMethod, GameMobTargetSelector.PrioritySelector targetPrioritySelector) : base(default(TargetSelector<BaseGameMob>.TargetsEstimator))
		{
			this.SetTargetsEstimationParams(targetSelectionMethod, targetPrioritySelector, true);
		}

		// Token: 0x06000F16 RID: 3862 RVA: 0x0002FEC9 File Offset: 0x0002E0C9
		public GameMobTargetSelector() : this(GameMobTargetSelector.SelectionMethod.Closest, GameMobTargetSelector.PrioritySelector.Default)
		{
		}

		// Token: 0x06000F17 RID: 3863 RVA: 0x0002FED4 File Offset: 0x0002E0D4
		public bool IsTargetInRange(BaseGameMob target)
		{
			if (this.targetSelectionPoint == null || this.targetSelectionRadius <= 0f)
			{
				return true;
			}
			Vector2 position = target.Position;
			Vector2 vector = new Vector2
			{
				x = position.x - this.targetSelectionPoint.Value.x,
				y = position.y - this.targetSelectionPoint.Value.y
			};
			return vector.x * vector.x + vector.y * vector.y < this.targetSelectionRadius * this.targetSelectionRadius;
		}

		// Token: 0x06000F18 RID: 3864 RVA: 0x0002FF74 File Offset: 0x0002E174
		public bool IsValidTarget(BaseGameMob target, bool checkRange)
		{
			return base.IsValidTarget(target) && (this.skipTargetInRangeCheck || !checkRange || this.IsTargetInRange(target));
		}

		// Token: 0x06000F19 RID: 3865 RVA: 0x0002FF95 File Offset: 0x0002E195
		public override bool IsValidTarget(BaseGameMob target)
		{
			return this.IsValidTarget(target, true);
		}

		// Token: 0x06000F1A RID: 3866 RVA: 0x0002FF9F File Offset: 0x0002E19F
		public void SetTargetsEstimationParams(GameMobTargetSelector.SelectionMethod selectionMethod, GameMobTargetSelector.PrioritySelector prioritySelector)
		{
			this.SetTargetsEstimationParams(selectionMethod, prioritySelector, false);
		}

		// Token: 0x06000F1B RID: 3867 RVA: 0x0002FFAA File Offset: 0x0002E1AA
		public void SetCustomTargetsEstimation(Func<BaseGameMob, float> estimationFunc, bool isMinValueEstimation)
		{
			if (estimationFunc == null)
			{
				return;
			}
			base.CurrentTargetsEstimator = new TargetSelector<BaseGameMob>.TargetsEstimator(estimationFunc, base.CurrentTargetsEstimator.PriorityEstimationFunc, isMinValueEstimation);
			this.targetSelectionMethod = GameMobTargetSelector.SelectionMethod.Custom;
		}

		// Token: 0x06000F1C RID: 3868 RVA: 0x0002FFD0 File Offset: 0x0002E1D0
		public void SetCustomTargetsPriorityEstimation(Func<BaseGameMob, TargetSelector<BaseGameMob>.TargetPriorityEstimation> priorityEstimationFunc)
		{
			if (priorityEstimationFunc == null)
			{
				return;
			}
			TargetSelector<BaseGameMob>.TargetsEstimator currentTargetsEstimator = base.CurrentTargetsEstimator;
			base.CurrentTargetsEstimator = new TargetSelector<BaseGameMob>.TargetsEstimator(currentTargetsEstimator.EstimationFunc, priorityEstimationFunc, currentTargetsEstimator.IsMinValueEstimator);
			this.priorityTargetSelector = GameMobTargetSelector.PrioritySelector.Custom;
		}

		// Token: 0x040008E2 RID: 2274
		private const float MaxMobHealth = 10000f;

		// Token: 0x040008E3 RID: 2275
		public Vector2? targetSelectionPoint;

		// Token: 0x040008E4 RID: 2276
		public float targetSelectionRadius;

		// Token: 0x040008E5 RID: 2277
		public bool skipTargetInRangeCheck;

		// Token: 0x040008E6 RID: 2278
		private GameMobTargetSelector.SelectionMethod targetSelectionMethod;

		// Token: 0x040008E7 RID: 2279
		private GameMobTargetSelector.PrioritySelector priorityTargetSelector;

		// Token: 0x02000498 RID: 1176
		public enum SelectionMethod
		{
			// Token: 0x040018D8 RID: 6360
			None = -1,
			// Token: 0x040018D9 RID: 6361
			Closest,
			// Token: 0x040018DA RID: 6362
			Farthest,
			// Token: 0x040018DB RID: 6363
			MinHealthCapacity,
			// Token: 0x040018DC RID: 6364
			MaxHealthCapacity,
			// Token: 0x040018DD RID: 6365
			MinCurrentHealth,
			// Token: 0x040018DE RID: 6366
			MaxCurrentHealth,
			// Token: 0x040018DF RID: 6367
			RandomTarget,
			// Token: 0x040018E0 RID: 6368
			Custom
		}

		// Token: 0x02000499 RID: 1177
		public enum PrioritySelector
		{
			// Token: 0x040018E2 RID: 6370
			Default = -1,
			// Token: 0x040018E3 RID: 6371
			MaxAttackSpace,
			// Token: 0x040018E4 RID: 6372
			Player,
			// Token: 0x040018E5 RID: 6373
			Closest,
			// Token: 0x040018E6 RID: 6374
			MinHealthCapacity,
			// Token: 0x040018E7 RID: 6375
			Custom = 255
		}
	}
}
