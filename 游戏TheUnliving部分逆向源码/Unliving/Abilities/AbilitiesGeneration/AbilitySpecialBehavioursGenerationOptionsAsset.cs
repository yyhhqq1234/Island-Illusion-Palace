using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unliving.DataParsing;

namespace Unliving.Abilities.AbilitiesGeneration
{
	// Token: 0x020003F0 RID: 1008
	[CreateAssetMenu(fileName = "AbilitySpecialBehavioursGenerationOptions", menuName = "Game/Abilities Generation/Ability Special Behaviours Generation Options Asset")]
	public sealed class AbilitySpecialBehavioursGenerationOptionsAsset : AbilitySpecialBehavioursGenerationOptionsAssetBase, ISerializationCallbackReceiver
	{
		// Token: 0x170006DC RID: 1756
		// (get) Token: 0x060021F0 RID: 8688 RVA: 0x00069C47 File Offset: 0x00067E47
		// (set) Token: 0x060021F1 RID: 8689 RVA: 0x00069C4F File Offset: 0x00067E4F
		public TextAsset SpecialBehavioursGenerationTable
		{
			get
			{
				return this.specialBehavioursGenerationTable;
			}
			set
			{
				this.specialBehavioursGenerationTable = value;
			}
		}

		// Token: 0x170006DD RID: 1757
		// (get) Token: 0x060021F2 RID: 8690 RVA: 0x00069C58 File Offset: 0x00067E58
		// (set) Token: 0x060021F3 RID: 8691 RVA: 0x00069C60 File Offset: 0x00067E60
		public List<AbilitySpecialBehaviourGenerationOption> SpecialBehavioursGenerationOptions
		{
			get
			{
				return this.specialBehavioursGenerationOptions;
			}
			set
			{
				this.specialBehavioursGenerationOptions = value;
			}
		}

		// Token: 0x060021F4 RID: 8692 RVA: 0x00069C6C File Offset: 0x00067E6C
		private void ParseOptionsTable(string table)
		{
			List<List<string>> list;
			if (!ParsingUtility.TryParseCsvTable(this.specialBehavioursGenerationTable, out list))
			{
				return;
			}
			List<string> list2 = list[0];
			if (list2.Count == 0)
			{
				return;
			}
			this.specialBehavioursGenerationOptions.Clear();
			for (int i = 1; i < list.Count; i++)
			{
				List<string> list3 = list[i];
				string text = list3[0];
				if (!string.IsNullOrWhiteSpace(text))
				{
					int behaviourID;
					if (!AbilitySpecialBehavioursGenerationOptionsAssetBase.TryGetBehaviourIDValue(text, out behaviourID))
					{
						this.<ParseOptionsTable>g__ShowWarning|9_0("Modifier ID " + text, i, 0);
					}
					else
					{
						for (int j = 1; j < list3.Count; j++)
						{
							string text2 = list2[j];
							if (!string.IsNullOrWhiteSpace(text2))
							{
								int behaviourActivatorID;
								if (!AbilitySpecialBehavioursGenerationOptionsAssetBase.TryGetBehaviourActivatorIDValue(text2, out behaviourActivatorID))
								{
									this.<ParseOptionsTable>g__ShowWarning|9_0("Activator ID " + text2, 0, j);
								}
								else
								{
									string text3 = list3[j];
									if (!string.IsNullOrWhiteSpace(text3))
									{
										float num;
										if (!float.TryParse(text3, NumberStyles.Float, CultureInfo.InvariantCulture, out num))
										{
											this.<ParseOptionsTable>g__ShowWarning|9_0("Option Weight " + text3, i, j);
										}
										else if (num > 0f)
										{
											AbilitySpecialBehaviourGenerationOption item = new AbilitySpecialBehaviourGenerationOption
											{
												behaviourID = (AbilitySpecialBehaviourID)behaviourID,
												behaviourActivatorID = (AbilitySpecialBehaviourActivatorID)behaviourActivatorID,
												Weight = num
											};
											this.specialBehavioursGenerationOptions.Add(item);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060021F5 RID: 8693 RVA: 0x00069DC4 File Offset: 0x00067FC4
		private void ParseOptionsTable()
		{
			if (this.specialBehavioursGenerationTable == null)
			{
				return;
			}
			this.ParseOptionsTable(this.specialBehavioursGenerationTable.text);
		}

		// Token: 0x060021F6 RID: 8694 RVA: 0x00069DE6 File Offset: 0x00067FE6
		public override ICollection<AbilitySpecialBehaviourGenerationOption> GetAbilityBehavioursDescriptions()
		{
			return this.specialBehavioursGenerationOptions;
		}

		// Token: 0x060021F7 RID: 8695 RVA: 0x00069DEE File Offset: 0x00067FEE
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		// Token: 0x060021F8 RID: 8696 RVA: 0x00069DF0 File Offset: 0x00067FF0
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x060021FA RID: 8698 RVA: 0x00069DFA File Offset: 0x00067FFA
		[CompilerGenerated]
		private void <ParseOptionsTable>g__ShowWarning|9_0(string message, int row, int column)
		{
			if (Debug.isDebugBuild)
			{
				Debug.LogWarning(string.Format("Unable to parse {0} at {1} ({2}, {3}).", new object[]
				{
					message,
					this,
					row + 1,
					column + 1
				}));
			}
		}

		// Token: 0x0400156A RID: 5482
		[SerializeField]
		private TextAsset specialBehavioursGenerationTable;

		// Token: 0x0400156B RID: 5483
		[SerializeField]
		private List<AbilitySpecialBehaviourGenerationOption> specialBehavioursGenerationOptions;

		// Token: 0x0400156C RID: 5484
		[SerializeField]
		[HideInInspector]
		private string lastTableAssetHash;
	}
}
