using System;
using Game.Core;
using UnityEngine;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000272 RID: 626
	public sealed class LocationDependentObjectSwitcherComponent : GameBehaviourBase
	{
		// Token: 0x0600156B RID: 5483 RVA: 0x000448C8 File Offset: 0x00042AC8
		private void Start()
		{
			for (int i = 0; i < this.targetObjects.Length; i++)
			{
				GameObject gameObject = this.targetObjects[i].gameObject;
				if (gameObject != null)
				{
					gameObject.SetActive(false);
				}
			}
			LocationDependentObjectInfo locationDependentObjectInfo;
			if (LocationDependentDataSelector<LocationDependentObjectInfo>.TryGetData(this.targetObjects, base.CurrentGame, out locationDependentObjectInfo))
			{
				GameObject gameObject2 = locationDependentObjectInfo.gameObject;
				if (gameObject2 != null)
				{
					gameObject2.SetActive(true);
				}
			}
		}

		// Token: 0x04000C63 RID: 3171
		public LocationDependentObjectInfo[] targetObjects;
	}
}
