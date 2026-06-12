using UnityEngine;

/// <summary>
/// 敌人行为策略接口
/// </summary>
public interface IEnemyBehavior
{
    EnemyAI.BehaviorMode GetBehaviorMode();
    void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext context);
}

/// <summary>
/// 行为执行上下文 - 提供行为所需的所有组件引用
/// </summary>
public class EnemyBehaviorContext
{
    public EnemyMovementComponent Movement;
    public EnemyCombatComponent Combat;
    public EnemyAnimationComponent Animation;
    public EnemyTargetingComponent Targeting;
    public EnemyStateComponent State;
    public Transform Transform;

    public void SetMovementDirection(Vector2 direction)
    {
        Movement?.SetMovementDirection(direction);
    }

    public void SetMovementVelocity(Vector2 velocity)
    {
        Movement?.SetMovementVelocity(velocity);
    }

    public void StopMovement()
    {
        Movement?.StopMovement();
    }

    public float GetCurrentMoveSpeed()
    {
        return Movement != null ? Movement.GetCurrentMoveSpeed() : 3f;
    }

    public bool IsTargetInAttackRange(float distanceSqr)
    {
        return Combat != null && Combat.IsTargetInRange(distanceSqr);
    }

    public void AttackTarget(Transform target)
    {
        Combat?.PerformAttack(target);
    }

    public void FaceTarget(Transform target)
    {
        Animation?.FaceTarget(target);
    }

    public Vector2 ApplySafeZoneRestriction(Vector2 direction)
    {
        return Movement != null ? Movement.ApplySafeZoneRestriction(direction) : direction;
    }

    public Transform CurrentTarget => Targeting?.CurrentTarget;
}

/// <summary>
/// 追击行为 - 靠近目标并在攻击范围内时攻击
/// </summary>
public class PursuitBehavior : IEnemyBehavior
{
    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Pursuit;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        float attackRangeSqr = ctx.Combat.GetAttackRangeSqr();

        if (distanceToTargetSqr > attackRangeSqr)
        {
            Vector2 direction = ((Vector2)(target.position - ctx.Transform.position)).normalized;
            direction = ctx.ApplySafeZoneRestriction(direction);
            ctx.SetMovementDirection(direction);
        }
        else
        {
            ctx.StopMovement();
            ctx.AttackTarget(target);
        }
    }
}

/// <summary>
/// 远程行为 - 保持距离，太近时后退，在最佳距离攻击
/// </summary>
public class RangedBehavior : IEnemyBehavior
{
    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Ranged;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        float attackRange = ctx.Combat.attackRange;
        float preferredRangeSqr = (attackRange * 1.5f) * (attackRange * 1.5f);
        float tooCloseRangeSqr = (attackRange * 0.8f) * (attackRange * 0.8f);

        if (distanceToTargetSqr > preferredRangeSqr)
        {
            Vector2 direction = ((Vector2)(target.position - ctx.Transform.position)).normalized;
            direction = ctx.ApplySafeZoneRestriction(direction);
            ctx.SetMovementVelocity(direction * ctx.GetCurrentMoveSpeed() * 0.7f);
        }
        else if (distanceToTargetSqr < tooCloseRangeSqr)
        {
            Vector2 direction = ((Vector2)(ctx.Transform.position - target.position)).normalized;
            direction = ctx.ApplySafeZoneRestriction(direction);
            ctx.SetMovementVelocity(direction * ctx.GetCurrentMoveSpeed() * 0.5f);
        }
        else
        {
            ctx.AttackTarget(target);
            ctx.FaceTarget(target);
        }
    }
}

/// <summary>
/// 狂暴行为 - 追击并攻击
/// </summary>
public class BerserkBehavior : IEnemyBehavior
{
    private PursuitBehavior pursuit = new PursuitBehavior();

    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Berserk;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        pursuit.ExecuteBehavior(distanceToTargetSqr, target, ctx);
    }
}

/// <summary>
/// 伏击行为 - 追击并攻击
/// </summary>
public class AmbushBehavior : IEnemyBehavior
{
    private PursuitBehavior pursuit = new PursuitBehavior();

    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Ambush;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        pursuit.ExecuteBehavior(distanceToTargetSqr, target, ctx);
    }
}

/// <summary>
/// 召唤者行为 - 远程攻击
/// </summary>
public class SummonerBehavior : IEnemyBehavior
{
    private RangedBehavior ranged = new RangedBehavior();

    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Summoner;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        ranged.ExecuteBehavior(distanceToTargetSqr, target, ctx);
    }
}

/// <summary>
/// 战术行为 - 追击并攻击
/// </summary>
public class TacticalBehavior : IEnemyBehavior
{
    private PursuitBehavior pursuit = new PursuitBehavior();

    public EnemyAI.BehaviorMode GetBehaviorMode() => EnemyAI.BehaviorMode.Tactical;

    public void ExecuteBehavior(float distanceToTargetSqr, Transform target, EnemyBehaviorContext ctx)
    {
        pursuit.ExecuteBehavior(distanceToTargetSqr, target, ctx);
    }
}