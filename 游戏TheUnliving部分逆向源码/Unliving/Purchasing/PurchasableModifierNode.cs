using System;
using Game.Factories;
using GraphProcessor;
using Unliving.Mobs.ActivationModifiers;

namespace Unliving.Purchasing
{
	// Token: 0x0200010A RID: 266
	[NodeMenuItem("Purchase Manager/Purchasable Modifier Node", null)]
	[Serializable]
	public class PurchasableModifierNode : PurchasableItemNodeBase<PurchasableActivationModifier, MobActivationModifierID>
	{
		// Token: 0x1700010B RID: 267
		// (get) Token: 0x0600067C RID: 1660 RVA: 0x000155A3 File Offset: 0x000137A3
		public override bool HomespaceSpawnerRequired
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600067D RID: 1661 RVA: 0x000155A6 File Offset: 0x000137A6
		public override object GetObjectID()
		{
			return this.modifier;
		}

		// Token: 0x0600067E RID: 1662 RVA: 0x000155B3 File Offset: 0x000137B3
		public override bool HasDefaultID()
		{
			return this.modifier == (MobActivationModifierID)this.GetDefaultID();
		}

		// Token: 0x0600067F RID: 1663 RVA: 0x000155C8 File Offset: 0x000137C8
		public override void SetID(object id)
		{
			if (id is MobActivationModifierID)
			{
				MobActivationModifierID mobActivationModifierID = (MobActivationModifierID)id;
				MobActivationModifierID mobActivationModifierID2 = this.modifier;
				this.modifier = mobActivationModifierID;
				if (this.data == null)
				{
					this.data = new PurchasableActivationModifier(mobActivationModifierID);
				}
				else
				{
					this.data.PurchaseItem = mobActivationModifierID;
				}
				PurchaseManagerGraph graph = base.Graph;
				if (graph == null)
				{
					return;
				}
				graph.OnNodeIDChanged(this, mobActivationModifierID2);
			}
		}

		// Token: 0x06000680 RID: 1664 RVA: 0x0001562B File Offset: 0x0001382B
		public override object GetDefaultID()
		{
			return MobActivationModifierID.None;
		}

		// Token: 0x06000681 RID: 1665 RVA: 0x00015634 File Offset: 0x00013834
		public override void SetDefaultID()
		{
			object defaultID = this.GetDefaultID();
			this.SetID(defaultID);
			this.modifier = (MobActivationModifierID)defaultID;
		}

		// Token: 0x04000404 RID: 1028
		[ObjectFactoryIDPopup(typeof(MobActivationAbilityModifier))]
		public MobActivationModifierID modifier;
	}
}
