using System;
using Common;
using Common.Editor;
using Common.PivotGroup;
using Game.Core;
using Game.Damage;
using Game.Factories;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.LevelGeneration;
using Unliving.Mobs.Animation;

namespace Unliving.Mobs
{
	// Token: 0x020001DA RID: 474
	[Serializable]
	public sealed class GameMobsSummoner : ICloneable<GameMobsSummoner>
	{
		// Token: 0x1700030E RID: 782
		// (get) Token: 0x06000F55 RID: 3925 RVA: 0x000305C7 File Offset: 0x0002E7C7
		public bool IsActive
		{
			get
			{
				return this.summonableMobID != MobBehaviour.ID.None || this.summonableMobPrefab != null;
			}
		}

		// Token: 0x06000F56 RID: 3926 RVA: 0x000305E0 File Offset: 0x0002E7E0
		private bool TryRestoreSummonedMob(Vector2 summoningPosition, IGameMob summoner, out BaseGameMob restoredMob)
		{
			restoredMob = null;
			if (this.additionalSummoningCheckRange > 0f)
			{
				GameMobFactions faction = (this.summonedMobsFactionOverride != GameMobFactions.None) ? this.summonedMobsFactionOverride : summoner.Faction;
				GameMobsSummoner.MobsRestoringCheckArgs.position = summoningPosition;
				GameMobsSummoner.MobsRestoringCheckArgs.range = this.additionalSummoningCheckRange;
				GameMobsSummoner.MobsRestoringCheckArgs.layers = 1 << this.mobsFactory.GetFactionInfo(faction).mobsLayer;
				BaseGameMob[] array;
				int mobsInRange = summoner.CurrentLocation.GetMobsInRange(GameMobsSummoner.MobsRestoringCheckArgs, out array);
				for (int i = 0; i < mobsInRange; i++)
				{
					BaseGameMob baseGameMob = array[i];
					GameMobSummoningContext summonerInfo = baseGameMob.SummonerInfo;
					if (((summonerInfo != null) ? summonerInfo.summoner : null) == summoner)
					{
						if (this.summonableMobPrefab != null)
						{
							if (baseGameMob.MobPrototypeObject != this.summonableMobPrefab)
							{
								goto IL_152;
							}
						}
						else
						{
							MobBehaviour mobBehaviour = baseGameMob as MobBehaviour;
							MobBehaviour.ID? id = (mobBehaviour != null) ? new MobBehaviour.ID?(mobBehaviour.ObjectID) : null;
							MobBehaviour.ID id2 = this.summonableMobID;
							if (!(id.GetValueOrDefault() == id2 & id != null))
							{
								goto IL_152;
							}
						}
						restoredMob = baseGameMob;
						IDamageable hitPointsController = baseGameMob.HitPointsController;
						if (hitPointsController != null)
						{
							float num = hitPointsController.MaxHitPoints - hitPointsController.CurrentHitPoints;
							if (num > 0f)
							{
								GameMobsSummoner.MobsHPRestoringArgs.amount = num + 1f;
								hitPointsController.ModifyHitPoints(summoner, GameMobsSummoner.MobsHPRestoringArgs);
								break;
							}
						}
					}
					IL_152:;
				}
			}
			return restoredMob != null;
		}

		// Token: 0x06000F57 RID: 3927 RVA: 0x00030752 File Offset: 0x0002E952
		public void SetMobsFactory(IGameMobsFactory mobsFactory)
		{
			this.mobsFactory = mobsFactory;
		}

		// Token: 0x06000F58 RID: 3928 RVA: 0x0003075B File Offset: 0x0002E95B
		public bool SetMobsFactory(IGame game)
		{
			this.SetMobsFactory((game != null) ? game.Services.Get<IGameMobsFactory>() : null);
			return this.mobsFactory != null;
		}

		// Token: 0x06000F59 RID: 3929 RVA: 0x0003077D File Offset: 0x0002E97D
		public bool SetMobsFactory(IGameBehaviour gameBehaviour)
		{
			return this.SetMobsFactory(gameBehaviour.CurrentGame);
		}

