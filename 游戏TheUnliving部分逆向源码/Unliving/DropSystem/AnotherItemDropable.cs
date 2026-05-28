using System;
using UnityEngine;
using Unliving.Factories;
using Unliving.Pickables;

namespace Unliving.DropSystem
{
	// Token: 0x0200028F RID: 655
	[Serializable]
	public class AnotherItemDropable : DropableBase<NonFactoryPickableType>
	{
		// Token: 0x170004DB RID: 1243
		// (get) Token: 0x0600169F RID: 5791 RVA: 0x0004894F File Offset: 0x00046B4F
		public override Type FactoryType
		{
			get
			{
				return null;
			}
		}

		// Token: 0x170004DC RID: 1244
		// (get) Token: 0x060016A0 RID: 5792 RVA: 0x00048952 File Offset: 0x00046B52
		public override NonFactoryPickableType ID
		{
			get
			{
				return NonFactoryPickableType.None;
			}
		}

		// Token: 0x170004DD RID: 1245
		// (get) Token: 0x060016A1 RID: 5793 RVA: 0x00048955 File Offset: 0x00046B55
		// (set) Token: 0x060016A2 RID: 5794 RVA: 0x00048958 File Offset: 0x00046B58
		public override bool PickedUp
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		// Token: 0x060016A3 RID: 5795 RVA: 0x0004895A File Offset: 0x00046B5A
		public override MultiRepresentationObjectInstantiator.IArgs CreateQuery()
		{
			return new MultiRepresentationObjectInstantiator.DefaultArgs();
		}

		// Token: 0x04000D1E RID: 3358
		public GameObject prefab;

		// Token: 0x04000D1F RID: 3359
		[Tooltip("-1 - использовать значение из префаба")]
		public int minAmount = -1;

		// Token: 0x04000D20 RID: 3360
		public int maxAmount = -1;
	}
}
