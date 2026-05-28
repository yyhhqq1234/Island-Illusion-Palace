using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common;
using Common.Factories;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Abilities;
using Game.Buffs;
using Game.Core;
using Game.Damage;
using Game.Damage.Projectiles;
using Game.ObjectPool;
using UnityEngine;
using Unliving.Abilities.AbilitiesGeneration;
using Unliving.Abilities.Modifiers;
using Unliving.Abilities.VFX;
using Unliving.Factories;
using Unliving.LeveledItems;
using Unliving.MobsStats;
using Unliving.Pickables;

namespace Unliving.Abilities
{
	// Token: 0x0200039A RID: 922
	[Service(typeof(AbilitiesFactory), new Type[]
	{
		typeof(IGameAbilitiesFactory),
		typeof(IObjectFactory<BaseAbility>)
	})]
	public class AbilitiesFactory : PrototypeBasedFactory<AbilityFactoryPrototype, BaseAbility>, IGameAbilitiesFactory, IFactory<AbilityFactoryArgs, UnityEngine.Object>, IFactory, IAbilityPropertiesOverridesHandler, IInitializable<IGame>
	{
		// Token: 0x06001E67 RID: 7783 RVA: 0x0006045A File Offset: 0x0005E65A
		public static bool CanGenerateEffectZone(BaseAbility ability)
		{
			return ability != null && ability.CanGenerateBuffs;
		}

		// Token: 0x06001E68 RID: 7784 RVA: 0x00060470 File Offset: 0x0005E670
		public static AbilityEffectZone CreateEffectZone(BaseAbility ability, BaseAbility.UsingArgs usingArgs, MonoBehaviour owner, float lifetime, float customRange = 0f, MobStatModifier? damageModifier = null)
		{
			float range = (customRange > 0f) ? customRange : ability.Range;
			Vector3 position = (usingArgs != null) ? usingArgs.targetPosition : owner.transform.position;
			IBuffsGenerator[] array;
			ability.BuffsGeneratorsBuilders.Instantiate(out array);
			if (damageModifier != null)
			{
				MobStatModifier value = damageModifier.Value;
				if (!value.IsNeutral())
				{
					for (int i = 0; i < array.Length; i++)
					{
						IDamageSender damageSender = array[i] as IDamageSender;
						DamageGenerator damageGenerator = (damageSender != null) ? damageSender.DamageGenerator : null;
						if (damageGenerator != null)
						{
							damageGenerator.SetDamageAmount(value.GetModifiedStatValue(damageGenerator.amount), -1f);
						}
					}
				}
			}
			AbilityEffectZone abilityEffectZone = AbilityEffectZone.Create(array, position, owner, lifetime, range);
			if (abilityEffectZone != null)
			{
				abilityEffectZone.affectableObjectsLayers = ability.ValidObjectLayers;
				MobAbilityParameters mobAbilityParameters;
				if (ability.TryGetExtension(out mobAbilityParameters) && mobAbilityParameters.useDescriptionForTargetsFiltering && !mobAbilityParameters.allowedTargetsDescription.IsBlank())
				{
					abilityEffectZone.affectableMobsDescription = mobAbilityParameters.allowedTargetsDescription;
				}
			}
			return abilityEffectZone;
		}

		// Token: 0x06001E69 RID: 7785 RVA: 0x00060568 File Offset: 0x0005E768
		public static AbilityEffectZone CreateAuraEffect(MonoBehaviour owner, BaseAbility ability, MobStatModifier? damageModifier = null)
		{
			AbilityEffectZone abilityEffectZone = AbilitiesFactory.CreateEffectZone(ability, null, owner, -1f, 0f, damageModifier);
			if (abilityEffectZone != null)
			{
				abilityEffectZone.transform.parent = owner.transform;
				IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
				for (int i = 0; i < extensions.Count; i++)
				{
					INotifyAuraEffectCreated notifyAuraEffectCreated = extensions[i] as INotifyAuraEffectCreated;
					if (notifyAuraEffectCreated != null)
					{
						notifyAuraEffectCreated.OnAuraEffectCreated(ability, abilityEffectZone);
					}
				}
			}
			return abilityEffectZone;
		}

