using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 时空门状态枚举
/// </summary>
public enum PortalState { Locked, Unlocking, Open }

/// <summary>
/// 时空门 — 稳定区域连接器，含目的地预览
/// 设计文档参考：幻宫_时空回响_地图探索和生成系统拆解.md 4.1节
/// 幻宫_时空回响_整合优化策划案 6章 - 时空门奖励预览机制
/// - 击败区域时空守护者后解锁
/// - 每个时空门连接2-3个特定区域
/// - 解锁时显示目标区域类型标签、难度等级、高亮信息
/// - 已解锁的门可在营火处快速传送
/// </summary>
public class TimePortal : MonoBehaviour
{
    [Header("传送门状态")]
    public PortalState currentState = PortalState.Locked;
    public MapType destinationMap = MapType.Forest;
    public float unlockDelay = 1.5f;

    [Header("视觉效果")]
    public ParticleSystem portalParticles;
    public Animator animator;

    [Header("预览设置（P0）")]
    [Tooltip("解锁时是否触发目的地预览")]
    public bool showPreviewOnUnlock = true;

    [Header("连接配置")]
    [Tooltip("是否使用随机连接表（随机从2-3个可选目的地中选择）")]
    public bool useConnectionTable = true;

    private static readonly Dictionary<MapType, MapType[]> ConnectionTable = new Dictionary<MapType, MapType[]>
    {
        { MapType.Forest,         new[] { MapType.Wasteland, MapType.Desert, MapType.Wetland } },
        { MapType.Wasteland,      new[] { MapType.Desert, MapType.RockLand, MapType.RuinCity } },
        { MapType.Desert,         new[] { MapType.RockLand, MapType.Volcano, MapType.RuinCity } },
        { MapType.RockLand,       new[] { MapType.Desert, MapType.IceField, MapType.Volcano } },
        { MapType.Wetland,        new[] { MapType.Forest, MapType.IceField, MapType.RuinCity } },
        { MapType.IceField,       new[] { MapType.Wetland, MapType.RockLand, MapType.ForgottenManor } },
        { MapType.Volcano,        new[] { MapType.RockLand, MapType.AncientTemple, MapType.LabFragment } },
        { MapType.RuinCity,       new[] { MapType.ForgottenManor, MapType.AncientTemple, MapType.LabFragment } },
        { MapType.ForgottenManor, new[] { MapType.RuinCity, MapType.AncientTemple, MapType.MemoryFragment } },
        { MapType.AncientTemple,  new[] { MapType.RuinCity, MapType.ForgottenManor, MapType.MemoryFragment } },
        { MapType.LabFragment,    new[] { MapType.RuinCity, MapType.Volcano, MapType.MemoryFragment } },
        { MapType.MemoryFragment, new[] { MapType.LabFragment, MapType.AncientTemple, MapType.TruthCorridor } },
        { MapType.TruthCorridor,  new[] { MapType.MemoryFragment } },
    };

    private bool isUnlocked;
    private Collider2D portalCollider;

    void Awake()
    {
        portalCollider = GetComponent<Collider2D>();
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        RefreshVisuals();
    }

    /// <summary>
    /// 由 BossRoomManager 调用 — 击败守护者后解锁时空门
    /// </summary>
    public void Unlock()
    {
        if (currentState != PortalState.Locked) return;
        StartCoroutine(UnlockSequence());
    }

    System.Collections.IEnumerator UnlockSequence()
    {
        currentState = PortalState.Unlocking;
        RefreshVisuals();
        Debug.Log("[时空传送门] 正在激活...");

        if (useConnectionTable)
            SelectDestinationFromTable();

        yield return new WaitForSeconds(unlockDelay);

        currentState = PortalState.Open;
        isUnlocked = true;
        RefreshVisuals();

        // P0: 触发目的地预览
        if (showPreviewOnUnlock)
        {
            BroadcastPreviewInfo();
        }

        Debug.Log($"[时空传送门] 已激活！目的地: {destinationMap}");
    }

    /// <summary>
    /// P0: 广播目的地预览信息给UI
    /// 包含：目标区域类型标签、难度等级、随机高亮信息
    /// </summary>
    void BroadcastPreviewInfo()
    {
        string mapTypeName = GetMapTypeDisplayName(destinationMap);
        string difficulty = EstimateDifficulty(destinationMap);
        string highlight = GetRandomHighlight(destinationMap);

        Debug.Log($"[时空传送门] 预览: {mapTypeName} | 难度={difficulty} | 特色={highlight}");
        GlobalEventManager.Instance.ShowNotification(
            $"时空门已激活 → {mapTypeName} [{difficulty}]\n{highlight}", 5f);
    }

