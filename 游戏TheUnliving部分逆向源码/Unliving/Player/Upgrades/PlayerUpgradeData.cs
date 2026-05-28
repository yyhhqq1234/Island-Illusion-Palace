using System;
using Common.Editor.Reorderable;
using Common.Factories;
using UnityEngine;
using Unliving.Scripting;

namespace Unliving.Player.Upgrades
{
	// Token: 0x02000166 RID: 358
	[Serializable]
	public sealed class PlayerUpgradeData : IBaseObjectDescription
	{
		// Token: 0x17000192 RID: 402
		// (get) Token: 0x060009F1 RID: 2545 RVA: 0x00021D5C File Offset: 0x0001FF5C
		// (set) Token: 0x060009F2 RID: 2546 RVA: 0x00021D69 File Offset: 0x0001FF69
		public ScriptVariablesOverrides[] UpgradePropertiesPerLevel
		{
			get
			{
				return this.upgradePropertiesPerLevel;
			}
			set
			{
				this.upgradePropertiesPerLevel.list = value;
			}
		}

		// Token: 0x17000193 RID: 403
		// (get) Token: 0x060009F3 RID: 2547 RVA: 0x00021D77 File Offset: 0x0001FF77
		// (set) Token: 0x060009F4 RID: 2548 RVA: 0x00021D7F File Offset: 0x0001FF7F
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

		// Token: 0x060009F5 RID: 2549 RVA: 0x00021D88 File Offset: 0x0001FF88
		public ScriptVariablesOverrides GetPropertiesOverrides(int upgradeLevel)
		{
			ScriptVariablesOverrides[] array = this.upgradePropertiesPerLevel;
			if (array == null || array.Length == 0)
			{
				return null;
			}
			int num = upgradeLevel - 1;
			if (num < 0)
			{
				num = 0;
			}
			else if (num >= array.Length)
			{
				return null;
			}
			return array[num];
		}

		// Token: 0x040005CD RID: 1485
		public PlayerUpgradeID upgradeID;

		// Token: 0x040005CE RID: 1486
		public ScriptableObject upgradePrototype;

		// Token: 0x040005CF RID: 1487
		[SerializeField]
		private ReorderableListAdapter<ScriptVariablesOverrides[]> upgradePropertiesPerLevel;
	}
}
