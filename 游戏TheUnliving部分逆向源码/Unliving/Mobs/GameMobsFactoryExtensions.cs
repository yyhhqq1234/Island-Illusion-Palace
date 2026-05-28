using System;

namespace Unliving.Mobs
{
	// Token: 0x020001C0 RID: 448
	public static class GameMobsFactoryExtensions
	{
		// Token: 0x06000DB5 RID: 3509 RVA: 0x0002BC62 File Offset: 0x00029E62
		private static GameMobGroupController CreateIndividualGroup(BaseGameMob targetMob)
		{
			GameMobGroupController gameMobGroupController = new GameMobGroupController(1, 0);
			gameMobGroupController.Faction = targetMob.Faction;
			gameMobGroupController.Initialize(targetMob.GetInstanceID(), targetMob.gameObject, targetMob.Position);
			gameMobGroupController.AddMob(targetMob, null);
			gameMobGroupController.Leader = targetMob;
			return gameMobGroupController;
		}

		// Token: 0x06000DB6 RID: 3510 RVA: 0x0002BCA0 File Offset: 0x00029EA0
		public static IGameMob SummonMob(this IGameMobsFactory factory, GameMobSummoningContext context, GameMobsFactoryArgsBase factoryArgs, bool summonToIndividualGroups = false, float lifetime = -1f, bool canBeHealed = false)
		{
			if (summonToIndividualGroups)
			{
				IGameMob gameMob = factory.SummonMob(context, null, factoryArgs, lifetime, canBeHealed);
				BaseGameMob baseGameMob = gameMob as BaseGameMob;
				if (baseGameMob != null)
				{
					GameMobsFactoryExtensions.CreateIndividualGroup(baseGameMob);
				}
				return gameMob;
			}
			GameMobGroupController gameMobGroupController = context.GetSummonerGroup() as GameMobGroupController;
			if (gameMobGroupController == null)
			{
				return null;
			}
			GameMobGroupController summonableMobsGroup = gameMobGroupController.GetSummonableMobsGroup(factoryArgs.mobFaction);
			if (summonableMobsGroup == null)
			{
				return null;
			}
			return factory.SummonMob(context, summonableMobsGroup, factoryArgs, lifetime, canBeHealed);
		}
	}
}
