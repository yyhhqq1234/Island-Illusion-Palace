using System;
using UnityEngine;

namespace Unliving.GoogleSheetExport
{
	// Token: 0x020002A4 RID: 676
	public class GoogleSheetExportItemComponent : MonoBehaviour, IExportItem
	{
		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x06001794 RID: 6036 RVA: 0x0004AFE4 File Offset: 0x000491E4
		public string Name
		{
			get
			{
				return this.columnName;
			}
		}

		// Token: 0x04000D89 RID: 3465
		[Tooltip("Имя столбца в Google таблице")]
		public string columnName = "Custom object";
	}
}
