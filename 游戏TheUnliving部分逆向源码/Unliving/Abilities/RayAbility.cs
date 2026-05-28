using System;
using System.Collections.Generic;
using Game.Abilities;
using Game.Stats;
using UnityEngine;
using Unliving.MobsStats;

namespace Unliving.Abilities
{
	// Token: 0x020003B4 RID: 948
	[CreateAssetMenu(fileName = "RayAbility", menuName = "Abilities/Ray Ability")]
	public sealed class RayAbility : EffectBasedAbility, IMobStatsListProvider, IStatsListProvider<MobStatModifier>
	{
		// Token: 0x17000658 RID: 1624
		// (get) Token: 0x06001F92 RID: 8082 RVA: 0x00063594 File Offset: 0x00061794
		// (set) Token: 0x06001F93 RID: 8083 RVA: 0x0006359C File Offset: 0x0006179C
		public override int ID { get; set; }

		// Token: 0x17000659 RID: 1625
		// (get) Token: 0x06001F94 RID: 8084 RVA: 0x000635A5 File Offset: 0x000617A5
		// (set) Token: 0x06001F95 RID: 8085 RVA: 0x000635AD File Offset: 0x000617AD
		public override int Type
		{
			get
			{
				return (int)this.abilityType;
			}
			set
			{
				this.abilityType = (AbilityTypes)value;
			}
		}

		// Token: 0x1700065A RID: 1626
		// (get) Token: 0x06001F96 RID: 8086 RVA: 0x000635B6 File Offset: 0x000617B6
		// (set) Token: 0x06001F97 RID: 8087 RVA: 0x000635BE File Offset: 0x000617BE
		public float RayThickness
		{
			get
			{
				return this._rayThickness;
			}
			set
			{
				this._rayThickness = value;
			}
		}

		// Token: 0x1700065B RID: 1627
		// (get) Token: 0x06001F98 RID: 8088 RVA: 0x000635C7 File Offset: 0x000617C7
		// (set) Token: 0x06001F99 RID: 8089 RVA: 0x000635CF File Offset: 0x000617CF
		public int MaxAffectedObjectsCount
		{
			get
			{
				return this._maxAffectedObjectsCount;
			}
			set
			{
				this._maxAffectedObjectsCount = value;
			}
		}

		// Token: 0x1700065C RID: 1628
		// (get) Token: 0x06001F9A RID: 8090 RVA: 0x000635D8 File Offset: 0x000617D8
		// (set) Token: 0x06001F9B RID: 8091 RVA: 0x000635E0 File Offset: 0x000617E0
		public LayerMask RayObstacleLayers
		{
			get
			{
				return this._rayObstacleLayers;
			}
			set
			{
				this._rayObstacleLayers = value;
			}
		}

		// Token: 0x1700065D RID: 1629
		// (get) Token: 0x06001F9C RID: 8092 RVA: 0x000635E9 File Offset: 0x000617E9
		public override bool IsTargetedAbility
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700065E RID: 1630
		// (get) Token: 0x06001F9D RID: 8093 RVA: 0x000635EC File Offset: 0x000617EC
		public override bool IsObjectTargetRequired
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700065F RID: 1631
		// (get) Token: 0x06001F9E RID: 8094 RVA: 0x000635EF File Offset: 0x000617EF
		public override bool CanBeUsedOnOwner
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000660 RID: 1632
		// (get) Token: 0x06001F9F RID: 8095 RVA: 0x000635F2 File Offset: 0x000617F2
		public override bool IsZoneEffectAbility
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000661 RID: 1633
		// (get) Token: 0x06001FA0 RID: 8096 RVA: 0x000635F5 File Offset: 0x000617F5
		public float CurrentRayLength
		{
			get
			{
				return this._currentRayLength;
			}
		}

		// Token: 0x17000662 RID: 1634
		// (get) Token: 0x06001FA1 RID: 8097 RVA: 0x000635FD File Offset: 0x000617FD
		IReadOnlyList<IModifiableStat<MobStatModifier>> IStatsListProvider<MobStatModifier>.Stats
		{
			get
			{
				return this.statsListProvider.Stats;
			}
		}

