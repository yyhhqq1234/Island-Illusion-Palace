using System;
using FlowCanvas;
using ParadoxNotion.Design;
using Unliving.GameSession.PlayerArmySizeLimiting;
using Unliving.MobsStats;

namespace Unliving.Scripting.FlowCanvas.PlayerArmySizeLimit
{
	// Token: 0x020000CF RID: 207
	[Name("Modify Player Army Size Limit", 0)]
	[Category("Unliving/Player Army Size Limit")]
	public sealed class PlayerArmySizeLimitModificationNode : GameContextDependentNodeBase
	{
		// Token: 0x06000519 RID: 1305 RVA: 0x00012804 File Offset: 0x00010A04
		private void ApplyModifiers(Flow flow)
		{
			IPlayerArmySizeLimitManager playerArmySizeLimitManager;
			if (base.CurrentGame.Services.TryGet<IPlayerArmySizeLimitManager>(out playerArmySizeLimitManager))
			{
				playerArmySizeLimitManager.ModifyMaxArmySize(new MobStatModifier(0f, 0f, this.maxArmySizeCoeff.value));
				playerArmySizeLimitManager.ModifyMobsGainCoeff(new MobStatModifier(0f, 0f, this.mobStatsGainCoeff.value));
			}
			this.flowOut.Call(flow);
		}

		// Token: 0x0600051A RID: 1306 RVA: 0x00012874 File Offset: 0x00010A74
		protected override void RegisterPorts()
		{
			base.AddFlowInput("", new FlowHandler(this.ApplyModifiers), "");
			this.maxArmySizeCoeff = base.AddValueInput<float>("maxArmySizeCoeff", "");
			this.maxArmySizeCoeff.SetDefaultAndSerializedValue(1f);
			this.mobStatsGainCoeff = base.AddValueInput<float>("mobStatsGainCoeff", "");
			this.mobStatsGainCoeff.SetDefaultAndSerializedValue(1f);
			this.flowOut = base.AddFlowOutput("", "");
		}

		// Token: 0x0400037B RID: 891
		private ValueInput<float> maxArmySizeCoeff;

		// Token: 0x0400037C RID: 892
		private ValueInput<float> mobStatsGainCoeff;

		// Token: 0x0400037D RID: 893
		private FlowOutput flowOut;
	}
}
