using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Common.Utility;
using UnityEngine;

namespace Unliving.Pickables
{
	// Token: 0x02000198 RID: 408
	public class PickableObjectCollector : MonoBehaviour, IPickableObjectCollector
	{
		// Token: 0x1700020D RID: 525
		// (get) Token: 0x06000BAF RID: 2991 RVA: 0x00025374 File Offset: 0x00023574
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x06000BB0 RID: 2992 RVA: 0x00025377 File Offset: 0x00023577
		// (set) Token: 0x06000BB1 RID: 2993 RVA: 0x0002538F File Offset: 0x0002358F
		public Comparison<IPickableObject> BestPickingCandidateSelector
		{
			get
			{
				return this.bestPickingCandidateSelector ?? new Comparison<IPickableObject>(this.DefaultBestPickingCandidateSelector);
			}
			set
			{
				this.bestPickingCandidateSelector = value;
			}
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x06000BB2 RID: 2994 RVA: 0x00025398 File Offset: 0x00023598
		// (set) Token: 0x06000BB3 RID: 2995 RVA: 0x000253A0 File Offset: 0x000235A0
		public IPickableObject CurrentBestPickingCandidate { get; private set; }

		// Token: 0x14000080 RID: 128
		// (add) Token: 0x06000BB4 RID: 2996 RVA: 0x000253AC File Offset: 0x000235AC
		// (remove) Token: 0x06000BB5 RID: 2997 RVA: 0x000253E4 File Offset: 0x000235E4
		public event Action<IPickableObjectCollector, IPickableObject> ObjectPickedUp;

		// Token: 0x14000081 RID: 129
		// (add) Token: 0x06000BB6 RID: 2998 RVA: 0x0002541C File Offset: 0x0002361C
		// (remove) Token: 0x06000BB7 RID: 2999 RVA: 0x00025454 File Offset: 0x00023654
		public event Action<IPickableObjectCollector, IPickableObject, PickingUpErrorType> ObjectPickingUpFailed;

		// Token: 0x14000082 RID: 130
		// (add) Token: 0x06000BB8 RID: 3000 RVA: 0x0002548C File Offset: 0x0002368C
		// (remove) Token: 0x06000BB9 RID: 3001 RVA: 0x000254C4 File Offset: 0x000236C4
		public event Action<IPickableObjectCollector, IPickableObject> BestPickingCandidateChanged;

		// Token: 0x14000083 RID: 131
		// (add) Token: 0x06000BBA RID: 3002 RVA: 0x000254FC File Offset: 0x000236FC
		// (remove) Token: 0x06000BBB RID: 3003 RVA: 0x00025534 File Offset: 0x00023734
		public event Action<IPickableObjectCollector, IPickableObject> PickableObjectCollected;

		// Token: 0x06000BBC RID: 3004 RVA: 0x0002556C File Offset: 0x0002376C
		private int DefaultBestPickingCandidateSelector(IPickableObject candidate0, IPickableObject candidate1)
		{
			Vector2 b = base.transform.position;
			Vector2 vector = candidate0.WorldPosition - b;
			Vector2 vector2 = candidate1.WorldPosition - b;
			return -(vector.x * vector.x + vector.y * vector.y).CompareTo(vector2.x * vector2.x + vector2.y * vector2.y);
		}

		// Token: 0x06000BBD RID: 3005 RVA: 0x000255E3 File Offset: 0x000237E3
		private void SetBestPickingCandidate(IPickableObject newBestPickingCandidate)
		{
			if (this.CurrentBestPickingCandidate != newBestPickingCandidate)
			{
				this.CurrentBestPickingCandidate = newBestPickingCandidate;
				Action<IPickableObjectCollector, IPickableObject> bestPickingCandidateChanged = this.BestPickingCandidateChanged;
				if (bestPickingCandidateChanged == null)
				{
					return;
				}
				bestPickingCandidateChanged(this, newBestPickingCandidate);
			}
		}

		// Token: 0x06000BBE RID: 3006 RVA: 0x00025608 File Offset: 0x00023808
		private void UpdateBestPickingCandidate()
		{
			if (this.pickingCandidates.Count == 0)
			{
				this.SetBestPickingCandidate(null);
				return;
			}
			if (this.pickingCandidates.Count == 1)
			{
				this.SetBestPickingCandidate(this.pickingCandidates[0]);
				return;
			}
			Comparison<IPickableObject> comparison = this.BestPickingCandidateSelector;
			IPickableObject pickableObject = this.CurrentBestPickingCandidate;
			for (int i = 0; i < this.pickingCandidates.Count; i++)
			{
				IPickableObject pickableObject2 = this.pickingCandidates[i];
				if (!pickableObject2.IsNull() && (pickableObject.IsNull() || comparison(pickableObject2, pickableObject) > 0))
				{
					pickableObject = pickableObject2;
				}
			}
			this.SetBestPickingCandidate(pickableObject);
		}

		// Token: 0x06000BBF RID: 3007 RVA: 0x000256A0 File Offset: 0x000238A0
		public void AddPickingCandidate(IPickableObject obj)
		{
			if (!obj.IsNull() && !this.pickingCandidates.Contains(obj))
			{
				this.pickingCandidates.Add(obj);
				base.enabled = true;
			}
		}

		// Token: 0x06000BC0 RID: 3008 RVA: 0x000256CB File Offset: 0x000238CB
		public void RemovePickingCandidate(IPickableObject obj)
		{
			if (this.pickingCandidates.Remove(obj) && this.pickingCandidates.Count == 0)
			{
				this.UpdateBestPickingCandidate();
				base.enabled = false;
			}
		}

		// Token: 0x06000BC1 RID: 3009 RVA: 0x000256F8 File Offset: 0x000238F8
		public bool PickUp(IPickableObject obj, PickingArgs args)
		{
			if (obj.IsNull())
			{
				return false;
			}
			if (!args.forcePickingUp && Time.unscaledTime < this.nextPickingTime)
			{
				return false;
			}
			PickingUpErrorType pickingUpErrorType;
			if (!obj.CanBePickedUp(this, args, out pickingUpErrorType))
			{
				if (this.pickingCandidates.Contains(obj))
				{
					obj.OnPickUpFailed(this, pickingUpErrorType);
					Action<IPickableObjectCollector, IPickableObject, PickingUpErrorType> objectPickingUpFailed = this.ObjectPickingUpFailed;
					if (objectPickingUpFailed != null)
					{
						objectPickingUpFailed(this, obj, pickingUpErrorType);
					}
				}
				return false;
			}
			if (obj.UpdatePickingProgress(args) >= 1f)
			{
				obj.OnPickedUp(this);
				Action<IPickableObjectCollector, IPickableObject> objectPickedUp = this.ObjectPickedUp;
				if (objectPickedUp != null)
				{
					objectPickedUp(this, obj);
				}
				this.nextPickingTime = Time.unscaledTime + 0.1f;
				return true;
			}
			return false;
		}

		// Token: 0x06000BC2 RID: 3010 RVA: 0x0002579A File Offset: 0x0002399A
		public bool PickUpBestObject(PickingArgs args)
		{
			return this.PickUp(this.CurrentBestPickingCandidate, args);
		}

		// Token: 0x06000BC3 RID: 3011 RVA: 0x000257A9 File Offset: 0x000239A9
		protected virtual void Awake()
		{
			this.bestCandidateUpdateTimer = new FixedRateTimer(5f);
		}

		// Token: 0x06000BC4 RID: 3012 RVA: 0x000257BB File Offset: 0x000239BB
		private void Update()
		{
			if (this.bestCandidateUpdateTimer.IsTimeReached(false))
			{
				this.UpdateBestPickingCandidate();
			}
		}

		// Token: 0x06000BC5 RID: 3013 RVA: 0x000257D1 File Offset: 0x000239D1
		private void OnDestroy()
		{
			this.SetBestPickingCandidate(null);
			this.pickingCandidates.Clear();
		}

		// Token: 0x06000BC6 RID: 3014 RVA: 0x000257E5 File Offset: 0x000239E5
		public void OnPickableObjectCollected(IPickableObject obj)
		{
			Action<IPickableObjectCollector, IPickableObject> pickableObjectCollected = this.PickableObjectCollected;
			if (pickableObjectCollected == null)
			{
				return;
			}
			pickableObjectCollected(this, obj);
		}

		// Token: 0x0400068A RID: 1674
		private const float PickingObjectTimeout = 0.1f;

		// Token: 0x04000690 RID: 1680
		private readonly List<IPickableObject> pickingCandidates = new List<IPickableObject>();

		// Token: 0x04000691 RID: 1681
		private Comparison<IPickableObject> bestPickingCandidateSelector;

		// Token: 0x04000692 RID: 1682
		private FixedRateTimer bestCandidateUpdateTimer;

		// Token: 0x04000693 RID: 1683
		private float nextPickingTime;
	}
}
