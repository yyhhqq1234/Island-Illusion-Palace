using System;
using GraphProcessor;
using Unliving.Abilities.AbilitiesGeneration;

namespace Unliving.Purchasing
{
	// Token: 0x02000105 RID: 261
	[NodeMenuItem("Purchase Manager/Purchasable Ability Special Behaviour Node", null)]
	[Serializable]
	public class PurchasableAbilitySpecialBehaviourNode : PurchasableItemNodeBase<PurchasableAbilitySpecialBehaviour, AbilitySpecialBehaviourID>
	{
		// Token: 0x17000100 RID: 256
		// (get) Token: 0x06000652 RID: 1618 RVA: 0x0001520B File Offset: 0x0001340B
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000653 RID: 1619 RVA: 0x00015210 File Offset: 0x00013410
		public override void SetID(object id)
		{
			if (id is AbilitySpecialBehaviourID)
			{
				AbilitySpecialBehaviourID purchaseItem = (AbilitySpecialBehaviourID)id;
				AbilitySpecialBehaviourID abilitySpecialBehaviourID = this.specialBehaviourID;
				this.specialBehaviourID = purchaseItem;
				if (this.data == null)
				{
					this.data = new PurchasableAbilitySpecialBehaviour(purchaseItem);
				}
				else
				{
					this.data.PurchaseItem = purchaseItem;
				}
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
				graph2.OnNodeIDChanged(this, abilitySpecialBehaviourID);
			}
		}

		// Token: 0x06000654 RID: 1620 RVA: 0x00015285 File Offset: 0x00013485
		public override object GetObjectID()
		{
			return this.specialBehaviourID;
		}

		// Token: 0x06000655 RID: 1621 RVA: 0x00015292 File Offset: 0x00013492
		public override bool HasDefaultID()
		{
			return this.specialBehaviourID == (AbilitySpecialBehaviourID)this.GetDefaultID();
		}

		// Token: 0x06000656 RID: 1622 RVA: 0x000152A7 File Offset: 0x000134A7
		public override object GetDefaultID()
		{
			return AbilitySpecialBehaviourID.None;
		}

		// Token: 0x06000657 RID: 1623 RVA: 0x000152AF File Offset: 0x000134AF
		public override void SetDefaultID()
		{
			this.SetID(this.GetDefaultID());
		}

		// Token: 0x040003FE RID: 1022
		public AbilitySpecialBehaviourID specialBehaviourID;
	}
}
