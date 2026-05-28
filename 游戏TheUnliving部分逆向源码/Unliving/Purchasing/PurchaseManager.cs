using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Editor;
using Common.RestorableState;
using Common.UnityExtensions;
using Game.Core;
using GraphProcessor;
using UnityEngine;
using Unliving.Currencies;
using Unliving.Player.Upgrades;
using Unliving.PlayerProfileManagement;

namespace Unliving.Purchasing
{
	// Token: 0x020000F6 RID: 246
	[CreateAssetMenu(fileName = "PurchaseManager", menuName = "Game/Purchase System/Purchase Manager")]
	public sealed class PurchaseManager : GlobalManagerBase
	{
		// Token: 0x170000E8 RID: 232
		// (get) Token: 0x060005E5 RID: 1509 RVA: 0x000146A2 File Offset: 0x000128A2
		// (set) Token: 0x060005E6 RID: 1510 RVA: 0x000146BA File Offset: 0x000128BA
		public bool DebugUnlockAll
		{
			get
			{
				return GameApplicationSettings.IsDebugBuild && Application.isPlaying && this.debugUnlockAll;
			}
			set
			{
				this.debugUnlockAll = value;
			}
		}

		// Token: 0x14000035 RID: 53
		// (add) Token: 0x060005E7 RID: 1511 RVA: 0x000146C4 File Offset: 0x000128C4
		// (remove) Token: 0x060005E8 RID: 1512 RVA: 0x000146FC File Offset: 0x000128FC
		public event Action<IPurchasable> ItemPurchased;

		// Token: 0x060005E9 RID: 1513 RVA: 0x00014734 File Offset: 0x00012934
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
		}

