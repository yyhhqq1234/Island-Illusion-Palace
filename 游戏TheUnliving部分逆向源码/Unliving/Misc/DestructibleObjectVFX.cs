using System;
using System.Collections.Generic;
using Common.UnityExtensions;
using Game.Utility;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x02000240 RID: 576
	[Obsolete]
	public sealed class DestructibleObjectVFX : MonoBehaviour
	{
		// Token: 0x06001391 RID: 5009 RVA: 0x0003D408 File Offset: 0x0003B608
		private void Awake()
		{
			base.GetComponentsInChildren<Rigidbody2D>(this.rigidbodies);
			foreach (Rigidbody2D rigidbody2D in this.rigidbodies)
			{
				Vector2 force = new Vector2(UnityEngine.Random.Range(-1f, 1f) * this.maxForce, 0f);
				rigidbody2D.AddForce(force, ForceMode2D.Impulse);
				rigidbody2D.AddTorque(UnityEngine.Random.Range(0f, this.maxTorque));
			}
		}

		// Token: 0x06001392 RID: 5010 RVA: 0x0003D4A0 File Offset: 0x0003B6A0
		private void Update()
		{
			this.staticRigidbodies.Clear();
			for (int i = this.rigidbodies.Count - 1; i >= 0; i--)
			{
				Rigidbody2D rigidbody2D = this.rigidbodies[i];
				if (rigidbody2D.IsNull())
				{
					this.rigidbodies.RemoveAt(i);
				}
				else if (Mathf.Abs(rigidbody2D.velocity.x) < 0.01f)
				{
					rigidbody2D.velocity = Vector2.zero;
					this.staticRigidbodies.Add(rigidbody2D);
					this.rigidbodies.RemoveAt(i);
				}
				else
				{
					rigidbody2D.velocity = new Vector2(rigidbody2D.velocity.x, -this.gravityScale);
				}
			}
			if (this.staticRigidbodies.Count > 0)
			{
				foreach (Rigidbody2D rigidbody2D2 in this.staticRigidbodies)
				{
					SpriteFader spriteFader;
					if (!this.enablePhysics || rigidbody2D2.TryGetComponent<SpriteFader>(out spriteFader))
					{
						UnityEngine.Object.Destroy(rigidbody2D2.GetComponent<Collider2D>());
						UnityEngine.Object.Destroy(rigidbody2D2);
					}
					else
					{
						rigidbody2D2.gameObject.layer = 0;
						rigidbody2D2.velocity = Vector2.zero;
					}
				}
			}
			if (this.rigidbodies.Count == 0)
			{
				Collider2D[] array = this.bottomColliders;
				for (int j = 0; j < array.Length; j++)
				{
					UnityEngine.Object.Destroy(array[j]);
				}
				UnityEngine.Object.Destroy(this);
			}
		}

		// Token: 0x04000B63 RID: 2915
		public bool enablePhysics;

		// Token: 0x04000B64 RID: 2916
		public float maxForce = 2f;

		// Token: 0x04000B65 RID: 2917
		public float maxTorque = 2f;

		// Token: 0x04000B66 RID: 2918
		public float gravityScale = 100f;

		// Token: 0x04000B67 RID: 2919
		public Collider2D[] bottomColliders;

		// Token: 0x04000B68 RID: 2920
		private readonly List<Rigidbody2D> rigidbodies = new List<Rigidbody2D>(25);

		// Token: 0x04000B69 RID: 2921
		private readonly List<Rigidbody2D> staticRigidbodies = new List<Rigidbody2D>(25);
	}
}
