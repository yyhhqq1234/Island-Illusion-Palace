using System;
using Game.Core;
using Unliving.GameScene;

namespace Unliving.LevelGeneration
{
	// Token: 0x0200026F RID: 623
	public static class LocationDependentDataSelector<TData> where TData : ILocationDependentData
	{
		// Token: 0x06001565 RID: 5477 RVA: 0x00044770 File Offset: 0x00042970
		public static bool TryGetData(TData[] locationDependentData, IGameLocationProvider locationProvider, out TData data)
		{
			if (locationDependentData != null && locationProvider != null)
			{
				GameLocation.TypeID locationType = locationProvider.LocationType;
				int num = -1;
				for (int i = 0; i < locationDependentData.Length; i++)
				{
					GameLocation.TypeID locationID = locationDependentData[i].LocationID;
					if (locationID == locationType)
					{
						data = locationDependentData[i];
						return true;
					}
					if (locationID == GameLocation.TypeID.Undefined)
					{
						num = i;
					}
				}
				if (num >= 0)
				{
					data = locationDependentData[num];
					return true;
				}
			}
			data = default(TData);
			return false;
		}

		// Token: 0x06001566 RID: 5478 RVA: 0x000447E2 File Offset: 0x000429E2
		public static bool TryGetData(TData[] locationDependentData, IGame game, out TData data)
		{
			return LocationDependentDataSelector<TData>.TryGetData(locationDependentData, game.Services.Get<IGameLocationProvider>(), out data);
		}
	}
}
