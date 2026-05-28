using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B6 RID: 182
	[Name("Mobs Group Events", 0)]
	[Category("Unliving/Mobs")]
	public sealed class MobsGroupEventBusNode : ObjectEventBusNodeBase
	{
		// Token: 0x060004A1 RID: 1185 RVA: 0x0001083E File Offset: 0x0000EA3E
		protected override GameObject GetEventsSourceObject()
		{
			GameMobsGroupControllerBase gameMobsGroupControllerBase = this.currentGroup;
			if (gameMobsGroupControllerBase == null)
			{
				return null;
			}
			return gameMobsGroupControllerBase.GroupHolder;
		}

		// Token: 0x060004A2 RID: 1186 RVA: 0x00010854 File Offset: 0x0000EA54
		private void OnGroupMobAdded(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			this.addedMob = mob;
			this.mobAdded.Call(default(Flow));
		}

		// Token: 0x060004A3 RID: 1187 RVA: 0x0001087C File Offset: 0x0000EA7C
		private void OnGroupMobRemoved(GameMobsGroupControllerBase group, BaseGameMob mob)
		{
			Flow f = default(Flow);
			this.removedMob = mob;
			this.mobRemoved.Call(f);
			if (!group.HasMobs)
			{
				this.allMobsRemoved.Call(f);
			}
		}

		// Token: 0x060004A4 RID: 1188 RVA: 0x000108B8 File Offset: 0x0000EAB8
		private void OnGroupDestinationReached(GameMobGroupController group)
		{
			this.destinationReached.Call(default(Flow));
		}

		// Token: 0x060004A5 RID: 1189 RVA: 0x000108DC File Offset: 0x0000EADC
		private void OnTotalGroupHitPointsSumChanged(GameMobGroupController group, float lastHP, float currentHP)
		{
			float value = this.targetHPThreshold.value;
			if (!this.isHPThresholdEventTotallyReached && value > 0f)
			{
				bool flag = currentHP / group.MaxGroupCharactersHitPointsSum < Mathf.Clamp01(this.targetHPThreshold.value);
				if (this.isHPThresholdReached != flag)
				{
					if (flag)
					{
						this.totalHPThresholdReached.Call(default(Flow));
						this.isHPThresholdEventTotallyReached = this.useHPThresholdOnce.value;
					}
					this.isHPThresholdReached = flag;
				}
			}
			if (lastHP - currentHP > 0f)
			{
				this.totalHPChanged.Call(default(Flow));
			}
		}

		// Token: 0x060004A6 RID: 1190 RVA: 0x00010978 File Offset: 0x0000EB78
		private void OnGroupBattleStateChanged(GameMobsGroupControllerBase group, bool isBattleActive)
		{
			Flow f = default(Flow);
			if (isBattleActive)
			{
				this.battleStarted.Call(f);
				return;
			}
			this.battleCompleted.Call(f);
		}

		// Token: 0x060004A7 RID: 1191 RVA: 0x000109AC File Offset: 0x0000EBAC
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.groupObject = base.AddValueInput<GameObject>("groupObject", "");
			this.groupMob = base.AddValueInput<BaseGameMob>("groupMob", "");
			this.targetHPThreshold = base.AddValueInput<float>("targetHPThreshold", "");
			this.useHPThresholdOnce = base.AddValueInput<bool>("useHPThresholdOnce", "");
			this.useHPThresholdOnce.SetDefaultAndSerializedValue(true);
			this.mobAdded = base.AddFlowOutput("mobAdded", "");
			this.mobRemoved = base.AddFlowOutput("mobRemoved", "");
			this.destinationReached = base.AddFlowOutput("destinationReached", "");
			this.totalHPChanged = base.AddFlowOutput("totalHPChanged", "");
			this.totalHPThresholdReached = base.AddFlowOutput("totalHPThresholdReached", "");
			this.battleStarted = base.AddFlowOutput("battleStarted", "");
			this.battleCompleted = base.AddFlowOutput("battleCompleted", "");
			this.allMobsRemoved = base.AddFlowOutput("allMobsRemoved", "");
			base.AddValueOutput<GameMobsGroupControllerBase>("Current Group", () => this.currentGroup, "");
			base.AddValueOutput<int>("Mobs Count ", delegate()
			{
				if (this.currentGroup == null)
				{
					return 0;
				}
				return this.currentGroup.Mobs.Count;
			}, "");
			base.AddValueOutput<BaseGameMob>("Last Added Mob", () => this.addedMob, "");
			base.AddValueOutput<BaseGameMob>("Last Removed Mob", () => this.removedMob, "");
		}

		// Token: 0x060004A8 RID: 1192 RVA: 0x00010B48 File Offset: 0x0000ED48
		protected override void OnInitialize(Flow flow)
		{
			GameObject value = this.groupObject.value;
			BaseGameMob value2 = this.groupMob.value;
			if (value2 != null)
			{
				this.currentGroup = value2.Group;
				this.<OnInitialize>g__CheckGroup|24_0();
			}
			else if (value != null)
			{
				IGameMobGroupControllerProvider component = value.GetComponent<IGameMobGroupControllerProvider>();
				this.currentGroup = ((component != null) ? component.GroupController : null);
				MobBehaviourSpawner mobBehaviourSpawner;
				if (this.currentGroup == null && value.TryGetComponent<MobBehaviourSpawner>(out mobBehaviourSpawner))
				{
					this.currentGroup = mobBehaviourSpawner.GetOrCreateMobsGroup(null);
				}
				this.<OnInitialize>g__CheckGroup|24_0();
			}
			if (this.currentGroup != null)
			{
				IReadOnlyList<BaseGameMob> mobs = this.currentGroup.Mobs;
				for (int i = 0; i < mobs.Count; i++)
				{
					this.OnGroupMobAdded(this.currentGroup, mobs[i]);
				}
				this.currentGroup.MobAdded += this.OnGroupMobAdded;
				this.currentGroup.MobRemoved += this.OnGroupMobRemoved;
				GameMobGroupController gameMobGroupController = this.currentGroup as GameMobGroupController;
				if (gameMobGroupController != null)
				{
					gameMobGroupController.GroupDestinationReached += this.OnGroupDestinationReached;
					gameMobGroupController.GroupCharactersHitPointsSumChanged += this.OnTotalGroupHitPointsSumChanged;
					gameMobGroupController.BattleStateChanged += new Action<GameMobGroupController, bool>(this.OnGroupBattleStateChanged);
				}
			}
		}

		// Token: 0x060004A9 RID: 1193 RVA: 0x00010C88 File Offset: 0x0000EE88
		protected override void OnFinalize()
		{
			if (this.currentGroup == null)
			{
				return;
			}
			this.currentGroup.MobAdded -= this.OnGroupMobAdded;
			this.currentGroup.MobRemoved -= this.OnGroupMobRemoved;
			GameMobGroupController gameMobGroupController = this.currentGroup as GameMobGroupController;
			if (gameMobGroupController != null)
			{
				gameMobGroupController.GroupDestinationReached -= this.OnGroupDestinationReached;
				gameMobGroupController.GroupCharactersHitPointsSumChanged -= this.OnTotalGroupHitPointsSumChanged;
				gameMobGroupController.BattleStateChanged -= new Action<GameMobGroupController, bool>(this.OnGroupBattleStateChanged);
			}
		}

		// Token: 0x060004AF RID: 1199 RVA: 0x00010D4D File Offset: 0x0000EF4D
		[CompilerGenerated]
		private void <OnInitialize>g__CheckGroup|24_0()
		{
			GameMobsGroupControllerBase gameMobsGroupControllerBase = this.currentGroup;
		}

		// Token: 0x0400030C RID: 780
		private ValueInput<GameObject> groupObject;

		// Token: 0x0400030D RID: 781
		private ValueInput<BaseGameMob> groupMob;

		// Token: 0x0400030E RID: 782
		private ValueInput<float> targetHPThreshold;

		// Token: 0x0400030F RID: 783
		private ValueInput<bool> useHPThresholdOnce;

		// Token: 0x04000310 RID: 784
		private FlowOutput mobAdded;

		// Token: 0x04000311 RID: 785
		private FlowOutput mobRemoved;

		// Token: 0x04000312 RID: 786
		private FlowOutput destinationReached;

		// Token: 0x04000313 RID: 787
		private FlowOutput totalHPChanged;

		// Token: 0x04000314 RID: 788
		private FlowOutput totalHPThresholdReached;

		// Token: 0x04000315 RID: 789
		private FlowOutput battleStarted;

		// Token: 0x04000316 RID: 790
		private FlowOutput battleCompleted;

		// Token: 0x04000317 RID: 791
		private FlowOutput allMobsRemoved;

		// Token: 0x04000318 RID: 792
		private GameMobsGroupControllerBase currentGroup;

		// Token: 0x04000319 RID: 793
		private BaseGameMob addedMob;

		// Token: 0x0400031A RID: 794
		private BaseGameMob removedMob;

		// Token: 0x0400031B RID: 795
		private bool isHPThresholdReached;

		// Token: 0x0400031C RID: 796
		private bool isHPThresholdEventTotallyReached;
	}
}
