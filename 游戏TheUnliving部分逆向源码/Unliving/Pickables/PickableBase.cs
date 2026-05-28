using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.UnityExtensions;
using Game.Core;
using Game.InputManager;
using UnityEngine;
using Unliving.Factories;
using Unliving.Player;

namespace Unliving.Pickables
{
	// Token: 0x02000197 RID: 407
	public abstract class PickableBase : GameBehaviourBase, IPickableObject
	{
		// Token: 0x17000202 RID: 514
		// (get) Token: 0x06000B7E RID: 2942
		public abstract PickableObjectData PickableObjectData { get; }

		// Token: 0x17000203 RID: 515
		// (get) Token: 0x06000B7F RID: 2943 RVA: 0x00024B1E File Offset: 0x00022D1E
		public Vector2 WorldPosition
		{
			get
			{
				return base.transform.position;
			}
		}

		// Token: 0x17000204 RID: 516
		// (get) Token: 0x06000B80 RID: 2944 RVA: 0x00024B30 File Offset: 0x00022D30
		public IList<IPickableObjectCollector> CollectorsInRange
		{
			get
			{
				return this._collectorsInRange;
			}
		}

		// Token: 0x17000205 RID: 517
		// (get) Token: 0x06000B81 RID: 2945 RVA: 0x00024B38 File Offset: 0x00022D38
		public virtual IPickableObjectCollector PointerEventsSender
		{
			get
			{
				return this.CurrentPlayer.PickableObjectsController;
			}
		}

		// Token: 0x17000206 RID: 518
		// (get) Token: 0x06000B82 RID: 2946 RVA: 0x00024B45 File Offset: 0x00022D45
		public MultiRepresentationObjectInstantiator.IObjectData ObjectData
		{
			get
			{
				return this.objectData;
			}
		}

