using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000271 RID: 625
	[Serializable]
	public struct LocationDependentObjectInfo : ILocationDependentData
	{
		// Token: 0x17000486 RID: 1158
		// (get) Token: 0x06001569 RID: 5481 RVA: 0x000448B7 File Offset: 0x00042AB7
		GameLocation.TypeID ILocationDependentData.LocationID
		{
			get
			{
				return this.locationID;
			}
		}

		// Token: 0x17000487 RID: 1159
		// (get) Token: 0x0600156A RID: 5482 RVA: 0x000448BF File Offset: 0x00042ABF
		UnityEngine.Object ILocationDependentData.Data
		{
			get
			{
				return this.gameObject;
			}
		}

		// Token: 0x04000C61 RID: 3169
		public GameLocation.TypeID locationID;

		// Token: 0x04000C62 RID: 3170
		[FormerlySerializedAs("prefab")]
		public GameObject gameObject;
	}
}
