using System;
using Game.Core;
using UnityEngine;
using Unliving.Player;

namespace Unliving
{
	// Token: 0x0200000B RID: 11
	public class ParallaxEffect : GameBehaviourBase
	{
		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000026 RID: 38 RVA: 0x000028D4 File Offset: 0x00000AD4
		private Vector2 travel
		{
			get
			{
				return this.gameCamera.transform.position - (this.startPosition + this.centerShift);
			}
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00002904 File Offset: 0x00000B04
		public void Start()
		{
			this.startPosition = base.transform.position;
			this.startZ = base.transform.position.z;
			PlayerBehaviour playerBehaviour;
			if (!base.CurrentGame.TryGetPlayer(out playerBehaviour))
			{
				Debug.LogError("Failed to get player for the Parallax component");
			}
			if (!base.CurrentGame.Services.TryGet<PlayerCameraFollow>(out this.gameCamera))
			{
				Debug.LogError("Failed to get camera for the Parallax component");
			}
			this.playerTransform = playerBehaviour.transform;
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002984 File Offset: 0x00000B84
		public void Update()
		{
			Vector2 vector = this.startPosition + this.travel * this.parallaxFactor;
			base.transform.position = new Vector3(vector.x, vector.y, this.startZ);
		}

		// Token: 0x06000029 RID: 41 RVA: 0x000029D0 File Offset: 0x00000BD0
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(new Vector3(this.centerShift.x + base.transform.position.x, this.centerShift.y + base.transform.position.y, 0f), 0.3f);
		}

		// Token: 0x04000015 RID: 21
		private PlayerCameraFollow gameCamera;

		// Token: 0x04000016 RID: 22
		private Transform playerTransform;

		// Token: 0x04000017 RID: 23
		[Range(-1f, 1f)]
		public float parallaxFactor;

		// Token: 0x04000018 RID: 24
		[Tooltip("Shift the object's center point; parallax shift is zero when the camera is in the center point")]
		public Vector2 centerShift;

		// Token: 0x04000019 RID: 25
		private Vector2 startPosition;

		// Token: 0x0400001A RID: 26
		private float startZ;
	}
}