		// Token: 0x17000207 RID: 519
		// (get) Token: 0x06000B83 RID: 2947 RVA: 0x00024B4D File Offset: 0x00022D4D
		public Renderer Renderer
		{
			get
			{
				if (this._renderer == null)
				{
					this._renderer = this.FindRenderer();
				}
				return this._renderer;
			}
		}

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x06000B84 RID: 2948
		protected abstract string LocalizationID { get; }

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x06000B85 RID: 2949 RVA: 0x00024B70 File Offset: 0x00022D70
		protected PlayerBehaviour CurrentPlayer
		{
			get
			{
				IPlayerProvider playerProvider;
				if (this.currentPlayer == null && base.CurrentGame.Services.TryGet<IPlayerProvider>(out playerProvider) && playerProvider.CurrentPlayer != null)
				{
					this.currentPlayer = playerProvider.CurrentPlayer;
				}
				return this.currentPlayer;
			}
		}

		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06000B86 RID: 2950 RVA: 0x00024BBF File Offset: 0x00022DBF
		protected PlayerInputController PlayerInput
		{
			get
			{
				return this.CurrentPlayer.PlayerInputController;
			}
		}

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06000B87 RID: 2951 RVA: 0x00024BCC File Offset: 0x00022DCC
		protected virtual IPickingSettings PickupSettings
		{
			get
			{
				return this.pickupSettings ?? new OnClickPickingSettings();
			}
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x06000B88 RID: 2952 RVA: 0x00024BDD File Offset: 0x00022DDD
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x1400007A RID: 122
		// (add) Token: 0x06000B89 RID: 2953 RVA: 0x00024BE0 File Offset: 0x00022DE0
		// (remove) Token: 0x06000B8A RID: 2954 RVA: 0x00024C18 File Offset: 0x00022E18
		public event Action<IPickableObjectCollector> CollectorEnteredPickingRange;

		// Token: 0x1400007B RID: 123
		// (add) Token: 0x06000B8B RID: 2955 RVA: 0x00024C50 File Offset: 0x00022E50
		// (remove) Token: 0x06000B8C RID: 2956 RVA: 0x00024C88 File Offset: 0x00022E88
		public event Action<IPickableObjectCollector> CollectorExitedPickingRange;

		// Token: 0x1400007C RID: 124
		// (add) Token: 0x06000B8D RID: 2957 RVA: 0x00024CC0 File Offset: 0x00022EC0
		// (remove) Token: 0x06000B8E RID: 2958 RVA: 0x00024CF8 File Offset: 0x00022EF8
		public event Action<float> PickingUpProgressChanged;

		// Token: 0x1400007D RID: 125
		// (add) Token: 0x06000B8F RID: 2959 RVA: 0x00024D30 File Offset: 0x00022F30
		// (remove) Token: 0x06000B90 RID: 2960 RVA: 0x00024D68 File Offset: 0x00022F68
		public event Action<IPickableObject, IPickableObjectCollector> PickedUp;

		// Token: 0x1400007E RID: 126
		// (add) Token: 0x06000B91 RID: 2961 RVA: 0x00024DA0 File Offset: 0x00022FA0
		// (remove) Token: 0x06000B92 RID: 2962 RVA: 0x00024DD8 File Offset: 0x00022FD8
		public event Action<IPickableObjectCollector, PickingUpErrorType> PickedUpFailed;

		// Token: 0x1400007F RID: 127
		// (add) Token: 0x06000B93 RID: 2963 RVA: 0x00024E10 File Offset: 0x00023010
		// (remove) Token: 0x06000B94 RID: 2964 RVA: 0x00024E48 File Offset: 0x00023048
		public event Action Dropped;

		// Token: 0x06000B95 RID: 2965 RVA: 0x00024E80 File Offset: 0x00023080
		private void HandleCollector(IPickableObjectCollector collector, bool add)
		{
			if (collector.IsNull())
			{
				return;
			}
			if (!add)
			{
				if (this._collectorsInRange.Remove(collector))
				{
					this.OnCollectorExitedRange(collector);
				}
				return;
			}
			if (this._collectorsInRange.Contains(collector))
			{
				return;
			}
			this._collectorsInRange.Add(collector);
			this.OnCollectorEnteredRange(collector);
		}

		// Token: 0x06000B96 RID: 2966 RVA: 0x00024ED4 File Offset: 0x000230D4
		private void ClearAllCollectors()
		{
			for (int i = this._collectorsInRange.Count - 1; i >= 0; i--)
			{
				this.HandleCollector(this._collectorsInRange[i], false);
			}
		}

		// Token: 0x06000B97 RID: 2967 RVA: 0x00024F0C File Offset: 0x0002310C
		private Renderer FindRenderer()
		{
			Renderer renderer = PickableBase.<FindRenderer>g__GetRenderer|55_0(this);
			if (renderer == null)
			{
				int childCount = base.transform.childCount;
				int num = 0;
				while (num < childCount && !((renderer = PickableBase.<FindRenderer>g__GetRenderer|55_0(base.transform.GetChild(num))) != null))
				{
					num++;
				}
			}
			return renderer;
		}

		// Token: 0x06000B98 RID: 2968 RVA: 0x00024F60 File Offset: 0x00023160
		private void UpdateSprite(MultiRepresentationObjectInstantiator.IObjectData data)
		{
			if (this.pickableObjectData == null)
			{
				this.pickableObjectData = new PickableObjectData
				{
					icon = data.UIIcon
				};
			}
			else
			{
				this.pickableObjectData.icon = data.UIIcon;
			}
			if (data.ObjectIcon != null)
			{
				SpriteRenderer spriteRenderer = this.Renderer as SpriteRenderer;
				if (spriteRenderer != null)
				{
					spriteRenderer.sprite = data.ObjectIcon;
				}
			}
		}

		// Token: 0x06000B99 RID: 2969 RVA: 0x00024FC8 File Offset: 0x000231C8
		public void SetPickingSettings(IPickingSettings pickingSettings)
		{
			this.pickupSettings = pickingSettings;
		}

		// Token: 0x06000B9A RID: 2970 RVA: 0x00024FD4 File Offset: 0x000231D4
		public float UpdatePickingProgress(PickingArgs args)
		{
			float currentProgress = this.PickupSettings.CurrentProgress;
			float num = this.PickupSettings.UpdateCurrentPickingProgress(args.inputArgs);
			if (num != currentProgress)
			{
				Action<float> pickingUpProgressChanged = this.PickingUpProgressChanged;
				if (pickingUpProgressChanged != null)
				{
					pickingUpProgressChanged(num);
				}
			}
			return num;
		}

		// Token: 0x06000B9B RID: 2971
		protected abstract void PerformPickingUp(IPickableObjectCollector targetCollector);

		// Token: 0x06000B9C RID: 2972 RVA: 0x00025018 File Offset: 0x00023218
		protected virtual IPickableObjectCollector GetCollectorBehaviour(GameObject collectorObject)
		{
			IPickableObjectCollector result;
			if (collectorObject.InLayerMask(this.validCollectorLayers.value) && collectorObject.TryGetComponent<IPickableObjectCollector>(out result))
			{
				return result;
			}
			return null;
		}

		// Token: 0x06000B9D RID: 2973 RVA: 0x00025048 File Offset: 0x00023248
		protected bool IsPickingBlockedByObstacle(IPickableObjectCollector targetCollector)
		{
			if (this.pickingObstacleLayers.value != 0)
			{
				Transform transform = (targetCollector as Component).transform;
				return transform != null && Physics2D.Linecast(base.transform.position, transform.position, this.pickingObstacleLayers);
			}
			return false;
		}

		// Token: 0x06000B9E RID: 2974 RVA: 0x000250AB File Offset: 0x000232AB
		public virtual void InitializeData(object args, MultiRepresentationObjectInstantiator.IObjectData data)
		{
			this.objectData = data;
			this.UpdateSprite(data);
		}

		// Token: 0x06000B9F RID: 2975 RVA: 0x000250BB File Offset: 0x000232BB
		public void PickupByPointerEventsSender()
		{
			this.PickupByPointerEventsSender(false, true);
		}

		// Token: 0x06000BA0 RID: 2976 RVA: 0x000250C5 File Offset: 0x000232C5
		public void PickupByPointerEventsSenderForce()
		{
			this.PickupByPointerEventsSender(true, true);
		}

		// Token: 0x06000BA1 RID: 2977 RVA: 0x000250D0 File Offset: 0x000232D0
		public void PickupByPointerEventsSender(bool force, bool lockInput)
		{
			PickingArgs args = this.playerPickupArgs;
			args.forcePickingUp = force;
			if (this.PlayerInput != null && this.PointerEventsSender != null && this.PointerEventsSender.PickUp(this, args) && lockInput)
			{
				int keyID;
				if (this.inputManager.TryGetActionElementID(15, InputAxisContribution.Positive, out keyID))
				{
					this.PlayerInput.LockInput(keyID, PlayerAction.NONE);
				}
				int keyID2;
				if (this.inputManager.TryGetActionElementID(24, InputAxisContribution.Positive, out keyID2))
				{
					this.PlayerInput.LockInput(keyID2, PlayerAction.NONE);
				}
			}
		}

		// Token: 0x06000BA2 RID: 2978
		public abstract bool CanBeUsedByCollector(IPickableObjectCollector targetCollector);

		// Token: 0x06000BA3 RID: 2979 RVA: 0x00025150 File Offset: 0x00023350
		public virtual bool CanBePickedUp(IPickableObjectCollector targetCollector, PickingArgs args, out PickingUpErrorType error)
		{
			bool flag = this.CanBeUsedByCollector(targetCollector) && !targetCollector.IsNull() && (args.forcePickingUp || this._collectorsInRange.Contains(targetCollector));
			if (!flag)
			{
				error = PickingUpErrorType.FullInventory;
				return false;
			}
			flag &= !this.IsPickingBlockedByObstacle(targetCollector);
			error = (flag ? PickingUpErrorType.None : PickingUpErrorType.BlockedByObstacle);
			return flag;
		}

		// Token: 0x06000BA4 RID: 2980 RVA: 0x000251A8 File Offset: 0x000233A8
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			base.TryGetComponent<Collider2D>(out this.collider2D);
			currentGame.Services.TryGet<IInputManager>(out this.inputManager);
		}

