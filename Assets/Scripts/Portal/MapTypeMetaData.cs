/// <summary>
/// MapType 静态元数据类
/// 提供地图类型的显示名称、难度等级、高亮信息、高难地图列表等查询方法
/// 整合自 TimePortal.cs 和 TimeRift.cs 中的分散数据
/// </summary>
public static class MapTypeMetaData
{
    /// <summary>
    /// 获取地图类型的中文显示名称
    /// </summary>
    public static string GetDisplayName(MapType type)
    {
        return type switch
        {
            MapType.Forest          => "森林",
            MapType.Wasteland       => "荒原",
            MapType.Desert          => "荒漠",
            MapType.Wetland         => "湿地",
            MapType.Volcano         => "火山",
            MapType.IceField        => "冰原",
            MapType.RockLand        => "岩地",
            MapType.RuinCity        => "废墟都市",
            MapType.ForgottenManor  => "遗忘庄园",
            MapType.AncientTemple   => "古代神殿",
            MapType.LabFragment     => "实验室碎片",
            MapType.MemoryFragment  => "记忆碎片区域",
            MapType.TruthCorridor   => "真理长廊",
            _                       => "未知区域"
        };
    }

    /// <summary>
    /// 获取地图难度等级
    /// 分类：简单（自然地图）、适中（人文地图）、困难（高难地图）、极难（特殊区域）
    /// </summary>
    public static string GetDifficulty(MapType type)
    {
        // 高难地图
        if (type == MapType.Volcano || type == MapType.IceField ||
            type == MapType.RockLand || type == MapType.AncientTemple)
            return "困难";

        // 人文地图
        if (type == MapType.RuinCity || type == MapType.ForgottenManor)
            return "适中";

        // 特殊区域
        if (type == MapType.LabFragment || type == MapType.MemoryFragment ||
            type == MapType.TruthCorridor)
            return "极难";

        // 自然地图（Forest, Wasteland, Desert, Wetland）
        return "简单";
    }

    /// <summary>
    /// 获取地图的高亮特色信息数组
    /// 用于时空门解锁时的随机预览展示
    /// </summary>
    public static string[] GetHighlights(MapType type)
    {
        return type switch
        {
            MapType.Forest          => new[] { "基础材料丰富", "适合新手探索", "隐藏路径较多" },
            MapType.Wasteland       => new[] { "稀有材料出现", "精英敌人较少" },
            MapType.Desert          => new[] { "开阔地形", "侦察优势" },
            MapType.Wetland         => new[] { "记忆残渣丰富", "环境减速注意" },
            MapType.Volcano         => new[] { "熔岩核心可采集", "精英敌人密集", "环境伤害注意" },
            MapType.IceField        => new[] { "极寒冰屑可采集", "冰原狼出没" },
            MapType.RockLand        => new[] { "地脉结晶可采集", "容易卡位" },
            MapType.RuinCity        => new[] { "机械核心可采集", "科技造物密集" },
            MapType.ForgottenManor  => new[] { "记忆碎片线索", "灵魂残留密集" },
            MapType.AncientTemple   => new[] { "远古铭文石可采集", "精英敌人密集" },
            MapType.LabFragment     => new[] { "隐藏配方线索", "炼金材料丰富", "智慧属性要求" },
            MapType.MemoryFragment  => new[] { "记忆碎片获取", "记忆守护者挑战" },
            MapType.TruthCorridor   => new[] { "真结局关键区域", "极限挑战" },
            _                       => new[] { "未知区域，探索需谨慎" }
        };
    }

    /// <summary>
    /// 获取全部高难度地图类型列表
    /// 用于时空裂隙的目的地随机选取
    /// </summary>
    public static MapType[] GetHighDifficultyMaps()
    {
        return new MapType[]
        {
            MapType.Volcano,
            MapType.IceField,
            MapType.RockLand,
            MapType.RuinCity,
            MapType.AncientTemple,
            MapType.ForgottenManor,
        };
    }

    /// <summary>
    /// 判断给定地图类型是否为高难度地图
    /// </summary>
    public static bool IsHighDifficulty(MapType type)
    {
        return type == MapType.Volcano || type == MapType.IceField ||
               type == MapType.RockLand || type == MapType.AncientTemple ||
               type == MapType.RuinCity || type == MapType.ForgottenManor;
    }
}
