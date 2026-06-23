using System.Collections.Generic;
using GameSystems;

/// <summary>
/// 材料数据静态数据库 — 单一数据源，消除三处重复的 GetMaterialData() 24-way switch。
/// 对应 I_IP材料清单.md 定义。
/// 显式使用 GameSystems.MaterialData 避免与 AlchemyUI.MaterialData 冲突。
/// </summary>
public static class MaterialDatabase
{
    private static readonly Dictionary<MaterialTypeEnum, GameSystems.MaterialData> _data;

    static MaterialDatabase()
    {
        _data = new Dictionary<MaterialTypeEnum, GameSystems.MaterialData>
        {
            // ═══════════════════════════════════════
            // 基础材料（10种，价值1-10）
            // ═══════════════════════════════════════
            [MaterialTypeEnum.RottenWood]          = new GameSystems.MaterialData(MaterialTypeEnum.RottenWood,          "腐朽木屑",   1, "枯朽的木材碎片。",                       "常见", "森林地图采集"),
            [MaterialTypeEnum.BoneFragments]        = new GameSystems.MaterialData(MaterialTypeEnum.BoneFragments,       "碎骨",       2, "断裂的骨骼。",                           "常见", "骷髅战士、腐化村民掉落"),
            [MaterialTypeEnum.StarlightGrass]       = new GameSystems.MaterialData(MaterialTypeEnum.StarlightGrass,      "星光草",     3, "夜晚发光的植物。",                       "常见", "森林地图采集"),
            [MaterialTypeEnum.CorruptedTissue]      = new GameSystems.MaterialData(MaterialTypeEnum.CorruptedTissue,     "腐化组织",   4, "被幻宫能量侵蚀的肉块。",                 "常见", "腐化村民、沼泽潜伏者掉落"),
            [MaterialTypeEnum.MoonlightFlower]      = new GameSystems.MaterialData(MaterialTypeEnum.MoonlightFlower,     "月影花",     5, "满月夜绽放的银色花朵。",                 "常见", "月光下的森林采集"),
            [MaterialTypeEnum.RustedParts]          = new GameSystems.MaterialData(MaterialTypeEnum.RustedParts,         "锈蚀零件",   6, "废弃机械的金属零件。",                   "常见", "机械残骸掉落，废墟地图采集"),
            [MaterialTypeEnum.SoulDust]             = new GameSystems.MaterialData(MaterialTypeEnum.SoulDust,            "灵魂微尘",   7, "逸散的灵魂能量微粒。",                   "常见", "怨魂等灵魂类敌人掉落"),
            [MaterialTypeEnum.CloudyDew]            = new GameSystems.MaterialData(MaterialTypeEnum.CloudyDew,           "浑浊露珠",   8, "凝结时空能量的水珠。",                   "常见", "森林、荒原、湿地、冰原采集"),
            [MaterialTypeEnum.PurifyingSalt]        = new GameSystems.MaterialData(MaterialTypeEnum.PurifyingSalt,       "净化盐晶",   9, "蕴含净化能量的盐结晶。",                 "常见", "岩地、沙漠采集，结晶蜥蜴、石像鬼掉落"),
            [MaterialTypeEnum.WailingVine]          = new GameSystems.MaterialData(MaterialTypeEnum.WailingVine,         "哀嚎藤蔓",  10, "触碰时会发出悲鸣的黑暗植物。",           "常见", "沼泽潜伏者掉落，湿地地图采集"),

            // ═══════════════════════════════════════
            // 稀有材料（10种，价值11-20）
            // ═══════════════════════════════════════
            [MaterialTypeEnum.MemoryResidue]        = new GameSystems.MaterialData(MaterialTypeEnum.MemoryResidue,       "记忆残渣",  11, "记忆逸散的能量渣滓。",                   "稀有", "记忆碎片区域采集，怨魂掉落（低概率）"),
            [MaterialTypeEnum.TimeFragment]          = new GameSystems.MaterialData(MaterialTypeEnum.TimeFragment,        "时空碎片",  12, "包含不稳定时空能量的脆弱碎片。",           "稀有", "时空守护者掉落，时空裂隙区域采集"),
            [MaterialTypeEnum.SoulCrystal]          = new GameSystems.MaterialData(MaterialTypeEnum.SoulCrystal,         "灵魂结晶",  13, "提纯凝固的灵魂能量块。",                 "稀有", "灵魂吞噬者掉落"),
            [MaterialTypeEnum.CrystalizedCore]      = new GameSystems.MaterialData(MaterialTypeEnum.CrystalizedCore,     "晶化残核",  14, "晶化生物遗留的惰性能量核。",             "稀有", "结晶蜥蜴掉落"),
            [MaterialTypeEnum.MechCore]             = new GameSystems.MaterialData(MaterialTypeEnum.MechCore,            "机械核心",  15, "古代机械的微型动力核心。",               "稀有", "机械构造体掉落"),
            [MaterialTypeEnum.AncientTreeResin]     = new GameSystems.MaterialData(MaterialTypeEnum.AncientTreeResin,    "古树树脂",  16, "古老树木渗出的生命力树脂。",             "稀有", "森林地图特定古树采集"),
            [MaterialTypeEnum.LavaCore]             = new GameSystems.MaterialData(MaterialTypeEnum.LavaCore,            "熔岩核心",  17, "一小块永不熄灭的炽热能量核。",           "稀有", "熔岩元素掉落"),
            [MaterialTypeEnum.GargoyleFragment]     = new GameSystems.MaterialData(MaterialTypeEnum.GargoyleFragment,    "石像鬼碎片", 18, "活化雕像上剥落的带魔力石片。",           "稀有", "石像鬼掉落"),
            [MaterialTypeEnum.FrostShard]           = new GameSystems.MaterialData(MaterialTypeEnum.FrostShard,          "极寒冰屑",  19, "永恒冻土中形成的刺骨冰屑。",             "稀有", "冰原采集，冰原狼掉落"),
            [MaterialTypeEnum.AncientDragonBonePowder] = new GameSystems.MaterialData(MaterialTypeEnum.AncientDragonBonePowder, "古龙骨粉", 20, "研磨自远古巨兽骨骼的能量粉末。", "稀有", "岩地、冰原远古遗骸处采集"),

            // ═══════════════════════════════════════
            // 史诗材料（3种）
            // ═══════════════════════════════════════
            [MaterialTypeEnum.SoulEssence]          = new GameSystems.MaterialData(MaterialTypeEnum.SoulEssence,         "灵魂精华",   1, "用于炼金的高纯度灵魂能量。",             "史诗", "灵魂炼金（灵魂×50）"),
            [MaterialTypeEnum.LeylineCrystal]       = new GameSystems.MaterialData(MaterialTypeEnum.LeylineCrystal,      "地脉结晶",  30, "大地能量脉络中凝结的坚固结晶。",         "史诗", "岩地深处采集"),
            [MaterialTypeEnum.AncientRuneStone]     = new GameSystems.MaterialData(MaterialTypeEnum.AncientRuneStone,    "远古铭文石", 33, "刻有失落知识的石碑碎片。",               "史诗", "神殿、废墟解谜区域采集"),

            // ═══════════════════════════════════════
            // 传奇材料（1种）
            // ═══════════════════════════════════════
            [MaterialTypeEnum.ParadoxShard]         = new GameSystems.MaterialData(MaterialTypeEnum.ParadoxShard,        "悖时薄片",  40, "凝固的、记录时间悖论的时空片段。",       "传奇", "时空守护者掉落"),
        };
    }

    /// <summary>获取材料数据，不存在时返回默认未知材料</summary>
    public static GameSystems.MaterialData GetMaterialData(MaterialTypeEnum type)
    {
        return _data.TryGetValue(type, out var data) ? data : new GameSystems.MaterialData(type, type.ToString(), 0, "未知材料", "未知", "未知");
    }

    /// <summary>获取材料中文名称</summary>
    public static string GetMaterialName(MaterialTypeEnum type) => GetMaterialData(type).name;

    /// <summary>获取材料炼金价值</summary>
    public static int GetMaterialValue(MaterialTypeEnum type) => GetMaterialData(type).value;
}
