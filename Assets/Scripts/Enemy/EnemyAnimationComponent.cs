using UnityEngine;
using System;

/// <summary>
/// 敌人动画控制组件 - 负责动画参数设置和朝向管理
/// </summary>
public class EnemyAnimationComponent : MonoBehaviour
{
    private const float MIN_VELOCITY_THRESHOLD = 0.1f;

    [Header("调试设置")]
    public bool debugAnimationFacing = false;
    public bool showScaleDebug = false;

    public float ScaleMultiplier { get; set; } = 2.5f;

    // 内部引用
    private Animator anim;
    private Rigidbody2D rb;

    // 动画状态
    public bool IsMoving { get; private set; }

    public void Initialize(Animator animator, Rigidbody2D rigidbody, float scaleMultiplier)
    {
        anim = animator;
        rb = rigidbody;
        ScaleMultiplier = scaleMultiplier;
        transform.localScale = Vector3.one * ScaleMultiplier;
    }

    /// <summary>
    /// 根据移动方向和状态更新动画参数
    /// </summary>
    public void UpdateAnimation(Vector2 velocity, bool isChasing, bool isPatrolling, bool isFleeing, Vector2 fleeDirection, Transform currentTarget)
    {
        if (anim == null) return;

        bool currentlyMoving = velocity.sqrMagnitude > MIN_VELOCITY_THRESHOLD;

        if (currentlyMoving != IsMoving)
        {
            IsMoving = currentlyMoving;
            SetAnimatorBoolSafe("IsMoving", IsMoving);
        }

        if (isFleeing)
        {
            // 逃跑时使用逃跑方向
            SetDirectionalAnimation(fleeDirection);
        }
        else if (currentlyMoving)
        {
            // 移动时使用移动方向
            SetDirectionalAnimation(velocity.normalized);
        }
        else if (isChasing && currentTarget != null)
        {
            // 追击时看向目标
            Vector2 directionToTarget = ((Vector2)(currentTarget.position - transform.position)).normalized;
            SetDirectionalAnimation(directionToTarget);
        }
        else if (isPatrolling)
        {
            SetAnimatorBoolSafe("IsMoving", currentlyMoving);
            if (currentlyMoving)
            {
                SetDirectionalAnimation(velocity);
            }
            else
            {
                SetAllDirectionalBools(false);
            }
        }
        else
        {
            SetAnimatorBoolSafe("IsMoving", false);
            SetAllDirectionalBools(false);
        }
    }

    /// <summary>
    /// 更新朝向动画（简化版，用于非移动状态）
    /// </summary>
    public void UpdateFacingDirection(Vector2 direction)
    {
        if (anim == null) return;
        SetAllDirectionalBools(false);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            SetAnimatorBoolSafe(direction.x > 0 ? "MoveRight" : "MoveLeft", true);
        }
        else
        {
            SetAnimatorBoolSafe(direction.y > 0 ? "MoveUp" : "MoveDown", true);
        }
    }

    /// <summary>
    /// 面朝目标方向
    /// </summary>
    public void FaceTarget(Transform target)
    {
        if (target == null) return;
        Vector2 direction = ((Vector2)(target.position - transform.position)).normalized;
        UpdateFacingDirection(direction);

        // 更新朝向缩放
        if (target.position.x > transform.position.x)
            transform.localScale = new Vector3(ScaleMultiplier, ScaleMultiplier, ScaleMultiplier);
        else
            transform.localScale = new Vector3(-ScaleMultiplier, ScaleMultiplier, ScaleMultiplier);
    }

    /// <summary>
    /// 触发攻击动画
    /// </summary>
    public void TriggerAttack()
    {
        SetAnimatorTriggerSafe("Attack");
    }

    /// <summary>
    /// 触发死亡动画
    /// </summary>
    public void TriggerDeath()
    {
        SetAnimatorBoolSafe("IsMoving", false);
        SetAllDirectionalBools(false);
        SetAnimatorTriggerSafe("Die");
    }

    /// <summary>
    /// 触发狂暴动画
    /// </summary>
    public void TriggerBerserk()
    {
        SetAnimatorTriggerSafe("Berserk");
    }

    /// <summary>
    /// 设置缩放大小
    /// </summary>
    public void SetScale(float newScale)
    {
        ScaleMultiplier = newScale;
        transform.localScale = Vector3.one * ScaleMultiplier;
    }

    private void SetDirectionalAnimation(Vector2 direction)
    {
        SetAllDirectionalBools(false);

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            SetAnimatorBoolSafe(direction.x > 0 ? "MoveRight" : "MoveLeft", true);
        }
        else
        {
            SetAnimatorBoolSafe(direction.y > 0 ? "MoveUp" : "MoveDown", true);
        }
    }

    private void SetAllDirectionalBools(bool value)
    {
        SetAnimatorBoolSafe("MoveUp", value);
        SetAnimatorBoolSafe("MoveDown", value);
        SetAnimatorBoolSafe("MoveLeft", value);
        SetAnimatorBoolSafe("MoveRight", value);
    }

    private void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (anim == null) return;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Bool)
            {
                anim.SetBool(paramName, value);
                return;
            }
        }
    }

    private void SetAnimatorTriggerSafe(string paramName)
    {
        if (anim == null) return;
        foreach (AnimatorControllerParameter param in anim.parameters)
        {
            if (param.name == paramName && param.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(paramName);
                return;
            }
        }
    }
}