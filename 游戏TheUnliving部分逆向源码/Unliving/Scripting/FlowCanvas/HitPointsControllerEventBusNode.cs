using System;
using Common.UnityExtensions;
using FlowCanvas;
using Game.Damage;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A2 RID: 162
	[Name("HP Controller Events", 0)]
	[Category("Unliving/Events")]
	public sealed class HitPointsControllerEventBusNode : ObjectEventBusNodeBase
	{
		// Token: 0x0600043F RID: 1087 RVA: 0x0000EEF0 File Offset: 0x0000D0F0
		private void UpdateLastHP()
		{
			this.lastHP = this.hpController.CurrentHitPoints;
		}

		// Token: 0x06000440 RID: 1088 RVA: 0x0000EF03 File Offset: 0x0000D103
		private bool IsEventReceivingBlocked(IHitPointsChangingArgs args)
		{
			return !this.canReceiveEvents || args is VitalEnergyHitPointsController.ConsumeVitalEnergyArgs || args is VitalEnergyHitPointsController.RestoreVitalEnergyArgs;
		}

		// Token: 0x06000441 RID: 1089 RVA: 0x0000EF20 File Offset: 0x0000D120
		private bool IsLastHPContainerReached()
		{
			ContainerHitPointsController containerHitPointsController = this.hpController as ContainerHitPointsController;
			return containerHitPointsController != null && containerHitPointsController.CurrentSlotsCount - containerHitPointsController.EmptySlotsCount < 2;
		}

		// Token: 0x06000442 RID: 1090 RVA: 0x0000EF50 File Offset: 0x0000D150
		private void OnBeforeHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			this.args = args;
			this.canReceiveEvents = (!this.isLastHPContainerEvents.value || this.IsLastHPContainerReached());
			if (this.IsEventReceivingBlocked(args))
			{
				return;
			}
			Flow f = default(Flow);
			this.isDamage = args.IsDamage;
			this.sender = (sender as Component);
			this.amount = args.Amount;
			this.newHitPoints = Mathf.Clamp(hitPointsSource.CurrentHitPoints + (this.isDamage ? (-this.amount) : this.amount), 0f, hitPointsSource.InitialHitPoints);
			this.beforeHitPointsChanged.Call(f);
		}

		// Token: 0x06000443 RID: 1091 RVA: 0x0000EFF8 File Offset: 0x0000D1F8
		private void OnHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			if (this.IsEventReceivingBlocked(args))
			{
				return;
			}
			Flow f = default(Flow);
			if (args.Amount != 0f)
			{
				this.hitPointsChangeAttempted.Call(f);
			}
			this.amount = Mathf.Abs(this.hpController.CurrentHitPoints - this.lastHP);
			if (this.amount != 0f)
			{
				this.newHitPoints = hitPointsSource.CurrentHitPoints;
				this.hitPointsChanged.Call(f);
			}
			this.UpdateLastHP();
		}

		// Token: 0x06000444 RID: 1092 RVA: 0x0000F078 File Offset: 0x0000D278
		protected override GameObject GetEventsSourceObject()
		{
			return this.targetObject.value;
		}

		// Token: 0x06000445 RID: 1093 RVA: 0x0000F088 File Offset: 0x0000D288
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.targetObject = base.AddValueInput<GameObject>("targetObject", "");
			this.isLastHPContainerEvents = base.AddValueInput<bool>("isLastHPContainerEvents", "");
			this.beforeHitPointsChanged = base.AddFlowOutput("beforeHitPointsChanged", "");
			this.hitPointsChangeAttempted = base.AddFlowOutput("hitPointsChangeAttempted", "");
			this.hitPointsChanged = base.AddFlowOutput("hitPointsChanged", "");
			base.AddValueOutput<Component>("sender", () => this.sender, "");
			base.AddValueOutput<float>("amount", () => this.amount, "");
			base.AddValueOutput<bool>("isDamage", () => this.isDamage, "");
			base.AddValueOutput<float>("newHitPoints", () => this.newHitPoints, "");
			base.AddValueOutput<IHitPointsChangingArgs>("args", () => this.args, "");
		}

		// Token: 0x06000446 RID: 1094 RVA: 0x0000F19C File Offset: 0x0000D39C
		protected override void OnInitialize(Flow flow)
		{
			if (this.targetObject.value == null || !this.targetObject.value.TryGetComponent<HitPointsController>(out this.hpController))
			{
				return;
			}
			this.UpdateLastHP();
			this.hpController.BeforeHitPointsChanged += this.OnBeforeHitPointsChanged;
			this.hpController.HitPointsChanged += this.OnHitPointsChanged;
		}

		// Token: 0x06000447 RID: 1095 RVA: 0x0000F209 File Offset: 0x0000D409
		protected override void OnFinalize()
		{
			if (!this.hpController.IsNull())
			{
				this.hpController.BeforeHitPointsChanged -= this.OnBeforeHitPointsChanged;
				this.hpController.HitPointsChanged -= this.OnHitPointsChanged;
			}
		}

		// Token: 0x040002B2 RID: 690
		private ValueInput<GameObject> targetObject;

		// Token: 0x040002B3 RID: 691
		private ValueInput<bool> isLastHPContainerEvents;

		// Token: 0x040002B4 RID: 692
		private FlowOutput beforeHitPointsChanged;

		// Token: 0x040002B5 RID: 693
		private FlowOutput hitPointsChangeAttempted;

		// Token: 0x040002B6 RID: 694
		private FlowOutput hitPointsChanged;

		// Token: 0x040002B7 RID: 695
		private HitPointsController hpController;

		// Token: 0x040002B8 RID: 696
		private bool canReceiveEvents;

		// Token: 0x040002B9 RID: 697
		private float lastHP;

		// Token: 0x040002BA RID: 698
		private Component sender;

		// Token: 0x040002BB RID: 699
		private float amount;

		// Token: 0x040002BC RID: 700
		private bool isDamage;

		// Token: 0x040002BD RID: 701
		private float newHitPoints;

		// Token: 0x040002BE RID: 702
		private IHitPointsChangingArgs args;
	}
}
