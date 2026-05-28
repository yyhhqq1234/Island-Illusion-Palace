using System;
using System.Collections.Generic;
using Common.Editor;
using UnityEngine;
using Unliving.DataParsing;
using Unliving.MobsStats;
using Unliving.Purchasing;

namespace Unliving.PermanentUpgrades
{
	// Token: 0x020001A1 RID: 417
	[CreateAssetMenu(fileName = "PermanentUpgradesData", menuName = "Permanent Upgrades/Permanent Upgrades Data")]
	public sealed class PermanentUpgradesDataAsset : ScriptableObject, ISerializationCallbackReceiver, IPurchasablesExternalSource
	{
		// Token: 0x17000216 RID: 534
		// (get) Token: 0x06000BE9 RID: 3049 RVA: 0x00025AA1 File Offset: 0x00023CA1
		public IReadOnlyList<PermanentUpgradesDataAsset.UpgradesCollectionData> UpgradesCollections
		{
			get
			{
				return this.upgradesCollections;
			}
		}

		// Token: 0x06000BEA RID: 3050 RVA: 0x00025AAC File Offset: 0x00023CAC
		[ContextMenu("Load Data From Table")]
		private void LoadDataFromTable()
		{
			this.upgradesCollections.Clear();
			List<List<string>> list;
			if (!ParsingUtility.TryParseCsvTable(this.upgradesTable, out list))
			{
				return;
			}
			for (int i = 1; i < list.Count; i++)
			{
				List<string> list2 = list[i];
				int j = 0;
				int id;
				if (!int.TryParse(list2[j++], out id))
				{
					id = -1;
				}
				int cost;
				if (!int.TryParse(list2[j++], out cost))
				{
					cost = -1;
				}
				PermanentUpgradesDataAsset.UpgradesBuffer.Clear();
				while (j < list2.Count - 1)
				{
					TargetedMobStatModifier targetedMobStatModifier;
					if (ParsingUtility.TryParseMobStatModifier(list2[j], list2[j + 1], out targetedMobStatModifier, MobStatModifierType.None))
					{
						PermanentUpgradesDataAsset.UpgradesBuffer.Add(new PermanentUpgradesDataAsset.UpgradeData
						{
							statID = targetedMobStatModifier.targetStat,
							statModifier = targetedMobStatModifier
						});
					}
					j += 2;
				}
				if (PermanentUpgradesDataAsset.UpgradesBuffer.Count != 0)
				{
					PermanentUpgradesDataAsset.UpgradesCollectionData item = new PermanentUpgradesDataAsset.UpgradesCollectionData
					{
						id = id,
						cost = cost,
						upgrades = PermanentUpgradesDataAsset.UpgradesBuffer.ToArray()
					};
					this.upgradesCollections.Add(item);
				}
			}
		}

		// Token: 0x06000BEB RID: 3051 RVA: 0x00025BC8 File Offset: 0x00023DC8
		public IList<IPurchasable> GetPurchasables()
		{
			List<IPurchasable> list = new List<IPurchasable>();
			IPurchasable purchasable = null;
			for (int i = 0; i < this.upgradesCollections.Count; i++)
			{
				PurchasablePermanentUpgradeCollection purchasablePermanentUpgradeCollection = new PurchasablePermanentUpgradeCollection(this.upgradesCollections[i]);
				if (purchasable != null)
				{
					purchasablePermanentUpgradeCollection.Parents = new List<IPurchasable>
					{
						purchasable
					};
				}
				purchasable = purchasablePermanentUpgradeCollection;
				list.Add(purchasablePermanentUpgradeCollection);
			}
			return list;
		}

		// Token: 0x06000BEC RID: 3052 RVA: 0x00025C24 File Offset: 0x00023E24
		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		// Token: 0x06000BED RID: 3053 RVA: 0x00025C26 File Offset: 0x00023E26
		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
		}

		// Token: 0x0400069F RID: 1695
		private static readonly List<PermanentUpgradesDataAsset.UpgradeData> UpgradesBuffer = new List<PermanentUpgradesDataAsset.UpgradeData>(8);

		// Token: 0x040006A0 RID: 1696
		public TextAsset upgradesTable;

		// Token: 0x040006A1 RID: 1697
		[SerializeField]
		private List<PermanentUpgradesDataAsset.UpgradesCollectionData> upgradesCollections;

		// Token: 0x040006A2 RID: 1698
		[SerializeField]
		[HideInInspector]
		private string lastTableAssetHash;

		// Token: 0x02000479 RID: 1145
		[Serializable]
		public struct UpgradeData
		{
			// Token: 0x0400177B RID: 6011
			[EnumPopup]
			public MobStatID statID;

			// Token: 0x0400177C RID: 6012
			public MobStatModifier statModifier;
		}

		// Token: 0x0200047A RID: 1146
		[Serializable]
		public sealed class UpgradesCollectionData
		{
			// Token: 0x0400177D RID: 6013
			public int id;

			// Token: 0x0400177E RID: 6014
			public int cost;

			// Token: 0x0400177F RID: 6015
			public PermanentUpgradesDataAsset.UpgradeData[] upgrades;
		}
	}
}
