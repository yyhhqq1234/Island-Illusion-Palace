using System;
using System.Collections.Generic;
using Common;

namespace Unliving.Player
{
	// Token: 0x02000151 RID: 337
	[Serializable]
	public class PlayerActionsDictionary : SerializableDictionary<PlayerAction, PlayerInputController.InputBehaviour>, ICloneable<PlayerActionsDictionary>
	{
		// Token: 0x06000941 RID: 2369 RVA: 0x0001F5E4 File Offset: 0x0001D7E4
		public PlayerActionsDictionary Clone()
		{
			PlayerActionsDictionary playerActionsDictionary = new PlayerActionsDictionary();
			foreach (KeyValuePair<PlayerAction, PlayerInputController.InputBehaviour> keyValuePair in this)
			{
				playerActionsDictionary.Add(keyValuePair.Key, keyValuePair.Value);
			}
			return playerActionsDictionary;
		}
	}
}
