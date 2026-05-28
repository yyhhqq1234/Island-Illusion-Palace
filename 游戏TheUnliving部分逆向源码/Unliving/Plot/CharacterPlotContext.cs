using System;
using Game.Core;

namespace Unliving.Plot
{
	// Token: 0x020002D4 RID: 724
	public sealed class CharacterPlotContext
	{
		// Token: 0x04000E30 RID: 3632
		public IGame currentGame;

		// Token: 0x04000E31 RID: 3633
		public TotalGamePlotProgressBase totalPlotProgress;

		// Token: 0x04000E32 RID: 3634
		public string characterID;

		// Token: 0x04000E33 RID: 3635
		public ICharacterPlot characterPlot;

		// Token: 0x04000E34 RID: 3636
		public ICharacterPlotProgress characterPlotProgress;
	}
}
