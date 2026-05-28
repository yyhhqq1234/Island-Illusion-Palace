using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Abilities;
using UnityEngine;
using UnityEngine.Serialization;
using Unliving.Mobs;

namespace Unliving.Abilities
{
	// Token: 0x02000381 RID: 897
	[Serializable]
	public sealed class AbilityAdditionAbilityEffect : StateBasedAbilityEffect
	{
		// Token: 0x06001D9C RID: 7580 RVA: 0x0005DF60 File Offset: 0x0005C160
		public AbilityAdditionAbilityEffect()
		{
		}

		// Token: 0x06001D9D RID: 7581 RVA: 0x0005DF84 File Offset: 0x0005C184
		public AbilityAdditionAbilityEffect(AbilityAdditionAbilityEffect effectPrototype)
		{
			this.abilitiesToAdd = effectPrototype.abilitiesToAdd;
			this.passIDToChildAbility = effectPrototype.passIDToChildAbility;
			this.abilityLevelOverride = effectPrototype.abilityLevelOverride;
			this.passLevelToChildAbility = effectPrototype.passLevelToChildAbility;
			this.passEffectSenderToChildAbility = effectPrototype.passEffectSenderToChildAbility;
			this.removeAbilitiesAfterEffectUsing = effectPrototype.removeAbilitiesAfterEffectUsing;
			base.CopyCommonParameters(effectPrototype);
		}

