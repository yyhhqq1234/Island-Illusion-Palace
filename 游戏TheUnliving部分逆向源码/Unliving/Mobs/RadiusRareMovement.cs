using System;
using UnityEngine;
using UnityEngine.AI;

namespace Unliving.Mobs
{
	// Token: 0x020001FA RID: 506
	public sealed class RadiusRareMovement : GroupDestinationsGeneratorBase
	{
		// Token: 0x1700037E RID: 894
		// (get) Token: 0x060010E5 RID: 4325 RVA: 0x00034E2D File Offset: 0x0003302D
		public override bool GenerateIndividualMobDestinations
		{
			get
			{
				return this.generateIndividualPoints;
			}
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x00034E38 File Offset: 0x00033038
		private void Start()
		{
			if (this.customRadius > 0f)
			{
				this.currentRadius = this.customRadius;
				return;
			}
			MobBehaviourSpawner mobBehaviourSpawner;
			if (base.TryGetComponent<MobBehaviourSpawner>(out mobBehaviourSpawner))
			{
				this.customRadius = mobBehaviourSpawner.MaxPursuitDistance;
			}
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x00034E78 File Offset: 0x00033078
		public override bool TryGetNewDestination(out Vector2 position, out bool isForcedPosition)
		{
			if (base.TryGetNewDestination(out position, out isForcedPosition) && this.currentRadius > 0f)
			{
				GameMobsGroupControllerBase currentGroup = base.CurrentGroup;
				Vector2 vector = (currentGroup != null) ? currentGroup.InitialPosition : base.transform.position;
				Vector2 vector2 = vector + UnityEngine.Random.insideUnitCircle * this.currentRadius;
				NavMeshHit navMeshHit;
				if (NavMesh.Raycast(vector, vector2, out navMeshHit, -1))
				{
					vector2 = navMeshHit.position;
				}
				position = vector2;
			}
			return true;
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x00034EFE File Offset: 0x000330FE
		private void OnDrawGizmosSelected()
		{
			if (this.currentRadius > 0f)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(base.transform.position, this.currentRadius);
			}
		}

		// Token: 0x04000991 RID: 2449
		public float customRadius;

		// Token: 0x04000992 RID: 2450
		public bool generateIndividualPoints;

		// Token: 0x04000993 RID: 2451
		private float currentRadius;
	}
}
