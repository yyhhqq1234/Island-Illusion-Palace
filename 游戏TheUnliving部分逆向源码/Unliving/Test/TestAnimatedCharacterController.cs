using System;
using Game.Damage.Projectiles;
using UnityEngine;

namespace Unliving.Test
{
	// Token: 0x02000042 RID: 66
	public sealed class TestAnimatedCharacterController : MonoBehaviour
	{
		// Token: 0x06000231 RID: 561 RVA: 0x00009264 File Offset: 0x00007464
		private void UpdateVelocity()
		{
			if (this.desiredVelocity.x != 0f || this.desiredVelocity.y != 0f)
			{
				return;
			}
			this.desiredVelocity.x = Input.GetAxisRaw("Horizontal");
			this.desiredVelocity.y = Input.GetAxisRaw("Vertical");
			this.desiredVelocity.Normalize();
			this.desiredVelocity *= this.movementSpeed;
		}

		// Token: 0x06000232 RID: 562 RVA: 0x000092E2 File Offset: 0x000074E2
		private void LaunchProjectile()
		{
		}

		// Token: 0x06000233 RID: 563 RVA: 0x000092E4 File Offset: 0x000074E4
		private void UpdateAttackAnimationIndex()
		{
			this.currentAttackIndex = (this.currentAttackIndex + 1) % TestAnimatedCharacterController.AttackFlagsID.Length;
		}

		// Token: 0x06000234 RID: 564 RVA: 0x000092FC File Offset: 0x000074FC
		private void Awake()
		{
			this.currentCamera = Camera.main;
			this.rigidbody = base.GetComponent<Rigidbody2D>();
			this.animator = base.GetComponent<Animator>();
			TestAnimatedCharacterController.ProjectileLaunchArgs.launcher = this;
			this.nextProjectileLaunchTime = -1f;
			this.currentAttackIndex = 0;
		}

		// Token: 0x06000235 RID: 565 RVA: 0x0000934C File Offset: 0x0000754C
		private void Update()
		{
			this.desiredVelocity = default(Vector2);
			bool flag = false;
			AnimatorStateInfo currentAnimatorStateInfo = this.animator.GetCurrentAnimatorStateInfo(0);
			if (this.currentCamera != null)
			{
				Vector2 vector = this.currentCamera.ScreenToWorldPoint(Input.mousePosition);
				this.currentLookDirection = Mathf.Sign(vector.x - base.transform.position.x);
				base.transform.localScale = new Vector3(this.currentLookDirection, 1f, 1f);
			}
			if (Input.GetMouseButton(0))
			{
				flag = true;
			}
			if (currentAnimatorStateInfo.tagHash == TestAnimatedCharacterController.AttackStateTagID)
			{
				this.isAttackStateActive = true;
				if (this.switchAttackAnimations)
				{
					int id = TestAnimatedCharacterController.AttackFlagsID[this.currentAttackIndex];
					if (currentAnimatorStateInfo.normalizedTime % 1f > 0.98f && this.animator.GetBool(id))
					{
						this.animator.SetBool(id, false);
						this.UpdateAttackAnimationIndex();
					}
				}
				else if (Time.time > this.nextProjectileLaunchTime)
				{
					this.LaunchProjectile();
					this.nextProjectileLaunchTime = Time.time + Mathf.Max(this.projectileLaunchDelay, 0.1f);
				}
			}
			else
			{
				if (!flag && this.isAttackStateActive)
				{
					if (this.switchAttackAnimations)
					{
						this.UpdateAttackAnimationIndex();
					}
					this.isAttackStateActive = false;
				}
				this.UpdateVelocity();
				this.nextProjectileLaunchTime = -1f;
			}
			if (!this.freezeOnAttack)
			{
				this.UpdateVelocity();
			}
			this.animator.SetBool(TestAnimatedCharacterController.MovementFlagID, this.desiredVelocity.SqrMagnitude() > 0.1f);
			this.animator.SetBool(TestAnimatedCharacterController.AttackFlagsID[this.currentAttackIndex], flag);
		}

		// Token: 0x06000236 RID: 566 RVA: 0x000094F5 File Offset: 0x000076F5
		private void LateUpdate()
		{
			this.rigidbody.velocity = this.desiredVelocity;
		}

		// Token: 0x06000237 RID: 567 RVA: 0x00009508 File Offset: 0x00007708
		private void OnAnimationEventFired(string eventID)
		{
			if (this.switchAttackAnimations && eventID.Equals(TestAnimatedCharacterController.AttackAnimationEventID, StringComparison.OrdinalIgnoreCase))
			{
				this.LaunchProjectile();
			}
		}

		// Token: 0x04000150 RID: 336
		private const float MovementStateThreshold = 0.1f;

		// Token: 0x04000151 RID: 337
		private static readonly int MovementFlagID = Animator.StringToHash("Move");

		// Token: 0x04000152 RID: 338
		private static readonly int[] AttackFlagsID = new int[]
		{
			Animator.StringToHash("Attack"),
			Animator.StringToHash("SecondaryAttack")
		};

		// Token: 0x04000153 RID: 339
		private static readonly int AttackStateTagID = Animator.StringToHash("AttackState");

		// Token: 0x04000154 RID: 340
		private static readonly string AttackAnimationEventID = "Attack";

		// Token: 0x04000155 RID: 341
		private static readonly ProjectileLaunchArgs ProjectileLaunchArgs = new ProjectileLaunchArgs();

		// Token: 0x04000156 RID: 342
		public float movementSpeed = 5f;

		// Token: 0x04000157 RID: 343
		public bool freezeOnAttack = true;

		// Token: 0x04000158 RID: 344
		public bool switchAttackAnimations;

		// Token: 0x04000159 RID: 345
		public Transform projectileLaunchPivot;

		// Token: 0x0400015A RID: 346
		public float projectileLaunchDelay = 1f;

		// Token: 0x0400015B RID: 347
		private Camera currentCamera;

		// Token: 0x0400015C RID: 348
		private Rigidbody2D rigidbody;

		// Token: 0x0400015D RID: 349
		private Animator animator;

		// Token: 0x0400015E RID: 350
		private Vector2 desiredVelocity;

		// Token: 0x0400015F RID: 351
		private float currentLookDirection;

		// Token: 0x04000160 RID: 352
		private float nextProjectileLaunchTime;

		// Token: 0x04000161 RID: 353
		private int currentAttackIndex;

		// Token: 0x04000162 RID: 354
		private bool isAttackStateActive;
	}
}
