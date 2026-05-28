using System;
using Game.Core;
using UnityEngine;

namespace Unliving.Tutorial
{
	// Token: 0x02000030 RID: 48
	public abstract class TutorialHintBase : ITutorialHint
	{
		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000199 RID: 409 RVA: 0x00006BE6 File Offset: 0x00004DE6
		public string ID
		{
			get
			{
				return this.id;
			}
		}

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x0600019A RID: 410 RVA: 0x00006BEE File Offset: 0x00004DEE
		public int Priority
		{
			get
			{
				return this.priority;
			}
		}

		// Token: 0x1400001C RID: 28
		// (add) Token: 0x0600019B RID: 411 RVA: 0x00006BF8 File Offset: 0x00004DF8
		// (remove) Token: 0x0600019C RID: 412 RVA: 0x00006C30 File Offset: 0x00004E30
		public event Action<ITutorialHint> HintTriggersReached;

		// Token: 0x0600019D RID: 413 RVA: 0x00006C65 File Offset: 0x00004E65
		public virtual void OnSceneLoaded(IGame game)
		{
			this.game = game;
		}

		// Token: 0x0600019E RID: 414 RVA: 0x00006C70 File Offset: 0x00004E70
		public void UpdateState()
		{
			if (this.isCompleted)
			{
				return;
			}
			if (this.IsConditionReached())
			{
				this.currentCallsCount++;
				if (this.currentCallsCount >= this.maxHintCallsCount)
				{
					this.isCompleted = true;
					this.OnHintCompleted();
				}
				this.OnHintConditionReached();
				Action<ITutorialHint> hintTriggersReached = this.HintTriggersReached;
				if (hintTriggersReached == null)
				{
					return;
				}
				hintTriggersReached(this);
			}
		}

		// Token: 0x0600019F RID: 415
		protected abstract void OnHintConditionReached();

		// Token: 0x060001A0 RID: 416
		protected abstract bool IsConditionReached();

		// Token: 0x060001A1 RID: 417
		protected abstract void OnHintCompleted();

		// Token: 0x060001A2 RID: 418 RVA: 0x00006CD0 File Offset: 0x00004ED0
		public TutorialHintSerializationData GetSerializationData()
		{
			return new TutorialHintSerializationData
			{
				id = this.id,
				currentCallsCount = this.currentCallsCount
			};
		}

		// Token: 0x060001A3 RID: 419 RVA: 0x00006D00 File Offset: 0x00004F00
		public void SetSerializationData(TutorialHintSerializationData data)
		{
			this.currentCallsCount = data.currentCallsCount;
			if (this.currentCallsCount >= this.maxHintCallsCount)
			{
				this.isCompleted = true;
				this.OnHintCompleted();
			}
		}

		// Token: 0x040000D4 RID: 212
		[SerializeField]
		private string id;

		// Token: 0x040000D5 RID: 213
		[SerializeField]
		private int priority;

		// Token: 0x040000D6 RID: 214
		[SerializeField]
		private int maxHintCallsCount;

		// Token: 0x040000D7 RID: 215
		protected IGame game;

		// Token: 0x040000D8 RID: 216
		protected bool isCompleted;

		// Token: 0x040000D9 RID: 217
		private int currentCallsCount;
	}
}
