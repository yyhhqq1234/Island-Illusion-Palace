using System;
using Game.Damage;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001DD RID: 477
	[DisallowMultipleComponent]
	public sealed class DamageFilterComponent : MonoBehaviour
	{
		// Token: 0x1700031B RID: 795
		// (get) Token: 0x06000F99 RID: 3993 RVA: 0x000315E9 File Offset: 0x0002F7E9
		// (set) Token: 0x06000F9A RID: 3994 RVA: 0x000315F1 File Offset: 0x0002F7F1
		public GameMobDescription[] AllowedDamageSenders
		{
			get
			{
				return this.allowedDamageSenders;
			}
			set
			{
				this.allowedDamageSenders = value;
			}
		}

		// Token: 0x06000F9B RID: 3995 RVA: 0x000315FC File Offset: 0x0002F7FC
		private void OnBeforeHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (this.allowedDamageSenders == null || this.allowedDamageSenders.Length == 0)
			{
				return;
			}
			if (!args.IsForcedChanging && args.IsDamage && args.Amount != 0f)
			{
				IGameMob gameMob = sender as IGameMob;
				if (gameMob != null)
				{
					bool flag = true;
					for (int i = 0; i < this.allowedDamageSenders.Length; i++)
					{
						if (this.allowedDamageSenders[i].IsMatch(gameMob))
						{
							flag = false;
							break;
						}
					}
					if (flag)
					{
						args.Amount = 0f;
					}
				}
			}
		}

		// Token: 0x06000F9C RID: 3996 RVA: 0x0003167D File Offset: 0x0002F87D
		private void Start()
		{
			if (base.TryGetComponent<HitPointsController>(out this.hitPointsController))
			{
				this.hitPointsController.BeforeHitPointsChanged += this.OnBeforeHitPointsChanged;
			}
		}

		// Token: 0x06000F9D RID: 3997 RVA: 0x000316A4 File Offset: 0x0002F8A4
		private void OnDestroy()
		{
			if (this.hitPointsController != null)
			{
				this.hitPointsController.BeforeHitPointsChanged -= this.OnBeforeHitPointsChanged;
			}
		}

		// Token: 0x0400091C RID: 2332
		[SerializeField]
		private GameMobDescription[] allowedDamageSenders;

		// Token: 0x0400091D RID: 2333
		private HitPointsController hitPointsController;
	}
}
