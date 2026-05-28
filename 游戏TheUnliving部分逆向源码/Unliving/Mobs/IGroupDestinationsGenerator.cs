using System;
using UnityEngine;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs
{
	// Token: 0x020001F8 RID: 504
	public interface IGroupDestinationsGenerator
	{
		// Token: 0x1700037A RID: 890
		// (get) Token: 0x060010D6 RID: 4310
		// (set) Token: 0x060010D7 RID: 4311
		bool IsActive { get; set; }

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x060010D8 RID: 4312
		bool GenerateIndividualMobDestinations { get; }

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x060010D9 RID: 4313
		// (set) Token: 0x060010DA RID: 4314
		GameMobsGroupControllerBase CurrentGroup { get; set; }

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x060010DB RID: 4315
		bool PassToSumoningGroups { get; }

		// Token: 0x060010DC RID: 4316
		bool TryGetNewDestination(out Vector2 position, out bool isForcedDestination);

		// Token: 0x060010DD RID: 4317
		bool TryGetAdditionalVelocity(Vector2 mobPosition, float mobSpeed, out Vector2 additionalVelocity);

		// Token: 0x060010DE RID: 4318
		IGroupDestinationsGenerator Clone(GameMobsGroupControllerBase targetGroup);

		// Token: 0x060010DF RID: 4319
		bool CanGenerateNewIndividualDestination(GameMobMotionController mobMotionController);

		// Token: 0x060010E0 RID: 4320
		bool TryGetIndividualDestination(GameMobMotionController mobMotionController, out Vector2 destination);
	}
}
