using System;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000141 RID: 321
	public sealed class PlayerFactoryArgs : GameMobsFactoryArgsBase
	{
		// Token: 0x1700015C RID: 348
		// (get) Token: 0x06000858 RID: 2136 RVA: 0x0001BFAD File Offset: 0x0001A1AD
		// (set) Token: 0x06000859 RID: 2137 RVA: 0x0001BFB5 File Offset: 0x0001A1B5
		public override int ObjectID
		{
			get
			{
				return (int)this.playerID;
			}
			set
			{
				this.playerID = (PlayerBehaviour.ID)value;
			}
		}

		// Token: 0x040004BB RID: 1211
		public PlayerBehaviour.ID playerID;

		// Token: 0x040004BC RID: 1212
		public InitialPlayerAbilitiesInfo initialPlayerAbilitiesOverrides;

		// Token: 0x040004BD RID: 1213
		public string appearanceAnimationTrigger;

		// Token: 0x040004BE RID: 1214
		public MobSpawnerOverrides playerMobsSpawnerOverrides;
	}
}
