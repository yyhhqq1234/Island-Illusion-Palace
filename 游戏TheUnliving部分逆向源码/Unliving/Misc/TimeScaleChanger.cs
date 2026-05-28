using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x02000249 RID: 585
	public sealed class TimeScaleChanger : GameBehaviourBase
	{
		// Token: 0x060013A8 RID: 5032 RVA: 0x0003D972 File Offset: 0x0003BB72
		public void ResetTimeScale()
		{
			IGame currentGame = base.CurrentGame;
			if (currentGame == null)
			{
				return;
			}
			currentGame.ResetTimeScale();
		}

		// Token: 0x060013A9 RID: 5033 RVA: 0x0003D984 File Offset: 0x0003BB84
		public void SetTimeScale(float timeScale)
		{
			Time.timeScale = timeScale;
		}
	}
}
