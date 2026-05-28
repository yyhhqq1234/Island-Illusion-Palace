using System;
using FlowCanvas;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000CB RID: 203
	[Name("Summon Mob", 0)]
	[Category("Unliving/Spawners")]
	public sealed class SummonMobNode : FlowControlNode
	{
		// Token: 0x060004FF RID: 1279 RVA: 0x00012290 File Offset: 0x00010490
		private void SummonMob(Flow flow)
		{
			this.summonedMob = null;
			if (this.summoner.value == null || (this.mobID.value == MobBehaviour.ID.None && this.mobPrefab.value == null))
			{
				return;
			}
			if (this.mobsSummoner == null)
			{
				this.mobsSummoner = new GameMobsSummoner();
			}
			this.mobsSummoner.summonableMobID = this.mobID.value;
			this.mobsSummoner.summonableMobPrefab = this.mobPrefab.value;
			this.mobsSummoner.summonedMobsFactionOverride = this.factionOverride.value;
			this.mobsSummoner.summonedMobsLifetime = this.lifetime.value;
			this.mobsSummoner.summonToIndividualGroups = this.ignoreGroupReviving.value;
			this.mobsSummoner.inheritSummonerLookDirection = this.inheritLookDirection.value;
			this.mobsSummoner.killSummonedMobsWithSummoner = this.killWithSummoner.value;
			GameMobSummoningContext summoningContext = new GameMobSummoningContext
			{
				summoner = this.summoner.value,
				summoningSource = base.graph
			};
			bool flag;
			this.summonedMob = this.mobsSummoner.SummonMob(summoningContext, this.targetPosition.value, out flag, false);
			this.flowOut.Call(flow);
		}

		// Token: 0x06000500 RID: 1280 RVA: 0x000123D4 File Offset: 0x000105D4
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.SummonMob), "");
			this.summoner = base.AddValueInput<BaseGameMob>("summoner", "");
			this.targetPosition = base.AddValueInput<Vector2>("targetPosition", "");
			this.mobID = base.AddValueInput<MobBehaviour.ID>("mobID", "");
			this.mobID.serializedValue = MobBehaviour.ID.None;
			this.mobPrefab = base.AddValueInput<GameObject>("mobPrefab", "");
			this.factionOverride = base.AddValueInput<GameMobFactions>("factionOverride", "");
			this.factionOverride.serializedValue = GameMobFactions.None;
			this.lifetime = base.AddValueInput<float>("lifetime", "");
			this.lifetime.serializedValue = 10f;
			this.ignoreGroupReviving = base.AddValueInput<bool>("ignoreGroupReviving", "");
			this.inheritLookDirection = base.AddValueInput<bool>("inheritLookDirection", "");
			this.killWithSummoner = base.AddValueInput<bool>("killWithSummoner", "");
			this.flowOut = base.AddFlowOutput("", "");
			base.AddValueOutput<IGameMob>("summonedMob", () => this.summonedMob, "");
		}

		// Token: 0x04000367 RID: 871
		private ValueInput<BaseGameMob> summoner;

		// Token: 0x04000368 RID: 872
		private ValueInput<Vector2> targetPosition;

		// Token: 0x04000369 RID: 873
		private ValueInput<MobBehaviour.ID> mobID;

		// Token: 0x0400036A RID: 874
		private ValueInput<GameObject> mobPrefab;

		// Token: 0x0400036B RID: 875
		private ValueInput<GameMobFactions> factionOverride;

		// Token: 0x0400036C RID: 876
		private ValueInput<float> lifetime;

		// Token: 0x0400036D RID: 877
		private ValueInput<bool> ignoreGroupReviving;

		// Token: 0x0400036E RID: 878
		private ValueInput<bool> inheritLookDirection;

		// Token: 0x0400036F RID: 879
		private ValueInput<bool> killWithSummoner;

		// Token: 0x04000370 RID: 880
		private FlowOutput flowOut;

		// Token: 0x04000371 RID: 881
		private GameMobsSummoner mobsSummoner;

		// Token: 0x04000372 RID: 882
		private IGameMob summonedMob;
	}
}