		// Token: 0x06000F5A RID: 3930 RVA: 0x0003078C File Offset: 0x0002E98C
		public IGameMob SummonMob(GameMobSummoningContext summoningContext, Vector2 targetPosition, out bool isRestoredMob, bool addRandomOffset = false)
		{
			isRestoredMob = false;
			if (this.summonableMobPrefab == null && this.summonableMobID == MobBehaviour.ID.None)
			{
				return null;
			}
			IGameMob summoner = summoningContext.summoner;
			if (this.mobsFactory == null && !this.SetMobsFactory(summoner as IGameBehaviour))
			{
				return null;
			}
			if (!string.IsNullOrEmpty(this.summoningPivot))
			{
				IPivotGroupProvider<string> pivotGroupProvider = summoningContext.summoner as IPivotGroupProvider<string>;
				IPivot pivot = (pivotGroupProvider != null) ? pivotGroupProvider.PivotGroup.GetPivot(this.summoningPivot) : null;
				if (pivot != null)
				{
					targetPosition = pivot.WorldPosition;
				}
			}
			BaseGameMob result;
			if (this.TryRestoreSummonedMob(targetPosition, summoner, out result))
			{
				return result;
			}
			GameMobsSummoner.SpawningArgs.spawner = summoner;
			GameMobsSummoner.SummoningArgs.mobID = this.summonableMobID;
			GameMobsSummoner.SummoningArgs.arbitraryMobPrefab = this.summonableMobPrefab;
			GameMobsSummoner.SummoningArgs.mobFaction = this.summonedMobsFactionOverride;
			GameMobsSummoner.SummoningArgs.spawnPosition = targetPosition;
			IGameMob gameMob = this.mobsFactory.SummonMob(summoningContext, GameMobsSummoner.SummoningArgs, this.summonToIndividualGroups, this.summonedMobsLifetime, false);
			if (this.killSummonedMobsWithSummoner)
			{
				BaseGameMob baseGameMob = gameMob as BaseGameMob;
				if (baseGameMob != null)
				{
					baseGameMob.KillWithSummoner();
				}
			}
			if (gameMob != null)
			{
				if (addRandomOffset)
				{
					MobBehaviourSpawner.AddRandomOffset(gameMob);
				}
				if (this.inheritSummonerLookDirection)
				{
					BaseGameMob baseGameMob2 = summoner as BaseGameMob;
					if (baseGameMob2 != null)
					{
						GameObject gameObject = gameMob.GameObject;
						Transform transform = (gameObject != null) ? gameObject.transform : null;
						if (transform != null)
						{
							GameMobAnimationController.SetLookDirection(transform, baseGameMob2.CurrentLookDirection.x);
						}
					}
				}
			}
			return gameMob;
		}

		// Token: 0x06000F5B RID: 3931 RVA: 0x000308FC File Offset: 0x0002EAFC
		public GameMobsSummoner Clone()
		{
			return new GameMobsSummoner
			{
				summonableMobID = this.summonableMobID,
				summonableMobPrefab = this.summonableMobPrefab,
				summonedMobsFactionOverride = this.summonedMobsFactionOverride,
				summonedMobsLifetime = this.summonedMobsLifetime,
				additionalSummoningCheckRange = this.additionalSummoningCheckRange,
				summonToIndividualGroups = this.summonToIndividualGroups,
				inheritSummonerLookDirection = this.inheritSummonerLookDirection,
				killSummonedMobsWithSummoner = this.killSummonedMobsWithSummoner
			};
		}

		// Token: 0x040008F8 RID: 2296
		private static readonly GameMobSpawningInfo SpawningArgs = new GameMobSpawningInfo
		{
			isAggressiveMob = true,
			isAggressionReactiveMob = true,
			canShareAggression = true
		};

		// Token: 0x040008F9 RID: 2297
		private static readonly MobBehaviour.FactoryArgs SummoningArgs = new MobBehaviour.FactoryArgs
		{
			spawnerInfo = GameMobsSummoner.SpawningArgs
		};

		// Token: 0x040008FA RID: 2298
		private static readonly GameLocation.MobsGatheringArgs MobsRestoringCheckArgs = new GameLocation.MobsGatheringArgs();

		// Token: 0x040008FB RID: 2299
		private static readonly HitPointsController.HPChangingArgs MobsHPRestoringArgs = new HitPointsController.HPChangingArgs(false)
		{
			isForcedChanging = true,
			isSilentChanging = true,
			disableTargetReaction = true
		};

		// Token: 0x040008FC RID: 2300
		[FormerlySerializedAs("summonableMobsID")]
		[ObjectFactoryIDPopup(typeof(IGameMob))]
		public MobBehaviour.ID summonableMobID;

		// Token: 0x040008FD RID: 2301
		public GameObject summonableMobPrefab;

		// Token: 0x040008FE RID: 2302
		[FormerlySerializedAs("summonedMobsFaction")]
		public GameMobFactions summonedMobsFactionOverride = GameMobFactions.None;

		// Token: 0x040008FF RID: 2303
		public float summonedMobsLifetime = 10f;

		// Token: 0x04000900 RID: 2304
		[Tag]
		public string summoningPivot;

		// Token: 0x04000901 RID: 2305
		[Tooltip("При значениях > 0 будут совершаться попытки восстановить ХП призванных мобов в данном рейндже вместо призыва новых.")]
		public float additionalSummoningCheckRange = -1f;

		// Token: 0x04000902 RID: 2306
		public bool summonToIndividualGroups;

		// Token: 0x04000903 RID: 2307
		public bool inheritSummonerLookDirection;

		// Token: 0x04000904 RID: 2308
		public bool killSummonedMobsWithSummoner;

		// Token: 0x04000905 RID: 2309
		private IGameMobsFactory mobsFactory;
	}
}
