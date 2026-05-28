using System;
using Game.Buffs;
using Game.LevelGeneration;
using Unliving.Currencies;
using Unliving.LevelGeneration;
using Unliving.MobsStats;

namespace Unliving.GameSession.DifficultyAdvancing
{
	// Token: 0x020002C2 RID: 706
	[Serializable]
	public sealed class DifficultyLevelData
	{
		// Token: 0x0600189D RID: 6301 RVA: 0x0004D340 File Offset: 0x0004B540
		public bool TryGetLocationMobStatModifiers(GameLocation.TypeID locationID, out TargetedMobStatModifier[] statModifiers)
		{
			for (int i = 0; i < this.locationEnemyMobStatModifiers.Length; i++)
			{
				if (this.locationEnemyMobStatModifiers[i].locationID == locationID)
				{
					statModifiers = this.locationEnemyMobStatModifiers[i].statModifiers;
					return statModifiers != null && statModifiers.Length != 0;
				}
			}
			statModifiers = Array.Empty<TargetedMobStatModifier>();
			return false;
		}

		// Token: 0x0600189E RID: 6302 RVA: 0x0004D39C File Offset: 0x0004B59C
		public bool TryGetCurrencyMultiplier(CurrencyID currencyID, out float multiplier)
		{
			for (int i = 0; i < this.playerCurrencyMultipliers.Length; i++)
			{
				if (this.playerCurrencyMultipliers[i].currencyID == currencyID)
				{
					multiplier = this.playerCurrencyMultipliers[i].value;
					return true;
				}
			}
			multiplier = 1f;
			return false;
		}

		// Token: 0x0600189F RID: 6303 RVA: 0x0004D3F0 File Offset: 0x0004B5F0
		public bool TryGetCompletedLevelRewards(GameLocation.TypeID locationID, out DifficultyLevelData.CurrencyValue[] rewards)
		{
			for (int i = 0; i < this.completedLevelPlayerReward.Length; i++)
			{
				if (this.completedLevelPlayerReward[i].locationID == locationID)
				{
					rewards = this.completedLevelPlayerReward[i].rewards;
					return rewards != null && rewards.Length != 0;
				}
			}
			rewards = Array.Empty<DifficultyLevelData.CurrencyValue>();
			return false;
		}

		// Token: 0x04000DD5 RID: 3541
		public DifficultyLevelData.LocationMobStatModifiers[] locationEnemyMobStatModifiers;

		// Token: 0x04000DD6 RID: 3542
		public DifficultyLevelData.CurrencyValue[] playerCurrencyMultipliers;

		// Token: 0x04000DD7 RID: 3543
		public DifficultyLevelData.LocationRewardData[] completedLevelPlayerReward;

		// Token: 0x04000DD8 RID: 3544
		public int[] enemyMobBuffsCount;

		// Token: 0x04000DD9 RID: 3545
		public float enemyMobsPowerCharacteristic = 100f;

		// Token: 0x02000528 RID: 1320
		[Serializable]
		public struct LocationMobStatModifiers
		{
			// Token: 0x04001B4C RID: 6988
			public GameLocation.TypeID locationID;

			// Token: 0x04001B4D RID: 6989
			public TargetedMobStatModifier[] statModifiers;
		}

		// Token: 0x02000529 RID: 1321
		[Serializable]
		public struct CurrencyValue
		{
			// Token: 0x06002657 RID: 9815 RVA: 0x00078181 File Offset: 0x00076381
			public CurrencyValue(CurrencyID currencyID, float value)
			{
				this.currencyID = currencyID;
				this.value = value;
			}

			// Token: 0x04001B4E RID: 6990
			public CurrencyID currencyID;

			// Token: 0x04001B4F RID: 6991
			public float value;
		}

		// Token: 0x0200052A RID: 1322
		[Serializable]
		public struct LocationRewardData
		{
			// Token: 0x06002658 RID: 9816 RVA: 0x00078191 File Offset: 0x00076391
			public LocationRewardData(GameLocation.TypeID locationID, DifficultyLevelData.CurrencyValue[] rewards)
			{
				this.locationID = locationID;
				this.rewards = rewards;
			}

			// Token: 0x06002659 RID: 9817 RVA: 0x000781A1 File Offset: 0x000763A1
			public LocationRewardData(int locationID, DifficultyLevelData.CurrencyValue[] rewards)
			{
				this.locationID = (GameLocation.TypeID)locationID;
				this.rewards = rewards;
			}

			// Token: 0x04001B50 RID: 6992
			public GameLocation.TypeID locationID;

			// Token: 0x04001B51 RID: 6993
			public DifficultyLevelData.CurrencyValue[] rewards;
		}

		// Token: 0x0200052B RID: 1323
		[Serializable]
		public struct MobGainData
		{
			// Token: 0x0600265A RID: 9818 RVA: 0x000781B1 File Offset: 0x000763B1
			public bool IsAllowedLocation(GameLocation.TypeID locationID)
			{
				return Array.IndexOf<GameLocation.TypeID>(this.allowedLocations, locationID) != -1;
			}

			// Token: 0x0600265B RID: 9819 RVA: 0x000781C5 File Offset: 0x000763C5
			public bool IsAllowedChunk(ILocationChunk chunk)
			{
				return !chunk.HasType(LocationChunk.TypeID.BossChunk) || this.affectBossChunkMobs;
			}

			// Token: 0x04001B52 RID: 6994
			public GameLocation.TypeID[] allowedLocations;

			// Token: 0x04001B53 RID: 6995
			public bool affectBossChunkMobs;

			// Token: 0x04001B54 RID: 6996
			public BuffsGeneratorBuilderAsset.Reference[] buffGeneratorAssets;
		}
	}
}
