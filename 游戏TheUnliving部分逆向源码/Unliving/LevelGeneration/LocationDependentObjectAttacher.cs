using System;
using Common.Editor;
using Common.PivotGroup;
using Game.Core;
using UnityEngine;
using Unliving.GameScene;

namespace Unliving.LevelGeneration
{
	// Token: 0x02000270 RID: 624
	public static class LocationDependentObjectAttacher
	{
		// Token: 0x06001567 RID: 5479 RVA: 0x000447F8 File Offset: 0x000429F8
		public static bool TryAttachObject(LocationDependentObjectAttacher.Data[] locationDependentData, GameObject targetObject, IGameLocationProvider locationProvider, out GameObject attachedObject)
		{
			LocationDependentObjectAttacher.Data data;
			if (LocationDependentDataSelector<LocationDependentObjectAttacher.Data>.TryGetData(locationDependentData, locationProvider, out data))
			{
				attachedObject = data.InstantiatePrefab();
				if (attachedObject != null)
				{
					Vector3 vector = data.attachedObjectOffset;
					string attachmentPivot = data.attachmentPivot;
					IPivotGroupProvider<string> pivotGroupProvider;
					if (!string.IsNullOrEmpty(attachmentPivot) && attachmentPivot != "Untagged" && targetObject.TryGetComponent<IPivotGroupProvider<string>>(out pivotGroupProvider))
					{
						IPivot pivot = pivotGroupProvider.PivotGroup.GetPivot(TaggedPivotGroup.TagToHash(attachmentPivot));
						if (pivot != null)
						{
							vector += pivot.LocalPosition;
						}
					}
					attachedObject.transform.parent = targetObject.transform;
					attachedObject.transform.localPosition = vector;
					return true;
				}
			}
			attachedObject = null;
			return false;
		}

		// Token: 0x06001568 RID: 5480 RVA: 0x000448A2 File Offset: 0x00042AA2
		public static bool TryAttachObject(LocationDependentObjectAttacher.Data[] locationDependentData, GameObject targetObject, IGame game, out GameObject attachedObject)
		{
			return LocationDependentObjectAttacher.TryAttachObject(locationDependentData, targetObject, game.Services.Get<IGameLocationProvider>(), out attachedObject);
		}

		// Token: 0x020004F7 RID: 1271
		[Serializable]
		public struct Data : ILocationDependentData
		{
			// Token: 0x170007AC RID: 1964
			// (get) Token: 0x060025BF RID: 9663 RVA: 0x00075F42 File Offset: 0x00074142
			GameLocation.TypeID ILocationDependentData.LocationID
			{
				get
				{
					return this.locationID;
				}
			}

			// Token: 0x170007AD RID: 1965
			// (get) Token: 0x060025C0 RID: 9664 RVA: 0x00075F4A File Offset: 0x0007414A
			UnityEngine.Object ILocationDependentData.Data
			{
				get
				{
					return this.objectPrefab;
				}
			}

			// Token: 0x060025C1 RID: 9665 RVA: 0x00075F52 File Offset: 0x00074152
			internal GameObject InstantiatePrefab()
			{
				if (!(this.objectPrefab != null))
				{
					return null;
				}
				return UnityEngine.Object.Instantiate<GameObject>(this.objectPrefab);
			}

			// Token: 0x04001A90 RID: 6800
			public GameLocation.TypeID locationID;

			// Token: 0x04001A91 RID: 6801
			public GameObject objectPrefab;

			// Token: 0x04001A92 RID: 6802
			[Tag]
			public string attachmentPivot;

			// Token: 0x04001A93 RID: 6803
			public Vector3 attachedObjectOffset;
		}
	}
}
