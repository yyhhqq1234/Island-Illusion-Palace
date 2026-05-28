using System;
using Common.Factories;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A5 RID: 421
	public sealed class PassiveAbilityFactoryQuery : IBaseObjectDescription, MultiRepresentationObjectInstantiator.IArgs
	{
		// Token: 0x17000220 RID: 544
		// (get) Token: 0x06000C03 RID: 3075 RVA: 0x00025D76 File Offset: 0x00023F76
		// (set) Token: 0x06000C04 RID: 3076 RVA: 0x00025D7E File Offset: 0x00023F7E
		int IBaseObjectDescription.ObjectID
		{
			get
			{
				return (int)this.abilityID;
			}
			set
			{
				this.abilityID = (PassiveAbilityID)value;
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06000C05 RID: 3077 RVA: 0x00025D87 File Offset: 0x00023F87
		// (set) Token: 0x06000C06 RID: 3078 RVA: 0x00025D8F File Offset: 0x00023F8F
		MultiRepresentationObjectInstantiator.ObjectType MultiRepresentationObjectInstantiator.IArgs.Type
		{
			get
			{
				return this.objectType;
			}
			set
			{
				this.objectType = value;
			}
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x06000C07 RID: 3079 RVA: 0x00025D98 File Offset: 0x00023F98
		Vector3 MultiRepresentationObjectInstantiator.IArgs.SpawnPosition
		{
			get
			{
				return this.spawnPosition;
			}
		}

		// Token: 0x040006B3 RID: 1715
		public PassiveAbilityID abilityID;

		// Token: 0x040006B4 RID: 1716
		public MultiRepresentationObjectInstantiator.ObjectType objectType;

		// Token: 0x040006B5 RID: 1717
		public Vector2 spawnPosition;
	}
}
