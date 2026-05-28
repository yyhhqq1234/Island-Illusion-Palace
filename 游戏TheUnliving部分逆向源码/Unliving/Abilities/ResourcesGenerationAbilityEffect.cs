using System;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Core;
using Game.Damage;
using UnityEngine;
using Unliving.AbilityResources;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000393 RID: 915
	[Serializable]
	public sealed class ResourcesGenerationAbilityEffect : AbilityEffectBase, IGameContextDependent
	{
		// Token: 0x17000621 RID: 1569
		// (get) Token: 0x06001E2F RID: 7727 RVA: 0x0005FBB4 File Offset: 0x0005DDB4
		// (set) Token: 0x06001E30 RID: 7728 RVA: 0x0005FBBC File Offset: 0x0005DDBC
		IGame IGameContextDependent.CurrentGame
		{
			get
			{
				return this.currentGame;
			}
			set
			{
				this.currentGame = value;
			}
		}

		// Token: 0x06001E31 RID: 7729 RVA: 0x0005FBC5 File Offset: 0x0005DDC5
		private int GetResourceAmount()
		{
			return UnityEngine.Random.Range(this.minResourceAmount, this.maxResourceAmount);
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x0005FBD8 File Offset: 0x0005DDD8
		public ResourcesGenerationAbilityEffect()
		{
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x0005FBF0 File Offset: 0x0005DDF0
		public ResourcesGenerationAbilityEffect(ResourcesGenerationAbilityEffect effectPrototype)
		{
			this.resourcesSource = effectPrototype.resourcesSource;
			this.resourceType = effectPrototype.resourceType;
			this.minResourceAmount = effectPrototype.minResourceAmount;
			this.maxResourceAmount = effectPrototype.maxResourceAmount;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x0005FC48 File Offset: 0x0005DE48
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x0005FC4F File Offset: 0x0005DE4F
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x0005FC54 File Offset: 0x0005DE54
		public override void Use(BaseAbility.UsingArgs abilityUsingArgs, float dt)
		{
			if (this.resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.Factory && (this.resourcesFactory != null || this.currentGame.Services.TryGet<ICollectableAbilityResourcesFactory>(out this.resourcesFactory)))
			{
				ResourcesGenerationAbilityEffect.FactoryArgs.resourceType = this.resourceType;
				ResourcesGenerationAbilityEffect.FactoryArgs.position = new Vector2?(abilityUsingArgs.targetPosition);
				this.resourcesFactory.Create(ResourcesGenerationAbilityEffect.FactoryArgs);
				return;
			}
			if (this.resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.AbilityOwner)
			{
				this.forceUseOnOwner = true;
			}
			base.Use(abilityUsingArgs, dt);
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x0005FCE0 File Offset: 0x0005DEE0
		protected override bool Use(Component effectTarget, float dt)
		{
			IGameMob gameMob = effectTarget.CastOrGetComponent<IGameMob>();
			if (gameMob == null)
			{
				return false;
			}
			AbilityResourcesGenerator resourcesGenerator = gameMob.ResourcesGenerator;
			if (resourcesGenerator == null)
			{
				return false;
			}
			if (this.resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.AliveMob || this.resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.UndeadMob)
			{
				ResourcesGenerationAbilityEffect.ResourcesSource resourcesSource = this.resourcesSource;
				if (resourcesSource != ResourcesGenerationAbilityEffect.ResourcesSource.AliveMob)
				{
					if (resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.UndeadMob)
					{
						if (!gameMob.IsUndead())
						{
							return false;
						}
					}
				}
				else if (gameMob.IsUndead())
				{
					return false;
				}
			}
			if (this.resourcesSource == ResourcesGenerationAbilityEffect.ResourcesSource.KilledByAbilityOwnerMob && !gameMob.HitPointsController.IsFinishedOffBy(base.GetEffectOwner()))
			{
				return false;
			}
			resourcesGenerator.GenerateResources(this.resourceType, this.GetResourceAmount(), null);
			return true;
		}

		// Token: 0x06001E38 RID: 7736 RVA: 0x0005FD6E File Offset: 0x0005DF6E
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new ResourcesGenerationAbilityEffect((ResourcesGenerationAbilityEffect)originalBaseEffect);
		}

		// Token: 0x04001108 RID: 4360
		private static readonly CollectableAbilityResourcesFactoryArgs FactoryArgs = new CollectableAbilityResourcesFactoryArgs
		{
			isStatic = true
		};

		// Token: 0x04001109 RID: 4361
		public ResourcesGenerationAbilityEffect.ResourcesSource resourcesSource;

		// Token: 0x0400110A RID: 4362
		public AbilityResourceType resourceType;

		// Token: 0x0400110B RID: 4363
		public int minResourceAmount = 1;

		// Token: 0x0400110C RID: 4364
		public int maxResourceAmount = 1;

		// Token: 0x0400110D RID: 4365
		private IGame currentGame;

		// Token: 0x0400110E RID: 4366
		private ICollectableAbilityResourcesFactory resourcesFactory;

		// Token: 0x02000576 RID: 1398
		public enum ResourcesSource
		{
			// Token: 0x04001C5B RID: 7259
			AbilityOwner,
			// Token: 0x04001C5C RID: 7260
			AliveMob,
			// Token: 0x04001C5D RID: 7261
			UndeadMob,
			// Token: 0x04001C5E RID: 7262
			KilledByAbilityOwnerMob,
			// Token: 0x04001C5F RID: 7263
			Factory
		}
	}
}