    /// <summary>
    /// 获取地图类型的显示名称
    /// </summary>
    string GetMapTypeDisplayName(MapType mapType)
    {
        return mapType switch
        {
            MapType.Forest => "森林",
            MapType.Wasteland => "荒原",
            MapType.Desert => "荒漠",
            MapType.Wetland => "湿地",
            MapType.Volcano => "火山",
            MapType.IceField => "冰原",
            MapType.RockLand => "岩地",
            MapType.RuinCity => "废墟都市",
            MapType.ForgottenManor => "遗忘庄园",
            MapType.AncientTemple => "古代神殿",
            MapType.LabFragment => "实验室碎片",
            MapType.MemoryFragment => "记忆碎片区域",
            MapType.TruthCorridor => "真理长廊",
            _ => "未知区域"
        };
    }

    /// <summary>
    /// 预估目的地难度等级
    /// </summary>
    string EstimateDifficulty(MapType mapType)
    {
        // 高难地图
        if (mapType == MapType.Volcano || mapType == MapType.IceField ||
            mapType == MapType.RockLand || mapType == MapType.AncientTemple)
            return "困难";

        // 人文地图
        if (mapType == MapType.RuinCity || mapType == MapType.ForgottenManor)
            return "适中";

        // 特殊区域
        if (mapType == MapType.LabFragment || mapType == MapType.MemoryFragment ||
            mapType == MapType.TruthCorridor)
            return "极难";

        // 自然地图
        return "简单";
    }

    /// <summary>
    /// 随机生成一个高亮特色信息
    /// </summary>
    string GetRandomHighlight(MapType mapType)
    {
        string[] highlights = mapType switch
        {
            MapType.Forest => new[] { "基础材料丰富", "适合新手探索", "隐藏路径较多" },
            MapType.Wasteland => new[] { "稀有材料出现", "精英敌人较少" },
            MapType.Desert => new[] { "开阔地形", "侦察优势" },
            MapType.Wetland => new[] { "记忆残渣丰富", "环境减速注意" },
            MapType.Volcano => new[] { "熔岩核心可采集", "精英敌人密集", "环境伤害注意" },
            MapType.IceField => new[] { "极寒冰屑可采集", "冰原狼出没" },
            MapType.RockLand => new[] { "地脉结晶可采集", "容易卡位" },
            MapType.RuinCity => new[] { "机械核心可采集", "科技造物密集" },
            MapType.ForgottenManor => new[] { "记忆碎片线索", "灵魂残留密集" },
            MapType.AncientTemple => new[] { "远古铭文石可采集", "精英敌人密集" },
            MapType.LabFragment => new[] { "隐藏配方线索", "炼金材料丰富", "智慧属性要求" },
            MapType.MemoryFragment => new[] { "记忆碎片获取", "记忆守护者挑战" },
            MapType.TruthCorridor => new[] { "真结局关键区域", "极限挑战" },
            _ => new[] { "未知区域，探索需谨慎" }
        };

        return highlights[Random.Range(0, highlights.Length)];
    }

    void SelectDestinationFromTable()
    {
        var currentMap = GetCurrentMapType();
        if (ConnectionTable.TryGetValue(currentMap, out MapType[] destinations))
        {
            destinationMap = destinations[Random.Range(0, destinations.Length)];
        }
        else
        {
            destinationMap = MapType.Forest;
            Debug.LogWarning($"[时空传送门] 未找到 {currentMap} 的连接表，默认传送到 Forest");
        }
    }

    MapType GetCurrentMapType()
    {
        var map = FindObjectOfType<IntegratedMapSystem>();
        return map != null ? map.currentMapType : MapType.Forest;
    }

    void RefreshVisuals()
    {
        // 粒子：Locked时停，Unlocking和Open时播
        switch (currentState)
        {
            case PortalState.Locked:
                if (portalParticles != null) portalParticles.Stop();
                break;
            case PortalState.Unlocking:
            case PortalState.Open:
                if (portalParticles != null) portalParticles.Play();
                break;
        }

        // Animator由自身参数驱动，这里只触发动机
        if (animator != null)
        {
            animator.SetInteger("State", (int)currentState);
            if (currentState == PortalState.Unlocking)
                animator.SetTrigger("Unlock");
        }

        if (portalCollider != null)
            portalCollider.enabled = (currentState == PortalState.Open);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isUnlocked) return;

        Debug.Log($"[时空传送门] 玩家进入传送门，传送到 {destinationMap}");
        var map = FindObjectOfType<IntegratedMapSystem>();
        if (map != null)
        {
            map.SetMapType(destinationMap);
            map.GenerateNewMap();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = currentState == PortalState.Open ? Color.magenta : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
