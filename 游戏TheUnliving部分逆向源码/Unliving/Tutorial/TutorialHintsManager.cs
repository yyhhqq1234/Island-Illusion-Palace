using System;
using Common;
using Common.CollectionsExtensions;
using Common.Editor;
using Common.RestorableState;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.GameScene;
using Unliving.Mobs.ActivationModifiers;
using Unliving.Player;
using Unliving.PlayerProfileManagement;

namespace Unliving.Tutorial
{
	// Token: 0x02000033 RID: 51
	[CreateAssetMenu(fileName = "TutorialHintsManager", menuName = "Game/Tutorial/Hints Manager")]
	public sealed class TutorialHintsManager : GlobalManagerBase
	{
		// Token: 0x1400001D RID: 29
		// (add) Token: 0x060001B4 RID: 436 RVA: 0x00006F68 File Offset: 0x00005168
		// (remove) Token: 0x060001B5 RID: 437 RVA: 0x00006FA0 File Offset: 0x000051A0
		public event Action<ITutorialHint> TutorialHintTriggersReached;

		// Token: 0x060001B6 RID: 438 RVA: 0x00006FD8 File Offset: 0x000051D8
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (!this.isActive)
			{
				return;
			}
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
			for (int i = 0; i < this.hints.Length; i++)
			{
				ITutorialHint tutorialHint = this.hints[i];
				if (tutorialHint != null)
				{
					tutorialHint.HintTriggersReached += this.OnHintTriggersReached;
				}
			}
		}

		// Token: 0x060001B7 RID: 439 RVA: 0x00007066 File Offset: 0x00005266
		private void OnPlayerProfileLoaded(PlayerProfile obj)
		{
			this.profileManager.LoadTutorialHintsState(this);
		}

