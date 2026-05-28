using System;
using Common.Factories;
using UnityEngine;
using Unliving.LeveledItems;
using Unliving.Scripting;

namespace Unliving.Player.Upgrades
{
	// Token: 0x0200016F RID: 367
	public sealed class PlayerUpgradesFactoryArgs : IBaseObjectDescription, IItemLevelProvider
	{
		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06000A32 RID: 2610 RVA: 0x00022196 File Offset: 0x00020396
		// (set) Token: 0x06000A33 RID: 2611 RVA: 0x000221A4 File Offset: 0x000203A4
		public int UpgradeLevel
		{
			get
			{
				return Mathf.Max(this.upgradeLevel, 1);
			}
			set
			{
				this.upgradeLevel = value;
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06000A34 RID: 2612 RVA: 0x000221AD File Offset: 0x000203AD
		// (set) Token: 0x06000A35 RID: 2613 RVA: 0x000221B5 File Offset: 0x000203B5
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.upgradeID;
			}
			set
			{
				this.upgradeID = (PlayerUpgradeID)value;
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06000A36 RID: 2614 RVA: 0x000221BE File Offset: 0x000203BE
		int IItemLevelProvider.ItemLevel
		{
			get
			{
				return this.UpgradeLevel;
			}
		}

		// Token: 0x04000600 RID: 1536
		public PlayerUpgradeID upgradeID;

		// Token: 0x04000601 RID: 1537
		public IPlayerUpgrade upgradePrototype;

		// Token: 0x04000602 RID: 1538
		public ScriptVariablesOverrides upgradePropertiesOverrides;

		// Token: 0x04000603 RID: 1539
		private int upgradeLevel;
	}
}
