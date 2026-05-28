using System;
using Common;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.DropSystem
{
	// Token: 0x02000290 RID: 656
	public abstract class DropableBase<T> : IDropable, IWeighted, ICloneable<IDropable> where T : Enum
	{
		// Token: 0x170004DE RID: 1246
		// (get) Token: 0x060016A5 RID: 5797
		public abstract Type FactoryType { get; }

		// Token: 0x170004DF RID: 1247
		// (get) Token: 0x060016A6 RID: 5798 RVA: 0x00048977 File Offset: 0x00046B77
		public Type Type
		{
			get
			{
				return typeof(T);
			}
		}

		// Token: 0x170004E0 RID: 1248
		// (get) Token: 0x060016A7 RID: 5799 RVA: 0x00048983 File Offset: 0x00046B83
		public int NumericID
		{
			get
			{
				return (int)((object)this.ID);
			}
		}

		// Token: 0x170004E1 RID: 1249
		// (get) Token: 0x060016A8 RID: 5800 RVA: 0x00048995 File Offset: 0x00046B95
		// (set) Token: 0x060016A9 RID: 5801 RVA: 0x0004899D File Offset: 0x00046B9D
		public float Weight
		{
			get
			{
				return this.weight;
			}
			set
			{
				this.weight = Mathf.Clamp01(value);
			}
		}

		// Token: 0x170004E2 RID: 1250
		// (get) Token: 0x060016AA RID: 5802
		public abstract T ID { get; }

		// Token: 0x170004E3 RID: 1251
		// (get) Token: 0x060016AB RID: 5803 RVA: 0x000489AB File Offset: 0x00046BAB
		// (set) Token: 0x060016AC RID: 5804 RVA: 0x000489B3 File Offset: 0x00046BB3
		public virtual bool PickedUp { get; set; }

		// Token: 0x170004E4 RID: 1252
		// (get) Token: 0x060016AD RID: 5805 RVA: 0x000489BC File Offset: 0x00046BBC
		public object ObjectID
		{
			get
			{
				return this.ID;
			}
		}

		// Token: 0x170004E5 RID: 1253
		// (get) Token: 0x060016AE RID: 5806 RVA: 0x000489C9 File Offset: 0x00046BC9
		// (set) Token: 0x060016AF RID: 5807 RVA: 0x000489D1 File Offset: 0x00046BD1
		public GameObject ObjectInstance { get; private set; }

		// Token: 0x060016B0 RID: 5808
		public abstract MultiRepresentationObjectInstantiator.IArgs CreateQuery();

		// Token: 0x060016B1 RID: 5809 RVA: 0x000489DA File Offset: 0x00046BDA
		public void SetQueryOverride(MultiRepresentationObjectInstantiator.IArgs queryOverride)
		{
			this.queryOverride = queryOverride;
		}

		// Token: 0x060016B2 RID: 5810 RVA: 0x000489E3 File Offset: 0x00046BE3
		public virtual void OnObjectSpawned(GameObject spawnedObject)
		{
			this.ObjectInstance = spawnedObject;
		}

		// Token: 0x060016B3 RID: 5811 RVA: 0x000489EC File Offset: 0x00046BEC
		public IDropable Clone()
		{
			return (IDropable)base.MemberwiseClone();
		}

		// Token: 0x04000D23 RID: 3363
		[SerializeField]
		[Range(0f, 1f)]
		private float weight;

		// Token: 0x04000D24 RID: 3364
		protected MultiRepresentationObjectInstantiator.IArgs queryOverride;
	}
}
