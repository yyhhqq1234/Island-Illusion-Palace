using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Collections;
using Common.RestorableState;
using Common.ServiceRegistry;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unliving.PlayerProfileManagement;
using Unliving.Purchasing;

namespace Unliving.Plot.Milestones
{
	// Token: 0x02000315 RID: 789
	[CreateAssetMenu(fileName = "PlotMilestoneManager", menuName = "Game/Plot/Milestone Manager")]
	[Service(typeof(PlotMilestoneManager), new Type[]
	{
		typeof(IPlotMilestoneManager)
	})]
	public sealed class PlotMilestoneManager : GlobalManagerBase, IPlotMilestoneManager
	{
		// Token: 0x14000102 RID: 258
		// (add) Token: 0x06001A80 RID: 6784 RVA: 0x00052C3C File Offset: 0x00050E3C
		// (remove) Token: 0x06001A81 RID: 6785 RVA: 0x00052C74 File Offset: 0x00050E74
		public event Action<PlotMilestoneNode> MilestoneReached;

		// Token: 0x1700058E RID: 1422
		// (get) Token: 0x06001A82 RID: 6786 RVA: 0x00052CA9 File Offset: 0x00050EA9
		public IReadOnlyList<PlotMilestoneNode> Milestones
		{
			get
			{
				return new ReadOnlyList<PlotMilestoneNode>(this.milestones, -1);
			}
		}

		// Token: 0x1700058F RID: 1423
		// (get) Token: 0x06001A83 RID: 6787 RVA: 0x00052CB7 File Offset: 0x00050EB7
		public IReadOnlyList<string> ReachedMilestones
		{
			get
			{
				return this.reachedMilestones;
			}
		}

		// Token: 0x17000590 RID: 1424
		// (get) Token: 0x06001A84 RID: 6788 RVA: 0x00052CBF File Offset: 0x00050EBF
		public IReadOnlyList<string> CurrentGameSessionReachedMilestones
		{
			get
			{
				return this.currentGameSessionReachedMilestones;
			}
		}

		// Token: 0x06001A85 RID: 6789 RVA: 0x00052CC8 File Offset: 0x00050EC8
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			if (base.CurrentGame.Services.TryGet<IGameManager>(out this.gameManager))
			{
				this.gameManager.GameStarted += this.OnGameStarted;
			}
			if (base.CurrentGame.Services.TryGet<PlayerProfileManager>(out this.profileManager))
			{
				this.profileManager.ProfileLoaded += this.OnPlayerProfileLoaded;
				this.OnPlayerProfileLoaded(this.profileManager.CurrentPlayerProfile);
			}
			if (currentGame.Services.TryGet<ICharacterConversationManager>(out this.conversationManager))
			{
				this.conversationManager.ConversationCompleted += new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
			if (base.CurrentGame.Services.TryGet<PurchaseManager>(out this.purchaseManager))
			{
				this.purchaseManager.ItemPurchased += this.OnItemPurchased;
			}
		}

		// Token: 0x06001A86 RID: 6790 RVA: 0x00052DA4 File Offset: 0x00050FA4
		private void OnGameStarted(IGameManager obj)
		{
			if (this.gameManager.IsNewGameRun)
			{
				this.currentGameSessionReachedMilestones.Clear();
			}
		}

		// Token: 0x06001A87 RID: 6791 RVA: 0x00052DC0 File Offset: 0x00050FC0
		private void OnPlayerProfileLoaded(PlayerProfile profile)
		{
			this.milestones = new PlotMilestoneNode[this.graph.nodes.Count];
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.graph.nodes[i] as PlotMilestoneNode;
				if (plotMilestoneNode != null)
				{
					this.milestones[i] = plotMilestoneNode.Clone();
				}
			}
			this.profileManager.LoadPlayerMilestones(this);
		}

		// Token: 0x06001A88 RID: 6792 RVA: 0x00052E2F File Offset: 0x0005102F
		protected override void OnSceneLoaded(Scene loadedScene)
		{
			base.OnSceneLoaded(loadedScene);
			if (base.CurrentGame.Services.TryGet<IGameSessionManager>(out this.gameSessionManager))
			{
				this.gameSessionManager.SessionStateChanged += this.OnGameSessionStateChanged;
			}
			this.UpdateMilestonesState();
		}

