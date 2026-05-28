using System;
using System.Collections;
using System.Collections.Generic;
using Game.Damage;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001DC RID: 476
	public class ContainerHitPointsController : MobHealthController, IContainerBasedHPController, IDamageable, IHitPointsSource
	{
		// Token: 0x17000310 RID: 784
		// (get) Token: 0x06000F69 RID: 3945 RVA: 0x00030C7B File Offset: 0x0002EE7B
		public override bool IsAlive
		{
			get
			{
				return this.healthContainers.Count > 0 && this.healthContainers[0] != null && this.healthContainers[0].IsAlive;
			}
		}

		// Token: 0x17000311 RID: 785
		// (get) Token: 0x06000F6A RID: 3946 RVA: 0x00030CAC File Offset: 0x0002EEAC
		public override float MaxHitPoints
		{
			get
			{
				return this.maxHealth;
			}
		}

		// Token: 0x17000312 RID: 786
		// (get) Token: 0x06000F6B RID: 3947 RVA: 0x00030CB4 File Offset: 0x0002EEB4
		// (set) Token: 0x06000F6C RID: 3948 RVA: 0x00030CBC File Offset: 0x0002EEBC
		public override float InitialHitPoints
		{
			get
			{
				return this.initialContainerEnergyAmount;
			}
			set
			{
				if (value != this.initialContainerEnergyAmount)
				{
					this.initialContainerEnergyAmount = value;
					foreach (IEnergyContainer energyContainer in this.healthContainers)
					{
						energyContainer.InitialEnergyAmount = this.initialContainerEnergyAmount;
					}
				}
			}
		}

		// Token: 0x17000313 RID: 787
		// (get) Token: 0x06000F6D RID: 3949 RVA: 0x00030D24 File Offset: 0x0002EF24
		public override float CurrentHitPoints
		{
			get
			{
				return this.currentHealthCached;
			}
		}

		// Token: 0x17000314 RID: 788
		// (get) Token: 0x06000F6E RID: 3950 RVA: 0x00030D2C File Offset: 0x0002EF2C
		public IEnergyContainer CurrentHealthContainer
		{
			get
			{
				if (this.currentContainerIdx >= 0 && this.currentContainerIdx < this.healthContainers.Count)
				{
					return this.healthContainers[this.currentContainerIdx];
				}
				return null;
			}
		}

		// Token: 0x17000315 RID: 789
		// (get) Token: 0x06000F6F RID: 3951 RVA: 0x00030D5D File Offset: 0x0002EF5D
		public IReadOnlyCollection<IEnergyContainer> HealthContainers
		{
			get
			{
				return this.healthContainers;
			}
		}

		// Token: 0x17000316 RID: 790
		// (get) Token: 0x06000F70 RID: 3952 RVA: 0x00030D65 File Offset: 0x0002EF65
		// (set) Token: 0x06000F71 RID: 3953 RVA: 0x00030D74 File Offset: 0x0002EF74
		public float DamageResistanceTimeout
		{
			get
			{
				return this.damageResistanceTimeout * this.DamageResistanceTimeoutMultiplier;
			}
			set
			{
				this.damageResistanceTimeout = value;
			}
		}

		// Token: 0x17000317 RID: 791
		// (get) Token: 0x06000F72 RID: 3954 RVA: 0x00030D7D File Offset: 0x0002EF7D
		// (set) Token: 0x06000F73 RID: 3955 RVA: 0x00030D85 File Offset: 0x0002EF85
		public float DamageResistanceTimeoutMultiplier { get; set; } = 1f;

		// Token: 0x17000318 RID: 792
		// (get) Token: 0x06000F74 RID: 3956 RVA: 0x00030D8E File Offset: 0x0002EF8E
		// (set) Token: 0x06000F75 RID: 3957 RVA: 0x00030D96 File Offset: 0x0002EF96
		public int MaxSlotsCount { get; protected set; }

		// Token: 0x17000319 RID: 793
		// (get) Token: 0x06000F76 RID: 3958 RVA: 0x00030D9F File Offset: 0x0002EF9F
		// (set) Token: 0x06000F77 RID: 3959 RVA: 0x00030DA7 File Offset: 0x0002EFA7
		public int CurrentSlotsCount { get; protected set; }

		// Token: 0x1700031A RID: 794
		// (get) Token: 0x06000F78 RID: 3960 RVA: 0x00030DB0 File Offset: 0x0002EFB0
		public int EmptySlotsCount
		{
			get
			{
				return this.CurrentSlotsCount - this.healthContainers.Count;
			}
		}

		// Token: 0x140000A4 RID: 164
		// (add) Token: 0x06000F79 RID: 3961 RVA: 0x00030DC4 File Offset: 0x0002EFC4
		// (remove) Token: 0x06000F7A RID: 3962 RVA: 0x00030DFC File Offset: 0x0002EFFC
		public virtual event Action<IEnergyContainer> ContainerAdded;

		// Token: 0x140000A5 RID: 165
		// (add) Token: 0x06000F7B RID: 3963 RVA: 0x00030E34 File Offset: 0x0002F034
		// (remove) Token: 0x06000F7C RID: 3964 RVA: 0x00030E6C File Offset: 0x0002F06C
		public virtual event Action<IEnergyContainer> ContainerRemoved;

		// Token: 0x140000A6 RID: 166
		// (add) Token: 0x06000F7D RID: 3965 RVA: 0x00030EA4 File Offset: 0x0002F0A4
		// (remove) Token: 0x06000F7E RID: 3966 RVA: 0x00030EDC File Offset: 0x0002F0DC
		public event Action<IEnergyContainer> ContainerDestroyed;

		// Token: 0x140000A7 RID: 167
		// (add) Token: 0x06000F7F RID: 3967 RVA: 0x00030F14 File Offset: 0x0002F114
		// (remove) Token: 0x06000F80 RID: 3968 RVA: 0x00030F4C File Offset: 0x0002F14C
		public event Action<int, int> ContainerSlotsCountChanged;

		// Token: 0x140000A8 RID: 168
		// (add) Token: 0x06000F81 RID: 3969 RVA: 0x00030F84 File Offset: 0x0002F184
		// (remove) Token: 0x06000F82 RID: 3970 RVA: 0x00030FBC File Offset: 0x0002F1BC
		public event Action DamageResistActivated;

		// Token: 0x140000A9 RID: 169
		// (add) Token: 0x06000F83 RID: 3971 RVA: 0x00030FF4 File Offset: 0x0002F1F4
		// (remove) Token: 0x06000F84 RID: 3972 RVA: 0x0003102C File Offset: 0x0002F22C
		public event Action DamageResistDeactivated;

		// Token: 0x06000F85 RID: 3973 RVA: 0x00031061 File Offset: 0x0002F261
		protected virtual bool IsDamageResistanceActive(IHitPointsChangingArgs args)
		{
			return args.IsDamage && this.damageResistanceTime > Time.time;
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x0003107A File Offset: 0x0002F27A
		protected override bool IsHitPointsModificationNotAllowed(object sender, IHitPointsChangingArgs args)
		{
			return !args.IsForcedChanging && (this.IsDamageResistanceActive(args) || base.IsHitPointsModificationNotAllowed(sender, args));
		}

		// Token: 0x06000F87 RID: 3975 RVA: 0x0003109C File Offset: 0x0002F29C
		protected override void UpdateHitPointsAmount(object sender, IHitPointsChangingArgs args)
		{
			IEnergyContainer currentHealthContainer = this.CurrentHealthContainer;
			if (currentHealthContainer != null)
			{
				float num;
				currentHealthContainer.ChangeEnergyAmount(args.Amount * (float)(args.IsDamage ? -1 : 1), out num);
			}
			if (this.CurrentHitPoints > 0f && (this.CurrentHealthContainer == null || !this.CurrentHealthContainer.IsAlive))
			{
				this.ActivateNextContainer(sender);
				return;
			}
			this.UpdateCurrentAmount();
		}

		// Token: 0x06000F88 RID: 3976 RVA: 0x00031100 File Offset: 0x0002F300
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.healthContainers = null;
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x00031110 File Offset: 0x0002F310
		public virtual void SetContainersData(float resistTimeout, List<IEnergyContainer> containers, int maxSlotsCount, int currentSlotsCount)
		{
			this.CurrentSlotsCount = currentSlotsCount;
			this.MaxSlotsCount = maxSlotsCount;
			this.DamageResistanceTimeout = resistTimeout;
			this.healthContainers.Clear();
			this.currentHealthCached = 0f;
			foreach (IEnergyContainer energyContainer in containers)
			{
				this.healthContainers.Add(energyContainer);
				this.currentHealthCached = energyContainer.CurrentEnergyAmount;
				if (energyContainer.IsAlive)
				{
					this.currentContainerIdx = this.healthContainers.Count - 1;
				}
				if (this.healthContainers.Count >= currentSlotsCount)
				{
					break;
				}
			}
			this.UpdateMaxAmount();
			Action<IEnergyContainer> containerAdded = this.ContainerAdded;
			if (containerAdded == null)
			{
				return;
			}
			containerAdded(null);
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x000311E0 File Offset: 0x0002F3E0
		public bool CanAddContainerSlot()
		{
			return this.CurrentSlotsCount < this.MaxSlotsCount;
		}

		// Token: 0x06000F8B RID: 3979 RVA: 0x000311F0 File Offset: 0x0002F3F0
		public bool AddContainerSlot()
		{
			if (!this.CanAddContainerSlot())
			{
				return false;
			}
			int currentSlotsCount = this.CurrentSlotsCount;
			this.CurrentSlotsCount = currentSlotsCount + 1;
			int arg = currentSlotsCount;
			Action<int, int> containerSlotsCountChanged = this.ContainerSlotsCountChanged;
			if (containerSlotsCountChanged != null)
			{
				containerSlotsCountChanged(arg, this.CurrentSlotsCount);
			}
			return true;
		}

		// Token: 0x06000F8C RID: 3980 RVA: 0x00031234 File Offset: 0x0002F434
		public bool RemoveContainerSlot()
		{
			if (this.CurrentSlotsCount <= 0)
			{
				return false;
			}
			int currentSlotsCount = this.CurrentSlotsCount;
			this.CurrentSlotsCount = currentSlotsCount - 1;
			int arg = currentSlotsCount;
			Action<int, int> containerSlotsCountChanged = this.ContainerSlotsCountChanged;
			if (containerSlotsCountChanged != null)
			{
				containerSlotsCountChanged(arg, this.CurrentSlotsCount);
			}
			return true;
		}

		// Token: 0x06000F8D RID: 3981 RVA: 0x00031277 File Offset: 0x0002F477
		public virtual bool CanAddContainer()
		{
			return this.healthContainers.Count < this.CurrentSlotsCount;
		}

		// Token: 0x06000F8E RID: 3982 RVA: 0x0003128C File Offset: 0x0002F48C
		public virtual bool AddContainer(IEnergyContainer container)
		{
			if (!this.CanAddContainer())
			{
				return false;
			}
			if (this.healthContainers.Contains(container))
			{
				return false;
			}
			this.healthContainers.Insert(0, container);
			this.currentContainerIdx++;
			this.UpdateMaxAmount();
			this.UpdateCurrentAmount();
			Action<IEnergyContainer> containerAdded = this.ContainerAdded;
			if (containerAdded != null)
			{
				containerAdded(container);
			}
			return true;
		}

		// Token: 0x06000F8F RID: 3983 RVA: 0x000312ED File Offset: 0x0002F4ED
		public virtual bool RemoveContainer(IEnergyContainer container, object damageSender)
		{
			if (!this.healthContainers.Contains(container))
			{
				return false;
			}
			this.healthContainers.Remove(container);
			this.UpdateMaxAmount();
			this.UpdateCurrentAmount();
			Action<IEnergyContainer> containerRemoved = this.ContainerRemoved;
			if (containerRemoved != null)
			{
				containerRemoved(container);
			}
			return true;
		}

		// Token: 0x06000F90 RID: 3984 RVA: 0x0003132C File Offset: 0x0002F52C
		public virtual int GetContainerIndex(IEnergyContainer container)
		{
			for (int i = 0; i < this.healthContainers.Count; i++)
			{
				if (this.healthContainers[i].Equals(container))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000F91 RID: 3985 RVA: 0x00031366 File Offset: 0x0002F566
		public void ActivateDamageResist(float time)
		{
			this.damageResistanceTime = Time.time + time;
			Action damageResistActivated = this.DamageResistActivated;
			if (damageResistActivated != null)
			{
				damageResistActivated();
			}
			base.StopAllCoroutines();
			base.StartCoroutine(this.NotifyDamageResistDeactivated(this.DamageResistanceTimeout));
		}

		// Token: 0x06000F92 RID: 3986 RVA: 0x0003139F File Offset: 0x0002F59F
		public void DeactivateDamageResist()
		{
			this.damageResistanceTime = Time.time - 1f;
			Action damageResistDeactivated = this.DamageResistDeactivated;
			if (damageResistDeactivated == null)
			{
				return;
			}
			damageResistDeactivated();
		}

		// Token: 0x06000F93 RID: 3987 RVA: 0x000313C4 File Offset: 0x0002F5C4
		protected virtual void UpdateMaxAmount()
		{
			this.maxHealth = 0f;
			foreach (IEnergyContainer energyContainer in this.healthContainers)
			{
				this.maxHealth += energyContainer.InitialEnergyAmount;
			}
		}

		// Token: 0x06000F94 RID: 3988 RVA: 0x00031430 File Offset: 0x0002F630
		protected virtual void UpdateCurrentAmount()
		{
			this.currentHealthCached = 0f;
			foreach (IEnergyContainer energyContainer in this.healthContainers)
			{
				if (energyContainer.IsAlive)
				{
					this.currentHealthCached += energyContainer.CurrentEnergyAmount;
				}
			}
		}

		// Token: 0x06000F95 RID: 3989 RVA: 0x000314A4 File Offset: 0x0002F6A4
		protected void ActivateNextContainer(object damageSender)
		{
			if (this.CurrentHealthContainer != null && this.CurrentHealthContainer.IsAlive)
			{
				return;
			}
			Action<IEnergyContainer> containerDestroyed = this.ContainerDestroyed;
			if (containerDestroyed != null)
			{
				containerDestroyed(this.CurrentHealthContainer);
			}
			this.RemoveContainer(this.CurrentHealthContainer, damageSender);
			this.ActivateDamageResist(this.DamageResistanceTimeout);
			for (int i = this.healthContainers.Count - 1; i >= 0; i--)
			{
				if (this.healthContainers[i] != null && this.healthContainers[i].IsAlive)
				{
					this.currentContainerIdx = i;
					return;
				}
			}
		}

		// Token: 0x06000F96 RID: 3990 RVA: 0x00031539 File Offset: 0x0002F739
		private IEnumerator NotifyDamageResistDeactivated(float timeout)
		{
			yield return new WaitForSeconds(timeout);
			Action damageResistDeactivated = this.DamageResistDeactivated;
			if (damageResistDeactivated != null)
			{
				damageResistDeactivated();
			}
			yield break;
		}

		// Token: 0x06000F97 RID: 3991 RVA: 0x00031550 File Offset: 0x0002F750
		public override void ApplyLethalDamage(object damageSender = null)
		{
			int count = this.healthContainers.Count;
			for (int i = 0; i < count; i++)
			{
				this.RemoveContainer(this.healthContainers[0], damageSender);
			}
			HitPointsController.HPChangingArgs args = new HitPointsController.HPChangingArgs(true)
			{
				amount = this.CurrentHitPoints + 1f,
				isSilentChanging = true,
				disableTargetReaction = true,
				isForcedChanging = true
			};
			base.TryModifyHitPoints(damageSender, args);
		}

		// Token: 0x04000915 RID: 2325
		protected List<IEnergyContainer> healthContainers = new List<IEnergyContainer>();

		// Token: 0x04000916 RID: 2326
		private float maxHealth;

		// Token: 0x04000917 RID: 2327
		protected float initialContainerEnergyAmount;

		// Token: 0x04000918 RID: 2328
		protected float currentHealthCached;

		// Token: 0x04000919 RID: 2329
		protected int currentContainerIdx;

		// Token: 0x0400091A RID: 2330
		protected float damageResistanceTime = float.MinValue;

		// Token: 0x0400091B RID: 2331
		private float damageResistanceTimeout;
	}
}
