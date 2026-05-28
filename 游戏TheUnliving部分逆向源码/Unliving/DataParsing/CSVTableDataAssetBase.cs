using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.DataParsing
{
	// Token: 0x020002CF RID: 719
	public abstract class CSVTableDataAssetBase<TData> : ScriptableObject, ISerializationCallbackReceiver
	{
		// Token: 0x06001903 RID: 6403 RVA: 0x0004EF36 File Offset: 0x0004D136
		private void ParseTable(bool force = false)
		{
		}

		// Token: 0x06001904 RID: 6404 RVA: 0x0004EF38 File Offset: 0x0004D138
		protected void ForceParseTable()
		{
			this.ParseTable(true);
		}

		// Token: 0x06001905 RID: 6405
		protected abstract TextAsset GetTableAsset();

		// Token: 0x06001906 RID: 6406
		protected abstract TData ParseTable(List<List<string>> table);

		// Token: 0x06001907 RID: 6407
		protected abstract void OnTableParsed(TData data);

		// Token: 0x06001908 RID: 6408 RVA: 0x0004EF41 File Offset: 0x0004D141
		protected virtual void OnEnable()
		{
			this.ParseTable(false);
		}

		// Token: 0x06001909 RID: 6409 RVA: 0x0004EF4A File Offset: 0x0004D14A
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this.ParseTable(false);
		}

		// Token: 0x0600190A RID: 6410 RVA: 0x0004EF53 File Offset: 0x0004D153
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x04000E1C RID: 3612
		[SerializeField]
		[HideInInspector]
		private string tableHash;
	}
}
