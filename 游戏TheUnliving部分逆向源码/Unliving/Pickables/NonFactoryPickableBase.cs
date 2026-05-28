using System;
using Common.UnityExtensions;
using UnityEngine;
using Unliving.Factories;

namespace Unliving.Pickables
{
	// Token: 0x0200017E RID: 382
	[DefaultExecutionOrder(-100)]
	public abstract class NonFactoryPickableBase : PickableObjectBase<NonFactoryPickableType>
	{
		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06000A99 RID: 2713
		public abstract override NonFactoryPickableType ID { get; }

		// Token: 0x06000A9A RID: 2714 RVA: 0x00022E74 File Offset: 0x00021074
		protected virtual void Start()
		{
			if (this.IsPurchasable)
			{
				base.PurchasableData = this.GetPurchasableData(this.ID);
				base.PurchasableData.SetGameData(base.CurrentGame);
			}
			Sprite sprite = this.PickableObjectData.icon;
			Sprite sprite2 = this.PickableObjectData.icon;
			if (!base.Renderer.IsNull())
			{
				SpriteRenderer spriteRenderer = base.Renderer as SpriteRenderer;
				if (spriteRenderer != null)
				{
					sprite = spriteRenderer.sprite;
					if (sprite2 == null)
					{
						sprite2 = sprite;
					}
				}
			}
			this.objectData = new MultiRepresentationObjectInstantiator.DefaultData
			{
				objectIcon = sprite,
				uiIcon = sprite2
			};
		}
	}
}