		// Token: 0x060001B8 RID: 440 RVA: 0x00007074 File Offset: 0x00005274
		protected override void OnSceneLoaded(Scene scene)
		{
			base.OnSceneLoaded(scene);
			if (!this.isActive)
			{
				return;
			}
			IPlayerProvider playerProvider;
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider))
			{
				playerProvider.PlayerRegistered += this.OnPlayerRegistered;
			}
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out this.sceneManager))
			{
				this.sceneManager.InvokeAfterLocationGenerated(new Action<GameSceneManager>(this.OnLocationGenerated));
			}
			for (int i = 0; i < this.hints.Length; i++)
			{
				ITutorialHint tutorialHint = this.hints[i];
				if (tutorialHint != null)
				{
					tutorialHint.OnSceneLoaded(base.CurrentGame);
				}
			}
			if (this.profileManager != null)
			{
				this.profileManager.UpdateTutorialHintsState(this);
			}
		}

		// Token: 0x060001B9 RID: 441 RVA: 0x0000712C File Offset: 0x0000532C
		private void OnLocationGenerated(GameSceneManager sceneManager)
		{
			this.isTutorialLocation = sceneManager.GeneratedLocation.IsTutorialLocation;
		}

		// Token: 0x060001BA RID: 442 RVA: 0x00007140 File Offset: 0x00005340
		private void OnPlayerRegistered(PlayerBehaviour player)
		{
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkEntered -= this.OnPlayerLocationChunkEntered;
				this.currentPlayer.ActivationModifiersController.ModifierAdded -= this.OnModifierAdded;
			}
			this.currentPlayer = player;
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkEntered += this.OnPlayerLocationChunkEntered;
				this.currentPlayer.ActivationModifiersController.ModifierAdded += this.OnModifierAdded;
			}
		}

		// Token: 0x060001BB RID: 443 RVA: 0x000071D4 File Offset: 0x000053D4
		private void OnModifierAdded(MobsActivationModifiersController controller, int slot)
		{
			this.UpdateHintsState();
		}

		// Token: 0x060001BC RID: 444 RVA: 0x000071DC File Offset: 0x000053DC
		private void OnPlayerLocationChunkEntered(ILocationChunk oldChunk, ILocationChunk targetChunk)
		{
			this.UpdateHintsState();
		}

		// Token: 0x060001BD RID: 445 RVA: 0x000071E4 File Offset: 0x000053E4
		private void UpdateHintsState()
		{
			if (this.isTutorialLocation)
			{
				return;
			}
			for (int i = 0; i < this.hints.Length; i++)
			{
				ITutorialHint tutorialHint = this.hints[i];
				if (tutorialHint != null)
				{
					tutorialHint.UpdateState();
				}
			}
		}

		// Token: 0x060001BE RID: 446 RVA: 0x00007220 File Offset: 0x00005420
		private void OnHintTriggersReached(ITutorialHint hint)
		{
			Action<ITutorialHint> tutorialHintTriggersReached = this.TutorialHintTriggersReached;
			if (tutorialHintTriggersReached == null)
			{
				return;
			}
			tutorialHintTriggersReached(hint);
		}

		// Token: 0x060001BF RID: 447 RVA: 0x00007234 File Offset: 0x00005434
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.currentPlayer.IsNull())
			{
				this.currentPlayer.LocationChunkEntered -= this.OnPlayerLocationChunkEntered;
				this.currentPlayer.ActivationModifiersController.ModifierAdded -= this.OnModifierAdded;
			}
		}

		// Token: 0x040000E6 RID: 230
		public bool isActive;

		// Token: 0x040000E7 RID: 231
		[SerializeReference]
		[ManagedObjectField(typeof(ITutorialHint))]
		public ITutorialHint[] hints;

		// Token: 0x040000E8 RID: 232
		private PlayerBehaviour currentPlayer;

		// Token: 0x040000E9 RID: 233
		private GameSceneManager sceneManager;

		// Token: 0x040000EA RID: 234
		private bool isTutorialLocation;

		// Token: 0x040000EB RID: 235
		private PlayerProfileManager profileManager;

		// Token: 0x0200040B RID: 1035
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<TutorialHintsManager>, ICloneable<TutorialHintsManager.RestorableState>
		{
			// Token: 0x0600223F RID: 8767 RVA: 0x0006A6D9 File Offset: 0x000688D9
			public RestorableState() : base(null)
			{
			}

			// Token: 0x06002240 RID: 8768 RVA: 0x0006A6EE File Offset: 0x000688EE
			public RestorableState(TutorialHintsManager tutorialHintsManager) : base(tutorialHintsManager)
			{
			}

			// Token: 0x06002241 RID: 8769 RVA: 0x0006A703 File Offset: 0x00068903
			public TutorialHintsManager.RestorableState Clone()
			{
				return new TutorialHintsManager.RestorableState
				{
					hintsData = this.hintsData.CloneArray<TutorialHintSerializationData>()
				};
			}

			// Token: 0x06002242 RID: 8770 RVA: 0x0006A71C File Offset: 0x0006891C
			public override void Restore(TutorialHintsManager targetObject, object args = null)
			{
				if (this.hintsData.Length == 0)
				{
					return;
				}
				foreach (ITutorialHint tutorialHint in targetObject.hints)
				{
					for (int j = 0; j < this.hintsData.Length; j++)
					{
						TutorialHintSerializationData tutorialHintSerializationData = this.hintsData[j];
						if (string.Equals(tutorialHint.ID, tutorialHintSerializationData.id))
						{
							tutorialHint.SetSerializationData(tutorialHintSerializationData);
							break;
						}
					}
				}
			}

			// Token: 0x06002243 RID: 8771 RVA: 0x0006A78C File Offset: 0x0006898C
			public override void Store(TutorialHintsManager targetObject)
			{
				ITutorialHint[] hints = targetObject.hints;
				this.hintsData = new TutorialHintSerializationData[hints.Length];
				for (int i = 0; i < hints.Length; i++)
				{
					this.hintsData[i] = hints[i].GetSerializationData();
				}
			}

			// Token: 0x040015AD RID: 5549
			public TutorialHintSerializationData[] hintsData = new TutorialHintSerializationData[0];
		}
	}
}
