using UnityEngine;
using IIP.Ending;

/// <summary>
/// Boss房间管理器 — 管理Boss的生成、击败流程与奖励掉落
/// 
/// 职责：
/// 1. 检测玩家进入Boss房间 -> 生成Boss
/// 2. 监听Boss击败事件 -> 触发奖励掉落
/// 3. 生成传送门(时间传送门)和宝箱
/// 4. 剧情触发（最终Boss时调用EndingSystem）
/// 5. 成就/统计记录
/// </summary>
public class BossRoomManager : MonoBehaviour
{
    // ═══════════════════════════════════════════
    // Boss配置
    // ═══════════════════════════════════════════

    [Header("Boss配置")]
    [Tooltip("时空守护者预制件")]
    public GameObject timeGuardianPrefab;

    [Tooltip("Boss生成点")]
    public Transform bossSpawnPoint;

    [Tooltip("是否为最终Boss房间（S-SN）")]
    public bool isFinalBossRoom = false;

    // ═══════════════════════════════════════════
    // Boss击败奖励配置
    // ═══════════════════════════════════════════

    [Header("Boss击败奖励")]
    [Tooltip("时间传送门预制件（Boss击败后生成）")]
    public GameObject timePortalPrefab;

    [Tooltip("宝箱预制件（Boss击败后生成）")]
    public GameObject treasureChestPrefab;

    [Tooltip("是否在击败后生成传送门")]
    public bool spawnPortalOnDefeat = true;

    [Tooltip("是否在击败后生成宝箱")]
    public bool spawnChestOnDefeat = true;

    // ═══════════════════════════════════════════
    // 掉落参数（按策划文档数值）
    // ═══════════════════════════════════════════

    [Header("掉落参数")]
    [Tooltip("灵魂掉落量范围（最小~最大）")]
    public Vector2Int soulDropRange = new Vector2Int(60, 90);

    [Tooltip("灵魂精华掉落数量范围")]
    public Vector2Int essenceDropRange = new Vector2Int(3, 6);

    [Tooltip("记忆碎片掉落概率 (0~1)")]
    [Range(0f, 1f)]
    public float memoryFragmentDropChance = 0.05f;

    [Tooltip("灵魂之核品质（对应颜色）")]
    public SoulCoreQuality droppedSoulCoreQuality = SoulCoreQuality.Gold;

    // ═══════════════════════════════════════════
    // 裂隙配置
    // ═══════════════════════════════════════════

    [Header("裂隙配置")]
    [Tooltip("Boss房间内是否允许生成时空裂隙")]
    public bool enableRiftsInBossRoom = false;

    [Tooltip("时空裂隙预制件")]
    public GameObject riftPrefab;

    // ═══════════════════════════════════════════
    // 运行时状态
    // ═══════════════════════════════════════════

    private GameObject spawnedBoss;
    private bool bossDefeated = false;
    private bool playerEntered = false;

    /// <summary>Boss战开始时间</summary>
    private float battleStartTime;

    /// <summary>当前Boss引用（供外部查询）</summary>
    public GameObject SpawnedBoss => spawnedBoss;

    /// <summary>Boss是否已被击败</summary>
    public bool IsBossDefeated => bossDefeated;

    // ═══════════════════════════════════════════
    // 生命周期
    // ═══════════════════════════════════════════

    void Start()
    {
        // 订阅全局事件
        GlobalEventManager.Instance.OnBossDefeated += OnBossDefeated;
    }

    void OnDestroy()
    {
        // 取消订阅，防止内存泄漏
        var gem = GlobalEventManager.Instance;
        if (gem != null)
            gem.OnBossDefeated -= OnBossDefeated;
    }

    // ═══════════════════════════════════════════
    // 玩家检测与Boss生成
    // ═══════════════════════════════════════════

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (bossDefeated) return;
        if (playerEntered) return;