		// Token: 0x06001D9E RID: 7582 RVA: 0x0005E004 File Offset: 0x0005C204
		protected override void SetEffectActive(Component effectTarget, bool isActive)
		{
			if (this.abilitiesToAdd == null || this.abilitiesToAdd.Length == 0)
			{
				return;
			}
			if (isActive)
			{
				BaseGameMob baseGameMob = effectTarget.CastOrGetComponent<BaseGameMob>();
				BaseAbilitiesController baseAbilitiesController = (baseGameMob != null) ? baseGameMob.AbilitiesController : null;
				if (baseAbilitiesController != null)
				{
					IGameAbilitiesFactory gameAbilitiesFactory = baseGameMob.CurrentGame.Services.Get<IGameAbilitiesFactory>();
					if (gameAbilitiesFactory == null)
					{
						return;
					}
					AbilityFactoryArgs abilitiesFactoryArgs = AbilityAdditionAbilityEffect.AbilitiesFactoryArgs;
					AbilityID abilityID;
					if (!this.passIDToChildAbility)
					{
						abilityID = AbilityID.None;
					}
					else
					{
						BaseAbility currentAbility = this.currentAbility;
						abilityID = (AbilityID)((currentAbility != null) ? currentAbility.ID : 0);
					}
					abilitiesFactoryArgs.abilityID = abilityID;
					AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.abilityEffectSender = null;
					if (this.passEffectSenderToChildAbility)
					{
						BaseAbility currentAbility2 = this.currentAbility;
						object obj = (currentAbility2 != null) ? currentAbility2.AbilityEffectSender : null;
						if (obj == null)
						{
							obj = base.GetEffectOwner();
						}
						BaseGameMob baseGameMob2 = obj as BaseGameMob;
						if (baseGameMob2 != null && baseGameMob2.IsPlayerMob)
						{
							obj = baseGameMob2.Group.Leader;
						}
						AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.abilityEffectSender = obj;
					}
					if (this.abilityLevelOverride > 0)
					{
						AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.abilityLevel = this.abilityLevelOverride;
					}
					else if (this.passLevelToChildAbility)
					{
						AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.SetLevelFromAbility(this.currentAbility);
					}
					else
					{
						AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.abilityLevel = 1;
					}
					for (int i = 0; i < this.abilitiesToAdd.Length; i++)
					{
						BaseAbility baseAbility = this.abilitiesToAdd[i];
						if (!(baseAbility == null) && (this.skipAbilityDuplicatesCheck || !baseAbilitiesController.HasAbilityWithPrototype(baseAbility)))
						{
							AbilityAdditionAbilityEffect.AbilitiesFactoryArgs.abilityPrototype = baseAbility;
							BaseAbility baseAbility2 = (BaseAbility)gameAbilitiesFactory.Create(AbilityAdditionAbilityEffect.AbilitiesFactoryArgs);
							if (baseAbilitiesController.AddAbility(baseAbility2))
							{
								if (this.removeAbilitiesAfterEffectUsing)
								{
									this.addedAbilities.Add(new AbilityAdditionAbilityEffect.AddedAbilityInfo(baseAbilitiesController, baseAbility2));
								}
							}
							else if (baseAbility2 != null)
							{
								UnityEngine.Object.Destroy(baseAbility2);
							}
						}
					}
					return;
				}
			}
			else if (this.addedAbilities.Count != 0)
			{
				for (int j = this.addedAbilities.Count - 1; j >= 0; j--)
				{
					AbilityAdditionAbilityEffect.AddedAbilityInfo addedAbilityInfo = this.addedAbilities[j];
					if (addedAbilityInfo.Ability.IsNull())
					{
						this.addedAbilities.RemoveAt(j);
					}
					else
					{
						MonoBehaviour ownerBehaviour = addedAbilityInfo.Ability.OwnerBehaviour;
						if (addedAbilityInfo.Ability.IsPostMortemAbility)
						{
							BaseGameMob baseGameMob3 = ownerBehaviour as BaseGameMob;
							if (baseGameMob3 != null && baseGameMob3.IsKilled)
							{
								goto IL_254;
							}
						}
						if (ownerBehaviour == effectTarget)
						{
							addedAbilityInfo.AbilitiesController.RemoveAbility(addedAbilityInfo.Ability);
							this.addedAbilities.RemoveAt(j);
							return;
						}
					}
					IL_254:;
				}
			}
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x0005E273 File Offset: 0x0005C473
		protected override float GetEffectAmount()
		{
			return 0f;
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x0005E27A File Offset: 0x0005C47A
		protected override void SetEffectAmount(float newAmount)
		{
		}

		// Token: 0x06001DA1 RID: 7585 RVA: 0x0005E27C File Offset: 0x0005C47C
		protected override AbilityEffectBase Clone(AbilityEffectBase originalBaseEffect)
		{
			return new AbilityAdditionAbilityEffect((AbilityAdditionAbilityEffect)originalBaseEffect);
		}

		// Token: 0x040010BE RID: 4286
		private static readonly AbilityFactoryArgs AbilitiesFactoryArgs = new AbilityFactoryArgs
		{
			canGenerateBuffs = true
		};

		// Token: 0x040010BF RID: 4287
		public BaseAbility[] abilitiesToAdd;

		// Token: 0x040010C0 RID: 4288
		[Tooltip("Если активно, то мобу будут добавлены абилити из списка даже в том случае, если они у него уже есть.")]
		public bool skipAbilityDuplicatesCheck;

		// Token: 0x040010C1 RID: 4289
		[FormerlySerializedAs("passParentAbilityID")]
		[Tooltip("Опция для передачи id от текущей абилити к добавленной мобу. Рекомендуется использовать только в том случае, когда чайлд и парент абилити идентичны по действию и чайлд абилити должна создавать баффы.")]
		public bool passIDToChildAbility;

		// Token: 0x040010C2 RID: 4290
		public int abilityLevelOverride;

		// Token: 0x040010C3 RID: 4291
		public bool passLevelToChildAbility = true;

		// Token: 0x040010C4 RID: 4292
		[FormerlySerializedAs("passParentAbilityEffectSender")]
		public bool passEffectSenderToChildAbility = true;

		// Token: 0x040010C5 RID: 4293
		[Tooltip("Если опция активна, то добавленные абилити будут удалены либо после уничтожения баффа, либо когда закончится продолжительное использование абилити.")]
		public bool removeAbilitiesAfterEffectUsing;

		// Token: 0x040010C6 RID: 4294
		private readonly List<AbilityAdditionAbilityEffect.AddedAbilityInfo> addedAbilities = new List<AbilityAdditionAbilityEffect.AddedAbilityInfo>(16);

		// Token: 0x0200056F RID: 1391
		private readonly struct AddedAbilityInfo
		{
			// Token: 0x06002722 RID: 10018 RVA: 0x0007A1D2 File Offset: 0x000783D2
			public AddedAbilityInfo(BaseAbilitiesController controller, BaseAbility ability)
			{
				this.AbilitiesController = controller;
				this.Ability = ability;
			}

			// Token: 0x04001C45 RID: 7237
			public readonly BaseAbilitiesController AbilitiesController;

			// Token: 0x04001C46 RID: 7238
			public readonly BaseAbility Ability;
		}
	}
}
