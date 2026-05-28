using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unliving.Test
{
	// Token: 0x02000038 RID: 56
	public sealed class DestructionForceTests : MonoBehaviour
	{
		// Token: 0x060001FE RID: 510 RVA: 0x00007D83 File Offset: 0x00005F83
		private IEnumerator Start()
		{
			yield return new WaitForSecondsRealtime(2f + UnityEngine.Random.value);
			Vector2 b = base.transform.TransformPoint(this.localExplosionCenter);
			base.GetComponentsInChildren<Rigidbody2D>(this.debris);
			Vector3 vector = base.transform.TransformPoint(this.localGroundPoint);
			for (int i = 0; i < this.debris.Count; i++)
			{
				this.debris[i].mass = UnityEngine.Random.Range(this.minFragmentMass, this.maxFragmentMass);
				this.debris[i].gravityScale = 0f;
				this.debris[i].drag = 0f;
				this.debris[i].angularDrag = 2f;
				float height = Mathf.Abs(this.debris[i].transform.position.y - vector.y);
				Vector2 vector2 = Vector2.Scale((this.debris[i].position - b).normalized, this.explosionImpulse);
				this.debris[i].AddTorque((UnityEngine.Random.value * 2f - 1f) * this.maxFragmentAngularImpulse, ForceMode2D.Impulse);
				this.bodies.Add(new DestructionForceTests.SimulatedBody(this.debris[i], this.bounciness, height, new Vector3(vector2.x, vector2.y, this.verticalImpulse)));
			}
			yield break;
		}

		// Token: 0x060001FF RID: 511 RVA: 0x00007D94 File Offset: 0x00005F94
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.R))
			{
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}

		// Token: 0x06000200 RID: 512 RVA: 0x00007DBC File Offset: 0x00005FBC
		private void FixedUpdate()
		{
			for (int i = 0; i < this.bodies.Count; i++)
			{
				this.bodies[i].Update(Time.deltaTime);
			}
		}

		// Token: 0x06000201 RID: 513 RVA: 0x00007DF8 File Offset: 0x00005FF8
		private void OnDrawGizmos()
		{
			Vector3 center = base.transform.TransformPoint(this.localGroundPoint);
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(center, 0.5f);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(base.transform.TransformPoint(this.localExplosionCenter), 0.1f);
		}

		// Token: 0x04000107 RID: 263
		public Vector2 localGroundPoint;

		// Token: 0x04000108 RID: 264
		public Vector2 localExplosionCenter;

		// Token: 0x04000109 RID: 265
		[HideInInspector]
		public float gravity = 10f;

		// Token: 0x0400010A RID: 266
		public float minFragmentMass = 1f;

		// Token: 0x0400010B RID: 267
		public float maxFragmentMass = 3f;

		// Token: 0x0400010C RID: 268
		[Range(0f, 0.9f)]
		public float bounciness = 0.5f;

		// Token: 0x0400010D RID: 269
		public Vector2 explosionImpulse = new Vector2(2f, 2f);

		// Token: 0x0400010E RID: 270
		public float verticalImpulse = 4f;

		// Token: 0x0400010F RID: 271
		public float maxFragmentAngularImpulse = 10f;

		// Token: 0x04000110 RID: 272
		[HideInInspector]
		public float minDebrisPhysicsLifetime = 0.2f;

		// Token: 0x04000111 RID: 273
		[HideInInspector]
		public float maxDebrisPhysicsLifetime = 0.5f;

		// Token: 0x04000112 RID: 274
		private readonly List<DestructionForceTests.SimulatedBody> bodies = new List<DestructionForceTests.SimulatedBody>(50);

		// Token: 0x04000113 RID: 275
		private readonly List<Rigidbody2D> debris = new List<Rigidbody2D>(50);

		// Token: 0x02000410 RID: 1040
		private sealed class SimulatedBody
		{
			// Token: 0x0600224F RID: 8783 RVA: 0x0006A950 File Offset: 0x00068B50
			private static float GetDragCoeff(float drag, float dt)
			{
				return Mathf.Clamp01(1f - drag * dt);
			}

			// Token: 0x06002250 RID: 8784 RVA: 0x0006A960 File Offset: 0x00068B60
			public SimulatedBody(Rigidbody2D rigidbody, float bounciness, float height, Vector3 initialImpulse)
			{
				this.rigidbody = rigidbody;
				this.bounciness = bounciness;
				this.height = height;
				this.velocity = default(Vector2);
				this.verticalVelocity = 0f;
				this.AddImpulse(initialImpulse);
				this.AddVerticalImpulse(initialImpulse.z);
			}

			// Token: 0x06002251 RID: 8785 RVA: 0x0006A9B9 File Offset: 0x00068BB9
			public void AddImpulse(Vector2 impulse)
			{
				this.velocity += impulse / this.rigidbody.mass;
			}

			// Token: 0x06002252 RID: 8786 RVA: 0x0006A9DD File Offset: 0x00068BDD
			public void AddVerticalImpulse(float impulse)
			{
				this.verticalVelocity += impulse / this.rigidbody.mass;
			}

			// Token: 0x06002253 RID: 8787 RVA: 0x0006A9FC File Offset: 0x00068BFC
			public void Update(float dt)
			{
				if (this.height > 0f)
				{
					this.verticalVelocity -= 10f * dt;
				}
				else
				{
					this.rigidbody.angularVelocity *= DestructionForceTests.SimulatedBody.GetDragCoeff(8f, dt);
					if (this.bounciness <= 0f)
					{
						this.verticalVelocity = 0f;
					}
					else
					{
						this.verticalVelocity = -this.verticalVelocity * Mathf.Min(this.bounciness, 0.9f);
					}
				}
				this.height += this.verticalVelocity * dt;
				this.velocity *= DestructionForceTests.SimulatedBody.GetDragCoeff(4f, dt);
				this.rigidbody.velocity = this.velocity + new Vector2
				{
					y = this.verticalVelocity
				};
			}

			// Token: 0x040015B7 RID: 5559
			private const float Gravity = 10f;

			// Token: 0x040015B8 RID: 5560
			private const float Drag = 4f;

			// Token: 0x040015B9 RID: 5561
			private const float GroundDrag = 8f;

			// Token: 0x040015BA RID: 5562
			private readonly Rigidbody2D rigidbody;

			// Token: 0x040015BB RID: 5563
			private readonly float bounciness;

			// Token: 0x040015BC RID: 5564
			private float height;

			// Token: 0x040015BD RID: 5565
			private Vector2 velocity;

			// Token: 0x040015BE RID: 5566
			private float verticalVelocity;
		}
	}
}
