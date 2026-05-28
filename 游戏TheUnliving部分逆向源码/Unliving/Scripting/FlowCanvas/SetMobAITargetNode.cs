using System;
using System.Linq;
using System.Runtime.CompilerServices;
using FlowCanvas;
using Game.Abilities;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Abilities;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C4 RID: 196
	[Name("Set Mob AI Target", 0)]
	[Category("Unliving/Mobs")]
	public sealed class SetMobAITargetNode : GameContextDependentNodeBase
	{
		// Token: 0x060004E7 RID: 1255 RVA: 0x00011C60 File Offset: 0x0000FE60
		private void SetTarget(Flow flow, bool setTarget)
		{
			if (this.setTarget == setTarget)
			{
				return;
			}
			this.setTarget = setTarget;
			BaseGameMob value = this.targetMob.value;
			if (value == null)
			{
				return;
			}
			GameMobAIController aicontroller = value.AIController;
			if (aicontroller == null)
			{
				return;
			}
			if (setTarget)
			{
				if (this.lastExternalTarget != this.externalTarget.value)
				{
					this.currentTarget = this.externalTarget.value;
				}
				this.lastExternalTarget = this.externalTarget.value;
				IGameMobsFactory gameMobsFactory;
				if (this.currentTarget == null && base.CurrentGame.Services.TryGet<IGameMobsFactory>(out gameMobsFactory))
				{
					int layer = value.gameObject.layer;
					GameMobFactions faction = value.Faction;
					GameMobFactionInfo gameMobFactionInfo = this.setAsAttackTarget.value ? gameMobsFactory.GetEnemyFactionsInfo(layer).First<GameMobFactionInfo>() : gameMobsFactory.GetFactionInfo(faction);
					this.currentTarget = new DummyMob(gameMobFactionInfo.mobsLayer, null)
					{
						Name = string.Format("{0}_dummyMob_{1}", base.graph.name, value.GetInstanceID()),
						Faction = gameMobFactionInfo.faction,
						Radius = 0.1f
					};
					this.updateTargetPosition = true;
				}
				float finalTime = (this.minTargetHoldDuration.value > 0f) ? (Time.time + this.minTargetHoldDuration.value) : -1f;
				this.UpdateTarget(aicontroller, finalTime, this.minTargetHoldAbilitiesUsingCount.value);
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x060004E8 RID: 1256 RVA: 0x00011DE0 File Offset: 0x0000FFE0
		private void UpdateTarget(GameMobAIController aiController, float finalTime, int maxAbilityUsingCount)
		{
			SetMobAITargetNode.<UpdateTarget>d__16 <UpdateTarget>d__;
			<UpdateTarget>d__.<>4__this = this;
			<UpdateTarget>d__.aiController = aiController;
			<UpdateTarget>d__.finalTime = finalTime;
			<UpdateTarget>d__.maxAbilityUsingCount = maxAbilityUsingCount;
			<UpdateTarget>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<UpdateTarget>d__.<>1__state = -1;
			AsyncVoidMethodBuilder <>t__builder = <UpdateTarget>d__.<>t__builder;
			<>t__builder.Start<SetMobAITargetNode.<UpdateTarget>d__16>(ref <UpdateTarget>d__);
		}

		// Token: 0x060004E9 RID: 1257 RVA: 0x00011E31 File Offset: 0x00010031
		private void SetTarget(Flow flow)
		{
			this.SetTarget(flow, true);
		}

		// Token: 0x060004EA RID: 1258 RVA: 0x00011E3B File Offset: 0x0001003B
		private void ResetTarget(Flow flow)
		{
			this.SetTarget(flow, false);
		}

		// Token: 0x060004EB RID: 1259 RVA: 0x00011E48 File Offset: 0x00010048
		protected override void RegisterPorts()
		{
			base.AddFlowInput("SetTarget", new FlowHandler(this.SetTarget), "");
			base.AddFlowInput("ResetTarget", new FlowHandler(this.ResetTarget), "");
			this.targetMob = base.AddValueInput<BaseGameMob>("targetMob", "");
			this.externalTarget = base.AddValueInput<BaseGameMob>("externalTarget", "");
			this.targetPosition = base.AddValueInput<Vector2>("targetPosition", "");
			this.targetPlacementPivot = base.AddValueInput<Transform>("targetPlacementPivot", "");
			this.setAsAttackTarget = base.AddValueInput<bool>("setAsAttackTarget", "");
			this.setAsAttackTarget.SetDefaultAndSerializedValue(true);
			this.minTargetHoldDuration = base.AddValueInput<float>("minTargetHoldDuration", "");
			this.minTargetHoldDuration.SetDefaultAndSerializedValue(-1f);
			this.minTargetHoldAbilitiesUsingCount = base.AddValueInput<int>("minTargetHoldAbilitiesUsingCount", "");
			this.minTargetHoldAbilitiesUsingCount.SetDefaultAndSerializedValue(-1);
			base.AddValueOutput<IGameMob>("currentTarget", () => this.currentTarget, "");
			base.AddValueOutput<BaseGameMob>("targetHoldingMob", () => this.targetHoldingMob, "");
			this.flowOut = base.AddFlowOutput("", "");
			this.targetHoldCompleted = base.AddFlowOutput("targetHoldCompleted", "");
		}

		// Token: 0x060004EC RID: 1260 RVA: 0x00011FBC File Offset: 0x000101BC
		public override void OnGraphStoped()
		{
			this.ResetTarget(default(Flow));
		}

		// Token: 0x04000352 RID: 850
		private ValueInput<BaseGameMob> targetMob;

		// Token: 0x04000353 RID: 851
		private ValueInput<BaseGameMob> externalTarget;

		// Token: 0x04000354 RID: 852
		private ValueInput<Vector2> targetPosition;

		// Token: 0x04000355 RID: 853
		private ValueInput<Transform> targetPlacementPivot;

		// Token: 0x04000356 RID: 854
		private ValueInput<bool> setAsAttackTarget;

		// Token: 0x04000357 RID: 855
		private ValueInput<float> minTargetHoldDuration;

		// Token: 0x04000358 RID: 856
		private ValueInput<int> minTargetHoldAbilitiesUsingCount;

		// Token: 0x04000359 RID: 857
		private FlowOutput flowOut;

		// Token: 0x0400035A RID: 858
		private FlowOutput targetHoldCompleted;

		// Token: 0x0400035B RID: 859
		private BaseGameMob lastExternalTarget;

		// Token: 0x0400035C RID: 860
		private IGameMob currentTarget;

		// Token: 0x0400035D RID: 861
		private bool updateTargetPosition;

		// Token: 0x0400035E RID: 862
		private bool setTarget;

		// Token: 0x0400035F RID: 863
		private BaseGameMob targetHoldingMob;

		// Token: 0x02000421 RID: 1057
		private sealed class AbilityUseCounter
		{
			// Token: 0x06002289 RID: 8841 RVA: 0x0006B684 File Offset: 0x00069884
			private void OnAbilityCompleted(IAbility ability, object usingArgs)
			{
				BaseAbility baseAbility = (BaseAbility)ability;
				if ((this.isBattleAbilityCounter && !baseAbility.IsBattleAbility()) || (!this.isBattleAbilityCounter && !baseAbility.IsSupportAbility()))
				{
					return;
				}
				if (baseAbility.WasUsed)
				{
					this.currentUseCount++;
				}
			}

			// Token: 0x0600228A RID: 8842 RVA: 0x0006B6CF File Offset: 0x000698CF
			public AbilityUseCounter(BaseGameMob targetMob, bool isBattleAbilityCounter, int targetUseCount)
			{
				this.targetMob = targetMob;
				this.isBattleAbilityCounter = isBattleAbilityCounter;
				this.targetUseCount = targetUseCount;
				this.currentUseCount = 0;
				targetMob.AbilitiesController.AbilityCompleted += this.OnAbilityCompleted;
			}

			// Token: 0x0600228B RID: 8843 RVA: 0x0006B70A File Offset: 0x0006990A
			public bool IsUseCountReached()
			{
				return this.targetUseCount > 0 && this.currentUseCount >= this.targetUseCount;
			}

			// Token: 0x0600228C RID: 8844 RVA: 0x0006B728 File Offset: 0x00069928
			public void Release()
			{
				if (this.targetMob != null)
				{
					this.targetMob.AbilitiesController.AbilityCompleted -= this.OnAbilityCompleted;
				}
			}

			// Token: 0x040015EF RID: 5615
			private readonly BaseGameMob targetMob;

			// Token: 0x040015F0 RID: 5616
			private readonly bool isBattleAbilityCounter;

			// Token: 0x040015F1 RID: 5617
			private readonly int targetUseCount;

			// Token: 0x040015F2 RID: 5618
			private int currentUseCount;
		}
	}
}
