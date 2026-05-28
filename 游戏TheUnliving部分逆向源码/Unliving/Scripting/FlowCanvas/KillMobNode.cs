using System;
using Common.UnityExtensions;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000A8 RID: 168
	[Name("Kill Mob", 0)]
	[Category("Unliving/Mobs")]
	public sealed class KillMobNode : CallableActionNode<BaseGameMob, UnityEngine.Object>
	{
		// Token: 0x0600045B RID: 1115 RVA: 0x0000F47B File Offset: 0x0000D67B
		public override void Invoke(BaseGameMob mobToKill, UnityEngine.Object mobKiller)
		{
			if (mobToKill.IsNull())
			{
				return;
			}
			mobToKill.KillMob(mobKiller);
		}
	}
}
