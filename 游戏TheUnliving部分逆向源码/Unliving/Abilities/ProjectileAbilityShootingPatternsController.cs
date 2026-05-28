using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.CollectionsExtensions;
using Common.Editor;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Damage.Projectiles;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x0200037D RID: 893
	[CreateAssetMenu(fileName = "ProjectileAbilityShootingPatternsController", menuName = "Abilities/Controllers/Projectile Ability Shooting Patterns Controller")]
	public sealed class ProjectileAbilityShootingPatternsController : AbilityExtensionAssetBase
	{
		// Token: 0x06001D62 RID: 7522 RVA: 0x0005D164 File Offset: 0x0005B364
		private static ProjectileLaunchArgs PrepareProjectileLaunchArgs(ProjectileAbilityBase ability, Vector2 launchPoint, Vector2 targetPoint, IProjectile customProjectilePrototype = null)
		{
			IProjectile projectile = customProjectilePrototype;
			if (projectile == null)
			{
				projectile = ability.ProjectilePrototype;
			}
			ProjectileAbilityShootingPatternsController.ProjectilesLaunchArgs.PrepareProjectileLaunchArgs(projectile, launchPoint, targetPoint);
			ProjectileAbilityShootingPatternsController.ProjectilesLaunchArgs.launcher = ability.Owner;
			return ProjectileAbilityShootingPatternsController.ProjectilesLaunchArgs;
		}

		// Token: 0x06001D63 RID: 7523 RVA: 0x0005D1A0 File Offset: 0x0005B3A0
		private static void LaunchProjectileChain(ProjectileAbilityBase ability, Transform path, bool destroyedProjectilesCanBreakPath)
		{
			ProjectileAbilityShootingPatternsController.<LaunchProjectileChain>d__7 <LaunchProjectileChain>d__;
			<LaunchProjectileChain>d__.ability = ability;
			<LaunchProjectileChain>d__.path = path;
			<LaunchProjectileChain>d__.destroyedProjectilesCanBreakPath = destroyedProjectilesCanBreakPath;
			<LaunchProjectileChain>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<LaunchProjectileChain>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <LaunchProjectileChain>d__.<>t__builder;
			<>t__builder.Start<ProjectileAbilityShootingPatternsController.<LaunchProjectileChain>d__7>(ref <LaunchProjectileChain>d__);
		}

		// Token: 0x17000610 RID: 1552
		// (get) Token: 0x06001D64 RID: 7524 RVA: 0x0005D1E9 File Offset: 0x0005B3E9
		public override bool IsSharedExtension
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x0005D1EC File Offset: 0x0005B3EC
		private Vector2 GetPatternPlacingPosition(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			ProjectileAbilityShootingPatternsController.PatternPlacing patternPlacing = this.patternPlacing;
			if (patternPlacing == ProjectileAbilityShootingPatternsController.PatternPlacing.OwnerPosition)
			{
				return ability.OwnerPosition;
			}
			if (patternPlacing != ProjectileAbilityShootingPatternsController.PatternPlacing.SpawnerPosition)
			{
				return usingArgs.targetPosition;
			}
			return ((BaseGameMob)ability.OwnerBehaviour).Group.InitialPosition;
		}

		// Token: 0x06001D66 RID: 7526 RVA: 0x0005D238 File Offset: 0x0005B438
		private void PlacePattern(Transform shootingPattern, BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			Vector2 patternPlacingPosition = this.GetPatternPlacingPosition(ability, usingArgs);
			shootingPattern.position = patternPlacingPosition;
			if (this.rotatePatternToTarget)
			{
				Vector2 vector;
				if (this.patternPlacing == ProjectileAbilityShootingPatternsController.PatternPlacing.OwnerPosition)
				{
					vector = usingArgs.targetPosition;
				}
				else
				{
					vector = patternPlacingPosition;
				}
				vector -= ability.OwnerPosition;
				shootingPattern.rotation = QuaternionExtensions.Get2DRotation(vector, 0f);
			}
		}

		// Token: 0x06001D67 RID: 7527 RVA: 0x0005D2A0 File Offset: 0x0005B4A0
		private void DestroyActiveShootingPattern(IAbility ability)
		{
			int abilityInstanceID = AbilityExtensionAssetBase.GetAbilityInstanceID(ability);
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (!this.activeShootingPatterns.TryGetValue(abilityInstanceID, out patternInstanceData))
			{
				return;
			}
			patternInstanceData.DestroyPattern();
			this.activeShootingPatterns.Remove(abilityInstanceID);
		}

		// Token: 0x06001D68 RID: 7528 RVA: 0x0005D2DC File Offset: 0x0005B4DC
		public int GetActiveShootingPatternPointsCount(BaseAbility ability)
		{
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (this.activeShootingPatterns.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out patternInstanceData))
			{
				return Mathf.Min(patternInstanceData.ShotsCount, patternInstanceData.PointsCount);
			}
			return 0;
		}

		// Token: 0x06001D69 RID: 7529 RVA: 0x0005D314 File Offset: 0x0005B514
		public IReadOnlyList<Vector2> GetActiveShootingPatternPoints(BaseAbility ability, BaseAbility.UsingArgs usingArgs)
		{
			ProjectileAbilityShootingPatternsController.PatternPointsBuffer.Clear();
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (this.activeShootingPatterns.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out patternInstanceData))
			{
				this.PlacePattern(patternInstanceData.Pattern, ability, usingArgs);
				int num = Mathf.Min(patternInstanceData.ShotsCount, patternInstanceData.PointsCount);
				for (int i = 0; i < num; i++)
				{
					ProjectileAbilityShootingPatternsController.PatternPointsBuffer.Add(patternInstanceData.GetPoint(i).position);
				}
			}
			return ProjectileAbilityShootingPatternsController.PatternPointsBuffer;
		}

		// Token: 0x06001D6A RID: 7530 RVA: 0x0005D390 File Offset: 0x0005B590
		private void OnBeforeAbilityActivated(IAbility ability, object usingArgs)
		{
			float prepProgress = ability.PrepProgress;
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (prepProgress == 0f)
			{
				ProjectileAbilityShootingPatternsController.PatternData patternData;
				if (this.shootingPatterns.GetRandomWeightedItem(out patternData, 0, 2147483647, null))
				{
					ProjectileAbilityBase ability2 = (ProjectileAbilityBase)ability;
					ProjectileAbilityShootingPatternsController.PatternInstanceData value = new ProjectileAbilityShootingPatternsController.PatternInstanceData(patternData, ability2, this.isProjectilePathPattern, this.patternRendererTag);
					this.activeShootingPatterns.Add(AbilityExtensionAssetBase.GetAbilityInstanceID(ability2), value);
					return;
				}
			}
			else if (ability.PrepTime > 0f && this.activeShootingPatterns.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out patternInstanceData))
			{
				this.PlacePattern(patternInstanceData.Pattern, (BaseAbility)ability, (BaseAbility.UsingArgs)usingArgs);
				patternInstanceData.SetPatternRendererVisibile(prepProgress < 1f);
			}
		}

		// Token: 0x06001D6B RID: 7531 RVA: 0x0005D444 File Offset: 0x0005B644
		private void OnBeforeAbilityUsed(IAbility ability, object args)
		{
			ProjectileAbilityBase projectileAbilityBase = (ProjectileAbilityBase)ability;
			if (projectileAbilityBase.WasUsed)
			{
				return;
			}
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (!this.activeShootingPatterns.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(projectileAbilityBase), out patternInstanceData))
			{
				return;
			}
			if (!this.isProjectilePathPattern)
			{
				projectileAbilityBase.MaxShotsPerUsing = patternInstanceData.ShotsCount;
			}
			this.PlacePattern(patternInstanceData.Pattern, projectileAbilityBase, (BaseAbility.UsingArgs)args);
		}

		// Token: 0x06001D6C RID: 7532 RVA: 0x0005D4A0 File Offset: 0x0005B6A0
		private void OnBeforeProjectileLaunched(ProjectileAbilityBase.LaunchEventArgs args)
		{
			if (args.abilityUsingArgs == null)
			{
				return;
			}
			ProjectileAbilityBase ability = args.ability;
			ProjectileAbilityShootingPatternsController.PatternInstanceData patternInstanceData;
			if (!this.activeShootingPatterns.TryGetValue(AbilityExtensionAssetBase.GetAbilityInstanceID(ability), out patternInstanceData))
			{
				return;
			}
			ProjectileAbilityBase.ProjectileLaunchPoint projectileLaunchPoint;
			Vector2 launchPoint = ability.GetProjectileLaunchPosition(args.launchPointIndex, out projectileLaunchPoint);
			Vector2 targetPoint = patternInstanceData.GetPoint(this.isProjectilePathPattern ? 0 : (args.shotIndex % patternInstanceData.PointsCount)).position;
			ProjectileLaunchArgs projectileLaunchArgs = ProjectileAbilityShootingPatternsController.PrepareProjectileLaunchArgs(ability, launchPoint, targetPoint, null);
			projectileLaunchArgs.target = args.abilityUsingArgs.targetObject;
			ability.SetProjectileLaunchArgsOverride(projectileLaunchArgs);
			if (this.isProjectilePathPattern)
			{
				ProjectileAbilityShootingPatternsController.LaunchProjectileChain(ability, patternInstanceData.Pattern, this.destroyedProjectilesCanBreakPathPattern);
				this.activeShootingPatterns.Remove(AbilityExtensionAssetBase.GetAbilityInstanceID(ability));
			}
		}

		// Token: 0x06001D6D RID: 7533 RVA: 0x0005D563 File Offset: 0x0005B763
		private void OnAbilityCompleted(IAbility ability, object usingArgs)
		{
			this.DestroyActiveShootingPattern(ability);
		}

		// Token: 0x06001D6E RID: 7534 RVA: 0x0005D56C File Offset: 0x0005B76C
		public override void OnAddedToAbility(BaseAbility ability)
		{
			base.OnAddedToAbility(ability);
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.Activating += this.OnBeforeAbilityActivated;
				projectileAbilityBase.BeforeUsed += this.OnBeforeAbilityUsed;
				projectileAbilityBase.LaunchingProjectile += this.OnBeforeProjectileLaunched;
				projectileAbilityBase.Completed += this.OnAbilityCompleted;
			}
		}

		// Token: 0x06001D6F RID: 7535 RVA: 0x0005D5D4 File Offset: 0x0005B7D4
		public override void OnRemovedFromAbility(BaseAbility ability)
		{
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				this.DestroyActiveShootingPattern(ability);
				projectileAbilityBase.Activating -= this.OnBeforeAbilityActivated;
				projectileAbilityBase.BeforeUsed -= this.OnBeforeAbilityUsed;
				projectileAbilityBase.LaunchingProjectile -= this.OnBeforeProjectileLaunched;
				projectileAbilityBase.Completed -= this.OnAbilityCompleted;
			}
			base.OnRemovedFromAbility(ability);
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x0005D684 File Offset: 0x0005B884
		[CompilerGenerated]
		internal static IProjectile <LaunchProjectileChain>g__LaunchNextProjectile|7_0(ProjectileAbilityBase launcher, Transform projectilePath, Vector2 lastProjectilePosition, ref int pointIndex, ref Vector2 targetPoint, out ProjectileAbilityBase customProjectileAbility)
		{
			Transform child = projectilePath.GetChild(pointIndex - 1);
			ProjectileDataBase projectileDataBase = null;
			int num = pointIndex;
			pointIndex = num + 1;
			targetPoint = projectilePath.GetChild(num).position;
			customProjectileAbility = null;
			GameAbilityInstantiator gameAbilityInstantiator;
			if (child.TryGetComponent<GameAbilityInstantiator>(out gameAbilityInstantiator) && gameAbilityInstantiator.AbilityPrototype != null)
			{
				ProjectileAbilityShootingPatternsController.AbilitiesFactoryArgs.abilityOwner = launcher.Owner;
				ProjectileAbilityShootingPatternsController.AbilitiesFactoryArgs.parentAbility = launcher;
				customProjectileAbility = (ProjectileAbilityBase)gameAbilityInstantiator.InstantiateAbility(ProjectileAbilityShootingPatternsController.AbilitiesFactoryArgs);
				launcher.GetController().AddAbility(customProjectileAbility);
				launcher = customProjectileAbility;
			}
			else
			{
				IProjectileDataProvider component = child.GetComponent<IProjectileDataProvider>();
				projectileDataBase = ((component != null) ? component.ProjectileData : null);
			}
			ProjectileLaunchArgs launchArgs = ProjectileAbilityShootingPatternsController.PrepareProjectileLaunchArgs(launcher, lastProjectilePosition, targetPoint, (projectileDataBase != null) ? projectileDataBase.ProjectilePrototype : null);
			IProjectile projectile = launcher.LaunchProjectile(launchArgs, null, projectileDataBase);
			projectile.Generation = pointIndex;
			return projectile;
		}

		// Token: 0x0400109A RID: 4250
		private static readonly AbilityFactoryArgs AbilitiesFactoryArgs = new AbilityFactoryArgs();

		// Token: 0x0400109B RID: 4251
		private static readonly ProjectileLaunchArgs ProjectilesLaunchArgs = new ProjectileLaunchArgs();

		// Token: 0x0400109C RID: 4252
		private static readonly List<Vector2> PatternPointsBuffer = new List<Vector2>(32);

		// Token: 0x0400109D RID: 4253
		public ProjectileAbilityShootingPatternsController.PatternPlacing patternPlacing;

		// Token: 0x0400109E RID: 4254
		[FormerlySerializedAs("headPatternToTarget")]
		public bool rotatePatternToTarget;

		// Token: 0x0400109F RID: 4255
		public ProjectileAbilityShootingPatternsController.PatternData[] shootingPatterns;

		// Token: 0x040010A0 RID: 4256
		public bool isProjectilePathPattern;

		// Token: 0x040010A1 RID: 4257
		public bool destroyedProjectilesCanBreakPathPattern;

		// Token: 0x040010A2 RID: 4258
		[Tag]
		public string patternRendererTag = "ShootingPatternRenderer";

		// Token: 0x040010A3 RID: 4259
		private readonly Dictionary<int, ProjectileAbilityShootingPatternsController.PatternInstanceData> activeShootingPatterns = new Dictionary<int, ProjectileAbilityShootingPatternsController.PatternInstanceData>(32);

		// Token: 0x0200056B RID: 1387
		public enum PatternPlacing
		{
			// Token: 0x04001C30 RID: 7216
			UsingPoint,
			// Token: 0x04001C31 RID: 7217
			OwnerPosition,
			// Token: 0x04001C32 RID: 7218
			SpawnerPosition
		}

		// Token: 0x0200056C RID: 1388
		[Serializable]
		public struct PatternData : IWeighted
		{
			// Token: 0x170007EF RID: 2031
			// (get) Token: 0x06002719 RID: 10009 RVA: 0x00079D7A File Offset: 0x00077F7A
			// (set) Token: 0x0600271A RID: 10010 RVA: 0x00079D82 File Offset: 0x00077F82
			public float Weight
			{
				get
				{
					return this.weight;
				}
				set
				{
					this.weight = value;
				}
			}

			// Token: 0x04001C33 RID: 7219
			public GameObject prefab;

			// Token: 0x04001C34 RID: 7220
			public bool useAbilityShotsCount;

			// Token: 0x04001C35 RID: 7221
			[SerializeField]
			[Range(0f, 1f)]
			private float weight;
		}

		// Token: 0x0200056D RID: 1389
		private readonly struct PatternInstanceData
		{
			// Token: 0x170007F0 RID: 2032
			// (get) Token: 0x0600271B RID: 10011 RVA: 0x00079D8B File Offset: 0x00077F8B
			public int PointsCount { get; }

			// Token: 0x0600271C RID: 10012 RVA: 0x00079D94 File Offset: 0x00077F94
			public PatternInstanceData(ProjectileAbilityShootingPatternsController.PatternData patternData, ProjectileAbilityBase ability, bool isPathPattern, string patternRendererTag)
			{
				this.Pattern = UnityEngine.Object.Instantiate<GameObject>(patternData.prefab).transform;
				this.PointsCount = this.Pattern.childCount;
				this.rendererObject = null;
				if (!string.IsNullOrEmpty(patternRendererTag))
				{
					for (int i = 0; i < this.PointsCount; i++)
					{
						Transform child = this.Pattern.GetChild(i);
						if (child.gameObject.CompareTag(patternRendererTag))
						{
							child.SetAsLastSibling();
							int pointsCount = this.PointsCount;
							this.PointsCount = pointsCount - 1;
							this.rendererObject = child.gameObject;
							break;
						}
					}
				}
				this.ShotsCount = ((!isPathPattern && patternData.useAbilityShotsCount) ? ability.MaxShotsPerUsing : this.PointsCount);
				Renderer obj;
				if (this.Pattern.TryGetComponent<Renderer>(out obj))
				{
					UnityEngine.Object.Destroy(obj);
				}
				this.SetPatternRendererVisibile(false);
			}

			// Token: 0x0600271D RID: 10013 RVA: 0x00079E64 File Offset: 0x00078064
			public Transform GetPoint(int pointIndex)
			{
				return this.Pattern.GetChild(pointIndex);
			}

			// Token: 0x0600271E RID: 10014 RVA: 0x00079E72 File Offset: 0x00078072
			public void SetPatternRendererVisibile(bool isVisible)
			{
				if (this.rendererObject != null)
				{
					this.rendererObject.SetActive(isVisible);
				}
			}

			// Token: 0x0600271F RID: 10015 RVA: 0x00079E8E File Offset: 0x0007808E
			public void DestroyPattern()
			{
				if (!this.Pattern.IsNull())
				{
					UnityEngine.Object.Destroy(this.Pattern.gameObject);
				}
			}

			// Token: 0x04001C37 RID: 7223
			public readonly Transform Pattern;

			// Token: 0x04001C38 RID: 7224
			public readonly int ShotsCount;

			// Token: 0x04001C39 RID: 7225
			private readonly GameObject rendererObject;
		}
	}
}
