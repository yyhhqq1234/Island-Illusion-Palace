using System;
using Game.Abilities;

namespace Unliving.Player.TemporaryItemsStorage
{
	// Token: 0x02000173 RID: 371
	public sealed class ActivatedMobsCountingTemporaryItem : TemporaryItemBase
	{
		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06000A42 RID: 2626 RVA: 0x00022343 File Offset: 0x00020543
		public override bool IsExpired
		{
			get
			{
				return this.currentActivatedMobsCount >= this.targetActivatedMobsCount;
			}
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x00022356 File Offset: 0x00020556
		public override float ExpirationProgress
		{
			get
			{
				return (float)this.currentActivatedMobsCount / (float)this.targetActivatedMobsCount;
			}
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06000A44 RID: 2628 RVA: 0x00022367 File Offset: 0x00020567
		public override object CurrentProgressValue
		{
			get
			{
				return this.currentActivatedMobsCount;
			}
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x00022374 File Offset: 0x00020574
		public override object TargetProgressValue
		{
			get
			{
				return this.targetActivatedMobsCount;
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06000A46 RID: 2630 RVA: 0x00022381 File Offset: 0x00020581
		public override string ID
		{
			get
			{
				return "activated_mobs_counting_item";
			}
		}

		// Token: 0x06000A47 RID: 2631 RVA: 0x00022388 File Offset: 0x00020588
		private void OnAbilityActivated(IAbility ability, object abilityArgs)
		{
			if (ability.ID != 1037)
			{
				return;
			}
			this.currentActivatedMobsCount++;
		}

		// Token: 0x06000A48 RID: 2632 RVA: 0x000223A6 File Offset: 0x000205A6
		public ActivatedMobsCountingTemporaryItem(object content, int targetActivatedMobsCount) : base(content)
		{
			this.targetActivatedMobsCount = targetActivatedMobsCount;
		}

		// Token: 0x06000A49 RID: 2633 RVA: 0x000223B6 File Offset: 0x000205B6
		public override void OnStored(TemporaryItemsStorageController storage)
		{
			storage.ControllerOwner.AbilitiesController.AbilityActivated += this.OnAbilityActivated;
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x000223D4 File Offset: 0x000205D4
		public override void OnDiscarded(TemporaryItemsStorageController storage)
		{
			storage.ControllerOwner.AbilitiesController.AbilityActivated -= this.OnAbilityActivated;
		}

		// Token: 0x04000609 RID: 1545
		private const int SacrificeAbilityID = 1037;

		// Token: 0x0400060A RID: 1546
		private readonly int targetActivatedMobsCount;

		// Token: 0x0400060B RID: 1547
		private int currentActivatedMobsCount;
	}
}