		// Token: 0x06001FA2 RID: 8098 RVA: 0x0006360C File Offset: 0x0006180C
		private float GetBlockingObjectDistance(int maxObjectsCount, Vector2 startPosition, Vector2 rayDirection)
		{
			float num = float.NegativeInfinity;
			for (int i = 0; i < maxObjectsCount; i++)
			{
				float num2 = Vector2.Dot(RayAbility.RayIntersectionsBuffer[i].transform.position - startPosition, rayDirection);
				if (num2 > num)
				{
					num = num2;
				}
			}
			return num;
		}

		// Token: 0x06001FA3 RID: 8099 RVA: 0x00063658 File Offset: 0x00061858
		protected override void SetAbilityTargets(BaseAbility.UsingArgs usingArgs)
		{
			Vector2 vector = base.OwnerPosition;
			Vector2 vector2 = this.currentUsingArgs.targetPosition - vector;
			this.targetRayLength = Mathf.Min(vector2.magnitude, base.Range);
			if (this.targetRayLength < 0.01f)
			{
				return;
			}
			if (this._rayObstacleLayers != 0)
			{
				RaycastHit2D raycastHit2D = Physics2D.Raycast(vector, vector2, this.targetRayLength, this._rayObstacleLayers);
				if (raycastHit2D.collider != null)
				{
					this.targetRayLength = raycastHit2D.distance + 0.1f;
				}
			}
			float angle = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
			Vector2 point = vector + vector2 * 0.5f;
			Vector2 size = new Vector2
			{
				x = this.targetRayLength,
				y = this._rayThickness
			};
			int num = Physics2D.OverlapCapsuleNonAlloc(point, size, CapsuleDirection2D.Horizontal, angle, RayAbility.RayIntersectionsBuffer, this._validObjectLayers);
			if (num != 0)
			{
				usingArgs.targetsList = RayAbility.RayIntersectionsBuffer;
				usingArgs.targetsCountOverride = ((this._maxAffectedObjectsCount > 0) ? Mathf.Min(this._maxAffectedObjectsCount, num) : num);
				float blockingObjectDistance = this.GetBlockingObjectDistance(usingArgs.targetsCountOverride, vector, vector2.normalized);
				if (blockingObjectDistance > 0f && this.targetRayLength > blockingObjectDistance)
				{
					this.targetRayLength = blockingObjectDistance;
				}
			}
			else
			{
				usingArgs.targetsList = null;
			}
			if (!this.HasUsingDuration())
			{
				this._currentRayLength = this.targetRayLength;
			}
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x000637DC File Offset: 0x000619DC
		public override void UpdateAbility(float deltaTime)
		{
			base.UpdateAbility(deltaTime);
			if (this.targetRayLength > 0f)
			{
				if (base.UsingLoopStep > 0f && Mathf.Abs(this._currentRayLength - this.targetRayLength) < 0.5f)
				{
					this._currentRayLength = Mathf.Lerp(this._currentRayLength, this.targetRayLength, 25f * deltaTime);
					return;
				}
				this._currentRayLength = this.targetRayLength;
			}
		}

		// Token: 0x06001FA5 RID: 8101 RVA: 0x0006384E File Offset: 0x00061A4E
		protected override void OnInitialize(object context)
		{
			base.OnInitialize(context);
			this.statsListProvider = new AbilityStatsListGenerator(this);
		}

		// Token: 0x06001FA6 RID: 8102 RVA: 0x00063863 File Offset: 0x00061A63
		protected override void OnCompleted(BaseAbility.UsingArgs usingArgs)
		{
			this._currentRayLength = 0f;
			this.targetRayLength = 0f;
			base.OnCompleted(usingArgs);
		}

		// Token: 0x040013F0 RID: 5104
		private static readonly Collider2D[] RayIntersectionsBuffer = new Collider2D[64];

		// Token: 0x040013F2 RID: 5106
		public AbilityTypes abilityType;

		// Token: 0x040013F3 RID: 5107
		[SerializeField]
		private float _rayThickness;

		// Token: 0x040013F4 RID: 5108
		[SerializeField]
		private int _maxAffectedObjectsCount;

		// Token: 0x040013F5 RID: 5109
		[SerializeField]
		private LayerMask _rayObstacleLayers;

		// Token: 0x040013F6 RID: 5110
		private float _currentRayLength;

		// Token: 0x040013F7 RID: 5111
		private float targetRayLength;

		// Token: 0x040013F8 RID: 5112
		private AbilityStatsListGenerator statsListProvider;
	}
}
