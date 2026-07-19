using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 小地图UI组件
/// 显示玩家周围区域的简化地图，包含玩家位置、敌人位置、重要地标等
/// </summary>
public class MinimapUI : MonoBehaviour
{
    [Header("UI引用")]
    [Tooltip("小地图背景")]
    public Image background;
    
    [Tooltip("小地图遮罩（圆形）")]
    public Image mask;
    
    [Tooltip("玩家图标")]
    public Image playerIcon;
    
    [Tooltip("敌人图标预制体")]
    public GameObject enemyIconPrefab;
    
    [Tooltip("地标图标预制体")]
    public GameObject landmarkIconPrefab;

    [Header("样式配置")]
    [Tooltip("小地图大小")]
    public float minimapSize = 150f;
    
    [Tooltip("背景颜色")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.15f, 0.8f);
    
    [Tooltip("边框颜色")]
    public Color borderColor = new Color(0.4f, 0.4f, 0.5f, 1f);
    
    [Tooltip("玩家图标颜色")]
    public Color playerColor = Color.cyan;
    
    [Tooltip("敌人图标颜色")]
    public Color enemyColor = Color.red;
    
    [Tooltip("地标图标颜色")]
    public Color landmarkColor = Color.yellow;

    [Header("显示范围")]
    [Tooltip("小地图显示范围（半径）")]
    public float displayRange = 50f;
    
    [Tooltip("玩家图标大小")]
    public float playerIconSize = 8f;
    
    [Tooltip("敌人图标大小")]
    public float enemyIconSize = 6f;
    
    [Tooltip("地标图标大小")]
    public float landmarkIconSize = 10f;

    [Header("图层设置")]
    [Tooltip("玩家图层")]
    public LayerMask playerLayer;
    
    [Tooltip("敌人图层")]
    public LayerMask enemyLayer;

    private Transform playerTransform;
    private List<GameObject> enemyIcons = new List<GameObject>();
    private List<GameObject> landmarkIcons = new List<GameObject>();
    private RectTransform containerRect;

    void Start()
    {
        EnsureContainerLayout();
        // 查找玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("[MinimapUI] 未找到玩家对象");
        }

