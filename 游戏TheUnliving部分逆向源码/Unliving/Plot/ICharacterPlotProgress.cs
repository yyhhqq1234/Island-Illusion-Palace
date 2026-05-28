using System;

namespace Unliving.Plot
{
	// Token: 0x020002DF RID: 735
	public interface ICharacterPlotProgress
	{
		// Token: 0x06001973 RID: 6515
		bool IsCompletedPlotItem(ICharacterPlotItemArgs args);

		// Token: 0x06001974 RID: 6516
		bool IsPlotItemInProgress(ICharacterPlotItemArgs args);

		// Token: 0x06001975 RID: 6517
		void Update(ICharacterPlotItemArgs args);
	}
}
