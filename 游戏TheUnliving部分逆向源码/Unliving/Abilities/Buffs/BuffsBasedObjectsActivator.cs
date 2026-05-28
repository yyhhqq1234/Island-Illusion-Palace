using System;
using Common.UnityExtensions;
using Game.Buffs;
using UnityEngine;

namespace Unliving.Abilities.Buffs
{
	// Token: 0x020003DC RID: 988
	public sealed class BuffsBasedObjectsActivator : MonoBehaviour
	{
		// Token: 0x170006D1 RID: 1745
		// (get) Token: 0x06002190 RID: 8592 RVA: 0x00068E58 File Offset: 0x00067058
		// (set) Token: 0x06002191 RID: 8593 RVA: 0x00068E60 File Offset: 0x00067060
		public GameObject BuffsReceiver
		{
			get
			{
				return this.buffsReceiver;
			}
			set
			{
				this.buffsReceiver = value;
			}
		}

		// Token: 0x170006D2 RID: 1746
		// (get) Token: 0x06002192 RID: 8594 RVA: 0x00068E69 File Offset: 0x00067069
		// (set) Token: 0x06002193 RID: 8595 RVA: 0x00068E71 File Offset: 0x00067071
		public BuffsBasedObjectsActivator.ObjectInfo[] TargetObjects
		{
			get
			{
				return this.targetObjects;
			}
			set
			{
				this.targetObjects = value;
			}
		}

		// Token: 0x06002194 RID: 8596 RVA: 0x00068E7C File Offset: 0x0006707C
		private void OnBuffsTriggerStateChanged(int buffID, bool isActive)
		{
			if (this.targetObjects != null)
			{
				for (int i = 0; i < this.targetObjects.Length; i++)
				{
					if (this.targetObjects[i].IsTargetBuff(buffID))
					{
						this.targetObjects[i].SetObjectActive(isActive);
					}
				}
			}
		}

		// Token: 0x06002195 RID: 8597 RVA: 0x00068ECC File Offset: 0x000670CC
		private void Start()
		{
			IBuffableObject buffableObject;
			if (this.buffsReceiver != null && this.buffsReceiver.TryGetComponent<IBuffableObject>(out buffableObject))
			{
				if (this.targetObjects != null)
				{
					for (int i = 0; i < this.targetObjects.Length; i++)
					{
						this.targetObjects[i].SetObjectActive(false);
					}
				}
				this.buffsReceivingTrigger = new BuffsReceivingTrigger(buffableObject, null);
				this.buffsReceivingTrigger.BuffsStateChanged += this.OnBuffsTriggerStateChanged;
			}
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x00068F47 File Offset: 0x00067147
		private void OnDestroy()
		{
			this.buffsReceivingTrigger.BuffsStateChanged -= this.OnBuffsTriggerStateChanged;
			this.buffsReceivingTrigger.Dispose();
		}

		// Token: 0x040014F0 RID: 5360
		[SerializeField]
		private GameObject buffsReceiver;

		// Token: 0x040014F1 RID: 5361
		[SerializeField]
		private BuffsBasedObjectsActivator.ObjectInfo[] targetObjects;

		// Token: 0x040014F2 RID: 5362
		private BuffsReceivingTrigger buffsReceivingTrigger;

		// Token: 0x02000596 RID: 1430
		[Serializable]
		public struct ObjectInfo
		{
			// Token: 0x060027B4 RID: 10164 RVA: 0x0007C4FD File Offset: 0x0007A6FD
			public bool IsValid()
			{
				return this.expectedBuffsGenerator != null && this.targetObjects != null && this.targetObjects.Length != 0;
			}

			// Token: 0x060027B5 RID: 10165 RVA: 0x0007C521 File Offset: 0x0007A721
			public bool IsTargetBuff(int buffID)
			{
				return this.IsValid() && this.expectedBuffsGenerator.IsRelatedBuff(buffID);
			}

			// Token: 0x060027B6 RID: 10166 RVA: 0x0007C53C File Offset: 0x0007A73C
			public void SetObjectActive(bool isActive)
			{
				if (!this.IsValid())
				{
					return;
				}
				for (int i = 0; i < this.targetObjects.Length; i++)
				{
					GameObject gameObject = this.targetObjects[i];
					if (!gameObject.IsNull())
					{
						gameObject.SetActive(isActive);
					}
				}
			}

			// Token: 0x04001D01 RID: 7425
			public BuffsGeneratorBuilderAsset expectedBuffsGenerator;

			// Token: 0x04001D02 RID: 7426
			public GameObject[] targetObjects;
		}
	}
}
