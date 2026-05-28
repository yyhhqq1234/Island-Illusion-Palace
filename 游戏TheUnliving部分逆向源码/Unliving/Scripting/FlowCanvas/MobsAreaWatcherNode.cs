using System;
using System.Collections.Generic;
using FlowCanvas;
using Game.Utility;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000B5 RID: 181
	[Name("Mobs Area Watcher", 0)]
	[Category("Unliving/Mobs")]
	public sealed class MobsAreaWatcherNode : ObjectEventBusNodeBase
	{
		// Token: 0x06000491 RID: 1169 RVA: 0x000101F0 File Offset: 0x0000E3F0
		private void CountMob(BaseGameMob mob, bool isEntered)
		{
			Flow f = default(Flow);
			int value = this.targetMobsCount.value;
			float value2 = this.targetMobsGroupRatio.value;
			bool flag = this.isPlayerInRange;
			if (isEntered)
			{
				this.mobsCount++;
				this.lastEnteredMob = mob;
				if (!this.isPlayerInRange && mob is PlayerBehaviour)
				{
					flag = true;
				}
			}
			else
			{
				this.mobsCount--;
				this.lastExitedMob = mob;
				if (this.isPlayerInRange && mob is PlayerBehaviour)
				{
					flag = false;
				}
			}
			this.mobsGroupRatio = Mathf.Clamp01((float)this.mobsCount / (float)mob.Group.Mobs.Count);
			if (isEntered)
			{
				this.mobEntered.Call(f);
			}
			else
			{
				this.mobExited.Call(f);
			}
			if (this.isPlayerInRange != flag)
			{
				if (this.isPlayerInRange = flag)
				{
					this.playerEntered.Call(f);
				}
				else
				{
					this.playerExited.Call(f);
				}
			}
			if (value >= 0)
			{
				bool flag2 = this.mobsCount >= value;
				if (flag2 != this.isMobsCountReached && (this.isMobsCountReached = flag2))
				{
					this.mobsCountReached.Call(f);
				}
			}
			if (value2 >= 0f && mob.Group != null)
			{
				bool flag3 = this.mobsGroupRatio >= value2;
				if (flag3 != this.isMobsGroupRatioReached && (this.isMobsGroupRatioReached = flag3))
				{
					this.mobsGroupRatioReached.Call(f);
				}
			}
		}

		// Token: 0x06000492 RID: 1170 RVA: 0x00010363 File Offset: 0x0000E563
		protected override GameObject GetEventsSourceObject()
		{
			return this.currentWatcher.gameObject;
		}

		// Token: 0x06000493 RID: 1171 RVA: 0x00010370 File Offset: 0x0000E570
		private void OnMobEnteredArea(BaseGameMob mob)
		{
			this.CountMob(mob, true);
		}

		// Token: 0x06000494 RID: 1172 RVA: 0x0001037A File Offset: 0x0000E57A
		private void OnMobExitedArea(BaseGameMob mob)
		{
			this.CountMob(mob, false);
		}

		// Token: 0x06000495 RID: 1173 RVA: 0x00010384 File Offset: 0x0000E584
		protected override void RegisterPorts()
		{
			base.RegisterPorts();
			this.externalMobsWatcher = base.AddValueInput<AreaWatcher<BaseGameMob>>("externalMobsWatcher", "");
			this.watchableAreaPivot = base.AddValueInput<Transform>("watchableAreaPivot", "");
			this.watchableMobsLayers = base.AddValueInput<LayerMask>("watchableMobsLayers", "");
			this.watchableMobsDescription = base.AddValueInput<GameMobDescription>("watchableMobsDescription", "");
			this.watchableMobsDescription.SetDefaultAndSerializedValue(GameMobDescription.BlankDescription);
			this.watchableAreaRange = base.AddValueInput<float>("watchableAreaRange", "");
			this.watchableAreaRange.SetDefaultAndSerializedValue(2f);
			this.watchableAreaPosition = base.AddValueInput<Vector2>("watchableAreaPosition", "");
			this.targetMobsCount = base.AddValueInput<int>("targetMobsCount", "");
			this.targetMobsCount.SetDefaultAndSerializedValue(-1);
			this.targetMobsGroupRatio = base.AddValueInput<float>("targetMobsGroupRatio", "");
			this.mobEntered = base.AddFlowOutput("mobEntered", "");
			this.mobExited = base.AddFlowOutput("mobExited", "");
			this.playerEntered = base.AddFlowOutput("playerEntered", "");
			this.playerExited = base.AddFlowOutput("playerExited", "");
			this.mobsCountReached = base.AddFlowOutput("mobsCountReached", "");
			this.mobsGroupRatioReached = base.AddFlowOutput("mobsGroupRatioReached", "");
			base.AddValueOutput<AreaWatcher<BaseGameMob>>("currentWatcher", () => this.currentWatcher, "");
			base.AddValueOutput<BaseGameMob>("lastEnteredMob", () => this.lastEnteredMob, "");
			base.AddValueOutput<BaseGameMob>("lastExitedMob", () => this.lastExitedMob, "");
			base.AddValueOutput<int>("mobsCount", () => this.mobsCount, "");
			base.AddValueOutput<float>("mobsGroupRatio", () => this.mobsGroupRatio, "");
			base.AddValueOutput<bool>("isPlayerInRange", () => this.isPlayerInRange, "");
			base.AddValueOutput<bool>("isMobsCountReached", () => this.isMobsCountReached, "");
			base.AddValueOutput<bool>("isMobsGroupRatioReached", () => this.isMobsGroupRatioReached, "");
		}

		// Token: 0x06000496 RID: 1174 RVA: 0x000105E4 File Offset: 0x0000E7E4
		protected override void OnInitialize(Flow flow)
		{
			AreaWatcher<BaseGameMob> value = this.externalMobsWatcher.value;
			float value2 = this.watchableAreaRange.value;
			this.watchableMobsDescriptionValue = this.watchableMobsDescription.value;
			if (value != null)
			{
				this.currentWatcher = value;
			}
			else if (value2 > 0f)
			{
				GameMobAreaObserver gameMobAreaObserver = new GameObject(base.graph.name + "_mobsWatcher").AddComponent<GameMobAreaObserver>();
				CircleCollider2D circleCollider2D = gameMobAreaObserver.gameObject.AddComponent<CircleCollider2D>();
				circleCollider2D.radius = value2;
				gameMobAreaObserver.AreaCollider = circleCollider2D;
				gameMobAreaObserver.isContinuousObjectValidationEnabled = false;
				gameMobAreaObserver.CurrentObjectSelectionMethod = GameMobAreaObserver.ObjectSelectionMethod.None;
				gameMobAreaObserver.AdditionalObjectValidatorType = GameMobAreaObserver.AdditionalObjectValidatorTypes.None;
				gameMobAreaObserver.observableLayers = this.watchableMobsLayers.value;
				Transform value3 = this.watchableAreaPivot.value;
				if (value3 != null)
				{
					if (gameMobAreaObserver.transform != value3)
					{
						gameMobAreaObserver.transform.parent = value3;
						gameMobAreaObserver.transform.localPosition = default(Vector3);
						gameMobAreaObserver.gameObject.AddComponent<Rigidbody2D>().isKinematic = true;
					}
				}
				else
				{
					gameMobAreaObserver.transform.position = this.watchableAreaPosition.value;
				}
				this.currentWatcher = gameMobAreaObserver;
			}
			if (this.currentWatcher != null)
			{
				if (!this.watchableMobsDescriptionValue.IsBlank())
				{
					this.currentWatcher.AdditionalObjectValidator = new Predicate<BaseGameMob>(this.watchableMobsDescriptionValue.IsMatch);
				}
				IReadOnlyList<BaseGameMob> objectsInRange = this.currentWatcher.ObjectsInRange;
				for (int i = 0; i < objectsInRange.Count; i++)
				{
					this.CountMob(objectsInRange[i], true);
				}
				this.currentWatcher.ObjectEnteredArea += this.OnMobEnteredArea;
				this.currentWatcher.ObjectExitedArea += this.OnMobExitedArea;
			}
		}

		// Token: 0x06000497 RID: 1175 RVA: 0x000107B7 File Offset: 0x0000E9B7
		protected override void OnFinalize()
		{
			if (this.currentWatcher == null)
			{
				return;
			}
			this.currentWatcher.ObjectEnteredArea -= this.OnMobEnteredArea;
			this.currentWatcher.ObjectExitedArea -= this.OnMobExitedArea;
		}

		// Token: 0x040002F5 RID: 757
		private ValueInput<AreaWatcher<BaseGameMob>> externalMobsWatcher;

		// Token: 0x040002F6 RID: 758
		private ValueInput<LayerMask> watchableMobsLayers;

		// Token: 0x040002F7 RID: 759
		private ValueInput<GameMobDescription> watchableMobsDescription;

		// Token: 0x040002F8 RID: 760
		private ValueInput<float> watchableAreaRange;

		// Token: 0x040002F9 RID: 761
		private ValueInput<Transform> watchableAreaPivot;

		// Token: 0x040002FA RID: 762
		private ValueInput<Vector2> watchableAreaPosition;

		// Token: 0x040002FB RID: 763
		private ValueInput<int> targetMobsCount;

		// Token: 0x040002FC RID: 764
		private ValueInput<float> targetMobsGroupRatio;

		// Token: 0x040002FD RID: 765
		private FlowOutput mobEntered;

		// Token: 0x040002FE RID: 766
		private FlowOutput mobExited;

		// Token: 0x040002FF RID: 767
		private FlowOutput playerEntered;

		// Token: 0x04000300 RID: 768
		private FlowOutput playerExited;

		// Token: 0x04000301 RID: 769
		private FlowOutput mobsCountReached;

		// Token: 0x04000302 RID: 770
		private FlowOutput mobsGroupRatioReached;

		// Token: 0x04000303 RID: 771
		private AreaWatcher<BaseGameMob> currentWatcher;

		// Token: 0x04000304 RID: 772
		private GameMobDescription watchableMobsDescriptionValue;

		// Token: 0x04000305 RID: 773
		private BaseGameMob lastEnteredMob;

		// Token: 0x04000306 RID: 774
		private BaseGameMob lastExitedMob;

		// Token: 0x04000307 RID: 775
		private int mobsCount;

		// Token: 0x04000308 RID: 776
		private float mobsGroupRatio;

		// Token: 0x04000309 RID: 777
		private bool isPlayerInRange;

		// Token: 0x0400030A RID: 778
		private bool isMobsCountReached;

		// Token: 0x0400030B RID: 779
		private bool isMobsGroupRatioReached;
	}
}
