using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unliving.Plot
{
	// Token: 0x020002E5 RID: 741
	[Serializable]
	public class TotalGamePlotProgress<TCharacterPlotProgress> : TotalGamePlotProgressBase where TCharacterPlotProgress : ICharacterPlotProgress, new()
	{
		// Token: 0x06001994 RID: 6548 RVA: 0x000502AC File Offset: 0x0004E4AC
		public TotalGamePlotProgress(IEnumerable<KeyValuePair<string, TCharacterPlotProgress>> data)
		{
			if (data != null)
			{
				foreach (KeyValuePair<string, TCharacterPlotProgress> keyValuePair in data)
				{
					if (!string.IsNullOrEmpty(keyValuePair.Key) && keyValuePair.Value != null)
					{
						this.data.Add(keyValuePair.Key, keyValuePair.Value);
					}
				}
			}
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x00050338 File Offset: 0x0004E538
		public override ICharacterPlotProgress GetCharacterPlotProgress(string characterID)
		{
			TCharacterPlotProgress tcharacterPlotProgress;
			if (this.data.TryGetValue(characterID, out tcharacterPlotProgress))
			{
				return tcharacterPlotProgress;
			}
			tcharacterPlotProgress = Activator.CreateInstance<TCharacterPlotProgress>();
			this.data.Add(characterID, tcharacterPlotProgress);
			return tcharacterPlotProgress;
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x00050375 File Offset: 0x0004E575
		public void UpdateCharacterPlotProgress(string characterID, TCharacterPlotProgress characterPlotProgress)
		{
			this.data[characterID] = characterPlotProgress;
		}

		// Token: 0x04000E55 RID: 3669
		[SerializeField]
		protected Dictionary<string, TCharacterPlotProgress> data = new Dictionary<string, TCharacterPlotProgress>();
	}
}
