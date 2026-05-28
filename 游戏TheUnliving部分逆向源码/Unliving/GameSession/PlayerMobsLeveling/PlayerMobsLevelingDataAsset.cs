using System;
using System.Collections.Generic;
using UnityEngine;
using Unliving.DataParsing;
using Unliving.MobsStats;

namespace Unliving.GameSession.PlayerMobsLeveling
{
	// Token: 0x020002AF RID: 687
	[CreateAssetMenu(fileName = "PlayerMobsLevelingData", menuName = "Game/Player Mobs Leveling Data")]
	public sealed class PlayerMobsLevelingDataAsset : CSVTableDataAssetBase<List<PlayerMobsLevelingDataAsset.MobLevelStatsModifiers>>
	{
		// Token: 0x0600180B RID: 6155 RVA: 0x0004BCB4 File Offset: 0x00049EB4
		protected override TextAsset GetTableAsset()
		{
			return this.dataTable;
		}

		// Token: 0x0600180C RID: 6156 RVA: 0x0004BCBC File Offset: 0x00049EBC
		protected override List<PlayerMobsLevelingDataAsset.MobLevelStatsModifiers> ParseTable(List<List<string>> table)
		{
			this.data.Clear();
			for (int i = 1; i < table.Count; i++)
			{
				List<string> list = table[i];
				PlayerMobsLevelingDataAsset.StatModifiersBuffer.Clear();
				for (int j = 1; j <= PlayerMobsLevelingDataAsset.ColumnStats.Length; j++)
				{
					TargetedMobStatModifier item;
					if (ParsingUtility.TryParseMobStatModifier(PlayerMobsLevelingDataAsset.ColumnStats[j - 1], list[j], out item, MobStatModifierType.ExtraModifier))
					{
						PlayerMobsLevelingDataAsset.StatModifiersBuffer.Add(item);
					}
				}
				this.data.Add(new PlayerMobsLevelingDataAsset.MobLevelStatsModifiers(PlayerMobsLevelingDataAsset.StatModifiersBuffer.ToArray()));
			}
			return this.data;
		}

		// Token: 0x0600180D RID: 6157 RVA: 0x0004BD4E File Offset: 0x00049F4E
		protected override void OnTableParsed(List<PlayerMobsLevelingDataAsset.MobLevelStatsModifiers> data)
		{
			this.data = data;
		}

		// Token: 0x0600180E RID: 6158 RVA: 0x0004BD58 File Offset: 0x00049F58
		public bool TryGetMobStatsModifiers(int mobLevel, out IReadOnlyList<TargetedMobStatModifier> modifiers)
		{
			mobLevel = Mathf.Clamp(mobLevel, 1, this.data.Count);
			if (mobLevel <= this.data.Count)
			{
				modifiers = this.data[mobLevel - 1].modifiers;
				return modifiers.Count != 0;
			}
			modifiers = Array.Empty<TargetedMobStatModifier>();
			return false;
		}

		// Token: 0x04000DA1 RID: 3489
		private static readonly List<TargetedMobStatModifier> StatModifiersBuffer = new List<TargetedMobStatModifier>(8);

		// Token: 0x04000DA2 RID: 3490
		private static readonly MobStatID[] ColumnStats = new MobStatID[]
		{
			MobStatID.MobDamage,
			MobStatID.MobHealth,
			MobStatID.MobActivationDamage,
			MobStatID.MobRottingSpeed,
			MobStatID.MobCrowdPassPriority,
			MobStatID.MobActivationReward
		};

		// Token: 0x04000DA3 RID: 3491
		public TextAsset dataTable;

		// Token: 0x04000DA4 RID: 3492
		[SerializeField]
		private List<PlayerMobsLevelingDataAsset.MobLevelStatsModifiers> data;

		// Token: 0x02000523 RID: 1315
		[Serializable]
		public struct MobLevelStatsModifiers
		{
			// Token: 0x06002643 RID: 9795 RVA: 0x00077E29 File Offset: 0x00076029
			public MobLevelStatsModifiers(TargetedMobStatModifier[] modifiers)
			{
				this.modifiers = modifiers;
			}

			// Token: 0x04001B3B RID: 6971
			public TargetedMobStatModifier[] modifiers;
		}
	}
}
