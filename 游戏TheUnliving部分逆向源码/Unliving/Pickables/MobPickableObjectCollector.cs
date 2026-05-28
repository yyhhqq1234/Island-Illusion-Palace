using System;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.Mobs;

namespace Unliving.Pickables
{
	// Token: 0x02000193 RID: 403
	public sealed class MobPickableObjectCollector : GameBehaviourBase, IPickableObjectCollector
	{
		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06000B69 RID: 2921 RVA: 0x00024875 File Offset: 0x00022A75
		public Component Component
		{
			get
			{
				return this;
			}
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06000B6A RID: 2922 RVA: 0x00024878 File Offset: 0x00022A78
		// (set) Token: 0x06000B6B RID: 2923 RVA: 0x00024880 File Offset: 0x00022A80
		public BaseGameMob CurrentMob { get; private set; }

		// Token: 0x17000201 RID: 513
		// (get) Token: 0x06000B6C RID: 2924 RVA: 0x00024889 File Offset: 0x00022A89
		// (set) Token: 0x06000B6D RID: 2925 RVA: 0x00024891 File Offset: 0x00022A91
		public IPickableObject CurrentBestPickingCandidate { get; private set; }

		// Token: 0x14000076 RID: 118
		// (add) Token: 0x06000B6E RID: 2926 RVA: 0x0002489C File Offset: 0x00022A9C
		// (remove) Token: 0x06000B6F RID: 2927 RVA: 0x000248D4 File Offset: 0x00022AD4
		public event Action<IPickableObjectCollector, IPickableObject> ObjectPickedUp;

		// Token: 0x14000077 RID: 119
		// (add) Token: 0x06000B70 RID: 2928 RVA: 0x0002490C File Offset: 0x00022B0C
		// (remove) Token: 0x06000B71 RID: 2929 RVA: 0x00024944 File Offset: 0x00022B44
		public event Action<IPickableObjectCollector, IPickableObject, PickingUpErrorType> ObjectPickingUpFailed;

		// Token: 0x14000078 RID: 120
		// (add) Token: 0x06000B72 RID: 2930 RVA: 0x0002497C File Offset: 0x00022B7C
		// (remove) Token: 0x06000B73 RID: 2931 RVA: 0x000249B4 File Offset: 0x00022BB4
		public event Action<IPickableObjectCollector, IPickableObject> BestPickingCandidateChanged;

		// Token: 0x14000079 RID: 121
		// (add) Token: 0x06000B74 RID: 2932 RVA: 0x000249EC File Offset: 0x00022BEC
		// (remove) Token: 0x06000B75 RID: 2933 RVA: 0x00024A24 File Offset: 0x00022C24
		public event Action<IPickableObjectCollector, IPickableObject> PickableObjectCollected;

		// Token: 0x06000B76 RID: 2934 RVA: 0x00024A59 File Offset: 0x00022C59
		public override void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			this.CurrentMob = base.GetComponent<BaseGameMob>();
		}

		// Token: 0x06000B77 RID: 2935 RVA: 0x00024A6E File Offset: 0x00022C6E
		public void AddPickingCandidate(IPickableObject obj)
		{
			if (!obj.IsNull())
			{
				return;
			}
			this.CurrentBestPickingCandidate = obj;
		}

		// Token: 0x06000B78 RID: 2936 RVA: 0x00024A80 File Offset: 0x00022C80
		public bool PickUp(IPickableObject obj, PickingArgs args)
		{
			if (obj.IsNull())
			{
				return false;
			}
			PickingUpErrorType arg;
			if (obj.CanBePickedUp(this, args, out arg))
			{
				obj.OnPickedUp(this);
				Action<IPickableObjectCollector, IPickableObject> objectPickedUp = this.ObjectPickedUp;
				if (objectPickedUp != null)
				{
					objectPickedUp(this, obj);
				}
				return true;
			}
			Action<IPickableObjectCollector, IPickableObject, PickingUpErrorType> objectPickingUpFailed = this.ObjectPickingUpFailed;
			if (objectPickingUpFailed != null)
			{
				objectPickingUpFailed(this, obj, arg);
			}
			return false;
		}

		// Token: 0x06000B79 RID: 2937 RVA: 0x00024AD4 File Offset: 0x00022CD4
		public bool PickUpBestObject(PickingArgs args)
		{
			return this.PickUp(this.CurrentBestPickingCandidate, args);
		}

		// Token: 0x06000B7A RID: 2938 RVA: 0x00024AE3 File Offset: 0x00022CE3
		public void RemovePickingCandidate(IPickableObject obj)
		{
			if (obj.Equals(this.CurrentBestPickingCandidate))
			{
				this.CurrentBestPickingCandidate = null;
			}
		}

		// Token: 0x06000B7B RID: 2939 RVA: 0x00024AFA File Offset: 0x00022CFA
		public void OnPickableObjectCollected(IPickableObject obj)
		{
			Action<IPickableObjectCollector, IPickableObject> pickableObjectCollected = this.PickableObjectCollected;
			if (pickableObjectCollected == null)
			{
				return;
			}
			pickableObjectCollected(this, obj);
		}
	}
}
