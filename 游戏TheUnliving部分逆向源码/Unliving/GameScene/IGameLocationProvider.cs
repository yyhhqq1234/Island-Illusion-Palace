using System;
using Unliving.LevelGeneration;

namespace Unliving.GameScene
{
	// Token: 0x0200025E RID: 606
	public interface IGameLocationProvider
	{
		// Token: 0x17000448 RID: 1096
		// (get) Token: 0x06001424 RID: 5156
		float LocationGenerationProgress { get; }

		// Token: 0x17000449 RID: 1097
		// (get) Token: 0x06001425 RID: 5157
		GameLocation CurrentLocation { get; }

		// Token: 0x1700044A RID: 1098
		// (get) Token: 0x06001426 RID: 5158
		GameLocation.TypeID LocationType { get; }

		// Token: 0x1700044B RID: 1099
		// (get) Token: 0x06001427 RID: 5159
		string LevelID { get; }
	}
}