		// Token: 0x17000623 RID: 1571
		// (get) Token: 0x06001E6A RID: 7786 RVA: 0x000605D5 File Offset: 0x0005E7D5
		// (set) Token: 0x06001E6B RID: 7787 RVA: 0x000605DD File Offset: 0x0005E7DD
		public IAbilityPropertiesOverridesSource AbilityPropertiesOverridesSource { get; set; }

		// Token: 0x06001E6C RID: 7788 RVA: 0x000605E8 File Offset: 0x0005E7E8
		private void SetAbilityEffectsPool(BaseAbility ability)
		{
			IReadOnlyList<IAbilityExtension> extensions = ability.Extensions;
			for (int i = 0; i < extensions.Count; i++)
			{
				AbilitiesFactory.<SetAbilityEffectsPool>g__SetEffectsPool|16_0(extensions[i], this.abilityEffectsPool);
			}
		}

		// Token: 0x06001E6D RID: 7789 RVA: 0x00060620 File Offset: 0x0005E820
		private void InitializeAbility(BaseAbility ability, AbilityFactoryArgs args, BaseAbility abilityPrototype)
		{
			ability.Prototype = (args.abilityPrototype ?? abilityPrototype);
			ability.ID = (int)args.abilityID;
			ILeveledItem leveledItem = ability as ILeveledItem;
			if (leveledItem != null)
			{
				leveledItem.ItemLevel = Mathf.Max(args.abilityLevel, 1);
			}
			if (this.specialBehavioursGenerator != null && args.specialBehaviourDescription != null)
			{
				this.specialBehavioursGenerator.TrySetSpecialBehaviour(ability, args.specialBehaviourDescription);
			}
			if (args.targetsLayersOverride != null)
			{
				ability.ValidObjectLayers = args.targetsLayersOverride.Value;
			}
			if (args.rangeOverride != null)
			{
				ability.Range = args.rangeOverride.Value;
			}
			if (args.reloadingTimeOverride != null)
			{
				ability.ReloadingTime = args.reloadingTimeOverride.Value;
			}
			if (!args.canGenerateBuffs)
			{
				ability.BuffsGeneratorsBuilders = null;
			}
			if (args.reloadingProgressOverride != null)
			{
				ability.SetReloadingProgress(args.reloadingProgressOverride.Value);
			}
			ProjectileAbilityBase projectileAbilityBase = ability as ProjectileAbilityBase;
			if (projectileAbilityBase != null)
			{
				projectileAbilityBase.ProjectilesSpawner = this.projectilesSpawner;
			}
			ability.ParentAbility = args.parentAbility;
			ability.AbilityEffectSender = args.abilityEffectSender;
			ability.Owner = args.abilityOwner;
			ability.Initialize(this);
			EffectBasedAbility effectBasedAbility = ability as EffectBasedAbility;
			AbilityEffectBase[] array = (effectBasedAbility != null) ? effectBasedAbility.AbilityEffects : null;
			if (array != null)
			{
				for (int i = 0; i < array.Length; i++)
				{
					IGameContextDependent gameContextDependent = array[i] as IGameContextDependent;
					if (gameContextDependent != null)
					{
						gameContextDependent.CurrentGame = this.currentGame;
					}
				}
			}
			if (this.abilityEffectsPool != null || this.currentGame.Services.TryGet<IUnityObjectPool<ParticleSystem>>(out this.abilityEffectsPool))
			{
				this.SetAbilityEffectsPool(ability);
			}
			IAbilityLevelingController abilityLevelingController;
			if (this.AbilityPropertiesOverridesSource != null && ability.TryGetLevelingController(out abilityLevelingController))
			{
				IAbilityLevelingController extension = abilityPrototype.GetExtension<IAbilityLevelingController>();
				abilityLevelingController.AbilityPropertiesOverrides = this.AbilityPropertiesOverridesSource.GetAbilityPropertiesOverrides(extension);
			}
		}

		// Token: 0x06001E6E RID: 7790 RVA: 0x000607F8 File Offset: 0x0005E9F8
		protected override BaseAbility Create(AbilityFactoryPrototype data, IBaseObjectDescription args)
		{
			if (data == null)
			{
				return null;
			}
			BaseAbility baseAbility = UnityEngine.Object.Instantiate<BaseAbility>(data.abilityPrototype);
			this.InitializeAbility(baseAbility, (AbilityFactoryArgs)args, data.abilityPrototype);
			return baseAbility;
		}

