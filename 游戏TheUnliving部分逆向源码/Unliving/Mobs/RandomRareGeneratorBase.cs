using System;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001FB RID: 507
	[RequireComponent(typeof(Collider2D))]
	public abstract class RandomRareGeneratorBase : GroupDestinationsGeneratorBase
	{
		// Token: 0x060010EA RID: 4330 RVA: 0x00034F38 File Offset: 0x00033138
		private void Awake()
		{
			Collider2D collider2D;
			if (base.TryGetComponent<Collider2D>(out collider2D))
			{
				collider2D.isTrigger = true;
			}
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x00034F56 File Offset: 0x00033156
		public override bool TryGetNewDestination(out Vector2 position, out bool isForcedPosition)
		{
			if (base.TryGetNewDestination(out position, out isForcedPosition) && this.collider != null)
			{
				position = this.collider.GetRandomPointInCollider();
				return true;
			}
			return false;
		}

		// Token: 0x04000994 RID: 2452
		private Collider2D collider;
	}
}
