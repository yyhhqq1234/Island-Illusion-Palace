using System;
using Game.Core;
using Unliving.LevelGeneration;

namespace Unliving.GameScene
{
	// Token: 0x0200025C RID: 604
	public sealed class TutorialLocationObjectActivitySwitcher : GameBehaviourBase
	{
		// Token: 0x06001410 RID: 5136 RVA: 0x0003F378 File Offset: 0x0003D578
		private void Start()
		{
			IGameLocationProvider gameLocationProvider;
			if (base.CurrentGame.Services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				GameLocation currentLocation = gameLocationProvider.CurrentLocation;
				if (currentLocation != null && currentLocation.IsTutorialLocation)
				{
					base.gameObject.SetActive(this.targetActivityState);
				}
			}
		}

		// Token: 0x04000BB5 RID: 2997
		public bool targetActivityState;
	}
}
