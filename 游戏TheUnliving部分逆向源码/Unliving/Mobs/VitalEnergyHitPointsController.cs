using System;
using System.Collections.Generic;
using System.Linq;
using Game.Abilities;
using Game.Damage;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Currencies;

namespace Unliving.Mobs
{
	// Token: 0x020001E6 RID: 486
	public class VitalEnergyHitPointsController : ContainerHitPointsController, IContainerBasedHPControllerWithVitalEnergy, IContainerBasedHPController, IDamageable, IHitPointsSource, IAbilitiesEnergySource, IEnergyRegenerable, IAbilityActivatedContainersController
	{
		// Token: 0x1700032C RID: 812
		// (get) Token: 0x06000FEE RID: 4078 RVA: 0x00031FC5 File Offset: 0x000301C5
		public override float HitPointsLack
		{
			get
			{
				return this.MaxVitalAmount - this.CurrentVitalAmount + base.HitPointsLack;
			}
		}

		// Token: 0x1700032D RID: 813
		// (get) Token: 0x06000FEF RID: 4079 RVA: 0x00031FDB File Offset: 0x000301DB
		public override float CurrentHitPoints
		{
			get
			{
				return this.currentHealthCached;
			}
		}

		// Token: 0x1700032E RID: 814
		// (get) Token: 0x06000FF0 RID: 4080 RVA: 0x00031FE3 File Offset: 0x000301E3
		public IReadOnlyCollection<VitalContainer> VitalContainers
		{
			get
			{
				return this.vitalContainers;
			}
		}

		// Token: 0x1700032F RID: 815
		// (get) Token: 0x06000FF1 RID: 4081 RVA: 0x00031FEB File Offset: 0x000301EB
		// (set) Token: 0x06000FF2 RID: 4082 RVA: 0x00031FF3 File Offset: 0x000301F3
		public float MaxVitalAmount { get; private set; }

		// Token: 0x17000330 RID: 816
		// (get) Token: 0x06000FF3 RID: 4083 RVA: 0x00031FFC File Offset: 0x000301FC
		// (set) Token: 0x06000FF4 RID: 4084 RVA: 0x00032004 File Offset: 0x00030204
		public override float InitialHitPoints
		{
			get
			{
				return base.InitialHitPoints;
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
					foreach (VitalContainer vitalContainer in this.vitalContainers)
					{
						vitalContainer.InitialEnergyAmount = this.initialContainerEnergyAmount;
					}
					this.UpdateCurrentAmount();
					this.UpdateMaxAmount();
				}
			}
		}

		// Token: 0x17000331 RID: 817
		// (get) Token: 0x06000FF5 RID: 4085 RVA: 0x000320C0 File Offset: 0x000302C0
		// (set) Token: 0x06000FF6 RID: 4086 RVA: 0x000320C8 File Offset: 0x000302C8
		public float CurrentVitalAmount { get; private set; }

		// Token: 0x17000332 RID: 818
		// (get) Token: 0x06000FF7 RID: 4087 RVA: 0x000320D1 File Offset: 0x000302D1
		// (set) Token: 0x06000FF8 RID: 4088 RVA: 0x000320D9 File Offset: 0x000302D9
		public bool RestoreVitalEnergyOnHealing
		{
			get
			{
				return this.restoreVitalEnergyOnHealing;
			}
			set
			{
				this.restoreVitalEnergyOnHealing = value;
			}
		}

		// Token: 0x140000AF RID: 175
		// (add) Token: 0x06000FF9 RID: 4089 RVA: 0x000320E4 File Offset: 0x000302E4
		// (remove) Token: 0x06000FFA RID: 4090 RVA: 0x0003211C File Offset: 0x0003031C
		public override event Action<IEnergyContainer> ContainerAdded;

		// Token: 0x140000B0 RID: 176
		// (add) Token: 0x06000FFB RID: 4091 RVA: 0x00032154 File Offset: 0x00030354
		// (remove) Token: 0x06000FFC RID: 4092 RVA: 0x0003218C File Offset: 0x0003038C
		public override event Action<IEnergyContainer> ContainerRemoved;

