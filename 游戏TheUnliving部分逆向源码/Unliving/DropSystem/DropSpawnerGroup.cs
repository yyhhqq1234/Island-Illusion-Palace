using System;
using Game.Core;
using Unliving.Pickables;

namespace Unliving.DropSystem
{
	// Token: 0x0200028B RID: 651
	public class DropSpawnerGroup : GameBehaviourBase
	{
		// Token: 0x06001690 RID: 5776 RVA: 0x00048804 File Offset: 0x00046A04
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			DropSpawner[] array = this.dropSpawners;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].PickablePickedUp += this.OnPickablePickedUp;
			}
		}

		// Token: 0x06001691 RID: 5777 RVA: 0x00048844 File Offset: 0x00046A44
		private void OnPickablePickedUp(DropSpawner dropSpawner, IPickableObject pickable)
		{
			foreach (DropSpawner dropSpawner2 in this.dropSpawners)
			{
				dropSpawner2.PickablePickedUp -= this.OnPickablePickedUp;
				if (!(dropSpawner == dropSpawner2))
				{
					dropSpawner2.DestroySpawnedPickable();
				}
			}
		}

		// Token: 0x04000D1A RID: 3354
		public DropSpawner[] dropSpawners;
	}
}
