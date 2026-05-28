using System;
using Game.Damage;

namespace Unliving.Mobs
{
	// Token: 0x020001E5 RID: 485
	[Serializable]
	public class VitalContainer : EnergyContainer
	{
		// Token: 0x17000329 RID: 809
		// (get) Token: 0x06000FE4 RID: 4068 RVA: 0x00031E32 File Offset: 0x00030032
		// (set) Token: 0x06000FE5 RID: 4069 RVA: 0x00031E3C File Offset: 0x0003003C
		public HealthContainer HealthContainer
		{
			get
			{
				return this.healthContainer;
			}
			set
			{
				if (this.healthContainer != null)
				{
					this.healthContainer.ContainerDestroyed -= this.OnHealthContainerDestroyed;
					this.healthContainer.EnergyAmountChanged -= this.OnHealthContainerAmountChanged;
				}
				this.healthContainer = value;
				if (this.healthContainer != null)
				{
					this.healthContainer.ContainerDestroyed += this.OnHealthContainerDestroyed;
					this.initialAmount = this.healthContainer.InitialEnergyAmount;
					this.currentAmount = this.initialAmount;
				}
			}
		}

		// Token: 0x1700032A RID: 810
		// (get) Token: 0x06000FE6 RID: 4070 RVA: 0x00031EC2 File Offset: 0x000300C2
		public override bool IsAlive
		{
			get
			{
				return this.healthContainer != null && this.healthContainer.IsAlive;
			}
		}

		// Token: 0x1700032B RID: 811
		// (get) Token: 0x06000FE7 RID: 4071 RVA: 0x00031ED9 File Offset: 0x000300D9
		// (set) Token: 0x06000FE8 RID: 4072 RVA: 0x00031EE1 File Offset: 0x000300E1
		public override float InitialEnergyAmount
		{
			get
			{
				return base.InitialEnergyAmount;
			}
			set
			{
				base.InitialEnergyAmount = value;
				this.healthContainer.InitialEnergyAmount = value;
			}
		}

		// Token: 0x06000FE9 RID: 4073 RVA: 0x00031EF6 File Offset: 0x000300F6
		private void OnHealthContainerAmountChanged()
		{
			if (base.CurrentEnergyAmount > this.healthContainer.CurrentEnergyAmount)
			{
				this.currentAmount = this.healthContainer.CurrentEnergyAmount;
			}
		}

		// Token: 0x06000FEA RID: 4074 RVA: 0x00031F1C File Offset: 0x0003011C
		public VitalContainer(HealthContainer healthContainer)
		{
			this.HealthContainer = healthContainer;
		}

		// Token: 0x06000FEB RID: 4075 RVA: 0x00031F2C File Offset: 0x0003012C
		public override void SetContainerData(EnergyContainer.EnergyContainerData data)
		{
			HealthContainer healthContainer = this.healthContainer;
			if (healthContainer != null)
			{
				healthContainer.SetContainerData(data);
			}
			VitalContainer.VitalContainerData vitalContainerData = data as VitalContainer.VitalContainerData;
			if (vitalContainerData != null)
			{
				this.currentAmount = vitalContainerData.currentVitalAmount;
				this.initialAmount = vitalContainerData.initialAmount;
			}
		}

		// Token: 0x06000FEC RID: 4076 RVA: 0x00031F70 File Offset: 0x00030170
		public override EnergyContainer.EnergyContainerData GetContainerData()
		{
			VitalContainer.VitalContainerData vitalContainerData = new VitalContainer.VitalContainerData();
			vitalContainerData.currentVitalAmount = this.currentAmount;
			EnergyContainer.EnergyContainerData containerData = this.healthContainer.GetContainerData();
			vitalContainerData.currentAmount = containerData.currentAmount;
			vitalContainerData.initialAmount = containerData.initialAmount;
			return vitalContainerData;
		}

		// Token: 0x06000FED RID: 4077 RVA: 0x00031FB2 File Offset: 0x000301B2
		private void OnHealthContainerDestroyed()
		{
			this.currentAmount = 0f;
			this.OnContainerDestroyed();
		}

		// Token: 0x04000936 RID: 2358
		private HealthContainer healthContainer;

		// Token: 0x0200049F RID: 1183
		[Serializable]
		public class VitalContainerData : EnergyContainer.EnergyContainerData
		{
			// Token: 0x040018FD RID: 6397
			public float currentVitalAmount;
		}
	}
}
