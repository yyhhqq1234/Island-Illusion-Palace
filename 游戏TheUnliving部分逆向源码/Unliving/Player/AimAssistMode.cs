using System;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x02000137 RID: 311
	public class AimAssistMode : AimAssistModeBase
	{
		// Token: 0x1700013B RID: 315
		// (get) Token: 0x060007E0 RID: 2016 RVA: 0x00019F16 File Offset: 0x00018116
		public override AimAssistType AimAssistType
		{
			get
			{
				return AimAssistType.AimAssist;
			}
		}

		// Token: 0x060007E1 RID: 2017 RVA: 0x00019F19 File Offset: 0x00018119
		public AimAssistMode(BaseGameMob owner) : base(owner)
		{
		}

		// Token: 0x060007E2 RID: 2018 RVA: 0x00019F22 File Offset: 0x00018122
		protected override Vector2 GetSearchPosition()
		{
			return this.playerInput.CurrentWorldCursorPosition;
		}
	}
}
