using System;
using System.Collections.Generic;
using Common;
using Unliving.Plot;
using Unliving.Plot.TreeBasedCharacterPlot;

namespace Unliving.PlayerProfileManagement
{
	// Token: 0x02000123 RID: 291
	[Serializable]
	public sealed class PlayerProfilePlotProgress : TotalGamePlotProgress<CharacterPlotTreeProgress>, ICloneable<PlayerProfilePlotProgress>
	{
		// Token: 0x06000758 RID: 1880 RVA: 0x00017873 File Offset: 0x00015A73
		public PlayerProfilePlotProgress() : base(null)
		{
		}

		// Token: 0x06000759 RID: 1881 RVA: 0x0001787C File Offset: 0x00015A7C
		public PlayerProfilePlotProgress(IEnumerable<KeyValuePair<string, CharacterPlotTreeProgress>> data) : base(data)
		{
		}

		// Token: 0x0600075A RID: 1882 RVA: 0x00017885 File Offset: 0x00015A85
		public PlayerProfilePlotProgress Clone()
		{
			return new PlayerProfilePlotProgress(this.data);
		}
	}
}
