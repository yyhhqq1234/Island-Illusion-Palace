using System;
using System.Collections.Generic;
using System.Linq;
using Common.UnityExtensions;
using Game.Core;
using Game.LevelGeneration;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using Unliving.GameScene;
using Unliving.LevelGeneration;
using Unliving.Pickables;
using Unliving.Plot;
using Unliving.Plot.Milestones;
using Unliving.Plot.TreeBasedCharacterPlot;
using Unliving.Plot.TreeBasedCharacterPlot.Test;

namespace Unliving.Interactables
{
	// Token: 0x0200029C RID: 668
	public abstract class NPCControllerBase<TObjectID> : PickableObjectBase<TObjectID>, ILocationChunkVisitor, ILocationObject, IPlotCharacter where TObjectID : Enum
	{
		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x060016F1 RID: 5873 RVA: 0x00049482 File Offset: 0x00047682
		public int? LocationObjectType
		{
			get
			{
				return new int?((int)this.locationObjectType);
			}
		}

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x060016F2 RID: 5874 RVA: 0x0004948F File Offset: 0x0004768F
		// (set) Token: 0x060016F3 RID: 5875 RVA: 0x00049497 File Offset: 0x00047697
		public ILocationChunk CurrentLocationChunk { get; set; }

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x060016F4 RID: 5876 RVA: 0x000494A0 File Offset: 0x000476A0
		public float Orientation
		{
			get
			{
				return base.transform.GetRotation2D(false);
			}
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x060016F5 RID: 5877 RVA: 0x000494AE File Offset: 0x000476AE
		public bool AffectLocationChunkVisibility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x060016F6 RID: 5878 RVA: 0x000494B1 File Offset: 0x000476B1
		// (set) Token: 0x060016F7 RID: 5879 RVA: 0x000494C3 File Offset: 0x000476C3
		public Vector2 Position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x060016F8 RID: 5880 RVA: 0x000494D6 File Offset: 0x000476D6
		public virtual bool IsDynamicLocationObject
		{
			get
			{
				return false;
			}
		}

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x060016F9 RID: 5881 RVA: 0x000494D9 File Offset: 0x000476D9
		public string CharacterID
		{
			get
			{
				CharacterPlotNodeGraph characterPlotNodeGraph = this.characterPlotGraph;
				if (characterPlotNodeGraph == null)
				{
					return null;
				}
				return characterPlotNodeGraph.characterID;
			}
		}

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x060016FA RID: 5882 RVA: 0x000494EC File Offset: 0x000476EC
		public IReadOnlyList<ConversationBranch> AvailableConversationBranches
		{
			get
			{
				return this.availableConversationBranches;
			}
		}

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x060016FB RID: 5883 RVA: 0x000494F4 File Offset: 0x000476F4
		public ICharacterPlot CharacterPlot
		{
			get
			{
				return this.characterPlotGraph;
			}
		}

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x060016FC RID: 5884 RVA: 0x000494FC File Offset: 0x000476FC
		// (set) Token: 0x060016FD RID: 5885 RVA: 0x00049504 File Offset: 0x00047704
		public ICharactersConversation PreparedConversation { get; set; }

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x060016FE RID: 5886 RVA: 0x0004950D File Offset: 0x0004770D
		public ICharactersConversation CurrentConversation
		{
			get
			{
				return this.currentConversation;
			}
		}

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x060016FF RID: 5887 RVA: 0x00049515 File Offset: 0x00047715
		private bool HasPlot
		{
			get
			{
				return this.characterPlotGraph != null;
			}
		}

		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x06001700 RID: 5888 RVA: 0x00049523 File Offset: 0x00047723
		public ICharacterPlotProgress PlotProgressOverride
		{
			get
			{
				return this.plotProgressOverride;
			}
		}

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06001701 RID: 5889 RVA: 0x0004952B File Offset: 0x0004772B
		protected override string LocalizationID
		{
			get
			{
				return "char_name_" + this.characterPlotGraph.characterID;
			}
		}

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06001702 RID: 5890 RVA: 0x00049542 File Offset: 0x00047742
		public UnityEvent OnConversationPreparedEvent
		{
			get
			{
				return this.onConversationPreparedEvent;
			}
		}

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x06001703 RID: 5891 RVA: 0x0004954A File Offset: 0x0004774A
		public UnityEvent OnConversationStartedEvent
		{
			get
			{
				return this.onConversationStartedEvent;
			}
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x06001704 RID: 5892 RVA: 0x00049552 File Offset: 0x00047752
		public UnityEvent OnConversationCompletedEvent
		{
			get
			{
				return this.onConversationCompletedEvent;
			}
		}

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x06001705 RID: 5893 RVA: 0x0004955A File Offset: 0x0004775A
		public UnityEvent<CharacterPhrase> OnPhraseStartedEvent
		{
			get
			{
				return this.onPhraseStartedEvent;
			}
		}

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x06001706 RID: 5894 RVA: 0x00049562 File Offset: 0x00047762
		public UnityEvent<CharacterPhrase> OnPhraseCompletedEvent
		{
			get
			{
				return this.onPhraseCompletedEvent;
			}
		}

		// Token: 0x06001707 RID: 5895 RVA: 0x0004956A File Offset: 0x0004776A
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.availableConversationBranches = this.conversationBranches.ToList<ConversationBranch>();
		}

		// Token: 0x06001708 RID: 5896 RVA: 0x00049584 File Offset: 0x00047784
		private void PlayRandomAnimation()
		{
			AnimationClip animationClip = this.randomAnimations[UnityEngine.Random.Range(0, this.randomAnimations.Length)];
			if (animationClip != null)
			{
				this.anim.Play(animationClip.name);
			}
			this.ResetTime();
		}

		// Token: 0x06001709 RID: 5897 RVA: 0x000495C7 File Offset: 0x000477C7
		private void ResetTime()
		{
			this.nextRandomAnimTime = Time.time + UnityEngine.Random.Range(this.minRandomAnimInterval, this.maxRandomAnimInterval);
		}

		// Token: 0x0600170A RID: 5898 RVA: 0x000495E6 File Offset: 0x000477E6
		public void CompleteConversation()
		{
			if (this.currentConversation == null)
			{
				return;
			}
			this.conversationManager.CompleteConversation(this, this.currentConversation);
		}

		// Token: 0x0600170B RID: 5899 RVA: 0x00049603 File Offset: 0x00047803
		public override bool CanBePickedUp(IPickableObjectCollector targetCollector, PickingArgs args, out PickingUpErrorType error)
		{
			if (this.IsPurchasable)
			{
				return base.CanBePickedUp(targetCollector, args, out error);
			}
			error = PickingUpErrorType.None;
			return targetCollector == this.PointerEventsSender;
		}

		// Token: 0x0600170C RID: 5900 RVA: 0x00049623 File Offset: 0x00047823
		protected override void PerformPickingUp(IPickableObjectCollector targetCollector)
		{
			if (this.PreparedConversation == null)
			{
				this.OnConversationCompleted(this, null);
				return;
			}
			this.StartConversation();
		}

		// Token: 0x0600170D RID: 5901 RVA: 0x0004963D File Offset: 0x0004783D
		public override bool CanBeUsedByCollector(IPickableObjectCollector targetCollector)
		{
			return !this.HasPlot || (this.PreparedConversation != null && this.currentConversation == null);
		}

		// Token: 0x0600170E RID: 5902 RVA: 0x0004965C File Offset: 0x0004785C
		public bool StartConversation(out ICharactersConversation conversation)
		{
			conversation = null;
			if (!this.HasPlot)
			{
				this.OnConversationCompleted(this, null);
				return false;
			}
			if (this.PreparedConversation != null)
			{
				ICharacterConversationManager characterConversationManager = this.conversationManager;
				this.currentConversation = ((characterConversationManager != null) ? characterConversationManager.StartConversation(this) : null);
				if (this.currentConversation != null)
				{
					this.isConversationAlreadyStarted = true;
					conversation = this.currentConversation;
					conversation.PhraseStarted += this.OnConversationPhraseStarted;
					conversation.PhraseCompleted += this.OnConversationPhraseCompleted;
					this.OnConversationStarted(this.currentConversation);
					UnityEvent unityEvent = this.onConversationStartedEvent;
					if (unityEvent != null)
					{
						unityEvent.Invoke();
					}
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600170F RID: 5903 RVA: 0x000496FC File Offset: 0x000478FC
		public bool StartConversation()
		{
			ICharactersConversation charactersConversation;
			return this.StartConversation(out charactersConversation);
		}

		// Token: 0x06001710 RID: 5904 RVA: 0x00049711 File Offset: 0x00047911
		bool ILocationChunkVisitor.CanVisitLocationChunk(ILocationChunk locationChunk)
		{
			return locationChunk != null;
		}

		// Token: 0x06001711 RID: 5905 RVA: 0x00049717 File Offset: 0x00047917
		void ILocationChunkVisitor.OnAddedToLocationChunk(ILocationChunk locationChunk)
		{
		}

		// Token: 0x06001712 RID: 5906 RVA: 0x00049719 File Offset: 0x00047919
		void ILocationChunkVisitor.OnChunkTransitionStateChanged(ILocationChunkTransitionArea transitionArea, bool isActive)
		{
		}

		// Token: 0x06001713 RID: 5907 RVA: 0x0004971B File Offset: 0x0004791B
		private void Update()
		{
			if (this.randomAnimations.Length != 0 && this.nextRandomAnimTime <= Time.time)
			{
				this.PlayRandomAnimation();
			}
		}

		// Token: 0x06001714 RID: 5908 RVA: 0x0004973C File Offset: 0x0004793C
		protected virtual void Start()
		{
			this.ResetTime();
			GameSceneManager gameSceneManager;
			if (base.CurrentGame.Services.TryGet<GameSceneManager>(out gameSceneManager))
			{
				ILocationChunk locationChunkAtPoint = gameSceneManager.GeneratedLocation.GetLocationChunkAtPoint(this.Position, false);
				if (locationChunkAtPoint != null)
				{
					locationChunkAtPoint.AddVisitor(this);
				}
			}
			if (!this.plotProgressOverrideGenerator.IsEmpty())
			{
				this.plotProgressOverride = this.plotProgressOverrideGenerator.CreatePlotProgress();
				this.plotProgressOverride.UseCurrentPlotThreadForce = true;
			}
			if (this.HasPlot && base.CurrentGame.Services.TryGet<ICharacterConversationManager>(out this.conversationManager))
			{
				this.PrepareConversation();
				this.conversationManager.ConversationCompleted += this.OnConversationCompleted;
			}
			if (base.CurrentGame.Services.TryGet<IPlotMilestoneManager>(out this.milestoneManager))
			{
				this.milestoneManager.MilestoneReached += this.OnMilestoneReached;
			}
		}

		// Token: 0x06001715 RID: 5909 RVA: 0x00049818 File Offset: 0x00047A18
		protected virtual void OnConversationStarted(ICharactersConversation conversation)
		{
		}

		// Token: 0x06001716 RID: 5910 RVA: 0x0004981C File Offset: 0x00047A1C
		protected virtual void OnConversationCompleted(IPlotCharacter character, ICharactersConversation conversation)
		{
			if (this.currentConversation == conversation)
			{
				if (this.currentConversation != null)
				{
					this.availableConversationBranches.Remove(conversation.ConversationBranch);
					conversation.PhraseStarted -= this.OnConversationPhraseStarted;
					conversation.PhraseCompleted -= this.OnConversationPhraseCompleted;
				}
				UnityEvent unityEvent = this.onConversationCompletedEvent;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
				this.currentConversation = null;
			}
			if (this.availableConversationBranches.Count > 0)
			{
				this.PrepareConversation();
			}
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x0004989C File Offset: 0x00047A9C
		private void OnConversationPhraseStarted(ICharactersConversation conversation, ICharacterPlotItem phrase)
		{
			CharacterPhrase characterPhrase = phrase as CharacterPhrase;
			if (characterPhrase != null)
			{
				UnityEvent<CharacterPhrase> unityEvent = this.onPhraseStartedEvent;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(characterPhrase);
			}
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x000498C4 File Offset: 0x00047AC4
		private void OnConversationPhraseCompleted(ICharactersConversation conversation, ICharacterPlotItem phrase)
		{
			CharacterPhrase characterPhrase = phrase as CharacterPhrase;
			if (characterPhrase != null)
			{
				UnityEvent<CharacterPhrase> unityEvent = this.onPhraseCompletedEvent;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke(characterPhrase);
			}
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x000498EC File Offset: 0x00047AEC
		private void OnMilestoneReached(PlotMilestoneNode milestone)
		{
			if (this.isConversationAlreadyStarted)
			{
				return;
			}
			try
			{
				this.PrepareConversation();
			}
			catch (AssertionException ex)
			{
				Debug.LogError("Failed to prepare conversation for " + base.name + ", with error: " + ex.Message);
				throw;
			}
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x00049940 File Offset: 0x00047B40
		private void PrepareConversation()
		{
			this.PreparedConversation = null;
			if (this.conversationManager != null)
			{
				this.conversationManager.PrepareConversation(this);
			}
			if (this.PreparedConversation != null)
			{
				UnityEvent unityEvent = this.onConversationPreparedEvent;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x00049978 File Offset: 0x00047B78
		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.RemoveFromAllChunks(this.CurrentLocationChunk);
			if (this.currentConversation != null)
			{
				this.currentConversation.PhraseStarted -= this.OnConversationPhraseStarted;
				this.currentConversation.PhraseCompleted -= this.OnConversationPhraseCompleted;
			}
			if (this.conversationManager != null)
			{
				this.conversationManager.ConversationCompleted -= this.OnConversationCompleted;
				if (this.PreparedConversation != null && this.PreparedConversation.RuntimeData.isSingleActivationAttemptItem)
				{
					((CharacterPlotTreeProgress)(this.conversationManager as BaseCharacterConversationManager).GetCharacterPlotProgress(this)).AddCompletedExpositionThread(this.PreparedConversation.RuntimeData.parentPlotItem);
				}
			}
			if (this.milestoneManager != null)
			{
				this.milestoneManager.MilestoneReached -= this.OnMilestoneReached;
			}
		}

		// Token: 0x04000D52 RID: 3410
		public ConversationBranch[] conversationBranches = new ConversationBranch[1];

		// Token: 0x04000D53 RID: 3411
		public CharacterPlotNodeGraph characterPlotGraph;

		// Token: 0x04000D54 RID: 3412
		public CharacterPlotTreeProgressGenerator plotProgressOverrideGenerator;

		// Token: 0x04000D55 RID: 3413
		public Animator anim;

		// Token: 0x04000D56 RID: 3414
		public AnimationClip[] randomAnimations;

		// Token: 0x04000D57 RID: 3415
		public float minRandomAnimInterval = 3f;

		// Token: 0x04000D58 RID: 3416
		public float maxRandomAnimInterval = 5f;

		// Token: 0x04000D59 RID: 3417
		public LocationObjectType locationObjectType;

		// Token: 0x04000D5A RID: 3418
		[SerializeField]
		private UnityEvent onConversationPreparedEvent;

		// Token: 0x04000D5B RID: 3419
		[SerializeField]
		private UnityEvent onConversationStartedEvent;

		// Token: 0x04000D5C RID: 3420
		[SerializeField]
		private UnityEvent onConversationCompletedEvent;

		// Token: 0x04000D5D RID: 3421
		[SerializeField]
		private UnityEvent<CharacterPhrase> onPhraseStartedEvent;

		// Token: 0x04000D5E RID: 3422
		[SerializeField]
		private UnityEvent<CharacterPhrase> onPhraseCompletedEvent;

		// Token: 0x04000D5F RID: 3423
		private List<ConversationBranch> availableConversationBranches;

		// Token: 0x04000D60 RID: 3424
		private ICharactersConversation currentConversation;

		// Token: 0x04000D61 RID: 3425
		private float nextRandomAnimTime;

		// Token: 0x04000D62 RID: 3426
		private CharacterPlotTreeProgress plotProgressOverride;

		// Token: 0x04000D63 RID: 3427
		private ICharacterConversationManager conversationManager;

		// Token: 0x04000D64 RID: 3428
		private bool isConversationAlreadyStarted;

		// Token: 0x04000D65 RID: 3429
		private IPlotMilestoneManager milestoneManager;
	}
}
