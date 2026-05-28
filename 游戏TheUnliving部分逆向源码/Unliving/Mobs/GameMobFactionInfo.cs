using System;
using Common.Editor;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001B9 RID: 441
	[Serializable]
	public struct GameMobFactionInfo
	{
		// Token: 0x06000D92 RID: 3474 RVA: 0x0002AF3C File Offset: 0x0002913C
		public static GameMobFactionInfo GetInvalidInfo()
		{
			return new GameMobFactionInfo
			{
				faction = GameMobFactions.None,
				mobsLayer = -1,
				enemyMobLayers = 0
			};
		}

		// Token: 0x06000D93 RID: 3475 RVA: 0x0002AF6F File Offset: 0x0002916F
		public bool IsValid()
		{
			return this.mobsLayer != -1 && this.faction != GameMobFactions.None;
		}

		// Token: 0x06000D94 RID: 3476 RVA: 0x0002AF88 File Offset: 0x00029188
		public bool IsEnemyFaction(int mobLayer)
		{
			return (1 << mobLayer & this.enemyMobLayers) != 0;
		}

		// Token: 0x040007CB RID: 1995
		public GameMobFactions faction;

		// Token: 0x040007CC RID: 1996
		[Layer]
		public int mobsLayer;

		// Token: 0x040007CD RID: 1997
		public LayerMask enemyMobLayers;
	}
}
