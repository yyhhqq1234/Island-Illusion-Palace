using System;
using System.Collections.Generic;
using FlowCanvas;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A7 RID: 167
	[Name("Kill All Mobs", 0)]
	[Category("Unliving/Mobs")]
	public sealed class KillAllMobsNode : GameContextDependentNodeBase
	{
		// Token: 0x06000458 RID: 1112 RVA: 0x0000F3D0 File Offset: 0x0000D5D0
		private async void KillAllMobs(Flow flow)
		{
			GameSessionManager gameSessionManager;
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out gameSessionManager))
			{
				GameSessionRules currentGameRules = gameSessionManager.CurrentGameRules;
				int killPerFrame = this.killPerFrameMobsCount.value;
				bool value = this.killOnlyEnemyMobs.value;
				List<BaseGameMob> mobsToKill = new List<BaseGameMob>();
				foreach (BaseGameMob baseGameMob in gameSessionManager.RegisteredMobs)
				{
					if (!(baseGameMob is PlayerBehaviour) && baseGameMob != null)
					{
						BaseGameMob baseGameMob2 = baseGameMob;
						if (!value || currentGameRules.IsPlayerEnemyFaction(baseGameMob2.Faction))
						{
							mobsToKill.Add(baseGameMob2);
						}
					}
				}
				for (int i = 0; i < mobsToKill.Count; i++)
				{
					if (i % killPerFrame == 0)
					{
						await new WaitForEndOfFrame();
					}
					mobsToKill[i].KillMob(this);
				}
				mobsToKill = null;
			}
		}

		// Token: 0x06000459 RID: 1113 RVA: 0x0000F40C File Offset: 0x0000D60C
		protected override void RegisterPorts()
		{
			base.AddFlowInput(string.Empty, new FlowHandler(this.KillAllMobs), "");
			this.killOnlyEnemyMobs = base.AddValueInput<bool>("killOnlyEnemyMobs", "");
			this.killPerFrameMobsCount = base.AddValueInput<int>("killPerFrameMobsCount", "");
			base.AddFlowOutput(string.Empty, "");
		}

		// Token: 0x040002C5 RID: 709
		private ValueInput<bool> killOnlyEnemyMobs;

		// Token: 0x040002C6 RID: 710
		private ValueInput<int> killPerFrameMobsCount;
	}
}
