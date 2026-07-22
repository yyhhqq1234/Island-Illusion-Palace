using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

/// <summary>
/// 小地图UI组件
/// 显示玩家周围区域的简化地图，包含玩家位置、敌人位置、重要地标等
/// 策划规范 1.2.4：右上角 180x180 圆形裁剪，M键全屏，滚轮缩放 80-240px，静止3秒降至30%透明度
/// </summary>
public class MinimapUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("小地图背景（圆角外框）")]
    public Image background;

    [Tooltip("小地图遮罩（圆形裁剪用，需 Mask 组件）")]
    public Image mask;

    [Tooltip("玩家图标（中心箭头）")]
    public Image playerIcon;

    [Tooltip("敌人图标预制体（可选，不填用默认）")]
    public GameObject enemyIconPrefab;

    [Tooltip("地标图标预制体（可选，不填用默认）")]
    public GameObject landmarkIconPrefab;

    [Header("尺寸与缩放")]
    [Tooltip("小地图基础大小")]
    public float minimapSize = 180f;

    [Tooltip("滚轮缩放最小值")]
    public float minSize = 80f;

    [Tooltip("滚轮缩放最大值")]
    public float maxSize = 240f;

    [Tooltip("全屏地图尺寸")]
    public float fullscreenSize = 600f;

    [Header("样式配置")]
    [Tooltip("背景颜色")]
    public Color backgroundColor = IIPUIStyle.MinimapBackground;

    [Tooltip("边框颜色")]
    public Color borderColor = IIPUIStyle.BorderBright;

    [Tooltip("玩家图标颜色（青色）")]
    public Color playerColor = IIPUIStyle.AccentCyan;

    [Tooltip("敌人图标颜色（红色）")]
    public Color enemyColor = IIPUIStyle.AccentRed;

    [Header("地标颜色")]
    [Tooltip("营火地标颜色（橙）")]
    public Color campfireColor = IIPUIStyle.WarningOrange;

    [Tooltip("安全区地标颜色（绿）")]
    public Color safeZoneColor = IIPUIStyle.HealthFill;

    [Tooltip("传送点地标颜色（紫）")]
    public Color portalColor = IIPUIStyle.AccentPurple;

    [Header("显示范围")]
    [Tooltip("小地图显示范围（世界半径）")]
    public float displayRange = 50f;

    [Tooltip("玩家图标大小")]
    public float playerIconSize = 8f;

    [Tooltip("敌人图标大小")]
    public float enemyIconSize = 6f;

    [Tooltip("地标图标大小（营火/传送点）")]
    public float landmarkIconSize = 8f;

    [Tooltip("安全区图标大小")]
    public float safeZoneIconSize = 10f;

    [Header("图层设置")]
    [Tooltip("敌人图层")]
    public LayerMask enemyLayer;

    [Header("透明度与交互")]
    [Tooltip("激活时透明度（默认70%）")]
    public float activeAlpha = 0.7f;

    [Tooltip("静止超时后透明度（30%）")]
    public float idleAlpha = 0.3f;

    [Tooltip("静止超时时间（秒）")]
    public float idleTimeout = 3f;

    [Tooltip("透明度过渡速度")]
    public float alphaLerpSpeed = 4f;

    [Header("轮询间隔")]
    [Tooltip("敌人位置轮询间隔（秒）")]
    public float enemyPollInterval = 0.3f;

    [Tooltip("地标轮询间隔（秒）")]
    public float landmarkPollInterval = 0.5f;

    [Tooltip("对象池预创建数量")]
    public int poolPrewarmCount = 16;

    [Header("圆形遮罩")]
    [Tooltip("是否使用圆形遮罩（需要圆形 Sprite；false 则退化为 RectMask2D 圆角矩形）")]
    public bool useCircleMask = true;

    private Transform playerTransform;
    private Rigidbody2D playerRb; // 缓存玩家刚体，避免每帧 GetComponent
    private RectTransform containerRect; // 外层 container（缩放/全屏切换作用对象）
    private Transform iconParent; // 动态图标挂载点（maskObj，受 Mask/RectMask2D 裁剪）
    private CanvasGroup canvasGroup;
    private bool isFullscreen = false;

    // 对象池
    private List<GameObject> enemyIconPool = new List<GameObject>();
    private List<GameObject> landmarkIconPool = new List<GameObject>();
    private int enemyIconActiveCount = 0;
    private int landmarkIconActiveCount = 0;

    // 轮询计时器
    private float enemyPollTimer = 0f;
    private float landmarkPollTimer = 0f;

    // 静止透明度
    private float lastActivityTime = 0f;
    private Vector3 lastPlayerPos = Vector3.zero;
    private const float MoveThreshold = 0.1f;

    void Start()
    {
        EnsureContainerLayout();

        // 玩家查找
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerRb = player.GetComponent<Rigidbody2D>(); // 缓存一次
            lastPlayerPos = playerTransform.position;
        }
        else
        {
            Debug.LogWarning("[MinimapUI] 未找到玩家对象");
        }

        InitializeUI();
        InitializeCanvasGroup();
        InitializePools();
        lastActivityTime = Time.time;
    }

    void OnEnable()
    {
        // 小地图以轮询为主，无事件订阅；保留方法便于未来接入 OnMapTypeChanged 等
    }

    void OnDisable()
    {
    }

    /// <summary>
    /// 确保根 RectTransform 右上角布局
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -24);
        rt.sizeDelta = new Vector2(minimapSize, minimapSize);
    }

    /// <summary>
    /// 初始化 CanvasGroup（用于透明度过渡）
    /// </summary>
    void InitializeCanvasGroup()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = activeAlpha;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    void InitializePools()
    {
        // Inspector 赋了 background 导致 InitializeUI 未调 CreateDefaultUI 时，containerRect 可能为 null
        if (containerRect == null) CreateDefaultUI();
        if (containerRect == null) return;
        for (int i = 0; i < poolPrewarmCount; i++)
        {
            GameObject enemy = CreateEnemyIcon();
            enemy.SetActive(false);
            enemyIconPool.Add(enemy);

            GameObject landmark = CreateLandmarkIcon();
            landmark.SetActive(false);
            landmarkIconPool.Add(landmark);
        }
    }

    void Update()
    {
        if (playerTransform == null)
        {
            // 玩家可能延迟生成，重试查找
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerRb = player.GetComponent<Rigidbody2D>();
                lastPlayerPos = playerTransform.position;
            }
            else
            {
                return;
            }
        }

        HandleInput();
        UpdateIdleAlpha();
        UpdateMinimap();
    }

    /// <summary>
    /// 输入：M键全屏切换 + 滚轮缩放
    /// </summary>
    void HandleInput()
    {
        // M 键切换全屏
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleFullscreen();
            lastActivityTime = Time.time;
        }

        // 滚轮缩放（非全屏时）
        if (!isFullscreen)
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                minimapSize = Mathf.Clamp(minimapSize + scroll * 30f, minSize, maxSize);
                if (containerRect != null)
                {
                    containerRect.sizeDelta = new Vector2(minimapSize, minimapSize);
                }
                lastActivityTime = Time.time;
            }
        }
    }

    /// <summary>
    /// 切换全屏/小窗
    /// </summary>
    void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen;
        if (containerRect == null) return;

        if (isFullscreen)
        {
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero;
            containerRect.sizeDelta = new Vector2(fullscreenSize, fullscreenSize);
            Debug.Log($"[MinimapUI] 切换至全屏 {fullscreenSize}px");
        }
        else
        {
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(1, 1);
            containerRect.anchoredPosition = new Vector2(-20, -24);
            containerRect.sizeDelta = new Vector2(minimapSize, minimapSize);
            Debug.Log($"[MinimapUI] 切换至小窗 {minimapSize}px");
        }
    }

    /// <summary>
    /// 静止透明度过渡
    /// </summary>
    void UpdateIdleAlpha()
    {
        if (canvasGroup == null) return;

        if (isFullscreen)
        {
            // 全屏时保持 100%
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 1f, Time.deltaTime * alphaLerpSpeed);
            return;
        }

        // 玩家移动检测
        Vector3 cur = playerTransform.position;
        if (Vector3.Distance(cur, lastPlayerPos) > MoveThreshold)
        {
            lastActivityTime = Time.time;
            lastPlayerPos = cur;
        }

        float targetAlpha = (Time.time - lastActivityTime > idleTimeout) ? idleAlpha : activeAlpha;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * alphaLerpSpeed);
    }

    /// <summary>
    /// 更新小地图内容
    /// </summary>
    void UpdateMinimap()
    {
        // 玩家图标始终居中
        if (playerIcon != null)
        {
            playerIcon.rectTransform.anchoredPosition = Vector2.zero;
            // 玩家朝向旋转（使用缓存的 playerRb，取 velocity 方向）
            if (playerRb != null && playerRb.velocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(playerRb.velocity.y, playerRb.velocity.x) * Mathf.Rad2Deg;
                playerIcon.rectTransform.localEulerAngles = new Vector3(0, 0, angle - 90f);
            }
        }

        // 定时轮询敌人
        enemyPollTimer += Time.deltaTime;
        if (enemyPollTimer >= enemyPollInterval)
        {
            enemyPollTimer = 0f;
            UpdateEnemyIcons();
        }

        // 定时轮询地标
        landmarkPollTimer += Time.deltaTime;
        if (landmarkPollTimer >= landmarkPollInterval)
        {
            landmarkPollTimer = 0f;
            UpdateLandmarkIcons();
        }
    }

    /// <summary>
    /// 更新敌人图标（对象池 + 定时轮询）
    /// </summary>
    void UpdateEnemyIcons()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerTransform.position, displayRange, enemyLayer);
        int need = enemies.Length;

        // 扩展池
        while (enemyIconPool.Count < need)
        {
            GameObject icon = CreateEnemyIcon();
            icon.SetActive(false);
            enemyIconPool.Add(icon);
        }

        // 激活需要的
        for (int i = 0; i < need; i++)
        {
            GameObject icon = enemyIconPool[i];
            if (!icon.activeSelf) icon.SetActive(true);

            Vector2 worldOffset = (Vector2)(enemies[i].transform.position) - (Vector2)playerTransform.position;
            Vector2 minimapPos = ConvertToMinimapPosition(worldOffset);
            icon.transform.localPosition = new Vector3(minimapPos.x, minimapPos.y, 0);
        }

        // 关闭多余的
        for (int i = need; i < enemyIconActiveCount; i++)
        {
            if (i < enemyIconPool.Count && enemyIconPool[i] != null)
            {
                enemyIconPool[i].SetActive(false);
            }
        }
        enemyIconActiveCount = need;
    }

    /// <summary>
    /// 更新地标图标（营火 / 安全区 / 传送点）
    /// </summary>
    void UpdateLandmarkIcons()
    {
        // 收集地标：[worldPos, color, size]
        var landmarks = new List<System.Tuple<Vector3, Color, float>>();

        // 营火（CampfireSystem 组件）——未注册的 Tag 会抛 UnityException，故统一用类型查找
        var campfires = Object.FindObjectsOfType<CampfireSystem>();
        if (campfires != null)
        {
            foreach (var c in campfires)
            {
                if (c == null) continue;
                landmarks.Add(System.Tuple.Create(c.transform.position, campfireColor, landmarkIconSize));
            }
        }

        // 安全区（SafeZoneDetector 组件）
        var safeZones = Object.FindObjectsOfType<SafeZoneDetector>();
        if (safeZones != null)
        {
            foreach (var s in safeZones)
            {
                if (s == null) continue;
                landmarks.Add(System.Tuple.Create(s.transform.position, safeZoneColor, safeZoneIconSize));
            }
        }

        // 传送点（TimePortal 组件）
        var portals = Object.FindObjectsOfType<TimePortal>();
        if (portals != null)
        {
            foreach (var p in portals)
            {
                if (p == null) continue;
                landmarks.Add(System.Tuple.Create(p.transform.position, portalColor, landmarkIconSize));
            }
        }

        int need = landmarks.Count;
        while (landmarkIconPool.Count < need)
        {
            GameObject icon = CreateLandmarkIcon();
            icon.SetActive(false);
            landmarkIconPool.Add(icon);
        }

        for (int i = 0; i < need; i++)
        {
            GameObject icon = landmarkIconPool[i];
            if (!icon.activeSelf) icon.SetActive(true);

            Image img = icon.GetComponent<Image>();
            if (img != null)
            {
                img.color = landmarks[i].Item2;
                img.rectTransform.sizeDelta = new Vector2(landmarks[i].Item3, landmarks[i].Item3);
            }

            Vector2 worldOffset = (Vector2)landmarks[i].Item1 - (Vector2)playerTransform.position;
            Vector2 minimapPos = ConvertToMinimapPosition(worldOffset);
            icon.transform.localPosition = new Vector3(minimapPos.x, minimapPos.y, 0);
        }

        for (int i = need; i < landmarkIconActiveCount; i++)
        {
            if (i < landmarkIconPool.Count && landmarkIconPool[i] != null)
            {
                landmarkIconPool[i].SetActive(false);
            }
        }
        landmarkIconActiveCount = need;
    }

    /// <summary>
    /// 将世界坐标偏移转换为小地图本地坐标，超出范围时钳制到圆周（指示方向）
    /// </summary>
    Vector2 ConvertToMinimapPosition(Vector2 worldOffset)
    {
        float halfSize = (containerRect != null ? containerRect.rect.width : minimapSize) * 0.5f;
        float maxDistance = halfSize - landmarkIconSize * 0.5f;
        if (maxDistance < 1f) maxDistance = 1f;

        float distance = worldOffset.magnitude;
        if (distance < 0.001f) return Vector2.zero;

        if (distance > displayRange)
        {
            // 钳制到圆周边缘，保留方向
            return worldOffset.normalized * maxDistance;
        }

        float normalizedDistance = distance / displayRange;
        return worldOffset.normalized * normalizedDistance * maxDistance;
    }

    /// <summary>
    /// 创建敌人图标（挂在 iconParent/maskObj 下受裁剪）
    /// </summary>
    GameObject CreateEnemyIcon()
    {
        Transform parent = iconParent != null ? iconParent : containerRect;
        if (enemyIconPrefab != null)
        {
            return Instantiate(enemyIconPrefab, parent);
        }
        GameObject icon = new GameObject("EnemyIcon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(parent, false);
        Image img = icon.GetComponent<Image>();
        img.color = enemyColor;
        img.raycastTarget = false;
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(enemyIconSize, enemyIconSize);
        return icon;
    }

    /// <summary>
    /// 创建地标图标（挂在 iconParent/maskObj 下受裁剪）
    /// </summary>
    GameObject CreateLandmarkIcon()
    {
        Transform parent = iconParent != null ? iconParent : containerRect;
        if (landmarkIconPrefab != null)
        {
            return Instantiate(landmarkIconPrefab, parent);
        }
        GameObject icon = new GameObject("LandmarkIcon", typeof(RectTransform), typeof(Image));
        icon.transform.SetParent(parent, false);
        Image img = icon.GetComponent<Image>();
        img.color = Color.yellow;
        img.raycastTarget = false;
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(landmarkIconSize, landmarkIconSize);
        return icon;
    }

    /// <summary>
    /// 初始化UI
    /// </summary>
    void InitializeUI()
    {
        if (background == null)
        {
            CreateDefaultUI();
        }
        ApplyStyleSettings();
    }

    /// <summary>
    /// 应用样式配置
    /// </summary>
    void ApplyStyleSettings()
    {
        if (background != null) background.color = backgroundColor;
        if (playerIcon != null)
        {
            playerIcon.color = playerColor;
            playerIcon.rectTransform.sizeDelta = new Vector2(playerIconSize, playerIconSize);
        }
    }

    /// <summary>
    /// 创建默认UI：圆角外框 + 边框 + 圆形遮罩 + 玩家图标 + 地图标签
    /// </summary>
    void CreateDefaultUI()
    {
        // 主容器（实际承载图标）
        GameObject container = new GameObject("MinimapContainer");
        container.transform.SetParent(transform, false);
        containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 1);
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(1, 1);
        containerRect.anchoredPosition = new Vector2(-20, -24);
        containerRect.sizeDelta = new Vector2(minimapSize, minimapSize);

        // 圆角背景（外框底色）
        GameObject bgObj = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgObj.transform.SetParent(container.transform, false);
        background = bgObj.GetComponent<Image>();
        background.color = backgroundColor;
        IIPUIFactory.ApplyRounded(background, true);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 边框
        IIPUIFactory.CreateBorder(container.transform, IIPUIFactory.BorderBright, true);

        // 圆形遮罩节点（承载所有动态图标）
        GameObject maskObj = new GameObject("Mask", typeof(RectTransform), typeof(Image));
        maskObj.transform.SetParent(container.transform, false);
        mask = maskObj.GetComponent<Image>();
        mask.color = Color.white;
        mask.raycastTarget = false;
        RectTransform maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;

        if (useCircleMask)
        {
            // 尝试加载圆形 Sprite 作为遮罩；若无资源则退化为 RectMask2D
            Sprite circle = Resources.Load<Sprite>("UI/CircleMask");
            if (circle != null)
            {
                mask.sprite = circle;
                mask.type = Image.Type.Simple;
                Mask maskComp = maskObj.AddComponent<Mask>();
                maskComp.showMaskGraphic = false;
            }
            else
            {
                Debug.LogWarning("[MinimapUI] 未找到 Resources/UI/CircleMask 圆形 Sprite，退化为 RectMask2D 圆角矩形裁剪");
                Object.Destroy(mask); // 运行时用 Destroy，避免 DestroyImmediate 立即销毁风险
                mask = null; // 字段置空，避免持有已销毁引用
                RectMask2D rectMask = maskObj.AddComponent<RectMask2D>();
                rectMask.softness = new Vector2Int(20, 20);
            }
        }
        else
        {
            Object.Destroy(mask); // 运行时用 Destroy，避免 DestroyImmediate 立即销毁风险
            mask = null;
            RectMask2D rectMask = maskObj.AddComponent<RectMask2D>();
            rectMask.softness = new Vector2Int(20, 20);
        }

        // 玩家中心箭头图标（挂在 mask 下，受 Mask/RectMask2D 裁剪）
        iconParent = maskObj.transform;
        GameObject playerObj = new GameObject("PlayerIcon", typeof(RectTransform), typeof(Image));
        playerObj.transform.SetParent(iconParent, false);
        playerIcon = playerObj.GetComponent<Image>();
        playerIcon.color = playerColor;
        playerIcon.raycastTarget = false;
        RectTransform playerRect = playerObj.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerRect.anchorMax = new Vector2(0.5f, 0.5f);
        playerRect.pivot = new Vector2(0.5f, 0.5f);
        playerRect.anchoredPosition = Vector2.zero;
        playerRect.sizeDelta = new Vector2(playerIconSize, playerIconSize);

        // containerRect 保持指向外层 container（非 maskObj），滚轮缩放/全屏切换作用于外框，
        // maskObj 铺满 container 随之缩放，动态图标挂在 maskObj(iconParent) 下受裁剪
        // （containerRect 已在上方赋值为 container 的 RectTransform，此处不重定向）

        // 右上角"地图"标签
        IIPUIFactory.CreateLabelAnchored("MapLabel", container.transform,
            "地图", IIPUIStyle.FontSizeKey, IIPUIFactory.TextDim,
            new Vector2(0, 0.85f), new Vector2(1, 1),
            new Vector2(0, 0), new Vector2(0, -1), TextAnchor.UpperCenter);
    }
}
