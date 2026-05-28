using System;
using Common;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.DropSystem
{
	// Token: 0x02000293 RID: 659
	public interface IDropable : IWeighted, ICloneable<IDropable>
	{
		// Token: 0x170004EA RID: 1258
		// (get) Token: 0x060016BE RID: 5822
		object ObjectID { get; }

		// Token: 0x170004EB RID: 1259
		// (get) Token: 0x060016BF RID: 5823
		Type FactoryType { get; }

		// Token: 0x170004EC RID: 1260
		// (get) Token: 0x060016C0 RID: 5824
		Type Type { get; }

		// Token: 0x170004ED RID: 1261
		// (get) Token: 0x060016C1 RID: 5825
		int NumericID { get; }

		// Token: 0x170004EE RID: 1262
		// (get) Token: 0x060016C2 RID: 5826
		// (set) Token: 0x060016C3 RID: 5827
		bool PickedUp { get; set; }

		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x060016C4 RID: 5828
		GameObject ObjectInstance { get; }

		// Token: 0x060016C5 RID: 5829
		MultiRepresentationObjectInstantiator.IArgs CreateQuery();

		// Token: 0x060016C6 RID: 5830
		void SetQueryOverride(MultiRepresentationObjectInstantiator.IArgs queryOverride);

		// Token: 0x060016C7 RID: 5831
		void OnObjectSpawned(GameObject spawnedObject);
	}
}
