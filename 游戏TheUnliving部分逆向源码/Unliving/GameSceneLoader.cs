using System;
using System.Collections;
using Game.Core;

namespace Unliving
{
	// Token: 0x02000018 RID: 24
	public class GameSceneLoader : GameBehaviourBase
	{
		// Token: 0x06000127 RID: 295 RVA: 0x00005130 File Offset: 0x00003330
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (!string.IsNullOrEmpty(this.sceneName) && currentGame.Services.TryGet<GameManager>(out this.gameManager))
			{
				if (this.framesDelay == 0)
				{
					this.LoadScene();
					return;
				}
				base.StartCoroutine(this.LoadSceneRoutine());
			}
		}

		// Token: 0x06000128 RID: 296 RVA: 0x00005180 File Offset: 0x00003380
		private void LoadScene()
		{
			this.gameManager.LoadScene(this.sceneName, null);
		}

		// Token: 0x06000129 RID: 297 RVA: 0x00005194 File Offset: 0x00003394
		private IEnumerator LoadSceneRoutine()
		{
			int num;
			for (int i = 0; i < this.framesDelay; i = num + 1)
			{
				yield return null;
				num = i;
			}
			this.LoadScene();
			yield break;
		}

		// Token: 0x04000081 RID: 129
		public string sceneName;

		// Token: 0x04000082 RID: 130
		public int framesDelay;

		// Token: 0x04000083 RID: 131
		private GameManager gameManager;
	}
}
