using System;
using Common.Editor;
using UnityEngine;
using Unliving.Essence;
using Unliving.Factories;

namespace Unliving.DropSystem
{
	// Token: 0x02000291 RID: 657
	[Serializable]
	public class EssenceDropable : DropableBase<EssenceType>
	{
		// Token: 0x170004E6 RID: 1254
		// (get) Token: 0x060016B5 RID: 5813 RVA: 0x00048A01 File Offset: 0x00046C01
		public override Type FactoryType
		{
			get
			{
				return typeof(EssenceFactory);
			}
		}

		// Token: 0x170004E7 RID: 1255
		// (get) Token: 0x060016B6 RID: 5814 RVA: 0x00048A0D File Offset: 0x00046C0D
		public override EssenceType ID
		{
			get
			{
				return this.essenceType;
			}
		}

		// Token: 0x060016B7 RID: 5815 RVA: 0x00048A15 File Offset: 0x00046C15
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			return new EssenceFactoryArgs
			{
				essenceType = this.ID
			};
		}

		// Token: 0x060016B8 RID: 5816 RVA: 0x00048A28 File Offset: 0x00046C28
		public override void OnObjectSpawned(GameObject spawnedObject)
		{
			base.OnObjectSpawned(spawnedObject);
			Essence essence;
			if (spawnedObject.TryGetComponent<Essence>(out essence))
			{
				essence.dropSpawners = this.dropSpawners;
				essence.essenceType = this.essenceType;
			}
		}

		// Token: 0x04000D25 RID: 3365
		[EnumPopup]
		public EssenceType essenceType;

		// Token: 0x04000D26 RID: 3366
		public DropSpawner[] dropSpawners;
	}
}
