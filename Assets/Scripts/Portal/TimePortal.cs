using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 时空门状态枚举
/// </summary>
public enum PortalState { Locked, Unlocking, Open }

/// <summary>
/// 时空门 — 稳定区域连接器
/// 设计文档参考：幻宫_时空回响_地图探索和生成系统拆解.md 4.1节
/// - 击败区域时空守护者后解锁
/// - 每个时空门连接2-3个特定区域
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
        Debug.Log($"[时空传送门] 已激活！目的地: {destinationMap}");
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
