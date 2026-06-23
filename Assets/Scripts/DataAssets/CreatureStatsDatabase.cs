/// <summary>
/// 召唤物基础属性静态数据库 — 单一数据源，消除 GetBaseHealthForType() 两处重复。
/// 对应 I_IP召唤物清单.md 定义。
/// </summary>
public static class CreatureStatsDatabase
{
    /// <summary>获取召唤物基础生命值</summary>
    public static float GetBaseHealthForType(CreatureType type)
    {
        return type switch
        {
            CreatureType.CorruptedVillager    => 60f,
            CreatureType.CrystalLizard        => 45f,
            CreatureType.SwampStalker         => 80f,
            CreatureType.IceWolf              => 55f,
            CreatureType.MechanicalDebris     => 70f,
            CreatureType.SkeletonWarrior      => 40f,
            CreatureType.Wraith               => 45f,
            CreatureType.Gargoyle             => 110f,
            CreatureType.SoulEater            => 180f,
            CreatureType.LavaElemental        => 240f,
            CreatureType.MechanicalConstruct  => 300f,
            CreatureType.TimeGuardian         => 350f,
            CreatureType.MemoryGuardian       => 400f,
            CreatureType.CorruptionGuardian   => 600f,
            CreatureType.ScarletSoulShana     => 1000f,
            _ => 50f
        };
    }
}
