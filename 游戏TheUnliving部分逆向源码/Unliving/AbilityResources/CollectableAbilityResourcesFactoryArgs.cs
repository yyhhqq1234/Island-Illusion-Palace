using System;
using Common.Factories;
using UnityEngine;

namespace Unliving.AbilityResources
{
	// Token: 0x0200035F RID: 863
	public sealed class CollectableAbilityResourcesFactoryArgs : IBaseObjectDescription
	{
		// Token: 0x170005DD RID: 1501
		// (get) Token: 0x06001C59 RID: 7257 RVA: 0x00059A6D File Offset: 0x00057C6D
		// (set) Token: 0x06001C5A RID: 7258 RVA: 0x00059A75 File Offset: 0x00057C75
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.resourceType;
			}
			set
			{
				this.resourceType = (AbilityResourceType)value;
			}
		}

		// Token: 0x04001005 RID: 4101
		public AbilityResourceType resourceType;

		// Token: 0x04001006 RID: 4102
		public object resourceOwner;

		// Token: 0x04001007 RID: 4103
		public GameObject arbitraryResourceObject;

		// Token: 0x04001008 RID: 4104
		public Vector2? position;

		// Token: 0x04001009 RID: 4105
		public bool isStatic;

		// Token: 0x0400100A RID: 4106
		public Vector3 impulse;

		// Token: 0x0400100B RID: 4107
		public float angularImpulse;
	}
}