		// Token: 0x06001E6F RID: 7791 RVA: 0x0006082A File Offset: 0x0005EA2A
		public AbilityFactoryPrototype GetAbilityPrototypeData(int abilityID)
		{
			return base.GetObjectPrototype(abilityID);
		}

		// Token: 0x06001E70 RID: 7792 RVA: 0x00060834 File Offset: 0x0005EA34
		public UnityEngine.Object Create(AbilityFactoryArgs args)
		{
			if (args.abilityPrototype != null && args.objectType == MultiRepresentationObjectInstantiator.ObjectType.Default)
			{
				BaseAbility baseAbility = UnityEngine.Object.Instantiate<BaseAbility>(args.abilityPrototype);
				this.InitializeAbility(baseAbility, args, args.abilityPrototype);
				return baseAbility;
			}
			if (this.representationsInstantiator == null)
			{
				this.representationsInstantiator = new MultiRepresentationObjectInstantiator(new Func<MultiRepresentationObjectInstantiator.IArgs, MultiRepresentationObjectInstantiator.IObjectData>(base.GetObjectPrototype), this.defaultAbilitiesData, new Action<UnityEngine.Object, MultiRepresentationObjectInstantiator.IObjectData, MultiRepresentationObjectInstantiator.IArgs>(this.OnMultiRepresentationObjectCreated));
			}
			return this.representationsInstantiator.CreateObject<AbilityID>(args, new Func<object, object>(base.Create));
		}

		// Token: 0x06001E71 RID: 7793 RVA: 0x000608BD File Offset: 0x0005EABD
		public override object Create(object args)
		{
			return this.Create((AbilityFactoryArgs)args);
		}

		// Token: 0x06001E72 RID: 7794 RVA: 0x000608CC File Offset: 0x0005EACC
		protected virtual void OnMultiRepresentationObjectCreated(UnityEngine.Object createdObject, MultiRepresentationObjectInstantiator.IObjectData objectData, MultiRepresentationObjectInstantiator.IArgs args)
		{
			IPurchasableObject purchasableObject = createdObject.CastOrGetComponent<IPurchasableObject>();
			if (purchasableObject != null)
			{
				AbilityFactoryArgs abilityFactoryArgs = args as AbilityFactoryArgs;
				if (abilityFactoryArgs != null && !abilityFactoryArgs.preventRandomSpecialBehaviourGeneration && abilityFactoryArgs.specialBehaviourDescription == null)
				{
					abilityFactoryArgs.specialBehaviourDescription = this.GetRandomAbilitySpecialBehaviourDescription((int)abilityFactoryArgs.abilityID);
				}
				AbilityFactoryPrototype abilityFactoryPrototype = objectData as AbilityFactoryPrototype;
				if (abilityFactoryPrototype != null)
				{
					if (abilityFactoryPrototype.abilityIcon.IsNull())
					{
						abilityFactoryPrototype.abilityIcon = this.defaultAbilitiesData.objectIcon;
					}
					if (abilityFactoryPrototype.abilityUIIcon.IsNull())
					{
						abilityFactoryPrototype.abilityUIIcon = this.defaultAbilitiesData.uiIcon;
					}
				}
				purchasableObject.CurrentPickingContext = args.Type;
				purchasableObject.InitializeData(args, objectData);
			}
		}

		// Token: 0x06001E73 RID: 7795 RVA: 0x00060972 File Offset: 0x0005EB72
		public AbilitySpecialBehaviourDescription GetAbilitySpecialBehaviourDescription(BaseAbility ability)
		{
			AbilitySpecialBehaviourGenerator<AbilityID> abilitySpecialBehaviourGenerator = this.specialBehavioursGenerator;
			if (abilitySpecialBehaviourGenerator == null)
			{
				return null;
			}
			return abilitySpecialBehaviourGenerator.GetSpecialBehaviourDescription(ability);
		}

		// Token: 0x06001E74 RID: 7796 RVA: 0x00060986 File Offset: 0x0005EB86
		public AbilitySpecialBehaviourDescription GetRandomAbilitySpecialBehaviourDescription(int abilityID)
		{
			AbilitySpecialBehaviourGenerator<AbilityID> abilitySpecialBehaviourGenerator = this.specialBehavioursGenerator;
			if (abilitySpecialBehaviourGenerator == null)
			{
				return null;
			}
			return abilitySpecialBehaviourGenerator.GetRandomSpecialBehaviourDescription(abilityID, this.specialBehavioursOptionsFilter);
		}

