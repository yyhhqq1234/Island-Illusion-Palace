using System;
using System.Collections.Generic;
using Common;
using Common.Factories;
using Common.ServiceRegistry;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.Stats;
using UnityEngine;
using UnityEngine.AI;
using Unliving.Abilities;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.Mobs
{
	// Token: 0x020001BD RID: 445
	[Service(typeof(GameMobsFactory), new Type[]
	{
		typeof(IGameMobsFactory)
	})]
	public sealed class GameMobsFactory : GameMobsFactoryBase<MobBehaviour.FactoryPrototype>, IGameMobsFactory, IObjectFactory<IGameMob>, IFactory<IBaseObjectDescription, IGameMob>, IFactory
	{
		// Token: 0x06000DA3 RID: 3491 RVA: 0x0002B3C4 File Offset: 0x000295C4
		private Collider2D[] GetPlayerColliders()
		{
			PlayerBehaviour playerBehaviour;
			if (this.currentPlayer == null && this.currentGame.TryGetPlayer(out playerBehaviour))
			{
				this.playerColliders = playerBehaviour.GetComponentsInChildren<Collider2D>();
				this.currentPlayer = playerBehaviour;
			}
			return this.playerColliders;
		}

		// Token: 0x06000DA4 RID: 3492 RVA: 0x0002B407 File Offset: 0x00029607
		protected override GameMobFactionInfo GetMobFactionInfo(BaseGameMob mob, GameMobFactions faction)
		{
			return this.GetFactionInfo(faction);
		}

		// Token: 0x06000DA5 RID: 3493 RVA: 0x0002B410 File Offset: 0x00029610
		protected override void SetMobParams(MobBehaviour.FactoryPrototype mobData, GameObject mobPrefab, IGameMob mob, GameMobsFactoryArgsBase args)
		{
			base.SetMobParams(mobData, mobPrefab, mob, args);
			bool flag = mobData != null;
			if (flag)
			{
				BaseGameMob baseGameMob = mob as BaseGameMob;
				if (baseGameMob != null)
				{
					baseGameMob.mobName = mobData.MobName;
					baseGameMob.impulseDamping = mobData.impulseDamping;
					baseGameMob.maxAttackersCount = mobData.attackData.MaxAttackersCount;
					if (mobData.disableCrowdInteraction)
					{
						baseGameMob.isCrowdObstacle = false;
						if (baseGameMob.NavMeshAgent != null)
						{
							baseGameMob.NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
						}
						Collider2D[] array = this.GetPlayerColliders();
						if (array != null)
						{
							Collider2D hitCollider = baseGameMob.HitCollider;
							Collider2D[] array2 = array;
							for (int i = 0; i < array2.Length; i++)
							{
								Physics2D.IgnoreCollision(array2[i], hitCollider);
							}
						}
					}
					if (mobData.crowdPassPriorityOverride != 0)
					{
						baseGameMob.CrowdPassPriority = (float)mobData.crowdPassPriorityOverride;
					}
					if (baseGameMob.auraEffectAbility == null)
					{
						BaseGameMob baseGameMob2 = baseGameMob;
						IGameAbilitiesFactory gameAbilitiesFactory = this.abilitiesFactory;
						BaseAbility auraEffectAbility;
						if (gameAbilitiesFactory == null)
						{
							auraEffectAbility = null;
						}
						else
						{
							AbilityFactoryPrototype abilityPrototypeData = gameAbilitiesFactory.GetAbilityPrototypeData((int)mobData.AuraEffectAbility);
							auraEffectAbility = ((abilityPrototypeData != null) ? abilityPrototypeData.abilityPrototype : null);
						}
						baseGameMob2.auraEffectAbility = auraEffectAbility;
					}
				}
			}
			MobBehaviour mobBehaviour = mob as MobBehaviour;
			if (mobBehaviour != null)
			{
				MobBehaviour mobBehaviour2 = mobBehaviour;
				mobBehaviour2.crowdObstaclesLayers |= (mobBehaviour.LayerMask | mobBehaviour.enemyMobLayers | this.crowdCollisionLayers);
				MobBehaviour mobBehaviour3 = mobBehaviour;
				mobBehaviour3.crowdObstaclesLayers &= ~mobBehaviour.ignorableCrowdObstaclesLayers;
				if (flag)
				{
					mobBehaviour.ObjectID = mobData.objectID;
					mobBehaviour.ZombieMobID = mobData.reviveID;
					mobBehaviour.revivingEXPAmount = mobData.revivingEXPAmount;
					mobBehaviour.ActivationCost = mobData.activationCost;
					mobBehaviour.activationEnergyReturnAmount = mobData.activationEnergyReturnAmount;
					mobBehaviour.ActivationReward = (float)mobData.activationReward;
					mobBehaviour.MeleeAttackAbility = mobData.MeleeAttackAbility;
					mobBehaviour.RangeAttackAbility = mobData.RangeAttackAbility;
					mobBehaviour.Slot3 = mobData.Slot3;
					mobBehaviour.Slot4 = mobData.Slot4;
					mobBehaviour.Slot5 = mobData.Slot5;
					mobBehaviour.Slot6 = mobData.Slot6;
				}
			}
		}

		// Token: 0x06000DA6 RID: 3494 RVA: 0x0002B60C File Offset: 0x0002980C
		public GameMobsFactory(IEnumerable<GameMobFactionInfo> mobFactionsInfo, BaseGameMob.ResourcesGeneratorData globalResourcesGeneratorData) : base(globalResourcesGeneratorData)
		{
			this.mobFactionsInfo = new Dictionary<int, GameMobFactionInfo>();
			if (mobFactionsInfo != null)
			{
				foreach (GameMobFactionInfo gameMobFactionInfo in mobFactionsInfo)
				{
					if (gameMobFactionInfo.faction != GameMobFactions.None)
					{
						this.mobFactionsInfo.Add((int)gameMobFactionInfo.faction, gameMobFactionInfo);
					}
				}
			}
		}

		// Token: 0x06000DA7 RID: 3495 RVA: 0x0002B680 File Offset: 0x00029880
		public override void Initialize(IGame game)
		{
			base.Initialize(game);
			this.currentGame = game;
			game.Services.TryGet<IGameAbilitiesFactory>(out this.abilitiesFactory);
			foreach (KeyValuePair<int, MobBehaviour.FactoryPrototype> keyValuePair in this.ObjectsPrototypesData)
			{
				MobBehaviour.FactoryPrototype value = keyValuePair.Value;
				GameObject objectPrefab = value.objectPrefab;
				if (!(objectPrefab == null))
				{
					AbilityID[] array;
					int abilities = value.GetAbilities(out array);
					int i = 0;
					while (i < abilities)
					{
						AbilityID abilityID = array[i];
						AbilityFactoryPrototype abilityPrototypeData = this.abilitiesFactory.GetAbilityPrototypeData((int)abilityID);
						BaseAbility baseAbility = (abilityPrototypeData != null) ? abilityPrototypeData.abilityPrototype : null;
						MobActivationAbilityType activationType;
						if (!(baseAbility == null) && baseAbility.IsMobActivationAbility(out activationType))
						{
							MobBehaviour mobBehaviour;
							if (objectPrefab.TryGetComponent<MobBehaviour>(out mobBehaviour))
							{
								mobBehaviour.activationType = activationType;
								break;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
			}
		}

		// Token: 0x06000DA8 RID: 3496 RVA: 0x0002B774 File Offset: 0x00029974
		public GameMobFactionInfo GetFactionInfo(GameMobFactions faction)
		{
			GameMobFactionInfo result;
			if (this.mobFactionsInfo.TryGetValue((int)faction, out result))
			{
				return result;
			}
			return GameMobFactionInfo.GetInvalidInfo();
		}

		// Token: 0x06000DA9 RID: 3497 RVA: 0x0002B798 File Offset: 0x00029998
		public IEnumerable<GameMobFactionInfo> GetEnemyFactionsInfo(int mobLayer)
		{
			foreach (GameMobFactionInfo gameMobFactionInfo in this.mobFactionsInfo.Values)
			{
				if (gameMobFactionInfo.IsEnemyFaction(mobLayer))
				{
					yield return gameMobFactionInfo;
				}
			}
			Dictionary<int, GameMobFactionInfo>.ValueCollection.Enumerator enumerator = default(Dictionary<int, GameMobFactionInfo>.ValueCollection.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000DAA RID: 3498 RVA: 0x0002B7AF File Offset: 0x000299AF
		public IGameMob Create(GameMobsFactoryArgsBase args)
		{
			return this.Create(base.GetObjectPrototype(args.ObjectID), args);
		}

		// Token: 0x06000DAB RID: 3499 RVA: 0x0002B7C4 File Offset: 0x000299C4
		public IGameMob SummonMob(GameMobSummoningContext context, GameMobsGroupControllerBase group, GameMobsFactoryArgsBase factoryArgs, float lifetime = -1f, bool canBeHealed = false)
		{
			IGameMob gameMob = this.Create(factoryArgs);
			if (gameMob != null)
			{
				gameMob.SetCreationType(GameMobCreationType.Summoned, context);
				Component component = context.summoner as Component;
				if (component != null)
				{
					GameObject gameObject = gameMob.GameObject;
					((gameObject != null) ? gameObject.transform : null).parent = component.transform.parent;
				}
				HitPointsController hitPointsController = gameMob.HitPointsController as HitPointsController;
				if (!canBeHealed && hitPointsController != null)
				{
					hitPointsController.ModifyHealingResistance(1f);
				}
				BaseGameMob baseGameMob = gameMob as BaseGameMob;
				if (baseGameMob != null)
				{
					if (group != null)
					{
						group.AddMob(baseGameMob, null);
					}
				}
				else
				{
					gameMob.Group = group;
				}
				if (lifetime > 0f)
				{
					ILifetimeDependent lifetimeDependent = gameMob as ILifetimeDependent;
					if (lifetimeDependent != null)
					{
						lifetimeDependent.Lifetime = lifetime;
					}
					else if (hitPointsController != null)
					{
						float num = hitPointsController.MaxHitPoints / lifetime;
						if (num != 0f)
						{
							IBuffsController buffsController = gameMob.BuffsController;
							if (buffsController != null)
							{
								buffsController.AddBuff(new DamageDebuff(gameMob, num, null, true)
								{
									isForcedDamageDebuff = true,
									isSilentDebuff = true
								});
							}
						}
					}
				}
				IStatsOwner<MobStatModifier> statsOwner = gameMob as IStatsOwner<MobStatModifier>;
				IStatsController<MobStatModifier> statsController = (statsOwner != null) ? statsOwner.StatsController : null;
				if (statsController != null)
				{
					ValueTuple<MobStatID, MobStatModifier>[] statsModifiers = context.statsModifiers;
					if (statsModifiers != null && statsModifiers.Length != 0)
					{
						foreach (ValueTuple<MobStatID, MobStatModifier> valueTuple in statsModifiers)
						{
							MobStatID item = valueTuple.Item1;
							MobStatModifier item2 = valueTuple.Item2;
							statsController.AddModifier((int)item, item2);
						}
					}
					else
					{
						MobStatModifier totalDamageModifier = context.summoner.GetTotalDamageModifier();
						statsController.AddModifier(2, totalDamageModifier);
					}
				}
			}
			return gameMob;
		}

		// Token: 0x040007EE RID: 2030
		public int crowdCollisionLayers;

		// Token: 0x040007EF RID: 2031
		private readonly Dictionary<int, GameMobFactionInfo> mobFactionsInfo;

		// Token: 0x040007F0 RID: 2032
		private IGame currentGame;

		// Token: 0x040007F1 RID: 2033
		private IGameAbilitiesFactory abilitiesFactory;

		// Token: 0x040007F2 RID: 2034
		private PlayerBehaviour currentPlayer;

		// Token: 0x040007F3 RID: 2035
		private Collider2D[] playerColliders;
	}
}
