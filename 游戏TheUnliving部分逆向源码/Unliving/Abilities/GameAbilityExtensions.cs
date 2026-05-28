using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Abilities.TargetsCollection;
using Game.Core;
using Game.Damage;
using Game.Damage.Projectiles;
using Game.InputManager;
using Game.Stats;
using UnityEngine;
using Unliving.Abilities.Modifiers;
using Unliving.AbilityResources;
using Unliving.LeveledItems;
using Unliving.LevelGeneration;
using Unliving.Mobs;
using Unliving.Mobs.AbilityTriggers;
using Unliving.Mobs.Motion;
using Unliving.MobsStats;
using Unliving.Player;

namespace Unliving.Abilities
{
	// Token: 0x020003A5 RID: 933
	public static class GameAbilityExtensions
	{
		// Token: 0x06001EBA RID: 7866 RVA: 0x000614A0 File Offset: 0x0005F6A0
		private static BaseGameMob CastToMobBehaviour(Component sourceComponent)
		{
			BaseGameMob result;
			sourceComponent.TryGetComponent<BaseGameMob>(out result);
			return result;
		}

		// Token: 0x06001EBB RID: 7867 RVA: 0x000614B7 File Offset: 0x0005F6B7
		private static bool AbilityLockingPredicate(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			return false;
		}

		// Token: 0x06001EBC RID: 7868 RVA: 0x000614BA File Offset: 0x0005F6BA
		private static void PassSortingComparer(GameLocation.MobsGatheringArgs args, bool sortTargets)
		{
			if (sortTargets)
			{
				args.sortingComparer = AbilityTargetsCollectionArgs.DefaultTargetsComparer2D;
				return;
			}
			args.sortingComparer = null;
		}

		// Token: 0x06001EBD RID: 7869 RVA: 0x000614D4 File Offset: 0x0005F6D4
		private static bool HasAbilityWithID(this BaseAbilitiesController abilitiesController, int abilityID)
		{
			IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i].ID == abilityID)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EBE RID: 7870 RVA: 0x0006150C File Offset: 0x0005F70C
		private static float GetTriggerRange(BaseAbility ability, bool searchMaxRange, bool checkTargetedTriggersOnly)
		{
			IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
			float num = searchMaxRange ? 0f : float.PositiveInfinity;
			for (int i = 0; i < extensions.Count; i++)
			{
				IMobAbilityTrigger mobAbilityTrigger = extensions[i] as IMobAbilityTrigger;
				if (mobAbilityTrigger != null && (!checkTargetedTriggersOnly || mobAbilityTrigger.RequiresTarget))
				{
					float activationRange = mobAbilityTrigger.ActivationRange;
					if (searchMaxRange ? (activationRange > num) : (activationRange < num))
					{
						num = activationRange;
					}
				}
			}
			if (searchMaxRange || !float.IsInfinity(num))
			{
				return num;
			}
			return 0f;
		}

		// Token: 0x06001EBF RID: 7871 RVA: 0x0006158C File Offset: 0x0005F78C
		public static IAbility[] InitializeChildAbilitiesAsPrototypes<TChild>(this IList<TChild> childAbilities, ref IAbility[] targetArray) where TChild : IAbilityDecorator
		{
			if (targetArray == null)
			{
				if (childAbilities != null && childAbilities.Count != 0)
				{
					targetArray = new IAbility[childAbilities.Count];
					for (int i = 0; i < targetArray.Length; i++)
					{
						IAbility[] array = targetArray;
						int num = i;
						TChild tchild = childAbilities[i];
						array[num] = tchild.AbilityPrototype;
					}
				}
				else
				{
					targetArray = Array.Empty<IAbility>();
				}
			}
			return targetArray;
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x000615E8 File Offset: 0x0005F7E8
		public static bool HasID(this BaseAbility ability)
		{
			return ability.ID > 0;
		}

		// Token: 0x06001EC1 RID: 7873 RVA: 0x000615F3 File Offset: 0x0005F7F3
		public static bool IsInstantiated(this BaseAbility ability)
		{
			return ability.Prototype != null;
		}

		// Token: 0x06001EC2 RID: 7874 RVA: 0x00061601 File Offset: 0x0005F801
		public static bool IsHPContainerAbility(this AbilityID abilityID)
		{
			return abilityID >= AbilityID.AAC_UndeadExchangeAbility;
		}

		// Token: 0x06001EC3 RID: 7875 RVA: 0x0006160E File Offset: 0x0005F80E
		public static bool IsHPContainerAbility(this BaseAbility ability)
		{
			return ((AbilityID)ability.ID).IsHPContainerAbility();
		}

		// Token: 0x06001EC4 RID: 7876 RVA: 0x0006161C File Offset: 0x0005F81C
		public static IAbilitiesController GetController(this BaseAbility ability)
		{
			if (ability.CurrentController != null)
			{
				return ability.CurrentController;
			}
			BaseAbility parentAbility = ability.ParentAbility;
			IAbilitiesController abilitiesController = (parentAbility != null) ? parentAbility.CurrentController : null;
			IAbilitiesController result;
			if ((result = abilitiesController) == null)
			{
				BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
				if (baseGameMob == null)
				{
					return null;
				}
				result = baseGameMob.AbilitiesController;
			}
			return result;
		}

		// Token: 0x06001EC5 RID: 7877 RVA: 0x0006166D File Offset: 0x0005F86D
		public static bool ShouldBeActivatedByOwner(this BaseAbility ability)
		{
			return !ability.IsAutoUseAbility && !ability.IsPostMortemAbility;
		}

		// Token: 0x06001EC6 RID: 7878 RVA: 0x00061684 File Offset: 0x0005F884
		public static bool ShouldBeActivatedByOwner(this IAbility ability)
		{
			BaseAbility baseAbility = ability as BaseAbility;
			GameAbilityUsingContext gameAbilityUsingContext;
			return baseAbility == null || (baseAbility.ShouldBeActivatedByOwner() && (!baseAbility.TryGetExplicitUsingContext(out gameAbilityUsingContext) || gameAbilityUsingContext != GameAbilityUsingContext.SpecialUseCaseAbility));
		}

		// Token: 0x06001EC7 RID: 7879 RVA: 0x000616BA File Offset: 0x0005F8BA
		public static void SetReloadingProgress(this BaseAbility ability, float progress)
		{
			ability.ModifyReloadingProgress(ability.ReloadingTime * (progress - ability.ReloadingProgress), true);
		}

		// Token: 0x06001EC8 RID: 7880 RVA: 0x000616D4 File Offset: 0x0005F8D4
		public static int GetMaxTargetsInRangeCount(this BaseAbility ability)
		{
			Ability ability2 = ability as Ability;
			if (ability2 != null)
			{
				return ability2.MaxTargetsInRange;
			}
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase == null)
			{
				return -1;
			}
			return projectileAbilityBase.maxProjectileEffectTargets;
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x00061708 File Offset: 0x0005F908
		public static float GetAbilityEffectRange(this IAbility ability)
		{
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase == null)
			{
				return ability.Range;
			}
			return projectileAbilityBase.ProjectileEffectRange;
		}

