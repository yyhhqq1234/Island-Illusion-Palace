using System;
using Common.Editor;
using GraphProcessor;
using Unliving.Abilities;

namespace Unliving.Purchasing
{
	// Token: 0x02000103 RID: 259
	[NodeMenuItem("Purchase Manager/Purchasable Ability Activated Container Node", null)]
	[Serializable]
	public class PurchasableAbilityActivatedContainerNode : PurchasableItemNodeBase<PurchasableItemAbilityActivatedContainer, AbilityID>
	{
		// Token: 0x170000FE RID: 254
		// (get) Token: 0x06000644 RID: 1604 RVA: 0x0001506B File Offset: 0x0001326B
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000645 RID: 1605 RVA: 0x00015070 File Offset: 0x00013270
		public override void SetID(object id)
		{
			if (id is AbilityID)
			{
				AbilityID purchaseItem = (AbilityID)id;
				AbilityID abilityID = this.ability;
				this.ability = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchasableItemAbilityActivatedContainer();
				}
				this.data.PurchaseItem = purchaseItem;
				BaseGraph graph = this.graph;
				if (graph != null)
				{
					graph.NotifyNodeChanged(this);
				}
				PurchaseManagerGraph graph2 = base.Graph;
				if (graph2 == null)
				{
					return;
				}
				graph2.OnNodeIDChanged(this, abilityID);
			}
		}

		// Token: 0x06000646 RID: 1606 RVA: 0x000150E2 File Offset: 0x000132E2
		public override object GetObjectID()
		{
			return this.ability;
		}

		// Token: 0x06000647 RID: 1607 RVA: 0x000150EF File Offset: 0x000132EF
		public override bool HasDefaultID()
		{
			return this.ability == (AbilityID)this.GetDefaultID();
		}

		// Token: 0x06000648 RID: 1608 RVA: 0x00015104 File Offset: 0x00013304
		public override object GetDefaultID()
		{
			return AbilityID.None;
		}

		// Token: 0x06000649 RID: 1609 RVA: 0x0001510C File Offset: 0x0001330C
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
			this.ability = (AbilityID)defaultID;
		}

		// Token: 0x040003FC RID: 1020
		[EnumPopup]
		public AbilityID ability;
	}
}