		// Token: 0x06001A89 RID: 6793 RVA: 0x00052E6D File Offset: 0x0005106D
		private void OnGameSessionStateChanged(IGameSessionManager sessionManager, SessionState sessionState)
		{
			if (sessionState == SessionState.Victory || sessionState == SessionState.VictoryCutscene || sessionState == SessionState.Defeat || sessionState == SessionState.Exited)
			{
				this.UpdateMilestonesState();
			}
		}

		// Token: 0x06001A8A RID: 6794 RVA: 0x00052E85 File Offset: 0x00051085
		private void OnConversationCompleted(IPlotCharacter character, ICharacterPlotItem plotItem)
		{
			this.UpdateMilestonesState();
		}

		// Token: 0x06001A8B RID: 6795 RVA: 0x00052E8D File Offset: 0x0005108D
		private void OnItemPurchased(IPurchasable purchasable)
		{
			this.UpdateMilestonesState();
		}

		// Token: 0x06001A8C RID: 6796 RVA: 0x00052E98 File Offset: 0x00051098
		public void UpdateMilestonesState()
		{
			bool flag = false;
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.milestones[i];
				if (!this.reachedMilestones.Contains(plotMilestoneNode.milestoneID) && plotMilestoneNode.IsMilestoneReached(base.CurrentGame))
				{
					this.SetMilestoneReachedInternal(plotMilestoneNode);
					flag = true;
				}
			}
			if (flag)
			{
				this.profileManager.UpdatePlayerMilestones(this);
			}
		}

		// Token: 0x06001A8D RID: 6797 RVA: 0x00052EFC File Offset: 0x000510FC
		public void SetMilestoneReached(string milestoneID)
		{
			if (this.reachedMilestones.Contains(milestoneID))
			{
				return;
			}
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.milestones[i];
				if (string.Equals(plotMilestoneNode.milestoneID, milestoneID))
				{
					this.SetMilestoneReachedInternal(plotMilestoneNode);
					return;
				}
			}
		}

		// Token: 0x06001A8E RID: 6798 RVA: 0x00052F4C File Offset: 0x0005114C
		private void SetMilestoneReachedInternal(PlotMilestoneNode milestone)
		{
			string milestoneID = milestone.milestoneID;
			this.reachedMilestones.Add(milestoneID);
			this.currentGameSessionReachedMilestones.Add(milestoneID);
			Action<PlotMilestoneNode> milestoneReached = this.MilestoneReached;
			if (milestoneReached == null)
			{
				return;
			}
			milestoneReached(milestone);
		}

		// Token: 0x06001A8F RID: 6799 RVA: 0x00052F8C File Offset: 0x0005118C
		public bool HasMilestoneWithID(string milestoneID)
		{
			for (int i = 0; i < this.milestones.Length; i++)
			{
				if (string.Equals(this.milestones[i].milestoneID, milestoneID))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001A90 RID: 6800 RVA: 0x00052FC4 File Offset: 0x000511C4
		public bool IsMilestoneReached(string milestoneID)
		{
			if (string.IsNullOrWhiteSpace(milestoneID))
			{
				return true;
			}
			if (!this.HasMilestoneWithID(milestoneID))
			{
				Debug.LogError("Milestone with ID: " + milestoneID + " is missing. Have you spelled it correctly?");
				return false;
			}
			if (this.reachedMilestones.Contains(milestoneID))
			{
				return true;
			}
			PlotMilestoneNode plotMilestoneNode;
			this.CheckMilestoneState(milestoneID, out plotMilestoneNode);
			return this.reachedMilestones.Contains(milestoneID);
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x00053020 File Offset: 0x00051220
		public void CheckMilestoneState(string milestoneID, out PlotMilestoneNode milestoneNode)
		{
			milestoneNode = null;
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.milestones[i];
				if (plotMilestoneNode.milestoneID == milestoneID)
				{
					if (!this.reachedMilestones.Contains(milestoneID) && plotMilestoneNode.IsMilestoneReached(base.CurrentGame))
					{
						this.SetMilestoneReachedInternal(plotMilestoneNode);
					}
					milestoneNode = plotMilestoneNode;
					return;
				}
			}
		}

		// Token: 0x06001A92 RID: 6802 RVA: 0x00053084 File Offset: 0x00051284
		public IReadOnlyList<string> GetAchievementMilestones(bool onlyNotReached)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.milestones[i];
				if (plotMilestoneNode.achievementMilestone)
				{
					if (onlyNotReached)
					{
						if (!this.reachedMilestones.Contains(plotMilestoneNode.milestoneID) && !plotMilestoneNode.IsMilestoneReached(base.CurrentGame))
						{
							list.Add(plotMilestoneNode.milestoneID);
						}
					}
					else
					{
						list.Add(plotMilestoneNode.milestoneID);
					}
				}
			}
			return list;
		}

		// Token: 0x06001A93 RID: 6803 RVA: 0x000530FC File Offset: 0x000512FC
		public bool TryGetMilestoneIconSprite(string milestoneID, out Sprite sprite)
		{
			for (int i = 0; i < this.milestones.Length; i++)
			{
				PlotMilestoneNode plotMilestoneNode = this.milestones[i];
				if (plotMilestoneNode.milestoneID == milestoneID)
				{
					sprite = plotMilestoneNode.iconSprite;
					return !sprite.IsNull();
				}
			}
			sprite = null;
			return false;
		}

		// Token: 0x06001A94 RID: 6804 RVA: 0x0005314C File Offset: 0x0005134C
		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (!this.purchaseManager.IsNull())
			{
				this.purchaseManager.ItemPurchased -= this.OnItemPurchased;
			}
			if (!this.conversationManager.IsNull())
			{
				this.conversationManager.ConversationCompleted -= new Action<IPlotCharacter, ICharactersConversation>(this.OnConversationCompleted);
			}
			if (this.profileManager != null)
			{
				this.profileManager.ProfileLoaded -= this.OnPlayerProfileLoaded;
			}
			if (this.gameManager != null)
			{
				this.gameManager.GameStarted -= this.OnGameStarted;
			}
		}

		// Token: 0x04000EC0 RID: 3776
		public PlotMilestoneManagerGraph graph;

		// Token: 0x04000EC1 RID: 3777
		private IGameSessionManager gameSessionManager;

		// Token: 0x04000EC2 RID: 3778
		private PurchaseManager purchaseManager;

		// Token: 0x04000EC3 RID: 3779
		private List<string> reachedMilestones = new List<string>();

		// Token: 0x04000EC4 RID: 3780
		private List<string> currentGameSessionReachedMilestones = new List<string>();

		// Token: 0x04000EC5 RID: 3781
		private PlotMilestoneNode[] milestones;

		// Token: 0x04000EC6 RID: 3782
		private PlayerProfileManager profileManager;

		// Token: 0x04000EC7 RID: 3783
		private ICharacterConversationManager conversationManager;

		// Token: 0x04000EC8 RID: 3784
		private IGameManager gameManager;

		// Token: 0x02000549 RID: 1353
		[Serializable]
		public sealed class RestorableState : RestorableStateBase<PlotMilestoneManager>, ICloneable<PlotMilestoneManager.RestorableState>
		{
			// Token: 0x060026AE RID: 9902 RVA: 0x000786EF File Offset: 0x000768EF
			public RestorableState() : base(null)
			{
			}

			// Token: 0x060026AF RID: 9903 RVA: 0x0007870E File Offset: 0x0007690E
			public override void Restore(PlotMilestoneManager targetObject, object args = null)
			{
				targetObject.reachedMilestones = this.reachedMilestones;
				targetObject.currentGameSessionReachedMilestones = this.currentGameSessionReachedMilestones;
			}

			// Token: 0x060026B0 RID: 9904 RVA: 0x00078728 File Offset: 0x00076928
			public override void Store(PlotMilestoneManager targetObject)
			{
				this.reachedMilestones = targetObject.reachedMilestones;
				this.currentGameSessionReachedMilestones = targetObject.currentGameSessionReachedMilestones;
			}

			// Token: 0x060026B1 RID: 9905 RVA: 0x00078742 File Offset: 0x00076942
			public PlotMilestoneManager.RestorableState Clone()
			{
				return new PlotMilestoneManager.RestorableState
				{
					reachedMilestones = this.reachedMilestones.ToList<string>(),
					currentGameSessionReachedMilestones = this.currentGameSessionReachedMilestones.ToList<string>()
				};
			}

			// Token: 0x04001BA4 RID: 7076
			public List<string> reachedMilestones = new List<string>();

			// Token: 0x04001BA5 RID: 7077
			public List<string> currentGameSessionReachedMilestones = new List<string>();
		}
	}
}
