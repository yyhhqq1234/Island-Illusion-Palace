using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 敌人行为组件 - 管理行为策略的注册和执行
/// </summary>
public class EnemyBehaviorComponent : MonoBehaviour
{
    private Dictionary<EnemyAI.BehaviorMode, IEnemyBehavior> behaviors;
    private EnemyBehaviorContext context;

    public void Initialize(EnemyBehaviorContext ctx)
    {
        context = ctx;
        behaviors = new Dictionary<EnemyAI.BehaviorMode, IEnemyBehavior>();

        // 注册所有行为策略
        RegisterBehavior(new PursuitBehavior());
        RegisterBehavior(new RangedBehavior());
        RegisterBehavior(new BerserkBehavior());
        RegisterBehavior(new AmbushBehavior());
        RegisterBehavior(new SummonerBehavior());
        RegisterBehavior(new TacticalBehavior());
    }

    private void RegisterBehavior(IEnemyBehavior behavior)
    {
        behaviors[behavior.GetBehaviorMode()] = behavior;
    }

    /// <summary>
    /// 执行指定模式的行为
    /// </summary>
    public void ExecuteBehavior(EnemyAI.BehaviorMode mode, float distanceToTargetSqr, Transform target)
    {
        if (behaviors.TryGetValue(mode, out IEnemyBehavior behavior))
        {
            behavior.ExecuteBehavior(distanceToTargetSqr, target, context);
        }
        else
        {
            // 默认使用追击行为
            behaviors[EnemyAI.BehaviorMode.Pursuit].ExecuteBehavior(distanceToTargetSqr, target, context);
        }
    }
}