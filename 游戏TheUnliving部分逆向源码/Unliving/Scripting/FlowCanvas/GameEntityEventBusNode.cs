using System;
using Common;
using FlowCanvas;
using Game.Damage;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x02000095 RID: 149
	[Name("Game Entity Events", 0)]
	[Category("Unliving/Events")]
	public sealed class GameEntityEventBusNode : ObjectEventBusNodeBase, IUpdatable, IGraphElement
	{
		// Token: 0x060003F3 RID: 1011 RVA: 0x0000DAEC File Offset: 0x0000BCEC
		private void UpdateObjectActivityState(bool force)
		{
			if (this.targetObjectValue == null)
			{
				return;
			}
			if (force || this.targetObjectValue.activeInHierarchy != this.isObjectActive)
			{
				Flow f = default(Flow);
				if (this.isObjectActive = this.targetObjectValue.activeInHierarchy)
				{
					this.objectActivated.Call(f);
					return;
				}
				this.objectDeactivated.Call(f);
			}
		}

		// Token: 0x060003F4 RID: 1012 RVA: 0x0000DB58 File Offset: 0x0000BD58
		private void UnsubscribeAll()
		{
			if (this.mobComponent != null)
			{
				IDamageable hitPointsController = this.mobComponent.HitPointsController;
				if (hitPointsController != null)
				{
					hitPointsController.HitPointsChanged -= this.OnGameEntityHitPointsChanged;
				}
				this.mobComponent.Sacrificed -= this.OnMobSacrificed;
				this.mobComponent.Killed -= new Action<IGameMob>(this.OnGameEntityKilled);
			}
			if (this.damageReceiver != null)
			{
				this.damageReceiver.HitPointsChanged -= this.OnGameEntityHitPointsChanged;
				this.damageReceiver.TotallyDestroyed -= new Action<IDamageable>(this.OnGameEntityKilled);
			}
			if (this.destructionEventHolder != null)
			{
				this.destructionEventHolder.Destroyed -= this.OnGameEntityKilled;
			}
		}

		// Token: 0x060003F5 RID: 1013 RVA: 0x0000DC17 File Offset: 0x0000BE17
		protected override GameObject GetEventsSourceObject()
		{
			return null;
		}

		// Token: 0x060003F6 RID: 1014 RVA: 0x0000DC1C File Offset: 0x0000BE1C
		private void OnMobSacrificed(BaseGameMob mob)
		{
			this.sacrificed.Call(default(Flow));
		}

		// Token: 0x060003F7 RID: 1015 RVA: 0x0000DC40 File Offset: 0x0000BE40
		private void OnGameEntityHitPointsChanged(IHitPointsSource hitPointsSource, object sender, IHitPointsChangingArgs args)
		{
			float value = this.targetHPThreshold.value;
			if (!this.isHPThresholdEventTotallyReached && value > 0f && hitPointsSource.InitialHitPoints > 0f)
			{
				bool flag = hitPointsSource.CurrentHitPoints / hitPointsSource.InitialHitPoints < Mathf.Clamp01(this.targetHPThreshold.value);
				if (this.isHPThresholdReached != flag)
				{
					if (flag)
					{
						this.hitPointsThresholdReached.Call(default(Flow));
						this.isHPThresholdEventTotallyReached = this.useHPThresholdOnce.value;
					}
					this.isHPThresholdReached = flag;
				}
			}
			if (args.IsDamage && args.Amount != 0f)
			{
				this.damageReceived.Call(default(Flow));
			}
		}

		// Token: 0x060003F8 RID: 1016 RVA: 0x0000DCF8 File Offset: 0x0000BEF8
		private void OnGameEntityKilled(object entity)
		{
			this.UnsubscribeAll();
			this.killed.Call(default(Flow));
		}

		// Token: 0x060003F9 RID: 1017 RVA: 0x0000DD20 File Offset: 0x0000BF20
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.targetObject = base.AddValueInput<GameObject>("targetObject", "");
			this.targetHPThreshold = base.AddValueInput<float>("targetHPThreshold", "");
			this.useHPThresholdOnce = base.AddValueInput<bool>("useHPThresholdOnce", "");
			this.useHPThresholdOnce.SetDefaultAndSerializedValue(true);
			this.objectActivated = base.AddFlowOutput("objectActivated", "");
			this.objectDeactivated = base.AddFlowOutput("objectDeactivated", "");
			this.damageReceived = base.AddFlowOutput("damageReceived", "");
			this.hitPointsThresholdReached = base.AddFlowOutput("hitPointsThresholdReached", "");
			this.summoned = base.AddFlowOutput("summoned", "");
			this.revived = base.AddFlowOutput("revived", "");
			this.sacrificed = base.AddFlowOutput("sacrificed", "");
			this.killed = base.AddFlowOutput("killed", "");
		}

		// Token: 0x060003FA RID: 1018 RVA: 0x0000DE34 File Offset: 0x0000C034
		protected override void OnInitialize(Flow flow)
		{
			this.targetObjectValue = this.targetObject.value;
			if (this.targetObjectValue != null)
			{
				if (this.targetObjectValue.TryGetComponent<BaseGameMob>(out this.mobComponent))
				{
					IDamageable hitPointsController = this.mobComponent.HitPointsController;
					if (hitPointsController != null)
					{
						hitPointsController.HitPointsChanged += this.OnGameEntityHitPointsChanged;
					}
					if (this.mobComponent.IsRevived)
					{
						this.revived.Call(flow);
					}
					if (this.mobComponent.IsSummoned)
					{
						this.summoned.Call(flow);
					}
					if (this.mobComponent.IsSacrificed)
					{
						this.sacrificed.Call(flow);
					}
					else
					{
						this.mobComponent.Sacrificed += this.OnMobSacrificed;
					}
					if (this.mobComponent.IsKilled)
					{
						this.killed.Call(flow);
					}
					else
					{
						this.mobComponent.Killed += new Action<IGameMob>(this.OnGameEntityKilled);
					}
				}
				else if (this.targetObjectValue.TryGetComponent<IDamageable>(out this.damageReceiver))
				{
					this.damageReceiver.HitPointsChanged += this.OnGameEntityHitPointsChanged;
					this.damageReceiver.TotallyDestroyed += new Action<IDamageable>(this.OnGameEntityKilled);
				}
				else if (this.targetObjectValue.TryGetComponent<IDestroyable>(out this.destructionEventHolder))
				{
					this.destructionEventHolder.Destroyed += this.OnGameEntityKilled;
				}
				this.UpdateObjectActivityState(true);
			}
		}

		// Token: 0x060003FB RID: 1019 RVA: 0x0000DFA9 File Offset: 0x0000C1A9
		protected override void OnFinalize()
		{
			this.UnsubscribeAll();
		}

		// Token: 0x060003FC RID: 1020 RVA: 0x0000DFB1 File Offset: 0x0000C1B1
		public override void OnGraphStoped()
		{
			if (Application.isPlaying)
			{
				return;
			}
			this.UpdateObjectActivityState(false);
		}

		// Token: 0x060003FD RID: 1021 RVA: 0x0000DFC2 File Offset: 0x0000C1C2
		void IUpdatable.Update()
		{
			this.UpdateObjectActivityState(false);
		}

		// Token: 0x04000270 RID: 624
		private ValueInput<GameObject> targetObject;

		// Token: 0x04000271 RID: 625
		private ValueInput<float> targetHPThreshold;

		// Token: 0x04000272 RID: 626
		private ValueInput<bool> useHPThresholdOnce;

		// Token: 0x04000273 RID: 627
		private FlowOutput objectActivated;

		// Token: 0x04000274 RID: 628
		private FlowOutput objectDeactivated;

		// Token: 0x04000275 RID: 629
		private FlowOutput damageReceived;

		// Token: 0x04000276 RID: 630
		private FlowOutput hitPointsThresholdReached;

		// Token: 0x04000277 RID: 631
		private FlowOutput summoned;

		// Token: 0x04000278 RID: 632
		private FlowOutput revived;

		// Token: 0x04000279 RID: 633
		private FlowOutput sacrificed;

		// Token: 0x0400027A RID: 634
		private FlowOutput killed;

		// Token: 0x0400027B RID: 635
		private GameObject targetObjectValue;

		// Token: 0x0400027C RID: 636
		private BaseGameMob mobComponent;

		// Token: 0x0400027D RID: 637
		private IDamageable damageReceiver;

		// Token: 0x0400027E RID: 638
		private IDestroyable destructionEventHolder;

		// Token: 0x0400027F RID: 639
		private bool isHPThresholdReached;

		// Token: 0x04000280 RID: 640
		private bool isHPThresholdEventTotallyReached;

		// Token: 0x04000281 RID: 641
		private bool isObjectActive;
	}
}
