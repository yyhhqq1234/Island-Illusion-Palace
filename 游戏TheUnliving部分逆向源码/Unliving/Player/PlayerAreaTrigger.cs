using System;
using Common.Scene;
using Common.UnityExtensions;
using Game.Utility;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Player
{
	// Token: 0x0200014B RID: 331
	public sealed class PlayerAreaTrigger : AreaTrigger
	{
		// Token: 0x06000900 RID: 2304 RVA: 0x0001E39F File Offset: 0x0001C59F
		private Vector2 GetMobSpawningPosition()
		{
			return base.transform.position + this.mobSpawningOffset;
		}

		// Token: 0x06000901 RID: 2305 RVA: 0x0001E3BC File Offset: 0x0001C5BC
		protected override bool IsObjectValid(GameObject obj)
		{
			if (this.sessionManager != null)
			{
				return this.sessionManager.CurrentPlayer != null && this.sessionManager.CurrentPlayer.gameObject == obj;
			}
			return !obj.GetComponent<PlayerBehaviour>().IsNull();
		}

		// Token: 0x06000902 RID: 2306 RVA: 0x0001E411 File Offset: 0x0001C611
		public void SetActive(bool isActive)
		{
			base.gameObject.SetActive(isActive);
		}

		// Token: 0x06000903 RID: 2307 RVA: 0x0001E420 File Offset: 0x0001C620
		protected override void OnActivated(GameObject activator)
		{
			base.OnActivated(activator);
			if (this.nextScene.IsValid())
			{
				this.sessionManager.LoadScene(this.nextScene);
				return;
			}
			if (this.isSceneTransitionArea)
			{
				this.sessionManager.TryRestartCurrentGame(false);
				return;
			}
			if (this.mobToSpawn != MobBehaviour.ID.None && this.mobsFactory != null)
			{
				this.mobsFactory.Create(new MobBehaviour.FactoryArgs
				{
					mobID = this.mobToSpawn,
					spawnPosition = this.GetMobSpawningPosition()
				});
			}
		}

		// Token: 0x06000904 RID: 2308 RVA: 0x0001E4AC File Offset: 0x0001C6AC
		private void Start()
		{
			base.CurrentGame.Services.TryGet<IGameMobsFactory>(out this.mobsFactory);
			if (base.CurrentGame.Services.TryGet<GameSessionManager>(out this.sessionManager) && this.isSceneTransitionArea)
			{
				this.sessionManager.AddPlayerTransitionArea(this);
			}
		}

		// Token: 0x06000905 RID: 2309 RVA: 0x0001E4FC File Offset: 0x0001C6FC
		private void OnDrawGizmosSelected()
		{
			if (this.mobToSpawn != MobBehaviour.ID.None)
			{
				Vector2 mobSpawningPosition = this.GetMobSpawningPosition();
				Gizmos.color = Color.green;
				Gizmos.DrawLine(base.transform.position, mobSpawningPosition);
				Gizmos.DrawWireSphere(mobSpawningPosition, 0.1f);
			}
		}

		// Token: 0x04000504 RID: 1284
		[Space]
		public SceneAssetReference nextScene;

		// Token: 0x04000505 RID: 1285
		public MobBehaviour.ID mobToSpawn;

		// Token: 0x04000506 RID: 1286
		public Vector2 mobSpawningOffset;

		// Token: 0x04000507 RID: 1287
		public bool isSceneTransitionArea = true;

		// Token: 0x04000508 RID: 1288
		private GameSessionManager sessionManager;

		// Token: 0x04000509 RID: 1289
		private IGameMobsFactory mobsFactory;
	}
}