        playerEntered = true;
        SpawnBoss(other.transform); // 传入触发的玩家 Transform
    }

    /// <summary>
    /// 生成Boss — playerTransform 由 OnTriggerEnter2D 传入，确保坐标正确
    /// </summary>
    public void SpawnBoss(Transform playerTransform = null)
    {
        if (spawnedBoss != null)
        {
            Debug.LogWarning("[BossRoomManager] Boss已存在，跳过重复生成");
            return;
        }

        if (timeGuardianPrefab == null)
        {
            Debug.LogError("[BossRoomManager] 未设置Boss预制件！无法生成Boss");
            return;
        }

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Debug.LogWarning($"[BossRoomManager] 生成Boss bossPos={spawnPos} playerPos={playerTransform?.position}");

        spawnedBoss = Instantiate(timeGuardianPrefab, spawnPos, Quaternion.identity);
        spawnedBoss.name = isFinalBossRoom ? "FinalBoss_SSN" : "TimeGuardian";

        if (!spawnedBoss.CompareTag("Enemy"))
            spawnedBoss.tag = "Enemy";

        // 直接设置玩家引用，避免 FindGameObjectWithTag 找到错误对象
        var bossAI = spawnedBoss.GetComponent<BossAI>();
        if (bossAI != null)
        {
            if (playerTransform != null)
                bossAI.player = playerTransform;
            bossAI.Activate();
        }

        // 广播Boss遭遇事件
        GlobalEventManager.Instance.TriggerBossEncounter(spawnedBoss);
        battleStartTime = Time.time;

        // 显示UI通知
        string bossName = isFinalBossRoom ? "Scarlet Soul Shana" : "时空守护者";
        GlobalEventManager.Instance.ShowNotification($"警告：{bossName} 出现了！", 4f);

        // 切换音乐到Boss战状态
        GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.BossBattle);

        Debug.Log($"[BossRoomManager] {spawnedBoss.name} 已生成于 ({spawnPos.x:F1}, {spawnPos.y:F1})");
    }

    // ═══════════════════════════════════════════
    // Boss击败处理（核心奖励流程）
    // ═══════════════════════════════════════════

    /// <summary>
    /// Boss击败事件回调 — 由GlobalEventManager广播触发
    /// 奖励在Boss消失后延迟生成
    /// </summary>
    void OnBossDefeated(GameObject boss)
    {
        if (spawnedBoss == null || boss != spawnedBoss) return;

        bossDefeated = true;
        float battleDuration = Time.time - battleStartTime;

        Debug.Log($"[BossRoomManager] Boss被击败！战斗时长: {battleDuration:F1}s");
        RecordBattleStatistics(battleDuration);

        // 延迟生成奖励，等Boss消失动画播放完毕
        StartCoroutine(SpawnRewardsAfterDelay(battleDuration));
    }

    System.Collections.IEnumerator SpawnRewardsAfterDelay(float battleDuration)
    {
        // 等待Boss死亡动画 + 消失（BossAI.Die中 Destroy(gameObject, 1f)）
        yield return new WaitForSeconds(1.5f);

        Debug.Log("========== Boss已消失，开始生成奖励 ==========");

        ProcessRewardDrops();

        if (spawnPortalOnDefeat && timePortalPrefab != null)
            SpawnTimePortal();

        if (spawnChestOnDefeat && treasureChestPrefab != null)
            SpawnTreasureChest();

        if (isFinalBossRoom)
            TriggerEndingSequence();

        // Boss消失后再切换音乐
        GlobalEventManager.Instance.RequestMusicState(GlobalEventManager.MusicState.Exploration);
        GlobalEventManager.Instance.ShowNotification("Boss被击败！奖励已掉落！", 3f);

        Debug.Log("========== 奖励生成完成 ==========");
    }

    // ═══════════════════════════════════════════
    // 奖励掉落系统
    // ═══════════════════════════════════════════

    /// <summary>处理所有奖励掉落</summary>
    void ProcessRewardDrops()
    {
        // A. 灵魂掉落（必掉）
        DropSouls();

        // B. 灵魂之核掉落（必掉，金色品质）
        DropSoulCore();

        // C. 灵魂精华掉落（必掉）
        DropEssence();

        // D. 记忆碎片概率掉落
        TryDropMemoryFragment();
    }

    void DropSouls()
    {
        int soulAmount = Random.Range(soulDropRange.x, soulDropRange.y + 1);
        // 通过InventorySystem或事件系统添加灵魂
        // 这里使用事件广播让其他系统处理实际添加
        Debug.Log($"[BossRoomManager] 灵魂掉落: x{soulAmount}");
        // TODO: 调用 InventorySystem.AddSouls(soulAmount);
    }

    void DropSoulCore()
    {
        Debug.Log($"[BossRoomManager] 灵魂之核掉落: 品质={droppedSoulCoreQuality} (金色)");
        // TODO: 调用 SummonSystem.AddSoulCore(droppedSoulCoreQuality, enemyType);
    }

    void DropEssence()
    {
        int essenceCount = Random.Range(essenceDropRange.x, essenceDropRange.y + 1);
        Debug.Log($"[BossRoomManager] 灵魂精华掉落: x{essenceCount}");
        // TODO: 调用 InventorySystem.AddEssence(essenceCount);
    }

    void TryDropMemoryFragment()
    {
        if (Random.value <= memoryFragmentDropChance)
        {
            Debug.Log($"[BossRoomManager] ★ 记忆碎片掉落成功！(概率{memoryFragmentDropChance:P0})");
            // TODO: 调用 MemoryFragmentSystem.GrantRandomFragment();
            GlobalEventManager.Instance.ShowNotification("获得：记忆碎片！", 3f);
        }
        else
        {
            Debug.Log($"[BossRoomManager] 记忆碎片未掉落 (概率{memoryFragmentDropChance:P0})");
        }
    }

    // ═══════════════════════════════════════════
    // 场景对象生成
    // ═══════════════════════════════════════════

    void SpawnTimePortal()
    {
        // 在Boss生成点位生成传送门
        Vector3 portalPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        var portal = Instantiate(timePortalPrefab, portalPos, Quaternion.identity);
        portal.name = "TimePortal_BossReward";

        var tp = portal.GetComponent<TimePortal>();
        if (tp != null) tp.Unlock();

        Debug.Log($"[BossRoomManager] 时间传送门已生成于 ({portalPos.x:F1}, {portalPos.y:F1})");
    }

    void SpawnTreasureChest()
    {
        Vector3 chestPos = (bossSpawnPoint != null ? bossSpawnPoint.position : transform.position) + Vector3.right * 2f;
        var chest = Instantiate(treasureChestPrefab, chestPos, Quaternion.identity);
        chest.name = "TreasureChest_BossReward";

        Debug.Log($"[BossRoomManager] 宝箱已生成于 ({chestPos.x:F1}, {chestPos.y:F1})");
    }

    // ═══════════════════════════════════════════
    // 剧情触发（最终Boss专用）
    // ═══════════════════════════════════════════

    void TriggerEndingSequence()
    {
        Debug.Log("[BossRoomManager] ★ 检测到最终Boss defeated，触发结局序列...");

        // 查找结局系统
        var endingSystem = FindObjectOfType<EndingSystem>();
        if (endingSystem != null)
        {
            // 根据收集的记忆碎片数量决定结局
            var mf = FindObjectOfType<MemoryFragmentSystem>();
            int fragmentCount = mf != null ? mf.GetFragmentCount() : 0;

            endingSystem.TriggerEnding(fragmentCount);
            Debug.Log($"[BossRoomManager] 结局系统已触发，记忆碎片数: {fragmentCount}");
        }
        else
        {
            Debug.LogWarning("[BossRoomManager] 未找到EndingSystem，跳过结局触发");
        }
    }

    // ═══════════════════════════════════════════
    // 统计记录
    // ═══════════════════════════════════════════

    void RecordBattleStatistics(float duration)
    {
        Debug.Log("---------- Boss战统计 ----------");
        Debug.Log($"  房间类型: {(isFinalBossRoom ? "最终Boss" : "区域Boss")}");
        Debug.Log($"  Boss名称: {spawnedBoss?.name ?? "Unknown"}");
        Debug.Log($"  战斗时长: {duration:F1}s");

        // 尝试从BossAI获取详细数据
        if (spawnedBoss != null)
        {
            var bossAI = spawnedBoss.GetComponent<BossAI>();
            if (bossAI != null)
            {
                // 通过反射或公共方法获取统计数据
                // （BossAI中的统计数据是private的，可考虑添加公共只读属性）
                Debug.Log($"  最终阶段: {bossAI.currentPhase}");
            }
        }

        // 成就检查示例
        CheckAchievements(duration);
        Debug.Log("------------------------------");
    }

    void CheckAchievements(float duration)
    {
        // 示例成就检测（实际项目中应接入Steam Achievement API）

        // 速通成就：120秒内击败
        if (duration <= 120f)
        {
            Debug.Log("[BossRoomManager] 成就解锁候选: 速通者 (< 2分钟)");
        }

        // 无伤成就（需要额外追踪玩家是否受伤）
        // 完美战斗（未使用炼金物品等）

        Debug.Log("[BossRoomManager] 成就检测完成");
    }

    // ═══════════════════════════════════════════
    // 公共接口
    // ═══════════════════════════════════════════

    /// <summary>重置Boss房间状态（新周目/重新挑战）</summary>
    public void ResetRoom()
    {
        if (spawnedBoss != null)
        {
            Destroy(spawnedBoss);
            spawnedBoss = null;
        }
        bossDefeated = false;
        playerEntered = false;
        Debug.Log("[BossRoomManager] 房间状态已重置");
    }

    /// <summary>手动触发Boss生成（调试/测试用）</summary>
    [ContextMenu("调试：手动生成Boss")]
    public void DebugSpawnBoss()
    {
        playerEntered = true;
        SpawnBoss();
    }

    /// <summary>手动触发奖励生成（调试/测试用）</summary>
    [ContextMenu("调试：模拟Boss击败")]
    public void DebugSimulateBossDefeat()
    {
        if (spawnedBoss == null)
        {
            Debug.LogWarning("[BossRoomManager] 请先生成Boss");
            return;
        }
        OnBossDefeated(spawnedBoss);
    }

    // ═══════════════════════════════════════════
    // Gizmos 绘制
    // ═══════════════════════════════════════════

    void OnDrawGizmosSelected()
    {
        // Boss房间范围（39x26 对应标准地图子区域大小）
        Gizmos.color = bossDefeated ? new Color(0f, 1f, 0f, 0.3f) : new Color(1f, 0f, 0f, 0.4f);
        Vector3 pos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        Gizmos.DrawWireCube(pos, new Vector3(39f, 26f, 0f));

        // 生成点标记
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(pos, 0.5f);
    }
}

// ════════════════════════════════════════════════════════════════════════
// 辅助枚举/常量
// ════════════════════════════════════════════════════════════════════════

// SoulCoreQuality 枚举已定义在 SummonSystem.cs 中，此处复用
// (Common=White, Elite=Blue, Boss=Gold)
