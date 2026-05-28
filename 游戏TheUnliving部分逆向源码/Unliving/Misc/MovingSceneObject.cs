using System;
using UnityEngine;

namespace Unliving.Misc
{
	// Token: 0x02000241 RID: 577
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(Rigidbody2D))]
	public class MovingSceneObject : MonoBehaviour
	{
		// Token: 0x06001394 RID: 5012 RVA: 0x0003D662 File Offset: 0x0003B862
		private void Awake()
		{
			this.animator = base.GetComponent<Animator>();
			this.rigidbody = base.GetComponent<Rigidbody2D>();
		}

		// Token: 0x06001395 RID: 5013 RVA: 0x0003D67C File Offset: 0x0003B87C
		private void Update()
		{
			float num = (Mathf.Abs(this.rigidbody.velocity.x) > Mathf.Abs(this.rigidbody.velocity.y)) ? this.rigidbody.velocity.x : this.rigidbody.velocity.y;
			this.animator.SetFloat(MovingSceneObject.AnimatorMovementParamID, num * this.animationSpeedMult);
		}

		// Token: 0x04000B6A RID: 2922
		protected static readonly int AnimatorMovementParamID = Animator.StringToHash("MoveSpeed");

		// Token: 0x04000B6B RID: 2923
		public float animationSpeedMult = 0.1f;

		// Token: 0x04000B6C RID: 2924
		private Animator animator;

		// Token: 0x04000B6D RID: 2925
		private Rigidbody2D rigidbody;
	}
}
