using System;
using Common.UnityExtensions;
using Game.Damage;
using Game.Utility;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001D0 RID: 464
	[DefaultExecutionOrder(0)]
	public sealed class GameMobAreaObserver : AreaWatcher<BaseGameMob>
	{
		// Token: 0x06000ECD RID: 3789 RVA: 0x0002EFA3 File Offset: 0x0002D1A3
		private static bool FullHealthValidator(BaseGameMob mob)
		{
			return mob.HitPointsController.HitPointsLack < 0.01f;
		}

		// Token: 0x06000ECE RID: 3790 RVA: 0x0002EFB7 File Offset: 0x0002D1B7
		private static bool NotFullHealthValidator(BaseGameMob mob)
		{
			return mob.HitPointsController.HitPointsLack > 0.01f;
		}

		// Token: 0x06000ECF RID: 3791 RVA: 0x0002EFCB File Offset: 0x0002D1CB
		private static bool DeathMobValidator(BaseGameMob mob)
		{
			return !mob.IsAlive();
		}

		// Token: 0x06000ED0 RID: 3792 RVA: 0x0002EFD8 File Offset: 0x0002D1D8
		private static int MinHealthObjectSelector(BaseGameMob obj0, BaseGameMob obj1)
		{
			IDamageable damageable = (obj0 != null) ? obj0.HitPointsController : null;
			IDamageable damageable2 = (obj1 != null) ? obj1.HitPointsController : null;
			if (!damageable.IsNull() && !damageable2.IsNull())
			{
				return damageable.CurrentHitPoints.CompareTo(damageable2.CurrentHitPoints);
			}
			return 0;
		}

		// Token: 0x06000ED1 RID: 3793 RVA: 0x0002F025 File Offset: 0x0002D225
		private static int MaxHealthObjectSelector(BaseGameMob obj0, BaseGameMob obj1)
		{
			return -GameMobAreaObserver.MinHealthObjectSelector(obj0, obj1);
		}

		// Token: 0x06000ED2 RID: 3794 RVA: 0x0002F030 File Offset: 0x0002D230
		private static int MinHealthRatioObjectSelector(BaseGameMob obj0, BaseGameMob obj1)
		{
			IDamageable hitPointsController = obj0.HitPointsController;
			IDamageable hitPointsController2 = obj1.HitPointsController;
			if (!hitPointsController.IsNull() && !hitPointsController2.IsNull())
			{
				return hitPointsController2.HitPointsLack.CompareTo(hitPointsController.HitPointsLack);
			}
			return 0;
		}

		// Token: 0x06000ED3 RID: 3795 RVA: 0x0002F071 File Offset: 0x0002D271
		private static int MaxHealthRatioObjectSelector(BaseGameMob obj0, BaseGameMob obj1)
		{
			return -GameMobAreaObserver.MinHealthRatioObjectSelector(obj0, obj1);
		}

		// Token: 0x170002F2 RID: 754
		// (get) Token: 0x06000ED4 RID: 3796 RVA: 0x0002F07B File Offset: 0x0002D27B
		// (set) Token: 0x06000ED5 RID: 3797 RVA: 0x0002F083 File Offset: 0x0002D283
		public GameMobAreaObserver.ObjectSelectionMethod CurrentObjectSelectionMethod
		{
			get
			{
				return this._currentObjectSelectionMethod;
			}
			set
			{
				this.SetObjectSelectionMethod(value, false);
			}
		}

		// Token: 0x170002F3 RID: 755
		// (get) Token: 0x06000ED6 RID: 3798 RVA: 0x0002F08D File Offset: 0x0002D28D
		// (set) Token: 0x06000ED7 RID: 3799 RVA: 0x0002F095 File Offset: 0x0002D295
		public GameMobAreaObserver.AdditionalObjectValidatorTypes AdditionalObjectValidatorType
		{
			get
			{
				return this._additionalObjectValidatorType;
			}
			set
			{
				this.SetAdditionalObjectValidator(value, false);
			}
		}

		// Token: 0x06000ED8 RID: 3800 RVA: 0x0002F0A0 File Offset: 0x0002D2A0
		private int ClosestObjectSelector(BaseGameMob obj0, BaseGameMob obj1)
		{
			if (!obj0.IsNull() && !obj1.IsNull())
			{
				Vector2 b = base.transform.position;
				return (obj0.Position - b).sqrMagnitude.CompareTo((obj1.Position - b).sqrMagnitude);
			}
			return 0;
		}

		// Token: 0x06000ED9 RID: 3801 RVA: 0x0002F100 File Offset: 0x0002D300
		private void SetAdditionalObjectValidator(GameMobAreaObserver.AdditionalObjectValidatorTypes validatorType, bool force)
		{
			if (!force && this._additionalObjectValidatorType == validatorType)
			{
				return;
			}
			switch (validatorType)
			{
			case GameMobAreaObserver.AdditionalObjectValidatorTypes.OnlyFullHealth:
				this.AdditionalObjectValidator = new Predicate<BaseGameMob>(GameMobAreaObserver.FullHealthValidator);
				break;
			case GameMobAreaObserver.AdditionalObjectValidatorTypes.OnlyNotFullHealth:
				this.AdditionalObjectValidator = new Predicate<BaseGameMob>(GameMobAreaObserver.NotFullHealthValidator);
				break;
			case GameMobAreaObserver.AdditionalObjectValidatorTypes.OnlyDeathMobs:
				this.AdditionalObjectValidator = new Predicate<BaseGameMob>(GameMobAreaObserver.DeathMobValidator);
				break;
			}
			this._additionalObjectValidatorType = validatorType;
		}

		// Token: 0x06000EDA RID: 3802 RVA: 0x0002F174 File Offset: 0x0002D374
		private void SetObjectSelectionMethod(GameMobAreaObserver.ObjectSelectionMethod newMethod, bool force)
		{
			if (force || this._currentObjectSelectionMethod != newMethod)
			{
				switch (newMethod)
				{
				case GameMobAreaObserver.ObjectSelectionMethod.Closest:
					base.CurrentObjectSelector = new Comparison<BaseGameMob>(this.ClosestObjectSelector);
					break;
				case GameMobAreaObserver.ObjectSelectionMethod.MinHealth:
					base.CurrentObjectSelector = new Comparison<BaseGameMob>(GameMobAreaObserver.MinHealthObjectSelector);
					break;
				case GameMobAreaObserver.ObjectSelectionMethod.MaxHealth:
					base.CurrentObjectSelector = new Comparison<BaseGameMob>(GameMobAreaObserver.MaxHealthObjectSelector);
					break;
				case GameMobAreaObserver.ObjectSelectionMethod.MinHealthRatio:
					base.CurrentObjectSelector = new Comparison<BaseGameMob>(GameMobAreaObserver.MinHealthRatioObjectSelector);
					break;
				case GameMobAreaObserver.ObjectSelectionMethod.MaxHealthRatio:
					base.CurrentObjectSelector = new Comparison<BaseGameMob>(GameMobAreaObserver.MaxHealthRatioObjectSelector);
					break;
				default:
					base.CurrentObjectSelector = null;
					break;
				}
				this._currentObjectSelectionMethod = newMethod;
			}
		}

		// Token: 0x06000EDB RID: 3803 RVA: 0x0002F220 File Offset: 0x0002D420
		protected override void Start()
		{
			base.Start();
			this.SetObjectSelectionMethod(this._currentObjectSelectionMethod, true);
			this.SetAdditionalObjectValidator(this._additionalObjectValidatorType, true);
		}

		// Token: 0x06000EDC RID: 3804 RVA: 0x0002F244 File Offset: 0x0002D444
		private void OnDrawGizmosSelected()
		{
			if (Application.isPlaying && base.ObjectsInRange != null)
			{
				for (int i = 0; i < base.ObjectsInRange.Count; i++)
				{
					BaseGameMob baseGameMob = base.ObjectsInRange[i];
					if (!baseGameMob.IsNull())
					{
						Gizmos.color = ((baseGameMob == base.SelectedObject) ? Color.red : Color.blue);
						Gizmos.DrawLine(base.transform.position, baseGameMob.transform.position);
					}
				}
			}
		}

		// Token: 0x040008BD RID: 2237
		public const int ExecutionOrder = 0;

		// Token: 0x040008BE RID: 2238
		[SerializeField]
		[Tooltip("Принцип выбора цели из списка целей, находящихся в зоне обсервера.")]
		private GameMobAreaObserver.ObjectSelectionMethod _currentObjectSelectionMethod;

		// Token: 0x040008BF RID: 2239
		[SerializeField]
		[Tooltip("Тип дополнительной валидации целей, находящихся в зоне обсервера.")]
		private GameMobAreaObserver.AdditionalObjectValidatorTypes _additionalObjectValidatorType;

		// Token: 0x02000495 RID: 1173
		public enum ObjectSelectionMethod
		{
			// Token: 0x040018CC RID: 6348
			None,
			// Token: 0x040018CD RID: 6349
			Closest,
			// Token: 0x040018CE RID: 6350
			MinHealth,
			// Token: 0x040018CF RID: 6351
			MaxHealth,
			// Token: 0x040018D0 RID: 6352
			MinHealthRatio,
			// Token: 0x040018D1 RID: 6353
			MaxHealthRatio
		}

		// Token: 0x02000496 RID: 1174
		public enum AdditionalObjectValidatorTypes
		{
			// Token: 0x040018D3 RID: 6355
			None,
			// Token: 0x040018D4 RID: 6356
			OnlyFullHealth,
			// Token: 0x040018D5 RID: 6357
			OnlyNotFullHealth,
			// Token: 0x040018D6 RID: 6358
			OnlyDeathMobs
		}
	}
}
