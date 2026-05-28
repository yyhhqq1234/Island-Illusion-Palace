using System;
using Game.Abilities;
using Game.Factories;
using GraphProcessor;
using Unliving.Abilities;

namespace Unliving.Purchasing
{
	// Token: 0x02000104 RID: 260
	[NodeMenuItem("Purchase Manager/Purchasable Ability Node", null)]
	[Serializable]
	public class PurchasableAbilityNode : PurchasableItemNodeBase<PurchaseItemAbility, AbilityID>
	{
		// Token: 0x170000FF RID: 255
		// (get) Token: 0x0600064B RID: 1611 RVA: 0x0001513B File Offset: 0x0001333B
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600064C RID: 1612 RVA: 0x00015140 File Offset: 0x00013340
		public override void SetID(object id)
		{
			if (id is AbilityID)
			{
				AbilityID purchaseItem = (AbilityID)id;
				AbilityID abilityID = this.ability;
				this.ability = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchaseItemAbility();
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

		// Token: 0x0600064D RID: 1613 RVA: 0x000151B2 File Offset: 0x000133B2
		public override object GetObjectID()
		{
			return this.ability;
		}

		// Token: 0x0600064E RID: 1614 RVA: 0x000151BF File Offset: 0x000133BF
		public override bool HasDefaultID()
		{
			return this.ability == (AbilityID)this.GetDefaultID();
		}

		// Token: 0x0600064F RID: 1615 RVA: 0x000151D4 File Offset: 0x000133D4
		public override object GetDefaultID()
		{
			return AbilityID.None;
		}

		// Token: 0x06000650 RID: 1616 RVA: 0x000151DC File Offset: 0x000133DC
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
			this.ability = (AbilityID)defaultID;
		}

		// Token: 0x040003FD RID: 1021
		[ObjectFactoryIDPopup(typeof(BaseAbility))]
		public AbilityID ability;
	}
}
