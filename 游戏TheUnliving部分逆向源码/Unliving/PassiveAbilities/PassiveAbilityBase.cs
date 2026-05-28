using System;
using Common;
using Game.PassiveAbilities;
using UnityEngine;

namespace Unliving.PassiveAbilities
{
	// Token: 0x020001A9 RID: 425
	public abstract class PassiveAbilityBase : ScriptableObject, IPassiveAbility, IDestroyable
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x06000C1D RID: 3101 RVA: 0x000261B5 File Offset: 0x000243B5
		// (set) Token: 0x06000C1E RID: 3102 RVA: 0x000261BD File Offset: 0x000243BD
		public PassiveAbilityID ID { get; set; }

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x06000C1F RID: 3103 RVA: 0x000261C6 File Offset: 0x000243C6
		// (set) Token: 0x06000C20 RID: 3104 RVA: 0x000261CE File Offset: 0x000243CE
		int IPassiveAbility.NumericID
		{
			get
			{
				return (int)this.ID;
			}
			set
			{
				this.ID = (PassiveAbilityID)value;
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x06000C21 RID: 3105 RVA: 0x000261D7 File Offset: 0x000243D7
		// (set) Token: 0x06000C22 RID: 3106 RVA: 0x000261DF File Offset: 0x000243DF
		public object AbilityOwner { get; set; }

		// Token: 0x17000229 RID: 553
		// (get) Token: 0x06000C23 RID: 3107 RVA: 0x000261E8 File Offset: 0x000243E8
		// (set) Token: 0x06000C24 RID: 3108 RVA: 0x000261F0 File Offset: 0x000243F0
		public bool IsDestroyed { get; private set; }

		// Token: 0x14000084 RID: 132
		// (add) Token: 0x06000C25 RID: 3109 RVA: 0x000261FC File Offset: 0x000243FC
		// (remove) Token: 0x06000C26 RID: 3110 RVA: 0x00026234 File Offset: 0x00024434
		public event Action<object> Destroyed;

		// Token: 0x06000C27 RID: 3111 RVA: 0x00026269 File Offset: 0x00024469
		public void Destroy()
		{
			this.IsDestroyed = true;
			UnityEngine.Object.Destroy(this);
		}

		// Token: 0x06000C28 RID: 3112 RVA: 0x00026278 File Offset: 0x00024478
		public virtual void OnAddedToController(IPassiveAbilitiesController controller)
		{
		}

		// Token: 0x06000C29 RID: 3113 RVA: 0x0002627A File Offset: 0x0002447A
		public virtual void OnRemovedFromController(IPassiveAbilitiesController controller)
		{
		}

		// Token: 0x06000C2A RID: 3114 RVA: 0x0002627C File Offset: 0x0002447C
		protected virtual void OnDestroy()
		{
			this.IsDestroyed = true;
			Action<object> destroyed = this.Destroyed;
			if (destroyed == null)
			{
				return;
			}
			destroyed(this);
		}
	}
}
