using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unliving.Plot.TreeBasedCharacterPlot.Test
{
	// Token: 0x02000309 RID: 777
	[Serializable]
	public sealed class CharacterPlotTreeProgressGenerator
	{
		// Token: 0x06001A50 RID: 6736 RVA: 0x0005237C File Offset: 0x0005057C
		public CharacterPlotTreeProgress CreatePlotProgress()
		{
			CharacterPlotThreadNodeAsset characterPlotThreadNodeAsset = this.currentPlotThreadNode;
			string currentPlotThread = (characterPlotThreadNodeAsset != null) ? characterPlotThreadNodeAsset.Node.ID : null;
			int num = this.currentConversationIndex;
			CharacterPlotThreadNodeAsset[] array = this.completedPlotThreadNodes;
			IEnumerable<string> completedPlotThreads;
			if (array == null)
			{
				completedPlotThreads = null;
			}
			else
			{
				completedPlotThreads = from plotThread in array
				select plotThread.Node.ID;
			}
			return new CharacterPlotTreeProgress(currentPlotThread, num, completedPlotThreads);
		}

		// Token: 0x06001A51 RID: 6737 RVA: 0x000523DC File Offset: 0x000505DC
		public bool IsEmpty()
		{
			return this.currentPlotThreadNode == null && (this.completedPlotThreadNodes == null || this.completedPlotThreadNodes.Length == 0);
		}

		// Token: 0x04000E9B RID: 3739
		[SerializeField]
		private CharacterPlotThreadNodeAsset currentPlotThreadNode;

		// Token: 0x04000E9C RID: 3740
		[SerializeField]
		private int currentConversationIndex;

		// Token: 0x04000E9D RID: 3741
		[SerializeField]
		private CharacterPlotThreadNodeAsset[] completedPlotThreadNodes;
	}
}
