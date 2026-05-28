using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unliving.Mobs
{
	// Token: 0x020001FC RID: 508
	public sealed class WayPointsMovement : GroupDestinationsGeneratorBase
	{
		// Token: 0x1700037F RID: 895
		// (get) Token: 0x060010ED RID: 4333 RVA: 0x00034F8C File Offset: 0x0003318C
		// (set) Token: 0x060010EE RID: 4334 RVA: 0x00034F94 File Offset: 0x00033194
		public Transform[] WayPoints
		{
			get
			{
				return this.wayPoints;
			}
			set
			{
				this.wayPoints = value;
			}
		}

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x060010EF RID: 4335 RVA: 0x00034F9D File Offset: 0x0003319D
		public override bool GenerateIndividualMobDestinations
		{
			get
			{
				return this.individualWaypointDestinationReachDistance >= 0f;
			}
		}

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x060010F0 RID: 4336 RVA: 0x00034FAF File Offset: 0x000331AF
		public int CurrentWaypointIndex
		{
			get
			{
				return this.currentWaypointIndex;
			}
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x00034FB8 File Offset: 0x000331B8
		private int UpdateCurrentWaypointIndex()
		{
			if (this.isLooped)
			{
				int num = this.currentWaypointIndex + 1;
				this.currentWaypointIndex = num;
				return num % this.wayPoints.Length;
			}
			if (this.currentWaypointIndex < this.wayPoints.Length - 1)
			{
				int num = this.currentWaypointIndex + 1;
				this.currentWaypointIndex = num;
				return num;
			}
			return -1;
		}

		// Token: 0x060010F2 RID: 4338 RVA: 0x00035010 File Offset: 0x00033210
		private Vector2 GetCurrentWaypointDirection(out Vector2 waypoint0, out Vector2 waypoint1)
		{
			if (this.currentWaypointIndex > 0)
			{
				waypoint0 = this.wayPoints[this.currentWaypointIndex - 1].position;
				waypoint1 = this.wayPoints[this.currentWaypointIndex].position;
			}
			else
			{
				waypoint0 = this.wayPoints[0].position;
				waypoint1 = this.wayPoints[1].position;
			}
			Vector2 result = waypoint1 - waypoint0;
			result.Normalize();
			return result;
		}

		// Token: 0x060010F3 RID: 4339 RVA: 0x000350B0 File Offset: 0x000332B0
		public override bool TryGetNewDestination(out Vector2 position, out bool isForcedPosition)
		{
			if (base.TryGetNewDestination(out position, out isForcedPosition) && this.wayPoints != null && this.wayPoints.Length != 0)
			{
				int num = this.UpdateCurrentWaypointIndex();
				if (num >= 0)
				{
					position = this.wayPoints[num].position;
					return true;
				}
			}
			return false;
		}

		// Token: 0x060010F4 RID: 4340 RVA: 0x00035100 File Offset: 0x00033300
		public override bool TryGetAdditionalVelocity(Vector2 mobPosition, float mobSpeed, out Vector2 additionalVelocity)
		{
			if (this.tryKeepMobsOnPath && this.currentWaypointIndex > 0 && this.currentWaypointIndex < this.wayPoints.Length)
			{
				Vector2 vector;
				Vector2 vector2;
				Vector2 currentWaypointDirection = this.GetCurrentWaypointDirection(out vector, out vector2);
				Vector2 a = vector + Vector2.Dot(mobPosition - vector, currentWaypointDirection) * currentWaypointDirection;
				additionalVelocity = a - mobPosition;
				float sqrMagnitude = additionalVelocity.sqrMagnitude;
				float num = Mathf.InverseLerp(0f, 0.7f, sqrMagnitude);
				if (num > 0.01f)
				{
					additionalVelocity.Normalize();
					additionalVelocity *= mobSpeed * num;
					return true;
				}
			}
			return base.TryGetAdditionalVelocity(mobPosition, mobSpeed, out additionalVelocity);
		}

		// Token: 0x060010F5 RID: 4341 RVA: 0x000351B0 File Offset: 0x000333B0
		public override void OnCloned(GroupDestinationsGeneratorBase originalDestinationsGenerator, bool keepState)
		{
			base.OnCloned(originalDestinationsGenerator, keepState);
			WayPointsMovement wayPointsMovement = (WayPointsMovement)originalDestinationsGenerator;
			this.wayPoints = (Transform[])wayPointsMovement.WayPoints.Clone();
			if (keepState)
			{
				this.currentWaypointIndex = wayPointsMovement.CurrentWaypointIndex;
			}
		}

		// Token: 0x060010F6 RID: 4342 RVA: 0x000351F4 File Offset: 0x000333F4
		private void OnDrawGizmosSelected()
		{
			if (this.wayPoints != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(base.transform.position, this.wayPoints[0].position);
				for (int i = 0; i < this.wayPoints.Length; i++)
				{
					if (!(this.wayPoints[i] == null))
					{
						Gizmos.DrawWireSphere(this.wayPoints[i].position, 0.2f);
						if (i == this.wayPoints.Length - 1)
						{
							if (this.isLooped)
							{
								Gizmos.DrawLine(this.wayPoints[i].position, this.wayPoints[0].position);
							}
						}
						else if (!(this.wayPoints[i + 1] == null))
						{
							Gizmos.DrawLine(this.wayPoints[i].position, this.wayPoints[i + 1].position);
						}
					}
				}
			}
		}

		// Token: 0x04000995 RID: 2453
		[SerializeField]
		[FormerlySerializedAs("_wayPoints")]
		private Transform[] wayPoints;

		// Token: 0x04000996 RID: 2454
		public bool isLooped = true;

		// Token: 0x04000997 RID: 2455
		public float individualWaypointDestinationReachDistance = -1f;

		// Token: 0x04000998 RID: 2456
		public bool tryKeepMobsOnPath;

		// Token: 0x04000999 RID: 2457
		private int currentWaypointIndex = -1;
	}
}
