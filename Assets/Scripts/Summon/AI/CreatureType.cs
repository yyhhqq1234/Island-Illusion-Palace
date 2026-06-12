/// <summary>
/// 召唤物类型枚举 — 与 EnemyAI.EnemyType 值一一对应但独立定义
/// 解耦召唤系统对 EnemyAI 的依赖
/// </summary>
public enum CreatureType
{
    CorruptedVillager,
    CrystalLizard,
    SwampStalker,
    IceWolf,
    MechanicalDebris,
    SkeletonWarrior,
    Wraith,
    Gargoyle,
    SoulEater,
    LavaElemental,
    MechanicalConstruct,
    TimeGuardian,
    MemoryGuardian,
    CorruptionGuardian,
    ScarletSoulShana
}

/// <summary>
/// CreatureType 的静态转换工具方法
/// </summary>
public static class CreatureTypeConverter
{
    /// <summary>
/// 从 EnemyAI.EnemyType 转换为 CreatureType
/// </summary>
public static CreatureType FromEnemyType(EnemyAI.EnemyType enemyType)
{
    return (CreatureType)(int)enemyType;
}

/// <summary>
/// 从 CreatureType 转换为 EnemyAI.EnemyType
/// </summary>
public static EnemyAI.EnemyType ToEnemyType(CreatureType creatureType)
{
    return (EnemyAI.EnemyType)(int)creatureType;
}
}