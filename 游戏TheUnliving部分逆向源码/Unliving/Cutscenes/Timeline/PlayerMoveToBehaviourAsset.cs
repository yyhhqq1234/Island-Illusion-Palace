using System;
using Game.Core;
using UnityEngine;
using UnityEngine.Playables;
using Unliving.Player;
using Unliving.Scripting;

namespace Unliving.Cutscenes.Timeline
{
	// Token: 0x02000339 RID: 825
	public sealed class PlayerMoveToBehaviourAsset : PlayableAsset
	{
		// Token: 0x06001B27 RID: 6951 RVA: 0x00055E18 File Offset: 0x00054018
		private static async void SetPlayer(IGameBehaviour context, PlayerMoveToBehaviour behaviour)
		{
			PlayerBehaviour playerBehaviour;
			if (context.CurrentGame.TryGetPlayer(out playerBehaviour))
			{
				behaviour.player = playerBehaviour;
			}
			else
			{
				playerBehaviour = await context.CurrentGame.GetPlayerAsync();
				if (!(playerBehaviour == null) && Application.isPlaying && !GameApplication.IsGameStateChanging)
				{
					behaviour.player = playerBehaviour;
				}
			}
		}

		// Token: 0x06001B28 RID: 6952 RVA: 0x00055E5C File Offset: 0x0005405C
		public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
		{
			Transform exists = this.targetPoint.Resolve(graph.GetResolver());
			if (!exists)
			{
				PlayableDirector playableDirector = graph.GetResolver() as PlayableDirector;
				if (playableDirector.gameObject)
				{
					string name = playableDirector.gameObject.name;
				}
			}
			Transform lookToward = this.lookAtPoint.Resolve(graph.GetResolver());
			PlayerMoveToBehaviour playerMoveToBehaviour = new PlayerMoveToBehaviour
			{
				currentGraph = graph,
				targetPoint = exists,
				lookToward = lookToward,
				pauseTimelineExecution = this.pauseTimelineExecution
			};
			if (Application.isPlaying)
			{
				IGameBehaviour gameBehaviour;
				if (!owner.TryGetComponent<IGameBehaviour>(out gameBehaviour) || gameBehaviour.CurrentGame == null)
				{
					gameBehaviour = owner.AddComponent<GameContextProvider>();
				}
				PlayerMoveToBehaviourAsset.SetPlayer(gameBehaviour, playerMoveToBehaviour);
			}
			return ScriptPlayable<PlayerMoveToBehaviour>.Create(graph, playerMoveToBehaviour, 0);
		}

		// Token: 0x04000F4C RID: 3916
		public ExposedReference<Transform> targetPoint;

		// Token: 0x04000F4D RID: 3917
		[Tooltip("By the end of the move, the character will look at this object")]
		public ExposedReference<Transform> lookAtPoint;

		// Token: 0x04000F4E RID: 3918
		public bool pauseTimelineExecution = true;
	}
}
