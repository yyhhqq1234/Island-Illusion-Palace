using System;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200013A RID: 314
	public class AutoTargetMode : AimAssistModeBase
	{
		// Token: 0x1700013E RID: 318
		// (get) Token: 0x060007F0 RID: 2032 RVA: 0x0001A21E File Offset: 0x0001841E
		public override AimAssistType AimAssistType
		{
			get
			{
				return AimAssistType.AutoTarget;
			}
		}

		// Token: 0x060007F1 RID: 2033 RVA: 0x0001A221 File Offset: 0x00018421
		public AutoTargetMode(BaseGameMob owner) : base(owner)
		{
		}

		// Token: 0x060007F2 RID: 2034 RVA: 0x0001A22A File Offset: 0x0001842A
		protected override Vector2 GetSearchPosition()
		{
			return this.owner.Position;
		}
	}
}
