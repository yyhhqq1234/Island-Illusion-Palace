using System;
using Common;
using UnityEngine;

namespace Unliving.Mobs
{
	// Token: 0x020001B5 RID: 437
	public sealed class GameMobAIControllerParams : ICloneable<GameMobAIControllerParams>
	{
		// Token: 0x17000241 RID: 577
		// (get) Token: 0x06000CB6 RID: 3254 RVA: 0x00028838 File Offset: 0x00026A38
		// (set) Token: 0x06000CB7 RID: 3255 RVA: 0x00028863 File Offset: 0x00026A63
		public float TargetSearchRadius
		{
			get
			{
				float? num = this.tempTargetSearchRadius;
				if (num == null)
				{
					return this.targetSearchRadius;
				}
				return num.GetValueOrDefault();
			}
			set
			{
				this.targetSearchRadius = value;
			}
		}

		// Token: 0x06000CB8 RID: 3256 RVA: 0x0002886C File Offset: 0x00026A6C
		public bool IsAttackLayers(int layers)
		{
			return (this.attackLayers & layers) != 0;
		}

		// Token: 0x06000CB9 RID: 3257 RVA: 0x0002887E File Offset: 0x00026A7E
		public GameMobAIControllerParams Clone()
		{
			return (GameMobAIControllerParams)base.MemberwiseClone();
		}

		// Token: 0x06000CBA RID: 3258 RVA: 0x0002888B File Offset: 0x00026A8B
		public void SetTempTargetSearchRadius(float radius)
		{
			this.tempTargetSearchRadius = new float?(radius);
		}

		// Token: 0x06000CBB RID: 3259 RVA: 0x00028899 File Offset: 0x00026A99
		public void ResetTempTargetSearchRadius()
		{
			this.tempTargetSearchRadius = null;
		}

		// Token: 0x04000746 RID: 1862
		public GameMobTargetSelector.SelectionMethod targetSelectionMethod;

		// Token: 0x04000747 RID: 1863
		public GameMobTargetSelector.PrioritySelector priorityTargetSelector = GameMobTargetSelector.PrioritySelector.Default;

		// Token: 0x04000748 RID: 1864
		public LayerMask attackLayers = 1;

		// Token: 0x04000749 RID: 1865
		public GameMobDescription additionalAttackTargetsFilter = GameMobDescription.BlankDescription;

		// Token: 0x0400074A RID: 1866
		public LayerMask aggressionObstacleLayers = 0;

		// Token: 0x0400074B RID: 1867
		public LayerMask fearStateObstaclesLayers = 8192;

		// Token: 0x0400074C RID: 1868
		public float attackTargetChasingSpeedMultiplier;

		// Token: 0x0400074D RID: 1869
		public float minTargetFocusDuration = 3f;

		// Token: 0x0400074E RID: 1870
		public float maxTargetFollowingDuration = 5f;

		// Token: 0x0400074F RID: 1871
		public bool hasResponseAggression;

		// Token: 0x04000750 RID: 1872
		public bool shareAggression = true;

		// Token: 0x04000751 RID: 1873
		public bool isAggressiveByDefault = true;

		// Token: 0x04000752 RID: 1874
		public bool usePlayerAsDefaultAttackTarget;

		// Token: 0x04000753 RID: 1875
		public float fearStateSpeedMultiplier;

		// Token: 0x04000754 RID: 1876
		public bool canUseSupportAbilities = true;

		// Token: 0x04000755 RID: 1877
		public bool canBeScared = true;

		// Token: 0x04000756 RID: 1878
		public bool canUseSpecialStatuses = true;

		// Token: 0x04000757 RID: 1879
		private float targetSearchRadius = 5f;

		// Token: 0x04000758 RID: 1880
		private float? tempTargetSearchRadius;
	}
}
