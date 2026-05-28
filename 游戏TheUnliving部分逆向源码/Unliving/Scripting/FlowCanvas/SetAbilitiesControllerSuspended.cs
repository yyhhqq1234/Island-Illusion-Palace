using System;
using FlowCanvas.Nodes;
using ParadoxNotion.Design;
using Unliving.Mobs;
using Unliving.Player;

namespace Unliving.Scripting.FlowCanvas
{
	// Token: 0x020000C3 RID: 195
	[Name("Set Abilities Controller Suspended", 0)]
	[Category("Unliving/Mobs")]
	public sealed class SetAbilitiesControllerSuspended : CallableActionNode<BaseGameMob>
	{
		// Token: 0x060004E5 RID: 1253 RVA: 0x00011C2C File Offset: 0x0000FE2C
		public override void Invoke(BaseGameMob targetMob)
		{
			if (targetMob == null)
			{
				return;
			}
			IPlayerAbilitiesController playerAbilitiesController = targetMob.AbilitiesController as IPlayerAbilitiesController;
			if (playerAbilitiesController != null)
			{
				playerAbilitiesController.SetSuspended();
			}
		}
	}
}
