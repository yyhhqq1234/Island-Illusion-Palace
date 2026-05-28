using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Game.Abilities;
using Game.Buffs;
using Unliving.Abilities;
using Unliving.Abilities.Modifiers;

namespace Unliving.LeveledItems
{
	// Token: 0x0200024D RID: 589
	public static class AbilityPropertiesAccessors
	{
		// Token: 0x060013D0 RID: 5072 RVA: 0x0003E520 File Offset: 0x0003C720
		private static PropertyInfo GetPropertyInfo(Type objectType, string propertyName)
		{
			if (objectType == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = null;
			Dictionary<string, PropertyInfo> dictionary;
			if (!AbilityPropertiesAccessors.PropertiesInfoCache.TryGetValue(objectType, out dictionary))
			{
				PropertyInfo[] properties = objectType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				dictionary = new Dictionary<string, PropertyInfo>(properties.Length);
				foreach (PropertyInfo propertyInfo2 in properties)
				{
					if (propertyInfo2.CanRead && propertyInfo2.CanWrite)
					{
						if (propertyInfo == null && propertyInfo2.Name == propertyName)
						{
							propertyInfo = propertyInfo2;
						}
						dictionary.Add(propertyInfo2.Name, propertyInfo2);
					}
				}
				AbilityPropertiesAccessors.PropertiesInfoCache.Add(objectType, dictionary);
			}
			else
			{
				dictionary.TryGetValue(propertyName, out propertyInfo);
			}
			return propertyInfo;
		}

		// Token: 0x060013D1 RID: 5073 RVA: 0x0003E5C4 File Offset: 0x0003C7C4
		private static AbilityEffectBase GetEffect(IAbilityEffectsListProvider effectsListProvider, Type effectType)
		{
			IReadOnlyList<AbilityEffectBase> abilityEffects = effectsListProvider.AbilityEffects;
			for (int i = 0; i < abilityEffects.Count; i++)
			{
				AbilityEffectBase abilityEffectBase = abilityEffects[i];
				if (abilityEffectBase.GetType() == effectType)
				{
					return abilityEffectBase;
				}
			}
			return null;
		}

		// Token: 0x060013D2 RID: 5074 RVA: 0x0003E604 File Offset: 0x0003C804
		private static AbilityEffectBase GetEffect(BaseAbility ability, Type effectType, int buffsGeneratorIndex)
		{
			if (buffsGeneratorIndex >= 0)
			{
				IBuffsGenerator[] buffGenerators = ability.BuffGenerators;
				if (buffGenerators != null && buffsGeneratorIndex < buffGenerators.Length)
				{
					IAbilityEffectsListProvider abilityEffectsListProvider = buffGenerators[buffsGeneratorIndex] as IAbilityEffectsListProvider;
					if (abilityEffectsListProvider != null)
					{
						return AbilityPropertiesAccessors.GetEffect(abilityEffectsListProvider, effectType);
					}
				}
			}
			else
			{
				IAbilityEffectsListProvider abilityEffectsListProvider2 = ability as IAbilityEffectsListProvider;
				if (abilityEffectsListProvider2 != null)
				{
					return AbilityPropertiesAccessors.GetEffect(abilityEffectsListProvider2, effectType);
				}
			}
			return null;
		}

		// Token: 0x060013D3 RID: 5075 RVA: 0x0003E64C File Offset: 0x0003C84C
		private static bool TryGetModifiersOverrides(BaseAbility ability, out AbilityModifiersOverrides modifiersOverrides)
		{
			modifiersOverrides = ability.GetModifiersOverrides();
			return modifiersOverrides != null;
		}

		// Token: 0x060013D4 RID: 5076 RVA: 0x0003E65C File Offset: 0x0003C85C
		private static AbilityPropertiesAccessors.PropertyBuilderSignature CreatePropertyAccessorBuilder(Type abilityType)
		{
			Type type = typeof(AbilityPropertyAccessor<>).MakeGenericType(new Type[]
			{
				abilityType
			});
			BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Public;
			ConstructorInfo constructor = type.GetConstructor(bindingAttr, null, CallingConventions.HasThis, AbilityPropertiesAccessors.PropertyAccessorConstructorArgs, null);
			ParameterExpression[] array = new ParameterExpression[]
			{
				Expression.Parameter(AbilityPropertiesAccessors.PropertyAccessorConstructorArgs[0], "name"),
				Expression.Parameter(AbilityPropertiesAccessors.PropertyAccessorConstructorArgs[1], "getter"),
				Expression.Parameter(AbilityPropertiesAccessors.PropertyAccessorConstructorArgs[2], "setter")
			};
			ConstructorInfo constructor2 = constructor;
			Expression[] arguments = array;
			return Expression.Lambda<AbilityPropertiesAccessors.PropertyBuilderSignature>(Expression.New(constructor2, arguments), array).Compile();
		}

		// Token: 0x060013D5 RID: 5077 RVA: 0x0003E6EC File Offset: 0x0003C8EC
		private static void CreateEffectAmountAccessors(Type effectType, int buffsGeneratorIndex, out Func<BaseAbility, float> getter, out Action<BaseAbility, float> setter)
		{
			AbilityPropertiesAccessors.<>c__DisplayClass14_0 CS$<>8__locals1 = new AbilityPropertiesAccessors.<>c__DisplayClass14_0();
			CS$<>8__locals1.effectType = effectType;
			CS$<>8__locals1.buffsGeneratorIndex = buffsGeneratorIndex;
			getter = new Func<BaseAbility, float>(CS$<>8__locals1.<CreateEffectAmountAccessors>g__GetEffectAmount|0);
			setter = new Action<BaseAbility, float>(CS$<>8__locals1.<CreateEffectAmountAccessors>g__SetEffectAmount|1);
		}

		// Token: 0x060013D6 RID: 5078 RVA: 0x0003E72C File Offset: 0x0003C92C
		private static Delegate CreateAbilityPropertyAccessor(PropertyInfo abilityPropertyInfo, AbilityPropertiesAccessors.NestedPropertyDescription nestedPropertyDescription, bool isGetter = true)
		{
			Type declaringType = abilityPropertyInfo.DeclaringType;
			ParameterExpression parameterExpression = Expression.Parameter(declaringType, "ability");
			Type propertyType;
			Expression expression2;
			if (nestedPropertyDescription != null)
			{
				int propertyHolderIndex = nestedPropertyDescription.propertyHolderIndex;
				PropertyInfo propertyInfo = nestedPropertyDescription.propertyInfo;
				propertyType = propertyInfo.PropertyType;
				Expression expression;
				if (propertyHolderIndex >= 0)
				{
					expression = Expression.ArrayIndex(Expression.Property(parameterExpression, abilityPropertyInfo), Expression.Constant(propertyHolderIndex));
				}
				else
				{
					expression = Expression.Property(parameterExpression, abilityPropertyInfo);
				}
				expression = Expression.Convert(expression, propertyInfo.DeclaringType);
				expression2 = Expression.Property(expression, propertyInfo);
			}
			else
			{
				propertyType = abilityPropertyInfo.PropertyType;
				expression2 = Expression.Property(parameterExpression, abilityPropertyInfo);
			}
			bool flag = propertyType != AbilityPropertiesAccessors.FloatType;
			if (isGetter)
			{
				if (flag)
				{
					expression2 = Expression.Convert(expression2, AbilityPropertiesAccessors.FloatType);
				}
				return Expression.Lambda(typeof(Func<, >).MakeGenericType(new Type[]
				{
					declaringType,
					AbilityPropertiesAccessors.FloatType
				}), expression2, new ParameterExpression[]
				{
					parameterExpression
				}).Compile();
			}
			ParameterExpression parameterExpression2 = Expression.Parameter(AbilityPropertiesAccessors.FloatType, "value");
			expression2 = Expression.Assign(expression2, flag ? Expression.Convert(parameterExpression2, propertyType) : parameterExpression2);
			return Expression.Lambda(typeof(Action<, >).MakeGenericType(new Type[]
			{
				declaringType,
				AbilityPropertiesAccessors.FloatType
			}), expression2, new ParameterExpression[]
			{
				parameterExpression,
				parameterExpression2
			}).Compile();
		}

		// Token: 0x060013D7 RID: 5079 RVA: 0x0003E878 File Offset: 0x0003CA78
		public static IAbilityPropertyAccessor Get(BaseAbility ability, AbilityPropertyDescription propertyDescription)
		{
			IAbilityPropertyAccessor abilityPropertyAccessor;
			AbilityPropertyMetadata abilityPropertyMetadata;
			if (!AbilityPropertiesAccessors.PropertiesAccessors.TryGetValue(propertyDescription, out abilityPropertyAccessor) && AbilityPropertiesAccessors.PropertiesMetadata.TryGetValue(propertyDescription.propertyID, out abilityPropertyMetadata))
			{
				bool flag = false;
				if (abilityPropertyMetadata.AbilityEffectType != null)
				{
					Func<BaseAbility, float> getValueFunc;
					Action<BaseAbility, float> setValueFunc;
					AbilityPropertiesAccessors.CreateEffectAmountAccessors(abilityPropertyMetadata.AbilityEffectType, propertyDescription.buffsGeneratorIndex, out getValueFunc, out setValueFunc);
					abilityPropertyAccessor = new AbilityPropertyAccessor<BaseAbility>(abilityPropertyMetadata.PropertyName, getValueFunc, setValueFunc);
				}
				else
				{
					Type type = ability.GetType();
					AbilityPropertiesAccessors.NestedPropertyDescription nestedPropertyDescription = null;
					IBuffsGenerator[] buffGenerators = ability.BuffGenerators;
					int buffsGeneratorIndex = propertyDescription.buffsGeneratorIndex;
					PropertyInfo propertyInfo;
					if (buffsGeneratorIndex >= 0)
					{
						propertyInfo = AbilityPropertiesAccessors.GetPropertyInfo(type, "BuffGenerators");
						PropertyInfo propertyInfo2 = AbilityPropertiesAccessors.GetPropertyInfo((buffsGeneratorIndex < buffGenerators.Length) ? buffGenerators[buffsGeneratorIndex].GetType() : null, abilityPropertyMetadata.PropertyName);
						if (!(propertyInfo2 != null))
						{
							return null;
						}
						nestedPropertyDescription = new AbilityPropertiesAccessors.NestedPropertyDescription
						{
							propertyInfo = propertyInfo2,
							propertyHolderIndex = buffsGeneratorIndex
						};
					}
					else
					{
						propertyInfo = AbilityPropertiesAccessors.GetPropertyInfo(type, abilityPropertyMetadata.PropertyName);
					}
					if (propertyInfo != null)
					{
						Type declaringType = propertyInfo.DeclaringType;
						Delegate getter = AbilityPropertiesAccessors.CreateAbilityPropertyAccessor(propertyInfo, nestedPropertyDescription, true);
						Delegate setter = AbilityPropertiesAccessors.CreateAbilityPropertyAccessor(propertyInfo, nestedPropertyDescription, false);
						AbilityPropertiesAccessors.PropertyBuilderSignature propertyBuilderSignature;
						if (!AbilityPropertiesAccessors.PropertyAccessorsBuilders.TryGetValue(declaringType, out propertyBuilderSignature))
						{
							propertyBuilderSignature = AbilityPropertiesAccessors.CreatePropertyAccessorBuilder(declaringType);
							AbilityPropertiesAccessors.PropertyAccessorsBuilders.Add(declaringType, propertyBuilderSignature);
						}
						abilityPropertyAccessor = propertyBuilderSignature(abilityPropertyMetadata.PropertyName, getter, setter);
					}
					else
					{
						flag = AbilityPropertiesAccessors.FallbackPropertiesAccessors.TryGetValue(propertyDescription, out abilityPropertyAccessor);
					}
				}
				if (!flag && abilityPropertyAccessor != null)
				{
					AbilityPropertiesAccessors.PropertiesAccessors.Add(propertyDescription, abilityPropertyAccessor);
				}
			}
			return abilityPropertyAccessor;
		}

		// Token: 0x060013D8 RID: 5080 RVA: 0x0003E9F4 File Offset: 0x0003CBF4
		public static IAbilityPropertyAccessor GetPropertyAccessor(BaseAbility ability, AbilityPropertyID propertyID)
		{
			return AbilityPropertiesAccessors.Get(ability, (AbilityPropertyDescription)propertyID);
		}

		// Token: 0x04000B87 RID: 2951
		private static readonly Dictionary<AbilityPropertyID, AbilityPropertyMetadata> PropertiesMetadata = new Dictionary<AbilityPropertyID, AbilityPropertyMetadata>
		{
			{
				AbilityPropertyID.Range,
				new AbilityPropertyMetadata("Range", null)
			},
			{
				AbilityPropertyID.PrepTime,
				new AbilityPropertyMetadata("PrepTime", null)
			},
			{
				AbilityPropertyID.UsingDuration,
				new AbilityPropertyMetadata("UsingDuration", null)
			},
			{
				AbilityPropertyID.MaxUsingCount,
				new AbilityPropertyMetadata("MaxUsingCount", null)
			},
			{
				AbilityPropertyID.ReloadingTime,
				new AbilityPropertyMetadata("ReloadingTime", null)
			},
			{
				AbilityPropertyID.Cost,
				new AbilityPropertyMetadata("Cost", null)
			},
			{
				AbilityPropertyID.MaxTargetsInRange,
				new AbilityPropertyMetadata("MaxTargetsInRange", null)
			},
			{
				AbilityPropertyID.BuffsDuration,
				new AbilityPropertyMetadata("BuffDuration", null)
			},
			{
				AbilityPropertyID.MaxShotsPerUsing,
				new AbilityPropertyMetadata("MaxShotsPerUsing", null)
			},
			{
				AbilityPropertyID.ProjectileEffectRange,
				new AbilityPropertyMetadata("ProjectileEffectRange", null)
			},
			{
				AbilityPropertyID.MaxProjectileHitCount,
				new AbilityPropertyMetadata("ProjectileMaxHitCount", null)
			},
			{
				AbilityPropertyID.SummoningCount,
				new AbilityPropertyMetadata("SummoningCount", null)
			},
			{
				AbilityPropertyID.MaxSummoningCount,
				new AbilityPropertyMetadata("MaxSummoningCount", null)
			},
			{
				AbilityPropertyID.SummonedMobsMaxLifetime,
				new AbilityPropertyMetadata("SummonedMobsMaxLifetime", null)
			},
			{
				AbilityPropertyID.DamageEffectAmount,
				new AbilityPropertyMetadata(typeof(DamageAbilityEffect))
			},
			{
				AbilityPropertyID.HealingEffectAmount,
				new AbilityPropertyMetadata(typeof(HealingAbilityEffect))
			},
			{
				AbilityPropertyID.DamageMultiplierEffectAmount,
				new AbilityPropertyMetadata(typeof(IncomingDamageModificationAbilityEffect))
			},
			{
				AbilityPropertyID.PushingImpulseEffectAmount,
				new AbilityPropertyMetadata(typeof(PushingImpulseAbilityEffect))
			},
			{
				AbilityPropertyID.StaminaRestoringEffectAmount,
				new AbilityPropertyMetadata(typeof(StaminaRestoringAbilityEffect))
			},
			{
				AbilityPropertyID.SummoningEffectMobsCount,
				new AbilityPropertyMetadata(typeof(SummoningAbilityEffect))
			}
		};

		// Token: 0x04000B88 RID: 2952
		private static readonly Type[] PropertyAccessorConstructorArgs = new Type[]
		{
			typeof(string),
			typeof(Delegate),
			typeof(Delegate)
		};

		// Token: 0x04000B89 RID: 2953
		private static readonly Type FloatType = typeof(float);

		// Token: 0x04000B8A RID: 2954
		private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertiesInfoCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>(64);

		// Token: 0x04000B8B RID: 2955
		private static readonly Dictionary<Type, AbilityPropertiesAccessors.PropertyBuilderSignature> PropertyAccessorsBuilders = new Dictionary<Type, AbilityPropertiesAccessors.PropertyBuilderSignature>(64);

		// Token: 0x04000B8C RID: 2956
		private static readonly Dictionary<AbilityPropertyDescription, IAbilityPropertyAccessor> PropertiesAccessors = new Dictionary<AbilityPropertyDescription, IAbilityPropertyAccessor>(64)
		{
			{
				(AbilityPropertyDescription)AbilityPropertyID.ModifierUsingCount,
				new AbilityPropertyAccessor<BaseAbility>("ModifierUsingCount", delegate(BaseAbility ability)
				{
					AbilityModifiersOverrides abilityModifiersOverrides;
					if (!AbilityPropertiesAccessors.TryGetModifiersOverrides(ability, out abilityModifiersOverrides))
					{
						return 0f;
					}
					return (float)abilityModifiersOverrides.usingCountOverride;
				}, delegate(BaseAbility ability, float value)
				{
					ability.GetOrAddModifiersOverrides().usingCountOverride = (int)value;
				})
			},
			{
				(AbilityPropertyDescription)AbilityPropertyID.ModifierResourcesAddition,
				new AbilityPropertyAccessor<BaseAbility>("ModifierResourcesAddition", delegate(BaseAbility ability)
				{
					AbilityModifiersOverrides abilityModifiersOverrides;
					if (!AbilityPropertiesAccessors.TryGetModifiersOverrides(ability, out abilityModifiersOverrides))
					{
						return 0f;
					}
					return (float)abilityModifiersOverrides.activationResourcesAddition;
				}, delegate(BaseAbility ability, float value)
				{
					ability.GetOrAddModifiersOverrides().activationResourcesAddition = (int)value;
				})
			}
		};

		// Token: 0x04000B8D RID: 2957
		private static readonly Dictionary<AbilityPropertyDescription, IAbilityPropertyAccessor> FallbackPropertiesAccessors = new Dictionary<AbilityPropertyDescription, IAbilityPropertyAccessor>
		{
			{
				(AbilityPropertyDescription)AbilityPropertyID.SummonedMobsMaxLifetime,
				new AbilityPropertyAccessor<BaseAbility>("SummonedMobsMaxLifetime", delegate(BaseAbility ability)
				{
					AbilityModifiersOverrides abilityModifiersOverrides;
					if (!AbilityPropertiesAccessors.TryGetModifiersOverrides(ability, out abilityModifiersOverrides))
					{
						return 0f;
					}
					return abilityModifiersOverrides.summonedMobsLifetimeOverride;
				}, delegate(BaseAbility ability, float value)
				{
					ability.GetOrAddModifiersOverrides().summonedMobsLifetimeOverride = value;
				})
			}
		};

		// Token: 0x020004D2 RID: 1234
		private sealed class NestedPropertyDescription
		{
			// Token: 0x040019DE RID: 6622
			public PropertyInfo propertyInfo;

			// Token: 0x040019DF RID: 6623
			public int propertyHolderIndex;
		}

		// Token: 0x020004D3 RID: 1235
		// (Invoke) Token: 0x06002556 RID: 9558
		private delegate IAbilityPropertyAccessor PropertyBuilderSignature(string propertyName, Delegate getter, Delegate setter);
	}
}
