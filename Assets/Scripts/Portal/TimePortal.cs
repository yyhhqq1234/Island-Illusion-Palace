using UnityEngine;
using System.Collections;

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

    [Header("交互设置")]
    [Tooltip("玩家交互距离")]
    public float interactRange = 3f;
    [Tooltip("提示文字")]
    public string interactHint = "按 E 进入时空传送门";

    private bool isUnlocked;
    private bool playerInRange;
    private Collider2D portalCollider;
    private GameObject playerRef;

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

    void Update()
    {
        if (!isUnlocked) return;

        // 检测玩家是否在交互范围内
        if (playerRef != null)
        {
            float dist = Vector3.Distance(transform.position, playerRef.transform.position);
            playerInRange = dist <= interactRange;
        }

        // E键交互
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
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

        // 从所有地图池中随机选择目的地
        destinationMap = PortalUtility.GetRandomDestination();

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
        string mapTypeName = MapTypeMetaData.GetDisplayName(destinationMap);
        string difficulty = MapTypeMetaData.GetDifficulty(destinationMap);
        var highlights = MapTypeMetaData.GetHighlights(destinationMap);
        string highlight = highlights[Random.Range(0, highlights.Length)];

        Debug.Log($"[时空传送门] 预览: {mapTypeName} | 难度={difficulty} | 特色={highlight}");
        GlobalEventManager.Instance.ShowNotification(
            $"时空门已激活 → {mapTypeName} [{difficulty}]\n{highlight}", 5f);
    }

    void RefreshVisuals()
    {
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

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.speed = 1f; // 确保循环播放
            SetAnimatorParamSafe("State", (int)currentState);
            if (currentState == PortalState.Unlocking)
                SetAnimatorTriggerSafe("Unlock");
        }

        if (portalCollider != null)
            portalCollider.enabled = (currentState == PortalState.Open);
    }

    void SetAnimatorParamSafe(string name, int value)
    {
        foreach (var p in animator.parameters)
            if (p.name == name) { animator.SetInteger(name, value); return; }
    }

    void SetAnimatorTriggerSafe(string name)
    {
        foreach (var p in animator.parameters)
            if (p.name == name) { animator.SetTrigger(name); return; }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerRef = other.gameObject;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerRef = null;
            playerInRange = false;
        }
    }

    /// <summary>
    /// 从所有地图池中随机选择一个目的地并传送
    /// </summary>
    void TeleportPlayer()
    {
        destinationMap = PortalUtility.GetRandomDestination();
        Debug.Log($"[时空传送门] 玩家使用传送门，目的地: {destinationMap}");
        PortalUtility.TeleportTo(destinationMap);
    }

}