        InitializeUI();
    }

    void OnEnable()
    {
        // 小地图的玩家/敌人位置需每帧刷新，无对应事件，保留 Update 轮询
        // 此处仅作为事件订阅占位，便于未来接入地图刷新事件
    }

    /// <summary>
    /// 确保容器RectTransform布局正确（右上角）
    /// </summary>
    void EnsureContainerLayout()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-20, -20);
        rt.sizeDelta = new Vector2(minimapSize, minimapSize);
    }

    void Update()
    {
        if (playerTransform == null) return;
        
        UpdateMinimap();
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
    /// 更新小地图
    /// </summary>
    void UpdateMinimap()
    {
        // 更新玩家图标位置（始终在中心）
        if (playerIcon != null)
        {
            playerIcon.rectTransform.anchoredPosition = Vector2.zero;
        }
        
        // 更新敌人图标
        UpdateEnemyIcons();
        
        // 更新地标图标
        UpdateLandmarkIcons();
    }

    /// <summary>
    /// 更新敌人图标
    /// </summary>
    void UpdateEnemyIcons()
    {
        // 查找范围内的敌人
        Collider2D[] enemies = Physics2D.OverlapCircleAll(playerTransform.position, displayRange, enemyLayer);
        
        // 清理多余图标
        while (enemyIcons.Count > enemies.Length)
        {
            GameObject icon = enemyIcons[enemyIcons.Count - 1];
            enemyIcons.RemoveAt(enemyIcons.Count - 1);
            if (icon != null)
            {
                Destroy(icon);
            }
        }
        
        // 创建或更新图标
        for (int i = 0; i < enemies.Length; i++)
        {
            Transform enemyTransform = enemies[i].transform;
            Vector2 relativePos = enemyTransform.position - playerTransform.position;
            
            // 转换为小地图坐标
            Vector2 minimapPos = ConvertToMinimapPosition(relativePos);
            
            if (i >= enemyIcons.Count)
            {
                // 创建新图标
                GameObject icon = CreateEnemyIcon();
                enemyIcons.Add(icon);
            }
            
            // 更新位置
            enemyIcons[i].transform.localPosition = new Vector3(minimapPos.x, minimapPos.y, 0);
        }
    }

    /// <summary>
    /// 更新地标图标
    /// </summary>
    void UpdateLandmarkIcons()
    {
        // 这里可以添加地标（如宝箱、传送点等）的显示逻辑
        // 暂时留空，后续根据需求扩展
    }

    /// <summary>
    /// 将世界坐标转换为小地图坐标
    /// </summary>
    Vector2 ConvertToMinimapPosition(Vector2 worldOffset)
    {
        // 限制在显示范围内
        float maxDistance = minimapSize / 2f - landmarkIconSize;
        float distance = worldOffset.magnitude;
        
        if (distance > displayRange)
        {
            worldOffset = worldOffset.normalized * displayRange;
            distance = displayRange;
        }
        
        // 转换为小地图坐标（0-1范围）
        float normalizedDistance = distance / displayRange;
        Vector2 minimapOffset = worldOffset.normalized * normalizedDistance * maxDistance;
        
        return minimapOffset;
    }

    /// <summary>
    /// 创建敌人图标
    /// </summary>
    GameObject CreateEnemyIcon()
    {
        if (enemyIconPrefab != null)
        {
            return Instantiate(enemyIconPrefab, transform);
        }
        
        // 创建默认图标
        GameObject icon = new GameObject("EnemyIcon");
        icon.transform.SetParent(transform, false);
        
        Image iconImage = icon.AddComponent<Image>();
        iconImage.color = enemyColor;
        
        RectTransform rect = icon.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(enemyIconSize, enemyIconSize);
        
        return icon;
    }

    /// <summary>
    /// 应用样式配置
    /// </summary>
    void ApplyStyleSettings()
    {
        if (background != null)
        {
            background.color = backgroundColor;
        }
        
        if (playerIcon != null)
        {
            playerIcon.color = playerColor;
            playerIcon.rectTransform.sizeDelta = new Vector2(playerIconSize, playerIconSize);
        }
    }

    /// <summary>
    /// 创建默认UI
    /// </summary>
    void CreateDefaultUI()
    {
        // 创建主容器
        GameObject container = new GameObject("MinimapContainer");
        container.transform.SetParent(transform, false);
        
        containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 1); // 右上角
        containerRect.anchorMax = new Vector2(1, 1);
        containerRect.pivot = new Vector2(1, 1);
        containerRect.anchoredPosition = new Vector2(-20, -20);
        containerRect.sizeDelta = new Vector2(minimapSize, minimapSize);
        
        // 创建背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(container.transform, false);
        background = bgObj.AddComponent<Image>();
        background.color = backgroundColor;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // 创建圆形遮罩
        GameObject maskObj = new GameObject("Mask");
        maskObj.transform.SetParent(container.transform, false);
        mask = maskObj.AddComponent<Image>();
        mask.color = Color.white;
        mask.type = Image.Type.Simple;
        
        RectTransform maskRect = maskObj.GetComponent<RectTransform>();
        maskRect.anchorMin = new Vector2(0.05f, 0.05f);
        maskRect.anchorMax = new Vector2(0.95f, 0.95f);
        maskRect.offsetMin = Vector2.zero;
        maskRect.offsetMax = Vector2.zero;
        
        // 创建玩家图标
        GameObject playerObj = new GameObject("PlayerIcon");
        playerObj.transform.SetParent(container.transform, false);
        playerIcon = playerObj.AddComponent<Image>();
        playerIcon.color = playerColor;
        
        RectTransform playerRect = playerObj.GetComponent<RectTransform>();
        playerRect.anchorMin = new Vector2(0.5f, 0.5f);
        playerRect.anchorMax = new Vector2(0.5f, 0.5f);
        playerRect.pivot = new Vector2(0.5f, 0.5f);
        playerRect.anchoredPosition = Vector2.zero;
        playerRect.sizeDelta = new Vector2(playerIconSize, playerIconSize);
    }
}
