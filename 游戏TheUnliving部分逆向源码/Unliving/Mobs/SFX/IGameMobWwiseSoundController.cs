using System;
using AK.Wwise;

namespace Unliving.Mobs.SFX
{
	// Token: 0x0200020C RID: 524
	public interface IGameMobWwiseSoundController
	{
		// Token: 0x170003B3 RID: 947
		// (get) Token: 0x0600119C RID: 4508
		// (set) Token: 0x0600119D RID: 4509
		Event ResurrectionEvent { get; set; }

		// Token: 0x170003B4 RID: 948
		// (get) Token: 0x0600119E RID: 4510
		// (set) Token: 0x0600119F RID: 4511
		Event GraveResurrectionEvent { get; set; }

		// Token: 0x170003B5 RID: 949
		// (get) Token: 0x060011A0 RID: 4512
		// (set) Token: 0x060011A1 RID: 4513
		Event IdleStartEvent { get; set; }

		// Token: 0x170003B6 RID: 950
		// (get) Token: 0x060011A2 RID: 4514
		// (set) Token: 0x060011A3 RID: 4515
		Event IdleStopEvent { get; set; }

		// Token: 0x170003B7 RID: 951
		// (get) Token: 0x060011A4 RID: 4516
		// (set) Token: 0x060011A5 RID: 4517
		Event AggressionStartEvent { get; set; }

		// Token: 0x170003B8 RID: 952
		// (get) Token: 0x060011A6 RID: 4518
		// (set) Token: 0x060011A7 RID: 4519
		Event AggressionStopEvent { get; set; }

		// Token: 0x170003B9 RID: 953
		// (get) Token: 0x060011A8 RID: 4520
		// (set) Token: 0x060011A9 RID: 4521
		Event PanicStartEvent { get; set; }

		// Token: 0x170003BA RID: 954
		// (get) Token: 0x060011AA RID: 4522
		// (set) Token: 0x060011AB RID: 4523
		Event SacrificeEvent { get; set; }

		// Token: 0x170003BB RID: 955
		// (get) Token: 0x060011AC RID: 4524
		// (set) Token: 0x060011AD RID: 4525
		Event CommandConfirmationEvent { get; set; }

		// Token: 0x170003BC RID: 956
		// (get) Token: 0x060011AE RID: 4526
		// (set) Token: 0x060011AF RID: 4527
		Event SacrificationIndicationEvent { get; set; }

		// Token: 0x170003BD RID: 957
		// (get) Token: 0x060011B0 RID: 4528
		// (set) Token: 0x060011B1 RID: 4529
		Event DeathIndicationEvent { get; set; }
	}
}
