using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 武器数据资产生成器。
/// 菜单 "IIP/生成武器/15把武器Data资产" 在 Assets/DataAssets/Weapons/ 下生成 15 个 Weapon_<英文名>.asset。
/// 幂等：已存在且非空 asset 跳过，避免覆盖美术已拖入的 Sprite。
/// 仅 Editor 使用，运行时不依赖 AssetDatabase（运行时查找应通过 Inspector 拖引用或掉落表 DropEntry.weaponData 引用）。
/// </summary>
public static class WeaponDataGenerator
{
    private const string OutputDir = "Assets/DataAssets/Weapons";

    /// <summary>15把武器数据表（中文名|英文名|weaponType|elementType|rarity|damageMin|damageMax|interval|range|specialEffect|obtainMethod）</summary>
    private static readonly WeaponSpec[] Specs = new WeaponSpec[]
    {
        new WeaponSpec("破晓之刃", "Dawnblade", WeaponType.Sword, ElementType.None, WeaponData.Rarity.Common, 12, 15, 0.45f, 2f, "无", "初始武器"),
        new WeaponSpec("结晶法杖", "CrystalStaff", WeaponType.Staff, ElementType.Frost, WeaponData.Rarity.Common, 10, 14, 0.75f, 2f, "攻击有20%概率造成持续2秒的30%减速", "冰原宝箱"),
        new WeaponSpec("沼泽镰刀", "SwampScythe", WeaponType.Scythe, ElementType.None, WeaponData.Rarity.Uncommon, 18, 22, 0.85f, 2f, "对腐化类敌人伤害+20%", "沼泽潜伏者掉落"),
        new WeaponSpec("晶能短刃", "CrystalShard", WeaponType.CrystalArm, ElementType.Lightning, WeaponData.Rarity.Common, 8, 12, 0.38f, 2f, "攻击时有10%概率触发微弱电弧（额外20%雷伤）", "废墟都市宝箱"),
        new WeaponSpec("灵魂引导者", "SoulGuide", WeaponType.Staff, ElementType.Soul, WeaponData.Rarity.Uncommon, 14, 18, 0.75f, 2f, "灵魂微尘掉落率+15%", "怨魂掉落"),
        new WeaponSpec("熔火重剑", "MoltenGreatsword", WeaponType.Sword, ElementType.Fire, WeaponData.Rarity.Rare, 22, 28, 0.45f, 2f, "攻击附带持续灼烧效果（每秒8%伤害，持续3秒）", "熔岩元素掉落"),
        new WeaponSpec("冰霜法杖", "FrostStaff", WeaponType.Staff, ElementType.Frost, WeaponData.Rarity.Rare, 18, 24, 0.75f, 2f, "冰霜技能冷却时间减少15%", "冰原狼掉落（精英概率）"),
        new WeaponSpec("雷霆镰刃", "ThunderScythe", WeaponType.Scythe, ElementType.Lightning, WeaponData.Rarity.Rare, 26, 32, 0.85f, 2f, "重击有25%概率麻痹敌人1.5秒", "时空守护者掉落"),
        new WeaponSpec("圣光裁决", "HolyJudgment", WeaponType.CrystalArm, ElementType.Holy, WeaponData.Rarity.Rare, 20, 26, 0.38f, 2f, "对灵魂残留类敌人伤害+50%", "古代神殿祭坛供奉"),
        new WeaponSpec("腐化吞噬者", "CorruptionDevourer", WeaponType.Scythe, ElementType.Soul, WeaponData.Rarity.Uncommon, 24, 30, 0.85f, 2f, "击杀敌人恢复3%最大生命值", "腐化守护者掉落"),
        new WeaponSpec("时空裂隙匕首", "TemporalRiftDagger", WeaponType.CrystalArm, ElementType.Water, WeaponData.Rarity.Rare, 16, 20, 0.38f, 2f, "攻击有15%概率制造小型时空裂隙（困住敌人1秒+持续20%减速3秒）", "时空裂隙区域宝箱"),
        new WeaponSpec("绯红记忆之刃", "CrimsonMemoryBlade", WeaponType.Sword, ElementType.Fire, WeaponData.Rarity.Rare, 24, 30, 0.45f, 2f, "携带莎娜记忆碎片时，每片+5%伤害（最多+35%）；满7片时攻击附带灵魂伤害", "记忆碎片区域宝箱"),
        new WeaponSpec("岩地重锤", "StoneHammer", WeaponType.CrystalArm, ElementType.None, WeaponData.Rarity.Uncommon, 17, 20, 0.38f, 2f, "破甲效果（无视敌人15%防御力）", "石像鬼掉落"),
        new WeaponSpec("灵魂共鸣法杖", "SoulResonanceStaff", WeaponType.Staff, ElementType.Soul, WeaponData.Rarity.Legendary, 22, 28, 0.75f, 2f, "召唤物全属性+20%；灵魂负担增长减缓10%", "灵魂吞噬者掉落"),
        new WeaponSpec("绯红契约", "CrimsonPact", WeaponType.Scythe, ElementType.Holy, WeaponData.Rarity.Legendary, 28, 36, 0.85f, 2f, "召唤物在场时玩家伤害+20%；召唤物死亡时爆炸造成200%范围灵魂伤害（半径3m）", "收集3个莎娜灵魂核心碎片后自动解锁"),
    };

