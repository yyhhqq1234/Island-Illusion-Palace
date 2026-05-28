using System;
using Game.Core;
using UnityEngine;
using Unliving.GameScene;

namespace Unliving.Misc
{
	// Token: 0x02000242 RID: 578
	public class ParallaxRenderer : GameBehaviourBase
	{
		// Token: 0x06001398 RID: 5016 RVA: 0x0003D714 File Offset: 0x0003B914
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			GameSceneManager gameSceneManager;
			if (currentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				gameSceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
		}

		// Token: 0x06001399 RID: 5017 RVA: 0x0003D74C File Offset: 0x0003B94C
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			Bounds bounds = sceneManager.GeneratedLocation.Bounds;
			base.transform.position = bounds.center;
			for (int i = 0; i < this.sprites.Length; i++)
			{
				SpriteRenderer spriteRenderer = this.sprites[i];
				spriteRenderer.size = bounds.size * this.locationSizeMult;
				spriteRenderer.enabled = true;
			}
		}

		// Token: 0x04000B6E RID: 2926
		public SpriteRenderer[] sprites;

		// Token: 0x04000B6F RID: 2927
		[Tooltip("Какого размера должны быть спрайты паралакса, относительно размера локации")]
		public float locationSizeMult = 1.2f;
	}
}