		// Token: 0x06001E75 RID: 7797 RVA: 0x000609A0 File Offset: 0x0005EBA0
		public void SetAbilitySpecialBehaviourFilterPredicate(Predicate<AbilitySpecialBehaviourGenerationOption> optionsFilter)
		{
			this.specialBehavioursOptionsFilter = optionsFilter;
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x000609AC File Offset: 0x0005EBAC
		public AbilityEffectZone CreateEffectZone(AbilityID abilityID, MonoBehaviour owner, float zoneLifetime, float rangeOverride = 0f)
		{
			BaseAbility baseAbility = (BaseAbility)this.Create(new AbilityFactoryArgs
			{
				abilityID = abilityID
			});
			if (baseAbility != null)
			{
				baseAbility.Owner = owner;
				return AbilitiesFactory.CreateEffectZone(baseAbility, null, owner, zoneLifetime, rangeOverride, null);
			}
			return null;
		}

		// Token: 0x06001E77 RID: 7799 RVA: 0x000609F8 File Offset: 0x0005EBF8
		void IInitializable<IGame>.Initialize(IGame game)
		{
			this.currentGame = game;
			game.Services.TryGet<IProjectilesSpawner>(out this.projectilesSpawner);
			if (this.specialBehavioursGenerationData != null)
			{
				IAbilitySpecialBehavioursFactory abilitySpecialBehavioursFactory = game.Services.Get<IAbilitySpecialBehavioursFactory>();
				IAbilitySpecialBehavioursActivatorsFactory abilitySpecialBehavioursActivatorsFactory = game.Services.Get<IAbilitySpecialBehavioursActivatorsFactory>();
				if (abilitySpecialBehavioursFactory != null && abilitySpecialBehavioursActivatorsFactory != null)
				{
					this.specialBehavioursGenerator = new AbilitySpecialBehaviourGenerator<AbilityID>(this.specialBehavioursGenerationData, abilitySpecialBehavioursFactory, abilitySpecialBehavioursActivatorsFactory);
				}
			}
		}

		// Token: 0x06001E79 RID: 7801 RVA: 0x00060A60 File Offset: 0x0005EC60
		[CompilerGenerated]
		internal static void <SetAbilityEffectsPool>g__SetEffectsPool|16_0(IAbilityExtension abilityExtension, IUnityObjectPool<ParticleSystem> pool)
		{
			AbilityVFXController abilityVFXController = abilityExtension as AbilityVFXController;
			if (abilityVFXController != null)
			{
				abilityVFXController.EffectsPool = pool;
				return;
			}
			AbilityModifiersController abilityModifiersController = abilityExtension as AbilityModifiersController;
			if (abilityModifiersController == null)
			{
				return;
			}
			AbilityModifiersController.VFXController modifiersVFX = abilityModifiersController.ModifiersVFX;
			if (modifiersVFX != null && modifiersVFX.EffectsPool == null)
			{
				modifiersVFX.EffectsPool = pool;
			}
		}

		// Token: 0x04001123 RID: 4387
		public const int NullAbilityID = 0;

		// Token: 0x04001125 RID: 4389
		public MultiRepresentationObjectInstantiator.DefaultData defaultAbilitiesData;

		// Token: 0x04001126 RID: 4390
		public IReadOnlyList<AbilitySpecialBehaviourGenerator<AbilityID>.AbilityData> specialBehavioursGenerationData;

		// Token: 0x04001127 RID: 4391
		private IGame currentGame;

		// Token: 0x04001128 RID: 4392
		private MultiRepresentationObjectInstantiator representationsInstantiator;

		// Token: 0x04001129 RID: 4393
		private IProjectilesSpawner projectilesSpawner;

		// Token: 0x0400112A RID: 4394
		private IUnityObjectPool<ParticleSystem> abilityEffectsPool;

		// Token: 0x0400112B RID: 4395
		private AbilitySpecialBehaviourGenerator<AbilityID> specialBehavioursGenerator;

		// Token: 0x0400112C RID: 4396
		private Predicate<AbilitySpecialBehaviourGenerationOption> specialBehavioursOptionsFilter;
	}
}