		// Token: 0x06000FFD RID: 4093 RVA: 0x000321C1 File Offset: 0x000303C1
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.vitalContainers = null;
		}

		// Token: 0x06000FFE RID: 4094 RVA: 0x000321D0 File Offset: 0x000303D0
		protected override void UpdateHitPointsAmount(object sender, IHitPointsChangingArgs args)
		{
			float num = args.Amount;
			if (args.IsDamage)
			{
				if (args is VitalEnergyHitPointsController.ConsumeVitalEnergyArgs)
				{
					if (num > this.CurrentVitalAmount)
					{
						num = this.CurrentVitalAmount;
					}
					for (int i = this.vitalContainers.Count - 1; i >= 0; i--)
					{
						VitalContainer vitalContainer = this.vitalContainers[i];
						if (vitalContainer.IsAlive)
						{
							float num2;
							vitalContainer.ChangeEnergyAmount(-num, out num2);
							num += num2;
							if (num < 0.001f)
							{
								break;
							}
						}
					}
				}
				else
				{
					IEnergyContainer currentHealthContainer = base.CurrentHealthContainer;
					if (currentHealthContainer != null)
					{
						float num3;
						currentHealthContainer.ChangeEnergyAmount(-num, out num3);
					}
				}
			}
			else
			{
				bool flag = args is VitalEnergyHitPointsController.RestoreVitalEnergyArgs;
				float num4 = args.Amount;
				if (this.restoreVitalEnergyOnHealing || flag)
				{
					int num5 = 0;
					while (num5 < this.vitalContainers.Count && this.healthContainers[num5].IsAlive)
					{
						VitalContainer vitalContainer2 = this.vitalContainers[num5];
						if (!vitalContainer2.IsFull)
						{
							float num6;
							vitalContainer2.ChangeEnergyAmount(num4, out num6);
							num4 -= num6;
							if (num4 <= 0.0001f)
							{
								break;
							}
						}
						num5++;
					}
				}
				if (!flag)
				{
					num4 = args.Amount;
					for (int j = 0; j < this.healthContainers.Count; j++)
					{
						IEnergyContainer energyContainer = this.healthContainers[j];
						if (!energyContainer.IsAlive)
						{
							break;
						}
						if (!energyContainer.IsFull)
						{
							float num6;
							energyContainer.ChangeEnergyAmount(num4, out num6);
							num4 -= num6;
							if (num4 <= 0.0001f)
							{
								break;
							}
						}
					}
				}
			}
			if (this.CurrentHitPoints > 0f && (base.CurrentHealthContainer == null || !base.CurrentHealthContainer.IsAlive))
			{
				base.ActivateNextContainer(sender);
				return;
			}
			this.UpdateCurrentAmount();
		}

		// Token: 0x06000FFF RID: 4095 RVA: 0x00032380 File Offset: 0x00030580
		protected override bool IsDamageResistanceActive(IHitPointsChangingArgs args)
		{
			return base.IsDamageResistanceActive(args) && !(args is VitalEnergyHitPointsController.ConsumeVitalEnergyArgs);
		}

		// Token: 0x06001000 RID: 4096 RVA: 0x0003239C File Offset: 0x0003059C
		public override void SetContainersData(float resistTimeout, List<IEnergyContainer> containers, int maxSlotsCount, int currentSlotsCount)
		{
			base.CurrentSlotsCount = currentSlotsCount;
			base.MaxSlotsCount = maxSlotsCount;
			base.DamageResistanceTimeout = resistTimeout;
			this.healthContainers.Clear();
			this.vitalContainers.Clear();
			this.currentHealthCached = 0f;
			this.CurrentVitalAmount = 0f;
			foreach (IEnergyContainer energyContainer in containers)
			{
				VitalContainer vitalContainer = energyContainer as VitalContainer;
				if (vitalContainer == null)
				{
					vitalContainer = new VitalContainer(energyContainer as HealthContainer);
					vitalContainer.ResetEnergy();
				}
				this.vitalContainers.Add(vitalContainer);
				this.healthContainers.Add(vitalContainer.HealthContainer);
				this.currentHealthCached += vitalContainer.HealthContainer.CurrentEnergyAmount;
				this.CurrentVitalAmount += vitalContainer.CurrentEnergyAmount;
				if (vitalContainer.HealthContainer.IsAlive)
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

		// Token: 0x06001001 RID: 4097 RVA: 0x000324D8 File Offset: 0x000306D8
		public override bool AddContainer(IEnergyContainer container)
		{
			HealthContainer healthContainer = container as HealthContainer;
			if (healthContainer == null)
			{
				return false;
			}
			if (!base.AddContainer(container))
			{
				return false;
			}
			VitalContainer item = new VitalContainer(healthContainer);
			this.vitalContainers.Insert(0, item);
			this.UpdateMaxAmount();
			this.UpdateCurrentAmount();
			Action<IEnergyContainer> containerAdded = this.ContainerAdded;
			if (containerAdded != null)
			{
				containerAdded(container);
			}
			return true;
		}

		// Token: 0x06001002 RID: 4098 RVA: 0x00032530 File Offset: 0x00030730
		public override bool RemoveContainer(IEnergyContainer container, object damageSender)
		{
			if (!base.RemoveContainer(container, damageSender))
			{
				return false;
			}
			VitalContainer vitalContainer = null;
			foreach (VitalContainer vitalContainer2 in this.vitalContainers)
			{
				if (vitalContainer2.HealthContainer == container)
				{
					vitalContainer = vitalContainer2;
					break;
				}
			}
			if (vitalContainer != null)
			{
				this.vitalContainers.Remove(vitalContainer);
				((IAbilityActivatedContainersController)this).RemoveLastContainer(damageSender);
				Action<IEnergyContainer> containerRemoved = this.ContainerRemoved;
				if (containerRemoved != null)
				{
					containerRemoved(vitalContainer);
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001003 RID: 4099 RVA: 0x000325C4 File Offset: 0x000307C4
		public override int GetContainerIndex(IEnergyContainer container)
		{
			VitalContainer vitalContainer = container as VitalContainer;
			if (vitalContainer != null)
			{
				for (int i = 0; i < this.vitalContainers.Count; i++)
				{
					if (this.vitalContainers[i].Equals(vitalContainer))
					{
						return i;
					}
				}
				return -1;
			}
			return base.GetContainerIndex(container);
		}

		// Token: 0x06001004 RID: 4100 RVA: 0x00032610 File Offset: 0x00030810
		protected override void UpdateMaxAmount()
		{
			base.UpdateMaxAmount();
			this.MaxVitalAmount = 0f;
			foreach (VitalContainer vitalContainer in this.vitalContainers)
			{
				this.MaxVitalAmount += vitalContainer.InitialEnergyAmount;
			}
		}

		// Token: 0x06001005 RID: 4101 RVA: 0x00032680 File Offset: 0x00030880
		protected override void UpdateCurrentAmount()
		{
			base.UpdateCurrentAmount();
			this.CurrentVitalAmount = 0f;
			foreach (VitalContainer vitalContainer in this.vitalContainers)
			{
				if (vitalContainer.IsAlive)
				{
					this.CurrentVitalAmount += vitalContainer.CurrentEnergyAmount;
				}
			}
		}

		// Token: 0x06001006 RID: 4102 RVA: 0x000326F8 File Offset: 0x000308F8
		bool IAbilitiesEnergySource.HasEnoughEnergy(float energyAmount)
		{
			return this.CurrentVitalAmount >= energyAmount;
		}

		// Token: 0x140000B1 RID: 177
		// (add) Token: 0x06001007 RID: 4103 RVA: 0x00032708 File Offset: 0x00030908
		// (remove) Token: 0x06001008 RID: 4104 RVA: 0x00032740 File Offset: 0x00030940
		public event Action<int, IAbilityActivatedContainer> AbilityActivatedContainerAdded;

		// Token: 0x140000B2 RID: 178
		// (add) Token: 0x06001009 RID: 4105 RVA: 0x00032778 File Offset: 0x00030978
		// (remove) Token: 0x0600100A RID: 4106 RVA: 0x000327B0 File Offset: 0x000309B0
		public event Action<IAbilityActivatedContainer> AbilityActivatedContainerDestroyed;

		// Token: 0x0600100B RID: 4107 RVA: 0x000327E8 File Offset: 0x000309E8
		void IAbilityActivatedContainersController.AddContainer(AbilityInfo abilityInfo, ICurrencyOperationArgs destructionRewardArgs)
		{
			int count = this.healthContainers.Count;
			int num = this.healthContainers.Count - 1;
			if (num < 0)
			{
				return;
			}
			int num2 = count - this.abilityActivatedContainers.Count;
			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					this.AddEmptyAbilityActivatedContainer();
				}
			}
			IAbilityActivatedContainer abilityActivatedContainer = this.abilityActivatedContainers[num];
			abilityActivatedContainer.AddAbility(abilityInfo);
			abilityActivatedContainer.SetDestructionRewardArgs(destructionRewardArgs);
		}

		// Token: 0x0600100C RID: 4108 RVA: 0x00032854 File Offset: 0x00030A54
		void IAbilityActivatedContainersController.RemoveLastContainer(object damageSender)
		{
			if (this.abilityActivatedContainers.Count == 0)
			{
				return;
			}
			IAbilityActivatedContainer abilityActivatedContainer = this.abilityActivatedContainers.Last<IAbilityActivatedContainer>();
			Action<IAbilityActivatedContainer> abilityActivatedContainerDestroyed = this.AbilityActivatedContainerDestroyed;
			if (abilityActivatedContainerDestroyed != null)
			{
				abilityActivatedContainerDestroyed(abilityActivatedContainer);
			}
			abilityActivatedContainer.DestroyContainer(damageSender);
			this.abilityActivatedContainers.Remove(abilityActivatedContainer);
		}

		// Token: 0x0600100D RID: 4109 RVA: 0x000328A1 File Offset: 0x00030AA1
		IReadOnlyList<IAbilityActivatedContainer> IAbilityActivatedContainersController.GetContainers()
		{
			return this.abilityActivatedContainers;
		}

		// Token: 0x0600100E RID: 4110 RVA: 0x000328A9 File Offset: 0x00030AA9
		bool IAbilityActivatedContainersController.TryGetAbilityActivatedContainer(int containerIndex, out IAbilityActivatedContainer container)
		{
			container = null;
			if (containerIndex < this.abilityActivatedContainers.Count)
			{
				container = this.abilityActivatedContainers[containerIndex];
			}
			return container != null;
		}

		// Token: 0x0600100F RID: 4111 RVA: 0x000328D0 File Offset: 0x00030AD0
		bool IAbilityActivatedContainersController.HasAbilityContainer(AbilityID abilityID)
		{
			Func<BaseAbility, bool> <>9__0;
			for (int i = 0; i < this.abilityActivatedContainers.Count; i++)
			{
				IEnumerable<BaseAbility> abilities = this.abilityActivatedContainers[i].GetAbilities();
				Func<BaseAbility, bool> predicate;
				if ((predicate = <>9__0) == null)
				{
					predicate = (<>9__0 = ((BaseAbility a) => a.ID == (int)abilityID));
				}
				if (abilities.Any(predicate))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001010 RID: 4112 RVA: 0x0003293C File Offset: 0x00030B3C
		private void AddEmptyAbilityActivatedContainer()
		{
			AbilityActivatedContainer abilityActivatedContainer = new AbilityActivatedContainer(base.CurrentGame);
			this.abilityActivatedContainers.Add(abilityActivatedContainer);
			Action<int, IAbilityActivatedContainer> abilityActivatedContainerAdded = this.AbilityActivatedContainerAdded;
			if (abilityActivatedContainerAdded == null)
			{
				return;
			}
			abilityActivatedContainerAdded(this.abilityActivatedContainers.Count - 1, abilityActivatedContainer);
		}

		// Token: 0x06001011 RID: 4113 RVA: 0x0003297F File Offset: 0x00030B7F
		List<IAbilityActivatedContainerData> IAbilityActivatedContainersController.GetContainersData()
		{
			return (from c in this.abilityActivatedContainers
			select c.GetAbilitiesContainerData()).ToList<IAbilityActivatedContainerData>();
		}

		// Token: 0x06001012 RID: 4114 RVA: 0x000329B0 File Offset: 0x00030BB0
		void IAbilityActivatedContainersController.SetContainersData(List<IAbilityActivatedContainerData> data)
		{
			this.abilityActivatedContainers.Clear();
			for (int i = 0; i < data.Count; i++)
			{
				if (i >= this.healthContainers.Count)
				{
					return;
				}
				AbilityActivatedContainer item = new AbilityActivatedContainer(base.CurrentGame, data[i]);
				this.abilityActivatedContainers.Add(item);
				Action<IEnergyContainer> containerAdded = this.ContainerAdded;
				if (containerAdded != null)
				{
					containerAdded(null);
				}
			}
		}

		// Token: 0x17000333 RID: 819
		// (get) Token: 0x06001013 RID: 4115 RVA: 0x00032A19 File Offset: 0x00030C19
		float IEnergyRegenerable.Amount
		{
			get
			{
				return this.energyRegenAmount;
			}
		}

		// Token: 0x17000334 RID: 820
		// (get) Token: 0x06001014 RID: 4116 RVA: 0x00032A21 File Offset: 0x00030C21
		float IEnergyRegenerable.Timeout
		{
			get
			{
				return this.energyRegenTimeout;
			}
		}

		// Token: 0x17000335 RID: 821
		// (get) Token: 0x06001015 RID: 4117 RVA: 0x00032A29 File Offset: 0x00030C29
		float IEnergyRegenerable.Delay
		{
			get
			{
				return this.energyRegenDelay;
			}
		}

		// Token: 0x06001016 RID: 4118 RVA: 0x00032A31 File Offset: 0x00030C31
		void IEnergyRegenerable.SetEnergyRegenerationData(float amount, float timeout, float delay)
		{
			this.energyRegenAmount = amount;
			this.energyRegenTimeout = timeout;
			this.energyRegenDelay = delay;
		}

		// Token: 0x06001017 RID: 4119 RVA: 0x00032A48 File Offset: 0x00030C48
		void IEnergyRegenerable.RegenerateEnergy()
		{
			if (this.lastConsumeEnergyTime + this.energyRegenDelay > Time.time)
			{
				return;
			}
			if (this.lastEnergyRegenTime + this.energyRegenTimeout > Time.time)
			{
				return;
			}
			if (this.vitalContainers.Count == 0)
			{
				return;
			}
			VitalContainer vitalContainer = this.vitalContainers[0];
			if (vitalContainer == null || vitalContainer.IsFull)
			{
				return;
			}
			VitalEnergyHitPointsController.VitalEnergyRestoringArgs.amount = this.energyRegenAmount;
			if (base.TryModifyHitPoints(this, VitalEnergyHitPointsController.VitalEnergyRestoringArgs))
			{
				this.lastEnergyRegenTime = Time.time;
			}
		}

		// Token: 0x06001018 RID: 4120 RVA: 0x00032ACF File Offset: 0x00030CCF
		protected override void OnHitPointsChanged(object sender, IHitPointsChangingArgs args)
		{
			base.OnHitPointsChanged(sender, args);
			if (args is VitalEnergyHitPointsController.ConsumeVitalEnergyArgs)
			{
				this.lastConsumeEnergyTime = Time.time;
			}
		}

		// Token: 0x04000937 RID: 2359
		private static readonly VitalEnergyHitPointsController.RestoreVitalEnergyArgs VitalEnergyRestoringArgs = new VitalEnergyHitPointsController.RestoreVitalEnergyArgs();

		// Token: 0x0400093C RID: 2364
		[SerializeField]
		private bool restoreVitalEnergyOnHealing = true;

		// Token: 0x0400093D RID: 2365
		private List<VitalContainer> vitalContainers = new List<VitalContainer>();

		// Token: 0x04000940 RID: 2368
		private readonly List<IAbilityActivatedContainer> abilityActivatedContainers = new List<IAbilityActivatedContainer>();

		// Token: 0x04000941 RID: 2369
		private float energyRegenAmount;

		// Token: 0x04000942 RID: 2370
		private float energyRegenTimeout;

		// Token: 0x04000943 RID: 2371
		private float energyRegenDelay;

		// Token: 0x04000944 RID: 2372
		private float lastConsumeEnergyTime;

		// Token: 0x04000945 RID: 2373
		private float lastEnergyRegenTime;

		// Token: 0x020004A0 RID: 1184
		[Serializable]
		public sealed class ConsumeVitalEnergyArgs : HitPointsController.HPChangingArgs
		{
			// Token: 0x06002471 RID: 9329 RVA: 0x00070E56 File Offset: 0x0006F056
			public ConsumeVitalEnergyArgs() : base(true)
			{
			}
		}

		// Token: 0x020004A1 RID: 1185
		[Serializable]
		public sealed class RestoreVitalEnergyArgs : HitPointsController.HPChangingArgs
		{
			// Token: 0x06002472 RID: 9330 RVA: 0x00070E5F File Offset: 0x0006F05F
			public RestoreVitalEnergyArgs() : base(false)
			{
			}
		}
	}
}
