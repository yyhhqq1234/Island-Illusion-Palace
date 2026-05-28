using System;
using Game.Core;
using UnityEngine;

namespace Unliving
{
	// Token: 0x02000016 RID: 22
	[RequireComponent(typeof(SpriteRenderer))]
	public sealed class ObjectPositionSetter : GameBehaviourBase
	{
		// Token: 0x0600011E RID: 286 RVA: 0x00004EE4 File Offset: 0x000030E4
		public override async void Initialize(IGame currentGame)
		{
			base.Initialize(currentGame);
			await new WaitForEndOfFrame();
			this.cachedTransform = base.transform;
			this.CreateMaterialPropertyBlock();
		}

		// Token: 0x0600011F RID: 287 RVA: 0x00004F25 File Offset: 0x00003125
		[ContextMenu("Reset Bounds")]
		public void ResetBounds()
		{
			this.spriteBounds = Vector4.zero;
		}

		// Token: 0x06000120 RID: 288 RVA: 0x00004F32 File Offset: 0x00003132
		private void CreateMaterialPropertyBlock()
		{
			if (this.spriteRenderer == null)
			{
				base.TryGetComponent<SpriteRenderer>(out this.spriteRenderer);
			}
			this.materialPropertyBlock = new MaterialPropertyBlock();
		}

		// Token: 0x06000121 RID: 289 RVA: 0x00004F5C File Offset: 0x0000315C
		private void LateUpdate()
		{
			if (this.cachedTransform == null)
			{
				return;
			}
			if (!this.dynamicObject || Time.frameCount % 2 != 0)
			{
				return;
			}
			Vector3 position = this.cachedTransform.position;
			position.z = 0f;
			if ((position - this.lastPosition).sqrMagnitude < 0.01f)
			{
				return;
			}
			if (this.spriteBounds.sqrMagnitude < 0.01f)
			{
				Bounds bounds = this.spriteRenderer.bounds;
				this.spriteBounds = new Vector4(bounds.min.x - position.x, bounds.min.y - position.y, bounds.max.x - position.x, bounds.max.y - position.y);
			}
			this.spriteRenderer.GetPropertyBlock(this.materialPropertyBlock);
			this.materialPropertyBlock.SetVector(ObjectPositionSetter.ObjPosProperty, position);
			Vector4 value = this.spriteBounds;
			value.x += position.x;
			value.y += position.y;
			value.z += position.x;
			value.w += position.y;
			this.materialPropertyBlock.SetVector(ObjectPositionSetter.ObjBoundsProperty, value);
			this.spriteRenderer.SetPropertyBlock(this.materialPropertyBlock);
			this.lastPosition = position;
		}

		// Token: 0x04000079 RID: 121
		private static readonly int ObjPosProperty = Shader.PropertyToID("_ObjectPosition");

		// Token: 0x0400007A RID: 122
		private static readonly int ObjBoundsProperty = Shader.PropertyToID("_ObjectBounds");

		// Token: 0x0400007B RID: 123
		public bool dynamicObject;

		// Token: 0x0400007C RID: 124
		public Vector4 spriteBounds;

		// Token: 0x0400007D RID: 125
		private Transform cachedTransform;

		// Token: 0x0400007E RID: 126
		private SpriteRenderer spriteRenderer;

		// Token: 0x0400007F RID: 127
		private MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04000080 RID: 128
		private Vector3 lastPosition;
	}
}
