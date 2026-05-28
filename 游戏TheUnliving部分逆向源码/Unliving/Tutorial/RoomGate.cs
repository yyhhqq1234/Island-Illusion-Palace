using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Unliving.Tutorial
{
	// Token: 0x0200002C RID: 44
	[DefaultExecutionOrder(50)]
	public sealed class RoomGate : MonoBehaviour
	{
		// Token: 0x17000059 RID: 89
		// (get) Token: 0x06000185 RID: 389 RVA: 0x00006A28 File Offset: 0x00004C28
		public UnityEvent Locked
		{
			get
			{
				return this._locked;
			}
		}

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x06000186 RID: 390 RVA: 0x00006A30 File Offset: 0x00004C30
		public UnityEvent Unlocked
		{
			get
			{
				return this._unlocked;
			}
		}

		// Token: 0x06000187 RID: 391 RVA: 0x00006A38 File Offset: 0x00004C38
		private void SetLockState(bool isLocked, bool force)
		{
			if (!force && this._isLocked == isLocked)
			{
				return;
			}
			if (this.collider != null)
			{
				this.collider.enabled = isLocked;
			}
			if (this.navMeshObstacle != null)
			{
				this.navMeshObstacle.enabled = isLocked;
			}
			if (this._renderer != null)
			{
				this._renderer.sprite = (isLocked ? this.lockedStateSprite : this.unlockedStateSprite);
			}
			this._isLocked = isLocked;
			if (this._isLocked)
			{
				this._locked.Invoke();
				return;
			}
			this._unlocked.Invoke();
		}

		// Token: 0x06000188 RID: 392 RVA: 0x00006AD6 File Offset: 0x00004CD6
		public void SetLockState(bool isLocked)
		{
			this.SetLockState(isLocked, false);
		}

		// Token: 0x06000189 RID: 393 RVA: 0x00006AE0 File Offset: 0x00004CE0
		private void Start()
		{
			if (this._renderer == null)
			{
				this._renderer = base.GetComponent<SpriteRenderer>();
			}
			this.collider = base.GetComponent<Collider2D>();
			this.navMeshObstacle = base.GetComponent<NavMeshObstacle>();
			if (this.navMeshObstacle != null)
			{
				this.navMeshObstacle.carving = true;
			}
			this.SetLockState(this._isLocked, true);
		}

		// Token: 0x040000C6 RID: 198
		[SerializeField]
		private SpriteRenderer _renderer;

		// Token: 0x040000C7 RID: 199
		public Sprite unlockedStateSprite;

		// Token: 0x040000C8 RID: 200
		public Sprite lockedStateSprite;

		// Token: 0x040000C9 RID: 201
		[SerializeField]
		private bool _isLocked = true;

		// Token: 0x040000CA RID: 202
		[SerializeField]
		private UnityEvent _locked;

		// Token: 0x040000CB RID: 203
		[SerializeField]
		private UnityEvent _unlocked;

		// Token: 0x040000CC RID: 204
		private Collider2D collider;

		// Token: 0x040000CD RID: 205
		private NavMeshObstacle navMeshObstacle;
	}
}