		// Token: 0x060005EA RID: 1514 RVA: 0x00014788 File Offset: 0x00012988
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			this.purchasables.Clear();
			this.priceModifiers.Clear();
			List<BaseNode> nodes = this.purchaseManagerGraph.nodes;
			for (int i = 0; i < nodes.Count; i++)
			{
				IPurchasableItemNode purchasableItemNode = nodes[i] as IPurchasableItemNode;
				if (purchasableItemNode != null && !purchasableItemNode.HasDefaultID())
				{
					this.<OnPlayerProfileLoaded>g__AddPurchasable|16_0(purchasableItemNode.Data.Clone());
				}
			}
			for (int j = 0; j < this.externalSources.Length; j++)
			{
				IPurchasablesExternalSource purchasablesExternalSource = this.externalSources[j] as IPurchasablesExternalSource;
				if (purchasablesExternalSource != null)
				{
					IList<IPurchasable> list = purchasablesExternalSource.GetPurchasables();
					for (int k = 0; k < list.Count; k++)
					{
						this.<OnPlayerProfileLoaded>g__AddPurchasable|16_0(list[k].Clone());
					}
				}
			}
			this.profileManager.LoadPlayerPurchasements(this);
		}

		// Token: 0x060005EB RID: 1515 RVA: 0x00014856 File Offset: 0x00012A56
		public void AddCurrencyArgsModifier(PurchasableCurrencyArgsModifierBase modifier)
		{
			if (this.priceModifiers.Contains(modifier))
			{
				return;
			}
			this.priceModifiers.Add(modifier);
		}

		// Token: 0x060005EC RID: 1516 RVA: 0x00014873 File Offset: 0x00012A73
		public void RemoveCurrencyArgsModifier(PurchasableCurrencyArgsModifierBase modifier)
		{
			if (this.priceModifiers.Contains(modifier))
			{
				this.priceModifiers.Remove(modifier);
			}
		}

		// Token: 0x060005ED RID: 1517 RVA: 0x00014890 File Offset: 0x00012A90
		public void PurchaseItem(IPurchasable item)
		{
			this.PurchaseItem(item.ObjectID);
		}

		// Token: 0x060005EE RID: 1518 RVA: 0x000148A0 File Offset: 0x00012AA0
		public void PurchaseItem(object itemID)
		{
			IPurchasable obj;
			if (this.TryGetPurchasable(itemID, out obj))
			{
				Action<IPurchasable> itemPurchased = this.ItemPurchased;
				if (itemPurchased == null)
				{
					return;
				}
				itemPurchased(obj);
			}
		}

		// Token: 0x060005EF RID: 1519 RVA: 0x000148CC File Offset: 0x00012ACC
		public CurrencyOperationArgs[] GetUpgradesPrices(Type type)
		{
			for (int i = 0; i < this.upgradesPrices.Length; i++)
			{
				UpgradesPricesBase upgradesPricesBase = this.upgradesPrices[i];
				if (object.Equals(type, upgradesPricesBase.GetPurchasableType()))
				{
					return upgradesPricesBase.prices;
				}
			}
			return new CurrencyOperationArgs[0];
		}

		// Token: 0x060005F0 RID: 1520 RVA: 0x00014910 File Offset: 0x00012B10
		public List<T> GetPurchasablesOfType<T>(bool onlyPurchased = false)
		{
			List<T> list = new List<T>();
			foreach (IPurchasable purchasable in this.purchasables)
			{
				if ((object.Equals(purchasable.GetType(), typeof(T)) || typeof(T).IsAssignableFrom(purchasable.GetType())) && (!onlyPurchased || purchasable.Purchased))
				{
					list.Add((T)((object)purchasable));
				}
			}
			return list;
		}

		// Token: 0x060005F1 RID: 1521 RVA: 0x000149A8 File Offset: 0x00012BA8
		public IPlayerUpgrade GetRerollCurrencyUpgrade()
		{
			RerollCurrencyPlayerUpgrade rerollCurrencyPlayerUpgrade = this.rerollCurrencyUpgrade;
			if (rerollCurrencyPlayerUpgrade == null)
			{
				return null;
			}
			return rerollCurrencyPlayerUpgrade.Clone();
		}

		// Token: 0x060005F2 RID: 1522 RVA: 0x000149BC File Offset: 0x00012BBC
		public bool TryGetPurchasable(object itemID, out IPurchasable item)
		{
			if (Application.isPlaying)
			{
				for (int i = 0; i < this.purchasables.Count; i++)
				{
					IPurchasable purchasable = this.purchasables[i];
					if (object.Equals(itemID, purchasable.ObjectID))
					{
						item = purchasable;
						return true;
					}
				}
				item = null;
				return false;
			}
			if (this.purchaseManagerGraph.IsNull())
			{
				item = null;
				return false;
			}
			return this.purchaseManagerGraph.TryGetPurchasable(itemID, out item);
		}

		// Token: 0x060005F3 RID: 1523 RVA: 0x00014A2C File Offset: 0x00012C2C
		public IReadOnlyCollection<IPurchasable> GetAllPurchasables()
		{
			if (Application.isPlaying)
			{
				return this.purchasables;
			}
			if (this.purchaseManagerGraph == null)
			{
				return null;
			}
			List<BaseNode> nodes = this.purchaseManagerGraph.nodes;
			IPurchasable[] array = new IPurchasable[nodes.Count];
			for (int i = 0; i < nodes.Count; i++)
			{
				IPurchasableItemNode purchasableItemNode = nodes[i] as IPurchasableItemNode;
				if (purchasableItemNode != null && !purchasableItemNode.HasDefaultID())
				{
					array[i] = purchasableItemNode.Data;
				}
			}
			return array;
		}

		// Token: 0x060005F4 RID: 1524 RVA: 0x00014AA4 File Offset: 0x00012CA4
		public bool TryGetPriceModifiers(IPurchasable purchasable, out List<PurchasableCurrencyArgsModifierBase> modifiers)
		{
			modifiers = new List<PurchasableCurrencyArgsModifierBase>();
			if (!Application.isPlaying)
			{
				return false;
			}
			for (int i = 0; i < this.priceModifiers.Count; i++)
			{
				PurchasableCurrencyArgsModifierBase purchasableCurrencyArgsModifierBase = this.priceModifiers[i];
				if (purchasableCurrencyArgsModifierBase.IsMatch(purchasable))
				{
					modifiers.Add(purchasableCurrencyArgsModifierBase);
				}
			}
			return modifiers.Count > 0;
		}

		// Token: 0x060005F5 RID: 1525 RVA: 0x00014AFF File Offset: 0x00012CFF
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
		}

		// Token: 0x060005F7 RID: 1527 RVA: 0x00014B4A File Offset: 0x00012D4A
		[CompilerGenerated]
		private void <OnPlayerProfileLoaded>g__AddPurchasable|16_0(IPurchasable purchasable)
		{
			if (!this.purchasables.Contains(purchasable))
			{
				purchasable.SetGameData(base.CurrentGame);
				this.purchasables.Add(purchasable);
			}
		}

		// Token: 0x040003E7 RID: 999
		[SerializeField]
		private bool debugUnlockAll;

		// Token: 0x040003E8 RID: 1000
		[SerializeField]
		private RerollCurrencyPlayerUpgrade rerollCurrencyUpgrade;

		// Token: 0x040003E9 RID: 1001
		[SerializeField]
		private ScriptableObject[] externalSources;

		// Token: 0x040003EA RID: 1002
		[SerializeReference]
		[ManagedObjectField(typeof(UpgradesPricesBase))]
		private UpgradesPricesBase[] upgradesPrices;

		// Token: 0x040003EB RID: 1003
		public PurchaseManagerGraph purchaseManagerGraph;

		// Token: 0x040003EC RID: 1004
		private readonly List<IPurchasable> purchasables = new List<IPurchasable>();

		// Token: 0x040003ED RID: 1005
		private readonly List<PurchasableCurrencyArgsModifierBase> priceModifiers = new List<PurchasableCurrencyArgsModifierBase>();

		// Token: 0x040003EE RID: 1006
		private PlayerProfileManager profileManager;

		// Token: 0x0200042F RID: 1071
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PurchaseManager>, ICloneable<PurchaseManager.RestorableState>
		{
			// Token: 0x060022AB RID: 8875 RVA: 0x0006C08E File Offset: 0x0006A28E
			public RestorableState() : base(null)
			{
			}

			// Token: 0x060022AC RID: 8876 RVA: 0x0006C0A2 File Offset: 0x0006A2A2
			public PurchaseManager.RestorableState Clone()
			{
				return new PurchaseManager.RestorableState
				{
					purchasedItemsData = this.purchasedItemsData.ToList<IPurchasableData>()
				};
			}

			// Token: 0x060022AD RID: 8877 RVA: 0x0006C0BC File Offset: 0x0006A2BC
			public override void Store(PurchaseManager purchaseManager)
			{
				if (purchaseManager == null)
				{
					return;
				}
				this.purchasedItemsData.Clear();
				foreach (IPurchasable purchasable in purchaseManager.GetAllPurchasables())
				{
					if (purchasable.Purchased)
					{
						IPurchasableData purchasableData = purchasable.GetPurchasableData();
						this.purchasedItemsData.Add(purchasableData);
					}
				}
			}

			// Token: 0x060022AE RID: 8878 RVA: 0x0006C134 File Offset: 0x0006A334
			public override void Restore(PurchaseManager purchaseManager, object args = null)
			{
				for (int i = 0; i < this.purchasedItemsData.Count; i++)
				{
					IPurchasableData purchasableData = this.purchasedItemsData[i];
					IPurchasable purchasable;
					if (purchaseManager.TryGetPurchasable(purchasableData.ItemID, out purchasable))
					{
						purchasable.ChangePurchaseState(true);
						purchasable.SetPurchasableData(purchasableData);
					}
				}
			}

			// Token: 0x04001631 RID: 5681
			public List<IPurchasableData> purchasedItemsData = new List<IPurchasableData>();
		}
	}
}
