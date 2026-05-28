using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Unliving.Essence;
using Unliving.Pickables;

namespace Unliving.Tutorial
{
	// Token: 0x0200002B RID: 43
	public class PickItemTrigger : MonoBehaviour
	{
		// Token: 0x06000182 RID: 386 RVA: 0x000068D0 File Offset: 0x00004AD0
		private void Start()
		{
			this.allPickables = new List<PickableBase>();
			foreach (PickableBase item in this.pickables)
			{
				this.allPickables.Add(item);
			}
			foreach (EssenceSpawner essenceSpawner in this.essenceSpawners)
			{
				this.allPickables.Add(essenceSpawner.SpawnedOnStartObject.GetComponent<PickableBase>());
			}
			foreach (PickableBase pickableBase in this.allPickables)
			{
				pickableBase.PickedUp += this.PickUpTriggerReaction;
			}
		}

		// Token: 0x06000183 RID: 387 RVA: 0x000069D0 File Offset: 0x00004BD0
		private void PickUpTriggerReaction(IPickableObject obj, IPickableObjectCollector pickableCollector)
		{
			PickableBase pickableBase = (PickableBase)obj;
			pickableBase.PickedUp -= this.PickUpTriggerReaction;
			this.allPickables.Remove(pickableBase);
			if (this.allPickables.Count == 0)
			{
				UnityEvent afterPickUpEvents = this.AfterPickUpEvents;
				if (afterPickUpEvents == null)
				{
					return;
				}
				afterPickUpEvents.Invoke();
			}
		}

		// Token: 0x040000C2 RID: 194
		public List<PickableBase> pickables;

		// Token: 0x040000C3 RID: 195
		public List<EssenceSpawner> essenceSpawners;

		// Token: 0x040000C4 RID: 196
		public UnityEvent AfterPickUpEvents;

		// Token: 0x040000C5 RID: 197
		private List<PickableBase> allPickables;
	}
}
