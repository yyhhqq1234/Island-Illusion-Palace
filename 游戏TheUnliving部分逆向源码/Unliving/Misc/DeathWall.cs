using System;
using System.Collections;
using Common.UnityExtensions;
using Game.Core;
using UnityEngine;
using Unliving.LevelGeneration;
using Unliving.Player;

namespace Unliving.Misc
{
	// Token: 0x0200023E RID: 574
	public class DeathWall : GameBehaviourBase
	{
		// Token: 0x06001386 RID: 4998 RVA: 0x0003CF3F File Offset: 0x0003B13F
		public bool TryToStartMoving()
		{
			if (!this.nextChunk.IsExplored)
			{
				return false;
			}
			base.StartCoroutine(this.MovingRoutine());
			return true;
		}

		// Token: 0x06001387 RID: 4999 RVA: 0x0003CF5E File Offset: 0x0003B15E
		public void SetData(Vector2 direction, LocationChunk chunk, LocationChunk nextChunk, PlayerBehaviour player)
		{
			this.direction = direction;
			this.chunk = chunk;
			this.nextChunk = nextChunk;
			this.player = player;
			this.SetChunkBounds();
			this.UpdateBounds(this.progress);
			base.TryGetComponent<SpriteRenderer>(out this.sprite);
		}

		// Token: 0x06001388 RID: 5000 RVA: 0x0003CF9C File Offset: 0x0003B19C
		private IEnumerator MovingRoutine()
		{
			while (this.progress < this.maxSizeLimit)
			{
				yield return new WaitForEndOfFrame();
				float movingSpeed = this.GetMovingSpeed();
				this.progress += movingSpeed * Time.deltaTime * 0.01f;
				this.UpdateBounds(this.progress);
			}
			yield break;
		}

		// Token: 0x06001389 RID: 5001 RVA: 0x0003CFAB File Offset: 0x0003B1AB
		private float GetMovingSpeed()
		{
			if (this.sprite.isVisible)
			{
				return this.wallVisibleSpeed;
			}
			if (!this.chunk.Equals(this.player.CurrentLocationChunk))
			{
				return this.notVisibleOnAnotherChunkSpeed;
			}
			return this.notVisibleOnThisChunkSpeed;
		}

		// Token: 0x0600138A RID: 5002 RVA: 0x0003CFE8 File Offset: 0x0003B1E8
		private void SetChunkBounds()
		{
			base.transform.rotation = QuaternionExtensions.Get2DRotation(this.chunk.Orientation);
			Transform boundsPivot = this.chunk.boundsPivot0;
			Transform boundsPivot2 = this.chunk.boundsPivot1;
			if (boundsPivot != null && boundsPivot2 != null)
			{
				this.bounds = default(Bounds);
				Vector2 lhs = boundsPivot.position;
				Vector2 rhs = boundsPivot2.position;
				this.bounds.SetMinMax(Vector2.Min(lhs, rhs), Vector2.Max(lhs, rhs));
			}
			base.transform.localScale = this.bounds.size;
			base.transform.position = new Vector3(this.bounds.center.x, this.bounds.center.y, 1f);
		}

		// Token: 0x0600138B RID: 5003 RVA: 0x0003D0CC File Offset: 0x0003B2CC
		private void UpdateBounds(float progress)
		{
			Bounds bounds = this.bounds;
			if (this.direction.x < 0f)
			{
				bounds.min = new Vector2(bounds.max.x - bounds.size.x * progress, bounds.min.y);
			}
			else if (this.direction.x > 0f)
			{
				bounds.max = new Vector2(bounds.min.x + bounds.size.x * progress, bounds.max.y);
			}
			else if (this.direction.y < 0f)
			{
				bounds.min = new Vector2(bounds.min.x, bounds.max.y - bounds.size.y * progress);
			}
			else if (this.direction.y > 0f)
			{
				bounds.max = new Vector2(bounds.max.x, bounds.min.y + bounds.size.y * progress);
			}
			base.transform.localScale = bounds.size;
			base.transform.position = bounds.center;
		}

		// Token: 0x04000B53 RID: 2899
		public float wallVisibleSpeed;

		// Token: 0x04000B54 RID: 2900
		public float notVisibleOnThisChunkSpeed;

		// Token: 0x04000B55 RID: 2901
		public float notVisibleOnAnotherChunkSpeed;

		// Token: 0x04000B56 RID: 2902
		public float maxSizeLimit;

		// Token: 0x04000B57 RID: 2903
		private float progress;

		// Token: 0x04000B58 RID: 2904
		private LocationChunk chunk;

		// Token: 0x04000B59 RID: 2905
		private LocationChunk nextChunk;

		// Token: 0x04000B5A RID: 2906
		private Bounds bounds;

		// Token: 0x04000B5B RID: 2907
		private Vector2 direction;

		// Token: 0x04000B5C RID: 2908
		private SpriteRenderer sprite;

		// Token: 0x04000B5D RID: 2909
		private PlayerBehaviour player;
	}
}
