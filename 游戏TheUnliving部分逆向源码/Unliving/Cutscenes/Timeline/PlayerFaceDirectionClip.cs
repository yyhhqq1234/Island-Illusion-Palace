using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Playables;
using Unliving.Player;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000337 RID: 823
	public sealed class PlayerFaceDirectionClip : PlayableAsset
	{
		// Token: 0x06001B21 RID: 6945 RVA: 0x00055B58 File Offset: 0x00053D58
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			PlayerFaceDirectionBehaviour playerFaceDirectionBehaviour = new PlayerFaceDirectionBehaviour
			{
				targetPoint = this.targetPoint.Resolve(graph.GetResolver())
			};
			if (Application.isPlaying)
			{
				IGameBehaviour gameBehaviour;
				IPlayerProvider playerProvider;
				if (owner.TryGetComponent<IGameBehaviour>(out gameBehaviour))
				{
					playerProvider = gameBehaviour.CurrentGame.Services.Get<IPlayerProvider>();
				}
				else
				{
					owner.TryGetComponent<IPlayerProvider>(out playerProvider);
				}
				playerFaceDirectionBehaviour.player = playerProvider.CurrentPlayer;
			}
			return ScriptPlayable<PlayerFaceDirectionBehaviour>.Create(graph, playerFaceDirectionBehaviour, 0);
		}

		// Token: 0x04000F43 RID: 3907
		public ExposedReference<Transform> targetPoint;
	}
}
