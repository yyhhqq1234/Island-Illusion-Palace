using System;
using Common.UnityExtensions;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001CD RID: 461
	public sealed class FakeMobInstantiator
	{
		// Token: 0x06000E76 RID: 3702 RVA: 0x0002DC90 File Offset: 0x0002BE90
		public FakeMobBehaviour Instantiate(FakeMobInstantiationArgs args)
		{
			GameObject mobPrefab = args.mobPrefab;
			FakeMobBehaviour fakeMobBehaviour;
			if (mobPrefab != null)
			{
				fakeMobBehaviour = UnityEngine.Object.Instantiate<GameObject>(mobPrefab).GetComponentOrDestroy<FakeMobBehaviour>();
			}
			else
			{
				fakeMobBehaviour = new GameObject(args.owner.name + "_fakeMob").AddComponent<FakeMobBehaviour>();
			}
			GameMobSummoningContext gameMobSummoningContext = new GameMobSummoningContext();
			GameMobsGroupControllerBase group = args.group;
			gameMobSummoningContext.summoner = ((group != null) ? group.Leader : null);
			gameMobSummoningContext.summoningSource = args.owner;
			gameMobSummoningContext.abilitiesLevelOverride = args.abilitiesLevelOverride;
			gameMobSummoningContext.statsModifiers = args.statsModifiers;
			GameMobSummoningContext context = gameMobSummoningContext;
			fakeMobBehaviour.SetCreationType(GameMobCreationType.Default, context);
			fakeMobBehaviour.Group = args.group;
			fakeMobBehaviour.Position = args.position;
			fakeMobBehaviour.activateOnStart = args.activateOnStart;
			fakeMobBehaviour.Initialize(args.abilitiesOverride);
			return fakeMobBehaviour;
		}
	}
}
