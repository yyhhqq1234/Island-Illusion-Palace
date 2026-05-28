using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Common.Editor;
using Game.Abilities;
using Unity.Collections.LowLevel.Unsafe;
using Unliving.Abilities.Modifiers;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003E9 RID: 1001
	public sealed class AbilitySpecialBehaviourGenerator<TAbilityID> where TAbilityID : Enum
	{
		// Token: 0x060021D5 RID: 8661 RVA: 0x000698CC File Offset: 0x00067ACC
		private AbilitySpecialBehaviourDescription GetRandomSpecialBehaviourDescription(int abilityID, bool getCopy, Predicate<AbilitySpecialBehaviourGenerationOption> optionsFilter)
		{
			List<AbilitySpecialBehaviourGenerationOption> list;
			if (this.generationOptions.TryGetValue(abilityID, out list))
			{
				this.optionsBuffer.Clear();
				if (optionsFilter != null)
				{
					for (int i = 0; i < list.Count; i++)
					{
						AbilitySpecialBehaviourGenerationOption abilitySpecialBehaviourGenerationOption = list[i];
						if (optionsFilter(abilitySpecialBehaviourGenerationOption))
						{
							this.optionsBuffer.Add(abilitySpecialBehaviourGenerationOption);
						}
					}
				}
				if (this.optionsBuffer.Count == 0)
				{
					return null;
				}
				AbilitySpecialBehaviourGenerationOption abilitySpecialBehaviourGenerationOption2;
				if (this.optionsBuffer.GetRandomWeightedItem(out abilitySpecialBehaviourGenerationOption2, 0, 2147483647, null))
				{
					if (!getCopy)
					{
						return abilitySpecialBehaviourGenerationOption2;
					}
					return new AbilitySpecialBehaviourGenerationOption(abilitySpecialBehaviourGenerationOption2);
				}
			}
			return null;
		}

		// Token: 0x060021D6 RID: 8662 RVA: 0x00069960 File Offset: 0x00067B60
		public unsafe AbilitySpecialBehaviourGenerator(IReadOnlyCollection<AbilitySpecialBehaviourGenerator<TAbilityID>.AbilityData> specialBehavioursData, IAbilitySpecialBehavioursFactory behavioursFactory, IAbilitySpecialBehavioursActivatorsFactory activatorsFactory)
		{
			this.generationOptions = new Dictionary<int, List<AbilitySpecialBehaviourGenerationOption>>(specialBehavioursData.Count);
			foreach (AbilitySpecialBehaviourGenerator<TAbilityID>.AbilityData abilityData in specialBehavioursData)
			{
				int key = *UnsafeUtility.As<TAbilityID, int>(ref abilityData.abilityID);
				ICollection<AbilitySpecialBehaviourGenerationOption> abilityBehavioursDescriptions = abilityData.behavioursGenerationOptions.GetAbilityBehavioursDescriptions();
				this.generationOptions.Add(key, new List<AbilitySpecialBehaviourGenerationOption>(abilityBehavioursDescriptions));
			}
			this.behavioursFactory = behavioursFactory;
			this.activatorsFactory = activatorsFactory;
		}

		// Token: 0x060021D7 RID: 8663 RVA: 0x000699FC File Offset: 0x00067BFC
		public bool CanGenerateSpecialBehaviour(BaseAbility ability)
		{
			List<AbilitySpecialBehaviourGenerationOption> list;
			return this.generationOptions.TryGetValue(ability.ID, out list) && list.Count != 0;
		}

		// Token: 0x060021D8 RID: 8664 RVA: 0x00069A29 File Offset: 0x00067C29
		public AbilitySpecialBehaviourDescription GetRandomSpecialBehaviourDescription(int abilityID, Predicate<AbilitySpecialBehaviourGenerationOption> optionsFilter)
		{
			return this.GetRandomSpecialBehaviourDescription(abilityID, true, optionsFilter);
		}

		// Token: 0x060021D9 RID: 8665 RVA: 0x00069A34 File Offset: 0x00067C34
		public bool TrySetSpecialBehaviour(BaseAbility ability, AbilitySpecialBehaviourDescription behaviourDescription)
		{
			if (behaviourDescription == null || behaviourDescription.IsBlank())
			{
				return false;
			}
			AbilityModifiersActivatorBase abilityModifiersActivatorBase = this.activatorsFactory.Create(behaviourDescription.behaviourActivatorID);
			if (abilityModifiersActivatorBase != null)
			{
				AbilityModifiersController abilityModifiersController = this.behavioursFactory.Create(behaviourDescription.behaviourID);
				if (abilityModifiersController != null)
				{
					abilityModifiersController.Activator = abilityModifiersActivatorBase;
					ability.AddExtension(abilityModifiersController);
					return true;
				}
			}
			return false;
		}

		// Token: 0x060021DA RID: 8666 RVA: 0x00069A9C File Offset: 0x00067C9C
		public bool TryGenerateRandomSpecialBehaviour(BaseAbility ability, Predicate<AbilitySpecialBehaviourGenerationOption> optionsFilter)
		{
			AbilitySpecialBehaviourDescription randomSpecialBehaviourDescription = this.GetRandomSpecialBehaviourDescription(ability.ID, false, optionsFilter);
			return this.TrySetSpecialBehaviour(ability, randomSpecialBehaviourDescription);
		}

		// Token: 0x060021DB RID: 8667 RVA: 0x00069AC0 File Offset: 0x00067CC0
		public AbilitySpecialBehaviourDescription GetSpecialBehaviourDescription(BaseAbility ability)
		{
			AbilitySpecialBehaviourID specialBehaviourID = this.behavioursFactory.GetSpecialBehaviourID(ability);
			AbilitySpecialBehaviourDescription abilitySpecialBehaviourDescription = new AbilitySpecialBehaviourDescription
			{
				behaviourID = specialBehaviourID,
				behaviourActivatorID = AbilitySpecialBehaviourActivatorID.None
			};
			if (specialBehaviourID != AbilitySpecialBehaviourID.None)
			{
				abilitySpecialBehaviourDescription.behaviourActivatorID = this.activatorsFactory.GetSpecialBehaviourActivatorID(ability);
			}
			return abilitySpecialBehaviourDescription;
		}

		// Token: 0x0400152E RID: 5422
		private readonly Dictionary<int, List<AbilitySpecialBehaviourGenerationOption>> generationOptions;

		// Token: 0x0400152F RID: 5423
		private readonly IAbilitySpecialBehavioursFactory behavioursFactory;

		// Token: 0x04001530 RID: 5424
		private readonly IAbilitySpecialBehavioursActivatorsFactory activatorsFactory;

		// Token: 0x04001531 RID: 5425
		private readonly List<AbilitySpecialBehaviourGenerationOption> optionsBuffer = new List<AbilitySpecialBehaviourGenerationOption>(8);

		// Token: 0x02000599 RID: 1433
		[Serializable]
		public class AbilityData
		{
			// Token: 0x04001D07 RID: 7431
			[EnumPopup]
			public TAbilityID abilityID;

			// Token: 0x04001D08 RID: 7432
			public AbilitySpecialBehavioursGenerationOptionsAssetBase behavioursGenerationOptions;
		}
	}
}
