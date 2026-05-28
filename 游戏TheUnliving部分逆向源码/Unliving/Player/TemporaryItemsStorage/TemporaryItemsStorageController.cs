using System;
using System.Collections.Generic;
using Common.CollectionsExtensions;
using Game.Abilities;
using Game.Buffs;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Player.TemporaryItemsStorage
{
	// Token: 0x02000176 RID: 374
	public sealed class TemporaryItemsStorageController : BaseGameMob.ControllerBase<BaseGameMob>
	{
		// Token: 0x170001BC RID: 444
		// (get) Token: 0x06000A62 RID: 2658 RVA: 0x00022587 File Offset: 0x00020787
		public IReadOnlyList<TemporaryItemBase> Items
		{
			get
			{
				return this.items;
			}
		}

		// Token: 0x06000A63 RID: 2659 RVA: 0x00022590 File Offset: 0x00020790
		private bool HandleItem(TemporaryItemBase item, bool add)
		{
			if (!item.CanBeStored(this))
			{
				return false;
			}
			object content = item.Content;
			BaseAbility baseAbility = content as BaseAbility;
			if (baseAbility == null)
			{
				IBuff buff = content as IBuff;
				if (buff == null)
				{
					return false;
				}
				if (!add)
				{
					return buff.Complete();
				}
				if (buff.IsConstant)
				{
					IBuffsController buffsController = this.ControllerOwner.BuffsController;
					return buffsController != null && buffsController.AddBuff(buff);
				}
				return false;
			}
			else
			{
				if (add)
				{
					GameAbilitiesController abilitiesController = this.ControllerOwner.AbilitiesController;
					return abilitiesController != null && abilitiesController.AddAbility(baseAbility);
				}
				GameAbilitiesController abilitiesController2 = this.ControllerOwner.AbilitiesController;
				return abilitiesController2 == null || abilitiesController2.RemoveAbility(baseAbility);
			}
		}

		// Token: 0x06000A64 RID: 2660 RVA: 0x00022628 File Offset: 0x00020828
		private bool RemoveItem(int itemIndex)
		{
			if (itemIndex != -1)
			{
				TemporaryItemBase temporaryItemBase = this.items[itemIndex];
				if (this.HandleItem(temporaryItemBase, false))
				{
					temporaryItemBase.OnDiscarded(this);
					this.items.RemoveBySwap(itemIndex);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A65 RID: 2661 RVA: 0x00022666 File Offset: 0x00020866
		public TemporaryItemsStorageController(BaseGameMob targetMob) : base(targetMob)
		{
		}

		// Token: 0x06000A66 RID: 2662 RVA: 0x0002267B File Offset: 0x0002087B
		public bool AddItem(TemporaryItemBase item)
		{
			if (this.HandleItem(item, true))
			{
				this.items.Add(item);
				item.OnStored(this);
				return true;
			}
			return false;
		}

		// Token: 0x06000A67 RID: 2663 RVA: 0x0002269D File Offset: 0x0002089D
		public bool RemoveItem(TemporaryItemBase item)
		{
			return this.RemoveItem(this.items.IndexOf(item));
		}

		// Token: 0x06000A68 RID: 2664 RVA: 0x000226B4 File Offset: 0x000208B4
		public void Update()
		{
			for (int i = 0; i < this.items.Count; i++)
			{
				if (this.items[i].IsExpired)
				{
					this.RemoveItem(i);
				}
			}
		}

		// Token: 0x04000611 RID: 1553
		private readonly List<TemporaryItemBase> items = new List<TemporaryItemBase>(8);
	}
}