    [MenuItem("IIP/生成武器/15把武器Data资产")]
    public static void GenerateAll()
    {
        if (!AssetDatabase.IsValidFolder(OutputDir))
        {
            string parent = "Assets";
            string folder = "DataAssets";
            if (!AssetDatabase.IsValidFolder("Assets/DataAssets"))
                AssetDatabase.CreateFolder(parent, folder);
            AssetDatabase.CreateFolder("Assets/DataAssets", "Weapons");
        }

        int created = 0, skipped = 0;
        foreach (var spec in Specs)
        {
            string assetPath = $"{OutputDir}/Weapon_{spec.englishName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (existing != null && !IsWeaponDataEmpty(existing))
            {
                skipped++;
                continue;
            }

            WeaponData data = existing != null ? existing : ScriptableObject.CreateInstance<WeaponData>();
            ApplySpec(data, spec);
            if (existing == null)
                AssetDatabase.CreateAsset(data, assetPath);
            else
                EditorUtility.SetDirty(data);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[WeaponDataGenerator] 生成完成：新建/覆盖 {created} 个，跳过已存在 {skipped} 个。输出目录：{OutputDir}");
    }

    /// <summary>判断 WeaponData 是否为空（仅基础字段判定，不判断 sprite/icon，避免误判已有美术引用）</summary>
    private static bool IsWeaponDataEmpty(WeaponData data)
    {
        return string.IsNullOrEmpty(data.weaponName);
    }

    private static void ApplySpec(WeaponData data, WeaponSpec spec)
    {
        data.weaponName = spec.chineseName;
        data.weaponType = spec.weaponType;
        data.elementType = spec.elementType;
        data.rarity = spec.rarity;
        data.baseDamageMin = spec.damageMin;
        data.baseDamageMax = spec.damageMax;
        data.attackInterval = spec.interval;
        data.attackRange = spec.range;
        data.specialEffectDesc = spec.specialEffect;
        data.obtainMethod = spec.obtainMethod;
    }

    /// <summary>
    /// 按 WeaponData.weaponName（中文名）查找资产。仅 Editor 使用。
    /// 运行时请通过 Inspector 拖引用或 DropEntry.weaponData 引用，勿调用此方法。
    /// </summary>
    public static WeaponData GetWeaponDataByChineseName(string chineseName)
    {
        if (string.IsNullOrEmpty(chineseName)) return null;
        string[] guids = AssetDatabase.FindAssets($"t:{nameof(WeaponData)}", new[] { OutputDir });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var data = AssetDatabase.LoadAssetAtPath<WeaponData>(path);
            if (data != null && data.weaponName == chineseName)
                return data;
        }
        return null;
    }

    [Serializable]
    private class WeaponSpec
    {
        public string chineseName;
        public string englishName;
        public WeaponType weaponType;
        public ElementType elementType;
        public WeaponData.Rarity rarity;
        public float damageMin;
        public float damageMax;
        public float interval;
        public float range;
        public string specialEffect;
        public string obtainMethod;

        public WeaponSpec(string cn, string en, WeaponType wt, ElementType et, WeaponData.Rarity r,
            float dMin, float dMax, float iv, float rg, string eff, string obtain)
        {
            chineseName = cn; englishName = en; weaponType = wt; elementType = et; rarity = r;
            damageMin = dMin; damageMax = dMax; interval = iv; range = rg;
            specialEffect = eff; obtainMethod = obtain;
        }
    }
}
