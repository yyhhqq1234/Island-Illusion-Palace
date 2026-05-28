using System;
using UnityEngine;

namespace Unliving.Mobs.Motion
{
	// Token: 0x0200020D RID: 525
	public sealed class GameMobDestinationPoint
	{
		// Token: 0x170003BE RID: 958
		// (get) Token: 0x060011B2 RID: 4530 RVA: 0x0003763B File Offset: 0x0003583B
		public bool HasValue
		{
			get
			{
				return this.hasValue;
			}
		}

		// Token: 0x170003BF RID: 959
		// (get) Token: 0x060011B3 RID: 4531 RVA: 0x00037643 File Offset: 0x00035843
		public Vector2? Value
		{
			get
			{
				return this.value;
			}
		}

		// Token: 0x140000BD RID: 189
		// (add) Token: 0x060011B4 RID: 4532 RVA: 0x0003764C File Offset: 0x0003584C
		// (remove) Token: 0x060011B5 RID: 4533 RVA: 0x00037684 File Offset: 0x00035884
		public event Action Changed;

		// Token: 0x060011B6 RID: 4534 RVA: 0x000376B9 File Offset: 0x000358B9
		public Vector2 GetValue()
		{
			return this.value.Value;
		}

		// Token: 0x060011B7 RID: 4535 RVA: 0x000376C6 File Offset: 0x000358C6
		public bool TryGetValue(out Vector2 value)
		{
			if (this.hasValue)
			{
				value = this.value.Value;
				return true;
			}
			value = default(Vector2);
			return false;
		}

		// Token: 0x060011B8 RID: 4536 RVA: 0x000376EC File Offset: 0x000358EC
		public bool CanBeUpdated(Vector2 newValue)
		{
			return this.value == null || (this.value.Value - newValue).SqrMagnitude() > this.newValueDistanceThreshold * this.newValueDistanceThreshold;
		}

		// Token: 0x060011B9 RID: 4537 RVA: 0x00037730 File Offset: 0x00035930
		public bool Update(Vector2? newValue, bool force = false)
		{
			if (force || this.value == null || newValue == null || this.CanBeUpdated(newValue.Value))
			{
				this.value = newValue;
				this.hasValue = (newValue != null);
				Action changed = this.Changed;
				if (changed != null)
				{
					changed();
				}
				return true;
			}
			return false;
		}

		// Token: 0x04000A18 RID: 2584
		public float newValueDistanceThreshold = 0.1f;

		// Token: 0x04000A19 RID: 2585
		private Vector2? value;

		// Token: 0x04000A1A RID: 2586
		private bool hasValue;
	}
}
