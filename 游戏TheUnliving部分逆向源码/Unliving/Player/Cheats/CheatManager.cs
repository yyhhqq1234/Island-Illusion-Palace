using System;
using System.Collections.Generic;
using Common.Editor;
using Common.ServiceRegistry;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.PlayerProfileManagement;

namespace Unliving.Player.Cheats
{
	// Token: 0x0200017A RID: 378
	[CreateAssetMenu(fileName = "CheatManager", menuName = "Game/Cheat Manager")]
	public sealed class CheatManager : GlobalManagerBase
	{
		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06000A74 RID: 2676 RVA: 0x000227CF File Offset: 0x000209CF
		public IReadOnlyList<CheatBase> AvailableCheats
		{
			get
			{
				return this.availableCheats;
			}
		}

		// Token: 0x06000A75 RID: 2677 RVA: 0x000227D8 File Offset: 0x000209D8
		private int GetCheatIndex(string cheatID)
		{
			for (int i = 0; i < this.availableCheats.Length; i++)
			{
				if (this.availableCheats[i].HasID(cheatID))
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x06000A76 RID: 2678 RVA: 0x0002280B File Offset: 0x00020A0B
		private int GetCheatIndex(CheatBase cheat)
		{
			return Array.IndexOf<CheatBase>(this.availableCheats, cheat);
		}

		// Token: 0x06000A77 RID: 2679 RVA: 0x00022819 File Offset: 0x00020A19
		private void SelectCheat(int cheatIndex)
		{
			if (cheatIndex >= 0 && !this.selectedCheats.Contains(cheatIndex))
			{
				this.selectedCheats.Add(cheatIndex);
			}
		}

		// Token: 0x06000A78 RID: 2680 RVA: 0x00022839 File Offset: 0x00020A39
		private void UnselectCheat(int cheatIndex)
		{
			if (cheatIndex >= 0 && this.selectedCheats.Contains(cheatIndex))
			{
				this.selectedCheats.Remove(cheatIndex);
			}
		}

		// Token: 0x06000A79 RID: 2681 RVA: 0x0002285C File Offset: 0x00020A5C
		private void InitializeContext(PlayerBehaviour player)
		{
			this.context.Reset();
			this.context.currentPlayer = player;
			IServiceRegistry services = base.CurrentGame.Services;
			if (this.profileManager != null)
			{
				PlayerProfile currentPlayerProfile = this.profileManager.CurrentPlayerProfile;
				this.context.playerProfile = currentPlayerProfile;
			}
			IGameManager gameManager;
			if (services.TryGet<IGameManager>(out gameManager))
			{
				this.context.isHomespace = gameManager.IsHomespaceLoaded;
				this.context.isNewGame = (gameManager.IsNewGameRun && gameManager.IsStartSceneLoaded);
			}
			IGameLocationProvider gameLocationProvider;
			if (services.TryGet<IGameLocationProvider>(out gameLocationProvider))
			{
				this.context.locationID = gameLocationProvider.LocationType;
				CheatContext cheatContext = this.context;
				GameLocation currentLocation = gameLocationProvider.CurrentLocation;
				cheatContext.isTutorial = (currentLocation != null && currentLocation.IsTutorialLocation);
			}
		}

		// Token: 0x06000A7A RID: 2682 RVA: 0x00022920 File Offset: 0x00020B20
		private void ActivateCheats(PlayerBehaviour player)
		{
			this.InitializeContext(player);
			if (player != null)
			{
				foreach (int num in this.selectedCheats)
				{
					CheatBase cheatBase = this.availableCheats[num];
					if (cheatBase != null)
					{
						cheatBase.SetActive(this.context, true);
					}
				}
			}
		}

		// Token: 0x06000A7B RID: 2683 RVA: 0x00022998 File Offset: 0x00020B98
		private void ResetPlayerProvider()
		{
			if (this.playerProvider == null)
			{
				return;
			}
			this.playerProvider.PlayerRegistered -= this.ActivateCheats;
			this.playerProvider = null;
		}

		// Token: 0x06000A7C RID: 2684 RVA: 0x000229C4 File Offset: 0x00020BC4
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (currentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				PlayerProfile currentPlayerProfile = this.profileManager.CurrentPlayerProfile;
				if (currentPlayerProfile != null)
				{
					this.OnPlayerProfileLoaded(currentPlayerProfile);
				}
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
			}
			GameApplication.SceneLoadingStarted += this.OnSceneTransitionStarted;
		}

		// Token: 0x06000A7D RID: 2685 RVA: 0x00022A29 File Offset: 0x00020C29
		public bool IsSelected(CheatBase cheat)
		{
			return this.selectedCheats.Contains(this.GetCheatIndex(cheat));
		}

		// Token: 0x06000A7E RID: 2686 RVA: 0x00022A3D File Offset: 0x00020C3D
		public bool IsSelected(string cheatID)
		{
			return this.selectedCheats.Contains(this.GetCheatIndex(cheatID));
		}

		// Token: 0x06000A7F RID: 2687 RVA: 0x00022A51 File Offset: 0x00020C51
		public void SelectCheat(CheatBase cheat)
		{
			this.SelectCheat(this.GetCheatIndex(cheat));
		}

		// Token: 0x06000A80 RID: 2688 RVA: 0x00022A60 File Offset: 0x00020C60
		public void SelectCheat(string cheatID)
		{
			this.SelectCheat(this.GetCheatIndex(cheatID));
		}

		// Token: 0x06000A81 RID: 2689 RVA: 0x00022A6F File Offset: 0x00020C6F
		public void UnselectCheat(CheatBase cheat)
		{
			this.UnselectCheat(this.GetCheatIndex(cheat));
		}

		// Token: 0x06000A82 RID: 2690 RVA: 0x00022A80 File Offset: 0x00020C80
		public void SetSelectedCheats(IEnumerable<string> newSelectedCheats)
		{
			this.selectedCheats.Clear();
			if (newSelectedCheats == null)
			{
				return;
			}
			foreach (string cheatID in newSelectedCheats)
			{
				this.SelectCheat(cheatID);
			}
		}

		// Token: 0x06000A83 RID: 2691 RVA: 0x00022AD8 File Offset: 0x00020CD8
		public IEnumerable<CheatBase> GetSelectedCheats()
		{
			foreach (int num in this.selectedCheats)
			{
				yield return this.availableCheats[num];
			}
			List<int>.Enumerator enumerator = default(List<int>.Enumerator);
			yield break;
			yield break;
		}

		// Token: 0x06000A84 RID: 2692 RVA: 0x00022AE8 File Offset: 0x00020CE8
		private void OnPlayerProfileLoaded(PlayerProfile playerProfile)
		{
			this.profileManager.LoadSelectedCheats(this);
		}

		// Token: 0x06000A85 RID: 2693 RVA: 0x00022AF8 File Offset: 0x00020CF8
		private void OnSceneTransitionStarted()
		{
			foreach (int num in this.selectedCheats)
			{
				CheatBase cheatBase = this.availableCheats[num];
				if (cheatBase != null)
				{
					cheatBase.SetActive(this.context, false);
				}
			}
			this.ResetPlayerProvider();
		}

		// Token: 0x06000A86 RID: 2694 RVA: 0x00022B64 File Offset: 0x00020D64
		protected override void OnSceneLoaded(Scene scene)
		{
			if (base.CurrentGame.Services.TryGet<IPlayerProvider>(out this.playerProvider))
			{
				PlayerBehaviour currentPlayer = this.playerProvider.CurrentPlayer;
				if (currentPlayer != null)
				{
					this.ActivateCheats(currentPlayer);
					return;
				}
				this.playerProvider.PlayerRegistered += this.ActivateCheats;
			}
		}

		// Token: 0x06000A87 RID: 2695 RVA: 0x00022BC0 File Offset: 0x00020DC0
		protected override void OnDestroy()
		{
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
			GameApplication.SceneLoadingStarted -= this.OnSceneTransitionStarted;
			this.ResetPlayerProvider();
			base.OnDestroy();
		}

		// Token: 0x04000619 RID: 1561
		[SerializeField]
		[SerializeReference]
		[ManagedObjectField(typeof(CheatBase))]
		private CheatBase[] availableCheats;

		// Token: 0x0400061A RID: 1562
		private readonly List<int> selectedCheats = new List<int>(8);

		// Token: 0x0400061B RID: 1563
		private readonly CheatContext context = new CheatContext();

		// Token: 0x0400061C RID: 1564
		private PlayerProfileManager profileManager;

		// Token: 0x0400061D RID: 1565
		private IPlayerProvider playerProvider;
	}
}