		// Token: 0x06001ECA RID: 7882 RVA: 0x0006172C File Offset: 0x0005F92C
		public static bool HasAbilityWithPrototype(this BaseAbilitiesController abilitiesController, BaseAbility abilityPrototype)
		{
			IReadOnlyList<BaseAbility> abilities = abilitiesController.Abilities;
			for (int i = 0; i < abilities.Count; i++)
			{
				if (abilities[i].Prototype == abilityPrototype)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001ECB RID: 7883 RVA: 0x00061768 File Offset: 0x0005F968
		public static bool HasAbilityWithID(this GameAbilitiesController abilitiesController, AbilityID abilityID)
		{
			return abilityID != AbilityID.None && abilitiesController.HasAbilityWithID((int)abilityID);
		}

		// Token: 0x06001ECC RID: 7884 RVA: 0x00061778 File Offset: 0x0005F978
		public static void RemoveAllAbilitiesExtenders<TExtender>(this BaseAbilitiesController abilitiesController) where TExtender : AbilityExtensionAssetBase
		{
			IReadOnlyList<BaseAbility> readOnlyList = (abilitiesController != null) ? abilitiesController.Abilities : null;
			if (readOnlyList != null)
			{
				for (int i = 0; i < readOnlyList.Count; i++)
				{
					readOnlyList[i].RemoveExtension<TExtender>();
				}
			}
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x000617B3 File Offset: 0x0005F9B3
		public static MobAbilityParameters GetMobAbilityParams(this BaseAbility ability)
		{
			if (ability == null)
			{
				return null;
			}
			return ability.GetExtension<MobAbilityParameters>();
		}

		// Token: 0x06001ECE RID: 7886 RVA: 0x000617C0 File Offset: 0x0005F9C0
		public static PlayerAbilityParameters GetPlayerAbilityParams(this BaseAbility ability)
		{
			if (ability == null)
			{
				return null;
			}
			return ability.GetExtension<PlayerAbilityParameters>();
		}

		// Token: 0x06001ECF RID: 7887 RVA: 0x000617D0 File Offset: 0x0005F9D0
		public static bool IsProjectileAbility(this IAbility ability, bool checkCompositeAbility = false)
		{
			if (GameAbilityExtensions.<IsProjectileAbility>g__IsTargetAbility|24_0(ability))
			{
				return true;
			}
			if (checkCompositeAbility)
			{
				ICompositeAbility compositeAbility = ability as ICompositeAbility;
				if (compositeAbility != null)
				{
					return compositeAbility.Any(new Predicate<IAbility>(GameAbilityExtensions.<IsProjectileAbility>g__IsTargetAbility|24_0));
				}
			}
			return false;
		}

		// Token: 0x06001ED0 RID: 7888 RVA: 0x00061808 File Offset: 0x0005FA08
		public static bool TryGetProjectilesLaunchCenter(this ProjectileAbilityBase ability, out Vector2 launchCenter, bool isLocal = false)
		{
			IList<ProjectileAbilityBase.ProjectileLaunchPoint> projectileLaunchPoints = ability.ProjectileLaunchPoints;
			int num = (projectileLaunchPoints != null) ? projectileLaunchPoints.Count : 0;
			launchCenter = default(Vector2);
			if (num != 0)
			{
				for (int i = 0; i < num; i++)
				{
					launchCenter += projectileLaunchPoints[i].LocalPosition;
				}
				launchCenter /= (float)num;
				if (!isLocal)
				{
					launchCenter = projectileLaunchPoints[0].Transform.TransformPoint(launchCenter);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001ED1 RID: 7889 RVA: 0x0006189C File Offset: 0x0005FA9C
		public static bool HasActiveProjectiles(this IAbility ability)
		{
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			return projectileAbilityBase != null && projectileAbilityBase.LaunchedProjectilesCount != 0;
		}

		// Token: 0x06001ED2 RID: 7890 RVA: 0x000618C0 File Offset: 0x0005FAC0
		[return: TupleElementNames(new string[]
		{
			"obstacleLayers",
			"hitLayers"
		})]
		public static ValueTuple<int, int> GetProjectileLayers(this BaseAbility ability, bool checkCompositeAbility = false)
		{
			int item = 0;
			int item2 = 0;
			GameAbilityExtensions.<GetProjectileLayers>g__AddLayers|27_0(ability as ProjectileAbilityBase, ref item, ref item2);
			if (checkCompositeAbility)
			{
				ICompositeAbility compositeAbility = ability as ICompositeAbility;
				if (compositeAbility != null)
				{
					IList<IAbility> childAbilities = compositeAbility.ChildAbilities;
					for (int i = 0; i < childAbilities.Count; i++)
					{
						GameAbilityExtensions.<GetProjectileLayers>g__AddLayers|27_0(childAbilities[i] as ProjectileAbilityBase, ref item, ref item2);
					}
				}
			}
			return new ValueTuple<int, int>(item, item2);
		}

		// Token: 0x06001ED3 RID: 7891 RVA: 0x00061926 File Offset: 0x0005FB26
		public static float GetProjectileRadius(this ProjectileAbilityBase projectileAbility)
		{
			BaseProjectile baseProjectile = projectileAbility.ProjectilePrototype as BaseProjectile;
			if (baseProjectile == null)
			{
				return 0f;
			}
			return baseProjectile.HitDetectionRadius;
		}

		// Token: 0x06001ED4 RID: 7892 RVA: 0x00061944 File Offset: 0x0005FB44
		public static bool IsDamagingAbility(this BaseAbility ability, BaseGameMob damageableTarget = null)
		{
			IDamageSender damageSender = ability as IDamageSender;
			DamageGenerator damageGenerator = (damageSender != null) ? damageSender.DamageGenerator : null;
			return damageGenerator != null && damageGenerator.HasDamage() && (damageableTarget == null || damageableTarget.InLayerMask(ability.ValidObjectLayers));
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x00061990 File Offset: 0x0005FB90
		public static bool IsDamagingAbility(this BaseAbility ability, out float maxDamage)
		{
			IDamageSender damageSender = ability as IDamageSender;
			DamageGenerator damageGenerator = (damageSender != null) ? damageSender.DamageGenerator : null;
			if (damageGenerator != null)
			{
				float num = Mathf.Max(damageGenerator.amount, damageGenerator.maxAmount);
				if (num > 0f)
				{
					maxDamage = num;
					return true;
				}
			}
			maxDamage = 0f;
			return false;
		}

		// Token: 0x06001ED6 RID: 7894 RVA: 0x000619DA File Offset: 0x0005FBDA
		public static bool IsDamagingAbilityEffect(this AbilityEffectBase abilityEffect)
		{
			IDamageSender damageSender = abilityEffect as IDamageSender;
			return ((damageSender != null) ? damageSender.DamageGenerator : null) != null;
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x000619F4 File Offset: 0x0005FBF4
		public static void GetMaxAbilityEffectRange(this BaseAbility ability, out float maxRange, out float maxDamageRange)
		{
			maxRange = 0f;
			maxDamageRange = 0f;
			ICompositeAbility compositeAbility = ability as ICompositeAbility;
			float num;
			if (compositeAbility != null)
			{
				IList<IAbility> childAbilities = compositeAbility.ChildAbilities;
				for (int i = 0; i < childAbilities.Count; i++)
				{
					BaseAbility baseAbility = (BaseAbility)childAbilities[i];
					float abilityEffectRange = baseAbility.GetAbilityEffectRange();
					if (abilityEffectRange > maxRange)
					{
						maxRange = abilityEffectRange;
					}
					if (baseAbility.IsDamagingAbility(out num) && abilityEffectRange > maxDamageRange)
					{
						maxDamageRange = abilityEffectRange;
					}
				}
				return;
			}
			maxRange = ability.GetAbilityEffectRange();
			if (ability.IsDamagingAbility(out num))
			{
				maxDamageRange = maxRange;
			}
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x00061A74 File Offset: 0x0005FC74
		[return: TupleElementNames(new string[]
		{
			"jumpHeight",
			"jumpDistance"
		})]
		public static ValueTuple<float, float> GetJumpAbilityParams(this BaseAbility ability)
		{
			IGameMobJumpMotion gameMobJumpMotion = ability as IGameMobJumpMotion;
			if (gameMobJumpMotion != null)
			{
				return new ValueTuple<float, float>(gameMobJumpMotion.MaxHeight, gameMobJumpMotion.MaxDistance);
			}
			return new ValueTuple<float, float>(0f, 0f);
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x00061AAC File Offset: 0x0005FCAC
		public static bool IsJumpMotionAbility(this BaseAbility ability, out float jumpHeight, out float jumpDistance)
		{
			ValueTuple<float, float> jumpAbilityParams = ability.GetJumpAbilityParams();
			jumpHeight = jumpAbilityParams.Item1;
			jumpDistance = jumpAbilityParams.Item2;
			return jumpHeight > 0f || jumpDistance > 0f;
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x00061AE4 File Offset: 0x0005FCE4
		public static bool IsJumpMotionAbility(this BaseAbility ability)
		{
			float num;
			float num2;
			return ability.IsJumpMotionAbility(out num, out num2);
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x00061AFC File Offset: 0x0005FCFC
		[return: TupleElementNames(new string[]
		{
			"maxJumpHeight",
			"maxJumpDistance"
		})]
		public static ValueTuple<float, float> GetMaxJumpMotionEffectParams(this EffectBasedAbility ability)
		{
			ValueTuple<float, float> valueTuple = new ValueTuple<float, float>(0f, 0f);
			AbilityEffectBase[] abilityEffects = ability.AbilityEffects;
			for (int i = 0; i < abilityEffects.Length; i++)
			{
				IGameMobJumpMotion gameMobJumpMotion = abilityEffects[i] as IGameMobJumpMotion;
				if (gameMobJumpMotion != null)
				{
					if (gameMobJumpMotion.MaxHeight > valueTuple.Item1)
					{
						valueTuple.Item1 = gameMobJumpMotion.MaxHeight;
					}
					if (gameMobJumpMotion.MaxDistance > valueTuple.Item2)
					{
						valueTuple.Item2 = gameMobJumpMotion.MaxDistance;
					}
				}
			}
			return valueTuple;
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x00061B74 File Offset: 0x0005FD74
		public static bool TryGetExplicitUsingContext(this BaseAbility ability, out GameAbilityUsingContext usingContext)
		{
			usingContext = GameAbilityUsingContext.Auto;
			MobAbilityParameters mobAbilityParams = ability.GetMobAbilityParams();
			if (mobAbilityParams != null && mobAbilityParams.usingContext != GameAbilityUsingContext.Auto)
			{
				usingContext = mobAbilityParams.usingContext;
				return true;
			}
			GameAbilityExtensions.IExplicitUsingContextAbility explicitUsingContextAbility = ability as GameAbilityExtensions.IExplicitUsingContextAbility;
			if (explicitUsingContextAbility != null)
			{
				usingContext = explicitUsingContextAbility.UsingContext;
			}
			return usingContext > GameAbilityUsingContext.Auto;
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x00061BC0 File Offset: 0x0005FDC0
		public static bool IsBattleAbility(this BaseAbility ability)
		{
			GameAbilityUsingContext gameAbilityUsingContext;
			return (ability.TryGetExplicitUsingContext(out gameAbilityUsingContext) && gameAbilityUsingContext == GameAbilityUsingContext.BattleAbility) || ability.IsDamagingAbility(null);
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x00061BE4 File Offset: 0x0005FDE4
		public static bool IsPlayerMainBattleAbility(this BaseAbility ability)
		{
			PlayerAbilityParameters playerAbilityParameters;
			if ((playerAbilityParameters = ability.GetPlayerAbilityParams()) == null)
			{
				BaseAbility parentAbility = ability.ParentAbility;
				playerAbilityParameters = ((parentAbility != null) ? parentAbility.GetPlayerAbilityParams() : null);
			}
			PlayerAbilityParameters playerAbilityParameters2 = playerAbilityParameters;
			return playerAbilityParameters2 != null && playerAbilityParameters2.isMainBattleAbility && ability.IsDamagingAbility(null);
		}

		// Token: 0x06001EDF RID: 7903 RVA: 0x00061C28 File Offset: 0x0005FE28
		public static bool IsPlayerMainBattleAbilityPrototype(this BaseAbility ability)
		{
			PlayerAbilityParameters playerAbilityParams = ability.GetPlayerAbilityParams();
			return playerAbilityParams != null && playerAbilityParams.isMainBattleAbility;
		}

		// Token: 0x06001EE0 RID: 7904 RVA: 0x00061C50 File Offset: 0x0005FE50
		public static bool IsSupportAbility(this BaseAbility ability)
		{
			GameAbilityUsingContext gameAbilityUsingContext;
			return (ability.TryGetExplicitUsingContext(out gameAbilityUsingContext) && gameAbilityUsingContext == GameAbilityUsingContext.SupportAbility) || !ability.IsDamagingAbility(null);
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x00061C78 File Offset: 0x0005FE78
		public static bool IsHealingAbility(this BaseAbility ability)
		{
			EffectBasedAbility effectBasedAbility = ability as EffectBasedAbility;
			if (effectBasedAbility != null)
			{
				AbilityEffectBase[] abilityEffects = effectBasedAbility.AbilityEffects;
				for (int i = 0; i < abilityEffects.Length; i++)
				{
					if (abilityEffects[i] is HealingAbilityEffect)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x00061CB4 File Offset: 0x0005FEB4
		public static bool IsMobActivationAbility(this BaseAbility ability, out MobActivationAbilityType activationAbilityType)
		{
			MobSacrificeAbilityTrigger mobSacrificeAbilityTrigger;
			if (ability.IsPostMortemAbility && ability.TryGetExtension(out mobSacrificeAbilityTrigger))
			{
				ITypedMobActivationAbility typedMobActivationAbility = ability as ITypedMobActivationAbility;
				activationAbilityType = ((typedMobActivationAbility != null) ? typedMobActivationAbility.ActivationAbilityType : MobActivationAbilityType.None);
				return true;
			}
			activationAbilityType = MobActivationAbilityType.None;
			return false;
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x00061CF0 File Offset: 0x0005FEF0
		public static bool IsMobActivationAbility(this IAbility ability, out MobActivationAbilityType activationAbilityType)
		{
			activationAbilityType = MobActivationAbilityType.None;
			BaseAbility baseAbility = ability as BaseAbility;
			return baseAbility != null && baseAbility.IsMobActivationAbility(out activationAbilityType);
		}

		// Token: 0x06001EE4 RID: 7908 RVA: 0x00061D14 File Offset: 0x0005FF14
		public static bool IsMobActivationAbility(this BaseAbility ability)
		{
			MobActivationAbilityType mobActivationAbilityType;
			return ability.IsMobActivationAbility(out mobActivationAbilityType);
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x00061D2C File Offset: 0x0005FF2C
		public static bool IsMobActivationAbility(this IAbility ability)
		{
			MobActivationAbilityType mobActivationAbilityType;
			return ability.IsMobActivationAbility(out mobActivationAbilityType);
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x00061D41 File Offset: 0x0005FF41
		public static bool IsPlayerAbility(this BaseAbility ability)
		{
			return ability.OwnerBehaviour is PlayerBehaviour;
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x00061D54 File Offset: 0x0005FF54
		public static bool HasWaveEffect(this IAbility ability)
		{
			Ability ability2 = ability as Ability;
			return ability2 != null && ability2.HasWaveEffect;
		}

		// Token: 0x06001EE8 RID: 7912 RVA: 0x00061D74 File Offset: 0x0005FF74
		public static bool HasTargetedTriggers(this BaseAbility ability)
		{
			IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
			for (int i = 0; i < extensions.Count; i++)
			{
				IMobAbilityTrigger mobAbilityTrigger = extensions[i] as IMobAbilityTrigger;
				if (mobAbilityTrigger != null && mobAbilityTrigger.RequiresTarget)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EE9 RID: 7913 RVA: 0x00061DB4 File Offset: 0x0005FFB4
		public static float GetMinTriggersActivationRange(this BaseAbility ability, bool checkTargetedTriggersOnly = true)
		{
			return GameAbilityExtensions.GetTriggerRange(ability, false, checkTargetedTriggersOnly);
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x00061DBE File Offset: 0x0005FFBE
		public static float GetMaxTriggersActivationRange(this BaseAbility ability, bool checkTargetedTriggersOnly = true)
		{
			return GameAbilityExtensions.GetTriggerRange(ability, true, checkTargetedTriggersOnly);
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x00061DC8 File Offset: 0x0005FFC8
		public static bool IsActivationBlockedByTriggers(this BaseAbility ability, BaseAbility.UsingArgs customUsingArgs = null)
		{
			IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
			BaseAbility.UsingArgs usingArgs = customUsingArgs;
			if (usingArgs == null)
			{
				GameAbilityExtensions.TriggersCheckUsingArgs.Reset();
				GameAbilityExtensions.TriggersCheckUsingArgs.targetObject = ability.OwnerBehaviour;
				GameAbilityExtensions.TriggersCheckUsingArgs.targetPosition = ability.OwnerPosition;
				usingArgs = GameAbilityExtensions.TriggersCheckUsingArgs;
			}
			for (int i = 0; i < extensions.Count; i++)
			{
				IMobAbilityTrigger mobAbilityTrigger = extensions[i] as IMobAbilityTrigger;
				if (mobAbilityTrigger != null && !mobAbilityTrigger.IsConditionReached(ability, usingArgs))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x00061E3F File Offset: 0x0006003F
		public static void SetBlocked(this BaseAbility ability, bool isBlocked)
		{
			if (isBlocked)
			{
				ability.Complete();
				ability.AddPreActivationCondition(new BaseAbility.ActivationCondition(GameAbilityExtensions.AbilityLockingPredicate));
				return;
			}
			ability.RemovePreActivationCondition(new BaseAbility.ActivationCondition(GameAbilityExtensions.AbilityLockingPredicate));
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x00061E6F File Offset: 0x0006006F
		public static void SetAbilityUsingBlocked(this BaseAbility ability, bool isBlocked)
		{
			if (isBlocked)
			{
				ability.AddActivationCondition(new BaseAbility.ActivationCondition(GameAbilityExtensions.AbilityLockingPredicate));
				return;
			}
			ability.RemoveActivationCondition(new BaseAbility.ActivationCondition(GameAbilityExtensions.AbilityLockingPredicate));
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x00061E99 File Offset: 0x00060099
		public static IReadOnlyList<IModifiableStat<MobStatModifier>> GetAbilityStats(this IAbility ability)
		{
			IMobStatsListProvider mobStatsListProvider = ability as IMobStatsListProvider;
			if (mobStatsListProvider == null)
			{
				return null;
			}
			return mobStatsListProvider.Stats;
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x00061EAC File Offset: 0x000600AC
		public static bool ModifyAbilityStat(this IAbility ability, MobStatID targetStatID, MobStatModifier statModifier, bool addModifier)
		{
			return ability.GetAbilityStats().ModifyStat(targetStatID, statModifier, addModifier);
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x00061EBC File Offset: 0x000600BC
		public static void RemoveFromContainerOrDestroy(this BaseAbility ability)
		{
			if (ability == null)
			{
				return;
			}
			IAbilitiesController currentController = ability.CurrentController;
			if (currentController != null)
			{
				currentController.RemoveAbility(ability);
				return;
			}
			ability.Destroy();
		}

		// Token: 0x06001EF1 RID: 7921 RVA: 0x00061EEC File Offset: 0x000600EC
		public static void PrepareForTargetsCollection(this AbilityTargetsCollectionArgs args, BaseAbility ability, bool sortTargets, int maxTargetsCount = -1)
		{
			if (ability != null)
			{
				args.FromAbility(ability);
			}
			args.maxTargetsCount = maxTargetsCount;
			args.targetCastFunction = new Func<Component, Component>(GameAbilityExtensions.CastToMobBehaviour);
			if (sortTargets)
			{
				args.SetDefaultTargetsComparer(true);
				return;
			}
			args.sortingComparer = null;
		}

		// Token: 0x06001EF2 RID: 7922 RVA: 0x00061F2C File Offset: 0x0006012C
		public static void PrepareForTargetsCollection(this GameLocation.MobsGatheringArgs args, BaseAbility ability, BaseAbility.UsingArgs usingArgs, bool sortTargets, int maxTargetsCount = -1)
		{
			if (ability != null)
			{
				args.range = ((usingArgs != null) ? usingArgs.GetActualUsingRange(ability) : ability.Range);
				args.layers = ability.ValidObjectLayers;
			}
			args.maxCount = maxTargetsCount;
			if (usingArgs != null)
			{
				args.position = usingArgs.targetPosition;
			}
			GameAbilityExtensions.PassSortingComparer(args, sortTargets);
		}

		// Token: 0x06001EF3 RID: 7923 RVA: 0x00061F90 File Offset: 0x00060190
		public static void PrepareForTargetsCollection(this GameLocation.MobsGatheringArgs args, ProjectileAbilityBase ability, BaseAbility.UsingArgs projectileUsingArgs)
		{
			args.range = ability.ProjectileEffectRange;
			args.layers = ability.ValidObjectLayers;
			args.maxCount = ability.maxProjectileEffectTargets;
			args.position = projectileUsingArgs.targetPosition;
			args.filter = null;
			GameAbilityExtensions.PassSortingComparer(args, ability.sortAbilityTargets);
		}

		// Token: 0x06001EF4 RID: 7924 RVA: 0x00061FEC File Offset: 0x000601EC
		public static void CollectTargets(this BaseAbility ability, GameLocation.MobsGatheringArgs collectionArgs, BaseAbility.UsingArgs usingArgs)
		{
			IGameMob gameMob = (IGameMob)ability.Owner;
			if (gameMob == null)
			{
				return;
			}
			if (gameMob.CurrentLocation == null)
			{
				return;
			}
			BaseGameMob[] array;
			int mobsInRange = gameMob.CurrentLocation.GetMobsInRange(collectionArgs, out array);
			usingArgs.targetsCountOverride = mobsInRange;
			usingArgs.targetsList = ((mobsInRange != 0) ? array : null);
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x00062035 File Offset: 0x00060235
		public static void PrepareDamageGenerator(this BaseAbility ability, DamageGenerator damageGenerator)
		{
			if (ability == null || damageGenerator == null)
			{
				return;
			}
			damageGenerator.damageableLayers = ability.ValidObjectLayers;
			damageGenerator.DamageSender = (ability.AbilityEffectSender ?? ability.Owner);
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x00062068 File Offset: 0x00060268
		public static void SetProjectileLaunchDirection(this ProjectileLaunchArgs launchArgs, Vector3 launchPosition, BaseAbility.UsingArgs abilityUsingArgs)
		{
			launchArgs.position = launchPosition;
			launchArgs.direction = (abilityUsingArgs.targetPosition - launchPosition).normalized;
		}

		// Token: 0x06001EF7 RID: 7927 RVA: 0x00062098 File Offset: 0x00060298
		public static void PrepareProjectileLaunchArgs(this ProjectileLaunchArgs launchArgs, IProjectile projectilePrototype, Vector2 launchPosition, Vector2 targetPosition)
		{
			Vector2 a = targetPosition - launchPosition;
			float magnitude = a.magnitude;
			launchArgs.position = launchPosition;
			launchArgs.direction = a / magnitude;
			launchArgs.lifetimeOverride = projectilePrototype.GetTimeToReachTarget(launchArgs, magnitude);
		}

		// Token: 0x06001EF8 RID: 7928 RVA: 0x000620E4 File Offset: 0x000602E4
		public static Component GetAnyTarget(this BaseAbility.UsingArgs usingArgs)
		{
			if (usingArgs != null)
			{
				if (usingArgs.HasTargetObject)
				{
					return usingArgs.targetObject;
				}
				int targetsCount = usingArgs.TargetsCount;
				if (targetsCount != 0)
				{
					IList<Component> targetsList = usingArgs.targetsList;
					for (int i = 0; i < targetsCount; i++)
					{
						Component component = targetsList[i];
						if (component != null)
						{
							return component;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06001EF9 RID: 7929 RVA: 0x00062134 File Offset: 0x00060334
		public static void PrepareForUsingOnOwner(this BaseAbility.UsingArgs usingArgs, BaseAbility ability)
		{
			usingArgs.Reset();
			usingArgs.targetObject = ability.OwnerBehaviour;
			usingArgs.targetPosition = ability.OwnerPosition;
		}

		// Token: 0x06001EFA RID: 7930 RVA: 0x00062154 File Offset: 0x00060354
		public static void ProcessTargets(this BaseAbility.UsingArgs usingArgs, Action<Component> action)
		{
			int targetsCount = usingArgs.TargetsCount;
			if (targetsCount != 0)
			{
				IList<Component> targetsList = usingArgs.targetsList;
				for (int i = 0; i < targetsCount; i++)
				{
					action(targetsList[i]);
				}
				return;
			}
			if (usingArgs.HasTargetObject)
			{
				action(usingArgs.targetObject);
			}
		}

		// Token: 0x06001EFB RID: 7931 RVA: 0x000621A0 File Offset: 0x000603A0
		public static void TryUpdateTargetPosition(this BaseAbility.UsingArgs usingArgs)
		{
			usingArgs.targetPosition = usingArgs.TryGetTargetPosition();
		}

		// Token: 0x06001EFC RID: 7932 RVA: 0x000621B0 File Offset: 0x000603B0
		public static IGameMob FindAttackTarget(this BaseAbility ability)
		{
			BaseGameMob baseGameMob = ability.Owner as BaseGameMob;
			if (!(baseGameMob != null))
			{
				return null;
			}
			GameMobAIController aicontroller = baseGameMob.AIController;
			if (aicontroller == null)
			{
				return null;
			}
			return aicontroller.ForceGetAttackTarget(ability);
		}

		// Token: 0x06001EFD RID: 7933 RVA: 0x000621E8 File Offset: 0x000603E8
		public static bool FindAttackTarget(this BaseAbility ability, BaseAbility.UsingArgs usingArgs, bool force = false)
		{
			if (force || !usingArgs.HasTargetObject)
			{
				IGameMob gameMob = ability.FindAttackTarget();
				if (gameMob != null)
				{
					usingArgs.ResetTargetsList();
					usingArgs.targetObject = (gameMob as Component);
					usingArgs.targetPosition = gameMob.Position;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001EFE RID: 7934 RVA: 0x00062230 File Offset: 0x00060430
		public static bool PassWorldCursorPosition(this BaseAbility.UsingArgs usingArgs, BaseAbility ability)
		{
			GameBehaviourBase gameBehaviourBase = ability.OwnerBehaviour as GameBehaviourBase;
			IGame game = (gameBehaviourBase != null) ? gameBehaviourBase.CurrentGame : null;
			IInputManager inputManager;
			if (game != null && game.Services.TryGet<IInputManager>(out inputManager) && inputManager != null)
			{
				usingArgs.targetPosition = inputManager.GetWorldCursorPosition();
				return true;
			}
			if (Debug.isDebugBuild)
			{
				Debug.LogWarning(string.Format("Unable to pass world cursor position to {0}.", ability));
			}
			return false;
		}

		// Token: 0x06001EFF RID: 7935 RVA: 0x00062295 File Offset: 0x00060495
		public static bool TryGetAdditionalContext<TContext>(this BaseAbility.UsingArgs usingArgs, out TContext context) where TContext : class
		{
			context = (((usingArgs != null) ? usingArgs.additionalContext : null) as TContext);
			return context != null;
		}

		// Token: 0x06001F00 RID: 7936 RVA: 0x000622C1 File Offset: 0x000604C1
		public static AbilityModifiersOverrides GetModifiersOverrides(this BaseAbility ability)
		{
			AbilityModifiersOverrides result;
			if ((result = GameAbilityExtensions.<GetModifiersOverrides>g__GetOverrides|73_0(ability)) == null)
			{
				result = GameAbilityExtensions.<GetModifiersOverrides>g__GetOverrides|73_0((ability != null) ? ability.ParentAbility : null);
			}
			return result;
		}

		// Token: 0x06001F01 RID: 7937 RVA: 0x000622E0 File Offset: 0x000604E0
		public static AbilityModifiersOverrides GetOrAddModifiersOverrides(this BaseAbility ability)
		{
			AbilityModifiersOverrides abilityModifiersOverrides;
			if (ability.TryGetExtension(out abilityModifiersOverrides))
			{
				return abilityModifiersOverrides;
			}
			abilityModifiersOverrides = new AbilityModifiersOverrides();
			ability.AddExtension(abilityModifiersOverrides);
			return abilityModifiersOverrides;
		}

		// Token: 0x06001F02 RID: 7938 RVA: 0x00062307 File Offset: 0x00060507
		public static bool HasModifiersActivator(this BaseAbility ability, AbilityModifiersActivatorBase modifiersActivator)
		{
			AbilityModifiersController extension = ability.GetExtension<AbilityModifiersController>();
			return ((extension != null) ? extension.Activator : null) == modifiersActivator;
		}

		// Token: 0x06001F03 RID: 7939 RVA: 0x00062324 File Offset: 0x00060524
		public static void ApplyOverridesToCost(this IReadOnlyList<AbilityPropertyValuesOverrides> overrides, ref float cost, int level)
		{
			if (overrides != null)
			{
				for (int i = 0; i < overrides.Count; i++)
				{
					AbilityPropertyValuesOverrides abilityPropertyValuesOverrides = overrides[i];
					float num;
					if (abilityPropertyValuesOverrides.PropertyDescription.propertyID == AbilityPropertyID.Cost && abilityPropertyValuesOverrides.TryGetPropertyValue(level, out num))
					{
						cost = num;
						return;
					}
				}
			}
		}

		// Token: 0x06001F04 RID: 7940 RVA: 0x0006236C File Offset: 0x0006056C
		public static void ApplyOverridesToAbilityResourcesBlock(this IReadOnlyList<AbilityPropertyValuesOverrides> overrides, ref AbilityResourcesBlock block, int level)
		{
			if (overrides == null)
			{
				return;
			}
			for (int i = 0; i < overrides.Count; i++)
			{
				AbilityPropertyValuesOverrides abilityPropertyValuesOverrides = overrides[i];
				float num;
				if (abilityPropertyValuesOverrides.PropertyDescription.propertyID == AbilityPropertyID.ModifierResourcesAddition && abilityPropertyValuesOverrides.TryGetPropertyValue(level, out num))
				{
					block.Add((int)num, true);
					return;
				}
			}
		}

		// Token: 0x06001F05 RID: 7941 RVA: 0x000623BD File Offset: 0x000605BD
		public static void ApplyOverridesToAbilityResourcesBlock(this IAbilityLevelingController levelingController, ref AbilityResourcesBlock block, int level)
		{
			if (levelingController != null)
			{
				levelingController.AbilityPropertiesOverrides.ApplyOverridesToAbilityResourcesBlock(ref block, level);
			}
		}

		// Token: 0x06001F06 RID: 7942 RVA: 0x000623D0 File Offset: 0x000605D0
		public static bool TryGetActivationResources(this object activatableItem, out AbilityResourcesBlock requiredResources, out IAbilityResourcesConsumer activator, bool applyAmountOverrides = true)
		{
			AbilityModifiersController abilityModifiersController = activatableItem as AbilityModifiersController;
			activator = (((abilityModifiersController != null) ? abilityModifiersController.Activator : null) as IAbilityResourcesConsumer);
			BaseAbility baseAbility = activatableItem as BaseAbility;
			if (activator == null && baseAbility != null)
			{
				activator = GameAbilityExtensions.<TryGetActivationResources>g__GetAbilityActivator|79_0(baseAbility);
				if (!GameAbilityExtensions.<TryGetActivationResources>g__IsValidAbilityActivator|79_1(baseAbility, activator))
				{
					ICompositeAbility compositeAbility = baseAbility as ICompositeAbility;
					IList<IAbility> list = (compositeAbility != null) ? compositeAbility.ChildAbilities : null;
					if (list != null)
					{
						for (int i = 0; i < list.Count; i++)
						{
							BaseAbility ability = list[i] as BaseAbility;
							IAbilityResourcesConsumer abilityResourcesConsumer = GameAbilityExtensions.<TryGetActivationResources>g__GetAbilityActivator|79_0(ability);
							if (GameAbilityExtensions.<TryGetActivationResources>g__IsValidAbilityActivator|79_1(ability, abilityResourcesConsumer))
							{
								activator = abilityResourcesConsumer;
								break;
							}
						}
					}
				}
			}
			if (activator != null && baseAbility != null)
			{
				requiredResources = new AbilityResourcesBlock(activator.RequiredResources, true);
				if (applyAmountOverrides)
				{
					IAbilityLevelingController abilityLevelingController;
					int level;
					if (baseAbility.TryGetLevelingController(out abilityLevelingController) && abilityLevelingController.GetAbilityLevel(baseAbility, out level))
					{
						abilityLevelingController.ApplyOverridesToAbilityResourcesBlock(ref requiredResources, level);
					}
					AbilityModifiersOverrides modifiersOverrides = baseAbility.GetModifiersOverrides();
					if (modifiersOverrides != null)
					{
						modifiersOverrides.ApplyResourcesOverrides(ref requiredResources);
					}
				}
				return true;
			}
			requiredResources = default(AbilityResourcesBlock);
			return false;
		}

		// Token: 0x06001F07 RID: 7943 RVA: 0x000624C4 File Offset: 0x000606C4
		public static IMobsSummoner GetMobsSummoner(this BaseAbility ability)
		{
			IMobsSummoner mobsSummoner = ability as IMobsSummoner;
			if (mobsSummoner != null)
			{
				return mobsSummoner;
			}
			AbilityModifiersController abilityModifiersController;
			AbilitySummoningModifier result;
			if (ability.TryGetExtension(out abilityModifiersController) && abilityModifiersController.TryGetModifier<AbilitySummoningModifier>(ability, out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06001F08 RID: 7944 RVA: 0x000624F8 File Offset: 0x000606F8
		public static NecromancyAbilityEffect GetNecromancyEffect(this IAbility ability)
		{
			IEffectsBasedAbility effectsBasedAbility = ability as IEffectsBasedAbility;
			AbilityEffectBase[] array = (effectsBasedAbility != null) ? effectsBasedAbility.AbilityEffects : null;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					NecromancyAbilityEffect necromancyAbilityEffect = array[i] as NecromancyAbilityEffect;
					if (necromancyAbilityEffect != null)
					{
						return necromancyAbilityEffect;
					}
				}
			}
			return null;
		}

		// Token: 0x06001F09 RID: 7945 RVA: 0x00062538 File Offset: 0x00060738
		public static bool IsNecromancyAbility(this IAbility ability)
		{
			return ability.GetNecromancyEffect() != null;
		}

		// Token: 0x06001F0B RID: 7947 RVA: 0x0006254F File Offset: 0x0006074F
		[CompilerGenerated]
		internal static bool <IsProjectileAbility>g__IsTargetAbility|24_0(IAbility targetAbility)
		{
			return targetAbility is ProjectileAbilityBase;
		}

		// Token: 0x06001F0C RID: 7948 RVA: 0x0006255A File Offset: 0x0006075A
		[CompilerGenerated]
		internal static void <GetProjectileLayers>g__AddLayers|27_0(ProjectileAbilityBase projectileAbility, ref int obstacleLayers, ref int hitLayers)
		{
			if (projectileAbility != null)
			{
				obstacleLayers |= projectileAbility.ProjectileObstacleLayers;
				hitLayers |= projectileAbility.ValidObjectLayers;
			}
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x00062585 File Offset: 0x00060785
		[CompilerGenerated]
		internal static AbilityModifiersOverrides <GetModifiersOverrides>g__GetOverrides|73_0(BaseAbility ability)
		{
			if (ability == null)
			{
				return null;
			}
			return ability.GetExtension<AbilityModifiersOverrides>();
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x00062594 File Offset: 0x00060794
		[CompilerGenerated]
		internal static IAbilityResourcesConsumer <TryGetActivationResources>g__GetAbilityActivator|79_0(BaseAbility ability)
		{
			if (ability == null)
			{
				return null;
			}
			IAbilityResourcesConsumer result;
			if (!ability.TryGetExtension(out result))
			{
				AbilityModifiersController extension = ability.GetExtension<AbilityModifiersController>();
				result = (((extension != null) ? extension.Activator : null) as IAbilityResourcesConsumer);
			}
			return result;
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x000625CF File Offset: 0x000607CF
		[CompilerGenerated]
		internal static bool <TryGetActivationResources>g__IsValidAbilityActivator|79_1(BaseAbility ability, IAbilityResourcesConsumer abilityActivator)
		{
			return abilityActivator != null && (!ability.IsInstantiated() || abilityActivator.GetResourcesCollectionRange(ability) > 0f);
		}

		// Token: 0x040013A6 RID: 5030
		private const GameAbilityUsingContext UndefinedUsingContext = GameAbilityUsingContext.Auto;

		// Token: 0x040013A7 RID: 5031
		private static readonly BaseAbility.UsingArgs TriggersCheckUsingArgs = new BaseAbility.UsingArgs();

		// Token: 0x02000579 RID: 1401
		public interface IExplicitUsingContextAbility
		{
			// Token: 0x170007F3 RID: 2035
			// (get) Token: 0x06002733 RID: 10035
			GameAbilityUsingContext UsingContext { get; }
		}
	}
}
