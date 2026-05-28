using System;
using Common;
using UnityEngine;

namespace Unliving.Player
{
	// Token: 0x0200014E RID: 334
	[CreateAssetMenu(fileName = "PlayerGlobalData", menuName = "Game/Data/Player Data")]
	public sealed class PlayerGlobalData : SerializableDataAsset<PlayerBehaviour.ExposedParameters>
	{
	}
}