		// Token: 0x06000BA5 RID: 2981 RVA: 0x000251D0 File Offset: 0x000233D0
		protected virtual void OnCollectorEnteredRange(IPickableObjectCollector collector)
		{
			collector.AddPickingCandidate(this);
			Action<IPickableObjectCollector> collectorEnteredPickingRange = this.CollectorEnteredPickingRange;
			if (collectorEnteredPickingRange == null)
			{
				return;
			}
			collectorEnteredPickingRange(collector);
		}

		// Token: 0x06000BA6 RID: 2982 RVA: 0x000251EA File Offset: 0x000233EA
		protected virtual void OnCollectorExitedRange(IPickableObjectCollector collector)
		{
			collector.RemovePickingCandidate(this);
			Action<IPickableObjectCollector> collectorExitedPickingRange = this.CollectorExitedPickingRange;
			if (collectorExitedPickingRange == null)
			{
				return;
			}
			collectorExitedPickingRange(collector);
		}

		// Token: 0x06000BA7 RID: 2983 RVA: 0x00025204 File Offset: 0x00023404
		void IPickableObject.OnDropped()
		{
			Action dropped = this.Dropped;
			if (dropped == null)
			{
				return;
			}
			dropped();
		}

		// Token: 0x06000BA8 RID: 2984 RVA: 0x00025218 File Offset: 0x00023418
		void IPickableObject.OnPickedUp(IPickableObjectCollector collector)
		{
			this.PerformPickingUp(collector);
			Action<IPickableObject, IPickableObjectCollector> pickedUp = this.PickedUp;
			if (pickedUp != null)
			{
				pickedUp(this, collector);
			}
			this.PickupSettings.ResetCurrentPickingProgress();
			Action<float> pickingUpProgressChanged = this.PickingUpProgressChanged;
			if (pickingUpProgressChanged != null)
			{
				pickingUpProgressChanged(this.PickupSettings.CurrentProgress);
			}
			if (this.disableColliderOnPicking)
			{
				this.collider2D.enabled = false;
				Physics2D.Simulate(Time.fixedDeltaTime);
				this.ClearAllCollectors();
			}
		}

