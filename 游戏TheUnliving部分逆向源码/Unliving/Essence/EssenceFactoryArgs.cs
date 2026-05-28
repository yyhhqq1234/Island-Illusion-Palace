using System;
using Common.Factories;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Essence
{
	// Token: 0x020002CC RID: 716
	public class EssenceFactoryArgs : MultiRepresentationObjectInstantiator.IArgs, IBaseObjectDescription
	{
		// Token: 0x17000548 RID: 1352
		// (get) Token: 0x060018F1 RID: 6385 RVA: 0x0004EC27 File Offset: 0x0004CE27
		// (set) Token: 0x060018F2 RID: 6386 RVA: 0x0004EC2F File Offset: 0x0004CE2F
		public int ObjectID
		{
			get
			{
				return (int)this.essenceType;
			}
			set
			{
				this.essenceType = (EssenceType)value;
			}
		}

		// Token: 0x17000549 RID: 1353
		// (get) Token: 0x060018F3 RID: 6387 RVA: 0x0004EC38 File Offset: 0x0004CE38
		// (set) Token: 0x060018F4 RID: 6388 RVA: 0x0004EC40 File Offset: 0x0004CE40
		public MultiRepresentationObjectInstantiator.ObjectType Type
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

		// Token: 0x1700054A RID: 1354
		// (get) Token: 0x060018F5 RID: 6389 RVA: 0x0004EC49 File Offset: 0x0004CE49
		public Vector3 SpawnPosition
		{
			get
			{
				return this.spawnPosition;
			}
		}

		// Token: 0x04000E10 RID: 3600
		public MultiRepresentationObjectInstantiator.ObjectType objectType;

		// Token: 0x04000E11 RID: 3601
		public EssenceType essenceType;

		// Token: 0x04000E12 RID: 3602
		public Vector2 spawnPosition;
	}
}
