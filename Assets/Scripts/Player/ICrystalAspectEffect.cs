using UnityEngine;

/// <summary>
/// 晶能变体特殊效果类型
/// 用于标识每种变体所携带的特殊攻击效果
/// </summary>
public enum CrystalAspectEffectType
{
    None,       // 无特殊效果（标准攻击）
    Scatter,    // 散射：发射多枚散射弹
    Pull,       // 牵引：命中敌人向玩家方向牵引
    Resonance,  // 共鸣：召唤物数量加成伤害
    Reaper      // 收割：低血量敌人伤害加成
}

/// <summary>
/// 晶能变体特殊效果策略接口
/// 所有变体特殊效果的统一抽象，支持策略模式扩展
/// </summary>
public interface ICrystalAspectEffect
{
    /// <summary>判断当前变体类型是否应触发此效果</summary>
    bool CanExecute(CrystalAspectType aspectType);

    /// <summary>执行特殊效果，返回是否完全接管了攻击流程</summary>
    /// <param name="weaponSystem">武器系统引用</param>
    /// <param name="damage">基础伤害值</param>
    /// <returns>true=效果已完全处理攻击（调用方无需继续）；false=效果仅作为修饰器（调用方需继续标准攻击）</returns>
    bool Execute(WeaponSystem weaponSystem, float damage);
}