		// Token: 0x06000BA9 RID: 2985 RVA: 0x0002528B File Offset: 0x0002348B
		public virtual void OnPickUpFailed(IPickableObjectCollector collector, PickingUpErrorType error)
		{
			Action<IPickableObjectCollector, PickingUpErrorType> pickedUpFailed = this.PickedUpFailed;
			if (pickedUpFailed == null)
			{
				return;
			}
			pickedUpFailed(collector, error);
		}

		// Token: 0x06000BAA RID: 2986 RVA: 0x0002529F File Offset: 0x0002349F
		private void OnTriggerEnter2D(Collider2D collider)
		{
			this.HandleCollector(this.GetCollectorBehaviour(collider.gameObject), true);
		}

		// Token: 0x06000BAB RID: 2987 RVA: 0x000252B4 File Offset: 0x000234B4
		private void OnTriggerExit2D(Collider2D collider)
		{
			this.HandleCollector(this.GetCollectorBehaviour(collider.gameObject), false);
		}

		// Token: 0x06000BAC RID: 2988 RVA: 0x000252C9 File Offset: 0x000234C9
		protected override void OnDestroy()
		{
			this.ClearAllCollectors();
			base.OnDestroy();
		}

		// Token: 0x06000BAE RID: 2990 RVA: 0x00025348 File Offset: 0x00023548
		[CompilerGenerated]
		internal static Renderer <FindRenderer>g__GetRenderer|55_0(Component component)
		{
			if (component.IsNull())
			{
				return null;
			}
			Renderer renderer;
			if (!component.TryGetComponent<Renderer>(out renderer) || renderer is ParticleSystemRenderer)
			{
				return null;
			}
			return renderer;
		}

		// Token: 0x04000677 RID: 1655
		protected const string REPLACEMENT_OBJECT_TITLE_KEY = "replacement_object_title_key";

		// Token: 0x0400067E RID: 1662
		public LayerMask validCollectorLayers = -1;

		// Token: 0x0400067F RID: 1663
		public LayerMask pickingObstacleLayers = 0;

		// Token: 0x04000680 RID: 1664
		public bool disableColliderOnPicking = true;

		// Token: 0x04000681 RID: 1665
		[SerializeField]
		private Renderer _renderer;

		// Token: 0x04000682 RID: 1666
		private readonly List<IPickableObjectCollector> _collectorsInRange = new List<IPickableObjectCollector>();

		// Token: 0x04000683 RID: 1667
		private PlayerBehaviour currentPlayer;

		// Token: 0x04000684 RID: 1668
		protected Collider2D collider2D;

		// Token: 0x04000685 RID: 1669
		protected MultiRepresentationObjectInstantiator.IObjectData objectData;

		// Token: 0x04000686 RID: 1670
		protected PickableObjectData pickableObjectData;

		// Token: 0x04000687 RID: 1671
		private IInputManager inputManager;

		// Token: 0x04000688 RID: 1672
		protected IPickingSettings pickupSettings;

		// Token: 0x04000689 RID: 1673
		private readonly PickingArgs playerPickupArgs = new PickingArgs
		{
			inputArgs = new PlayerInputController.ActionArgs
			{
				actionFlags = new PlayerActionsMask(new PlayerAction[]
				{
					PlayerAction.PLAYER_COLLECT_OBJECT
				})
			}
		};
	}
}
