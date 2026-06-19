using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 晶能变体配置数据（单条记录）
/// 描述一个变体的所有数值参数和特殊效果类型
/// </summary>
[System.Serializable]
public class CrystalAspectData
{
    [Header("标识")]
    public CrystalAspectType aspectType;
    public WeaponType compatibleWeapon;

    [Header("显示信息")]
    public string aspectName = "";
    [TextArea(2, 4)] public string description = "";

    [Header("数值倍率")]
    [Range(0.3f, 3f)] public float damageMultiplier = 1f;
    [Range(0.3f, 3f)] public float rangeMultiplier = 1f;
    [Range(0.3f, 3f)] public float intervalMultiplier = 1f;

    [Header("特殊效果")]
    public CrystalAspectEffectType effectType = CrystalAspectEffectType.None;

    /// <summary>该变体嵌入晶能核心时的默认变体标记</summary>
    public bool isDefaultForWeapon = false;
}

/// <summary>
/// 晶能变体配置集（ScriptableObject）
/// 统一管理全部16种变体的参数，替代硬编码的switch-case
/// 在 Unity 编辑器中通过 CreateAssetMenu 创建 .asset 文件
/// </summary>
[CreateAssetMenu(fileName = "CrystalAspectConfigs", menuName = "IIP/Crystal Aspect Configs")]
public class CrystalAspectConfig : ScriptableObject
{
    [Header("全部变体配置列表")]
    [Tooltip("包含所有16种晶能变体的参数配置，每种武器4个变体")]
    public List<CrystalAspectData> aspects = new List<CrystalAspectData>();

    /// <summary>根据变体类型获取配置数据</summary>
    public CrystalAspectData GetAspectData(CrystalAspectType type)
    {
        return aspects.Find(a => a.aspectType == type);
    }

    /// <summary>获取指定武器的默认变体</summary>
    public CrystalAspectType GetDefaultAspectForWeapon(WeaponType weaponType)
    {
        var found = aspects.Find(a => a.compatibleWeapon == weaponType && a.isDefaultForWeapon);
        return found != null ? found.aspectType : CrystalAspectType.StandardSword;
    }

    /// <summary>获取指定武器的所有可用变体</summary>
    public List<CrystalAspectData> GetAspectsForWeapon(WeaponType weaponType)
    {
        return aspects.FindAll(a => a.compatibleWeapon == weaponType);
    }

    /// <summary>验证变体是否与武器兼容</summary>
    public bool IsAspectCompatible(CrystalAspectType aspectType, WeaponType weaponType)
    {
        var data = GetAspectData(aspectType);
        return data != null && data.compatibleWeapon == weaponType;
    }
}
