using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 时空裂隙 — 不稳定随机传送，支持"2选1"目的地预览
/// 设计文档参考：幻宫_时空回响_地图探索和生成系统拆解.md 4.2节
/// P1-004: 2选1目的地预览机制 — 5秒选择窗口，A/D切换，E确认，Esc取消
/// P1-006: 概率上限50%（由TimeRiftSpawner控制）
/// </summary>
public class TimeRift : MonoBehaviour
{
    [Header("裂隙设置")]
    public MapType destinationType = MapType.MemoryFragment;
    public float lifetime = 45f;

    [Header("视觉效果")]
    public ParticleSystem riftParticles;
    public Animator animator;

    [Header("预览设置（P1-004）")]
    [Tooltip("是否启用2选1预览机制")]
    public bool enablePreview = true;
    [Tooltip("预览选择时限（秒）")]
    public float previewTimeLimit = 5f;
    [Tooltip("预览交互距离")]
    public float previewInteractRange = 3f;

    [Header("传送设置")]
    [Tooltip("是否使用概率分配目的地（80%高难/20%记忆区域）")]
    public bool useProbabilityDestination = true;
    [Tooltip("高难地图比例（0~1）")]
    [Range(0f, 1f)]
    public float highDifficultyChance = 0.8f;

    // 预览状态
    private float age;
    private bool previewActive = false;
    private float countdownTimer;
    private MapType destinationA;
    private MapType destinationB;
    private int highlightedOption = 0; // 0=A, 1=B
    private bool playerInRange = false;
    private GameObject playerRef;

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        // 生成两个候选目的地
        GenerateDestinations();
    }

    /// <summary>
    /// 生成两个候选目的地（80%高难/20%记忆区域各抽一个，或两个高难）
    /// </summary>
    void GenerateDestinations()
    {
        if (useProbabilityDestination)
        {
            var currentMap = PortalUtility.GetCurrentMapType();
            var allHighDiffMaps = MapTypeMetaData.GetHighDifficultyMaps();

            // 过滤掉当前地图类型，避免传送到同一地图
            var highDiffMaps = new List<MapType>();
            foreach (var m in allHighDiffMaps)
            {
                if (m != currentMap) highDiffMaps.Add(m);
            }

            // 目的地A：80%高难 / 20%记忆
            if (Random.value < highDifficultyChance && highDiffMaps.Count > 0)
                destinationA = highDiffMaps[Random.Range(0, highDiffMaps.Count)];
            else
                destinationA = MapType.MemoryFragment;

            // 目的地B：同样规则，但确保与A不同
            MapType candidateB;
            int attempts = 0;
            do
            {
                if (Random.value < highDifficultyChance && highDiffMaps.Count > 1)
                {
                    candidateB = highDiffMaps[Random.Range(0, highDiffMaps.Count)];
                }
                else
                {
                    candidateB = MapType.MemoryFragment;
                }
                attempts++;
            } while (candidateB == destinationA && attempts < 10);

            destinationB = candidateB;

            // 默认目的地设为A
            destinationType = destinationA;
        }
        else
        {
            destinationA = destinationType;
            destinationB = destinationType;
        }

        Debug.Log($"[时空裂隙] 候选目的地 A={destinationA}, B={destinationB}");
    }

    void Update()
    {
        age += Time.deltaTime;

        // 最后10秒触发Animator淡出
        float remainingTime = lifetime - age;
        if (remainingTime <= 10f && animator != null)
            animator.SetBool("IsFadingOut", true);

        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 预览状态处理
        if (previewActive)
        {
            HandlePreviewInput();
            return;
        }

        // 检查玩家是否在交互范围内
        if (playerInRange && enablePreview && !previewActive)
        {
            // 玩家按E键触发预览
            if (Input.GetKeyDown(KeyCode.E))
            {
                ActivatePreview();
            }
        }
    }

    /// <summary>
    /// 激活目的地预览（P1-004）
    /// </summary>
    void ActivatePreview()
    {
        previewActive = true;
        countdownTimer = previewTimeLimit;
        highlightedOption = 0;
        Debug.Log($"[时空裂隙] 预览激活！A={destinationA}, B={destinationB}, 倒计时={countdownTimer}s");

        // 通知UI显示预览面板
        GlobalEventManager.Instance.TriggerRiftPreviewActivated(
            destinationA, destinationB, countdownTimer);
    }

    /// <summary>
    /// 处理预览期间的玩家输入
    /// </summary>
    void HandlePreviewInput()
    {
        countdownTimer -= Time.deltaTime;

        // 键盘切换选项
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            highlightedOption = 0;
            GlobalEventManager.Instance.TriggerRiftPreviewHighlightChanged(0);
            Debug.Log("[时空裂隙] 高亮选项A");
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            highlightedOption = 1;
            GlobalEventManager.Instance.TriggerRiftPreviewHighlightChanged(1);
            Debug.Log("[时空裂隙] 高亮选项B");
        }

        // 确认选择
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmSelection();
            return;
        }

        // 取消预览
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPreview();
            return;
        }

        // 玩家离开交互范围
        if (playerRef != null)
        {
            float dist = Vector3.Distance(transform.position, playerRef.transform.position);
            if (dist > previewInteractRange)
            {
                CancelPreview();
                return;
            }
        }

        // 超时处理
        if (countdownTimer <= 0)
        {
            OnPreviewTimeout();
        }
    }

    /// <summary>
    /// 确认选择并传送
    /// </summary>
    void ConfirmSelection()
    {
        MapType selected = (highlightedOption == 0) ? destinationA : destinationB;
        Debug.Log($"[时空裂隙] 玩家确认选择: {selected}");

        previewActive = false;
        GlobalEventManager.Instance.TriggerRiftPreviewClosed(selected);

        TeleportToDestination(selected);
    }

    /// <summary>
    /// 超时随机选择
    /// </summary>
    void OnPreviewTimeout()
    {
        MapType selected = (Random.value < 0.5f) ? destinationA : destinationB;
        Debug.Log($"[时空裂隙] 预览超时，随机选择: {selected}");

        previewActive = false;
        GlobalEventManager.Instance.TriggerRiftPreviewTimeout(selected);

        TeleportToDestination(selected);
    }

    /// <summary>
    /// 取消预览
    /// </summary>
    void CancelPreview()
    {
        previewActive = false;
        Debug.Log("[时空裂隙] 预览已取消，裂隙保留");
        GlobalEventManager.Instance.TriggerRiftPreviewCancelled();
    }

    /// <summary>
    /// 执行传送（委托给 PortalUtility）
    /// </summary>
    void TeleportToDestination(MapType destination)
    {
        PortalUtility.TeleportTo(destination);
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (age < 1f) return;

        playerInRange = true;
        playerRef = other.gameObject;

        // 如果不启用预览，直接传送（旧逻辑兼容）
        if (!enablePreview)
        {
            Debug.Log($"[时空裂隙] 玩家进入裂隙，传送到 {destinationType}");
            TeleportToDestination(destinationType);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerRef = null;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.8f);

        if (enablePreview)
        {
            Gizmos.color = new Color(1, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(transform.position, previewInteractRange);
        }
    }
}
