using System;
using System.Collections.Generic;
using Game.Buffs;

namespace Unliving.Player.TemporaryItemsStorage
{
	// Token: 0x02000177 RID: 375
	public static class TemporaryItemsStorageExtensions
	{
		// Token: 0x06000A69 RID: 2665 RVA: 0x000226F2 File Offset: 0x000208F2
		public static IBuff GenerateBuffForTempStorage(this IBuffsGenerator buffsGenerator, TemporaryItemsStorageController storage)
		{
			if (buffsGenerator == null)
			{
				return null;
			}
			return buffsGenerator.GenerateBuff(storage.ControllerOwner, true);
		}

		// Token: 0x06000A6A RID: 2666 RVA: 0x00022708 File Offset: 0x00020908
		public static bool TryGetStorageItemWithContent(this TemporaryItemsStorageController storage, object content, out TemporaryItemBase item)
		{
			IReadOnlyList<TemporaryItemBase> items = storage.Items;
			for (int i = 0; i < items.Count; i++)
			{
				item = items[i];
				TemporaryItemBase temporaryItemBase = item;
				if (object.Equals((temporaryItemBase != null) ? temporaryItemBase.Content : null, content))
				{
					return true;
				}
			}
			item = null;
			return false;
		}
	}
}
