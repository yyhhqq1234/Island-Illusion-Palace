using System;
using System.Collections.Generic;
using Common.Math;
using Common.UnityExtensions;
using Game.Damage;
using UnityEngine;
using UnityEngine.AI;
using Unliving.LevelGeneration;
using Unliving.Mobs.Motion;

namespace Unliving.Mobs
{
	// Token: 0x020001F4 RID: 500
	[Serializable]
	public class GameMobGroupController : GameMobsGroupControllerBase
	{
		// Token: 0x06001079 RID: 4217 RVA: 0x0003376C File Offset: 0x0003196C
		static GameMobGroupController()
		{
			for (int i = 0; i < 500; i++)
			{
				Vector2 r2Value = PhiSequence.GetR2Value(i + 1, false);
				r2Value.x -= 0.5f;
				r2Value.y -= 0.5f;
				GameMobGroupController.FormationPlacementOffsets[i] = r2Value;
			}
			Array.Sort<Vector2>(GameMobGroupController.FormationPlacementOffsets, (Vector2 p0, Vector2 p1) => (p0.x * p0.x + p0.y * p0.y).CompareTo(p1.x * p1.x + p1.y * p1.y));
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x000337E8 File Offset: 0x000319E8
		private static Rect CreateRect(Vector4 minMaxRectPoints, float sizeAddition = 0f)
		{
			if (sizeAddition > 0f)
			{
				minMaxRectPoints.x -= sizeAddition;
				minMaxRectPoints.y -= sizeAddition;
				minMaxRectPoints.z += sizeAddition;
				minMaxRectPoints.w += sizeAddition;
			}
			return Rect.MinMaxRect(minMaxRectPoints.x, minMaxRectPoints.y, minMaxRectPoints.z, minMaxRectPoints.w);
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0003384C File Offset: 0x00031A4C
		private static Vector4 GetInitialMinMaxVector()
		{
			return new Vector4
			{
				x = float.PositiveInfinity,
				y = float.PositiveInfinity,
				z = float.NegativeInfinity,
				w = float.NegativeInfinity
			};
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x00033894 File Offset: 0x00031A94
		private static Vector4 GetMinMaxVector(Vector4 currentMinMax, Vector4 newMinMax)
		{
			if (currentMinMax.x > newMinMax.x)
			{
				currentMinMax.x = newMinMax.x;
			}
			if (currentMinMax.y > newMinMax.y)
			{
				currentMinMax.y = newMinMax.y;
			}
			if (currentMinMax.z < newMinMax.z)
			{
				currentMinMax.z = newMinMax.z;
			}
			if (currentMinMax.w < newMinMax.w)
			{
				currentMinMax.w = newMinMax.w;
			}
			return currentMinMax;
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x00033910 File Offset: 0x00031B10
		private static Vector2 GetCenterRayIntersectionPoint(Rect rect, Vector2 centerRayDirection)
		{
			if (centerRayDirection.x == 0f && centerRayDirection.y == 0f)
			{
				return rect.center;
			}
			Vector2 center = rect.center;
			Vector2 vector = new Vector2
			{
				x = rect.xMax - center.x,
				y = rect.yMax - center.y
			};
			Vector2 vector2 = vector;
			vector2.x /= Mathf.Abs(centerRayDirection.x);
			vector2.y /= Mathf.Abs(centerRayDirection.y);
			if (vector2.x < vector2.y)
			{
				vector = new Vector2
				{
					x = ((centerRayDirection.x > 0f) ? rect.xMax : rect.xMin),
					y = centerRayDirection.y * vector2.x + center.y
				};
				return vector;
			}
			vector = new Vector2
			{
				x = centerRayDirection.x * vector2.y + center.x,
				y = ((centerRayDirection.y > 0f) ? rect.yMax : rect.yMin)
			};
			return vector;
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x00033A48 File Offset: 0x00031C48
		public static void ExcludeFromHitPointsSums(BaseGameMob mob, ref float hitPointsSum, ref float maxHitPointsSum)
		{
			if (!mob.IsCharacterByDefault())
			{
				return;
			}
			IDamageable hitPointsController = mob.HitPointsController;
			if (hitPointsController != null)
			{
				hitPointsSum -= hitPointsController.CurrentHitPoints;
				maxHitPointsSum -= hitPointsController.MaxHitPoints;
			}
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x00033A80 File Offset: 0x00031C80
		public static bool IsDeadGroup(GameMobsGroupControllerBase group)
		{
			if (group.HasMobs)
			{
				GameMobGroupController gameMobGroupController = group as GameMobGroupController;
				return gameMobGroupController != null && gameMobGroupController.CharactersCount == 0;
			}
			return true;
		}

		// Token: 0x17000359 RID: 857
		// (get) Token: 0x06001080 RID: 4224 RVA: 0x00033AAC File Offset: 0x00031CAC
		// (set) Token: 0x06001081 RID: 4225 RVA: 0x00033AB4 File Offset: 0x00031CB4
		public override IGameMob Leader
		{
			get
			{
				return this.leader;
			}
			set
			{
				if (this.leader != value)
				{
					this.arrivalRadius = ((value != null) ? (value.Radius + 0.3f) : 0.3f);
					this.leader = value;
				}
			}
		}

		// Token: 0x1700035A RID: 858
		// (get) Token: 0x06001082 RID: 4226 RVA: 0x00033AE4 File Offset: 0x00031CE4
		// (set) Token: 0x06001083 RID: 4227 RVA: 0x00033B0E File Offset: 0x00031D0E
		public sealed override Vector2? GroupDestination
		{
			get
			{
				if (this.groupDestination == null)
				{
					return null;
				}
				return this.groupDestination.Value;
			}
			set
			{
				this.SetDestination(value, null, false, false);
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x06001084 RID: 4228 RVA: 0x00033B1B File Offset: 0x00031D1B
		// (set) Token: 0x06001085 RID: 4229 RVA: 0x00033B23 File Offset: 0x00031D23
		public IGroupDestinationsGenerator GroupDestinationsGenerator
		{
			get
			{
				return this.destinationsGenerator;
			}
			set
			{
				this.destinationsGenerator = value;
				if (this.destinationsGenerator != null)
				{
					this.destinationsGenerator.CurrentGroup = this;
				}
			}
		}

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06001086 RID: 4230 RVA: 0x00033B40 File Offset: 0x00031D40
		// (set) Token: 0x06001087 RID: 4231 RVA: 0x00033B48 File Offset: 0x00031D48
		public float CombatSupportCallHPThreshold
		{
			get
			{
				return this.combatSupportCallHPThreshold;
			}
			set
			{
				this.combatSupportCallHPThreshold = Mathf.Clamp01(value);
			}
		}

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06001088 RID: 4232 RVA: 0x00033B56 File Offset: 0x00031D56
		public int CharactersCount
		{
			get
			{
				return this.charactersCount;
			}
		}

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06001089 RID: 4233 RVA: 0x00033B5E File Offset: 0x00031D5E
		public float MinGroupRadius
		{
			get
			{
				return this.minGroupRadius;
			}
		}

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x0600108A RID: 4234 RVA: 0x00033B66 File Offset: 0x00031D66
		public Rect TotalGroupRect
		{
			get
			{
				return this.totalGroupRect;
			}
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x0600108B RID: 4235 RVA: 0x00033B6E File Offset: 0x00031D6E
		public override Vector2 Position
		{
			get
			{
				return this.totalGroupRect.center;
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x0600108C RID: 4236 RVA: 0x00033B7B File Offset: 0x00031D7B
		public Rect DestinationMovementRect
		{
			get
			{
				return this.destinationMovementRect;
			}
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x0600108D RID: 4237 RVA: 0x00033B83 File Offset: 0x00031D83
		public Vector2 DesiredGroupVelocity
		{
			get
			{
				return this.desiredGroupVelocity;
			}
		}

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x0600108E RID: 4238 RVA: 0x00033B8B File Offset: 0x00031D8B
		public override Vector2 GroupDestinationDirection
		{
			get
			{
				return this.groupDestinationDirection;
			}
		}

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x0600108F RID: 4239 RVA: 0x00033B93 File Offset: 0x00031D93
		public override bool HasForcedGroupDestination
		{
			get
			{
				return this.hasForcedGroupDestination;
			}
		}

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x06001090 RID: 4240 RVA: 0x00033B9B File Offset: 0x00031D9B
		public override bool IsGroupDestinationReached
		{
			get
			{
				return this.isGroupDestinationReached;
			}
		}

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x06001091 RID: 4241 RVA: 0x00033BA3 File Offset: 0x00031DA3
		public override bool IsFollowingGroupLeader
		{
			get
			{
				return false;
			}
		}

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06001092 RID: 4242 RVA: 0x00033BA6 File Offset: 0x00031DA6
		public override bool IsVisibleGroup
		{
			get
			{
				return this.isVisibleGroup;
			}
		}

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06001093 RID: 4243 RVA: 0x00033BAE File Offset: 0x00031DAE
		public override bool HasAttackTargets
		{
			get
			{
				return this.hasAttackTargets;
			}
		}

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06001094 RID: 4244 RVA: 0x00033BB6 File Offset: 0x00031DB6
		public bool IsAttacking
		{
			get
			{
				return this.isAttacking;
			}
		}

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06001095 RID: 4245 RVA: 0x00033BBE File Offset: 0x00031DBE
		public bool IsUnderAttack
		{
			get
			{
				return this.isUnderAttack;
			}
		}

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06001096 RID: 4246 RVA: 0x00033BC6 File Offset: 0x00031DC6
		public override bool InBattle
		{
			get
			{
				return this.inBattle;
			}
		}

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06001097 RID: 4247 RVA: 0x00033BCE File Offset: 0x00031DCE
		public Vector2? CurrentBattleCenterPosition
		{
			get
			{
				return this.currentBattleCenterPosition;
			}
		}

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06001098 RID: 4248 RVA: 0x00033BD6 File Offset: 0x00031DD6
		public Vector2? CurrentBattleDirection
		{
			get
			{
				return this.currentBattleDirection;
			}
		}

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06001099 RID: 4249 RVA: 0x00033BDE File Offset: 0x00031DDE
		public override bool IsRetreating
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x0600109A RID: 4250 RVA: 0x00033BE1 File Offset: 0x00031DE1
		public IReadOnlyList<BaseGameMob> SharedAttackTargets
		{
			get
			{
				return this.sharedAttackTargets;
			}
		}

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x0600109B RID: 4251 RVA: 0x00033BE9 File Offset: 0x00031DE9
		public float MaxGroupCharactersHitPointsSum
		{
			get
			{
				return this.maxGroupCharactersHitPointsSum;
			}
		}

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x0600109C RID: 4252 RVA: 0x00033BF1 File Offset: 0x00031DF1
		public float CurrentGroupCharactersHitPointsSum
		{
			get
			{
				return this.currentGroupCharactersHitPointsSum;
			}
		}

		// Token: 0x140000B8 RID: 184
		// (add) Token: 0x0600109D RID: 4253 RVA: 0x00033BFC File Offset: 0x00031DFC
		// (remove) Token: 0x0600109E RID: 4254 RVA: 0x00033C34 File Offset: 0x00031E34
		public event Action<GameMobGroupController, Vector2?, object> GroupDestinationChanged;

		// Token: 0x140000B9 RID: 185
		// (add) Token: 0x0600109F RID: 4255 RVA: 0x00033C6C File Offset: 0x00031E6C
		// (remove) Token: 0x060010A0 RID: 4256 RVA: 0x00033CA4 File Offset: 0x00031EA4
		public event Action<GameMobGroupController> GroupDestinationReached;

		// Token: 0x140000BA RID: 186
		// (add) Token: 0x060010A1 RID: 4257 RVA: 0x00033CDC File Offset: 0x00031EDC
		// (remove) Token: 0x060010A2 RID: 4258 RVA: 0x00033D14 File Offset: 0x00031F14
		public event Action<GameMobGroupController, bool> BattleStateChanged;

		// Token: 0x140000BB RID: 187
		// (add) Token: 0x060010A3 RID: 4259 RVA: 0x00033D4C File Offset: 0x00031F4C
		// (remove) Token: 0x060010A4 RID: 4260 RVA: 0x00033D84 File Offset: 0x00031F84
		public event Action<GameMobGroupController, float, float> GroupCharactersHitPointsSumChanged;

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x060010A5 RID: 4261 RVA: 0x00033DB9 File Offset: 0x00031FB9
		// (set) Token: 0x060010A6 RID: 4262 RVA: 0x00033DC1 File Offset: 0x00031FC1
		private protected float AverageMobRadius { protected get; private set; }

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x060010A7 RID: 4263 RVA: 0x00033DCA File Offset: 0x00031FCA
		protected float MaxAttackTargetSearchRadius
		{
			get
			{
				return this.maxAttackTargetSearchRadius;
			}
		}

		// Token: 0x060010A8 RID: 4264 RVA: 0x00033DD2 File Offset: 0x00031FD2
		private float GetMobArea(float mobRadius)
		{
			return mobRadius * mobRadius * 4f;
		}

		// Token: 0x060010A9 RID: 4265 RVA: 0x00033DE0 File Offset: 0x00031FE0
		private void CountMobRadius(BaseGameMob mob, bool mobWasAdded)
		{
			float num = (mob.NavMeshAgent != null) ? mob.NavMeshAgent.radius : mob.Radius;
			float num2 = this.GetMobArea(num);
			if (!mobWasAdded)
			{
				num = -num;
				num2 = -num2;
			}
			this.mobRadiusSum += num;
			this.minGroupArea += num2;
			this.AverageMobRadius = ((this.mobRadiusSum > 0f) ? (this.mobRadiusSum / (float)this.mobs.Count) : 0f);
			this.minGroupRadius = ((this.minGroupArea > 0f) ? Mathf.Sqrt(this.minGroupArea / 3.1415927f) : 0f);
		}

		// Token: 0x060010AA RID: 4266 RVA: 0x00033E94 File Offset: 0x00032094
		private void UpdateMobsOnGroupDestinationChange(object destinationSource)
		{
			Vector2? value = this.groupDestination.Value;
			for (int i = 0; i < this.mobs.Count; i++)
			{
				GameMobMotionController gameMobMotionController = this.mobs[i].MotionController as GameMobMotionController;
				if (gameMobMotionController != null)
				{
					gameMobMotionController.OnGroupDestinationChanged(this, value, destinationSource);
				}
			}
		}

		// Token: 0x060010AB RID: 4267 RVA: 0x00033EE8 File Offset: 0x000320E8
		private void UpdateAIRelatedInfo(BaseGameMob currentMob, out IGameMob attackTarget, out bool isAttackDistanceReached)
		{
			GameMobAIController aicontroller = currentMob.AIController;
			attackTarget = null;
			isAttackDistanceReached = false;
			if (aicontroller == null)
			{
				return;
			}
			GameMobAIControllerParams currentParams = aicontroller.CurrentParams;
			float targetSearchRadius = currentParams.TargetSearchRadius;
			attackTarget = aicontroller.CurrentAttackTarget;
			int currentAbilityID = (int)aicontroller.CurrentAbilityID;
			isAttackDistanceReached = aicontroller.IsAttackDistanceReached;
			if (targetSearchRadius > this.maxAttackTargetSearchRadius)
			{
				this.maxAttackTargetSearchRadius = targetSearchRadius;
			}
			if (!this.hasThreateners)
			{
				this.hasThreateners = (currentMob.CurrentThreateners.Count != 0);
			}
			if (!this.isUnderAttack)
			{
				this.isUnderAttack = (currentMob.CurrentAttackers.Count != 0);
			}
			if (!this.hasAttackTargets)
			{
				this.hasAttackTargets = (attackTarget != null);
			}
			if (!this.isAttacking)
			{
				this.isAttacking = aicontroller.IsAttacking;
			}
			if (currentAbilityID != 0)
			{
				this.abilitiesInUse.Add(currentAbilityID);
			}
			if (currentParams.shareAggression && this.sharedAttackTargets.Count < 64)
			{
				BaseGameMob baseGameMob = attackTarget as BaseGameMob;
				if (baseGameMob != null)
				{
					this.sharedAttackTargets.Add(baseGameMob);
					return;
				}
				BaseGameMob firstAttacker = currentMob.GetFirstAttacker();
				if (firstAttacker != null)
				{
					this.sharedAttackTargets.Add(firstAttacker);
				}
			}
		}

		// Token: 0x060010AC RID: 4268 RVA: 0x00033FF8 File Offset: 0x000321F8
		private bool TryGetHitPointsInfo(BaseGameMob currentMob, out float currentHitPoints, out float maxHitPoints)
		{
			currentHitPoints = 0f;
			maxHitPoints = 0f;
			IDamageable hitPointsController = currentMob.HitPointsController;
			if (hitPointsController != null && currentMob.IsCharacterByDefault())
			{
				maxHitPoints = hitPointsController.MaxHitPoints;
				currentHitPoints = hitPointsController.CurrentHitPoints;
				return true;
			}
			return false;
		}

		// Token: 0x060010AD RID: 4269 RVA: 0x00034038 File Offset: 0x00032238
		private void UpdateGroupHitPointsSum(float newSum)
		{
			if (this.currentGroupCharactersHitPointsSum != newSum)
			{
				Action<GameMobGroupController, float, float> groupCharactersHitPointsSumChanged = this.GroupCharactersHitPointsSumChanged;
				if (groupCharactersHitPointsSumChanged != null)
				{
					groupCharactersHitPointsSumChanged(this, this.currentGroupCharactersHitPointsSum, newSum);
				}
				this.currentGroupCharactersHitPointsSum = newSum;
			}
		}

		// Token: 0x060010AE RID: 4270 RVA: 0x00034064 File Offset: 0x00032264
		private void TryRequestCombatSupport()
		{
			if (this.isVisibleGroup && this.inBattle && this.combatSupportCallHPThreshold > 0f && this.groupSpawningChunk != null && this.currentGroupCharactersHitPointsSum / this.maxGroupCharactersHitPointsSum <= this.combatSupportCallHPThreshold)
			{
				IReadOnlyList<GameMobsGroupControllerBase> spawnedMobGroups = this.groupSpawningChunk.SpawnedMobGroups;
				GameMobFactions faction = base.Faction;
				Vector2 position = this.Position;
				for (int i = 0; i < spawnedMobGroups.Count; i++)
				{
					GameMobsGroupControllerBase gameMobsGroupControllerBase = spawnedMobGroups[i];
					if (gameMobsGroupControllerBase.IsVisibleGroup && !gameMobsGroupControllerBase.InBattle && gameMobsGroupControllerBase.Faction == faction && gameMobsGroupControllerBase != this)
					{
						GameMobGroupController gameMobGroupController = gameMobsGroupControllerBase as GameMobGroupController;
						if (gameMobGroupController != null)
						{
							gameMobGroupController.SetDestination(new Vector2?(position), gameMobsGroupControllerBase, false, false);
						}
					}
				}
			}
		}

		// Token: 0x060010AF RID: 4271 RVA: 0x0003412F File Offset: 0x0003232F
		private void ResetGroupInfo()
		{
			this.desiredGroupVelocity = default(Vector2);
			this.totalGroupRect = default(Rect);
			this.destinationMovementRect = default(Rect);
			this.groupSpread = 0f;
			this.destinationReachedMobsCount = 0;
			this.isGroupDestinationReached = false;
		}

		// Token: 0x060010B0 RID: 4272 RVA: 0x00034170 File Offset: 0x00032370
		protected virtual bool SetDestination(Vector2? newDestination, object destinationSource, bool forceUpdate, bool isForcedDestination = false)
		{
			if (this.groupDestination.Update(newDestination, forceUpdate))
			{
				this.hasForcedGroupDestination = (isForcedDestination && newDestination != null);
				this.destinationReachedMobsCount = 0;
				this.groupDestinationDirection = default(Vector2);
				if (newDestination != null)
				{
					this.groupDestinationDirection = newDestination.Value - this.totalGroupRect.center;
					this.groupDestinationDirection.Normalize();
				}
				this.OnGroupDestinationChanged();
				this.UpdateMobsOnGroupDestinationChange(destinationSource);
				Action<GameMobGroupController, Vector2?, object> groupDestinationChanged = this.GroupDestinationChanged;
				if (groupDestinationChanged != null)
				{
					groupDestinationChanged(this, newDestination, destinationSource);
				}
				this.ResetGroupInfo();
				return true;
			}
			return false;
		}

		// Token: 0x060010B1 RID: 4273 RVA: 0x00034214 File Offset: 0x00032414
		public GameMobGroupController(int mobsCapacity, int coupledGroupsCapacity) : base(mobsCapacity, coupledGroupsCapacity)
		{
		}

		// Token: 0x060010B2 RID: 4274 RVA: 0x0003426E File Offset: 0x0003246E
		public GameMobGroupController() : this(64, 4)
		{
		}

		// Token: 0x060010B3 RID: 4275 RVA: 0x00034279 File Offset: 0x00032479
		public void SetForcedGroupDestination(Vector2 newDestination, object destinationSource)
		{
			this.SetDestination(new Vector2?(newDestination), destinationSource, false, true);
		}

		// Token: 0x060010B4 RID: 4276 RVA: 0x0003428C File Offset: 0x0003248C
		public void PlaceAsFormation(Vector2 targetPosition, int centerIndexOffset = 0)
		{
			int count = this.mobs.Count;
			if (count == 0)
			{
				return;
			}
			if (count == 1)
			{
				this.mobs[0].Position = targetPosition;
				return;
			}
			float d = this.minGroupRadius * (this.AverageMobRadius / (1f / Mathf.Sqrt((float)this.mobs.Count) * 0.5f));
			for (int i = 0; i < count; i++)
			{
				Vector2 vector = targetPosition + GameMobGroupController.FormationPlacementOffsets[i + centerIndexOffset] * d;
				NavMeshHit navMeshHit;
				if (NavMesh.Raycast(targetPosition, vector, out navMeshHit, -1))
				{
					vector = navMeshHit.position;
				}
				this.mobs[i].Position = vector;
			}
		}

		// Token: 0x060010B5 RID: 4277 RVA: 0x00034347 File Offset: 0x00032547
		public void PlaceAsFormation()
		{
			this.PlaceAsFormation(this.initialPosition, 0);
		}

		// Token: 0x060010B6 RID: 4278 RVA: 0x00034358 File Offset: 0x00032558
		public bool IsPointInsideSpawningArea(Vector2 point)
		{
			return this.minGroupRadius > 0f && (this.initialPosition - point).SqrMagnitude() < this.minGroupRadius * this.minGroupRadius * 4f;
		}

		// Token: 0x060010B7 RID: 4279 RVA: 0x0003439D File Offset: 0x0003259D
		public bool IsPointInsideSpawningArea(Vector2? point)
		{
			return point != null && this.IsPointInsideSpawningArea(point.Value);
		}

		// Token: 0x060010B8 RID: 4280 RVA: 0x000343B8 File Offset: 0x000325B8
		public Vector2? GetRandomGroupPoint()
		{
			if (this.totalGroupRect.width == 0f && this.totalGroupRect.height == 0f)
			{
				return null;
			}
			Vector2 center = this.totalGroupRect.center;
			center.x += (UnityEngine.Random.value - 0.5f) * this.totalGroupRect.width;
			center.y += (UnityEngine.Random.value - 0.5f) * this.totalGroupRect.height;
			return new Vector2?(center);
		}

		// Token: 0x060010B9 RID: 4281 RVA: 0x00034449 File Offset: 0x00032649
		public bool IsAbilityInUse(int abilityID)
		{
			return this.abilitiesInUse.Contains(abilityID);
		}

		// Token: 0x060010BA RID: 4282 RVA: 0x00034458 File Offset: 0x00032658
		public GameMobGroupController GetSummonableMobsGroup(GameMobFactions mobFactionOverride)
		{
			if (mobFactionOverride == GameMobFactions.None || mobFactionOverride == base.Faction)
			{
				return this;
			}
			GameMobGroup gameMobGroup;
			if (!this.summoningGroups.TryGetValue((int)mobFactionOverride, out gameMobGroup))
			{
				gameMobGroup = new GameObject(string.Format("{0}_{1}_SummonedMobsGroup", base.GroupID, mobFactionOverride)).AddComponent<GameMobGroup>();
				GameMobGroupController groupController = gameMobGroup.GroupController;
				groupController.Faction = mobFactionOverride;
				groupController.Leader = this.Leader;
				if (this.destinationsGenerator != null && this.destinationsGenerator.PassToSumoningGroups)
				{
					this.destinationsGenerator.Clone(groupController);
				}
				base.AddCoupledGroup(groupController);
				this.summoningGroups.Add((int)mobFactionOverride, gameMobGroup);
			}
			return gameMobGroup;
		}

		// Token: 0x060010BB RID: 4283 RVA: 0x00034504 File Offset: 0x00032704
		protected override void OnMobAdded(BaseGameMob mob)
		{
			if (mob.IsCharacterByDefault())
			{
				this.charactersCount++;
			}
			this.CountMobRadius(mob, true);
			if (this.groupDestination != null && this.groupDestination.HasValue)
			{
				GameMobMotionController gameMobMotionController = mob.MotionController as GameMobMotionController;
				if (gameMobMotionController == null)
				{
					return;
				}
				gameMobMotionController.OnGroupDestinationChanged(this, this.groupDestination.Value, this);
			}
		}

		// Token: 0x060010BC RID: 4284 RVA: 0x00034568 File Offset: 0x00032768
		protected override void OnMobRemoved(BaseGameMob mob)
		{
			if (mob.IsCharacterByDefault())
			{
				this.charactersCount--;
			}
			this.CountMobRadius(mob, false);
			if (this.mobs.Count == 0)
			{
				this.UpdateGroupHitPointsSum(0f);
				this.ResetGroupInfo();
				this.maxAttackTargetSearchRadius = 0f;
				this.destinationReachedMobsCount = 0;
			}
		}

		// Token: 0x060010BD RID: 4285 RVA: 0x000345C3 File Offset: 0x000327C3
		protected virtual void OnGroupDestinationChanged()
		{
		}

		// Token: 0x060010BE RID: 4286 RVA: 0x000345C5 File Offset: 0x000327C5
		protected virtual void OnGroupDestinationReached()
		{
		}

		// Token: 0x060010BF RID: 4287 RVA: 0x000345C7 File Offset: 0x000327C7
		protected virtual void OnGroupMobProceeded(BaseGameMob groupMob)
		{
		}

		// Token: 0x060010C0 RID: 4288 RVA: 0x000345C9 File Offset: 0x000327C9
		protected virtual void OnAttackingStateChanged(bool isAttacking)
		{
		}

		// Token: 0x060010C1 RID: 4289 RVA: 0x000345CB File Offset: 0x000327CB
		protected virtual void OnBattleStateChanged(bool inBattle)
		{
		}

		// Token: 0x060010C2 RID: 4290 RVA: 0x000345D0 File Offset: 0x000327D0
		public override void Initialize(int groupID, GameObject groupObject, Vector2 initialGroupPosition)
		{
			base.Initialize(groupID, groupObject, initialGroupPosition);
			this.arrivalRadius = 0.3f;
			this.groupDestination = new GameMobDestinationPoint
			{
				newValueDistanceThreshold = 0.1f
			};
			IGameMobsSpawner groupMobsSpawner = base.GroupMobsSpawner;
			this.groupSpawningChunk = (((groupMobsSpawner != null) ? groupMobsSpawner.SpawningChunk : null) as LocationChunk);
		}

		// Token: 0x060010C3 RID: 4291 RVA: 0x00034624 File Offset: 0x00032824
		public override void OnUpdate()
		{
			this.isVisibleGroup = false;
			this.hasThreateners = false;
			this.hasAttackTargets = false;
			this.isUnderAttack = false;
			this.isAttacking = false;
			this.maxGroupCharactersHitPointsSum = 0f;
			float num = 0f;
			this.maxAttackTargetSearchRadius = 0f;
			this.destinationMovementRect = default(Rect);
			this.desiredGroupVelocity = default(Vector2);
			this.totalGroupRect = default(Rect);
			this.groupSpread = 0f;
			this.sharedAttackTargets.Clear();
			this.abilitiesInUse.Clear();
			Vector2 vector = default(Vector2);
			int num2 = 0;
			int count = this.mobs.Count;
			if (count != 0)
			{
				float deltaTime = Time.deltaTime;
				Vector2? value = this.groupDestination.Value;
				float num3 = this.minGroupArea;
				Vector2 centerRayIntersectionPoint = GameMobGroupController.GetCenterRayIntersectionPoint(this.destinationMovementRect, this.desiredGroupVelocity);
				bool flag = this.groupSpread > 0.9f;
				Vector4 vector2 = GameMobGroupController.GetInitialMinMaxVector();
				Vector4 vector3 = GameMobGroupController.GetInitialMinMaxVector();
				Vector4 currentMinMax = GameMobGroupController.GetInitialMinMaxVector();
				Vector2 a = default(Vector2);
				float num4 = 0f;
				bool flag2 = this.destinationsGenerator != null;
				bool flag3 = flag2 && (count == 1 || this.destinationsGenerator.GenerateIndividualMobDestinations);
				int num5 = 0;
				for (int i = 0; i < count; i++)
				{
					BaseGameMob baseGameMob = this.mobs[i];
					if (!baseGameMob.IsNull())
					{
						Vector2 hitColliderCenter = baseGameMob.HitColliderCenter;
						Vector2 b = baseGameMob.transform.position;
						float radius = baseGameMob.Radius;
						Vector4 newMinMax = new Vector4
						{
							x = hitColliderCenter.x - radius,
							y = hitColliderCenter.y - radius,
							z = hitColliderCenter.x + radius,
							w = hitColliderCenter.y + radius
						};
						GameMobMotionController gameMobMotionController = baseGameMob.MotionController as GameMobMotionController;
						if (gameMobMotionController != null)
						{
							bool flag4 = value != null && gameMobMotionController.HasGroupDestination;
							Vector2 vector4 = baseGameMob.CurrentVelocity * deltaTime;
							newMinMax.x += vector4.x;
							newMinMax.y += vector4.y;
							newMinMax.z += vector4.x;
							newMinMax.w += vector4.y;
							if (flag2)
							{
								float actualMobSpeed = gameMobMotionController.GetActualMobSpeed();
								Vector2 velocity;
								if (this.destinationsGenerator.TryGetAdditionalVelocity(hitColliderCenter, actualMobSpeed, out velocity))
								{
									gameMobMotionController.Move(velocity);
								}
							}
							Vector2 value2;
							if (flag4 && !gameMobMotionController.IsGroupDestinationReached)
							{
								float num6 = (value.Value - b).SqrMagnitude();
								a += gameMobMotionController.DesiredVelocity;
								vector3 = GameMobGroupController.GetMinMaxVector(vector3, newMinMax);
								num5++;
								float num7 = (centerRayIntersectionPoint - b).SqrMagnitude() / (this.minGroupRadius * this.minGroupRadius * 4f);
								num4 += num7;
								if (flag)
								{
									if (num7 > 1f && this.maxCrowdStabilizationSpeedup > 0f)
									{
										float t = Mathf.InverseLerp(1f, 2f, num7);
										gameMobMotionController.ModifyNavMeshAgentVelocity(Mathf.Lerp(0f, this.maxCrowdStabilizationSpeedup, t));
									}
									else if (this.maxCrowdStabilizationSlowdown > 0f)
									{
										float t2 = Mathf.InverseLerp(0f, 1f, num7);
										gameMobMotionController.ModifyNavMeshAgentVelocity(-Mathf.Lerp(this.maxCrowdStabilizationSlowdown, 0f, t2));
									}
								}
								float num8 = this.arrivalRadius + radius + 0.3f;
								if (num6 < num8 * num8 || gameMobMotionController.IsGroupDestinationReachedByNeighbour())
								{
									currentMinMax = GameMobGroupController.GetMinMaxVector(currentMinMax, newMinMax);
									this.destinationReachedMobsCount++;
									gameMobMotionController.OnGroupDestinationReached(this.groupDestination.GetValue());
								}
							}
							else if (flag3 && this.destinationsGenerator.TryGetIndividualDestination(gameMobMotionController, out value2))
							{
								gameMobMotionController.IndividualDestination = new Vector2?(value2);
							}
						}
						IGameMob gameMob;
						bool flag5;
						this.UpdateAIRelatedInfo(baseGameMob, out gameMob, out flag5);
						if (flag5 && gameMob != null)
						{
							num2++;
							vector += gameMob.Position;
						}
						if (!this.isVisibleGroup && baseGameMob.IsRendererVisible)
						{
							this.isVisibleGroup = true;
						}
						float num9;
						float num10;
						if (this.TryGetHitPointsInfo(baseGameMob, out num9, out num10))
						{
							num += num9;
							this.maxGroupCharactersHitPointsSum += num10;
						}
						vector2 = GameMobGroupController.GetMinMaxVector(vector2, newMinMax);
						this.OnGroupMobProceeded(baseGameMob);
					}
				}
				if (num5 != 0)
				{
					a /= (float)num5;
					this.destinationMovementRect = GameMobGroupController.CreateRect(vector3, 0f);
				}
				this.desiredGroupVelocity = a;
				this.totalGroupRect = GameMobGroupController.CreateRect(vector2, 0f);
				this.groupSpread = num4;
				if (!this.isGroupDestinationReached && this.destinationReachedMobsCount >= this.mobs.Count)
				{
					this.OnGroupDestinationReached();
					this.isGroupDestinationReached = true;
					Action<GameMobGroupController> groupDestinationReached = this.GroupDestinationReached;
					if (groupDestinationReached != null)
					{
						groupDestinationReached(this);
					}
				}
				Vector2 value3;
				bool flag6;
				if (!flag3 && flag2 && (this.isGroupDestinationReached || !this.groupDestination.HasValue) && this.destinationsGenerator.TryGetNewDestination(out value3, out flag6) && this.SetDestination(new Vector2?(value3), this.destinationsGenerator, false, false))
				{
					this.hasForcedGroupDestination = flag6;
				}
			}
			this.UpdateGroupHitPointsSum(num);
			if (num2 != 0)
			{
				vector /= (float)num2;
				Vector2 value4 = vector - this.totalGroupRect.center;
				value4.Normalize();
				this.currentBattleCenterPosition = new Vector2?(vector);
				this.currentBattleDirection = new Vector2?(value4);
			}
			else
			{
				this.currentBattleCenterPosition = null;
				this.currentBattleDirection = null;
			}
			if (this.isAttacking != this.lastIsAttackingState)
			{
				this.OnAttackingStateChanged(this.isAttacking);
				this.lastIsAttackingState = this.isAttacking;
			}
			if ((this.inBattle = (this.hasThreateners || this.hasAttackTargets || this.isAttacking || this.isUnderAttack)) != this.lastInBattleState)
			{
				this.OnBattleStateChanged(this.inBattle);
				Action<GameMobGroupController, bool> battleStateChanged = this.BattleStateChanged;
				if (battleStateChanged != null)
				{
					battleStateChanged(this, this.inBattle);
				}
				this.lastInBattleState = this.inBattle;
			}
			this.TryRequestCombatSupport();
		}

		// Token: 0x0400095B RID: 2395
		private const float CircleToSquareAreaCoeff = 1.2732395f;

		// Token: 0x0400095C RID: 2396
		private const float DefaultArrivalRadius = 0.3f;

		// Token: 0x0400095D RID: 2397
		private const float NewDestinationDistanceThreshold = 0.1f;

		// Token: 0x0400095E RID: 2398
		private const float MaxGroupSpread = 0.9f;

		// Token: 0x0400095F RID: 2399
		private const int MaxSharedAttackTargetsCount = 64;

		// Token: 0x04000960 RID: 2400
		private static readonly Vector2[] FormationPlacementOffsets = new Vector2[500];

		// Token: 0x04000966 RID: 2406
		public float maxCrowdStabilizationSpeedup = 0.3f;

		// Token: 0x04000967 RID: 2407
		public float maxCrowdStabilizationSlowdown = 0.2f;

		// Token: 0x04000968 RID: 2408
		private float combatSupportCallHPThreshold = 0.7f;

		// Token: 0x04000969 RID: 2409
		protected GameMobDestinationPoint groupDestination;

		// Token: 0x0400096A RID: 2410
		protected IGroupDestinationsGenerator destinationsGenerator;

		// Token: 0x0400096B RID: 2411
		private readonly HashSet<int> abilitiesInUse = new HashSet<int>();

		// Token: 0x0400096C RID: 2412
		private readonly Dictionary<int, GameMobGroup> summoningGroups = new Dictionary<int, GameMobGroup>(4);

		// Token: 0x0400096D RID: 2413
		private readonly List<BaseGameMob> sharedAttackTargets = new List<BaseGameMob>(64);

		// Token: 0x0400096E RID: 2414
		private LocationChunk groupSpawningChunk;

		// Token: 0x0400096F RID: 2415
		private IGameMob leader;

		// Token: 0x04000970 RID: 2416
		private int charactersCount;

		// Token: 0x04000971 RID: 2417
		private float arrivalRadius;

		// Token: 0x04000972 RID: 2418
		private float mobRadiusSum;

		// Token: 0x04000973 RID: 2419
		private float minGroupRadius;

		// Token: 0x04000974 RID: 2420
		private float minGroupArea;

		// Token: 0x04000975 RID: 2421
		private Rect totalGroupRect;

		// Token: 0x04000976 RID: 2422
		private Rect destinationMovementRect;

		// Token: 0x04000977 RID: 2423
		private float groupSpread;

		// Token: 0x04000978 RID: 2424
		private Vector2 desiredGroupVelocity;

		// Token: 0x04000979 RID: 2425
		private Vector2 groupDestinationDirection;

		// Token: 0x0400097A RID: 2426
		private int destinationReachedMobsCount;

		// Token: 0x0400097B RID: 2427
		[NonSerialized]
		private bool hasForcedGroupDestination;

		// Token: 0x0400097C RID: 2428
		private bool isGroupDestinationReached;

		// Token: 0x0400097D RID: 2429
		private bool isVisibleGroup;

		// Token: 0x0400097E RID: 2430
		private float maxAttackTargetSearchRadius;

		// Token: 0x0400097F RID: 2431
		private bool hasThreateners;

		// Token: 0x04000980 RID: 2432
		private bool hasAttackTargets;

		// Token: 0x04000981 RID: 2433
		private bool isAttacking;

		// Token: 0x04000982 RID: 2434
		private bool lastIsAttackingState;

		// Token: 0x04000983 RID: 2435
		private bool isUnderAttack;

		// Token: 0x04000984 RID: 2436
		private bool inBattle;

		// Token: 0x04000985 RID: 2437
		private Vector2? currentBattleCenterPosition;

		// Token: 0x04000986 RID: 2438
		private Vector2? currentBattleDirection;

		// Token: 0x04000987 RID: 2439
		private bool lastInBattleState;

		// Token: 0x04000988 RID: 2440
		private float maxGroupCharactersHitPointsSum;

		// Token: 0x04000989 RID: 2441
		private float currentGroupCharactersHitPointsSum;
	}
}
